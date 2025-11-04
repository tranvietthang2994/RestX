using RestX.UI.Models.ViewModels;

namespace RestX.UI.Services.Interfaces
{
    public interface IOrderUIService
    {
        /// <summary>
        /// Get all orders for restaurant
        /// </summary>
        /// <returns>List of orders</returns>
        Task<List<OrderViewModel>> GetOrdersAsync();

        /// <summary>
        /// Get order by ID
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Order view model</returns>
        Task<OrderViewModel?> GetOrderByIdAsync(Guid orderId);

        /// <summary>
        /// Get order details by order ID
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>List of order details</returns>
        Task<List<OrderDetailViewModel>> GetOrderDetailsAsync(Guid orderId);

        /// <summary>
        /// Update order status
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="status">New status</param>
        /// <returns>Success status</returns>
        Task<bool> UpdateOrderStatusAsync(Guid orderId, string status);

        /// <summary>
        /// Update order detail status
        /// </summary>
        /// <param name="orderDetailId">Order detail ID</param>
        /// <param name="status">New status</param>
        /// <returns>Success status</returns>
        Task<bool> UpdateOrderDetailStatusAsync(Guid orderDetailId, string status);

        /// <summary>
        /// Get orders by status
        /// </summary>
        /// <param name="status">Order status</param>
        /// <returns>List of orders</returns>
        Task<List<OrderViewModel>> GetOrdersByStatusAsync(string status);

        /// <summary>
        /// Get orders by date range
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <returns>List of orders</returns>
        Task<List<OrderViewModel>> GetOrdersByDateRangeAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Get order history for customer
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>List of customer orders</returns>
        Task<List<OrderViewModel>> GetCustomerOrderHistoryAsync(Guid customerId);

        /// <summary>
        /// Get pending orders for kitchen
        /// </summary>
        /// <returns>List of pending orders</returns>
        Task<List<OrderViewModel>> GetPendingOrdersAsync();

        /// <summary>
        /// Get orders by customer ID and owner ID
        /// </summary>
        /// <param name="ownerId">Owner ID</param>
        /// <param name="customerId">Customer ID</param>
        /// <returns>List of cart view models</returns>
        Task<List<CartViewModel>> GetOrdersByCustomerIdOwnerIdAsync(Guid ownerId, Guid customerId);

        /// <summary>
        /// Cancel order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="reason">Cancellation reason</param>
        /// <returns>Success status and message</returns>
        Task<(bool Success, string? Message)> CancelOrderAsync(Guid orderId, string? reason);
    }
}
