using RestX.UI.Models.ApiModels;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Services.Implementations
{
    public class HomeUIService : IHomeUIService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<HomeUIService> _logger;
        public HomeUIService(IApiService apiService, ILogger<HomeUIService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<HomeViewModel?> GetHomeViewsAsync(Guid ownerId, int tableId)
        {
            try
            {
                _logger.LogInformation("Getting home view model for ownerId: {OwnerId}, tableId: {TableId}", ownerId, tableId);
                var endpoint = $"api/home/index/{ownerId}/{tableId}";
                _logger.LogDebug("Calling API endpoint: {Endpoint}", endpoint);
                
                var response = await _apiService.GetAsync<ApiResponse<HomeViewModel>>(endpoint);
                
                if (response == null)
                {
                    _logger.LogError("API response is null for endpoint: {Endpoint}", endpoint);
                    return new HomeViewModel
                    {
                        OwnerId = ownerId,
                        TableId = tableId,
                        Name = string.Empty,
                        Address = string.Empty,
                        FileName = "Defaul",
                        FileUrl = "/images/default.png",
                        TableNumber = 0,
                        ErrorMessage = "API returned null response. Please check API configuration and connectivity."
                    };
                }
                
                _logger.LogDebug("API response received. Success: {Success}, HasData: {HasData}", response.Success, response.Data != null);
                
                if (response.Success == true && response.Data != null)
                {
                    _logger.LogInformation("Successfully retrieved home view model");
                    return MapToHomeViewModel(response.Data);
                }
                
                _logger.LogWarning("Failed to get home view model. Success: {Success}, Message: {Message}", response.Success, response.Message);
                return new HomeViewModel
                {
                    OwnerId = ownerId,
                    TableId = tableId,
                    Name = string.Empty,
                    Address = string.Empty,
                    FileName = "Defaul",
                    FileUrl = "/images/default.png",
                    TableNumber = 0,
                    ErrorMessage = response?.Message ?? "Failed to load home view"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting home view model for ownerId: {OwnerId}, tableId: {TableId}. Exception: {ExceptionMessage}, StackTrace: {StackTrace}", 
                    ownerId, tableId, ex.Message, ex.StackTrace);
                return new HomeViewModel
                {
                    OwnerId = ownerId,
                    TableId = tableId,
                    Name = string.Empty,
                    Address = string.Empty,
                    FileName = "Defaul",
                    FileUrl = "/images/default.png",
                    TableNumber = 0,
                    ErrorMessage = $"An error occurred while loading the home view: {ex.Message}"
                };
            }
        }

        #region Private Mapping Methods
        private HomeViewModel MapToHomeViewModel(HomeViewModel apiModel)
        {
            return new HomeViewModel
            {
                OwnerId = apiModel.OwnerId,
                TableId = apiModel.TableId,
                Name = apiModel.Name,
                Address = apiModel.Address,
                FileName = apiModel.FileName,
                FileUrl =   apiModel.FileUrl,
                TableNumber = apiModel.TableNumber
            };
        }

        #endregion
    }
}
