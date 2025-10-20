using AutoMapper;
using RestX.API.Data.Repository.Interfaces;
using RestX.API.Extensions;
using RestX.API.Models.Entities;
using RestX.API.Models.ViewModels;
using RestX.API.Services.Interfaces;

namespace RestX.API.Services.Implementations

{
    public class OwnerService : BaseService, IOwnerService
    {
        private readonly IMapper mapper;
        public OwnerService(IRepository repo, IMapper mapper,IHttpContextAccessor httpContextAccessor) : base(repo, httpContextAccessor)
        {
            this.mapper = mapper;
        }

        public async Task<Owner> GetOwnerByIdAsync(Guid id)
        {
            return await Repo.GetOneAsync<Owner>(o => o.Id == id, "File");
        }

        public async Task<OwnerProfileViewModel?> GetOwnerProfileViewModelAsync()
        {
            var ownerId = UserHelper.GetCurrentOwnerId();
            var owner = await Repo.GetOneAsync<Owner>(o => o.Id == ownerId, "File");
            var vm = mapper.Map<OwnerProfileViewModel>(owner);
            return vm;
        }

        public async Task<(bool Success, string? PasswordMessage)> UpdateOwnerProfileAsync(OwnerProfileViewModel vm)
        {
            vm.OwnerId = UserHelper.GetCurrentOwnerId();
            var owner = await Repo.GetOneAsync<Owner>(o => o.Id == vm.OwnerId, "File");

            var account = await Repo.GetOneAsync<Account>(a => a.OwnerId == owner.Id);

            mapper.Map(vm, owner);

            string? passwordMessage = null;
            if (!string.IsNullOrEmpty(vm.OldPassword) && !string.IsNullOrEmpty(vm.NewPassword) && !string.IsNullOrEmpty(vm.ConfirmPassword))
            {
                if (account != null && account.Password == vm.OldPassword && vm.NewPassword == vm.ConfirmPassword)
                {
                    account.Password = vm.NewPassword;
                    passwordMessage = "Password changed successfully!";
                }
                else
                {
                    passwordMessage = "Password change failed. Please check your old password and confirmation.";
                }
            }

            Repo.Update(owner);
            if (account != null) Repo.Update(account);
            await Repo.SaveAsync();
            return (true, passwordMessage);
        }
    }
}
