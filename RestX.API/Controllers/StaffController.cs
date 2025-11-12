using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RestX.API.Hubs;
using RestX.API.Models.DTOs.Request;
using RestX.API.Models.ViewModels;
using RestX.API.Services.Interfaces;

namespace RestX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "Owner,Staff")] // Staff functions and management
    public class StaffController : ControllerBase
    {
        private readonly IMenuService menuService;
        private readonly IStaffService staffService;
        private readonly ITableService tableService;
        private readonly IDishService dishService;
        private readonly IOrderService orderService;
        private readonly IOrderDetailService orderDetailService;
        private readonly IStaffManagementService staffManagementService;
        private readonly IHubContext<SignalrServer> hubContext;
        private readonly IExceptionHandler exceptionHandler;

        public StaffController(
            IMenuService menuService,
            IStaffService staffService,
            ITableService tableService,
            IDishService dishService,
            IOrderService orderService,
            IOrderDetailService orderDetailService,
            IStaffManagementService staffManagementService,
            IHubContext<SignalrServer> hubContext,
            IExceptionHandler exceptionHandler)
        {
            this.menuService = menuService;
            this.staffService = staffService;
            this.tableService = tableService;
            this.dishService = dishService;
            this.orderService = orderService;
            this.orderDetailService = orderDetailService;
            this.staffManagementService = staffManagementService;
            this.hubContext = hubContext;
            this.exceptionHandler = exceptionHandler;
        }

        // ===== STAFF FUNCTIONS (for staff users) =====

        /// <summary>
        /// Lấy danh sách yêu cầu của khách hàng
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Danh sách customer requests</returns>
        [HttpGet("customer-requests")]
        public async Task<IActionResult> GetCustomerRequests(CancellationToken cancellationToken = default)
        {
            try
            {
                var customerRequests = await orderService.GetCustomerRequestsByStaffAsync(cancellationToken);
                return Ok(new { success = true, data = customerRequests });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Staff not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while processing CustomerRequest for Staff");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy menu nhà hàng (staff view)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Menu data</returns>
        [HttpGet("menu")]
        public async Task<IActionResult> GetMenu(CancellationToken cancellationToken = default)
        {
            try
            {
                var menuViewModel = await menuService.GetMenuViewModelAsync(cancellationToken);
                return Ok(new { success = true, data = menuViewModel });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while processing Menu for Staff");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy trạng thái tất cả bàn (staff view)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Table status data</returns>
        [HttpGet("table-status")]
        public async Task<IActionResult> GetTableStatus(CancellationToken cancellationToken = default)
        {
            try
            {
                var tableStatus = await tableService.GetAllTablesByCurrentStaff(cancellationToken);
                return Ok(new { success = true, data = tableStatus });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Staff not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while processing StatusTable for Staff");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy profile của staff hiện tại
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Staff profile data</returns>
        //[HttpGet("profile")]
        //public async Task<IActionResult> GetProfile(CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        var staffProfile = await staffService.GetStaffProfileAsync(cancellationToken);
        //        return Ok(new { success = true, data = staffProfile });
        //    }
        //    catch (UnauthorizedAccessException)
        //    {
        //        return Unauthorized(new { success = false, message = "Staff not authenticated" });
        //    }
        //    catch (Exception ex)
        //    {
        //        this.exceptionHandler.RaiseException(ex, "An error occurred while processing Profile for Staff.");
        //        return StatusCode(500, new { success = false, message = "Internal server error" });
        //    }
        //}

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile(CancellationToken cancellationToken = default)
        {
            try
            {
                var profileDto = await staffService.GetStaffProfileAsync(cancellationToken);
                return Ok(new { success = true, data = profileDto });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Staff not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }


        /// <summary>
        /// Cập nhật tình trạng món ăn (available/unavailable)
        /// </summary>
        /// <param name="request">Request data</param>
        /// <returns>Update result</returns>
        [HttpPut("dish-availability")]
        public async Task<IActionResult> UpdateDishAvailability([FromBody] UpdateDishAvailabilityRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var success = await dishService.UpdateDishAvailabilityAsync(request.DishId, request.IsActive);
                
                if (success)
                    return Ok(new { success = true, message = "Dish availability updated successfully." });
                else
                    return BadRequest(new { success = false, message = "Failed to update dish availability." });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Staff not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, $"An error occurred while updating dish availability for DishId: {request.DishId}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Cập nhật trạng thái order detail
        /// </summary>
        /// <param name="request">Request data</param>
        /// <returns>Update result</returns>
        [HttpPut("order-detail-status")]
        public async Task<IActionResult> UpdateOrderDetailStatus([FromBody] UpdateOrderDetailStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var success = await orderDetailService.UpdateOrderDetailStatusAsync(request.OrderDetailId, request.IsActive);
                
                // Broadcast updated order list via SignalR
                if (success)
                {
                    try
                    {
                        var customerRequest = await orderService.GetCustomerRequestsByStaffAsync();
                        await hubContext.Clients.All.SendAsync("ReceiveOrderList", customerRequest.Orders);
                    }
                    catch (Exception signalREx)
                    {
                        // Log SignalR error but don't fail the update
                        this.exceptionHandler.RaiseException(signalREx, "SignalR broadcast failed after order detail update");
                    }
                }

                return Ok(new { 
                    success = success, 
                    message = success ? "Order detail status updated successfully." : "Failed to update order detail status." 
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Staff not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, $"An error occurred while updating order detail status for OrderDetailId: {request.OrderDetailId}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy thông tin staff hiện tại
        /// </summary>
        /// <returns>Current staff info</returns>
        [HttpGet("current-info")]
        public async Task<IActionResult> GetCurrentStaffInfo()
        {
            try
            {
                var staffId = RestX.API.Extensions.UserHelper.GetCurrentStaffId();
                var staff = await staffManagementService.GetStaffByIdAsync(staffId);
                
                if (staff == null)
                    return NotFound(new { success = false, message = "Staff not found" });

                return Ok(new { success = true, data = staff });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Staff not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading current staff info.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy staff view model hiện tại
        /// </summary>
        /// <returns>Current staff view model</returns>
        [HttpGet("current-viewmodel")]
        public async Task<IActionResult> GetCurrentStaffViewModel()
        {
            try
            {
                var staffId = RestX.API.Extensions.UserHelper.GetCurrentStaffId();
                var staffViewModel = await staffManagementService.GetStaffViewModelByIdAsync(staffId);
                
                if (staffViewModel == null)
                    return NotFound(new { success = false, message = "Staff not found" });

                return Ok(new { success = true, data = staffViewModel });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Staff not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading current staff info.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // ===== STAFF MANAGEMENT FUNCTIONS (for owner) =====
        // Note: These should ideally be in a separate StaffManagementController

        /// <summary>
        /// Lấy danh sách staff theo owner
        /// </summary>
        /// <returns>List of staff under current owner</returns>
        [HttpGet("management")]
        public async Task<IActionResult> GetStaffsByOwner()
        {
            try
            {
                var ownerId = RestX.API.Extensions.UserHelper.GetCurrentOwnerId();
                var staffs = await staffManagementService.GetStaffsByOwnerIdAsync(ownerId);
                return Ok(new { success = true, data = staffs });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Owner not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading staffs.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy staff management view model
        /// </summary>
        /// <returns>Staff management data</returns>
        [HttpGet("management/viewmodel")]
        public async Task<IActionResult> GetStaffManagementViewModel()
        {
            try
            {
                var ownerId = RestX.API.Extensions.UserHelper.GetCurrentOwnerId();
                var viewModel = await staffManagementService.GetStaffManagementViewModelAsync(ownerId);
                return Ok(new { success = true, data = viewModel });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Owner not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading staff management.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Tạo mới hoặc cập nhật staff
        /// </summary>
        /// <param name="request">Staff data</param>
        /// <returns>Operation result</returns>
        [HttpPost("management")]
        public async Task<IActionResult> CreateStaff([FromBody] StaffRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var ownerId = RestX.API.Extensions.UserHelper.GetCurrentOwnerId();
                
                // Ensure it's a new staff
                request.Id = null;
                
                var resultStaffId = await staffManagementService.UpsertStaffAsync(request, ownerId);

                if (resultStaffId == null)
                    return BadRequest(new { success = false, message = "Operation failed" });

                return CreatedAtAction(nameof(GetStaffById), new { id = resultStaffId }, 
                    new { success = true, message = "Staff created successfully!", staffId = resultStaffId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Owner not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while creating the staff.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Cập nhật staff
        /// </summary>
        /// <param name="id">Staff ID</param>
        /// <param name="request">Updated staff data</param>
        /// <returns>Operation result</returns>
        [HttpPut("management/{id:guid}")]
        public async Task<IActionResult> UpdateStaff(Guid id, [FromBody] StaffRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var ownerId = RestX.API.Extensions.UserHelper.GetCurrentOwnerId();
                
                // Ensure the ID matches
                request.Id = id;
                
                var resultStaffId = await staffManagementService.UpsertStaffAsync(request, ownerId);

                if (resultStaffId == null)
                    return NotFound(new { success = false, message = "Staff not found" });

                return Ok(new { success = true, message = "Staff updated successfully!", staffId = resultStaffId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Owner not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while updating the staff.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy thông tin staff theo ID
        /// </summary>
        /// <param name="id">Staff ID</param>
        /// <returns>Staff info</returns>
        [HttpGet("management/{id:guid}")]
        public async Task<IActionResult> GetStaffById(Guid id)
        {
            try
            {
                var ownerId = RestX.API.Extensions.UserHelper.GetCurrentOwnerId();
                var staff = await staffManagementService.GetStaffByIdAsync(id);

                if (staff == null)
                    return NotFound(new { success = false, message = "Staff not found" });

                if (staff.OwnerId != ownerId)
                    return Forbid();

                var staffViewModel = await staffManagementService.GetStaffViewModelByIdAsync(id);
                return Ok(new { success = true, data = staffViewModel });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Owner not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading staff detail.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Xóa staff
        /// </summary>
        /// <param name="id">Staff ID</param>
        /// <returns>Delete result</returns>
        [HttpDelete("management/{id:guid}")]
        public async Task<IActionResult> DeleteStaff(Guid id)
        {
            try
            {
                var ownerId = RestX.API.Extensions.UserHelper.GetCurrentOwnerId();
                var staff = await staffManagementService.GetStaffByIdAsync(id);

                if (staff == null)
                    return NotFound(new { success = false, message = "Staff not found" });

                if (staff.OwnerId != ownerId)
                    return Forbid();

                var success = await staffManagementService.DeleteStaffAsync(id);
                
                if (success)
                    return Ok(new { success = true, message = "Staff deleted successfully!" });
                else
                    return BadRequest(new { success = false, message = "Staff not found or could not be deleted." });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Owner not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while deleting the staff.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }
}
