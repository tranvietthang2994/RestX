using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RestX.WebApp.Helper;
using RestX.WebApp.Models;
using RestX.WebApp.Models.Home;
using RestX.WebApp.Models.ViewModels;
using RestX.WebApp.Services.Interfaces;

namespace RestX.WebApp.Services.Services
{
    public class StaffService : BaseService, IStaffService
    {
        public StaffService(IRepository repo, IHttpContextAccessor httpContextAccessor) : base(repo, httpContextAccessor)
        {
        }

        public async Task<Staff> GetCurrentStaff(CancellationToken cancellationToken = default)
        {
            var id = UserHelper.GetCurrentStaffId();
            var staff = await Repo.GetOneAsync<Staff>(s => s.Id == id, "File");

            return staff;
        }

        public async Task<StaffProfileViewModel> GetStaffProfileAsync(CancellationToken cancellationToken = default)
        {         
            var id = UserHelper.GetCurrentStaffId();

            var staff = await Repo.GetOneAsync<Staff>(s => s.Id == id, "File");
            var staffViewModel = new StaffProfileViewModel
            {
                OwnerId = staff.OwnerId,
                FileId = staff.FileId,
                Name = staff.Name,
                Email = staff.Email,
                Phone = staff.Phone,
                IsActive = staff.IsActive,
                File = staff.File,
                Owner = staff.Owner
            };

            return staffViewModel;
        }
    }
}
