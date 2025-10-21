using Microsoft.AspNetCore.Mvc;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Controllers
{
    public class MenuController : Controller
    {
        private readonly IMenuUIService _menuService;
        private readonly ILogger<MenuController> _logger;

        public MenuController(IMenuUIService menuService, ILogger<MenuController> logger)
        {
            _menuService = menuService;
            _logger = logger;
        }

        /// <summary>
        /// Display restaurant menu
        /// </summary>
        /// <param name="ownerId">Restaurant owner ID</param>
        /// <param name="tableId">Table ID</param>
        /// <returns></returns>
        [HttpGet]
        [Route("Menu/Index/{ownerId:guid}/{tableId:int}")]
        public async Task<IActionResult> Index(Guid ownerId, int tableId)
        {
            try
            {
                _logger.LogInformation("Loading menu for owner: {OwnerId}, table: {TableId}", ownerId, tableId);
                
                var menu = await _menuService.GetMenuAsync(ownerId, tableId);
                
                if (menu == null || !string.IsNullOrEmpty(menu.ErrorMessage))
                {
                    _logger.LogWarning("Failed to load menu for owner: {OwnerId}, table: {TableId}", ownerId, tableId);
                    return View("Error", new ErrorViewModel 
                    { 
                        Message = menu?.ErrorMessage ?? "Unable to load menu",
                        StatusCode = 404
                    });
                }

                // Store context for cart operations
                HttpContext.Session.SetString("OwnerId", ownerId.ToString());
                HttpContext.Session.SetString("TableId", tableId.ToString());

                return View(menu);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading menu for owner: {OwnerId}, table: {TableId}", ownerId, tableId);
                return View("Error", new ErrorViewModel 
                { 
                    Message = "An error occurred while loading the menu"
                });
            }
        }

        /// <summary>
        /// Get dish details by ID
        /// </summary>
        /// <param name="dishId">Dish ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetDishDetails(int dishId)
        {
            try
            {
                var dish = await _menuService.GetDishByIdAsync(dishId);
                
                if (dish == null)
                {
                    return NotFound(new { success = false, message = "Dish not found" });
                }

                return Json(new { success = true, data = dish });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dish details for ID: {DishId}", dishId);
                return Json(new { success = false, message = "Error loading dish details" });
            }
        }

        /// <summary>
        /// Search dishes by name
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> SearchDishes(string searchTerm)
        {
            try
            {
                var ownerIdString = HttpContext.Session.GetString("OwnerId");
                if (!Guid.TryParse(ownerIdString, out var ownerId))
                {
                    return Json(new { success = false, message = "Invalid restaurant context" });
                }

                var dishes = await _menuService.SearchDishesAsync(ownerId, searchTerm);
                
                return Json(new { success = true, data = dishes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching dishes with term: {SearchTerm}", searchTerm);
                return Json(new { success = false, message = "Error searching dishes" });
            }
        }

        /// <summary>
        /// Get dishes by category
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetDishesByCategory(int categoryId)
        {
            try
            {
                var dishes = await _menuService.GetDishesByCategoryAsync(categoryId);
                
                return Json(new { success = true, data = dishes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dishes by category: {CategoryId}", categoryId);
                return Json(new { success = false, message = "Error loading dishes" });
            }
        }
    }
}