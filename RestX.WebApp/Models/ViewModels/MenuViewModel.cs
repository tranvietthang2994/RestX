namespace RestX.WebApp.Models.ViewModels
{
    public class MenuViewModel
    {
        public Guid OwnerId { get; set; }
        public int TableId { get; set; }
        public List<CategoryViewModel> Categories { get; set; } = new();
    }

    public class CategoryViewModel
    {
        public int Id { get; set; }    
        public string CategoryName { get; set; } = null!;
        public List<DishViewModel> Dishes { get; set; } = new();
    }
}
