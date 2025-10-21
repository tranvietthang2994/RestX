namespace RestX.UI.Models.ViewModels
{
    public class CartViewModel
    {
        public Guid OwnerId { get; set; }
        public int TableId { get; set; }
        public string? DishListJson { get; set; }
        public Guid? OrderId { get; set; }
        public string? Message { get; set; }
        public DateTime? Time { get; set; }
        public List<DishCartViewModel> DishList { get; set; } = new();
        public decimal TotalAmount => DishList.Sum(d => d.Price * d.Quantity);
        public string? ErrorMessage { get; set; }
    }

    public class DishCartViewModel
    {
        public int DishId { get; set; }
        public string DishName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ImgUrl { get; set; } = string.Empty;
        public decimal SubTotal => Price * Quantity;
    }
}
