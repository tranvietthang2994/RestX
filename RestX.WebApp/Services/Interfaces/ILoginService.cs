using RestX.WebApp.Models;

namespace RestX.WebApp.Services.Interfaces
{
    public interface ILoginService
    {
        Task<Account> GetAccountByUsernameAsync(string username, string password, CancellationToken cancellationToken);
    }
}
