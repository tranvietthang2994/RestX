using RestX.API.Models.DTOs.Response;
using RestX.API.Models.Entities;
using RestX.API.Models.ViewModels;

namespace RestX.API.Services.Interfaces
{
    public interface IStaffService
    {   
        public Task<StaffProfileDTO> GetStaffProfileAsync(CancellationToken cancellationToken = default);
        public Task<Staff> GetCurrentStaff(CancellationToken cancellationToken = default);
    }
}
