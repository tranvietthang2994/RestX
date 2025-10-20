using Microsoft.AspNetCore.SignalR;
using RestX.API.Models.ViewModels;

namespace RestX.API.Hubs
{
    public class SignalrServer : Hub
    {
        // Broadcast new order to all staff clients
        public async Task SendNewOrderToStaff(OrderRequestViewModel order)
        {
            await Clients.All.SendAsync("ReceiveNewOrder", order);
        }
    }
}
