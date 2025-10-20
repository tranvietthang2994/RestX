using RestX.API.Data.Repository.Interfaces;
using RestX.API.Models.Entities;
using RestX.API.Services.Interfaces;

namespace RestX.API.Services.Implementations
{
    public class LoginService : BaseService, ILoginService
    {
        public LoginService(IRepository repo, IHttpContextAccessor httpContextAccessor) : base(repo, httpContextAccessor)
        {
        }

        public Task<Account> GetAccountByUsernameAsync(string username, string password, CancellationToken cancellationToken)
        {
            return Repo.GetFirstAsync<Account>(
                filter: acc => acc.Username == username && acc.Password == password,
                includeProperties: "Staff,Owner"
                );
        }
    }
}
