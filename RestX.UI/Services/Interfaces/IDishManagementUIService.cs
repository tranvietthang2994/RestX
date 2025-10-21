using RestX.UI.Models.ViewModels;

namespace RestX.UI.Services.Interfaces
{
    public interface IDishManagementUIService
    {
        /// <summary>
        /// Get dishes management view model
        /// </summary>
        /// <returns>Dishes management view model</returns>
        Task<DishesManagementViewModel?> GetDishesManagementAsync();

        /// <summary>
        /// Get all dishes for restaurant
        /// </summary>
        /// <returns>List of dishes</returns>
        Task<List<DishViewModel>> GetDishesAsync();

        /// <summary>
        /// Get dish by ID
        /// </summary>
        /// <param name="dishId">Dish ID</param>
        /// <returns>Dish view model</returns>
        Task<DishViewModel?> GetDishByIdAsync(int dishId);

        /// <summary>
        /// Create new dish
        /// </summary>
        /// <param name="model">Dish data</param>
        /// <returns>Success status and message</returns>
        Task<(bool Success, string? Message)> CreateDishAsync(DishViewModel model);

        /// <summary>
        /// Update dish information
        /// </summary>
        /// <param name="model">Updated dish data</param>
        /// <returns>Success status and message</returns>
        Task<(bool Success, string? Message)> UpdateDishAsync(DishViewModel model);

        /// <summary>
        /// Delete dish
        /// </summary>
        /// <param name="dishId">Dish ID</param>
        /// <returns>Success status</returns>
        Task<bool> DeleteDishAsync(int dishId);

        /// <summary>
        /// Update dish availability
        /// </summary>
        /// <param name="dishId">Dish ID</param>
        /// <param name="isActive">Availability status</param>
        /// <returns>Success status</returns>
        Task<bool> UpdateDishAvailabilityAsync(int dishId, bool isActive);

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns>List of categories</returns>
        Task<List<Models.ViewModels.CategoryViewModel>> GetCategoriesAsync();

        /// <summary>
        /// Create new category
        /// </summary>
        /// <param name="model">Category data</param>
        /// <returns>Success status and message</returns>
        Task<(bool Success, string? Message)> CreateCategoryAsync(Models.ViewModels.CategoryViewModel model);

        /// <summary>
        /// Update category information
        /// </summary>
        /// <param name="model">Updated category data</param>
        /// <returns>Success status and message</returns>
        Task<(bool Success, string? Message)> UpdateCategoryAsync(Models.ViewModels.CategoryViewModel model);

        /// <summary>
        /// Delete category
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>Success status</returns>
        Task<bool> DeleteCategoryAsync(int categoryId);
    }
}
