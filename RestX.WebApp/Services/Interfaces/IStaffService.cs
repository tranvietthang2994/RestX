using RestX.WebApp.Models;
using RestX.WebApp.Models.ViewModels;

namespace RestX.WebApp.Services.Interfaces
{
    public interface IStaffService
    {   
        public Task<StaffProfileViewModel> GetStaffProfileAsync(CancellationToken cancellationToken = default);
        public Task<Staff> GetCurrentStaff(CancellationToken cancellationToken = default);
    }
}
