using RestX.WebApp.Models.ViewModels;

namespace RestX.WebApp.Services.Interfaces
{
    public interface IOrderService
    {
        Task<UniversalValue<Guid>> CreatedOrder(CartViewModel model);
        Task<UniversalValue<Guid[]>> CreatedOrderDetails(CartViewModel model);
        Task<UniversalValue<Guid[]>> CreatedOrderDetails(DishCartViewModel[] modelList, Guid OrderId);
        Task<UniversalValue<Guid>> CreatedOrderDetail(DishCartViewModel model, Guid OrderId);
        Task<CustomerRequestViewModel> GetCustomerRequestsByStaffAsync(CancellationToken cancellationToken = default);
        Task<List<CartViewModel>> GetOrdersByCustomerIdOwnerIdAsync(Guid ownerId, Guid customerId);
    }
}
