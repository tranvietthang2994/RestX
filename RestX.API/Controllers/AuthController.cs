using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestX.API.Models.DTOs.Request;
using RestX.API.Models.DTOs.Response;
using RestX.API.Services.Interfaces;

namespace RestX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly ILoginService _loginService;
        private readonly IAuthCustomerService _authCustomerService;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IJwtService jwtService,
            ILoginService loginService,
            IAuthCustomerService authCustomerService,
            IExceptionHandler exceptionHandler,
            ILogger<AuthController> logger)
        {
            _jwtService = jwtService;
            _loginService = loginService;
            _authCustomerService = authCustomerService;
            _exceptionHandler = exceptionHandler;
            _logger = logger;
        }

        /// <summary>
        /// Login for Owner/Staff
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>JWT token and user info</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new LoginResponse 
                    { 
                        Success = false, 
                        Message = "Invalid login data" 
                    });

                // Use existing login service
                var loginResult = await _loginService.LoginAsync(request.Username, request.Password);
                
                if (loginResult == null)
                {
                    return Unauthorized(new LoginResponse 
                    { 
                        Success = false, 
                        Message = "Invalid username or password" 
                    });
                }

                // Determine user type and create appropriate claims
                IEnumerable<System.Security.Claims.Claim> claims;
                UserInfo userInfo;

                if (loginResult.Owner != null)
                {
                    // Owner login
                    claims = _jwtService.CreateOwnerClaims(
                        loginResult.Owner.Id,
                        loginResult.Username,
                        loginResult.Owner.Name,
                        "owner@restaurant.com" // Default email for owner
                    );

                    userInfo = new UserInfo
                    {
                        Id = loginResult.Owner.Id,
                        Username = loginResult.Username,
                        Name = loginResult.Owner.Name,
                        Email = "owner@restaurant.com",
                        Role = "Owner",
                        OwnerId = loginResult.Owner.Id
                    };
                }
                else if (loginResult.Staff != null)
                {
                    // Staff login
                    claims = _jwtService.CreateStaffClaims(
                        loginResult.Staff.Id,
                        loginResult.Staff.OwnerId ?? Guid.Empty,
                        loginResult.Username,
                        loginResult.Staff.Name,
                        loginResult.Staff.Email
                    );

                    userInfo = new UserInfo
                    {
                        Id = loginResult.Staff.Id,
                        Username = loginResult.Username,
                        Name = loginResult.Staff.Name,
                        Email = loginResult.Staff.Email,
                        Role = "Staff",
                        OwnerId = loginResult.Staff.OwnerId,
                        StaffId = loginResult.Staff.Id
                    };
                }
                else
                {
                    return Unauthorized(new LoginResponse 
                    { 
                        Success = false, 
                        Message = "Invalid user type" 
                    });
                }

                // Generate tokens
                var accessToken = _jwtService.GenerateAccessToken(claims);
                var refreshToken = _jwtService.GenerateRefreshToken();

                var response = new LoginResponse
                {
                    Success = true,
                    Message = "Login successful",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60), // From JWT settings
                    User = userInfo
                };

                _logger.LogInformation("User {Username} logged in successfully as {Role}", request.Username, userInfo.Role);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _exceptionHandler.RaiseException(ex, "An error occurred during login");
                return StatusCode(500, new LoginResponse 
                { 
                    Success = false, 
                    Message = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// Login for Customer
        /// </summary>
        /// <param name="request">Customer login data</param>
        /// <returns>JWT token and customer info</returns>
        [HttpPost("customer-login")]
        public async Task<IActionResult> CustomerLogin([FromBody] CustomerLoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new LoginResponse 
                    { 
                        Success = false, 
                        Message = "Invalid login data" 
                    });

                // Use existing customer auth service
                var customer = await _authCustomerService.LoginOrCreateCustomerAsync(request.Name, request.Phone);
                
                if (customer == null)
                {
                    return BadRequest(new LoginResponse 
                    { 
                        Success = false, 
                        Message = "Failed to authenticate customer" 
                    });
                }

                HttpContext.Session.SetString("CustomerId", customer.Id.ToString());

                // Create customer claims
                var claims = _jwtService.CreateCustomerClaims(customer.Id, customer.Name, customer.Phone);

                var userInfo = new UserInfo
                {
                    Id = customer.Id,
                    Username = customer.Phone, // Use phone as username for customers
                    Name = customer.Name,
                    Email = "customer@restaurant.com", // Default email for customers
                    Role = "Customer",
                    CustomerId = customer.Id
                };

                // Generate tokens
                var accessToken = _jwtService.GenerateAccessToken(claims);
                var refreshToken = _jwtService.GenerateRefreshToken();

                var response = new LoginResponse
                {
                    Success = true,
                    Message = "Customer login successful",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    User = userInfo
                };

                _logger.LogInformation("Customer {Name} ({Phone}) logged in successfully", customer.Name, customer.Phone);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _exceptionHandler.RaiseException(ex, "An error occurred during customer login");
                return StatusCode(500, new LoginResponse 
                { 
                    Success = false, 
                    Message = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// Refresh JWT token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>New JWT tokens</returns>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new LoginResponse 
                    { 
                        Success = false, 
                        Message = "Invalid refresh token data" 
                    });

                var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
                if (principal == null)
                {
                    return Unauthorized(new LoginResponse 
                    { 
                        Success = false, 
                        Message = "Invalid access token" 
                    });
                }

                // TODO: Validate refresh token against stored tokens in database
                // For now, we'll generate new tokens

                var newAccessToken = _jwtService.GenerateAccessToken(principal.Claims);
                var newRefreshToken = _jwtService.GenerateRefreshToken();

                var response = new LoginResponse
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _exceptionHandler.RaiseException(ex, "An error occurred during token refresh");
                return StatusCode(500, new LoginResponse 
                { 
                    Success = false, 
                    Message = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// Logout (invalidate token)
        /// </summary>
        /// <returns>Logout result</returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // TODO: Add token to blacklist in database
                // For now, just return success (client should discard token)
                
                var username = User.Identity?.Name ?? "Unknown";
                _logger.LogInformation("User {Username} logged out", username);

                return Ok(new { success = true, message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _exceptionHandler.RaiseException(ex, "An error occurred during logout");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get current user info from JWT token
        /// </summary>
        /// <returns>Current user information</returns>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userInfo = new UserInfo
                {
                    Id = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ""),
                    Username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "",
                    Name = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "",
                    Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "",
                    Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "",
                    OwnerId = Guid.TryParse(User.FindFirst("OwnerId")?.Value, out var ownerId) ? ownerId : null,
                    StaffId = Guid.TryParse(User.FindFirst("StaffId")?.Value, out var staffId) ? staffId : null,
                    CustomerId = Guid.TryParse(User.FindFirst("CustomerId")?.Value, out var customerId) ? customerId : null
                };

                return Ok(new { success = true, data = userInfo });
            }
            catch (Exception ex)
            {
                _exceptionHandler.RaiseException(ex, "An error occurred while getting current user");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Validate JWT token
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Token))
                    return BadRequest(new { success = false, message = "Token is required" });

                var isValid = _jwtService.ValidateToken(request.Token);
                var claims = isValid ? _jwtService.GetClaimsFromToken(request.Token) : null;

                return Ok(new 
                { 
                    success = true, 
                    isValid = isValid,
                    claims = claims?.Select(c => new { c.Type, c.Value })
                });
            }
            catch (Exception ex)
            {
                _exceptionHandler.RaiseException(ex, "An error occurred during token validation");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }

    /// <summary>
    /// Customer login request
    /// </summary>
    public class CustomerLoginRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    /// <summary>
    /// Token validation request
    /// </summary>
    public class ValidateTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}
