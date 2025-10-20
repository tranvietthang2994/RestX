using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestX.WebApp.Models.ViewModels;

namespace RestX.WebApp.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<Dictionary<DateTime, decimal>> GetCostByDateAsync(CancellationToken cancellationToken = default);
        Task<Dictionary<DateTime, decimal>> GetProfitByDateAsync(CancellationToken cancellationToken = default);
        Task<Dictionary<string, List<DailyFinanceViewModel>>> GetMonthlyProfitDataAsync(CancellationToken cancellationToken = default); // Thêm dòng này
        Task<DashboardViewModel> GetDashboardViewModelAsync(CancellationToken cancellationToken = default);
    }
}
