using RestX.UI.Models.ApiModels;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Services.Implementations
{
    public class OwnerUIService : IOwnerUIService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<OwnerUIService> _logger;

        public OwnerUIService(IApiService apiService, ILogger<OwnerUIService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<DashboardViewModel?> GetDashboardAsync()
        {
            try
            {
                _logger.LogInformation("Getting dashboard data from API");

                var response = await _apiService.GetAsync<ApiResponse<DashboardViewModel>>("api/owner/dashboard");

                if (response?.Success == true && response.Data != null)
                {
                    _logger.LogInformation("Dashboard data retrieved successfully");
                    return response.Data;
                }

                _logger.LogWarning("Failed to retrieve dashboard data: {Message}", response?.Message);
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
                    ErrorMessage = "An error occurred while loading dashboard data"
                };
            }
        }

        public async Task<OwnerProfileViewModel?> GetOwnerProfileAsync()
        {
            try
            {
                _logger.LogInformation("Getting owner profile from API");

                var response = await _apiService.GetAsync<ApiResponse<OwnerProfileViewModel>>("api/owner/profile");

                if (response?.Success == true && response.Data != null)
                {
                    _logger.LogInformation("Owner profile retrieved successfully");
                    return response.Data;
                }

                _logger.LogWarning("Failed to retrieve owner profile: {Message}", response?.Message);
                return new OwnerProfileViewModel
                {
                    ErrorMessage = response?.Message ?? "Failed to load profile data"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting owner profile");
                return new OwnerProfileViewModel
                {
                    ErrorMessage = "An error occurred while loading profile data"
                };
            }
        }

        public async Task<(bool Success, string Message)> UpdateOwnerProfileAsync(OwnerProfileViewModel profileModel)
        {
            try
            {
                _logger.LogInformation("Updating owner profile");

                var response = await _apiService.PutAsync<OwnerProfileViewModel, ApiResponse<object>>("api/owner/profile", profileModel);

                if (response?.Success == true)
                {
                    _logger.LogInformation("Owner profile updated successfully");
                    return (true, response.Message ?? "Profile updated successfully");
                }

                _logger.LogWarning("Failed to update owner profile: {Message}", response?.Message);
                return (false, response?.Message ?? "Failed to update profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating owner profile");
                return (false, "An error occurred while updating profile");
            }
        }

        public async Task<RevenueStatisticsViewModel?> GetRevenueStatisticsAsync(DateTime? from = null, DateTime? to = null)
        {
            try
            {
                _logger.LogInformation("Getting revenue statistics from API");

                var endpoint = "api/owner/statistics/revenue";
                if (from.HasValue || to.HasValue)
                {
                    var queryParams = new List<string>();
                    if (from.HasValue) queryParams.Add($"from={from.Value:yyyy-MM-dd}");
                    if (to.HasValue) queryParams.Add($"to={to.Value:yyyy-MM-dd}");
                    endpoint += "?" + string.Join("&", queryParams);
                }

                var response = await _apiService.GetAsync<ApiResponse<RevenueStatisticsViewModel>>(endpoint);

                if (response?.Success == true && response.Data != null)
                {
                    _logger.LogInformation("Revenue statistics retrieved successfully");
                    return response.Data;
                }

                _logger.LogWarning("Failed to retrieve revenue statistics: {Message}", response?.Message);
                return new RevenueStatisticsViewModel
                {
                    ErrorMessage = response?.Message ?? "Failed to load revenue statistics"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue statistics");
                return new RevenueStatisticsViewModel
                {
                    ErrorMessage = "An error occurred while loading revenue statistics"
                };
            }
        }

        public async Task<OrderStatisticsViewModel?> GetOrderStatisticsAsync(DateTime? from = null, DateTime? to = null)
        {
            try
            {
                _logger.LogInformation("Getting order statistics from API");

                var endpoint = "api/owner/statistics/orders";
                if (from.HasValue || to.HasValue)
                {
                    var queryParams = new List<string>();
                    if (from.HasValue) queryParams.Add($"from={from.Value:yyyy-MM-dd}");
                    if (to.HasValue) queryParams.Add($"to={to.Value:yyyy-MM-dd}");
                    endpoint += "?" + string.Join("&", queryParams);
                }

                var response = await _apiService.GetAsync<ApiResponse<OrderStatisticsViewModel>>(endpoint);

                if (response?.Success == true && response.Data != null)
                {
                    _logger.LogInformation("Order statistics retrieved successfully");
                    return response.Data;
                }

                _logger.LogWarning("Failed to retrieve order statistics: {Message}", response?.Message);
                return new OrderStatisticsViewModel
                {
                    ErrorMessage = response?.Message ?? "Failed to load order statistics"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order statistics");
                return new OrderStatisticsViewModel
                {
                    ErrorMessage = "An error occurred while loading order statistics"
                };
            }
        }

        public async Task<OverviewReportViewModel?> GetOverviewReportAsync(string period = "month")
        {
            try
            {
                _logger.LogInformation("Getting overview report from API for period: {Period}", period);

                var response = await _apiService.GetAsync<ApiResponse<OverviewReportViewModel>>($"api/owner/reports/overview?period={period}");

                if (response?.Success == true && response.Data != null)
                {
                    _logger.LogInformation("Overview report retrieved successfully");
                    return response.Data;
                }

                _logger.LogWarning("Failed to retrieve overview report: {Message}", response?.Message);
                return new OverviewReportViewModel
                {
                    Period = period,
                    ErrorMessage = response?.Message ?? "Failed to load overview report"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overview report");
                return new OverviewReportViewModel
                {
                    Period = period,
                    ErrorMessage = "An error occurred while loading overview report"
                };
            }
        }

        public async Task<List<TopDishViewModel>?> GetTopDishesAsync(int limit = 10, string period = "month")
        {
            try
            {
                _logger.LogInformation("Getting top dishes from API with limit: {Limit}, period: {Period}", limit, period);

                var response = await _apiService.GetAsync<ApiResponse<List<TopDishViewModel>>>($"api/owner/reports/top-dishes?limit={limit}&period={period}");

                if (response?.Success == true && response.Data != null)
                {
                    _logger.LogInformation("Top dishes retrieved successfully");
                    return response.Data;
                }

                _logger.LogWarning("Failed to retrieve top dishes: {Message}", response?.Message);
                return new List<TopDishViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top dishes");
                return new List<TopDishViewModel>();
            }
        }
    }
}