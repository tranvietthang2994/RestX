using RestX.API.Models.Entities;
using RestX.API.Models.ViewModels;

namespace RestX.API.Services.Interfaces
{
    // <-- ĐÃ THAY ĐỔI TÊN INTERFACE
    public interface IAuthCustomerService
    {
        Task<Customer> LoginOrCreateAsync(LoginViewModel model, CancellationToken cancellationToken = default);
        Task<Customer?> FindCustomerByPhoneAsync(string phone, Guid ownerId, CancellationToken cancellationToken = default);
    }
}