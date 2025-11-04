using Microsoft.AspNetCore.Mvc;
using RestX.UI.Models.ViewModels;
using RestX.UI.Models.ApiModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Controllers
{
    public class AuthCustomerController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthCustomerController> _logger;

        public AuthCustomerController(IAuthService authService, ILogger<AuthCustomerController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Display customer login page
        /// </summary>
        /// <param name="ownerId">Restaurant owner ID</param>
        /// <param name="tableId">Table ID</param>
        /// <returns></returns>
        [HttpGet]
        [Route("AuthCustomer/Login/{ownerId:guid}/{tableId:int}")]
        public IActionResult Login(Guid ownerId, int tableId)
        {
            var model = new CustomerLoginViewModel
            {
                OwnerId = ownerId,
                TableId = tableId
            };

            return View(model);
        }

        /// <summary>
        /// Process customer login
        /// </summary>
        /// <param name="model">Customer login view model</param>
        /// <returns></returns>
        [HttpPost]
        [Route("AuthCustomer/Login/{ownerId:guid}/{tableId:int}")]
        public async Task<IActionResult> Login(CustomerLoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                _logger.LogInformation("Customer login attempt: {Name} - {Phone}", model.Name, model.Phone);

                var loginRequest = new CustomerLoginRequest
                {
                    Name = model.Name,
                    Phone = model.Phone
                };

                var response = await _authService.CustomerLoginAsync(loginRequest);

                if (response?.Success == true && response.User != null)
                {
                    // Store customer info in session
                    HttpContext.Session.SetString("CustomerId", response.User.Id.ToString());
                    HttpContext.Session.SetString("CustomerName", response.User.Name);
                    HttpContext.Session.SetString("CustomerPhone", model.Phone);

                    _logger.LogInformation("Customer login successful: {CustomerName}", model.Name);
                    _logger.LogInformation("Session CustomerId: {CustomerId}", HttpContext.Session.GetString("CustomerId"));

                    // Redirect back to home/menu
                    TempData["Message"] = $"Welcome, {response.User.Name}!";
                    return RedirectToAction("Index", "Home", new { ownerId = model.OwnerId, tableId = model.TableId });
                }
                else
                {
                    _logger.LogWarning("Customer login failed: {Name} - {Phone}", model.Name, model.Phone);
                    model.ErrorMessage = response?.Message ?? "Login failed. Please try again.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during customer login: {Name} - {Phone}", model.Name, model.Phone);
                model.ErrorMessage = "An error occurred during login. Please try again.";
                return View(model);
            }
        }

        /// <summary>
        /// Customer logout
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var customerName = HttpContext.Session.GetString("CustomerName");
                
                // Clear local session
                HttpContext.Session.Remove("CustomerId");
                HttpContext.Session.Remove("CustomerName");
                HttpContext.Session.Remove("CustomerPhone");

                // Call API logout (optional for customer)
                await _authService.LogoutAsync();

                _logger.LogInformation("Customer logout: {CustomerName}", customerName);

                TempData["Message"] = "You have been logged out successfully.";
                
                // Get redirect parameters from session if available
                var ownerIdString = HttpContext.Session.GetString("OwnerId");
                var tableIdString = HttpContext.Session.GetString("TableId");

                if (Guid.TryParse(ownerIdString, out var ownerId) && 
                    int.TryParse(tableIdString, out var tableId))
                {
                    return RedirectToAction("Index", "Home", new { ownerId, tableId });
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during customer logout");
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// AJAX customer login
        /// </summary>
        /// <param name="name">Customer name</param>
        /// <param name="phone">Customer phone</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AjaxLogin(string name, string phone)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
                {
                    return Json(new { success = false, message = "Name and phone are required" });
                }

                var loginRequest = new CustomerLoginRequest
                {
                    Name = name.Trim(),
                    Phone = phone.Trim()
                };

                var response = await _authService.CustomerLoginAsync(loginRequest);

                if (response?.Success == true && response.User != null)
                {
                    // Store customer info in session
                    HttpContext.Session.SetString("CustomerId", response.User.Id.ToString());
                    HttpContext.Session.SetString("CustomerName", response.User.Name);
                    HttpContext.Session.SetString("CustomerPhone", phone);

                    return Json(new 
                    { 
                        success = true, 
                        message = $"Welcome, {response.User.Name}!",
                        customer = new 
                        {
                            id = response.User.Id,
                            name = response.User.Name,
                            phone = phone
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = response?.Message ?? "Login failed" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AJAX customer login: {Name} - {Phone}", name, phone);
                return Json(new { success = false, message = "An error occurred during login" });
            }
        }

        /// <summary>
        /// Check if customer is logged in
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CheckLoginStatus()
        {
            try
            {
                var customerId = HttpContext.Session.GetString("CustomerId");
                var customerName = HttpContext.Session.GetString("CustomerName");
                var customerPhone = HttpContext.Session.GetString("CustomerPhone");

                if (!string.IsNullOrEmpty(customerId))
                {
                    return Json(new 
                    { 
                        success = true, 
                        isLoggedIn = true,
                        customer = new 
                        {
                            id = customerId,
                            name = customerName,
                            phone = customerPhone
                        }
                    });
                }

                return Json(new { success = true, isLoggedIn = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking customer login status");
                return Json(new { success = false, message = "Error checking login status" });
            }
        }
    }
}