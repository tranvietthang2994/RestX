using RestX.API.Models.Entities;

namespace RestX.API.Services.Interfaces
{
    public interface IOrderDetailService
    {
        Task<List<OrderDetail>> GetOrderDetailsByOwnerIdAsync(CancellationToken cancellationToken = default);
        Task<bool> UpdateOrderDetailStatusAsync(Guid orderDetailId, bool isActive, CancellationToken cancellationToken = default);
    }
}