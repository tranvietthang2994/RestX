using System.ComponentModel.DataAnnotations;

namespace RestX.UI.Models.ViewModels
{
    public class DishesManagementViewModel
    {
        public List<DishViewModel> Dishes { get; set; } = new();
        public List<CategoryViewModel> Categories { get; set; } = new();
        public DishViewModel NewDish { get; set; } = new();
        public CategoryViewModel NewCategory { get; set; } = new();
        
        // Statistics
        public int TotalDishes { get; set; }
        public int ActiveDishes { get; set; }
        public int TotalCategories { get; set; }
        public int ActiveCategories { get; set; }
        
        // Filters
        public string? SearchTerm { get; set; }
        public int? CategoryFilter { get; set; }
        public bool? StatusFilter { get; set; }
        
        public string? ErrorMessage { get; set; }
    }

    public class CategoryManagementViewModel
    {
        public List<CategoryViewModel> Categories { get; set; } = new();
        public CategoryViewModel NewCategory { get; set; } = new();
        public int TotalCategories { get; set; }
        public int ActiveCategories { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
