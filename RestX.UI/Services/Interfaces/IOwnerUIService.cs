using RestX.UI.Models.ViewModels;

namespace RestX.UI.Services.Interfaces
{
    public interface IOwnerUIService
    {
        /// <summary>
        /// Get owner profile information
        /// </summary>
        /// <returns>Owner profile view model</returns>
        Task<OwnerProfileViewModel?> GetOwnerProfileAsync();

        /// <summary>
        /// Update owner profile
        /// </summary>
        /// <param name="model">Updated profile data</param>
        /// <returns>Success status and message</returns>
        Task<(bool Success, string? Message)> UpdateOwnerProfileAsync(OwnerProfileViewModel model);

        /// <summary>
        /// Get dashboard data for owner
        /// </summary>
        /// <returns>Dashboard view model</returns>
        Task<DashboardViewModel?> GetDashboardAsync();

        /// <summary>
        /// Get restaurant information by owner ID
        /// </summary>
        /// <param name="ownerId">Owner ID</param>
        /// <returns>Restaurant info</returns>
        Task<OwnerProfileViewModel?> GetRestaurantInfoAsync(Guid ownerId);

        /// <summary>
        /// Update restaurant information
        /// </summary>
        /// <param name="model">Restaurant info to update</param>
        /// <returns>Success status</returns>
        Task<bool> UpdateRestaurantInfoAsync(OwnerProfileViewModel model);
    }
}
