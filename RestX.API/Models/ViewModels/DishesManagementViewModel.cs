using RestX.API.Models.DTOs.Response;

namespace RestX.API.Models.ViewModels
{
    public class DishesManagementViewModel
    {
        public List<DishViewModel> Dishes { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
    }
}

