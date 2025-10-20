namespace RestX.WebApp.Models.ViewModels
{
    public class CartViewModel
    {
        public Guid OwnerId { get; set; }
        public int TableId { get; set; }
        public string? DishListJson { get; set; }
        public Guid? OrderId { get; set; }
        public string? Message { get; set; }
        public DateTime? Time { get; set; }
        public DishCartViewModel[]? DishList { get; set; }
    }

    public class DishCartViewModel
    {
        public int DishId { get; set; }
        public string DishName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ImgUrl { get; set; }
    }
}
