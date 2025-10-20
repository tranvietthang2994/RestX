using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using RestX.WebApp.Models.ViewModels;
using RestX.WebApp.Services.Interfaces;
using RestX.WebApp.Services.SignalRLab;
using System.Text.Json;

namespace RestX.WebApp.Controllers
{
    public class CartController : BaseController
    {
        private readonly ICartService cartService;
        private readonly IOrderService orderService;
        private readonly IHubContext<SignalrServer> hubContext;
        public CartController(IExceptionHandler exceptionHandler, 
                              ICartService cartService, 
                              IOrderService orderService, 
                              IHubContext<SignalrServer> hubContext) : base(exceptionHandler)
        {
            this.cartService = cartService;
            this.orderService = orderService;
            this.hubContext = hubContext;
        }

        [HttpGet]
        [Route("Cart/Index/{ownerId:guid}/{tableId:int}")]
        public async Task<IActionResult> Index(CartViewModel model)
        {
            var tempModel = TempData["tempModel"];
            if (tempModel != null)
            {
                model = await cartService.JsonToCartViewModel(tempModel.ToString());
            }
            
            model = await cartService.JsonToDishList(model);

            if (model.Message != null) 
                ViewBag.Message = model.Message;

            return View(model);
        }

        [HttpPost]
        [Route("Cart/IndexPost/{ownerId:guid}/{tableId:int}")]
        public async Task<IActionResult> IndexPost(CartViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Message = "Ối! Có gì đó không ổn";
                TempData["tempModel"] = JsonSerializer.Serialize(model);
                return RedirectToAction("Index", new {OwnerId = model.OwnerId,
                                                      TableId = model.TableId});
            }

            UniversalValue<Guid> returnUVOrderId = await orderService.CreatedOrder(model);
            if (!returnUVOrderId.ErrorMessage.IsNullOrEmpty())
            {
                TempData["Message"] = returnUVOrderId.ErrorMessage;
                return RedirectToAction("Login", "AuthCustomer", new
                {
                    OwnerId = model.OwnerId,
                    TableId = model.TableId
                });
            }

            TempData["tempModel"] = JsonSerializer.Serialize(model);
            
            // Broadcast new order to staff in real-time
            if (returnUVOrderId.Data != Guid.Empty)
            {
                // Lấy toàn bộ danh sách order mới nhất
                var customerRequest = await orderService.GetCustomerRequestsByStaffAsync();
                await hubContext.Clients.All.SendAsync("ReceiveOrderList", customerRequest.Orders);
            }
            
            TempData["Message"] = returnUVOrderId.SuccessMessage;
            return RedirectToAction("Index", "Home", new { OwnerId = model.OwnerId,
                                                   TableId = model.TableId });
        }
    }
}
