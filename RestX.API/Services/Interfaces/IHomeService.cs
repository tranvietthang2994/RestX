using RestX.API.Models.ViewModels;

namespace RestX.API.Services.Interfaces
{
    public interface IHomeService
    {
        Task<HomeViewModel> GetHomeViewsAsync(CancellationToken cancellationToken = default);
    }
}
