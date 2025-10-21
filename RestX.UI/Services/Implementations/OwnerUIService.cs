using RestX.UI.Models.ApiModels;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Services.Implementations
{
    public class OwnerUIService : IOwnerUIService
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly ILogger<OwnerUIService> _logger;

        public OwnerUIService(
            IApiService apiService,
            IAuthService authService,
            ILogger<OwnerUIService> logger)
        {
            _apiService = apiService;
            _authService = authService;
            _logger = logger;
        }

        public async Task<OwnerProfileViewModel?> GetOwnerProfileAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null || currentUser.Role != "Owner")
                {
                    _logger.LogWarning("User is not authenticated or not an owner");
                    return null;
                }

                var response = await _apiService.GetAsync<ApiResponse<OwnerApiModel>>($"api/owner/{currentUser.Id}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return MapToOwnerProfileViewModel(response.Data, currentUser);
                }
                
                _logger.LogWarning("Failed to get owner profile for user: {UserId}", currentUser.Id);
                return new OwnerProfileViewModel 
                { 
                    ErrorMessage = response?.Message ?? "Failed to load owner profile" 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting owner profile");
                return new OwnerProfileViewModel 
                { 
                    ErrorMessage = "An error occurred while loading the profile" 
                };
            }
        }

        public async Task<(bool Success, string? Message)> UpdateOwnerProfileAsync(OwnerProfileViewModel model)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null || currentUser.Role != "Owner")
                {
                    return (false, "Unauthorized access");
                }

                var updateData = new
                {
                    Id = model.Id,
                    Name = model.Name,
                    Address = model.Address,
                    Information = model.Information,
                    Phone = model.Phone,
                    Email = model.Email,
                    Website = model.Website,
                    OpeningHours = model.OpeningHours,
                    IsActive = model.IsActive,
                    CurrentPassword = model.CurrentPassword,
                    NewPassword = model.NewPassword
                };

                var response = await _apiService.PutAsync<object, ApiResponse>($"api/owner/{model.Id}", updateData);
                
                if (response?.Success == true)
                {
                    return (true, response.Message ?? "Profile updated successfully");
                }
                
                return (false, response?.Message ?? "Failed to update profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating owner profile for ID: {OwnerId}", model.Id);
                return (false, "An error occurred while updating the profile");
            }
        }

        public async Task<DashboardViewModel?> GetDashboardAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null || currentUser.Role != "Owner")
                {
                    _logger.LogWarning("User is not authenticated or not an owner");
                    return null;
                }

                var response = await _apiService.GetAsync<ApiResponse<DashboardApiModel>>($"api/owner/dashboard/{currentUser.Id}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return MapToDashboardViewModel(response.Data);
                }
                
                _logger.LogWarning("Failed to get dashboard data for owner: {OwnerId}", currentUser.Id);
                return new DashboardViewModel 
                { 
                    ErrorMessage = response?.Message ?? "Failed to load dashboard data" 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard data");
                return new DashboardViewModel 
                { 
                    ErrorMessage = "An error occurred while loading the dashboard" 
                };
            }
        }

        public async Task<OwnerProfileViewModel?> GetRestaurantInfoAsync(Guid ownerId)
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<OwnerApiModel>>($"api/owner/{ownerId}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return MapToOwnerProfileViewModel(response.Data);
                }
                
                _logger.LogWarning("Failed to get restaurant info for owner: {OwnerId}", ownerId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting restaurant info for owner: {OwnerId}", ownerId);
                return null;
            }
        }

        public async Task<bool> UpdateRestaurantInfoAsync(OwnerProfileViewModel model)
        {
            try
            {
                var updateData = new
                {
                    Id = model.Id,
                    Name = model.Name,
                    Address = model.Address,
                    Information = model.Information,
                    Phone = model.Phone,
                    Email = model.Email,
                    Website = model.Website,
                    OpeningHours = model.OpeningHours,
                    IsActive = model.IsActive
                };

                var response = await _apiService.PutAsync<object, ApiResponse>($"api/owner/{model.Id}", updateData);
                
                return response?.Success == true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating restaurant info for ID: {OwnerId}", model.Id);
                return false;
            }
        }

        #region Private Mapping Methods

        private OwnerProfileViewModel MapToOwnerProfileViewModel(OwnerApiModel apiModel, UserInfo? userInfo = null)
        {
            return new OwnerProfileViewModel
            {
                Id = apiModel.Id,
                Name = apiModel.Name,
                Address = apiModel.Address,
                Information = apiModel.Information,
                IsActive = apiModel.IsActive,
                CreatedDate = apiModel.CreatedDate,
                ModifiedDate = apiModel.ModifiedDate,
                Username = userInfo?.Username
            };
        }

        private DashboardViewModel MapToDashboardViewModel(DashboardApiModel apiModel)
        {
            return new DashboardViewModel
            {
                TotalRevenue = apiModel.TotalRevenue,
                TotalOrders = apiModel.TotalOrders,
                RecentOrders = apiModel.RecentOrders?.Select(MapToRecentOrderViewModel).ToList() ?? new List<RecentOrderViewModel>()
            };
        }

        private RecentOrderViewModel MapToRecentOrderViewModel(RecentOrderApiModel apiModel)
        {
            return new RecentOrderViewModel
            {
                OrderId = apiModel.OrderId,
                CustomerName = apiModel.CustomerName,
                TableName = apiModel.TableName,
                Total = apiModel.Total,
                Status = apiModel.Status,
                OrderDate = apiModel.OrderDate
            };
        }

        #endregion
    }
}
