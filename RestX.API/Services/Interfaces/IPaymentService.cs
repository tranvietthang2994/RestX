using RestX.API.Models.DTOs.Request;
using RestX.API.Models.DTOs.Response;

namespace RestX.API.Services.Interfaces
{
    public interface IPaymentService
    {
        /// <summary>
        /// Create a payment link for an order
        /// </summary>
        Task<CreatePaymentLinkResponse> CreatePaymentLinkAsync(CreatePaymentLinkRequest request);

        /// <summary>
        /// Get payment information by order code
        /// </summary>
        Task<PaymentInfoResponse> GetPaymentInfoAsync(long orderCode);

        /// <summary>
        /// Cancel a payment
        /// </summary>
        Task<bool> CancelPaymentAsync(long orderCode, string? cancellationReason = null);

        /// <summary>
        /// Verify webhook signature and process payment callback
        /// </summary>
        Task<bool> ProcessWebhookAsync(string webhookBody, string signature);

        /// <summary>
        /// Handle payment return from PayOS
        /// </summary>
        Task<PaymentReturnResponse> HandlePaymentReturnAsync(string orderCode, string status);
    }
}
