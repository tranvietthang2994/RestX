using RestX.WebApp.Models;

using RestX.WebApp.Models.ViewModels;
using System.Threading.Tasks;

namespace RestX.WebApp.Services.Interfaces
{
    // <-- ĐÃ THAY ĐỔI TÊN INTERFACE
    public interface IAuthCustomerService
    {
        Task<Customer> LoginOrCreateAsync(LoginViewModel model, CancellationToken cancellationToken = default);
        Task<Customer?> FindCustomerByPhoneAsync(string phone, Guid ownerId, CancellationToken cancellationToken = default);
    }
}