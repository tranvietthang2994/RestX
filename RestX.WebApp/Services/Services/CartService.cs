using RestX.WebApp.Models;
using RestX.WebApp.Models.ViewModels;
using RestX.WebApp.Services.Interfaces;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace RestX.WebApp.Services.Services
{
    public class CartService : BaseService, ICartService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        public CartService(IRepository repo, IHttpContextAccessor httpContextAccessor) : base(repo, httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<CartViewModel> JsonToDishList(CartViewModel cart)
        {
            cart.DishList = JsonSerializer.Deserialize<DishCartViewModel[]>(cart.DishListJson);
            return cart;
        }

        public async Task<CartViewModel> JsonToCartViewModel(string cartJson)
        {
            CartViewModel cart = JsonSerializer.Deserialize<CartViewModel>(cartJson);
            return cart;
        }
    }
}
