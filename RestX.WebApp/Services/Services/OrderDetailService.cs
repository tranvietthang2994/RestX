using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RestX.WebApp.Models;
using RestX.WebApp.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using RestX.WebApp.Helper;

namespace RestX.WebApp.Services.Services
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