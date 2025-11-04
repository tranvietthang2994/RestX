using RestX.UI.Models.ApiModels;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Services.Implementations
{
    public class MenuUIService : IMenuUIService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<MenuUIService> _logger;
        public MenuUIService(IApiService apiService, ILogger<MenuUIService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<MenuViewModel?> GetMenuAsync(Guid ownerId, int tableId)
        {
            try
            {
                _logger.LogInformation("Getting menu for owner: {OwnerId}, table: {TableId}", ownerId, tableId);
                
                var response = await _apiService.GetAsync<ApiResponse<MenuApiModel>>($"api/menu/restaurant/{ownerId}/table/{tableId}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return MapToMenuViewModel(response.Data);
                }
                
                _logger.LogWarning("Failed to get menu for owner: {OwnerId}, table: {TableId}", ownerId, tableId);
                return new MenuViewModel 
                { 
                    OwnerId = ownerId, 
                    TableId = tableId,
                    ErrorMessage = response?.Message ?? "Failed to load menu"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting menu for owner: {OwnerId}, table: {TableId}", ownerId, tableId);
                return new MenuViewModel 
                { 
                    OwnerId = ownerId, 
                    TableId = tableId,
                    ErrorMessage = "An error occurred while loading the menu"
                };
            }
        }

        public async Task<MenuViewModel?> GetMenuByOwnerAsync(Guid ownerId)
        {
            try
            {
                _logger.LogInformation("Getting menu for owner: {OwnerId}", ownerId);
                
                var response = await _apiService.GetAsync<ApiResponse<MenuApiModel>>($"api/menu/restaurant/{ownerId}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return MapToMenuViewModel(response.Data);
                }
                
                _logger.LogWarning("Failed to get menu for owner: {OwnerId}", ownerId);
                return new MenuViewModel 
                { 
                    OwnerId = ownerId,
                    ErrorMessage = response?.Message ?? "Failed to load menu"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting menu for owner: {OwnerId}", ownerId);
                return new MenuViewModel 
                { 
                    OwnerId = ownerId,
                    ErrorMessage = "An error occurred while loading the menu"
                };
            }
        }

        public async Task<DishViewModel?> GetDishByIdAsync(int dishId)
        {
            try
            {
                _logger.LogInformation("Getting dish by ID: {DishId}", dishId);
                
                var response = await _apiService.GetAsync<ApiResponse<DishApiModel>>($"api/dish/{dishId}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return MapToDishViewModel(response.Data);
                }
                
                _logger.LogWarning("Failed to get dish by ID: {DishId}", dishId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dish by ID: {DishId}", dishId);
                return null;
            }
        }

        public async Task<List<DishViewModel>> SearchDishesAsync(Guid ownerId, string searchTerm)
        {
            try
            {
                _logger.LogInformation("Searching dishes for owner: {OwnerId}, term: {SearchTerm}", ownerId, searchTerm);
                
                var response = await _apiService.GetAsync<ApiResponse<List<DishApiModel>>>($"api/dish/search?ownerId={ownerId}&term={Uri.EscapeDataString(searchTerm)}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return response.Data.Select(MapToDishViewModel).ToList();
                }
                
                _logger.LogWarning("Failed to search dishes for owner: {OwnerId}, term: {SearchTerm}", ownerId, searchTerm);
                return new List<DishViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching dishes for owner: {OwnerId}, term: {SearchTerm}", ownerId, searchTerm);
                return new List<DishViewModel>();
            }
        }

        public async Task<List<DishViewModel>> GetDishesByCategoryAsync(int categoryId)
        {
            try
            {
                _logger.LogInformation("Getting dishes by category: {CategoryId}", categoryId);
                
                var response = await _apiService.GetAsync<ApiResponse<List<DishApiModel>>>($"api/dish/category/{categoryId}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return response.Data.Select(MapToDishViewModel).ToList();
                }
                
                _logger.LogWarning("Failed to get dishes by category: {CategoryId}", categoryId);
                return new List<DishViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dishes by category: {CategoryId}", categoryId);
                return new List<DishViewModel>();
            }
        }

        #region Private Mapping Methods

        private MenuViewModel MapToMenuViewModel(MenuApiModel apiModel)
        {
            return new MenuViewModel
            {
                OwnerId = apiModel.OwnerId,
                TableId = apiModel.TableId,
                Categories = apiModel.Categories?.Select(MapToCategoryViewModel).ToList() ?? new List<CategoryViewModel>()
            };
        }

        private CategoryViewModel MapToCategoryViewModel(CategoryApiDto apiCategory)
        {
            return new CategoryViewModel
            {
                Id = apiCategory.Id,
                CategoryName = apiCategory.CategoryName,
                Dishes = apiCategory.Dishes?.Select(MapToDishViewModel).ToList() ?? new List<DishViewModel>()
            };
        }

        private DishViewModel MapToDishViewModel(DishApiModel apiDish)
        {
            return new DishViewModel
            {
                Id = apiDish.Id,
                CategoryId = apiDish.CategoryId,
                CategoryName = apiDish.CategoryName,
                Name = apiDish.Name,
                Description = apiDish.Description,
                Price = apiDish.Price,
                ImageUrl = apiDish.ImageUrl,
                IsActive = apiDish.IsActive,
                CreatedDate = apiDish.CreatedDate,
                ModifiedDate = apiDish.ModifiedDate
            };
        }

        #endregion
    }
}
