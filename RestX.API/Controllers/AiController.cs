using Microsoft.AspNetCore.Mvc;
using RestX.API.Services.Interfaces;

namespace RestX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly IAiService aiService;
        private readonly IExceptionHandler exceptionHandler;

        public AiController(
            IAiService aiService, 
            IExceptionHandler exceptionHandler)
        {
            this.aiService = aiService;
            this.exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Lấy gợi ý món ăn từ AI
        /// </summary>
        /// <param name="request">Yêu cầu chat với thông tin context</param>
        /// <returns>Gợi ý món ăn từ AI</returns>
        [HttpPost("suggestions")]
        public async Task<IActionResult> GetDishSuggestions([FromBody] AiSuggestionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                if (string.IsNullOrWhiteSpace(request.Message))
                    return BadRequest(new { success = false, message = "Message is required" });

                var ownerId = RestX.API.Extensions.UserHelper.GetCurrentOwnerId();
                var response = await aiService.GetDishSuggestionsAsync(request.Message, ownerId);
                
                return Ok(new { 
                    success = true, 
                    response = response,
                    ownerId = ownerId
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while getting AI dish suggestions.");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Chat với AI
        /// </summary>
        /// <param name="request">Yêu cầu chat</param>
        /// <returns>Phản hồi từ AI</returns>
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] AiChatRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                if (string.IsNullOrWhiteSpace(request.Message))
                    return BadRequest(new { success = false, message = "Message is required" });

                var response = await aiService.ChatWithAiAsync(request.Message, request.ConversationHistory ?? "");
                
                return Ok(new { 
                    success = true, 
                    response = response
                });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while chatting with AI.");
                return StatusCode(500, new { success = false, message = "An error occurred while chatting with AI" });
            }
        }

        /// <summary>
        /// Lấy gợi ý món ăn cho bàn cụ thể
        /// </summary>
        /// <param name="ownerId">ID chủ nhà hàng</param>
        /// <param name="tableId">ID bàn</param>
        /// <param name="request">Yêu cầu gợi ý</param>
        /// <returns>Gợi ý món ăn</returns>
        [HttpPost("suggestions/{ownerId:guid}/table/{tableId:int}")]
        public async Task<IActionResult> GetTableSuggestions(Guid ownerId, int tableId, [FromBody] AiSuggestionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                if (string.IsNullOrWhiteSpace(request.Message))
                    return BadRequest(new { success = false, message = "Message is required" });

                var response = await aiService.GetDishSuggestionsAsync(request.Message, ownerId);
                
                return Ok(new { 
                    success = true, 
                    response = response,
                    ownerId = ownerId,
                    tableId = tableId
                });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, $"An error occurred while getting AI suggestions for table {tableId}.");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Lấy gợi ý menu theo sở thích
        /// </summary>
        /// <param name="request">Preferences và constraints</param>
        /// <returns>Menu suggestions</returns>
        [HttpPost("menu-recommendations")]
        public async Task<IActionResult> GetMenuRecommendations([FromBody] MenuRecommendationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var ownerId = RestX.API.Extensions.UserHelper.GetCurrentOwnerId();
                
                // Build preference message
                var preferenceMessage = $"Customer preferences: {request.Preferences ?? "None"}";
                if (!string.IsNullOrEmpty(request.Dietary))
                    preferenceMessage += $", Dietary restrictions: {request.Dietary}";
                if (!string.IsNullOrEmpty(request.Budget))
                    preferenceMessage += $", Budget: {request.Budget}";
                
                var response = await aiService.GetDishSuggestionsAsync(preferenceMessage, ownerId);
                
                return Ok(new { 
                    success = true, 
                    response = response,
                    preferences = request
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while getting menu recommendations.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Phân tích feedback khách hàng
        /// </summary>
        /// <param name="request">Customer feedback</param>
        /// <returns>AI analysis of feedback</returns>
        [HttpPost("analyze-feedback")]
        public async Task<IActionResult> AnalyzeFeedback([FromBody] FeedbackAnalysisRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                if (string.IsNullOrWhiteSpace(request.Feedback))
                    return BadRequest(new { success = false, message = "Feedback is required" });

                var analysisPrompt = $"Analyze this customer feedback: {request.Feedback}. Provide sentiment analysis and actionable insights.";
                var analysis = await aiService.ChatWithAiAsync(analysisPrompt, "");
                
                return Ok(new { 
                    success = true, 
                    analysis = analysis,
                    feedback = request.Feedback
                });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while analyzing feedback.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Tạo mô tả món ăn với AI
        /// </summary>
        /// <param name="request">Dish information</param>
        /// <returns>AI-generated dish description</returns>
        [HttpPost("generate-description")]
        public async Task<IActionResult> GenerateDishDescription([FromBody] DishDescriptionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                if (string.IsNullOrWhiteSpace(request.DishName))
                    return BadRequest(new { success = false, message = "Dish name is required" });

                var prompt = $"Create an appealing description for dish: {request.DishName}";
                if (!string.IsNullOrEmpty(request.Ingredients))
                    prompt += $" with ingredients: {request.Ingredients}";
                if (!string.IsNullOrEmpty(request.Style))
                    prompt += $", cooking style: {request.Style}";

                var description = await aiService.ChatWithAiAsync(prompt, "");
                
                return Ok(new { 
                    success = true, 
                    description = description,
                    dishInfo = request
                });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while generating dish description.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }

    /// <summary>
    /// Request model cho AI suggestions
    /// </summary>
    public class AiSuggestionRequest
    {
        public string Message { get; set; } = string.Empty;
        public string? Context { get; set; }
    }

    /// <summary>
    /// Request model cho AI chat
    /// </summary>
    public class AiChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public string? ConversationHistory { get; set; }
    }

    /// <summary>
    /// Request model cho menu recommendations
    /// </summary>
    public class MenuRecommendationRequest
    {
        public string? Preferences { get; set; }
        public string? Dietary { get; set; }
        public string? Budget { get; set; }
        public int? PartySize { get; set; }
    }

    /// <summary>
    /// Request model cho feedback analysis
    /// </summary>
    public class FeedbackAnalysisRequest
    {
        public string Feedback { get; set; } = string.Empty;
        public string? CustomerInfo { get; set; }
        public string? OrderInfo { get; set; }
    }

    /// <summary>
    /// Request model cho dish description generation
    /// </summary>
    public class DishDescriptionRequest
    {
        public string DishName { get; set; } = string.Empty;
        public string? Ingredients { get; set; }
        public string? Style { get; set; }
        public string? Cuisine { get; set; }
    }
}
