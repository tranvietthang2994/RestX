using Microsoft.AspNetCore.Mvc;
using RestX.API.Services.Interfaces;
namespace RestX.API.Controllers
{
    [Route("Order")]
    public class OrderController : BaseController
    {
        private readonly IOrderService orderService;
        public OrderController(IOrderService orderService, IExceptionHandler exceptionHandler) : base(exceptionHandler)
        {
            this.orderService = orderService;
        }

        [HttpGet("History/{ownerId:guid}/{tableId:int}")]
        public async Task<IActionResult> History(Guid ownerId, int tableId)
        {
            var customerIdString = HttpContext.Session.GetString("AuthCustomerId");
            if (string.IsNullOrEmpty(customerIdString)) // Fixed CS7036 by using the correct method call
            {
                TempData["Message"] = "Bạn hãy vui lòng đăng nhập!";
                return RedirectToAction("Login", "AuthCustomer", new
                {
                    OwnerId = ownerId,
                    TableId = tableId
                });
            }
            var customerId = Guid.Parse(customerIdString);

            var model = await orderService.GetOrdersByCustomerIdOwnerIdAsync(ownerId, customerId);
            return View(model);
        }
    }
}
