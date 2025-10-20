using System.ComponentModel.DataAnnotations;

namespace RestX.WebApp.Services.DataTransferObjects
{
    public class Dish
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Dish name is required")]
        [MaxLength(100, ErrorMessage = "Dish name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        public IFormFile? ImageFile { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
