using RestX.UI.Models.ViewModels;

namespace RestX.UI.Services.Interfaces
{
    public interface IOwnerUIService
    {
        /// <summary>
        /// Lấy thông tin dashboard của owner
        /// </summary>
        /// <returns>Dashboard view model</returns>
        Task<DashboardViewModel?> GetDashboardAsync();

        /// <summary>
        /// Lấy thông tin profile của owner
        /// </summary>
        /// <returns>Owner profile view model</returns>
        Task<OwnerProfileViewModel?> GetOwnerProfileAsync();

        /// <summary>
        /// Cập nhật thông tin profile của owner
        /// </summary>
        /// <param name="profileModel">Thông tin profile cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        Task<(bool Success, string Message)> UpdateOwnerProfileAsync(OwnerProfileViewModel profileModel);

        /// <summary>
        /// Lấy thống kê doanh thu
        /// </summary>
        /// <param name="from">Từ ngày</param>
        /// <param name="to">Đến ngày</param>
        /// <returns>Thống kê doanh thu</returns>
        Task<RevenueStatisticsViewModel?> GetRevenueStatisticsAsync(DateTime? from = null, DateTime? to = null);

        /// <summary>
        /// Lấy thống kê đơn hàng
        /// </summary>
        /// <param name="from">Từ ngày</param>
        /// <param name="to">Đến ngày</param>
        /// <returns>Thống kê đơn hàng</returns>
        Task<OrderStatisticsViewModel?> GetOrderStatisticsAsync(DateTime? from = null, DateTime? to = null);

        /// <summary>
        /// Lấy báo cáo tổng quan
        /// </summary>
        /// <param name="period">Kỳ báo cáo</param>
        /// <returns>Báo cáo tổng quan</returns>
        Task<OverviewReportViewModel?> GetOverviewReportAsync(string period = "month");

        /// <summary>
        /// Lấy món ăn bán chạy nhất
        /// </summary>
        /// <param name="limit">Số lượng món</param>
        /// <param name="period">Kỳ thống kê</param>
        /// <returns>Danh sách món ăn bán chạy</returns>
        Task<List<TopDishViewModel>?> GetTopDishesAsync(int limit = 10, string period = "month");
    }
}