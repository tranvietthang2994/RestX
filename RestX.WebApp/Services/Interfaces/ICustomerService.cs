using RestX.WebApp.Models.ViewModels;

namespace RestX.WebApp.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<List<CustomerViewModel>> GetCustomersByOwnerIdAsync();
        Task<CustomerViewModel?> GetCustomerByIdAsync(Guid id);
        Task<Guid?> UpsertCustomerAsync(CustomerViewModel model);
        Task<bool> DeleteCustomerAsync(Guid id);
    }
}
