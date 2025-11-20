using RestX.API.Models.ViewModels;

namespace RestX.API.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<List<CustomerViewModel>> GetCustomersByOwnerIdAsync();
        Task<CustomerViewModel?> GetCustomerByIdAsync(Guid id);
        Task<Guid?> UpsertCustomerAsync(CustomerViewModel model);
        Task<bool> DeleteCustomerAsync(Guid id);
    }
}
