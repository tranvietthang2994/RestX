using RestX.UI.Models.ViewModels;

namespace RestX.UI.Services.Interfaces
{
    public interface IStaffService
    {
        Task<IEnumerable<TableStatusViewModel>?> GetAllTablesAsync();
        Task<CustomerRequestViewModel> GetAllCustomerRequestsAsync();
        Task<MenuViewModel> GetMenuAsync();
        Task<StaffProfileAPIViewModel?> GetStaffProfileAsync();
    }
}
