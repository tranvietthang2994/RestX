using Microsoft.AspNetCore.Mvc;
using RestX.WebApp.Services.Interfaces;
using RestX.WebApp.Helper;
using RestX.WebApp.Models;
using System.Text.Json;

namespace RestX.WebApp.Controllers
{
    public class AiController : BaseController
    {
        private readonly IAiService _aiService;

        public AiController(IAiService aiService, IExceptionHandler exceptionHandler) 
            : base(exceptionHandler)
        {
            _aiService = aiService;
        }

        [HttpGet]
        [Route("Ai/Index/{ownerId:guid}/{tableId:int}")]
        public IActionResult Index(Guid ownerId, int tableId)
        {
            try
            {
                ViewBag.OwnerId = ownerId;
                ViewBag.TableId = tableId;
                return View();
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        public IActionResult Index()
        {
            try
            {
                // Fallback method để lấy từ RestaurantContext nếu không có route parameters
                var restaurantContext = HttpContext.Items["RestaurantContext"] as RestaurantContext;
                if (restaurantContext != null)
                {
                    ViewBag.OwnerId = restaurantContext.OwnerId;
                    ViewBag.TableId = restaurantContext.TableId;
                }
                else
                {
                    // Fallback to UserHelper if RestaurantContext is not available
                    var ownerId = UserHelper.GetCurrentOwnerId();
                    ViewBag.OwnerId = ownerId;
                    ViewBag.TableId = 1; // Default table ID
                }
                return View();
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetSuggestions([FromBody] ChatRequest request)
        {
            try
            {
                var ownerId = UserHelper.GetCurrentOwnerId();
                var response = await _aiService.GetDishSuggestionsAsync(request.Message, ownerId);
                
                return Json(new { success = true, response = response });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Có lỗi xảy ra khi xử lý yêu cầu của bạn." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            try
            {
                var response = await _aiService.ChatWithAiAsync(request.Message, request.ConversationHistory ?? "");
                
                return Json(new { success = true, response = response });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Có lỗi xảy ra khi chat với AI." });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
        public string ConversationHistory { get; set; }
    }
}
