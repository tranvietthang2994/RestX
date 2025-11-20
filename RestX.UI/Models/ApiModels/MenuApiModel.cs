namespace RestX.UI.Models.ApiModels
{
    public class MenuApiModel
    {
        public Guid OwnerId { get; set; }
        public int TableId { get; set; }
        public List<CategoryApiDto> Categories { get; set; } = new();
    }

    public class CategoryApiDto
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<DishApiModel> Dishes { get; set; } = new();
    }

    public class TableApiModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string QrCode { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
        public Guid OwnerId { get; set; }
    }

    public class CustomerApiModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int? Point { get; set; }
        public bool? IsActive { get; set; }
        public Guid? OwnerId { get; set; }
    }
}
