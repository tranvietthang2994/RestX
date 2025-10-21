using RestX.UI.Models.ApiModels;
using RestX.UI.Models.ViewModels;

namespace RestX.UI.Services.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Login for Owner/Staff
        /// </summary>
        /// <param name="request">Login request</param>
        /// <returns>Login response</returns>
        Task<LoginResponse?> LoginAsync(LoginRequest request);

        /// <summary>
        /// Login for Customer
        /// </summary>
        /// <param name="request">Customer login request</param>
        /// <returns>Login response</returns>
        Task<LoginResponse?> CustomerLoginAsync(CustomerLoginRequest request);

        /// <summary>
        /// Logout current user
        /// </summary>
        /// <returns>Success status</returns>
        Task<bool> LogoutAsync();

        /// <summary>
        /// Get current user info
        /// </summary>
        /// <returns>User info</returns>
        Task<UserInfo?> GetCurrentUserAsync();

        /// <summary>
        /// Refresh JWT token
        /// </summary>
        /// <returns>New login response</returns>
        Task<LoginResponse?> RefreshTokenAsync();

        /// <summary>
        /// Check if user is authenticated
        /// </summary>
        /// <returns>Authentication status</returns>
        bool IsAuthenticated();

        /// <summary>
        /// Get stored JWT token
        /// </summary>
        /// <returns>JWT token</returns>
        string? GetStoredToken();

        /// <summary>
        /// Store JWT token in session
        /// </summary>
        /// <param name="loginResponse">Login response with tokens</param>
        void StoreAuthTokens(LoginResponse loginResponse);

        /// <summary>
        /// Clear stored tokens
        /// </summary>
        void ClearStoredTokens();

        /// <summary>
        /// Get current user role
        /// </summary>
        /// <returns>User role</returns>
        string? GetCurrentUserRole();

        /// <summary>
        /// Get current user ID
        /// </summary>
        /// <returns>User ID</returns>
        Guid? GetCurrentUserId();

        /// <summary>
        /// Get current owner ID (for Staff and Owner)
        /// </summary>
        /// <returns>Owner ID</returns>
        Guid? GetCurrentOwnerId();
    }
}