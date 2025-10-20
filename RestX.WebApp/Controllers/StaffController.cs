using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RestX.WebApp.Models;
using RestX.WebApp.Services.DataTransferObjects;
using RestX.WebApp.Services.Interfaces;
using RestX.WebApp.Models.ViewModels;
using RestX.WebApp.Helper;
using System.Diagnostics;

namespace RestX.WebApp.Controllers
{
    public class StaffController : BaseController
    {
        public IMenuService menuService { get; }
        private readonly IStaffService staffService;
        private readonly ITableService tableService;
        private readonly IDishService dishService;
        private readonly IOrderService orderService;
        private readonly IOrderDetailService orderDetailService;
        private readonly IStaffManagementService staffManagementService;

        public StaffController(IMenuService menuService, IStaffService staffService, ITableService tableService, IDishService dishService, IOrderService orderService, IOrderDetailService orderDetailService, IStaffManagementService staffManagementService, IExceptionHandler exceptionHandler) : base(exceptionHandler)
        {
            this.menuService = menuService;
            this.staffService = staffService;
            this.tableService = tableService;
            this.dishService = dishService;
            this.orderService = orderService;
            this.orderDetailService = orderDetailService;
            this.staffManagementService = staffManagementService;
        }

        // ===== STAFF FUNCTIONS (for staff users) =====
        [HttpGet]
        [Route("Staff/CustomerRequest")]
        public async Task<IActionResult> CustomerRequest(CancellationToken cancellationToken)
        {
            try
            {
                var model = await orderService.GetCustomerRequestsByStaffAsync(cancellationToken);
                return View(model);
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while processing CustomerRequest for Staff");
                return this.BadRequest("An unexpected error occurred. Please try again later.");
            }
        }

        [HttpGet]
        [Route("Staff/Menu")]
        public async Task<IActionResult> Menu(CancellationToken cancellationToken)
        {
            try
            {
                var model = await menuService.GetMenuViewModelAsync(cancellationToken);
                return View(model);
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while processing Menu for Staff");
                return this.BadRequest("An unexpected error occurred. Please try again later.");
            }
        }

        [HttpGet]
        [Route("Staff/StatusTable")]
        public async Task<IActionResult> StatusTable(CancellationToken cancellationToken)
        {
            try
            {
                var model = await tableService.GetAllTablesByCurrentStaff(cancellationToken);
                return View(model);
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while processing StatusTable for Staff");
                return this.BadRequest("An unexpected error occurred. Please try again later.");
            }
        }

        [HttpGet]
        [Route("Staff/Profile")]
        public async Task<IActionResult> Profile(CancellationToken cancellationToken)
        {
            try
            {
                var staff = await staffService.GetStaffProfileAsync(cancellationToken);
                return View(staff);
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while processing Profile for Staff.");
                return this.BadRequest("An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPost]
        [Route("Staff/UpdateDishAvailability")]
        public async Task<IActionResult> UpdateDishAvailability([FromBody] UpdateDishAvailability request)
        {
            try
            {
                var success = await dishService.UpdateDishAvailabilityAsync(request.DishId, request.IsActive);
                return Json(new { success = true, message = "Dish availability updated successfully." });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, $"An error occurred while updating dish availability for DishId: {request.DishId}");
                return Json(new { success = false, message = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpPost]
        [Route("Staff/UpdateOrderDetailStatus")]
        public async Task<IActionResult> UpdateOrderDetailStatus([FromBody] UpdateOrderDetailStatus request)
        {
            try
            {
                var success = await orderDetailService.UpdateOrderDetailStatusAsync(request.OrderDetailId, request.IsActive);
                // Broadcast toàn bộ danh sách order mới nhất
                if (success)
                {
                    var customerRequest = await orderService.GetCustomerRequestsByStaffAsync();
                    var hubContext = HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.SignalR.IHubContext<RestX.WebApp.Services.SignalRLab.SignalrServer>)) as Microsoft.AspNetCore.SignalR.IHubContext<RestX.WebApp.Services.SignalRLab.SignalrServer>;
                    if (hubContext != null)
                    {
                        await hubContext.Clients.All.SendAsync("ReceiveOrderList", customerRequest.Orders);
                    }
                }
                return Json(new { success = success, message = success ? "Order detail status updated successfully." : "Failed to update order detail status." });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, $"An error occurred while updating order detail status for OrderDetailId: {request.OrderDetailId}");
                return Json(new { success = false, message = "An unexpected error occurred. Please try again later." });
            }
        }

        // ===== STAFF MANAGEMENT FUNCTIONS (for owner) =====

        [HttpGet]
        [Route("StaffManagement/Index")]
        public async Task<IActionResult> StaffManagementIndex(CancellationToken cancellationToken)
        {
            try
            {
                var ownerId = UserHelper.GetCurrentOwnerId();
                var model = await staffManagementService.GetStaffManagementViewModelAsync(ownerId);
                return View("~/Views/StaffManagement/Index.cshtml", model);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading staff management.");
                return View("Error");
            }
        }

        [HttpGet]
        [Route("StaffManagement/StaffManagement")]
        public async Task<IActionResult> StaffManagement(CancellationToken cancellationToken)
        {
            try
            {
                var ownerId = UserHelper.GetCurrentOwnerId();
                var model = await staffManagementService.GetStaffManagementViewModelAsync(ownerId);
                return View("~/Views/StaffManagement/StaffManagement.cshtml", model);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading staff management.");
                return View("Error");
            }
        }

        // API endpoints for Staff Management
        [HttpPost]
        [Route("StaffManagement/Upsert")]
        public async Task<IActionResult> UpsertStaff([FromForm] StaffRequest request)
        {
            try
            {
                var ownerId = UserHelper.GetCurrentOwnerId();
                var resultStaffId = await staffManagementService.UpsertStaffAsync(request, ownerId);

                if (resultStaffId == null)
                    return Json(new { success = false, message = "Operation failed." });

                string operation = request.Id.HasValue && request.Id.Value != Guid.Empty ? "updated" : "created";
                return Json(new { success = true, message = $"Staff {operation} successfully!", staffId = resultStaffId });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while saving the staff.");
                return Json(new { success = false, message = "An unexpected error occurred while saving staff." });
            }
        }

        [HttpPost]
        [Route("StaffManagement/Delete/{id:guid}")]
        public async Task<IActionResult> DeleteStaff(Guid id)
        {
            try
            {
                var ownerId = UserHelper.GetCurrentOwnerId();
                var staff = await staffManagementService.GetStaffByIdAsync(id);

                if (staff == null)
                    return Json(new { success = false, message = "Staff not found." });

                if (staff.OwnerId != ownerId)
                    return Json(new { success = false, message = "Unauthorized to delete this staff." });

                var success = await staffManagementService.DeleteStaffAsync(id);
                if (success)
                {
                    return Json(new { success = true, message = "Staff deleted successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Staff not found or could not be deleted." });
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while deleting the staff.");
                return Json(new { success = false, message = "An error occurred while deleting the staff." });
            }
        }

        [HttpGet]
        [Route("StaffManagement/Detail/{id:guid}")]
        public async Task<IActionResult> StaffDetail(Guid id)
        {
            try
            {
                var ownerId = UserHelper.GetCurrentOwnerId();
                var staff = await staffManagementService.GetStaffByIdAsync(id);

                if (staff == null)
                    return Json(new { success = false, message = "Staff not found." });

                if (staff.OwnerId != ownerId)
                    return Json(new { success = false, message = "Unauthorized to view this staff." });

                var staffViewModel = await staffManagementService.GetStaffViewModelByIdAsync(id);
                return Json(new { success = true, data = staffViewModel });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading staff detail.");
                return Json(new { success = false, message = "An error occurred while loading staff detail." });
            }
        }

        // Staff info endpoints
        [HttpGet]
        [Route("Staff/GetCurrentStaffInfo")]
        public async Task<IActionResult> GetCurrentStaffInfo()
        {
            try
            {
                var staffId = UserHelper.GetCurrentStaffId();
                var staff = await staffManagementService.GetStaffByIdAsync(staffId);
                if (staff == null)
                    return Json(new { success = false, message = "Staff not found." });

                return Json(new { success = true, data = staff });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, message = "Staff not authenticated." });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading current staff info.");
                return Json(new { success = false, message = "An error occurred while loading staff info." });
            }
        }

        [HttpGet]
        [Route("Staff/GetCurrentStaffViewModel")]
        public async Task<IActionResult> GetCurrentStaffViewModel()
        {
            try
            {
                var staffId = UserHelper.GetCurrentStaffId();
                var staffViewModel = await staffManagementService.GetStaffViewModelByIdAsync(staffId);
                if (staffViewModel == null)
                    return Json(new { success = false, message = "Staff not found." });

                return Json(new { success = true, data = staffViewModel });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, message = "Staff not authenticated." });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading current staff info.");
                return Json(new { success = false, message = "An error occurred while loading staff info." });
            }
        }

        [HttpGet]
        [Route("StaffManagement/GetStaffsByOwner")]
        public async Task<IActionResult> GetStaffsByOwner()
        {
            try
            {
                var ownerId = UserHelper.GetCurrentOwnerId();
                var staffs = await staffManagementService.GetStaffsByOwnerIdAsync(ownerId);
                return Json(new { success = true, data = staffs });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading staffs.");
                return Json(new { success = false, message = "An error occurred while loading staffs." });
            }
        }

        [HttpGet]
        [Route("StaffManagement/GetStaffById/{id:guid}")]
        public async Task<IActionResult> GetStaffById(Guid id)
        {
            try
            {
                var ownerId = UserHelper.GetCurrentOwnerId();
                var staff = await staffManagementService.GetStaffByIdAsync(id);

                if (staff == null)
                    return Json(new { success = false, message = "Staff not found." });

                if (staff.OwnerId != ownerId)
                    return Json(new { success = false, message = "Unauthorized to view this staff." });

                return Json(new { success = true, data = staff });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading staff.");
                return Json(new { success = false, message = "An error occurred while loading staff." });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}