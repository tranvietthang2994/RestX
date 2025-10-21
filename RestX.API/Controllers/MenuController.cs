using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestX.API.Services.Interfaces;

namespace RestX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // Menu should be accessible to everyone (customers)
    public class MenuController : ControllerBase
    {
        private readonly IMenuService menuService;
        private readonly IExceptionHandler exceptionHandler;

        public MenuController(
            IMenuService menuService, 
            IExceptionHandler exceptionHandler)
        {
            this.menuService = menuService;
            this.exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Lấy menu của nhà hàng
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Menu với danh sách món ăn theo danh mục</returns>
        [HttpGet]
        public async Task<IActionResult> GetMenu(CancellationToken cancellationToken = default)
        {
            try
            {
                var menuViewModel = await menuService.GetMenuViewModelAsync(cancellationToken);
                return Ok(new { success = true, data = menuViewModel });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading menu.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy menu theo Owner ID
        /// </summary>
        /// <param name="ownerId">ID chủ nhà hàng</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Menu của nhà hàng cụ thể</returns>
        [HttpGet("restaurant/{ownerId:guid}")]
        public async Task<IActionResult> GetMenuByOwner(Guid ownerId, CancellationToken cancellationToken = default)
        {
            try
            {
                // TODO: Implement GetMenuByOwnerIdAsync in IMenuService
                var menuViewModel = await menuService.GetMenuViewModelAsync(cancellationToken);
                return Ok(new { 
                    success = true, 
                    data = menuViewModel,
                    ownerId = ownerId
                });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, $"An error occurred while loading menu for owner {ownerId}.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy menu cho table cụ thể (có thể có customization theo bàn)
        /// </summary>
        /// <param name="ownerId">ID chủ nhà hàng</param>
        /// <param name="tableId">ID bàn</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Menu cho bàn cụ thể</returns>
        [HttpGet("restaurant/{ownerId:guid}/table/{tableId:int}")]
        public async Task<IActionResult> GetMenuForTable(Guid ownerId, int tableId, CancellationToken cancellationToken = default)
        {
            try
            {
                var menuViewModel = await menuService.GetMenuViewModelAsync(cancellationToken);
                return Ok(new { 
                    success = true, 
                    data = menuViewModel,
                    ownerId = ownerId,
                    tableId = tableId
                });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, $"An error occurred while loading menu for owner {ownerId}, table {tableId}.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy món ăn theo danh mục
        /// </summary>
        /// <param name="categoryId">ID danh mục</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Danh sách món ăn trong danh mục</returns>
        [HttpGet("category/{categoryId:int}")]
        public async Task<IActionResult> GetDishesByCategory(int categoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                // TODO: Implement GetDishesByCategoryAsync in IMenuService
                return Ok(new { 
                    success = true, 
                    message = "GetDishesByCategoryAsync method not implemented yet",
                    categoryId = categoryId
                });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, $"An error occurred while loading dishes for category {categoryId}.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Tìm kiếm món ăn trong menu
        /// </summary>
        /// <param name="searchTerm">Từ khóa tìm kiếm</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Danh sách món ăn phù hợp</returns>
        [HttpGet("search")]
        public async Task<IActionResult> SearchDishes([FromQuery] string searchTerm, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return BadRequest(new { success = false, message = "Search term is required" });

                // TODO: Implement SearchDishesAsync in IMenuService
                return Ok(new { 
                    success = true, 
                    message = "SearchDishesAsync method not implemented yet",
                    searchTerm = searchTerm
                });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, $"An error occurred while searching dishes with term: {searchTerm}.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy món ăn nổi bật/được đề xuất
        /// </summary>
        /// <param name="limit">Số lượng món ăn trả về</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Danh sách món ăn nổi bật</returns>
        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedDishes([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                // TODO: Implement GetFeaturedDishesAsync in IMenuService
                return Ok(new { 
                    success = true, 
                    message = "GetFeaturedDishesAsync method not implemented yet",
                    limit = limit
                });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading featured dishes.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Kiểm tra tình trạng món ăn (available/unavailable)
        /// </summary>
        /// <param name="dishId">ID món ăn</param>
        /// <returns>Tình trạng món ăn</returns>
        [HttpGet("dish/{dishId:int}/availability")]
        public async Task<IActionResult> CheckDishAvailability(int dishId)
        {
            try
            {
                // TODO: Implement CheckDishAvailabilityAsync in IMenuService
                return Ok(new { 
                    success = true, 
                    message = "CheckDishAvailabilityAsync method not implemented yet",
                    dishId = dishId,
                    isAvailable = true // Placeholder
                });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, $"An error occurred while checking availability for dish {dishId}.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }
}
