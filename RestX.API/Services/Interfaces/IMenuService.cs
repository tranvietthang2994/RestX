using RestX.API.Models.ViewModels;

namespace RestX.API.Services.Interfaces
{
    public interface IMenuService
    {
        Task<MenuViewModel> GetMenuViewModelAsync(CancellationToken cancellationToken = default);
    }
}
