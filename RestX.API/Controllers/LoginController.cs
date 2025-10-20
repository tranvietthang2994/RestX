using Microsoft.AspNetCore.Mvc;
using RestX.API.Services.Interfaces;

namespace RestX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService loginService;
        private readonly IExceptionHandler exceptionHandler;

        public LoginController(
            ILoginService loginService, 
            IExceptionHandler exceptionHandler)
        {
            this.loginService = loginService;
            this.exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Đăng nhập cho Staff/Owner
        /// </summary>
        /// <param name="request">Thông tin đăng nhập</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Thông tin account và JWT token</returns>
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] AdminLoginRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest(new { success = false, message = "Username and password are required" });

                var account = await loginService.GetAccountByUsernameAsync(request.Username, request.Password, cancellationToken);

                if (account == null)
                    return Unauthorized(new { success = false, message = "Invalid username or password" });

                // Prepare response data
                var responseData = new
                {
                    success = true,
                    message = "Login successful",
                    account = new
                    {
                        id = account.Id,
                        username = account.Username,
                        role = account.Role,
                        staffId = account.StaffId,
                        ownerId = account.OwnerId,
                        staffOwnerId = account.Staff?.OwnerId // For Staff role
                    },
                    // TODO: Add JWT token here
                    // token = "jwt_token_here",
                    redirectUrl = GetRedirectUrl(account.Role)
                };

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred during admin login.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Đăng xuất Staff/Owner
        /// </summary>
        /// <returns>Kết quả đăng xuất</returns>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            try
            {
                // TODO: Invalidate JWT token
                return Ok(new { success = true, message = "Logout successful" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred during logout.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái đăng nhập
        /// </summary>
        /// <returns>Thông tin user hiện tại</returns>
        [HttpGet("status")]
        public IActionResult GetLoginStatus()
        {
            try
            {
                // TODO: Extract user info from JWT token
                return Ok(new { 
                    success = true, 
                    message = "JWT authentication not implemented yet",
                    isAuthenticated = false,
                    user = (object?)null
                });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "Error checking login status");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Refresh JWT token cho admin
        /// </summary>
        /// <returns>New JWT token</returns>
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken()
        {
            try
            {
                // TODO: Implement JWT token refresh logic
                return Ok(new { 
                    success = true, 
                    message = "JWT token refresh not implemented yet"
                });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "Error refreshing token");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        private static string GetRedirectUrl(string role)
        {
            return role switch
            {
                "Staff" => "/staff/status-table",
                "Owner" => "/owner/dashboard", 
                _ => "/home"
            };
        }
    }

    /// <summary>
    /// Request model cho admin login
    /// </summary>
    public class AdminLoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
