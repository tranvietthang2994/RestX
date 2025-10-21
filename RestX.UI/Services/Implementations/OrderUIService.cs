using RestX.UI.Models.ApiModels;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Services.Implementations
{
    public class OrderUIService : IOrderUIService
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly ILogger<OrderUIService> _logger;

        public OrderUIService(
            IApiService apiService,
            IAuthService authService,
            ILogger<OrderUIService> logger)
        {
            _apiService = apiService;
            _authService = authService;
            _logger = logger;
        }

        public async Task<List<OrderViewModel>> GetOrdersAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var ownerId = currentUser?.OwnerId ?? currentUser?.Id;
                
                if (ownerId == null)
                {
                    _logger.LogWarning("No owner context found for orders");
                    return new List<OrderViewModel>();
                }

                var response = await _apiService.GetAsync<ApiResponse<List<OrderApiModel>>>($"api/order/owner/{ownerId}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return response.Data.Select(MapToOrderViewModel).ToList();
                }
                
                _logger.LogWarning("Failed to get orders for owner: {OwnerId}", ownerId);
                return new List<OrderViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders");
                return new List<OrderViewModel>();
            }
        }

        public async Task<OrderViewModel?> GetOrderByIdAsync(Guid orderId)
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<OrderApiModel>>($"api/order/{orderId}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return MapToOrderViewModel(response.Data);
                }
                
                _logger.LogWarning("Failed to get order by ID: {OrderId}", orderId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by ID: {OrderId}", orderId);
                return null;
            }
        }

        public async Task<List<OrderDetailViewModel>> GetOrderDetailsAsync(Guid orderId)
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<List<OrderDetailApiModel>>>($"api/order/{orderId}/details");
                
                if (response?.Success == true && response.Data != null)
                {
                    return response.Data.Select(MapToOrderDetailViewModel).ToList();
                }
                
                _logger.LogWarning("Failed to get order details for order: {OrderId}", orderId);
                return new List<OrderDetailViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order details for order: {OrderId}", orderId);
                return new List<OrderDetailViewModel>();
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string status)
        {
            try
            {
                var updateData = new
                {
                    OrderId = orderId,
                    Status = status
                };

                var response = await _apiService.PutAsync<object, ApiResponse>("api/order/status", updateData);
                
                if (response?.Success == true)
                {
                    _logger.LogInformation("Order status updated successfully: {OrderId} -> {Status}", orderId, status);
                    return true;
                }
                
                _logger.LogWarning("Failed to update order status: {OrderId} -> {Status}", orderId, status);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status: {OrderId} -> {Status}", orderId, status);
                return false;
            }
        }

        public async Task<bool> UpdateOrderDetailStatusAsync(Guid orderDetailId, string status)
        {
            try
            {
                var updateData = new
                {
                    OrderDetailId = orderDetailId,
                    Status = status
                };

                var response = await _apiService.PutAsync<object, ApiResponse>("api/order/detail/status", updateData);
                
                if (response?.Success == true)
                {
                    _logger.LogInformation("Order detail status updated successfully: {OrderDetailId} -> {Status}", orderDetailId, status);
                    return true;
                }
                
                _logger.LogWarning("Failed to update order detail status: {OrderDetailId} -> {Status}", orderDetailId, status);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order detail status: {OrderDetailId} -> {Status}", orderDetailId, status);
                return false;
            }
        }

        public async Task<List<OrderViewModel>> GetOrdersByStatusAsync(string status)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var ownerId = currentUser?.OwnerId ?? currentUser?.Id;
                
                if (ownerId == null)
                {
                    _logger.LogWarning("No owner context found for orders by status");
                    return new List<OrderViewModel>();
                }

                var response = await _apiService.GetAsync<ApiResponse<List<OrderApiModel>>>($"api/order/owner/{ownerId}/status/{Uri.EscapeDataString(status)}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return response.Data.Select(MapToOrderViewModel).ToList();
                }
                
                _logger.LogWarning("Failed to get orders by status for owner: {OwnerId}, status: {Status}", ownerId, status);
                return new List<OrderViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders by status: {Status}", status);
                return new List<OrderViewModel>();
            }
        }

        public async Task<List<OrderViewModel>> GetOrdersByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var ownerId = currentUser?.OwnerId ?? currentUser?.Id;
                
                if (ownerId == null)
                {
                    _logger.LogWarning("No owner context found for orders by date range");
                    return new List<OrderViewModel>();
                }

                var response = await _apiService.GetAsync<ApiResponse<List<OrderApiModel>>>(
                    $"api/order/owner/{ownerId}/daterange?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return response.Data.Select(MapToOrderViewModel).ToList();
                }
                
                _logger.LogWarning("Failed to get orders by date range for owner: {OwnerId}", ownerId);
                return new List<OrderViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders by date range: {FromDate} - {ToDate}", fromDate, toDate);
                return new List<OrderViewModel>();
            }
        }

        public async Task<List<OrderViewModel>> GetCustomerOrderHistoryAsync(Guid customerId)
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<List<OrderApiModel>>>($"api/order/customer/{customerId}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return response.Data.Select(MapToOrderViewModel).ToList();
                }
                
                _logger.LogWarning("Failed to get order history for customer: {CustomerId}", customerId);
                return new List<OrderViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer order history: {CustomerId}", customerId);
                return new List<OrderViewModel>();
            }
        }

        public async Task<List<OrderViewModel>> GetPendingOrdersAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var ownerId = currentUser?.OwnerId ?? currentUser?.Id;
                
                if (ownerId == null)
                {
                    _logger.LogWarning("No owner context found for pending orders");
                    return new List<OrderViewModel>();
                }

                var response = await _apiService.GetAsync<ApiResponse<List<OrderApiModel>>>($"api/order/owner/{ownerId}/pending");
                
                if (response?.Success == true && response.Data != null)
                {
                    return response.Data.Select(MapToOrderViewModel).ToList();
                }
                
                _logger.LogWarning("Failed to get pending orders for owner: {OwnerId}", ownerId);
                return new List<OrderViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending orders");
                return new List<OrderViewModel>();
            }
        }

        public async Task<(bool Success, string? Message)> CancelOrderAsync(Guid orderId, string? reason)
        {
            try
            {
                var cancelData = new
                {
                    OrderId = orderId,
                    Status = "Cancelled",
                    Reason = reason
                };

                var response = await _apiService.PutAsync<object, ApiResponse>("api/order/cancel", cancelData);
                
                if (response?.Success == true)
                {
                    _logger.LogInformation("Order cancelled successfully: {OrderId}", orderId);
                    return (true, response.Message ?? "Order cancelled successfully");
                }
                
                return (false, response?.Message ?? "Failed to cancel order");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order: {OrderId}", orderId);
                return (false, "An error occurred while cancelling the order");
            }
        }

        #region Private Mapping Methods

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
                OrderDate = apiModel.OrderDate,
                OrderDetails = apiModel.OrderDetails?.Select(MapToOrderDetailViewModel).ToList() ?? new List<OrderDetailViewModel>()
            };
        }

        private OrderDetailViewModel MapToOrderDetailViewModel(OrderDetailApiModel apiModel)
        {
            return new OrderDetailViewModel
            {
                Id = apiModel.Id,
                OrderId = apiModel.OrderId,
                DishId = apiModel.DishId,
                DishName = apiModel.DishName,
                Quantity = apiModel.Quantity,
                Price = apiModel.Price,
                Status = apiModel.Status
            };
        }

        #endregion
    }
}
