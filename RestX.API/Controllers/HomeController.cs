using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestX.API.Services.Interfaces;

namespace RestX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class HomeController : ControllerBase
    {

        private readonly IHomeService _homeService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IHomeService homeService, ILogger<HomeController> logger)
        {
            _homeService = homeService;
            _logger = logger;
        }

        /// <summary>
        /// Hiển thị trang chủ của nhà hàng
        /// </summary>
        /// <param name="ownerId">ID của chủ nhà hàng.</param>
        /// <param name="tableId">ID của bàn khách đang ngồi.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Index/{ownerId:guid}/{tableId:int}")]
        public async Task<IActionResult> Index(Guid ownerId, int tableId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("HomeController.Index called with ownerId: {OwnerId}, tableId: {TableId}", ownerId, tableId);
                
                // Set RestaurantContext in HttpContext.Items so BaseService can access it
                var restaurantContext = new RestX.API.Models.Entities.RestaurantContext
                {
                    OwnerId = ownerId,
                    TableId = tableId
                };
                HttpContext.Items["RestaurantContext"] = restaurantContext;
                _logger.LogDebug("RestaurantContext set: OwnerId={OwnerId}, TableId={TableId}", ownerId, tableId);

                var viewModel = await _homeService.GetHomeViewsAsync(cancellationToken);
                if (viewModel == null)
                {
                    _logger.LogWarning("HomeService returned null for ownerId: {OwnerId}, tableId: {TableId}", ownerId, tableId);
                    return NotFound("Không tìm thấy thông tin nhà hàng.");
                }

                _logger.LogInformation("HomeController.Index completed successfully for ownerId: {OwnerId}, tableId: {TableId}", ownerId, tableId);
                return Ok(new { success = true, data = viewModel });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HomeController.Index for ownerId: {OwnerId}, tableId: {TableId}. Exception: {ExceptionMessage}", 
                    ownerId, tableId, ex.Message);
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while processing your request.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
