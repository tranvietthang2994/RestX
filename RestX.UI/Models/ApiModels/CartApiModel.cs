namespace RestX.UI.Models.ApiModels
{
    public class CartApiModel
    {
        public Guid OwnerId { get; set; }
        public int TableId { get; set; }
        public string? DishListJson { get; set; }
        public Guid? OrderId { get; set; }
        public string? Message { get; set; }
        public DateTime? Time { get; set; }
        public DishCartApiModel[]? DishList { get; set; }
    }

    public class DishCartApiModel
    {
        public int DishId { get; set; }
        public string DishName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ImgUrl { get; set; } = string.Empty;
    }

    public class OrderApiModel
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int TableId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public List<OrderDetailApiModel> OrderDetails { get; set; } = new();
    }

    public class OrderDetailApiModel
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public int DishId { get; set; }
        public string DishName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
