using RestX.API.Models.DTOs.Request;
using RestX.API.Models.Entities;
using RestX.API.Models.ViewModels;

namespace RestX.API.Services.Interfaces
{
    public interface IDishService
    {
        Task<List<Dish>> GetDishesByOwnerIdAsync();
        Task<Dish> GetDishByIdAsync(int id);
        Task<DishViewModel> GetDishViewModelByIdAsync(int id);
        Task<int> UpsertDishAsync(DishRequest request);
        Task DeleteDishAsync(int id);
        Task<bool> UpdateDishAvailabilityAsync(int dishId, bool isActive); 
    }
}