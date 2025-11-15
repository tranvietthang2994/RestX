using RestX.UI.Models.ApiModels;
using RestX.UI.Services.Interfaces;
using System.Text.Json;

namespace RestX.UI.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IApiService _apiService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;

        private const string TokenKey = "JWT_TOKEN";
        private const string RefreshTokenKey = "REFRESH_TOKEN";
        private const string UserInfoKey = "USER_INFO";

        public AuthService(
            IApiService apiService, 
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthService> logger)
        {
            _apiService = apiService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Attempting login for user: {Username}", request.Username);
                
                var response = await _apiService.PostAsync<LoginRequest, LoginResponse>("api/auth/login", request);
                
                if (response?.Success == true && !string.IsNullOrEmpty(response.AccessToken))
                {
                    StoreAuthTokens(response);
                    _apiService.SetAuthToken(response.AccessToken);
                    _logger.LogInformation("Login successful for user: {Username}", request.Username);
                }
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
                return null;
            }
        }

        public async Task<LoginResponse?> CustomerLoginAsync(CustomerLoginRequest request)
        {
            try
            {
                _logger.LogInformation("Attempting customer login for: {Name} - {Phone}", request.Name, request.Phone);
                
                var response = await _apiService.PostAsync<CustomerLoginRequest, LoginResponse>("api/AuthCustomer/login", request);

                if (response == null || !response.Success)
                {
                    _logger.LogWarning("Primary customer login endpoint failed. Falling back to legacy endpoint.");
                    response = await _apiService.PostAsync<CustomerLoginRequest, LoginResponse>("api/auth/customer-login", request);
                }
                
                if (response?.Success == true)
                {
                    if (response.User == null && response.Customer != null)
                    {
                        response.User = new UserInfo
                        {
                            Id = response.Customer.Id,
                            Name = response.Customer.Name,
                            Username = response.Customer.Phone,
                            Phone = response.Customer.Phone,
                            Role = "Customer",
                            CustomerId = response.Customer.Id
                        };
                    }

                    if (!string.IsNullOrEmpty(response.AccessToken))
                    {
                        StoreAuthTokens(response);
                        _apiService.SetAuthToken(response.AccessToken);
                    }

                    _logger.LogInformation("Customer login successful for: {Name}", request.Name);
                }
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during customer login for: {Name}", request.Name);
                return null;
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                _logger.LogInformation("Attempting logout");
                
                // Call API logout endpoint
                var result = await _apiService.PostAsync<object, object>("api/auth/logout", new { });
                
                // Clear tokens regardless of API response
                ClearStoredTokens();
                _apiService.ClearAuthToken();
                
                _logger.LogInformation("Logout completed");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                
                // Still clear local tokens even if API call fails
                ClearStoredTokens();
                _apiService.ClearAuthToken();
                
                return false;
            }
        }

        public async Task<UserInfo?> GetCurrentUserAsync()
        {
            try
            {
                // First check session
                var userInfo = GetStoredUserInfo();
                if (userInfo != null)
                {
                    return userInfo;
                }

                // If not in session, call API
                var response = await _apiService.GetAsync<ApiResponse<UserInfo>>("api/auth/me");
                
                if (response?.Success == true && response.Data != null)
                {
                    // Store in session for future use
                    SetSessionValue(UserInfoKey, JsonSerializer.Serialize(response.Data));
                    return response.Data;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return null;
            }
        }

        public async Task<LoginResponse?> RefreshTokenAsync()
        {
            try
            {
                var accessToken = GetStoredToken();
                var refreshToken = GetStoredRefreshToken();
                
                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                {
                    return null;
                }

                var request = new RefreshTokenRequest
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };

                var response = await _apiService.PostAsync<RefreshTokenRequest, LoginResponse>("api/auth/refresh", request);
                
                if (response?.Success == true && !string.IsNullOrEmpty(response.AccessToken))
                {
                    StoreAuthTokens(response);
                    _apiService.SetAuthToken(response.AccessToken);
                    _logger.LogInformation("Token refresh successful");
                }
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return null;
            }
        }

        public bool IsAuthenticated()
        {
            var token = GetStoredToken();
            return !string.IsNullOrEmpty(token);
        }

        public string? GetStoredToken()
        {
            return GetSessionValue(TokenKey);
        }

        public void StoreAuthTokens(LoginResponse loginResponse)
        {
            if (!string.IsNullOrEmpty(loginResponse.AccessToken))
            {
                SetSessionValue(TokenKey, loginResponse.AccessToken);
            }

            if (!string.IsNullOrEmpty(loginResponse.RefreshToken))
            {
                SetSessionValue(RefreshTokenKey, loginResponse.RefreshToken);
            }

            if (loginResponse.User != null)
            {
                SetSessionValue(UserInfoKey, JsonSerializer.Serialize(loginResponse.User));
            }
        }

        public void ClearStoredTokens()
        {
            RemoveSessionValue(TokenKey);
            RemoveSessionValue(RefreshTokenKey);
            RemoveSessionValue(UserInfoKey);
        }

        public string? GetCurrentUserRole()
        {
            var userInfo = GetStoredUserInfo();
            return userInfo?.Role;
        }

        public Guid? GetCurrentUserId()
        {
            var userInfo = GetStoredUserInfo();
            return userInfo?.Id;
        }

        public Guid? GetCurrentOwnerId()
        {
            var userInfo = GetStoredUserInfo();
            return userInfo?.OwnerId;
        }

        #region Private Methods

        private string? GetStoredRefreshToken()
        {
            return GetSessionValue(RefreshTokenKey);
        }

        private UserInfo? GetStoredUserInfo()
        {
            var userJson = GetSessionValue(UserInfoKey);
            if (!string.IsNullOrEmpty(userJson))
            {
                try
                {
                    return JsonSerializer.Deserialize<UserInfo>(userJson);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deserializing user info from session");
                }
            }
            return null;
        }

        private string? GetSessionValue(string key)
        {
            return _httpContextAccessor.HttpContext?.Session.GetString(key);
        }

        private void SetSessionValue(string key, string value)
        {
            _httpContextAccessor.HttpContext?.Session.SetString(key, value);
        }

        private void RemoveSessionValue(string key)
        {
            _httpContextAccessor.HttpContext?.Session.Remove(key);
        }

        #endregion
    }
}
