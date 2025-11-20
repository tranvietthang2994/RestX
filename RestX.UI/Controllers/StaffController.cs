using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Controllers
{
    //[Authorize(Roles = "Owner,Staff")]
    public class StaffController : Controller
    {
        private readonly IStaffService _tableService;
        private readonly IStaffUIService _staffUIService;
        private readonly ILogger<StaffController> _logger;

        public StaffController(IStaffService tableService, IStaffUIService staffUIService,
            ILogger<StaffController> logger)
        {
            _tableService = tableService;
            _staffUIService = staffUIService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> StatusTable()
        {
            var tables = await _tableService.GetAllTablesAsync();
            Console.WriteLine($"Fetched tables: {tables?.Count()}");
            return View(tables ?? new List<TableStatusViewModel>());
        }
        [HttpGet]
        public async Task<IActionResult> CustomerRequests()
        {
            var customerRequests = await _tableService.GetAllCustomerRequestsAsync();
            return View(customerRequests);
        }

        [HttpGet]
        public async Task<IActionResult> Menu()
        {
            var menu = await _tableService.GetMenuAsync();
            return View(menu);
        }
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            try
            {

                var staffProfile = await _tableService.GetStaffProfileAsync();

                if (staffProfile == null)
                {
                    return View(new StaffProfileAPIViewModel());
                }

                return View(staffProfile);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading your profile.";
                return RedirectToAction("StatusTable");
            }
        }
        [HttpGet]
        public IActionResult PaymentReturn([FromQuery] string? orderCode, [FromQuery] string? status)
        {
            ViewBag.OrderCode = orderCode;
            ViewBag.Status = status;
            return View();
        }

        [HttpGet]
        public IActionResult PaymentCancel([FromQuery] string? orderCode, [FromQuery] string? status)
        {
            ViewBag.OrderCode = orderCode;
            ViewBag.Status = status;
            return View();
        }

        // === STAFF MANAGEMENT FUNCTIONS FOR CRUD ===

        /// <summary>
        /// Staff Management page - tương ứng với route /StaffManagement/StaffManagement
        /// </summary>
        ///         [HttpGet]
        public IActionResult PaymentReturn([FromQuery] string? orderCode, [FromQuery] string? status)
        {
            ViewBag.OrderCode = orderCode;
            ViewBag.Status = status;
            return View();
        }

        [HttpGet]
        public IActionResult PaymentCancel([FromQuery] string? orderCode, [FromQuery] string? status)
        {
            ViewBag.OrderCode = orderCode;
            ViewBag.Status = status;
            return View();
        }
        [HttpGet]
        [Route("StaffManagement/StaffManagement")]
        public async Task<IActionResult> StaffManagement()
        {
            try
            {
                _logger.LogInformation("Loading staff management page");

                var staffManagement = await _staffUIService.GetStaffManagementAsync();

                if (staffManagement == null)
                {
                    return View("Error", new ErrorViewModel
                    {
                        Message = "Unable to load staff management data"
                    });
                }

                return View("~/Views/Management/StaffManagement.cshtml", staffManagement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading staff management page");
                return View("Error", new ErrorViewModel
                {
                    Message = "An error occurred while loading staff management"
                });
            }
        }

        /// <summary>
        /// Create or Update Staff - cho JavaScript saveStaff() function
        /// </summary>
        [HttpPost]
        [Route("StaffManagement/Upsert")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> UpsertStaff([FromForm] StaffViewModel model, IFormFile? ImageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                // Handle image file if provided
                if (ImageFile != null)
                {
                    // TODO: Implement image upload logic
                    // model.ImageUrl = await UploadImageAsync(ImageFile);
                }

                // Determine if this is create or update
                if (model.Id == Guid.Empty)
                {
                    var (success, message) = await _staffUIService.CreateStaffAsync(model);
                    return Json(new { success, message = message ?? (success ? "Staff created successfully" : "Failed to create staff") });
                }
                else
                {
                    var (success, message) = await _staffUIService.UpdateStaffAsync(model);
                    return Json(new { success, message = message ?? (success ? "Staff updated successfully" : "Failed to update staff") });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting staff");
                return Json(new { success = false, message = "An error occurred while saving staff" });
            }
        }

        /// <summary>
        /// Get Staff Details - cho JavaScript viewStaff() và editStaff() functions
        /// </summary>
        [HttpGet]
        [Route("StaffManagement/Detail/{id:guid}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> StaffDetail(Guid id)
        {
            try
            {
                var staff = await _staffUIService.GetStaffByIdAsync(id);

                if (staff == null)
                {
                    return Json(new { success = false, message = "Staff not found" });
                }

                return Json(new { success = true, data = staff });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting staff details for ID: {StaffId}", id);
                return Json(new { success = false, message = "An error occurred while loading staff details" });
            }
        }
    }
}