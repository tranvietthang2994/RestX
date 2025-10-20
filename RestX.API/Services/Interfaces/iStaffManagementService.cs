using RestX.API.Models.Entities;
using RestX.API.Models.ViewModels;

namespace RestX.API.Services.Interfaces
{
    public interface IStaffManagementService
    {
        Task<StaffManagementViewModel> GetStaffManagementViewModelAsync(Guid ownerId);
        Task<List<Staff>> GetStaffsByOwnerIdAsync(Guid ownerId);
        Task<Staff?> GetStaffByIdAsync(Guid id);
        Task<StaffViewModel?> GetStaffViewModelByIdAsync(Guid staffId);
        Task<Guid?> UpsertStaffAsync(StaffRequest request, Guid ownerId);
        Task<bool> DeleteStaffAsync(Guid staffId);
    }
}