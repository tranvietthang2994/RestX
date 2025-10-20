using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestX.WebApp.Models;

namespace RestX.WebApp.Services.Interfaces
{
    public interface IOrderDetailService
    {
        Task<List<OrderDetail>> GetOrderDetailsByOwnerIdAsync(CancellationToken cancellationToken = default);
        Task<bool> UpdateOrderDetailStatusAsync(Guid orderDetailId, bool isActive, CancellationToken cancellationToken = default);
    }
}