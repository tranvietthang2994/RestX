namespace RestX.WebApp.Models.ViewModels
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
    }
}
