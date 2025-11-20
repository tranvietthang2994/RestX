using RestX.UI.Models.ApiModels;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Services.Implementations
{
    public class CustomerUIService : ICustomerUIService
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly ILogger<CustomerUIService> _logger;

        public CustomerUIService(
            IApiService apiService,
            IAuthService authService,
            ILogger<CustomerUIService> logger)
        {
            _apiService = apiService;
            _authService = authService;
            _logger = logger;
        }

        public async Task<List<CustomerViewModel>> GetCustomersAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var ownerId = currentUser?.OwnerId ?? currentUser?.Id;
                
                if (ownerId == null)
                {
                    _logger.LogWarning("No owner context found for customers");
                    return new List<CustomerViewModel>();
                }

                var response = await _apiService.GetAsync<ApiResponse<List<CustomerApiModel>>>($"api/customer/owner/{ownerId}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return response.Data.Select(MapToCustomerViewModel).ToList();
                }
                
                _logger.LogWarning("Failed to get customers for owner: {OwnerId}", ownerId);
                return new List<CustomerViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers");
                return new List<CustomerViewModel>();
            }
        }

        public async Task<CustomerViewModel?> GetCustomerByIdAsync(Guid customerId)
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<CustomerApiModel>>($"api/customer/{customerId}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return MapToCustomerViewModel(response.Data);
                }
                
                _logger.LogWarning("Failed to get customer by ID: {CustomerId}", customerId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by ID: {CustomerId}", customerId);
                return null;
            }
        }

        public async Task<List<CustomerViewModel>> SearchCustomersAsync(string searchTerm)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var ownerId = currentUser?.OwnerId ?? currentUser?.Id;
                
                if (ownerId == null)
                {
                    _logger.LogWarning("No owner context found for customer search");
                    return new List<CustomerViewModel>();
                }

                var response = await _apiService.GetAsync<ApiResponse<List<CustomerApiModel>>>(
                    $"api/customer/search?ownerId={ownerId}&term={Uri.EscapeDataString(searchTerm)}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return response.Data.Select(MapToCustomerViewModel).ToList();
                }
                
                _logger.LogWarning("Failed to search customers for owner: {OwnerId}, term: {SearchTerm}", ownerId, searchTerm);
                return new List<CustomerViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customers with term: {SearchTerm}", searchTerm);
                return new List<CustomerViewModel>();
            }
        }

        public async Task<(bool Success, string? Message)> CreateCustomerAsync(CustomerViewModel model)
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
                    Name = model.Name,
                    Phone = model.Phone,
                    Email = model.Email,
                    Address = model.Address,
                    DateOfBirth = model.DateOfBirth,
                    OwnerId = ownerId,
                    IsActive = model.IsActive ?? true
                };

                var response = await _apiService.PostAsync<object, ApiResponse>("api/customer", createData);
                
                if (response?.Success == true)
                {
                    return (true, response.Message ?? "Customer created successfully");
                }
                
                return (false, response?.Message ?? "Failed to create customer");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer: {CustomerName}", model.Name);
                return (false, "An error occurred while creating the customer");
            }
        }

        public async Task<(bool Success, string? Message)> UpdateCustomerAsync(CustomerViewModel model)
        {
            try
            {
                var updateData = new
                {
                    Id = model.Id,
                    Name = model.Name,
                    Phone = model.Phone,
                    Email = model.Email,
                    Address = model.Address,
                    DateOfBirth = model.DateOfBirth,
                    Point = model.Point,
                    IsActive = model.IsActive
                };

                var response = await _apiService.PutAsync<object, ApiResponse>($"api/customer/{model.Id}", updateData);
                
                if (response?.Success == true)
                {
                    return (true, response.Message ?? "Customer updated successfully");
                }
                
                return (false, response?.Message ?? "Failed to update customer");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer ID: {CustomerId}", model.Id);
                return (false, "An error occurred while updating the customer");
            }
        }

        public async Task<bool> DeleteCustomerAsync(Guid customerId)
        {
            try
            {
                var success = await _apiService.DeleteAsync($"api/customer/{customerId}");
                
                if (!success)
                {
                    _logger.LogWarning("Failed to delete customer: {CustomerId}", customerId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer: {CustomerId}", customerId);
                return false;
            }
        }

        public async Task<CustomerManagementViewModel?> GetCustomerManagementAsync()
        {
            try
            {
                var customers = await GetCustomersAsync();
                
                return new CustomerManagementViewModel
                {
                    Customers = customers,
                    TotalCustomers = customers.Count,
                    ActiveCustomers = customers.Count(c => c.IsActive == true),
                    NewCustomersThisMonth = customers.Count(c => c.CreatedDate >= DateTime.Now.AddMonths(-1)),
                    TotalCustomerValue = customers.Sum(c => c.TotalSpent)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer management data");
                return new CustomerManagementViewModel
                {
                    ErrorMessage = "An error occurred while loading customer management data"
                };
            }
        }

        public async Task<List<OrderViewModel>> GetCustomerOrdersAsync(Guid customerId)
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<List<OrderApiModel>>>($"api/order/customer/{customerId}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return response.Data.Select(MapToOrderViewModel).ToList();
                }
                
                _logger.LogWarning("Failed to get orders for customer: {CustomerId}", customerId);
                return new List<OrderViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer orders: {CustomerId}", customerId);
                return new List<OrderViewModel>();
            }
        }

        #region Private Mapping Methods

        private CustomerViewModel MapToCustomerViewModel(CustomerApiModel apiModel)
        {
            return new CustomerViewModel
            {
                Id = apiModel.Id,
                Name = apiModel.Name,
                Phone = apiModel.Phone,
                Point = apiModel.Point,
                IsActive = apiModel.IsActive,
                OwnerId = apiModel.OwnerId
            };
        }

        private OrderViewModel MapToOrderViewModel(OrderApiModel apiModel)
        {
            return new OrderViewModel
            {
                Id = apiModel.Id,
                CustomerId = apiModel.CustomerId,
                CustomerName = apiModel.CustomerName,
                TableId = apiModel.TableId,
                TableName = apiModel.TableName,
                TotalAmount = apiModel.TotalAmount,
                Status = apiModel.Status,
                OrderDate = apiModel.OrderDate
            };
        }

        #endregion
    }
}
