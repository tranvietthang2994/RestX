using Microsoft.AspNetCore.Mvc;
using RestX.API.Models.ViewModels;
using RestX.API.Services.Interfaces;

namespace RestX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
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

        /// <summary>
        /// Lấy danh sách khách hàng theo Owner ID
        /// </summary>
        /// <returns>Danh sách khách hàng</returns>
        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            try
            {
                var customers = await customerService.GetCustomersByOwnerIdAsync();
                return Ok(new { success = true, data = customers });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading customers.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết khách hàng theo ID
        /// </summary>
        /// <param name="id">ID khách hàng</param>
        /// <returns>Thông tin chi tiết khách hàng</returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCustomerById(Guid id)
        {
            try
            {
                var customer = await customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                    return NotFound(new { success = false, message = "Customer not found" });

                return Ok(new { success = true, data = customer });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, $"An error occurred while loading customer {id}.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Tạo mới khách hàng
        /// </summary>
        /// <param name="model">Thông tin khách hàng</param>
        /// <returns>Kết quả tạo khách hàng</returns>
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                // Ensure it's a new customer
                model.Id = Guid.Empty;
                
                var resultId = await customerService.UpsertCustomerAsync(model);
                if (resultId == null)
                    return BadRequest(new { success = false, message = "Operation failed" });

                return CreatedAtAction(nameof(GetCustomerById), new { id = resultId }, 
                    new { success = true, message = "Customer created successfully!", customerId = resultId });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while creating the customer.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Cập nhật thông tin khách hàng
        /// </summary>
        /// <param name="id">ID khách hàng</param>
        /// <param name="model">Thông tin cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] CustomerViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                // Ensure the ID matches
                model.Id = id;
                
                var resultId = await customerService.UpsertCustomerAsync(model);
                if (resultId == null)
                    return NotFound(new { success = false, message = "Customer not found" });

                return Ok(new { success = true, message = "Customer updated successfully!", customerId = resultId });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while updating the customer.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Xóa khách hàng
        /// </summary>
        /// <param name="id">ID khách hàng</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            try
            {
                var result = await customerService.DeleteCustomerAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Customer not found" });

                return Ok(new { success = true, message = "Customer deleted successfully!" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while deleting the customer.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Tìm kiếm khách hàng theo số điện thoại
        /// </summary>
        /// <param name="phone">Số điện thoại</param>
        /// <returns>Danh sách khách hàng</returns>
        [HttpGet("search")]
        public async Task<IActionResult> SearchCustomerByPhone([FromQuery] string phone)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phone))
                    return BadRequest(new { success = false, message = "Phone number is required" });

                // Note: Might need to implement search method in CustomerService
                var customers = await customerService.GetCustomersByOwnerIdAsync();
                var filteredCustomers = customers.Where(c => 
                    c.Phone != null && c.Phone.Contains(phone, StringComparison.OrdinalIgnoreCase)).ToList();

                return Ok(new { success = true, data = filteredCustomers });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, $"An error occurred while searching customers by phone: {phone}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }
}
