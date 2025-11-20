using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Controllers
{
    [Authorize(Roles = "Owner,Staff")]
    public class CategoryController : Controller
    {
        private readonly IDishManagementUIService _dishService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(
            IDishManagementUIService dishService,
            ILogger<CategoryController> logger)
        {
            _dishService = dishService;
            _logger = logger;
        }

        /// <summary>
        /// Categories management page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading categories management page");
                
                var categories = await _dishService.GetCategoriesAsync();
                
                var model = new CategoryManagementViewModel
                {
                    Categories = categories,
                    TotalCategories = categories.Count,
                    ActiveCategories = categories.Count(c => c.IsActive)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories management page");
                return View("Error", new ErrorViewModel 
                { 
                    Message = "An error occurred while loading categories"
                });
            }
        }

        /// <summary>
        /// Get all categories as JSON
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
        public async Task<IActionResult> Create(CategoryViewModel model)
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
        public async Task<IActionResult> Update(CategoryViewModel model)
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
        public async Task<IActionResult> Delete(int categoryId)
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
        /// Get dishes by category
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetDishes(int categoryId)
        {
            try
            {
                // This would typically call a method to get dishes by category
                // For now, we'll use the menu service
                var dishes = await _dishService.GetDishesAsync();
                var categoryDishes = dishes.Where(d => d.CategoryId == categoryId).ToList();
                
                return Json(new { success = true, data = categoryDishes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dishes by category: {CategoryId}", categoryId);
                return Json(new { success = false, message = "An error occurred while loading dishes" });
            }
        }
    }
}