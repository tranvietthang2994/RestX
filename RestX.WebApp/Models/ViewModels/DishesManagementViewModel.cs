namespace RestX.WebApp.Models.ViewModels
{
    public class DishesManagementViewModel
    {
        public List<DishViewModel> Dishes { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
    }
}

