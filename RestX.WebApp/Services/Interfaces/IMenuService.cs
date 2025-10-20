using RestX.WebApp.Models.ViewModels;

namespace RestX.WebApp.Services.Interfaces
{
    public interface IMenuService
    {
        Task<MenuViewModel> GetMenuViewModelAsync(CancellationToken cancellationToken = default);
    }
}
