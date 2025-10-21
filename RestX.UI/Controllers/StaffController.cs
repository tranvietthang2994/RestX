using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Controllers
{
    [Authorize(Roles = "Owner,Staff")]
    public class StaffController : Controller
    {
        private readonly IStaffUIService _staffService;
        private readonly IOrderUIService _orderService;
        private readonly IDishManagementUIService _dishService;
        private readonly ILogger<StaffController> _logger;

        public StaffController(
            IStaffUIService staffService,
            IOrderUIService orderService,
            IDishManagementUIService dishService,
            ILogger<StaffController> logger)
        {
            _staffService = staffService;
            _orderService = orderService;
            _dishService = dishService;
            _logger = logger;
        }

        /// <summary>
        /// Staff dashboard or profile page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUser = User;
                var userRole = currentUser?.FindFirst("Role")?.Value;

                if (userRole == "Staff")
                {
                    // Show staff profile for staff users
                    var profile = await _staffService.GetStaffProfileAsync();
                    return View("Profile", profile);
                }
                else if (userRole == "Owner")
                {
                    // Show staff management for owners
                    var staffManagement = await _staffService.GetStaffManagementAsync();
                    return View("StaffManagement", staffManagement);
                }

                return View("Error", new ErrorViewModel 
                { 
                    Message = "Unauthorized access",
                    StatusCode = 403
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading staff index page");
                return View("Error", new ErrorViewModel 
                { 
                    Message = "An error occurred while loading the page"
                });
            }
        }

        /// <summary>
        /// Staff profile page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var profile = await _staffService.GetStaffProfileAsync();
                
                if (profile == null)
                {
                    return View("Error", new ErrorViewModel 
                    { 
                        Message = "Unable to load staff profile"
                    });
                }

                return View(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading staff profile");
                return View("Error", new ErrorViewModel 
                { 
                    Message = "An error occurred while loading the profile"
                });
            }
        }

        /// <summary>
        /// Update staff profile
        /// </summary>
        /// <param name="model">Staff profile data</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> UpdateProfile(StaffProfileViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid data provided" });
                }

                var (success, message) = await _staffService.UpdateStaffProfileAsync(model);
                
                return Json(new { success, message = message ?? (success ? "Profile updated successfully" : "Update failed") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating staff profile");
                return Json(new { success = false, message = "An error occurred while updating the profile" });
            }
        }

        /// <summary>
        /// Staff management page (Owner only)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> StaffManagement()
        {
            try
            {
                var staffManagement = await _staffService.GetStaffManagementAsync();
                
                if (staffManagement == null)
                {
                    return View("Error", new ErrorViewModel 
                    { 
                        Message = "Unable to load staff management data"
                    });
                }

                return View(staffManagement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading staff management");
                return View("Error", new ErrorViewModel 
                { 
                    Message = "An error occurred while loading staff management"
                });
            }
        }

        /// <summary>
        /// Create new staff member
        /// </summary>
        /// <param name="model">Staff data</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> CreateStaff(StaffViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var (success, message) = await _staffService.CreateStaffAsync(model);
                
                return Json(new { success, message = message ?? (success ? "Staff member created successfully" : "Failed to create staff member") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating staff member");
                return Json(new { success = false, message = "An error occurred while creating the staff member" });
            }
        }

        /// <summary>
        /// Update staff member
        /// </summary>
        /// <param name="model">Updated staff data</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> UpdateStaff(StaffViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var (success, message) = await _staffService.UpdateStaffAsync(model);
                
                return Json(new { success, message = message ?? (success ? "Staff member updated successfully" : "Failed to update staff member") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating staff member");
                return Json(new { success = false, message = "An error occurred while updating the staff member" });
            }
        }

        /// <summary>
        /// Delete staff member
        /// </summary>
        /// <param name="staffId">Staff ID</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteStaff(Guid staffId)
        {
            try
            {
                var success = await _staffService.DeleteStaffAsync(staffId);
                
                if (success)
                {
                    return Json(new { success = true, message = "Staff member deleted successfully" });
                }
                
                return Json(new { success = false, message = "Failed to delete staff member" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting staff member: {StaffId}", staffId);
                return Json(new { success = false, message = "An error occurred while deleting the staff member" });
            }
        }

        /// <summary>
        /// Get staff by ID
        /// </summary>
        /// <param name="staffId">Staff ID</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetStaff(Guid staffId)
        {
            try
            {
                var staff = await _staffService.GetStaffByIdAsync(staffId);
                
                if (staff != null)
                {
                    return Json(new { success = true, data = staff });
                }
                
                return Json(new { success = false, message = "Staff member not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting staff by ID: {StaffId}", staffId);
                return Json(new { success = false, message = "An error occurred while loading staff data" });
            }
        }

        /// <summary>
        /// Kitchen/Order management page for staff
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
        /// Update dish availability
        /// </summary>
        /// <param name="dishId">Dish ID</param>
        /// <param name="isActive">Availability status</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateDishAvailability(int dishId, bool isActive)
        {
            try
            {
                var success = await _dishService.UpdateDishAvailabilityAsync(dishId, isActive);
                
                if (success)
                {
                    return Json(new { success = true, message = "Dish availability updated successfully" });
                }
                
                return Json(new { success = false, message = "Failed to update dish availability" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dish availability: {DishId} -> {IsActive}", dishId, isActive);
                return Json(new { success = false, message = "An error occurred while updating dish availability" });
            }
        }
    }
}