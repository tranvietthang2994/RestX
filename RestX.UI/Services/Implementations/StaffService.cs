using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Services.Implementations
{
    public class StaffService : IStaffService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<StaffService> _logger;

        public StaffService(IApiService apiService, ILogger<StaffService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<CustomerRequestViewModel> GetAllCustomerRequestsAsync()
        {
            try
            {
                _logger.LogInformation("Calling API: Staff/customer-requests");
                var apiUrl = "Staff/customer-requests";

                var response = await _apiService.GetAsync<ApiResponse<CustomerRequestViewModel>>(apiUrl);

                if (response?.Success == true)
                {
                    return response.Data ?? new CustomerRequestViewModel();
                }

                _logger.LogWarning("API returned error: {Message}", response?.Message);
                return new CustomerRequestViewModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customer requests");
                return new CustomerRequestViewModel();
            }
        }



        public async Task<IEnumerable<TableStatusViewModel>?> GetAllTablesAsync()
        {
            try
            {
                var apiUrl = "Staff/table-status";
                _logger.LogInformation("Calling API: {Url}", apiUrl);

                var response = await _apiService.GetAsync<ApiResponse<IEnumerable<TableStatusViewModel>>>(apiUrl);

                if (response == null)
                {
                    _logger.LogWarning("Response is null");
                    return new List<TableStatusViewModel>();
                }

                _logger.LogInformation("API response success = {Success}, data count = {Count}",
                    response.Success, response.Data?.Count() ?? 0);

                if (response.Success && response.Data != null)
                {
                    return response.Data;
                }

                _logger.LogWarning("API returned error: {Message}", response.Message);
                return new List<TableStatusViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching table statuses");
                return new List<TableStatusViewModel>();
            }
        }

        public async Task<MenuViewModel> GetMenuAsync()
        {
            try
            {
                _logger.LogInformation("Calling API: Staff/menu");
                var apiUrl = "Staff/menu";

                var response = await _apiService.GetAsync<ApiResponse<MenuViewModel>>(apiUrl);

                if (response?.Success == true)
                {
                    return response.Data ?? new MenuViewModel();
                }

                _logger.LogWarning("API returned error: {Message}", response?.Message);
                return new MenuViewModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching menu");
                return new MenuViewModel();
            }
        }

        public async Task<StaffProfileAPIViewModel?> GetStaffProfileAsync()
        {
            var response = await _apiService.GetAsync<ApiResponse<StaffProfileAPIViewModel>>("Staff/profile");
            return response?.Success == true ? response.Data : null;
        }




        // response wrapper (định nghĩa tạm bên trong class cho dễ dùng)
        private class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public T? Data { get; set; }
        }
    }
}
