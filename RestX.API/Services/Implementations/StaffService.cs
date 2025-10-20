using RestX.API.Data.Repository.Interfaces;
using RestX.API.Extensions;
using RestX.API.Models.Entities;
using RestX.API.Models.ViewModels;
using RestX.API.Services.Interfaces;

namespace RestX.API.Services.Implementations
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
