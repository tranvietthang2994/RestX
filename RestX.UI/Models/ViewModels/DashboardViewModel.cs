namespace RestX.UI.Models.ViewModels
{
    public class DailyFinanceViewModel
    {
        public DateTime Date { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
    }

    public class DashboardViewModel
    {
        public List<DailyFinanceViewModel> DailyFinances { get; set; } = new();
        public Dictionary<string, List<DailyFinanceViewModel>> MonthlyChartData { get; set; } = new();

        public decimal TotalRevenue { get; set; }
        public decimal GrowthRate { get; set; }
        public int TotalOrders { get; set; }
        public decimal MonthlyEarnings { get; set; }

        public Dictionary<int, decimal> YearlyRevenue { get; set; } = new();
        public decimal YearlyGrowthRate { get; set; }
        public decimal CurrentMonthRevenue { get; set; }
        public decimal PreviousMonthRevenue { get; set; }
        public decimal MonthlyGrowthRate { get; set; }

        public List<decimal> MonthlyEarningsTrend { get; set; } = new();

        public List<RecentOrderViewModel> RecentOrders { get; set; } = new();
        public List<ActivityViewModel> RecentActivities { get; set; } = new();

        public string? ErrorMessage { get; set; }
    }

    public class RecentOrderViewModel
    {
        public string OrderId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
    }

    public class ActivityViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Activity { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}
