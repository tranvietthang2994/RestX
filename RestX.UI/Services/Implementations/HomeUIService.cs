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
                _logger.LogInformation("Getting home view model");
                var response = await _apiService.GetAsync<ApiResponse<HomeViewModel>>($"api/home/index/{ownerId}/{tableId}");
                if (response?.Success == true && response.Data != null)
                {
                    return MapToHomeViewModel(response.Data);
                }
                _logger.LogWarning("Failed to get home view model");
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
                _logger.LogError(ex, "Error getting home view model");
                return new HomeViewModel
                {
                    OwnerId = ownerId,
                    TableId = tableId,
                    Name = string.Empty,
                    Address = string.Empty,
                    FileName = "Defaul",
                    FileUrl = "/images/default.png",
                    TableNumber = 0,
                    ErrorMessage = "An error occurred while loading the home view"
                }
            ;
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
