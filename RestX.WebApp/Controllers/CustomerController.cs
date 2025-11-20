using Microsoft.AspNetCore.Mvc;
using RestX.WebApp.Helper;
using RestX.WebApp.Models;
using RestX.WebApp.Models.ViewModels;
using RestX.WebApp.Services.Interfaces;
using System.Diagnostics;

namespace RestX.WebApp.Controllers
{
    [Route("Customer")]
    public class CustomerController : Controller
    {
        private readonly ICustomerService customerService;
        private readonly IExceptionHandler exceptionHandler;

        public CustomerController(
            ICustomerService customerService,
            IExceptionHandler exceptionHandler)
        {
            this.customerService = customerService;
            this.exceptionHandler = exceptionHandler;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> CustomersManagement()
        {
            try
            {
                var customers = await customerService.GetCustomersByOwnerIdAsync();
                var model = new CustomersManagementViewModel { Customers = customers };
                return View(model);
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading customers.");
                return View("Error");
            }
        }

        [HttpGet("Detail/{id:guid}")]
        public async Task<IActionResult> CustomerDetail(Guid id)
        {
            var customer = await customerService.GetCustomerByIdAsync(id);
            if (customer == null)
                return Json(new { success = false, message = "Customer not found." });
            return Json(new { success = true, data = customer });
        }

        [HttpPost("Upsert")]
        public async Task<IActionResult> UpsertCustomer([FromForm] CustomerViewModel model)
        {
            try
            {
                var resultId = await customerService.UpsertCustomerAsync(model);
                if (resultId == null)
                    return Json(new { success = false, message = "Operation failed." });

                string operation = model.Id != Guid.Empty ? "updated" : "created";
                return Json(new { success = true, message = $"Customer {operation} successfully!", customerId = resultId });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while saving the customer.");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("Delete/{id:guid}")]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            try
            {
                var result = await customerService.DeleteCustomerAsync(id);
                if (!result)
                    return Json(new { success = false, message = "Customer not found." });
                return Json(new { success = true, message = "Customer deleted successfully!" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while deleting the customer.");
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}