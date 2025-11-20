using RestX.UI.Models.ApiModels;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Services.Implementations
{
    public class StaffUIService : IStaffUIService
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly ILogger<StaffUIService> _logger;

        public StaffUIService(
            IApiService apiService,
            IAuthService authService,
            ILogger<StaffUIService> logger)
        {
            _apiService = apiService;
            _authService = authService;
            _logger = logger;
        }

        public async Task<List<StaffViewModel>> GetStaffListAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var ownerId = currentUser?.OwnerId ?? currentUser?.Id;
                
                if (ownerId == null)
                {
                    _logger.LogWarning("No owner context found for staff list");
                    return new List<StaffViewModel>();
                }

                var response = await _apiService.GetAsync<ApiResponse<List<StaffApiModel>>>($"api/staff/owner/{ownerId}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return response.Data.Select(MapToStaffViewModel).ToList();
                }
                
                _logger.LogWarning("Failed to get staff list for owner: {OwnerId}", ownerId);
                return new List<StaffViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting staff list");
                return new List<StaffViewModel>();
            }
        }

        public async Task<StaffViewModel?> GetStaffByIdAsync(Guid staffId)
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<StaffApiModel>>($"api/staff/{staffId}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return MapToStaffViewModel(response.Data);
                }
                
                _logger.LogWarning("Failed to get staff by ID: {StaffId}", staffId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting staff by ID: {StaffId}", staffId);
                return null;
            }
        }

        public async Task<StaffProfileViewModel?> GetStaffProfileAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null || currentUser.Role != "Staff" || currentUser.StaffId == null)
                {
                    _logger.LogWarning("User is not authenticated or not a staff member");
                    return null;
                }

                var response = await _apiService.GetAsync<ApiResponse<StaffApiModel>>($"api/staff/{currentUser.StaffId}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return MapToStaffProfileViewModel(response.Data, currentUser);
                }
                
                _logger.LogWarning("Failed to get staff profile for user: {UserId}", currentUser.Id);
                return new StaffProfileViewModel 
                { 
                    ErrorMessage = response?.Message ?? "Failed to load staff profile" 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting staff profile");
                return new StaffProfileViewModel 
                { 
                    ErrorMessage = "An error occurred while loading the profile" 
                };
            }
        }

        public async Task<(bool Success, string? Message)> UpdateStaffProfileAsync(StaffProfileViewModel model)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null || currentUser.Role != "Staff" || currentUser.StaffId == null)
                {
                    return (false, "Unauthorized access");
                }

                var updateData = new
                {
                    Id = model.Id,
                    Name = model.Name,
                    Email = model.Email,
                    Phone = model.Phone,
                    Position = model.Position,
                    CurrentPassword = model.CurrentPassword,
                    NewPassword = model.NewPassword
                };

                var response = await _apiService.PutAsync<object, ApiResponse>($"api/staff/{model.Id}", updateData);
                
                if (response?.Success == true)
                {
                    return (true, response.Message ?? "Profile updated successfully");
                }
                
                return (false, response?.Message ?? "Failed to update profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating staff profile for ID: {StaffId}", model.Id);
                return (false, "An error occurred while updating the profile");
            }
        }

        public async Task<(bool Success, string? Message)> CreateStaffAsync(StaffViewModel model)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var ownerId = currentUser?.OwnerId ?? currentUser?.Id;
                
                if (ownerId == null)
                {
                    return (false, "Owner context not found");
                }

                var createData = new
                {
                    OwnerId = ownerId,
                    Name = model.Name,
                    Email = model.Email,
                    Phone = model.Phone,
                    Position = model.Position,
                    Salary = model.Salary,
                    Username = model.Username,
                    Password = model.Password,
                    IsActive = model.IsActive ?? true
                };

                var response = await _apiService.PostAsync<object, ApiResponse>("api/staff", createData);
                
                if (response?.Success == true)
                {
                    return (true, response.Message ?? "Staff member created successfully");
                }
                
                return (false, response?.Message ?? "Failed to create staff member");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating staff member: {StaffName}", model.Name);
                return (false, "An error occurred while creating the staff member");
            }
        }

        public async Task<(bool Success, string? Message)> UpdateStaffAsync(StaffViewModel model)
        {
            try
            {
                var updateData = new
                {
                    Id = model.Id,
                    Name = model.Name,
                    Email = model.Email,
                    Phone = model.Phone,
                    Position = model.Position,
                    Salary = model.Salary,
                    IsActive = model.IsActive
                };

                var response = await _apiService.PutAsync<object, ApiResponse>($"api/staff/{model.Id}", updateData);
                
                if (response?.Success == true)
                {
                    return (true, response.Message ?? "Staff member updated successfully");
                }
                
                return (false, response?.Message ?? "Failed to update staff member");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating staff member ID: {StaffId}", model.Id);
                return (false, "An error occurred while updating the staff member");
            }
        }

        public async Task<bool> DeleteStaffAsync(Guid staffId)
        {
            try
            {
                var success = await _apiService.DeleteAsync($"api/staff/{staffId}");
                
                if (!success)
                {
                    _logger.LogWarning("Failed to delete staff member: {StaffId}", staffId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting staff member: {StaffId}", staffId);
                return false;
            }
        }

        public async Task<StaffManagementViewModel?> GetStaffManagementAsync()
        {
            try
            {
                var staffList = await GetStaffListAsync();
                
                return new StaffManagementViewModel
                {
                    StaffList = staffList,
                    TotalStaff = staffList.Count,
                    ActiveStaff = staffList.Count(s => s.IsActive == true)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting staff management data");
                return new StaffManagementViewModel
                {
                    ErrorMessage = "An error occurred while loading staff management data"
                };
            }
        }

        #region Private Mapping Methods

        private StaffViewModel MapToStaffViewModel(StaffApiModel apiModel)
        {
            return new StaffViewModel
            {
                Id = apiModel.Id,
                OwnerId = apiModel.OwnerId,
                Name = apiModel.Name,
                Email = apiModel.Email,
                Phone = apiModel.Phone,
                IsActive = apiModel.IsActive,
                CreatedDate = apiModel.CreatedDate,
                ModifiedDate = apiModel.ModifiedDate
            };
        }

        private StaffProfileViewModel MapToStaffProfileViewModel(StaffApiModel apiModel, UserInfo? userInfo = null)
        {
            return new StaffProfileViewModel
            {
                Id = apiModel.Id,
                Name = apiModel.Name,
                Email = apiModel.Email,
                Phone = apiModel.Phone,
                CreatedDate = apiModel.CreatedDate,
                ModifiedDate = apiModel.ModifiedDate,
                Username = userInfo?.Username
            };
        }

        #endregion
    }
}
