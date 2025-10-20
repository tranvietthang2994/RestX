using Microsoft.AspNetCore.Http;
using RestX.WebApp.Helper;
using RestX.WebApp.Models;
using RestX.WebApp.Models.ViewModels;
using RestX.WebApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestX.WebApp.Services.Services
{
    public class DashboardService : BaseService, IDashboardService
    {
        private readonly IOrderDetailService orderDetailService;
        private readonly IIngredientImportService ingredientImportService;

        public DashboardService(
            IRepository repo,
            IHttpContextAccessor httpContextAccessor,
            IOrderDetailService orderDetailService,
            IIngredientImportService ingredientImportService
        ) : base(repo, httpContextAccessor)
        {
            this.orderDetailService = orderDetailService;
            this.ingredientImportService = ingredientImportService;
        }

        public async Task<Dictionary<DateTime, decimal>> GetCostByDateAsync(CancellationToken cancellationToken = default)
        {
            var imports = await ingredientImportService.GetIngredientImportsByOwnerIdAsync(cancellationToken);
            return imports
                .Where(i => i.Time.HasValue)
                .GroupBy(i => i.Time.Value.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(i => i.TotalCost)
                );
        }

        public async Task<Dictionary<DateTime, decimal>> GetProfitByDateAsync(CancellationToken cancellationToken = default)
        {
            var orderDetails = await orderDetailService.GetOrderDetailsByOwnerIdAsync(cancellationToken);
            return orderDetails
                .Where(od => od.Order != null && od.Order.Time.HasValue)
                .GroupBy(od => od.Order.Time.Value.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(od => od.Quantity * od.Price)
                );
        }

        public async Task<decimal> GetTodayRevenueAsync(CancellationToken cancellationToken = default)
        {
            var orderDetails = await orderDetailService.GetOrderDetailsByOwnerIdAsync(cancellationToken);
            var today = DateTime.Today;
            return orderDetails
                .Where(od => od.Order != null && od.Order.Time.HasValue && od.Order.Time.Value.Date == today)
                .Sum(od => od.Quantity * od.Price);
        }

        public async Task<int> GetTotalOrdersAsync(CancellationToken cancellationToken = default)
        {
            var orderDetails = await orderDetailService.GetOrderDetailsByOwnerIdAsync(cancellationToken);
            return orderDetails
                .Where(od => od.Order != null)
                .Select(od => od.Order.Id)
                .Distinct()
                .Count();
        }

        public async Task<decimal> GetMonthlyEarningsAsync(CancellationToken cancellationToken = default)
        {
            var orderDetails = await orderDetailService.GetOrderDetailsByOwnerIdAsync(cancellationToken);
            var now = DateTime.Now;
            return orderDetails
                .Where(od => od.Order != null && od.Order.Time.HasValue &&
                             od.Order.Time.Value.Year == now.Year &&
                             od.Order.Time.Value.Month == now.Month)
                .Sum(od => od.Quantity * od.Price);
        }

        public async Task<decimal> GetGrowthRateAsync(CancellationToken cancellationToken = default)
        {
            var orderDetails = await orderDetailService.GetOrderDetailsByOwnerIdAsync(cancellationToken);
            var now = DateTime.Now;
            var lastMonth = now.AddMonths(-1);

            var currentMonthEarnings = orderDetails
                .Where(od => od.Order != null && od.Order.Time.HasValue &&
                             od.Order.Time.Value.Year == now.Year &&
                             od.Order.Time.Value.Month == now.Month)
                .Sum(od => od.Quantity * od.Price);

            var lastMonthEarnings = orderDetails
                .Where(od => od.Order != null && od.Order.Time.HasValue &&
                             od.Order.Time.Value.Year == lastMonth.Year &&
                             od.Order.Time.Value.Month == lastMonth.Month)
                .Sum(od => od.Quantity * od.Price);

            if (lastMonthEarnings == 0) return 0;
            return ((currentMonthEarnings - lastMonthEarnings) / lastMonthEarnings) * 100;
        }

        public async Task<Dictionary<string, List<DailyFinanceViewModel>>> GetMonthlyProfitDataAsync(CancellationToken cancellationToken = default)
        {
            var orderDetails = await orderDetailService.GetOrderDetailsByOwnerIdAsync(cancellationToken);
            var now = DateTime.Now;
            var monthlyData = new Dictionary<string, List<DailyFinanceViewModel>>();

            // Lấy data cho 4 tháng gần nhất
            for (int i = 0; i < 4; i++)
            {
                var targetMonth = now.AddMonths(-i);
                var monthKey = targetMonth.ToString("yyyy-MM"); // Format: "2025-06"

                var monthlyProfits = orderDetails
                    .Where(od => od.Order != null && od.Order.Time.HasValue &&
                                 od.Order.Time.Value.Year == targetMonth.Year &&
                                 od.Order.Time.Value.Month == targetMonth.Month)
                    .GroupBy(od => od.Order.Time.Value.Date)
                    .Select(g => new DailyFinanceViewModel
                    {
                        Date = g.Key,
                        Profit = g.Sum(od => od.Quantity * od.Price)
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                monthlyData[monthKey] = monthlyProfits;
            }

            return monthlyData;
        }

        // Thêm method để lấy dữ liệu cho Yearly Revenue (Breakup chart)
        public async Task<Dictionary<int, decimal>> GetYearlyRevenueAsync(CancellationToken cancellationToken = default)
        {
            var orderDetails = await orderDetailService.GetOrderDetailsByOwnerIdAsync(cancellationToken);
            var now = DateTime.Now;
            var yearlyRevenue = new Dictionary<int, decimal>();

            // Lấy dữ liệu cho 3 năm gần nhất
            for (int i = 0; i < 3; i++)
            {
                var targetYear = now.Year - i;
                var yearRevenue = orderDetails
                    .Where(od => od.Order != null && od.Order.Time.HasValue &&
                                 od.Order.Time.Value.Year == targetYear)
                    .Sum(od => od.Quantity * od.Price);

                if (yearRevenue > 0) // Chỉ thêm năm có doanh thu
                {
                    yearlyRevenue[targetYear] = yearRevenue;
                }
            }

            return yearlyRevenue;
        }

        public async Task<decimal> GetYearlyGrowthRateAsync(CancellationToken cancellationToken = default)
        {
            var orderDetails = await orderDetailService.GetOrderDetailsByOwnerIdAsync(cancellationToken);
            var now = DateTime.Now;

            var currentYearEarnings = orderDetails
                .Where(od => od.Order != null && od.Order.Time.HasValue &&
                             od.Order.Time.Value.Year == now.Year)
                .Sum(od => od.Quantity * od.Price);

            var lastYearEarnings = orderDetails
                .Where(od => od.Order != null && od.Order.Time.HasValue &&
                             od.Order.Time.Value.Year == now.Year - 1)
                .Sum(od => od.Quantity * od.Price);

            if (lastYearEarnings == 0) return 0;
            return ((currentYearEarnings - lastYearEarnings) / lastYearEarnings) * 100;
        }

        public async Task<List<decimal>> GetMonthlyEarningsTrendAsync(CancellationToken cancellationToken = default)
        {
            var orderDetails = await orderDetailService.GetOrderDetailsByOwnerIdAsync(cancellationToken);
            var now = DateTime.Now;
            var monthlyTrend = new List<decimal>();

            for (int i = 6; i >= 0; i--)
            {
                var targetMonth = now.AddMonths(-i);
                var monthEarnings = orderDetails
                    .Where(od => od.Order != null && od.Order.Time.HasValue &&
                                 od.Order.Time.Value.Year == targetMonth.Year &&
                                 od.Order.Time.Value.Month == targetMonth.Month)
                    .Sum(od => od.Quantity * od.Price);

                monthlyTrend.Add(monthEarnings);
            }

            return monthlyTrend;
        }

        public async Task<(decimal currentMonth, decimal previousMonth)> GetCurrentAndPreviousMonthRevenueAsync(CancellationToken cancellationToken = default)
        {
            var orderDetails = await orderDetailService.GetOrderDetailsByOwnerIdAsync(cancellationToken);
            var now = DateTime.Now;
            var lastMonth = now.AddMonths(-1);

            var currentMonthRevenue = orderDetails
                .Where(od => od.Order != null && od.Order.Time.HasValue &&
                             od.Order.Time.Value.Year == now.Year &&
                             od.Order.Time.Value.Month == now.Month)
                .Sum(od => od.Quantity * od.Price);

            var previousMonthRevenue = orderDetails
                .Where(od => od.Order != null && od.Order.Time.HasValue &&
                             od.Order.Time.Value.Year == lastMonth.Year &&
                             od.Order.Time.Value.Month == lastMonth.Month)
                .Sum(od => od.Quantity * od.Price);

            return (currentMonthRevenue, previousMonthRevenue);
        }

        public async Task<decimal> GetMonthlyGrowthRateAsync(CancellationToken cancellationToken = default)
        {
            var (currentMonth, previousMonth) = await GetCurrentAndPreviousMonthRevenueAsync(cancellationToken);

            if (previousMonth == 0) return 0;
            return ((currentMonth - previousMonth) / previousMonth) * 100;
        }

        // NEW: Get Recent Orders - Remove count limit, get all today's orders
        public async Task<List<RecentOrderViewModel>> GetRecentOrdersAsync(CancellationToken cancellationToken = default)
        {
            var ownerId = UserHelper.GetCurrentOwnerId();
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var orders = await Repo.GetAsync<Order>(
                filter: o => o.OwnerId == ownerId &&
                           o.IsActive == true &&
                           o.Time.HasValue &&
                           o.Time.Value >= today &&
                           o.Time.Value < tomorrow,
                orderBy: o => o.OrderByDescending(x => x.Time),
                includeProperties: "Customer,Table,OrderStatus,OrderDetails.Dish"
            // Removed take parameter to get all today's orders
            );

            return orders.Select(order => new RecentOrderViewModel
            {
                OrderId = $"#{order.Id.ToString().Substring(0, 8).ToUpper()}",
                CustomerName = order.Customer?.Name ?? "Guest",
                TableName = $"Table {order.Table?.TableNumber ?? 0}",
                Dishes = order.OrderDetails?.Select(od => new DishViewModel
                {
                    Id = od.Dish?.Id ?? 0,
                    Name = od.Dish?.Name ?? "Unknown",
                    Price = od.Price
                }).ToList() ?? new List<DishViewModel>(),
                Status = order.OrderStatus?.Name ?? "Unknown",
                Amount = order.OrderDetails?.Sum(od => od.Quantity * od.Price) ?? 0
            }).ToList();
        }

        public async Task<List<ActivityViewModel>> GetRecentActivitiesAsync(CancellationToken cancellationToken = default)
        {
            var ownerId = UserHelper.GetCurrentOwnerId();
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var recentOrders = await Repo.GetAsync<Order>(
                filter: o => o.OwnerId == ownerId &&
                           o.IsActive == true &&
                           o.Time.HasValue &&
                           o.Time.Value >= today &&
                           o.Time.Value < tomorrow,
                orderBy: o => o.OrderByDescending(x => x.Time),
                includeProperties: "Customer,Table,OrderDetails.Dish,Payments.PaymentMethod"
            );

            var activities = new List<ActivityViewModel>();

            foreach (var order in recentOrders)
            {
                // Calculate total amount for this order
                var totalAmount = order.OrderDetails?.Sum(od => od.Quantity * od.Price) ?? 0;

                // Check if order is paid - has active payments covering the total amount
                var totalPaid = order.Payments?.Where(p => p.IsActive == true).Sum(p => p.Cost) ?? 0;
                var isPaid = totalPaid >= totalAmount && totalAmount > 0;

                // Add order activity with dishes
                activities.Add(new ActivityViewModel
                {
                    Time = order.Time ?? DateTime.Now,
                    TableName = $"Table {order.Table?.TableNumber ?? 0}",
                    CustomerName = order.Customer?.Name ?? "Guest",
                    IsPaid = isPaid,
                    TotalAmount = totalAmount,
                    Dishes = order.OrderDetails?.Select(od => new DishViewModel
                    {
                        Id = od.Dish?.Id ?? 0,
                        Name = od.Dish?.Name ?? "Unknown",
                        Price = od.Price
                    }).ToList() ?? new List<DishViewModel>()
                });
            }

            return activities
                .OrderByDescending(a => a.Time)
                .ToList();
        }
        public async Task<DashboardViewModel> GetDashboardViewModelAsync(CancellationToken cancellationToken = default)
        {
            var profitByDate = await GetProfitByDateAsync(cancellationToken);
            var monthlyChartData = await GetMonthlyProfitDataAsync(cancellationToken);

            var dailyFinances = profitByDate.OrderBy(x => x.Key)
                .Select(x => new DailyFinanceViewModel
                {
                    Date = x.Key,
                    Profit = x.Value
                }).ToList();

            var totalRevenue = await GetTodayRevenueAsync(cancellationToken);
            var growthRate = await GetGrowthRateAsync(cancellationToken);
            var totalOrders = await GetTotalOrdersAsync(cancellationToken);
            var monthlyEarnings = await GetMonthlyEarningsAsync(cancellationToken);

            var yearlyRevenue = await GetYearlyRevenueAsync(cancellationToken);
            var yearlyGrowthRate = await GetYearlyGrowthRateAsync(cancellationToken);
            var (currentMonthRevenue, previousMonthRevenue) = await GetCurrentAndPreviousMonthRevenueAsync(cancellationToken);
            var monthlyGrowthRate = await GetMonthlyGrowthRateAsync(cancellationToken);
            var monthlyEarningsTrend = await GetMonthlyEarningsTrendAsync(cancellationToken);

            // Removed count parameters - get all today's data
            var recentOrders = await GetRecentOrdersAsync(cancellationToken);
            var recentActivities = await GetRecentActivitiesAsync(cancellationToken);

            return new DashboardViewModel
            {
                DailyFinances = dailyFinances,
                MonthlyChartData = monthlyChartData,
                TotalRevenue = totalRevenue,
                GrowthRate = growthRate,
                TotalOrders = totalOrders,
                MonthlyEarnings = monthlyEarnings,
                YearlyRevenue = yearlyRevenue,
                YearlyGrowthRate = yearlyGrowthRate,
                CurrentMonthRevenue = currentMonthRevenue,
                PreviousMonthRevenue = previousMonthRevenue,
                MonthlyGrowthRate = monthlyGrowthRate,
                MonthlyEarningsTrend = monthlyEarningsTrend,
                RecentOrders = recentOrders,
                RecentActivities = recentActivities
            };
        }
    }
}