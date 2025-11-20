using RestX.API.Data.Repository.Interfaces;
using RestX.API.Models.ViewModels;
using RestX.API.Services.Interfaces;
using System.Text.Json;

namespace RestX.API.Services.Implementations
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
            cart.DishList = JsonSerializer.Deserialize<List<DishCartViewModel>>(cart.DishListJson);
            return cart;
        }

        public async Task<CartViewModel> JsonToCartViewModel(string cartJson)
        {
            CartViewModel cart = JsonSerializer.Deserialize<CartViewModel>(cartJson);
            return cart;
        }
    }
}
