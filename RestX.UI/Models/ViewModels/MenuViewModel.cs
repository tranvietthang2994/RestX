namespace RestX.UI.Models.ViewModels
{
    public class MenuViewModel
    {
        public Guid OwnerId { get; set; }
        public int TableId { get; set; }
        public List<CategoryViewModel> Categories { get; set; } = new();
        public string? ErrorMessage { get; set; }

        // Additional properties for view compatibility
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? FileUrl { get; set; }
        public int? TableNumber { get; set; }
    }

    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<DishViewModel> Dishes { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }

    public class DishViewModel
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
