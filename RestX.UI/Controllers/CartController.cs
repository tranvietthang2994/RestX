using Microsoft.AspNetCore.Mvc;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;
using System.Numerics;
using System.Text.Json;

namespace RestX.UI.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartUIService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartUIService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        /// <summary>
        /// Display cart page
        /// </summary>
        /// <param name="ownerId">Restaurant owner ID</param>
        /// <param name="tableId">Table ID</param>
        /// <returns></returns>
        //[HttpGet]
        //[Route("Cart/Index/{ownerId:guid}/{tableId:int}")]
        //public async Task<IActionResult> Index(Guid ownerId, int tableId)
        //{
        //    try
        //    {
        //        _logger.LogInformation("Loading cart for owner: {OwnerId}, table: {TableId}", ownerId, tableId);

        //        var cart = await _cartService.GetCartAsync(ownerId, tableId);

        //        if (cart == null)
        //        {
        //            cart = new CartViewModel 
        //            { 
        //                OwnerId = ownerId, 
        //                TableId = tableId,
        //                ErrorMessage = "Unable to load cart"
        //            };
        //        }

        //        // Store context
        //        HttpContext.Session.SetString("OwnerId", ownerId.ToString());
        //        HttpContext.Session.SetString("TableId", tableId.ToString());

        //        return View(cart);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error loading cart for owner: {OwnerId}, table: {TableId}", ownerId, tableId);
        //        return View("Error", new ErrorViewModel 
        //        { 
        //            Message = "An error occurred while loading the cart"
        //        });
        //    }
        //}

        [HttpGet]
        [Route("Cart/Index/{ownerId:guid}/{tableId:int}")]
        public async Task<IActionResult> Index(CartViewModel model)
        {
            var tempModel = TempData["tempModel"];
            if (tempModel != null)
            {
                model = await _cartService.JsonToCartViewModel(tempModel.ToString());
            }

            model = await _cartService.JsonToDishList(model);

            if (model.Message != null)
                ViewBag.Message = model.Message;

            return View(model);
        }

        //[HttpPost]
        //[Route("Cart/IndexPost/{ownerId:guid}/{tableId:int}")]
        //public async Task<IActionResult> IndexPost(CartViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        model.Message = "Ối! Có gì đó không ổn";
        //        TempData["tempModel"] = JsonSerializer.Serialize(model);
        //        return RedirectToAction("Index", new
        //        {
        //            OwnerId = model.OwnerId,
        //            TableId = model.TableId
        //        });
        //    }

        //    UniversalValue<Guid> returnUVOrderId = await orderService.CreatedOrder(model);
        //    if (!returnUVOrderId.ErrorMessage.IsNullOrEmpty())
        //    {
        //        TempData["Message"] = returnUVOrderId.ErrorMessage;
        //        return RedirectToAction("Login", "AuthCustomer", new
        //        {
        //            OwnerId = model.OwnerId,
        //            TableId = model.TableId
        //        });
        //    }

        //    TempData["tempModel"] = JsonSerializer.Serialize(model);

        //    // Broadcast new order to staff in real-time
        //    if (returnUVOrderId.Data != Guid.Empty)
        //    {
        //        // Lấy toàn bộ danh sách order mới nhất
        //        var customerRequest = await orderService.GetCustomerRequestsByStaffAsync();
        //        await hubContext.Clients.All.SendAsync("ReceiveOrderList", customerRequest.Orders);
        //    }

        //    TempData["Message"] = returnUVOrderId.SuccessMessage;
        //    return RedirectToAction("Index", "Home", new
        //    {
        //        OwnerId = model.OwnerId,
        //        TableId = model.TableId
        //    });
        //}

        /// <summary>
        /// Add item to cart
        /// </summary>
        /// <param name="dishId">Dish ID</param>
        /// <param name="quantity">Quantity</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddToCart(int dishId, int quantity = 1)
        {
            try
            {
                var ownerIdString = HttpContext.Session.GetString("OwnerId");
                var tableIdString = HttpContext.Session.GetString("TableId");

                if (!Guid.TryParse(ownerIdString, out var ownerId) || 
                    !int.TryParse(tableIdString, out var tableId))
                {
                    return Json(new { success = false, message = "Invalid session data" });
                }

                var cart = await _cartService.AddToCartAsync(ownerId, tableId, dishId, quantity);
                
                if (cart != null && string.IsNullOrEmpty(cart.ErrorMessage))
                {
                    return Json(new { success = true, message = "Item added to cart", data = cart });
                }
                
                return Json(new { success = false, message = cart?.ErrorMessage ?? "Failed to add item to cart" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding dish {DishId} to cart", dishId);
                return Json(new { success = false, message = "An error occurred while adding item to cart" });
            }
        }

        /// <summary>
        /// Remove item from cart
        /// </summary>
        /// <param name="dishId">Dish ID</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int dishId)
        {
            try
            {
                var ownerIdString = HttpContext.Session.GetString("OwnerId");
                var tableIdString = HttpContext.Session.GetString("TableId");

                if (!Guid.TryParse(ownerIdString, out var ownerId) || 
                    !int.TryParse(tableIdString, out var tableId))
                {
                    return Json(new { success = false, message = "Invalid session data" });
                }

                var cart = await _cartService.RemoveFromCartAsync(ownerId, tableId, dishId);
                
                if (cart != null && string.IsNullOrEmpty(cart.ErrorMessage))
                {
                    return Json(new { success = true, message = "Item removed from cart", data = cart });
                }
                
                return Json(new { success = false, message = cart?.ErrorMessage ?? "Failed to remove item from cart" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing dish {DishId} from cart", dishId);
                return Json(new { success = false, message = "An error occurred while removing item from cart" });
            }
        }

        /// <summary>
        /// Update item quantity in cart
        /// </summary>
        /// <param name="dishId">Dish ID</param>
        /// <param name="quantity">New quantity</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int dishId, int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    return await RemoveFromCart(dishId);
                }

                var ownerIdString = HttpContext.Session.GetString("OwnerId");
                var tableIdString = HttpContext.Session.GetString("TableId");

                if (!Guid.TryParse(ownerIdString, out var ownerId) || 
                    !int.TryParse(tableIdString, out var tableId))
                {
                    return Json(new { success = false, message = "Invalid session data" });
                }

                var cart = await _cartService.UpdateQuantityAsync(ownerId, tableId, dishId, quantity);
                
                if (cart != null && string.IsNullOrEmpty(cart.ErrorMessage))
                {
                    return Json(new { success = true, message = "Quantity updated", data = cart });
                }
                
                return Json(new { success = false, message = cart?.ErrorMessage ?? "Failed to update quantity" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quantity for dish {DishId} to {Quantity}", dishId, quantity);
                return Json(new { success = false, message = "An error occurred while updating quantity" });
            }
        }

        /// <summary>
        /// Clear cart
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var ownerIdString = HttpContext.Session.GetString("OwnerId");
                var tableIdString = HttpContext.Session.GetString("TableId");

                if (!Guid.TryParse(ownerIdString, out var ownerId) || 
                    !int.TryParse(tableIdString, out var tableId))
                {
                    return Json(new { success = false, message = "Invalid session data" });
                }

                var success = await _cartService.ClearCartAsync(ownerId, tableId);
                
                if (success)
                {
                    return Json(new { success = true, message = "Cart cleared successfully" });
                }
                
                return Json(new { success = false, message = "Failed to clear cart" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return Json(new { success = false, message = "An error occurred while clearing cart" });
            }
        }

        /// <summary>
        /// Checkout cart
        /// </summary>
        /// <param name="customerName">Customer name</param>
        /// <param name="customerPhone">Customer phone</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Checkout(CartViewModel model)
        {

            var customerIdString = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdString))
            {
                TempData["Message"] = "Bạn hãy vui lòng đăng nhập!";
                return RedirectToAction("Login", "AuthCustomer", new
                {
                    OwnerId = model.OwnerId,
                    TableId = model.TableId,
                });
            }

            try
            {
                //if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(customerPhone))
                //{
                //    return Json(new { success = false, message = "Customer name and phone are required" });
                //}

                var ownerIdString = HttpContext.Session.GetString("OwnerId");
                var tableIdString = HttpContext.Session.GetString("TableId");

                if (!Guid.TryParse(ownerIdString, out var ownerId) || 
                    !int.TryParse(tableIdString, out var tableId))
                {
                    return Json(new { success = false, message = "Invalid session data" });
                }

                var success = await _cartService.CheckoutAsync(ownerId, tableId, model);

                TempData["tempModel"] = JsonSerializer.Serialize(model);


                if (success)
                {
                    TempData["Message"] = "Order placed successfully!";
                    return RedirectToAction("Index", "Home", new
                    {
                        OwnerId = model.OwnerId,
                        TableId = model.TableId
                    });
                }

                TempData["Message"] = "Failed to place order";
                return RedirectToAction("Index", "Home", new
                {
                    OwnerId = model.OwnerId,
                    TableId = model.TableId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout for customer");
                return Json(new { success = false, message = "An error occurred while placing the order" });
            }
        }

        /// <summary>
        /// Get current cart data as JSON
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetCartData()
        {
            try
            {
                var ownerIdString = HttpContext.Session.GetString("OwnerId");
                var tableIdString = HttpContext.Session.GetString("TableId");

                if (!Guid.TryParse(ownerIdString, out var ownerId) || 
                    !int.TryParse(tableIdString, out var tableId))
                {
                    return Json(new { success = false, message = "Invalid session data" });
                }

                var cart = await _cartService.GetCartAsync(ownerId, tableId);
                
                if (cart != null && string.IsNullOrEmpty(cart.ErrorMessage))
                {
                    return Json(new { success = true, data = cart });
                }
                
                return Json(new { success = false, message = cart?.ErrorMessage ?? "Failed to load cart data" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart data");
                return Json(new { success = false, message = "An error occurred while loading cart data" });
            }
        }
    }
}