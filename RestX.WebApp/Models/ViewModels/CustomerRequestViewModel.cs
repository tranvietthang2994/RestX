namespace RestX.WebApp.Models.ViewModels
{
    public class CustomerRequestViewModel
    {
        public List<OrderRequestViewModel> Orders { get; set; } = new();
    }

    public class OrderRequestViewModel
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public int TableNumber { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public DateTime? OrderTime { get; set; }
        public bool IsActive { get; set; }
        public List<OrderDetailRequestViewModel> OrderDetails { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }

    public class OrderDetailRequestViewModel
    {
        public Guid Id { get; set; }
        public int DishId { get; set; }
        public string DishName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public decimal SubTotal => Quantity * Price;
    }
} 