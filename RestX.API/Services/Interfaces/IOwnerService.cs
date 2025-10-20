using RestX.API.Models.Entities;
using RestX.API.Models.ViewModels;

namespace RestX.API.Services.Interfaces
{
    public interface IOwnerService
    {
        public Task<Owner> GetOwnerByIdAsync(Guid id);
        public Task<OwnerProfileViewModel?> GetOwnerProfileViewModelAsync();
        public Task<(bool Success, string? PasswordMessage)> UpdateOwnerProfileAsync(OwnerProfileViewModel vm);

    }
}
