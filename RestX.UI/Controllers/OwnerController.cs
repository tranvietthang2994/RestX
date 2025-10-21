using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestX.UI.Models.ViewModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Controllers
{
    [Authorize(Roles = "Owner")]
    public class OwnerController : Controller
    {
        private readonly IOwnerUIService _ownerService;
        private readonly ILogger<OwnerController> _logger;

        public OwnerController(
            IOwnerUIService ownerService,
            ILogger<OwnerController> logger)
        {
            _ownerService = ownerService;
            _logger = logger;
        }

        /// <summary>
        /// Owner profile page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading owner profile page");
                
                var profile = await _ownerService.GetOwnerProfileAsync();
                
                if (profile == null)
                {
                    return View("Error", new ErrorViewModel 
                    { 
                        Message = "Unable to load owner profile",
                        StatusCode = 404
                    });
                }

                return View(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading owner profile page");
                return View("Error", new ErrorViewModel 
                { 
                    Message = "An error occurred while loading the profile"
                });
            }
        }

        /// <summary>
        /// Update owner profile
        /// </summary>
        /// <param name="model">Owner profile data</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Index(OwnerProfileViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var (success, message) = await _ownerService.UpdateOwnerProfileAsync(model);
                
                if (success)
                {
                    TempData["SuccessMessage"] = message ?? "Profile updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    model.ErrorMessage = message ?? "Update failed!";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating owner profile");
                model.ErrorMessage = "An error occurred while updating the profile";
                return View(model);
            }
        }

        /// <summary>
        /// AJAX update owner profile
        /// </summary>
        /// <param name="model">Owner profile data</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(OwnerProfileViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var (success, message) = await _ownerService.UpdateOwnerProfileAsync(model);
                
                return Json(new { success, message = message ?? (success ? "Profile updated successfully!" : "Update failed!") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating owner profile via AJAX");
                return Json(new { success = false, message = "An error occurred while updating the profile" });
            }
        }

        /// <summary>
        /// Dashboard page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                _logger.LogInformation("Loading owner dashboard");
                
                var dashboard = await _ownerService.GetDashboardAsync();
                
                if (dashboard == null)
                {
                    return View("Error", new ErrorViewModel 
                    { 
                        Message = "Unable to load dashboard data",
                        StatusCode = 500
                    });
                }

                if (!string.IsNullOrEmpty(dashboard.ErrorMessage))
                {
                    return View("Error", new ErrorViewModel 
                    { 
                        Message = dashboard.ErrorMessage
                    });
                }

                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading owner dashboard");
                return View("Error", new ErrorViewModel 
                { 
                    Message = "An error occurred while loading the dashboard"
                });
            }
        }

        /// <summary>
        /// Get dashboard data as JSON for AJAX
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var dashboard = await _ownerService.GetDashboardAsync();
                
                if (dashboard != null && string.IsNullOrEmpty(dashboard.ErrorMessage))
                {
                    return Json(new { success = true, data = dashboard });
                }
                
                return Json(new { success = false, message = dashboard?.ErrorMessage ?? "Failed to load dashboard data" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard data via AJAX");
                return Json(new { success = false, message = "An error occurred while loading dashboard data" });
            }
        }

        /// <summary>
        /// Restaurant information page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> RestaurantInfo()
        {
            try
            {
                var profile = await _ownerService.GetOwnerProfileAsync();
                
                if (profile == null)
                {
                    return View("Error", new ErrorViewModel 
                    { 
                        Message = "Unable to load restaurant information"
                    });
                }

                return View(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading restaurant info page");
                return View("Error", new ErrorViewModel 
                { 
                    Message = "An error occurred while loading restaurant information"
                });
            }
        }

        /// <summary>
        /// Update restaurant information
        /// </summary>
        /// <param name="model">Restaurant info</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateRestaurantInfo(OwnerProfileViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid data provided" });
                }

                var success = await _ownerService.UpdateRestaurantInfoAsync(model);
                
                if (success)
                {
                    return Json(new { success = true, message = "Restaurant information updated successfully" });
                }
                
                return Json(new { success = false, message = "Failed to update restaurant information" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating restaurant info");
                return Json(new { success = false, message = "An error occurred while updating restaurant information" });
            }
        }
    }
}