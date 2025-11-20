using RestX.API.Models.ViewModels;

namespace RestX.API.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<List<CustomerViewModel>> GetCustomersByOwnerIdAsync(Guid ownerId); // Thêm overload này
        Task<CustomerViewModel?> GetCustomerByIdAsync(Guid id);
        Task<Guid?> UpsertCustomerAsync(CustomerViewModel model);
        Task<bool> DeleteCustomerAsync(Guid id);
    }
}
