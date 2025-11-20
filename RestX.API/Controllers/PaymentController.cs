using Microsoft.AspNetCore.Mvc;
using RestX.API.Models.DTOs.Request;
using RestX.API.Services.Interfaces;

namespace RestX.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IExceptionHandler exceptionHandler;

        public PaymentController(
            IPaymentService paymentService,
            IExceptionHandler exceptionHandler)
        {
            _paymentService = paymentService;
            this.exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Create a payment link for an order
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreatePaymentLink([FromBody] CreatePaymentLinkRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid request data", errors = ModelState });

                var response = await _paymentService.CreatePaymentLinkAsync(request);

                return Ok(new
                {
                    success = true,
                    message = "Payment link created successfully",
                    data = response
                });
            }
            catch (Exception ex)
            {
                exceptionHandler.RaiseException(ex, "Error creating payment link");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while creating payment link",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        /// <summary>
        /// Get payment information by order code
        /// </summary>
        [HttpGet("{orderCode}")]
        public async Task<IActionResult> GetPaymentInfo(long orderCode)
        {
            try
            {
                var response = await _paymentService.GetPaymentInfoAsync(orderCode);

                return Ok(new
                {
                    success = true,
                    data = response
                });
            }
            catch (Exception ex)
            {
                exceptionHandler.RaiseException(ex, $"Error getting payment info for order code: {orderCode}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while getting payment information",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Cancel a payment
        /// </summary>
        [HttpPost("{orderCode}/cancel")]
        public async Task<IActionResult> CancelPayment(long orderCode, [FromBody] string? cancellationReason = null)
        {
            try
            {
                var success = await _paymentService.CancelPaymentAsync(orderCode, cancellationReason);

                return Ok(new
                {
                    success = success,
                    message = success ? "Payment cancelled successfully" : "Failed to cancel payment"
                });
            }
            catch (Exception ex)
            {
                exceptionHandler.RaiseException(ex, $"Error cancelling payment for order code: {orderCode}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while cancelling payment",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Webhook endpoint for PayOS payment notifications
        /// </summary>
        [HttpPost("webhook")]
        public async Task<IActionResult> PayOSWebhook()
        {
            try
            {
                // Read raw body
                using var reader = new StreamReader(Request.Body);
                var webhookBody = await reader.ReadToEndAsync();

                // Get signature from header
                var signature = Request.Headers["X-PayOS-Signature"].FirstOrDefault() ?? string.Empty;

                var success = await _paymentService.ProcessWebhookAsync(webhookBody, signature);

                return Ok(new
                {
                    success = success,
                    message = success ? "Webhook processed successfully" : "Failed to process webhook"
                });
            }
            catch (Exception ex)
            {
                exceptionHandler.RaiseException(ex, "Error processing PayOS webhook");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while processing webhook"
                });
            }
        }

        /// <summary>
        /// Handle payment return from PayOS
        /// </summary>
        [HttpGet("return")]
        public async Task<IActionResult> PaymentReturn([FromQuery] string orderCode, [FromQuery] string status)
        {
            try
            {
                var response = await _paymentService.HandlePaymentReturnAsync(orderCode, status);

                return Ok(new
                {
                    success = response.Success,
                    message = response.Message,
                    data = response
                });
            }
            catch (Exception ex)
            {
                exceptionHandler.RaiseException(ex, "Error handling payment return");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while processing payment return",
                    error = ex.Message
                });
            }
        }
    }
}
