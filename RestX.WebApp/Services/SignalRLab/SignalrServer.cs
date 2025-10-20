using Microsoft.AspNetCore.SignalR;
using RestX.WebApp.Models.ViewModels;
using System.Threading.Tasks;

namespace RestX.WebApp.Services.SignalRLab
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
