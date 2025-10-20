using RestX.API.Data.Repository.Interfaces;
using RestX.API.Extensions;
using RestX.API.Models.Entities;
using RestX.API.Services.Interfaces;

namespace RestX.API.Services.Implementations
{
    public class OrderDetailService : BaseService, IOrderDetailService
    {
        public OrderDetailService(IRepository repo, IHttpContextAccessor httpContextAccessor)
            : base(repo, httpContextAccessor)
        {
        }

        public async Task<List<OrderDetail>> GetOrderDetailsByOwnerIdAsync(CancellationToken cancellationToken = default)
        {
            var ownerId = UserHelper.GetCurrentOwnerId();
            var orderDetails = await Repo.GetAsync<OrderDetail>(od => od.Order.OwnerId == ownerId, includeProperties: "Order");
            return orderDetails.ToList();
        }

        public async Task<bool> UpdateOrderDetailStatusAsync(Guid orderDetailId, bool isActive, CancellationToken cancellationToken = default)
        {
            try
            {
                var ownerId = UserHelper.GetCurrentOwnerId();
                var orderDetail = await Repo.GetOneAsync<OrderDetail>(
                    od => od.Id == orderDetailId && od.Order.OwnerId == ownerId,
                    includeProperties: "Order"
                );

                if (orderDetail == null)
                    return false;

                orderDetail.IsActive = isActive;
                Repo.Update(orderDetail, UserHelper.GetCurrentStaffId().ToString());
                await Repo.SaveAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}