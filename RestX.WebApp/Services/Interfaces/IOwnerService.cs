using RestX.WebApp.Models;
using RestX.WebApp.Models.ViewModels;

namespace RestX.WebApp.Services.Interfaces
{
    public interface IOwnerService
    {
        public Task<Owner> GetOwnerByIdAsync(Guid id);
        public Task<OwnerProfileViewModel?> GetOwnerProfileViewModelAsync();
        public Task<(bool Success, string? PasswordMessage)> UpdateOwnerProfileAsync(OwnerProfileViewModel vm);

    }
}
