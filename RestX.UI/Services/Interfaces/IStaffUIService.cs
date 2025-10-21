using RestX.UI.Models.ViewModels;

namespace RestX.UI.Services.Interfaces
{
    public interface IStaffUIService
    {
        /// <summary>
        /// Get all staff members for current owner
        /// </summary>
        /// <returns>List of staff members</returns>
        Task<List<StaffViewModel>> GetStaffListAsync();

        /// <summary>
        /// Get staff member by ID
        /// </summary>
        /// <param name="staffId">Staff ID</param>
        /// <returns>Staff view model</returns>
        Task<StaffViewModel?> GetStaffByIdAsync(Guid staffId);

        /// <summary>
        /// Get current staff profile
        /// </summary>
        /// <returns>Staff profile view model</returns>
        Task<StaffProfileViewModel?> GetStaffProfileAsync();

        /// <summary>
        /// Update staff profile
        /// </summary>
        /// <param name="model">Updated profile data</param>
        /// <returns>Success status and message</returns>
        Task<(bool Success, string? Message)> UpdateStaffProfileAsync(StaffProfileViewModel model);

        /// <summary>
        /// Create new staff member
        /// </summary>
        /// <param name="model">Staff data</param>
        /// <returns>Success status and message</returns>
        Task<(bool Success, string? Message)> CreateStaffAsync(StaffViewModel model);

        /// <summary>
        /// Update staff member
        /// </summary>
        /// <param name="model">Updated staff data</param>
        /// <returns>Success status and message</returns>
        Task<(bool Success, string? Message)> UpdateStaffAsync(StaffViewModel model);

        /// <summary>
        /// Delete staff member
        /// </summary>
        /// <param name="staffId">Staff ID</param>
        /// <returns>Success status</returns>
        Task<bool> DeleteStaffAsync(Guid staffId);

        /// <summary>
        /// Get staff management view model
        /// </summary>
        /// <returns>Staff management view model</returns>
        Task<StaffManagementViewModel?> GetStaffManagementAsync();
    }
}
