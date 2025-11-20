using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using RestX.API.Data.Repository.Interfaces;
using RestX.API.Models.DTOs.Request;
using RestX.API.Models.DTOs.Response;
using RestX.API.Models.Entities;
using RestX.API.Services.Interfaces;
using System.Text.Json;
using CreatePaymentRequest = RestX.API.Models.DTOs.Request.CreatePaymentLinkRequest;
using CreatePaymentResponse = RestX.API.Models.DTOs.Response.CreatePaymentLinkResponse;

namespace RestX.API.Services.Implementations
{
    public class PaymentService : BaseService, IPaymentService
    {
        private readonly PayOSClient _payOS;
        private readonly IConfiguration _configuration;
        private readonly ITableService _tableService;
        private readonly string _returnUrl;
        private readonly string _cancelUrl;

        public PaymentService(
            IRepository repo,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ITableService tableService) : base(repo, httpContextAccessor)
        {
            _configuration = configuration;
            _tableService = tableService;

            // Get PayOS configuration
            var clientId = _configuration["PayOS:ClientId"] ?? throw new InvalidOperationException("PayOS ClientId not configured");
            var apiKey = _configuration["PayOS:ApiKey"] ?? throw new InvalidOperationException("PayOS ApiKey not configured");
            var checksumKey = _configuration["PayOS:ChecksumKey"] ?? throw new InvalidOperationException("PayOS ChecksumKey not configured");

            _returnUrl = _configuration["PayOS:ReturnUrl"] ?? "https://localhost:7215/Order/PaymentReturn";
            _cancelUrl = _configuration["PayOS:CancelUrl"] ?? "https://localhost:7215/Order/PaymentCancel";

            // Initialize PayOS client
            _payOS = new PayOSClient(clientId, apiKey, checksumKey);
        }

        public async Task<CreatePaymentResponse> CreatePaymentLinkAsync(CreatePaymentRequest request)
        {
            // Get order with details
            var orders = await Repo.GetAsync<Order>(
                filter: o => o.Id == request.OrderId,
                includeProperties: "OrderDetails,OrderDetails.Dish,Customer"
            );

            var order = orders.FirstOrDefault();
            if (order == null)
                throw new InvalidOperationException("Order not found");

            // Calculate total amount
            decimal totalAmount = order.OrderDetails
                .Where(od => od.IsActive == true)
                .Sum(od => od.Price * od.Quantity);

            // Generate unique order code (using timestamp + random)
            long orderCode = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // Create payment description
            string description = request.Description ?? $"Payment for Order #{order.Id.ToString().Substring(0, 8)}";

            // Create payment link request for PayOS v2
            var payOSRequest = new PayOS.Models.V2.PaymentRequests.CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = (int)totalAmount, // PayOS requires int (VND doesn't have decimals)
                Description = description,
                ReturnUrl = _returnUrl,
                CancelUrl = _cancelUrl
            };

            // Call PayOS API
            var payOSResponse = await _payOS.PaymentRequests.CreateAsync(payOSRequest);

            // Create Payment record in database
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                PaymentMethodId = Guid.Parse("550e8400-e29b-41d4-a716-446655440138"), // Default payment method
                Time = DateTime.UtcNow,
                Cost = totalAmount,
                IsActive = true,
                OrderCode = orderCode,
                CheckoutUrl = payOSResponse.CheckoutUrl,
                QRBase64 = payOSResponse.QrCode,
                PayOSStatus = "PENDING",
                TransactionId = payOSResponse.PaymentLinkId,
                CreatedBy = OwnerId.ToString(),
                CreatedDate = DateTime.UtcNow
            };

            await Repo.CreateAsync(payment);
            await Repo.SaveAsync();

            return new CreatePaymentResponse
            {
                PaymentId = payment.Id,
                OrderCode = orderCode,
                CheckoutUrl = payOSResponse.CheckoutUrl,
                QRBase64 = payOSResponse.QrCode,
                Amount = totalAmount,
                Status = "PENDING"
            };
        }

        public async Task<PaymentInfoResponse> GetPaymentInfoAsync(long orderCode)
        {
            // Get payment from database with order relationship
            var payments = await Repo.GetAsync<Payment>(
                filter: p => p.OrderCode == orderCode && p.IsActive == true,
                includeProperties: "Order"
            );

            var payment = payments.FirstOrDefault();
            if (payment == null)
                throw new InvalidOperationException("Payment not found");

            // Get payment info from PayOS
            var payOSInfo = await _payOS.PaymentRequests.GetAsync(orderCode);

            // Update payment status if changed
            string currentStatus = payOSInfo.Status.ToString();
            bool wasUnpaid = payment.PayOSStatus != "PAID" && payment.PayOSStatus != "COMPLETED";

            if (currentStatus != payment.PayOSStatus)
            {
                payment.PayOSStatus = currentStatus;
                payment.ModifiedBy = "System";
                payment.ModifiedDate = DateTime.UtcNow;

                if (currentStatus == "PAID" || currentStatus == "COMPLETED")
                {
                    payment.PaidAt = DateTime.UtcNow;

                    // Update table status to Cleaning (4) when payment is completed and order status is Completed (4)
                    if (wasUnpaid && payment.Order != null && payment.Order.OrderStatusId == 4)
                    {
                        await _tableService.UpdateTableStatusAsync(payment.Order.TableId, 4); // 4 = Cleaning
                    }
                }

                Repo.Update(payment);
                await Repo.SaveAsync();
            }

            return new PaymentInfoResponse
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                OrderCode = orderCode,
                Amount = payment.Cost,
                Status = payment.PayOSStatus ?? "UNKNOWN",
                TransactionId = payment.TransactionId,
                PaidAt = payment.PaidAt,
                CreatedAt = payment.CreatedDate
            };
        }

        public async Task<bool> CancelPaymentAsync(long orderCode, string? cancellationReason = null)
        {
            try
            {
                // Cancel payment in PayOS
                var result = await _payOS.PaymentRequests.CancelAsync(orderCode, cancellationReason);

                // Update payment in database
                var payments = await Repo.GetAsync<Payment>(
                    filter: p => p.OrderCode == orderCode && p.IsActive == true
                );

                var payment = payments.FirstOrDefault();
                if (payment != null)
                {
                    payment.PayOSStatus = "CANCELLED";
                    payment.ModifiedBy = "System";
                    payment.ModifiedDate = DateTime.UtcNow;

                    Repo.Update(payment);
                    await Repo.SaveAsync();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ProcessWebhookAsync(string webhookBody, string signature)
        {
            try
            {
                // For now, just parse the webhook body manually
                // PayOS v2.0 might have different webhook structure
                using var doc = JsonDocument.Parse(webhookBody);
                var root = doc.RootElement;

                // Extract order code from webhook data
                if (!root.TryGetProperty("data", out var dataElement))
                    return false;

                if (!dataElement.TryGetProperty("orderCode", out var orderCodeElement))
                    return false;

                long orderCode = orderCodeElement.GetInt64();
                bool success = root.TryGetProperty("success", out var successElement) && successElement.GetBoolean();

                // Update payment in database
                var payments = await Repo.GetAsync<Payment>(
                    filter: p => p.OrderCode == orderCode && p.IsActive == true
                );

                var payment = payments.FirstOrDefault();
                if (payment != null)
                {
                    payment.PayOSStatus = success ? "PAID" : "FAILED";
                    payment.PaidAt = success ? DateTime.UtcNow : null;
                    payment.ModifiedBy = "WebhookSystem";
                    payment.ModifiedDate = DateTime.UtcNow;

                    Repo.Update(payment);
                    await Repo.SaveAsync();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<PaymentReturnResponse> HandlePaymentReturnAsync(string orderCode, string status)
        {
            try
            {
                if (!long.TryParse(orderCode, out long code))
                {
                    return new PaymentReturnResponse
                    {
                        Success = false,
                        Message = "Invalid order code"
                    };
                }

                // Get payment info
                var paymentInfo = await GetPaymentInfoAsync(code);

                return new PaymentReturnResponse
                {
                    Success = paymentInfo.Status == "PAID" || paymentInfo.Status == "COMPLETED",
                    Message = paymentInfo.Status == "PAID" || paymentInfo.Status == "COMPLETED"
                        ? "Payment successful"
                        : $"Payment {paymentInfo.Status.ToLower()}",
                    OrderId = paymentInfo.OrderId,
                    OrderCode = code,
                    Amount = paymentInfo.Amount,
                    Status = paymentInfo.Status
                };
            }
            catch (Exception ex)
            {
                return new PaymentReturnResponse
                {
                    Success = false,
                    Message = $"Error processing payment return: {ex.Message}"
                };
            }
        }
    }
}
