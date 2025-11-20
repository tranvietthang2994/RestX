using RestX.UI.Models.ApiModels;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;
using System.Text.Json;

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

                _logger.LogInformation("Getting customers for owner: {OwnerId}", ownerId);

                // Try different API endpoints that might exist
                string[] possibleEndpoints = {
            $"api/customer/owner/{ownerId}",
            $"api/customer",
            $"api/customer/list",
            $"api/customers/owner/{ownerId}",
            $"api/customers"
        };

                string? responseString = null;
                string? usedEndpoint = null;

                foreach (var endpoint in possibleEndpoints)
                {
                    try
                    {
                        _logger.LogInformation("Trying endpoint: {Endpoint}", endpoint);
                        responseString = await _apiService.GetStringAsync(endpoint);

                        if (!string.IsNullOrEmpty(responseString))
                        {
                            usedEndpoint = endpoint;
                            _logger.LogInformation("Successfully got response from: {Endpoint}", endpoint);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed to get data from endpoint {Endpoint}: {Error}", endpoint, ex.Message);
                        continue;
                    }
                }

                if (!string.IsNullOrEmpty(responseString))
                {
                    _logger.LogInformation("Customer API Response from {Endpoint}: {Response}", usedEndpoint, responseString);

                    try
                    {
                        // Parse the API response which might have structure: { "success": true, "data": [...] }
                        var apiResponseWrapper = JsonSerializer.Deserialize<ApiResponse<JsonElement[]>>(responseString, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (apiResponseWrapper?.Success == true && apiResponseWrapper.Data != null)
                        {
                            var customers = new List<CustomerViewModel>();

                            foreach (var customerElement in apiResponseWrapper.Data)
                            {
                                var customer = ParseCustomerFromJsonElement(customerElement, ownerId);
                                if (customer != null)
                                {
                                    customers.Add(customer);
                                }
                            }

                            _logger.LogInformation("Successfully parsed {Count} customers from API response wrapper", customers.Count);
                            return customers;
                        }
                        else
                        {
                            // Fallback: Try direct array parsing if it's not wrapped
                            var jsonDocument = JsonDocument.Parse(responseString);
                            var customers = new List<CustomerViewModel>();

                            if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var element in jsonDocument.RootElement.EnumerateArray())
                                {
                                    var customer = ParseCustomerFromJsonElement(element, ownerId);
                                    if (customer != null)
                                    {
                                        customers.Add(customer);
                                    }
                                }

                                _logger.LogInformation("Successfully parsed {Count} customers from direct array", customers.Count);
                                return customers;
                            }
                            else if (jsonDocument.RootElement.ValueKind == JsonValueKind.Object)
                            {
                                // Try to parse single object response
                                if (jsonDocument.RootElement.TryGetProperty("data", out var dataProperty) &&
                                    dataProperty.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var element in dataProperty.EnumerateArray())
                                    {
                                        var customer = ParseCustomerFromJsonElement(element, ownerId);
                                        if (customer != null)
                                        {
                                            customers.Add(customer);
                                        }
                                    }
                                    return customers;
                                }
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to parse customer API response from {Endpoint}", usedEndpoint);
                        _logger.LogError("Raw response that failed to parse: {Response}", responseString);
                    }
                }
                else
                {
                    _logger.LogWarning("All customer API endpoints returned empty responses");
                }

                return new List<CustomerViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers");
                return new List<CustomerViewModel>();
            }
        }

        private CustomerViewModel? ParseCustomerFromJsonElement(JsonElement customerElement, Guid? expectedOwnerId)
        {
            try
            {
                var customer = new CustomerViewModel
                {
                    Id = customerElement.TryGetProperty("id", out var idProp) &&
                         Guid.TryParse(idProp.GetString(), out var id) ? id : Guid.Empty,
                    Name = customerElement.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "" : "",
                    Phone = customerElement.TryGetProperty("phone", out var phoneProp) ? phoneProp.GetString() ?? "" : "",
                    Email = customerElement.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null,
                    Address = customerElement.TryGetProperty("address", out var addressProp) ? addressProp.GetString() : null,
                    Point = customerElement.TryGetProperty("point", out var pointProp) ? pointProp.GetInt32() : 0,
                    IsActive = customerElement.TryGetProperty("isActive", out var activeProp) ? activeProp.GetBoolean() : true,
                    OwnerId = customerElement.TryGetProperty("ownerId", out var ownerProp) &&
                             Guid.TryParse(ownerProp.GetString(), out var ownerGuid) ? ownerGuid : null,
                    CreatedDate = customerElement.TryGetProperty("createdDate", out var createdProp) &&
                                 DateTime.TryParse(createdProp.GetString(), out var created) ? created : DateTime.Now,
                    ModifiedDate = customerElement.TryGetProperty("modifiedDate", out var modifiedProp) &&
                                  DateTime.TryParse(modifiedProp.GetString(), out var modified) ? modified : null,
                    DateOfBirth = customerElement.TryGetProperty("dateOfBirth", out var dobProp) &&
                                 DateTime.TryParse(dobProp.GetString(), out var dob) ? dob : null
                };

                // Filter by owner if we have an expected owner ID and the customer has an owner ID
                if (expectedOwnerId.HasValue && customer.OwnerId.HasValue &&
                    customer.OwnerId.Value != expectedOwnerId.Value)
                {
                    _logger.LogDebug("Filtering out customer {CustomerId} - wrong owner {CustomerOwnerId} vs expected {ExpectedOwnerId}",
                        customer.Id, customer.OwnerId, expectedOwnerId);
                    return null;
                }

                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse customer element");
                return null;
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
