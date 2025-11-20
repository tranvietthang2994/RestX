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
            if (!string.IsNullOrEmpty(cart.DishListJson))
            {
                cart.DishList = JsonSerializer.Deserialize<List<DishCartViewModel>>(cart.DishListJson) ?? new List<DishCartViewModel>();
            }
            else
            {
                cart.DishList = new List<DishCartViewModel>();
            }
            return cart;
        }

        public async Task<CartViewModel> JsonToCartViewModel(string cartJson)
        {
            CartViewModel cart = JsonSerializer.Deserialize<CartViewModel>(cartJson);
            return cart;
        }
    }
}
