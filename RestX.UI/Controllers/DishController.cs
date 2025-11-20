using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Controllers
{
    //[Authorize(Roles = "Owner,Staff")]
    [Route("Dish")]
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
        /// Dishes management page - cập nhật để match với DishesManagement.cshtml
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
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

                // Return về đúng view path
                return View("~/Views/Management/DishesManagement.cshtml", dishesManagement);
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

        // ===== CẬP NHẬT CÁC METHOD CRUD ĐỂ MATCH VỚI JAVASCRIPT =====

        /// <summary>
        /// Upsert dish - match với JavaScript saveDish() function
        /// </summary>
        /// <param name="model">Dish data từ form</param>
        /// <param name="ImageFile">Upload image file</param>
        /// <returns></returns>
        [HttpPost("Upsert")]
        //[Authorize(Roles = "Owner")]
        public async Task<IActionResult> UpsertDish([FromForm] DishUpsertModel model, IFormFile? ImageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                // Convert to DishViewModel for service call
                var dishViewModel = new DishViewModel
                {
                    Id = model.Id ?? 0,
                    Name = model.Name,
                    CategoryId = model.CategoryId,
                    CategoryName = model.CategoryName ?? "",
                    Description = model.Description,
                    Price = model.Price,
                    IsActive = model.IsActive
                };

                // Handle image file upload if provided
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // TODO: Implement image upload logic
                    // dishViewModel.ImageUrl = await UploadImageAsync(ImageFile);
                }

                // Determine if create or update
                if (model.Id.HasValue && model.Id.Value > 0)
                {
                    var (success, message) = await _dishService.UpdateDishAsync(dishViewModel);
                    return Json(new { success, message = message ?? (success ? "Dish updated successfully!" : "Failed to update dish") });
                }
                else
                {
                    var (success, message) = await _dishService.CreateDishAsync(dishViewModel);
                    return Json(new { success, message = message ?? (success ? "Dish created successfully!" : "Failed to create dish") });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting dish: {DishName}", model.Name);
                return Json(new { success = false, message = "An error occurred while saving dish" });
            }
        }

        /// <summary>
        /// Get dish details - match với JavaScript viewDish() và editDish() functions
        /// </summary>
        /// <param name="id">Dish ID</param>
        /// <returns></returns>
        [HttpGet("Detail/{id:int}")]
        public async Task<IActionResult> DishDetail(int id)
        {
            try
            {
                var dish = await _dishService.GetDishByIdAsync(id);

                if (dish == null)
                {
                    return Json(new { success = false, message = "Dish not found" });
                }

                return Json(new { success = true, data = dish });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dish details for ID: {DishId}", id);
                return Json(new { success = false, message = "An error occurred while loading dish details" });
            }
        }

        /// <summary>
        /// Delete dish - match với JavaScript deleteDish() function
        /// </summary>
        /// <param name="id">Dish ID</param>
        /// <returns></returns>
        [HttpDelete("Delete/{id:int}")]
        //[Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteDish(int id)
        {
            try
            {
                var success = await _dishService.DeleteDishAsync(id);

                if (success)
                {
                    return Json(new { success = true, message = "Dish has been deleted." });
                }

                return Json(new { success = false, message = "Failed to delete dish" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dish: {DishId}", id);
                return Json(new { success = false, message = "An error occurred while deleting dish" });
            }
        }

        // ===== CATEGORY MANAGEMENT ĐỂ MATCH VỚI JAVASCRIPT =====

        /// <summary>
        /// Get categories - match với JavaScript loadCategories() function
        /// </summary>
        /// <returns></returns>
        [HttpGet("Categories")]
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
        /// Create category - match với JavaScript saveCategory() function
        /// </summary>
        /// <param name="model">Category data</param>
        /// <returns></returns>
        [HttpPost("Category/Create")]
        //[Authorize(Roles = "Owner")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateModel model)
        {
            try
            {
                if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Name))
                {
                    return Json(new { success = false, message = "Category name is required" });
                }

                var categoryViewModel = new CategoryViewModel
                {
                    CategoryName = model.Name.Trim(),
                    IsActive = true
                };

                var (success, message) = await _dishService.CreateCategoryAsync(categoryViewModel);

                return Json(new { success, message = message ?? (success ? "Category added successfully!" : "Failed to create category") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {CategoryName}", model.Name);
                return Json(new { success = false, message = "An error occurred while creating category" });
            }
        }

        // ===== GIỮ NGUYÊN CÁC METHOD KHÔNG LIÊN QUAN CRUD =====

        /// <summary>
        /// Get all dishes as JSON - GIỮ NGUYÊN
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetDishes")]
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
        /// Get dish by ID - GIỮ NGUYÊN (khác với Detail method ở trên)
        /// </summary>
        /// <param name="dishId">Dish ID</param>
        /// <returns></returns>
        [HttpGet("GetDish/{dishId:int}")]
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
        /// Update dish availability - GIỮ NGUYÊN
        /// </summary>
        /// <param name="dishId">Dish ID</param>
        /// <param name="isActive">Availability status</param>
        /// <returns></returns>
        [HttpPost("UpdateAvailability")]
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
        /// Update category - GIỮ NGUYÊN
        /// </summary>
        /// <param name="model">Updated category data</param>
        /// <returns></returns>
        [HttpPost("Category/Update")]
        //[Authorize(Roles = "Owner")]
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
        /// Delete category - GIỮ NGUYÊN
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns></returns>
        [HttpPost("Category/Delete")]
        //[Authorize(Roles = "Owner")]
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
        /// Search dishes - GIỮ NGUYÊN
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns></returns>
        [HttpGet("Search")]
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

        /// <summary>
        /// Error handling - GIỮ NGUYÊN
        /// </summary>
        /// <returns></returns>
        public IActionResult Error()
        {
            return View("Error", new ErrorViewModel
            {
                Message = "An unexpected error occurred"
            });
        }
    }

    // ===== VIEWMODELS =====

    /// <summary>
    /// Form model cho Dish Upsert operation
    /// </summary>
    public class DishUpsertModel
    {
        public int? Id { get; set; }  // Null cho create, có value cho update
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
        // ImageFile sẽ được handle riêng trong method parameter
    }

    /// <summary>
    /// Model cho Category creation
    /// </summary>
    public class CategoryCreateModel
    {
        public string Name { get; set; } = string.Empty;
    }
}