using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace RestX.UI.Controllers
{
    //[Authorize(Roles = "Owner,Staff")]
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
        public async Task<IActionResult> Index(Guid? ownerId = null, int? tableId = null)
        {
            var model = new AiAssistantViewModel();

            if (ownerId.HasValue)
            {
                ViewBag.OwnerId = ownerId;
                HttpContext.Session.SetString("OwnerId", ownerId.Value.ToString());
            }

            if (tableId.HasValue)
            {
                ViewBag.TableId = tableId;
                HttpContext.Session.SetString("TableId", tableId.Value.ToString());
            }

            var customerId = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerId))
            {
                if (ownerId.HasValue && tableId.HasValue)
                {
                    TempData["Message"] = "Vui lòng đăng nhập để sử dụng trợ lý AI và thêm món vào giỏ hàng.";
                    var returnUrl = Url.Action("Index", "Ai", new { ownerId = ownerId.Value, tableId = tableId.Value });
                    return RedirectToAction("Login", "AuthCustomer", new { ownerId = ownerId.Value, tableId = tableId.Value, returnUrl });
                }

                TempData["Message"] = "Vui lòng đăng nhập trước khi sử dụng trợ lý AI.";
                return RedirectToAction("Index", "Home");
            }

            if (ownerId.HasValue)
            {
                var (dishes, errorMessage) = await LoadAvailableDishesAsync(ownerId.Value, tableId);
                model.AvailableDishes = dishes;

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    model.ErrorMessage = errorMessage;
                }
            }

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
        /// Unified AI chat endpoint consumed by the web client.
        /// </summary>
        /// <param name="request">User message and optional conversation context.</param>
        /// <returns>AI response payload.</returns>
        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] AiMessageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, error = "Message cannot be empty." });
            }

            try
            {
                _logger.LogInformation("AI chat message received. UserId: {UserId}", User?.FindFirst("UserId")?.Value);

                var apiRequest = new
                {
                    request.Message,
                    request.ConversationHistory
                };

                var apiResponse = await _apiService.PostAsync<object, AiApiResponse>("api/ai/chat", apiRequest);

                return CreateClientResponse(apiResponse, "Không thể nhận phản hồi từ AI lúc này.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI chat message.");
                return Json(new { success = false, error = "Đã xảy ra lỗi khi gửi tin nhắn. Vui lòng thử lại." });
            }
        }

        /// <summary>
        /// Unified AI suggestion endpoint consumed by the web client.
        /// </summary>
        /// <param name="request">User message and optional table context.</param>
        /// <returns>AI suggestion payload.</returns>
        [HttpPost]
        public async Task<IActionResult> GetSuggestions([FromBody] AiMessageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, error = "Message cannot be empty." });
            }

            try
            {
                _logger.LogInformation(
                    "AI suggestion requested. OwnerId: {OwnerId}, TableId: {TableId}",
                    request.OwnerId,
                    request.TableId);

                var endpoint = BuildSuggestionEndpoint(request.OwnerId, request.TableId);
                var apiRequest = new
                {
                    request.Message,
                    Context = request.ConversationHistory
                };

                var apiResponse = await _apiService.PostAsync<object, AiApiResponse>(endpoint, apiRequest);

                return CreateClientResponse(apiResponse, "Không thể tạo gợi ý món ăn lúc này.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting AI suggestions.");
                return Json(new { success = false, error = "Đã xảy ra lỗi khi lấy gợi ý. Vui lòng thử lại." });
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

        private JsonResult CreateClientResponse(AiApiResponse? apiResponse, string defaultErrorMessage)
        {
            if (apiResponse?.Success == true)
            {
                var responseText = ExtractResponseText(apiResponse.Response);
                return Json(new
                {
                    success = true,
                    response = responseText,
                    ownerId = apiResponse.OwnerId,
                    tableId = apiResponse.TableId
                });
            }

            var errorMessage = apiResponse?.Message;
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                errorMessage = defaultErrorMessage;
            }

            return Json(new
            {
                success = false,
                error = errorMessage
            });
        }

        private static string ExtractResponseText(object? response)
        {
            if (response is null)
            {
                return string.Empty;
            }

            if (response is string responseText)
            {
                return responseText;
            }

            if (response is JsonElement jsonElement)
            {
                return jsonElement.ValueKind switch
                {
                    JsonValueKind.String => jsonElement.GetString() ?? string.Empty,
                    JsonValueKind.Undefined or JsonValueKind.Null => string.Empty,
                    _ => jsonElement.GetRawText()
                };
            }

            return response.ToString() ?? string.Empty;
        }

        private static string BuildSuggestionEndpoint(Guid? ownerId, int? tableId)
        {
            if (ownerId.HasValue && tableId.HasValue)
            {
                return $"api/ai/suggestions/{ownerId.Value}/table/{tableId.Value}";
            }

            return "api/ai/suggestions";
        }

        private async Task<(List<AiDishInfo> dishes, string? errorMessage)> LoadAvailableDishesAsync(Guid ownerId, int? tableId)
        {
            try
            {
                var endpoint = tableId.HasValue
                    ? $"api/menu/restaurant/{ownerId}/table/{tableId}"
                    : $"api/menu/restaurant/{ownerId}";

                var menuResponse = await _apiService.GetAsync<MenuApiResponse>(endpoint);

                if (menuResponse?.Success == true && menuResponse.Data?.Categories != null)
                {
                    var dishes = menuResponse.Data.Categories
                        .Where(category => category.Dishes != null)
                        .SelectMany(category => category.Dishes.Select(dish => new AiDishInfo
                        {
                            Id = dish.Id,
                            Name = dish.Name,
                            CategoryId = category.Id,
                            CategoryName = category.CategoryName,
                            Price = dish.Price,
                            Description = dish.Description,
                            IsActive = dish.IsActive ?? true
                        }))
                        .Where(dish => dish.IsActive)
                        .ToList();

                    return (dishes, null);
                }

                var message = menuResponse?.Message ?? "Không thể tải dữ liệu thực đơn. Bạn vẫn có thể trò chuyện với AI.";
                return (new List<AiDishInfo>(), message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading menu for AI assistant. OwnerId: {OwnerId}", ownerId);
                return (new List<AiDishInfo>(), "Không thể tải danh sách món ăn để thêm vào giỏ. Bạn vẫn có thể tiếp tục trò chuyện với AI.");
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
        public List<AiDishInfo> AvailableDishes { get; set; } = new();
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

    public class AiMessageRequest
    {
        [Required]
        public string Message { get; set; } = string.Empty;
        public string? ConversationHistory { get; set; }
        public Guid? OwnerId { get; set; }
        public int? TableId { get; set; }
    }

    public class AiApiResponse
    {
        public bool Success { get; set; }
        public object? Response { get; set; }
        public string? Message { get; set; }
        public Guid? OwnerId { get; set; }
        public int? TableId { get; set; }
    }

    public class AiDishInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class MenuApiResponse
    {
        public bool Success { get; set; }
        public MenuViewModel? Data { get; set; }
        public string? Message { get; set; }
    }
}
