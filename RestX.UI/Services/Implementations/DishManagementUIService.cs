using RestX.UI.Models.ApiModels;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Services.Implementations
{
    public class DishManagementUIService : IDishManagementUIService
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly ILogger<DishManagementUIService> _logger;

        public DishManagementUIService(
            IApiService apiService,
            IAuthService authService,
            ILogger<DishManagementUIService> logger)
        {
            _apiService = apiService;
            _authService = authService;
            _logger = logger;
        }

        public async Task<DishesManagementViewModel?> GetDishesManagementAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var ownerId = currentUser?.OwnerId ?? currentUser?.Id;

                if (ownerId == null)
                {
                    _logger.LogWarning("No owner context found for dishes management");
                    return new DishesManagementViewModel
                    {
                        ErrorMessage = "Owner context not found"
                    };
                }

                // Fix: Gọi API và expect DishesManagementViewModel thay vì List<DishApiModel>
                var response = await _apiService.GetAsync<ApiResponse<DishesManagementViewModel>>($"api/Dish/owner/{ownerId}");

                if (response?.Success == true && response.Data != null)
                {
                    // API đã trả về full DishesManagementViewModel rồi, chỉ cần convert
                    var result = response.Data;

                    // Convert dishes từ API format sang UI format nếu cần
                    if (result.Dishes != null)
                    {
                        // Nếu cần mapping từ API model sang UI model
                        var uiDishes = result.Dishes.Select(d => new DishViewModel
                        {
                            Id = d.Id,
                            Name = d.Name,
                            Description = d.Description,
                            Price = d.Price,
                            IsActive = d.IsActive,
                            CategoryId = d.CategoryId,        // Add this
                            CategoryName = d.CategoryName,    // Add this
                            ImageUrl = d.ImageUrl,           // Add this if missing
                            CreatedDate = d.CreatedDate,     // Add this if missing
                            ModifiedDate = d.ModifiedDate    // Add this if missing
                        }).ToList();
                        result.Dishes = uiDishes;
                    }

                    return result;
                }

                _logger.LogWarning("Failed to get dishes management for owner: {OwnerId}", ownerId);
                return new DishesManagementViewModel
                {
                    ErrorMessage = "Failed to load dishes management data"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dishes management data");
                return new DishesManagementViewModel
                {
                    ErrorMessage = "An error occurred while loading dishes management data"
                };
            }
        }

        public async Task<List<DishViewModel>> GetDishesAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var ownerId = currentUser?.OwnerId ?? currentUser?.Id;
                
                if (ownerId == null)
                {
                    _logger.LogWarning("No owner context found for dishes");
                    return new List<DishViewModel>();
                }

                var response = await _apiService.GetAsync<ApiResponse<List<DishApiModel>>>($"api/dish/owner/{ownerId}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return response.Data.Select(MapToDishViewModel).ToList();
                }
                
                _logger.LogWarning("Failed to get dishes for owner: {OwnerId}", ownerId);
                return new List<DishViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dishes");
                return new List<DishViewModel>();
            }
        }

        public async Task<DishViewModel?> GetDishByIdAsync(int dishId)
        {
            try
            {
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

        public async Task<(bool Success, string? Message)> CreateDishAsync(DishViewModel model)
        {
            try
            {
                var createData = new DishRequest
                {
                    CategoryId = model.CategoryId,
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    ImageUrl = model.ImageUrl,
                    IsActive = model.IsActive ?? true
                };

                var response = await _apiService.PostAsync<DishRequest, ApiResponse>("api/dish", createData);
                
                if (response?.Success == true)
                {
                    return (true, response.Message ?? "Dish created successfully");
                }
                
                return (false, response?.Message ?? "Failed to create dish");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating dish: {DishName}", model.Name);
                return (false, "An error occurred while creating the dish");
            }
        }

        public async Task<(bool Success, string? Message)> UpdateDishAsync(DishViewModel model)
        {
            try
            {
                var updateData = new DishRequest
                {
                    Id = model.Id,
                    CategoryId = model.CategoryId,
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    ImageUrl = model.ImageUrl,
                    IsActive = model.IsActive ?? true
                };

                var response = await _apiService.PutAsync<DishRequest, ApiResponse>($"api/dish/{model.Id}", updateData);
                
                if (response?.Success == true)
                {
                    return (true, response.Message ?? "Dish updated successfully");
                }
                
                return (false, response?.Message ?? "Failed to update dish");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dish ID: {DishId}", model.Id);
                return (false, "An error occurred while updating the dish");
            }
        }

        public async Task<bool> DeleteDishAsync(int dishId)
        {
            try
            {
                var success = await _apiService.DeleteAsync($"api/dish/{dishId}");
                
                if (!success)
                {
                    _logger.LogWarning("Failed to delete dish: {DishId}", dishId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dish: {DishId}", dishId);
                return false;
            }
        }

        public async Task<bool> UpdateDishAvailabilityAsync(int dishId, bool isActive)
        {
            try
            {
                var updateData = new
                {
                    DishId = dishId,
                    IsActive = isActive
                };

                var response = await _apiService.PutAsync<object, ApiResponse>("api/dish/availability", updateData);
                
                if (response?.Success == true)
                {
                    _logger.LogInformation("Dish availability updated: {DishId} -> {IsActive}", dishId, isActive);
                    return true;
                }
                
                _logger.LogWarning("Failed to update dish availability: {DishId}", dishId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dish availability: {DishId}", dishId);
                return false;
            }
        }

        public async Task<List<Models.ViewModels.CategoryViewModel>> GetCategoriesAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var ownerId = currentUser?.OwnerId ?? currentUser?.Id;
                
                if (ownerId == null)
                {
                    _logger.LogWarning("No owner context found for categories");
                    return new List<CategoryViewModel>();
                }

                var response = await _apiService.GetAsync<ApiResponse<List<CategoryApiModel>>>($"api/Category");
                
                if (response?.Success == true && response.Data != null)
                {
                    return response.Data.Select(MapToCategoryViewModel).ToList();
                }
                
                _logger.LogWarning("Failed to get categories for owner: {OwnerId}", ownerId);
                return new List<Models.ViewModels.CategoryViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return new List<Models.ViewModels.CategoryViewModel>();
            }
        }

        public async Task<(bool Success, string? Message)> CreateCategoryAsync(Models.ViewModels.CategoryViewModel model)
        {
            try
            {
                var createData = new CategoryRequest
                {
                    Name = model.CategoryName,
                    Description = model.CategoryName, // Using name as description if no description field
                    IsActive = model.IsActive
                };

                var response = await _apiService.PostAsync<CategoryRequest, ApiResponse>("api/category", createData);
                
                if (response?.Success == true)
                {
                    return (true, response.Message ?? "Category created successfully");
                }
                
                return (false, response?.Message ?? "Failed to create category");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {CategoryName}", model.CategoryName);
                return (false, "An error occurred while creating the category");
            }
        }

        public async Task<(bool Success, string? Message)> UpdateCategoryAsync(Models.ViewModels.CategoryViewModel model)
        {
            try
            {
                var updateData = new CategoryRequest
                {
                    Id = model.Id,
                    Name = model.CategoryName,
                    Description = model.CategoryName,
                    IsActive = model.IsActive
                };

                var response = await _apiService.PutAsync<CategoryRequest, ApiResponse>($"api/category/{model.Id}", updateData);
                
                if (response?.Success == true)
                {
                    return (true, response.Message ?? "Category updated successfully");
                }
                
                return (false, response?.Message ?? "Failed to update category");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category ID: {CategoryId}", model.Id);
                return (false, "An error occurred while updating the category");
            }
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            try
            {
                var success = await _apiService.DeleteAsync($"api/category/{categoryId}");
                
                if (!success)
                {
                    _logger.LogWarning("Failed to delete category: {CategoryId}", categoryId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", categoryId);
                return false;
            }
        }

        #region Private Mapping Methods

        private DishViewModel MapToDishViewModel(DishApiModel apiModel)
        {
            return new DishViewModel
            {
                Id = apiModel.Id,
                CategoryId = apiModel.CategoryId,
                CategoryName = apiModel.CategoryName,
                Name = apiModel.Name,
                Description = apiModel.Description,
                Price = apiModel.Price,
                ImageUrl = apiModel.ImageUrl,
                IsActive = apiModel.IsActive,
                CreatedDate = apiModel.CreatedDate,
                ModifiedDate = apiModel.ModifiedDate
            };
        }

        private Models.ViewModels.CategoryViewModel MapToCategoryViewModel(CategoryApiModel apiModel)
        {
            return new Models.ViewModels.CategoryViewModel
            {
                Id = apiModel.Id,
                CategoryName = apiModel.Name,
                IsActive = apiModel.IsActive
            };
        }

        #endregion
    }
}
