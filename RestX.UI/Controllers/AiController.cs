using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Controllers
{
    [Authorize(Roles = "Owner,Staff")]
    public class AiController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AiController> _logger;

        public AiController(
            IApiService apiService,
            ILogger<AiController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        /// <summary>
        /// AI assistant page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index()
        {
            var model = new AiAssistantViewModel();
            return View(model);
        }

        /// <summary>
        /// Send message to AI assistant
        /// </summary>
        /// <param name="message">User message</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SendMessage(string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    return Json(new { success = false, message = "Message cannot be empty" });
                }

                _logger.LogInformation("Sending AI message: {Message}", message);

                var requestData = new
                {
                    Message = message,
                    UserId = User?.FindFirst("UserId")?.Value,
                    Timestamp = DateTime.UtcNow
                };

                var response = await _apiService.PostAsync<object, object>("api/ai/chat", requestData);
                
                if (response != null)
                {
                    return Json(new { success = true, data = response });
                }
                
                return Json(new { success = false, message = "AI service is currently unavailable" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to AI: {Message}", message);
                return Json(new { success = false, message = "An error occurred while processing your message" });
            }
        }

        /// <summary>
        /// Get AI suggestions for menu optimization
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetMenuSuggestions()
        {
            try
            {
                _logger.LogInformation("Requesting AI menu suggestions");

                var response = await _apiService.PostAsync<object, object>("api/ai/menu-suggestions", new { });
                
                if (response != null)
                {
                    return Json(new { success = true, data = response });
                }
                
                return Json(new { success = false, message = "Unable to get menu suggestions at this time" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI menu suggestions");
                return Json(new { success = false, message = "An error occurred while getting menu suggestions" });
            }
        }

        /// <summary>
        /// Get AI insights for business analytics
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetBusinessInsights()
        {
            try
            {
                _logger.LogInformation("Requesting AI business insights");

                var response = await _apiService.PostAsync<object, object>("api/ai/business-insights", new { });
                
                if (response != null)
                {
                    return Json(new { success = true, data = response });
                }
                
                return Json(new { success = false, message = "Unable to get business insights at this time" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI business insights");
                return Json(new { success = false, message = "An error occurred while getting business insights" });
            }
        }

        /// <summary>
        /// Get AI recommendations for customer service
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetCustomerRecommendations(Guid customerId)
        {
            try
            {
                _logger.LogInformation("Requesting AI customer recommendations for: {CustomerId}", customerId);

                var requestData = new
                {
                    CustomerId = customerId
                };

                var response = await _apiService.PostAsync<object, object>("api/ai/customer-recommendations", requestData);
                
                if (response != null)
                {
                    return Json(new { success = true, data = response });
                }
                
                return Json(new { success = false, message = "Unable to get customer recommendations at this time" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI customer recommendations for: {CustomerId}", customerId);
                return Json(new { success = false, message = "An error occurred while getting customer recommendations" });
            }
        }

        /// <summary>
        /// Process AI-based order prediction
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PredictOrderTrends()
        {
            try
            {
                _logger.LogInformation("Requesting AI order trend predictions");

                var response = await _apiService.PostAsync<object, object>("api/ai/predict-orders", new { });
                
                if (response != null)
                {
                    return Json(new { success = true, data = response });
                }
                
                return Json(new { success = false, message = "Unable to predict order trends at this time" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI order predictions");
                return Json(new { success = false, message = "An error occurred while predicting order trends" });
            }
        }

        /// <summary>
        /// Clear AI chat history
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ClearChatHistory()
        {
            try
            {
                var userId = User?.FindFirst("UserId")?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not identified" });
                }

                var response = await _apiService.DeleteAsync($"api/ai/chat-history/{userId}");
                
                if (response)
                {
                    return Json(new { success = true, message = "Chat history cleared successfully" });
                }
                
                return Json(new { success = false, message = "Failed to clear chat history" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing AI chat history");
                return Json(new { success = false, message = "An error occurred while clearing chat history" });
            }
        }
    }

    // AI-related ViewModels
    public class AiAssistantViewModel
    {
        public List<AiChatMessage> ChatHistory { get; set; } = new();
        public string? CurrentMessage { get; set; }
        public bool IsLoading { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class AiChatMessage
    {
        public string Message { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty; // "User" or "AI"
        public DateTime Timestamp { get; set; }
        public string? MessageType { get; set; } // "text", "suggestion", "insight"
    }

    public class AiSuggestion
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal? ImpactScore { get; set; }
        public List<string> ActionItems { get; set; } = new();
    }
}
