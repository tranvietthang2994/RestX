using RestX.UI.Models.ApiModels;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Services.Implementations
{
    public class CartUIService : ICartUIService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<CartUIService> _logger;

        public CartUIService(IApiService apiService, ILogger<CartUIService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<CartViewModel?> GetCartAsync(Guid ownerId, int tableId)
        {
            try
            {
                _logger.LogInformation("Getting cart for owner: {OwnerId}, table: {TableId}", ownerId, tableId);
                
                var response = await _apiService.GetAsync<ApiResponse<CartApiModel>>($"api/cart/{ownerId}/{tableId}");
                
                if (response?.Success == true && response.Data != null)
                {
                    return MapToCartViewModel(response.Data);
                }
                
                _logger.LogWarning("Failed to get cart for owner: {OwnerId}, table: {TableId}", ownerId, tableId);
                return new CartViewModel 
                { 
                    OwnerId = ownerId, 
                    TableId = tableId,
                    ErrorMessage = response?.Message ?? "Failed to load cart"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart for owner: {OwnerId}, table: {TableId}", ownerId, tableId);
                return new CartViewModel 
                { 
                    OwnerId = ownerId, 
                    TableId = tableId,
                    ErrorMessage = "An error occurred while loading the cart"
                };
            }
        }

        public async Task<CartViewModel?> AddToCartAsync(Guid ownerId, int tableId, int dishId, int quantity)
        {
            try
            {
                _logger.LogInformation("Adding to cart - Owner: {OwnerId}, Table: {TableId}, Dish: {DishId}, Quantity: {Quantity}", 
                    ownerId, tableId, dishId, quantity);
                
                var request = new
                {
                    OwnerId = ownerId,
                    TableId = tableId,
                    DishId = dishId,
                    Quantity = quantity
                };

                var response = await _apiService.PostAsync<object, ApiResponse<CartApiModel>>("api/cart/add", request);
                
                if (response?.Success == true && response.Data != null)
                {
                    return MapToCartViewModel(response.Data);
                }
                
                _logger.LogWarning("Failed to add to cart - Owner: {OwnerId}, Table: {TableId}, Dish: {DishId}", 
                    ownerId, tableId, dishId);
                
                return new CartViewModel 
                { 
                    OwnerId = ownerId, 
                    TableId = tableId,
                    ErrorMessage = response?.Message ?? "Failed to add item to cart"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart - Owner: {OwnerId}, Table: {TableId}, Dish: {DishId}", 
                    ownerId, tableId, dishId);
                
                return new CartViewModel 
                { 
                    OwnerId = ownerId, 
                    TableId = tableId,
                    ErrorMessage = "An error occurred while adding item to cart"
                };
            }
        }

        public async Task<CartViewModel?> RemoveFromCartAsync(Guid ownerId, int tableId, int dishId)
        {
            try
            {
                _logger.LogInformation("Removing from cart - Owner: {OwnerId}, Table: {TableId}, Dish: {DishId}", 
                    ownerId, tableId, dishId);
                
                var response = await _apiService.DeleteAsync($"api/cart/{ownerId}/{tableId}/{dishId}");
                
                if (response)
                {
                    // Get updated cart
                    return await GetCartAsync(ownerId, tableId);
                }
                
                _logger.LogWarning("Failed to remove from cart - Owner: {OwnerId}, Table: {TableId}, Dish: {DishId}", 
                    ownerId, tableId, dishId);
                
                return new CartViewModel 
                { 
                    OwnerId = ownerId, 
                    TableId = tableId,
                    ErrorMessage = "Failed to remove item from cart"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from cart - Owner: {OwnerId}, Table: {TableId}, Dish: {DishId}", 
                    ownerId, tableId, dishId);
                
                return new CartViewModel 
                { 
                    OwnerId = ownerId, 
                    TableId = tableId,
                    ErrorMessage = "An error occurred while removing item from cart"
                };
            }
        }

        public async Task<CartViewModel?> UpdateQuantityAsync(Guid ownerId, int tableId, int dishId, int quantity)
        {
            try
            {
                _logger.LogInformation("Updating cart quantity - Owner: {OwnerId}, Table: {TableId}, Dish: {DishId}, Quantity: {Quantity}", 
                    ownerId, tableId, dishId, quantity);
                
                var request = new
                {
                    OwnerId = ownerId,
                    TableId = tableId,
                    DishId = dishId,
                    Quantity = quantity
                };

                var response = await _apiService.PutAsync<object, ApiResponse<CartApiModel>>("api/cart/update", request);
                
                if (response?.Success == true && response.Data != null)
                {
                    return MapToCartViewModel(response.Data);
                }
                
                _logger.LogWarning("Failed to update cart quantity - Owner: {OwnerId}, Table: {TableId}, Dish: {DishId}", 
                    ownerId, tableId, dishId);
                
                return new CartViewModel 
                { 
                    OwnerId = ownerId, 
                    TableId = tableId,
                    ErrorMessage = response?.Message ?? "Failed to update item quantity"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart quantity - Owner: {OwnerId}, Table: {TableId}, Dish: {DishId}", 
                    ownerId, tableId, dishId);
                
                return new CartViewModel 
                { 
                    OwnerId = ownerId, 
                    TableId = tableId,
                    ErrorMessage = "An error occurred while updating item quantity"
                };
            }
        }

        public async Task<bool> ClearCartAsync(Guid ownerId, int tableId)
        {
            try
            {
                _logger.LogInformation("Clearing cart - Owner: {OwnerId}, Table: {TableId}", ownerId, tableId);
                
                var result = await _apiService.DeleteAsync($"api/cart/{ownerId}/{tableId}");
                
                if (!result)
                {
                    _logger.LogWarning("Failed to clear cart - Owner: {OwnerId}, Table: {TableId}", ownerId, tableId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart - Owner: {OwnerId}, Table: {TableId}", ownerId, tableId);
                return false;
            }
        }

        public async Task<bool> CheckoutAsync(Guid ownerId, int tableId, string customerName, string customerPhone)
        {
            try
            {
                _logger.LogInformation("Checkout cart - Owner: {OwnerId}, Table: {TableId}, Customer: {CustomerName}", 
                    ownerId, tableId, customerName);
                
                var request = new
                {
                    OwnerId = ownerId,
                    TableId = tableId,
                    CustomerName = customerName,
                    CustomerPhone = customerPhone
                };

                var response = await _apiService.PostAsync<object, ApiResponse>("api/cart/checkout", request);
                
                if (response?.Success == true)
                {
                    _logger.LogInformation("Checkout successful - Owner: {OwnerId}, Table: {TableId}", ownerId, tableId);
                    return true;
                }
                
                _logger.LogWarning("Checkout failed - Owner: {OwnerId}, Table: {TableId}", ownerId, tableId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout - Owner: {OwnerId}, Table: {TableId}", ownerId, tableId);
                return false;
            }
        }

        #region Private Mapping Methods

        private CartViewModel MapToCartViewModel(CartApiModel apiModel)
        {
            return new CartViewModel
            {
                OwnerId = apiModel.OwnerId,
                TableId = apiModel.TableId,
                DishListJson = apiModel.DishListJson,
                OrderId = apiModel.OrderId,
                Message = apiModel.Message,
                Time = apiModel.Time,
                DishList = apiModel.DishList?.Select(MapToDishCartViewModel).ToList() ?? new List<DishCartViewModel>()
            };
        }

        private DishCartViewModel MapToDishCartViewModel(DishCartApiModel apiModel)
        {
            return new DishCartViewModel
            {
                DishId = apiModel.DishId,
                DishName = apiModel.DishName,
                Quantity = apiModel.Quantity,
                Price = apiModel.Price,
                ImgUrl = apiModel.ImgUrl
            };
        }

        #endregion
    }
}
