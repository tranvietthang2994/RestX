using RestX.WebApp.Models.Home;

namespace RestX.WebApp.Services.Interfaces
{
    public interface IHomeService
    {
        Task<HomeViewModel> GetHomeViewsAsync(CancellationToken cancellationToken = default);
    }
}
