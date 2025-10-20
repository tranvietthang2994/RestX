using RestX.WebApp.Models.ViewModels;

namespace RestX.WebApp.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartViewModel> JsonToDishList(CartViewModel cart);
        Task<CartViewModel> JsonToCartViewModel(string cartJson);

    }
}
