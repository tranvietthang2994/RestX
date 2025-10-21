using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Controllers
{
    [Authorize(Roles = "Owner,Staff")]
    public class OrderController : Controller
    {
        private readonly IOrderUIService _orderService;
        private readonly ICustomerUIService _customerService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderUIService orderService,
            ICustomerUIService customerService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _customerService = customerService;
            _logger = logger;
        }

        /// <summary>
        /// Orders management page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading orders management page");
                
                var orders = await _orderService.GetOrdersAsync();
                
                var historyModel = new OrderHistoryViewModel
                {
                    Orders = orders,
                    TotalOrders = orders.Count,
                    TotalRevenue = orders.Sum(o => o.TotalAmount)
                };

                return View(historyModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders management page");
                return View("Error", new ErrorViewModel 
                { 
                    Message = "An error occurred while loading orders"
                });
            }
        }

        /// <summary>
        /// Order history page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> History()
        {
            try
            {
                var orders = await _orderService.GetOrdersAsync();
                
                var historyModel = new OrderHistoryViewModel
                {
                    Orders = orders.OrderByDescending(o => o.OrderDate).ToList(),
                    TotalOrders = orders.Count,
                    TotalRevenue = orders.Sum(o => o.TotalAmount)
                };

                return View(historyModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order history page");
                return View("Error", new ErrorViewModel 
                { 
                    Message = "An error occurred while loading order history"
                });
            }
        }

        /// <summary>
        /// Get all orders as JSON
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var orders = await _orderService.GetOrdersAsync();
                return Json(new { success = true, data = orders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders");
                return Json(new { success = false, message = "An error occurred while loading orders" });
            }
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetOrder(Guid orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                
                if (order != null)
                {
                    return Json(new { success = true, data = order });
                }
                
                return Json(new { success = false, message = "Order not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by ID: {OrderId}", orderId);
                return Json(new { success = false, message = "An error occurred while loading order data" });
            }
        }

        /// <summary>
        /// Get order details
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(Guid orderId)
        {
            try
            {
                var orderDetails = await _orderService.GetOrderDetailsAsync(orderId);
                return Json(new { success = true, data = orderDetails });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order details: {OrderId}", orderId);
                return Json(new { success = false, message = "An error occurred while loading order details" });
            }
        }

        /// <summary>
        /// Order details page
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Details(Guid orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                
                if (order == null)
                {
                    return View("Error", new ErrorViewModel 
                    { 
                        Message = "Order not found",
                        StatusCode = 404
                    });
                }

                // Get order details if not already loaded
                if (!order.OrderDetails.Any())
                {
                    order.OrderDetails = await _orderService.GetOrderDetailsAsync(orderId);
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order details page: {OrderId}", orderId);
                return View("Error", new ErrorViewModel 
                { 
                    Message = "An error occurred while loading order details"
                });
            }
        }

        /// <summary>
        /// Update order status
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="status">New status</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                {
                    return Json(new { success = false, message = "Status is required" });
                }

                var success = await _orderService.UpdateOrderStatusAsync(orderId, status);
                
                if (success)
                {
                    return Json(new { success = true, message = "Order status updated successfully" });
                }
                
                return Json(new { success = false, message = "Failed to update order status" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status: {OrderId} -> {Status}", orderId, status);
                return Json(new { success = false, message = "An error occurred while updating order status" });
            }
        }

        /// <summary>
        /// Update order detail status
        /// </summary>
        /// <param name="orderDetailId">Order detail ID</param>
        /// <param name="status">New status</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateOrderDetailStatus(Guid orderDetailId, string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                {
                    return Json(new { success = false, message = "Status is required" });
                }

                var success = await _orderService.UpdateOrderDetailStatusAsync(orderDetailId, status);
                
                if (success)
                {
                    return Json(new { success = true, message = "Order item status updated successfully" });
                }
                
                return Json(new { success = false, message = "Failed to update order item status" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order detail status: {OrderDetailId} -> {Status}", orderDetailId, status);
                return Json(new { success = false, message = "An error occurred while updating order item status" });
            }
        }

        /// <summary>
        /// Cancel order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="reason">Cancellation reason</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CancelOrder(Guid orderId, string? reason)
        {
            try
            {
                var (success, message) = await _orderService.CancelOrderAsync(orderId, reason);
                
                return Json(new { success, message = message ?? (success ? "Order cancelled successfully" : "Failed to cancel order") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order: {OrderId}", orderId);
                return Json(new { success = false, message = "An error occurred while cancelling the order" });
            }
        }

        /// <summary>
        /// Get orders by status
        /// </summary>
        /// <param name="status">Order status</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetOrdersByStatus(string status)
        {
            try
            {
                var orders = await _orderService.GetOrdersByStatusAsync(status);
                return Json(new { success = true, data = orders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders by status: {Status}", status);
                return Json(new { success = false, message = "An error occurred while loading orders" });
            }
        }

        /// <summary>
        /// Get orders by date range
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetOrdersByDateRange(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var orders = await _orderService.GetOrdersByDateRangeAsync(fromDate, toDate);
                
                var result = new
                {
                    orders = orders,
                    totalOrders = orders.Count,
                    totalRevenue = orders.Sum(o => o.TotalAmount)
                };

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders by date range: {FromDate} - {ToDate}", fromDate, toDate);
                return Json(new { success = false, message = "An error occurred while loading orders" });
            }
        }

        /// <summary>
        /// Kitchen view for pending orders
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Kitchen()
        {
            try
            {
                var pendingOrders = await _orderService.GetPendingOrdersAsync();
                
                var kitchenModel = new KitchenOrderViewModel
                {
                    PendingOrders = pendingOrders,
                    TotalPendingOrders = pendingOrders.Count
                };

                return View(kitchenModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading kitchen page");
                return View("Error", new ErrorViewModel 
                { 
                    Message = "An error occurred while loading the kitchen page"
                });
            }
        }

        /// <summary>
        /// Export orders to CSV
        /// </summary>
        /// <param name="fromDate">Start date (optional)</param>
        /// <param name="toDate">End date (optional)</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> ExportOrders(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                List<OrderViewModel> orders;
                
                if (fromDate.HasValue && toDate.HasValue)
                {
                    orders = await _orderService.GetOrdersByDateRangeAsync(fromDate.Value, toDate.Value);
                }
                else
                {
                    orders = await _orderService.GetOrdersAsync();
                }
                
                // Simple CSV export
                var csv = "Order Date,Customer Name,Table,Total Amount,Status\n";
                foreach (var order in orders.OrderByDescending(o => o.OrderDate))
                {
                    csv += $"{order.OrderDate:yyyy-MM-dd HH:mm},{order.CustomerName},{order.TableName},{order.TotalAmount:F2},{order.Status}\n";
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                var fileName = fromDate.HasValue && toDate.HasValue 
                    ? $"orders_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.csv" 
                    : $"orders_{DateTime.Now:yyyyMMdd}.csv";

                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting orders");
                return Json(new { success = false, message = "An error occurred while exporting orders" });
            }
        }
    }
}