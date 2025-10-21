using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Controllers
{
    [Authorize(Roles = "Owner,Staff")]
    public class DishController : Controller
    {
        private readonly IDishManagementUIService _dishService;
        private readonly IMenuUIService _menuService;
        private readonly ILogger<DishController> _logger;

        public DishController(
            IDishManagementUIService dishService,
            IMenuUIService menuService,
            ILogger<DishController> logger)
        {
            _dishService = dishService;
            _menuService = menuService;
            _logger = logger;
        }

        /// <summary>
        /// Dishes management page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> DishesManagement()
        {
            try
            {
                _logger.LogInformation("Loading dishes management page");
                
                var dishesManagement = await _dishService.GetDishesManagementAsync();
                
                if (dishesManagement == null)
                {
                    return View("Error", new ErrorViewModel 
                    { 
                        Message = "Unable to load dishes management data"
                    });
                }

                if (!string.IsNullOrEmpty(dishesManagement.ErrorMessage))
                {
                    return View("Error", new ErrorViewModel 
                    { 
                        Message = dishesManagement.ErrorMessage
                    });
                }

                return View(dishesManagement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dishes management page");
                return View("Error", new ErrorViewModel 
                { 
                    Message = "An error occurred while loading dishes management"
                });
            }
        }

        /// <summary>
        /// Get all dishes as JSON
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetDishes()
        {
            try
            {
                var dishes = await _dishService.GetDishesAsync();
                return Json(new { success = true, data = dishes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dishes");
                return Json(new { success = false, message = "An error occurred while loading dishes" });
            }
        }

        /// <summary>
        /// Get dish by ID
        /// </summary>
        /// <param name="dishId">Dish ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetDish(int dishId)
        {
            try
            {
                var dish = await _dishService.GetDishByIdAsync(dishId);
                
                if (dish != null)
                {
                    return Json(new { success = true, data = dish });
                }
                
                return Json(new { success = false, message = "Dish not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dish by ID: {DishId}", dishId);
                return Json(new { success = false, message = "An error occurred while loading dish data" });
            }
        }

        /// <summary>
        /// Create new dish
        /// </summary>
        /// <param name="model">Dish data</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> CreateDish(DishViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var (success, message) = await _dishService.CreateDishAsync(model);
                
                return Json(new { success, message = message ?? (success ? "Dish created successfully" : "Failed to create dish") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating dish: {DishName}", model.Name);
                return Json(new { success = false, message = "An error occurred while creating the dish" });
            }
        }

        /// <summary>
        /// Update dish
        /// </summary>
        /// <param name="model">Updated dish data</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> UpdateDish(DishViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var (success, message) = await _dishService.UpdateDishAsync(model);
                
                return Json(new { success, message = message ?? (success ? "Dish updated successfully" : "Failed to update dish") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dish ID: {DishId}", model.Id);
                return Json(new { success = false, message = "An error occurred while updating the dish" });
            }
        }

        /// <summary>
        /// Delete dish
        /// </summary>
        /// <param name="dishId">Dish ID</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteDish(int dishId)
        {
            try
            {
                var success = await _dishService.DeleteDishAsync(dishId);
                
                if (success)
                {
                    return Json(new { success = true, message = "Dish deleted successfully" });
                }
                
                return Json(new { success = false, message = "Failed to delete dish" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dish: {DishId}", dishId);
                return Json(new { success = false, message = "An error occurred while deleting the dish" });
            }
        }

        /// <summary>
        /// Update dish availability
        /// </summary>
        /// <param name="dishId">Dish ID</param>
        /// <param name="isActive">Availability status</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateDishAvailability(int dishId, bool isActive)
        {
            try
            {
                var success = await _dishService.UpdateDishAvailabilityAsync(dishId, isActive);
                
                if (success)
                {
                    return Json(new { success = true, message = "Dish availability updated successfully" });
                }
                
                return Json(new { success = false, message = "Failed to update dish availability" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dish availability: {DishId} -> {IsActive}", dishId, isActive);
                return Json(new { success = false, message = "An error occurred while updating dish availability" });
            }
        }

        /// <summary>
        /// Get categories for dropdown
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _dishService.GetCategoriesAsync();
                return Json(new { success = true, data = categories });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return Json(new { success = false, message = "An error occurred while loading categories" });
            }
        }

        /// <summary>
        /// Create new category
        /// </summary>
        /// <param name="model">Category data</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> CreateCategory(CategoryViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var (success, message) = await _dishService.CreateCategoryAsync(model);
                
                return Json(new { success, message = message ?? (success ? "Category created successfully" : "Failed to create category") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {CategoryName}", model.CategoryName);
                return Json(new { success = false, message = "An error occurred while creating the category" });
            }
        }

        /// <summary>
        /// Update category
        /// </summary>
        /// <param name="model">Updated category data</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> UpdateCategory(CategoryViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var (success, message) = await _dishService.UpdateCategoryAsync(model);
                
                return Json(new { success, message = message ?? (success ? "Category updated successfully" : "Failed to update category") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category ID: {CategoryId}", model.Id);
                return Json(new { success = false, message = "An error occurred while updating the category" });
            }
        }

        /// <summary>
        /// Delete category
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            try
            {
                var success = await _dishService.DeleteCategoryAsync(categoryId);
                
                if (success)
                {
                    return Json(new { success = true, message = "Category deleted successfully" });
                }
                
                return Json(new { success = false, message = "Failed to delete category" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", categoryId);
                return Json(new { success = false, message = "An error occurred while deleting the category" });
            }
        }

        /// <summary>
        /// Search dishes
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> SearchDishes(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return Json(new { success = true, data = new List<DishViewModel>() });
                }

                // Get current user context for owner ID
                var ownerId = Guid.NewGuid(); // This should be retrieved from authentication context
                var dishes = await _menuService.SearchDishesAsync(ownerId, searchTerm);
                
                return Json(new { success = true, data = dishes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching dishes with term: {SearchTerm}", searchTerm);
                return Json(new { success = false, message = "An error occurred while searching dishes" });
            }
        }
    }
}