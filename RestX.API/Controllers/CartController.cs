using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RestX.API.Hubs;
using RestX.API.Models.ViewModels;
using RestX.API.Services.Interfaces;

namespace RestX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")] // Cart operations for customers
    public class CartController : ControllerBase
    {
        private readonly ICartService cartService;
        private readonly IOrderService orderService;
        private readonly IHubContext<SignalrServer> hubContext;
        private readonly IExceptionHandler exceptionHandler;

        public CartController(
            ICartService cartService,
            IOrderService orderService,
            IHubContext<SignalrServer> hubContext,
            IExceptionHandler exceptionHandler)
        {
            this.cartService = cartService;
            this.orderService = orderService;
            this.hubContext = hubContext;
            this.exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Lấy thông tin giỏ hàng
        /// </summary>
        /// <param name="ownerId">ID chủ nhà hàng</param>
        /// <param name="tableId">ID bàn</param>
        /// <returns>Thông tin giỏ hàng</returns>
        [HttpGet("{ownerId:guid}/table/{tableId:int}")]
        public async Task<IActionResult> GetCart(Guid ownerId, int tableId)
        {
            try
            {
                var cartViewModel = new CartViewModel
                {
                    OwnerId = ownerId,
                    TableId = tableId
                };

                var processedCart = await cartService.JsonToDishList(cartViewModel);
                
                return Ok(new { success = true, data = processedCart });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, $"Error loading cart for Owner: {ownerId}, Table: {tableId}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Xử lý giỏ hàng từ JSON string
        /// </summary>
        /// <param name="cartJson">JSON string của giỏ hàng</param>
        /// <returns>Thông tin giỏ hàng đã xử lý</returns>
        [HttpPost("process")]
        public async Task<IActionResult> ProcessCartFromJson([FromBody] CartJsonRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.CartJson))
                    return BadRequest(new { success = false, message = "Cart JSON is required" });

                var cartViewModel = await cartService.JsonToCartViewModel(request.CartJson);
                var processedCart = await cartService.JsonToDishList(cartViewModel);
                
                return Ok(new { success = true, data = processedCart });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "Error processing cart from JSON");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Tạo order từ giỏ hàng
        /// </summary>
        /// <param name="cartViewModel">Thông tin giỏ hàng</param>
        /// <returns>Kết quả tạo order</returns>
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CartViewModel cartViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid cart data", errors = ModelState });

                var result = await orderService.CreatedOrder(cartViewModel);
                
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    return BadRequest(new { 
                        success = false, 
                        message = result.ErrorMessage,
                        requiresAuth = result.ErrorMessage.Contains("login", StringComparison.OrdinalIgnoreCase)
                    });
                }

                // Broadcast new order to staff via SignalR
                if (result.Data != Guid.Empty)
                {
                    try
                    {
                        var customerRequests = await orderService.GetCustomerRequestsByStaffAsync();
                        await hubContext.Clients.All.SendAsync("ReceiveOrderList", customerRequests.Orders);
                    }
                    catch (Exception signalREx)
                    {
                        // Log SignalR error but don't fail the checkout
                        this.exceptionHandler.RaiseException(signalREx, "SignalR broadcast failed after order creation");
                    }
                }

                return Ok(new { 
                    success = true, 
                    message = result.SuccessMessage ?? "Order created successfully!",
                    orderId = result.Data
                });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "Error during checkout");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Validate giỏ hàng trước khi checkout
        /// </summary>
        /// <param name="cartViewModel">Thông tin giỏ hàng</param>
        /// <returns>Kết quả validation</returns>
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateCart([FromBody] CartViewModel cartViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid cart data", errors = ModelState });

                var processedCart = await cartService.JsonToDishList(cartViewModel);
                
                var validation = new
                {
                    isValid = processedCart.DishList?.Any() == true,
                    totalItems = processedCart.DishList?.Count() ?? 0,
                    totalAmount = processedCart.DishList?.Sum(d => d.Price * d.Quantity) ?? 0,
                    issues = new List<string>()
                };

                if (!validation.isValid)
                {
                    validation.issues.Add("Cart is empty");
                }

                return Ok(new { success = true, validation });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "Error validating cart");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Clear giỏ hàng
        /// </summary>
        /// <param name="ownerId">ID chủ nhà hàng</param>
        /// <param name="tableId">ID bàn</param>
        /// <returns>Kết quả clear cart</returns>
        [HttpDelete("{ownerId:guid}/table/{tableId:int}")]
        public IActionResult ClearCart(Guid ownerId, int tableId)
        {
            try
            {
                // TODO: Implement clear cart logic if needed
                return Ok(new { success = true, message = "Cart cleared successfully" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, $"Error clearing cart for Owner: {ownerId}, Table: {tableId}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }

    /// <summary>
    /// Request model cho cart JSON processing
    /// </summary>
    public class CartJsonRequest
    {
        public string CartJson { get; set; } = string.Empty;
    }
}
