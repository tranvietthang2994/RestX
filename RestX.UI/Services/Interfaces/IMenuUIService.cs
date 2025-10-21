using RestX.UI.Models.ViewModels;

namespace RestX.UI.Services.Interfaces
{
    public interface IMenuUIService
    {
        /// <summary>
        /// Get menu for restaurant and table
        /// </summary>
        /// <param name="ownerId">Restaurant owner ID</param>
        /// <param name="tableId">Table ID</param>
        /// <returns>Menu view model</returns>
        Task<MenuViewModel?> GetMenuAsync(Guid ownerId, int tableId);

        /// <summary>
        /// Get menu by owner ID only
        /// </summary>
        /// <param name="ownerId">Restaurant owner ID</param>
        /// <returns>Menu view model</returns>
        Task<MenuViewModel?> GetMenuByOwnerAsync(Guid ownerId);

        /// <summary>
        /// Get dish by ID
        /// </summary>
        /// <param name="dishId">Dish ID</param>
        /// <returns>Dish view model</returns>
        Task<DishViewModel?> GetDishByIdAsync(int dishId);

        /// <summary>
        /// Search dishes by name
        /// </summary>
        /// <param name="ownerId">Restaurant owner ID</param>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of dishes</returns>
        Task<List<DishViewModel>> SearchDishesAsync(Guid ownerId, string searchTerm);

        /// <summary>
        /// Get dishes by category
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>List of dishes</returns>
        Task<List<DishViewModel>> GetDishesByCategoryAsync(int categoryId);
    }
}
