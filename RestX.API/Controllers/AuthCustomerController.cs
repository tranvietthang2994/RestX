using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestX.API.Models.DTOs.Response;
using RestX.API.Models.ViewModels;
using RestX.API.Services.Implementations;
using RestX.API.Services.Interfaces;

namespace RestX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthCustomerController : ControllerBase
    {
        private readonly IAuthCustomerService authCustomerService;
        private readonly IExceptionHandler exceptionHandler;
        private readonly IJwtService _jwtService;

        public AuthCustomerController(
            IAuthCustomerService authCustomerService,
            IExceptionHandler exceptionHandler,
            IJwtService jwtService)
        {
            this.authCustomerService = authCustomerService;
            this.exceptionHandler = exceptionHandler;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Đăng nhập hoặc tạo mới customer
        /// </summary>
        /// <param name="model">Thông tin đăng nhập</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Thông tin customer và access token</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var authCustomer = await authCustomerService.LoginOrCreateAsync(model, cancellationToken);

                if (authCustomer == null)
                    return Unauthorized(new { success = false, message = "Login failed" });

                IEnumerable<System.Security.Claims.Claim> claims;
                UserInfo userInfo;

                HttpContext.Session.SetString("CustomerId", authCustomer.Id.ToString());


                claims = _jwtService.CreateCustomerClaims(
                    authCustomer.Id,
                    authCustomer.Name,
                    authCustomer.Phone
                    
                );

                var accessToken = _jwtService.GenerateAccessToken(claims);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // TODO: Generate JWT token instead of session
                var response = new
                {
                    success = true,
                    message = "Login successful",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    customer = new
                    {
                        id = authCustomer.Id,
                        name = authCustomer.Name,
                        phone = authCustomer.Phone,
                        point = authCustomer.Point
                    },
                    // TODO: Add JWT token here
                    // token = "jwt_token_here"
                };

                return Ok(response);
            }
            catch (ArgumentException argEx)
            {
                exceptionHandler.RaiseException(argEx, $"Invalid login data for OwnerId: {model.OwnerId}");
                return BadRequest(new { success = false, message = argEx.Message });
            }
            catch (Exception ex)
            {
                exceptionHandler.RaiseException(ex, $"Login error for OwnerId: {model.OwnerId}");
                return StatusCode(500, new { success = false, message = "An error occurred during login" });
            }
        }

        /// <summary>
        /// Đăng xuất customer
        /// </summary>
        /// <returns>Kết quả đăng xuất</returns>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            try
            {
                // TODO: Invalidate JWT token or handle logout logic
                return Ok(new { success = true, message = "Logout successful" });
            }
            catch (Exception ex)
            {
                exceptionHandler.RaiseException(ex, "Logout error");
                return StatusCode(500, new { success = false, message = "An error occurred during logout" });
            }
        }

        /// <summary>
        /// Kiểm tra số điện thoại đã tồn tại hay chưa
        /// </summary>
        /// <param name="ownerId">ID chủ nhà hàng</param>
        /// <param name="phone">Số điện thoại cần kiểm tra</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Thông tin customer nếu tồn tại</returns>
        [HttpGet("check-phone/{ownerId:guid}")]
        public async Task<IActionResult> CheckPhone(Guid ownerId, [FromQuery] string phone, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phone))
                    return Ok(new { exists = false, name = "" });

                var customer = await authCustomerService.FindCustomerByPhoneAsync(phone, ownerId, cancellationToken);

                return Ok(new
                {
                    exists = customer != null,
                    name = customer?.Name ?? "",
                    customer = customer != null ? new
                    {
                        id = customer.Id,
                        name = customer.Name,
                        phone = customer.Phone,
                        point = customer.Point
                    } : null
                });
            }
            catch (Exception ex)
            {
                exceptionHandler.RaiseException(ex, $"Error checking phone for OwnerId: {ownerId}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy thông tin customer hiện tại (cần JWT token)
        /// </summary>
        /// <returns>Thông tin customer</returns>
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentCustomer()
        {
            try
            {
                // TODO: Extract customer ID from JWT token
                // For now, return placeholder
                return Ok(new { 
                    success = true, 
                    message = "JWT authentication not implemented yet",
                    customer = (object?)null
                });
            }
            catch (Exception ex)
            {
                exceptionHandler.RaiseException(ex, "Error getting current customer");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Refresh JWT token
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
                exceptionHandler.RaiseException(ex, "Error refreshing token");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }
}
