using RestX.API.Models.Entities;

namespace RestX.API.Models.ViewModels
{
    public class DishesManagementViewModel
    {
        public List<DishViewModel> Dishes { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
    }
}

