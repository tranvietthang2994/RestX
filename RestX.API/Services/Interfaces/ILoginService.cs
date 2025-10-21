using RestX.API.Models.Entities;

namespace RestX.API.Services.Interfaces
{
    public interface ILoginService
    {
        Task<Account> GetAccountByUsernameAsync(string username, string password, CancellationToken cancellationToken);
        Task<Account?> LoginAsync(string username, string password);
    }
}
