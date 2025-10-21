using Microsoft.AspNetCore.Mvc;
using RestX.UI.Models.ViewModels;

namespace RestX.UI.Controllers
{
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// Handle exceptions and return appropriate error response
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="message">Custom error message</param>
        /// <returns></returns>
        protected IActionResult HandleError(Exception ex, string? message = null)
        {
            // Log the exception (this would typically use ILogger)
            var errorMessage = message ?? "An unexpected error occurred";
            
            // For AJAX requests, return JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = errorMessage });
            }
            
            // For regular requests, return error view
            return View("Error", new ErrorViewModel 
            { 
                Message = errorMessage,
                RequestId = HttpContext.TraceIdentifier
            });
        }

        /// <summary>
        /// Return success JSON response
        /// </summary>
        /// <param name="message">Success message</param>
        /// <param name="data">Optional data</param>
        /// <returns></returns>
        protected IActionResult SuccessJson(string message, object? data = null)
        {
            var response = new { success = true, message };
            
            if (data != null)
            {
                return Json(new { success = true, message, data });
            }
            
            return Json(response);
        }

        /// <summary>
        /// Return error JSON response
        /// </summary>
        /// <param name="message">Error message</param>
        /// <returns></returns>
        protected IActionResult ErrorJson(string message)
        {
            return Json(new { success = false, message });
        }

        /// <summary>
        /// Get current user ID from claims
        /// </summary>
        /// <returns></returns>
        protected Guid? GetCurrentUserId()
        {
            var userIdClaim = User?.FindFirst("UserId")?.Value ?? User?.FindFirst("Id")?.Value;
            
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            
            return null;
        }

        /// <summary>
        /// Get current user role
        /// </summary>
        /// <returns></returns>
        protected string? GetCurrentUserRole()
        {
            return User?.FindFirst("Role")?.Value ?? User?.FindFirst("role")?.Value;
        }

        /// <summary>
        /// Get current owner ID (for Owner and Staff users)
        /// </summary>
        /// <returns></returns>
        protected Guid? GetCurrentOwnerId()
        {
            var ownerIdClaim = User?.FindFirst("OwnerId")?.Value;
            
            if (Guid.TryParse(ownerIdClaim, out var ownerId))
            {
                return ownerId;
            }
            
            // If user is Owner, their ID is the owner ID
            var userRole = GetCurrentUserRole();
            if (userRole == "Owner")
            {
                return GetCurrentUserId();
            }
            
            return null;
        }

        /// <summary>
        /// Get current staff ID (for Staff users)
        /// </summary>
        /// <returns></returns>
        protected Guid? GetCurrentStaffId()
        {
            var staffIdClaim = User?.FindFirst("StaffId")?.Value;
            
            if (Guid.TryParse(staffIdClaim, out var staffId))
            {
                return staffId;
            }
            
            return null;
        }

        /// <summary>
        /// Check if current user has required role
        /// </summary>
        /// <param name="requiredRole">Required role</param>
        /// <returns></returns>
        protected bool HasRole(string requiredRole)
        {
            var userRole = GetCurrentUserRole();
            return string.Equals(userRole, requiredRole, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if current user has any of the required roles
        /// </summary>
        /// <param name="requiredRoles">Required roles</param>
        /// <returns></returns>
        protected bool HasAnyRole(params string[] requiredRoles)
        {
            var userRole = GetCurrentUserRole();
            return requiredRoles.Any(role => string.Equals(userRole, role, StringComparison.OrdinalIgnoreCase));
        }
    }
}