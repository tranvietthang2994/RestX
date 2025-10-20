using RestX.API.Models.ViewModels;

namespace RestX.API.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartViewModel> JsonToDishList(CartViewModel cart);
        Task<CartViewModel> JsonToCartViewModel(string cartJson);

    }
}
