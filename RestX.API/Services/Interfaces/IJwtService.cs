using RestX.API.Models.DTOs.Request;
using RestX.API.Models.DTOs.Response;
using System.Security.Claims;

namespace RestX.API.Services.Interfaces
{
    public interface IJwtService
    {
        /// <summary>
        /// Generate JWT access token
        /// </summary>
        /// <param name="claims">User claims</param>
        /// <returns>JWT token</returns>
        string GenerateAccessToken(IEnumerable<Claim> claims);

        /// <summary>
        /// Generate refresh token
        /// </summary>
        /// <returns>Refresh token</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Get principal from expired token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Claims principal</returns>
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

        /// <summary>
        /// Validate JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>True if valid</returns>
        bool ValidateToken(string token);

        /// <summary>
        /// Get claims from token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Claims</returns>
        IEnumerable<Claim>? GetClaimsFromToken(string token);

        /// <summary>
        /// Create user claims for Owner
        /// </summary>
        /// <param name="ownerId">Owner ID</param>
        /// <param name="username">Username</param>
        /// <param name="name">Display name</param>
        /// <param name="email">Email</param>
        /// <returns>Claims</returns>
        IEnumerable<Claim> CreateOwnerClaims(Guid ownerId, string username, string name, string email);

        /// <summary>
        /// Create user claims for Staff
        /// </summary>
        /// <param name="staffId">Staff ID</param>
        /// <param name="ownerId">Owner ID</param>
        /// <param name="username">Username</param>
        /// <param name="name">Display name</param>
        /// <param name="email">Email</param>
        /// <returns>Claims</returns>
        IEnumerable<Claim> CreateStaffClaims(Guid staffId, Guid ownerId, string username, string name, string email);

        /// <summary>
        /// Create user claims for Customer
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="name">Display name</param>
        /// <param name="phone">Phone number</param>
        /// <returns>Claims</returns>
        IEnumerable<Claim> CreateCustomerClaims(Guid customerId, string name, string phone);
    }
}

