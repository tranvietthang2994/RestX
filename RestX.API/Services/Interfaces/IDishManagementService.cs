using RestX.API.Models.ViewModels;

namespace RestX.API.Services.Interfaces
{
    public interface IDishManagementService
    {
        Task<DishesManagementViewModel> GetDishesAsync();
    }
}
