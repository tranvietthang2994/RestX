using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Controllers
{
    [Authorize(Roles = "Owner,Staff")]
    [Route("Customer")]
    public class CustomerController : Controller
    {
        private readonly ICustomerUIService _customerService;
        private readonly IOrderUIService _orderService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(
            ICustomerUIService customerService,
            IOrderUIService orderService,
            ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Customer management page
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]  // Matches /Customer
        [HttpGet("Index")]  // Matches /Customer/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading customers management page");

                var customerManagement = await _customerService.GetCustomerManagementAsync();

                if (customerManagement == null)
                {
                    return View("Error", new ErrorViewModel
                    {
                        Message = "Unable to load customer management data"
                    });
                }

                if (!string.IsNullOrEmpty(customerManagement.ErrorMessage))
                {
                    return View("Error", new ErrorViewModel
                    {
                        Message = customerManagement.ErrorMessage
                    });
                }

                return View("~/Views/Management/CustomersManagement.cshtml", customerManagement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customers management page");
                return View("Error", new ErrorViewModel
                {
                    Message = "An error occurred while loading customers management"
                });
            }
        }

        /// <summary>
        /// Get all customers as JSON
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAll")]  // Matches /Customer/GetAll
        public async Task<IActionResult> GetCustomers()
        {
            try
            {
                var customers = await _customerService.GetCustomersAsync();
                return Json(new { success = true, data = customers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers");
                return Json(new { success = false, message = "An error occurred while loading customers" });
            }
        }

        /// <summary>
        /// Get customer by ID
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns></returns>
        [HttpGet("Get/{customerId:guid}")]  // Matches /Customer/Get/{guid}
        public async Task<IActionResult> GetCustomer(Guid customerId)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(customerId);
                
                if (customer != null)
                {
                    return Json(new { success = true, data = customer });
                }
                
                return Json(new { success = false, message = "Customer not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by ID: {CustomerId}", customerId);
                return Json(new { success = false, message = "An error occurred while loading customer data" });
            }
        }

        /// <summary>
        /// Search customers
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns></returns>
        [HttpGet("Search")]  // Matches /Customer/Search?searchTerm=...
        public async Task<IActionResult> SearchCustomers(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return Json(new { success = true, data = new List<CustomerViewModel>() });
                }

                var customers = await _customerService.SearchCustomersAsync(searchTerm);
                return Json(new { success = true, data = customers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customers with term: {SearchTerm}", searchTerm);
                return Json(new { success = false, message = "An error occurred while searching customers" });
            }
        }

        /// <summary>
        /// Create new customer
        /// </summary>
        /// <param name="model">Customer data</param>
        /// <returns></returns>
        [HttpPost("Create")]  // Matches /Customer/Create
        public async Task<IActionResult> CreateCustomer(CustomerViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var (success, message) = await _customerService.CreateCustomerAsync(model);
                
                return Json(new { success, message = message ?? (success ? "Customer created successfully" : "Failed to create customer") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer: {CustomerName}", model.Name);
                return Json(new { success = false, message = "An error occurred while creating the customer" });
            }
        }

        /// <summary>
        /// Update customer
        /// </summary>
        /// <param name="model">Updated customer data</param>
        /// <returns></returns>
        [HttpPost("Update")]  // Matches /Customer/Update
        public async Task<IActionResult> UpdateCustomer(CustomerViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var (success, message) = await _customerService.UpdateCustomerAsync(model);
                
                return Json(new { success, message = message ?? (success ? "Customer updated successfully" : "Failed to update customer") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer ID: {CustomerId}", model.Id);
                return Json(new { success = false, message = "An error occurred while updating the customer" });
            }
        }

        /// <summary>
        /// Upsert customer (Create or Update)
        /// </summary>
        /// <param name="model">Customer data</param>
        /// <returns></returns>
        [HttpPost("Upsert")]  // Matches /Customer/Upsert
        public async Task<IActionResult> UpsertCustomer(CustomerViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                // If ID is empty or default Guid, it's a create operation
                if (model.Id == Guid.Empty)
                {
                    var (createSuccess, createMessage) = await _customerService.CreateCustomerAsync(model);
                    return Json(new { success = createSuccess, message = createMessage ?? (createSuccess ? "Customer created successfully" : "Failed to create customer") });
                }
                else
                {
                    var (updateSuccess, updateMessage) = await _customerService.UpdateCustomerAsync(model);
                    return Json(new { success = updateSuccess, message = updateMessage ?? (updateSuccess ? "Customer updated successfully" : "Failed to update customer") });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting customer: {CustomerName}", model.Name);
                return Json(new { success = false, message = "An error occurred while saving the customer" });
            }
        }

        /// <summary>
        /// Delete customer
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns></returns>
        [HttpDelete("Delete/{customerId:guid}")]  // Matches /Customer/Delete/{guid}
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteCustomer(Guid customerId)
        {
            try
            {
                var success = await _customerService.DeleteCustomerAsync(customerId);
                
                if (success)
                {
                    return Json(new { success = true, message = "Customer deleted successfully" });
                }
                
                return Json(new { success = false, message = "Failed to delete customer" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer: {CustomerId}", customerId);
                return Json(new { success = false, message = "An error occurred while deleting the customer" });
            }
        }

        /// <summary>
        /// Get customer order history
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns></returns>
        [HttpGet("Orders/{customerId:guid}")]  // Matches /Customer/Orders/{guid}
        public async Task<IActionResult> GetCustomerOrders(Guid customerId)
        {
            try
            {
                var orders = await _orderService.GetCustomerOrderHistoryAsync(customerId);
                return Json(new { success = true, data = orders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer orders: {CustomerId}", customerId);
                return Json(new { success = false, message = "An error occurred while loading customer orders" });
            }
        }

        /// <summary>
        /// Customer details page
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns></returns>
        [HttpGet("Details/{customerId:guid}")]  // Matches /Customer/Details/{guid}
        public async Task<IActionResult> Details(Guid customerId)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(customerId);
                
                if (customer == null)
                {
                    return Json(new { success = false, message = "Customer not found" });
                }

                // Get customer orders
                var orders = await _orderService.GetCustomerOrderHistoryAsync(customerId);
                customer.TotalOrders = orders.Count;
                customer.TotalSpent = orders.Sum(o => o.TotalAmount);
                customer.LastOrderDate = orders.OrderByDescending(o => o.OrderDate).FirstOrDefault()?.OrderDate;

                return Json(new { success = true, data = customer });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer details: {CustomerId}", customerId);
                return Json(new { success = false, message = "An error occurred while loading customer details" });
            }
        }

        /// <summary>
        /// Export customers to CSV
        /// </summary>
        /// <returns></returns>
        [HttpGet("Export")]  // Matches /Customer/Export
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> ExportCustomers()
        {
            try
            {
                var customers = await _customerService.GetCustomersAsync();
                
                // Simple CSV export
                var csv = "Name,Phone,Email,Point,Status,Registration Date\n";
                foreach (var customer in customers)
                {
                    csv += $"{customer.Name},{customer.Phone},{customer.Email},{customer.Point},{customer.StatusText},{customer.CreatedDate:yyyy-MM-dd}\n";
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                return File(bytes, "text/csv", $"customers_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting customers");
                return Json(new { success = false, message = "An error occurred while exporting customers" });
            }
        }
    }
}