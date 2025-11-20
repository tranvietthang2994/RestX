namespace RestX.UI.Models.ViewModels
{
    public class OwnerProfileViewModel
    {
        public Guid? OwnerId { get; set; }
        public Guid FileId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Information { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class RevenueStatisticsViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal PreviousPeriodRevenue { get; set; }
        public decimal GrowthPercentage { get; set; }
        public List<DailyRevenueViewModel> DailyRevenues { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class DailyRevenueViewModel
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class OrderStatisticsViewModel
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailyOrderViewModel> DailyOrders { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class DailyOrderViewModel
    {
        public DateTime Date { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class OverviewReportViewModel
    {
        public string Period { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal GrowthRate { get; set; }
        public List<CategoryRevenueViewModel> CategoryRevenues { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class CategoryRevenueViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class TopDishViewModel
    {
        public Guid DishId { get; set; }
        public string DishName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal Price { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}