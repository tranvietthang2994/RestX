using Microsoft.AspNetCore.Mvc;
using RestX.UI.Models.ViewModels;
using RestX.UI.Models.ApiModels;
using RestX.UI.Services.Interfaces;

namespace RestX.UI.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(IAuthService authService, ILogger<LoginController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Display login page for Owner/Staff
        /// </summary>
        /// <param name="returnUrl">Return URL after login</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index(string? returnUrl = null)
        {
            // If already authenticated, redirect
            if (_authService.IsAuthenticated())
            {
                var userRole = _authService.GetCurrentUserRole();
                return RedirectToRoleDashboard(userRole);
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        /// <summary>
        /// Process login for Owner/Staff
        /// </summary>
        /// <param name="model">Login view model</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                _logger.LogInformation("Login attempt for user: {Username}", model.Username);

                var loginRequest = new LoginRequest
                {
                    Username = model.Username,
                    Password = model.Password,
                    RememberMe = model.RememberMe
                };

                var response = await _authService.LoginAsync(loginRequest);

                if (response?.Success == true && response.User != null)
                {
                    _logger.LogInformation("Login successful for user: {Username}, Role: {Role}", 
                        model.Username, response.User.Role);

                    TempData["Message"] = $"Welcome back, {response.User.Name}!";

                    // Redirect based on return URL or user role
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return RedirectToRoleDashboard(response.User.Role);
                }
                else
                {
                    _logger.LogWarning("Login failed for user: {Username}", model.Username);
                    model.ErrorMessage = response?.Message ?? "Invalid username or password.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Username}", model.Username);
                model.ErrorMessage = "An error occurred during login. Please try again.";
                return View(model);
            }
        }

        /// <summary>
        /// Logout current user
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userName = _authService.GetCurrentUserRole();
                
                var success = await _authService.LogoutAsync();
                
                _logger.LogInformation("Logout completed for user role: {UserRole}", userName);
                
                TempData["Message"] = "You have been logged out successfully.";
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return RedirectToAction("Index", "Login");
            }
        }

        /// <summary>
        /// AJAX login endpoint
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="rememberMe">Remember me flag</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AjaxLogin(string username, string password, bool rememberMe = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    return Json(new { success = false, message = "Username and password are required" });
                }

                var loginRequest = new LoginRequest
                {
                    Username = username.Trim(),
                    Password = password,
                    RememberMe = rememberMe
                };

                var response = await _authService.LoginAsync(loginRequest);

                if (response?.Success == true && response.User != null)
                {
                    var redirectUrl = GetRoleDashboardUrl(response.User.Role);
                    
                    return Json(new 
                    { 
                        success = true, 
                        message = $"Welcome back, {response.User.Name}!",
                        redirectUrl = redirectUrl,
                        user = new 
                        {
                            id = response.User.Id,
                            name = response.User.Name,
                            role = response.User.Role,
                            username = response.User.Username
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = response?.Message ?? "Invalid credentials" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AJAX login for user: {Username}", username);
                return Json(new { success = false, message = "An error occurred during login" });
            }
        }

        /// <summary>
        /// Check authentication status
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> CheckAuthStatus()
        {
            try
            {
                if (_authService.IsAuthenticated())
                {
                    var user = await _authService.GetCurrentUserAsync();
                    
                    if (user != null)
                    {
                        return Json(new 
                        { 
                            success = true, 
                            isAuthenticated = true,
                            user = new 
                            {
                                id = user.Id,
                                name = user.Name,
                                role = user.Role,
                                username = user.Username
                            }
                        });
                    }
                }

                return Json(new { success = true, isAuthenticated = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking authentication status");
                return Json(new { success = false, message = "Error checking authentication status" });
            }
        }

        /// <summary>
        /// Access denied page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Private Methods

        /// <summary>
        /// Redirect to appropriate dashboard based on user role
        /// </summary>
        /// <param name="role">User role</param>
        /// <returns></returns>
        private IActionResult RedirectToRoleDashboard(string? role)
        {
            return role?.ToLower() switch
            {
                "owner" => RedirectToAction("Index", "Owner"),
                "staff" => RedirectToAction("Index", "Staff"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        /// <summary>
        /// Get dashboard URL for user role
        /// </summary>
        /// <param name="role">User role</param>
        /// <returns></returns>
        private string GetRoleDashboardUrl(string? role)
        {
            return role?.ToLower() switch
            {
                "owner" => Url.Action("Index", "Owner") ?? "/Owner",
                "staff" => Url.Action("Index", "Staff") ?? "/Staff",
                _ => Url.Action("Index", "Home") ?? "/"
            };
        }

        #endregion
    }
}