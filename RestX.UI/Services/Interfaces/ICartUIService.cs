using RestX.UI.Models.ViewModels;

namespace RestX.UI.Services.Interfaces
{
    public interface ICartUIService
    {
        /// <summary>
        /// Get cart for table
        /// </summary>
        /// <param name="ownerId">Owner ID</param>
        /// <param name="tableId">Table ID</param>
        /// <returns>Cart view model</returns>
        Task<CartViewModel?> GetCartAsync(Guid ownerId, int tableId);

        /// <summary>
        /// Add dish to cart
        /// </summary>
        /// <param name="ownerId">Owner ID</param>
        /// <param name="tableId">Table ID</param>
        /// <param name="dishId">Dish ID</param>
        /// <param name="quantity">Quantity</param>
        /// <returns>Updated cart</returns>
        Task<CartViewModel?> AddToCartAsync(Guid ownerId, int tableId, int dishId, int quantity);

        /// <summary>
        /// Remove dish from cart
        /// </summary>
        /// <param name="ownerId">Owner ID</param>
        /// <param name="tableId">Table ID</param>
        /// <param name="dishId">Dish ID</param>
        /// <returns>Updated cart</returns>
        Task<CartViewModel?> RemoveFromCartAsync(Guid ownerId, int tableId, int dishId);

        /// <summary>
        /// Update dish quantity in cart
        /// </summary>
        /// <param name="ownerId">Owner ID</param>
        /// <param name="tableId">Table ID</param>
        /// <param name="dishId">Dish ID</param>
        /// <param name="quantity">New quantity</param>
        /// <returns>Updated cart</returns>
        Task<CartViewModel?> UpdateQuantityAsync(Guid ownerId, int tableId, int dishId, int quantity);

        /// <summary>
        /// Clear cart
        /// </summary>
        /// <param name="ownerId">Owner ID</param>
        /// <param name="tableId">Table ID</param>
        /// <returns>Success status</returns>
        Task<bool> ClearCartAsync(Guid ownerId, int tableId);

        /// <summary>
        /// Checkout cart and create order
        /// </summary>
        /// <param name="ownerId">Owner ID</param>
        /// <param name="tableId">Table ID</param>
        /// <param name="customerName">Customer name</param>
        /// <param name="customerPhone">Customer phone</param>
        /// <returns>Order result</returns>
        Task<bool> CheckoutAsync(Guid ownerId, int tableId, CartViewModel model);

        Task<CartViewModel> JsonToDishList(CartViewModel cart);
        Task<CartViewModel> JsonToCartViewModel(string cartJson);
    }
}
