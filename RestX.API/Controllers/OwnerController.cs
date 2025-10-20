using Microsoft.AspNetCore.Mvc;
using RestX.API.Models.ViewModels;
using RestX.API.Services.Interfaces;

namespace RestX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OwnerController : ControllerBase
    {
        private readonly IDashboardService dashboardService;
        private readonly IOwnerService ownerService;
        private readonly IExceptionHandler exceptionHandler;

        public OwnerController(
            IDashboardService dashboardService,
            IOwnerService ownerService,
            IExceptionHandler exceptionHandler)
        {
            this.dashboardService = dashboardService;
            this.ownerService = ownerService;
            this.exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Lấy thông tin dashboard của owner
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Thông tin dashboard với thống kê</returns>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken = default)
        {
            try
            {
                var dashboardViewModel = await dashboardService.GetDashboardViewModelAsync(cancellationToken);
                return Ok(new { success = true, data = dashboardViewModel });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Owner not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading the dashboard.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy thông tin profile của owner hiện tại
        /// </summary>
        /// <returns>Thông tin profile owner</returns>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var profileViewModel = await ownerService.GetOwnerProfileViewModelAsync();
                return Ok(new { success = true, data = profileViewModel });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Owner not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading owner profile.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Cập nhật thông tin profile của owner
        /// </summary>
        /// <param name="viewModel">Thông tin profile cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] OwnerProfileViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var (success, message) = await ownerService.UpdateOwnerProfileAsync(viewModel);

                if (success)
                {
                    return Ok(new { success = true, message = message ?? "Profile updated successfully!" });
                }
                else
                {
                    return BadRequest(new { success = false, message = message ?? "Update failed!" });
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Owner not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while updating owner profile.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy thống kê doanh thu theo khoảng thời gian
        /// </summary>
        /// <param name="from">Từ ngày</param>
        /// <param name="to">Đến ngày</param>
        /// <returns>Thống kê doanh thu</returns>
        [HttpGet("statistics/revenue")]
        public async Task<IActionResult> GetRevenueStatistics([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            try
            {
                // TODO: Implement GetRevenueStatisticsAsync in IDashboardService
                return Ok(new { 
                    success = true, 
                    message = "GetRevenueStatisticsAsync method not implemented yet",
                    from = from,
                    to = to
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Owner not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading revenue statistics.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy thống kê đơn hàng theo khoảng thời gian
        /// </summary>
        /// <param name="from">Từ ngày</param>
        /// <param name="to">Đến ngày</param>
        /// <returns>Thống kê đơn hàng</returns>
        [HttpGet("statistics/orders")]
        public async Task<IActionResult> GetOrderStatistics([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            try
            {
                // TODO: Implement GetOrderStatisticsAsync in IDashboardService
                return Ok(new { 
                    success = true, 
                    message = "GetOrderStatisticsAsync method not implemented yet",
                    from = from,
                    to = to
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Owner not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading order statistics.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy báo cáo tổng quan
        /// </summary>
        /// <param name="period">Kỳ báo cáo (today, week, month, year)</param>
        /// <returns>Báo cáo tổng quan</returns>
        [HttpGet("reports/overview")]
        public async Task<IActionResult> GetOverviewReport([FromQuery] string period = "month")
        {
            try
            {
                // TODO: Implement GetOverviewReportAsync in IDashboardService
                return Ok(new { 
                    success = true, 
                    message = "GetOverviewReportAsync method not implemented yet",
                    period = period
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Owner not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading overview report.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy top món ăn bán chạy
        /// </summary>
        /// <param name="limit">Số lượng món ăn</param>
        /// <param name="period">Kỳ thống kê (week, month, year)</param>
        /// <returns>Danh sách món ăn bán chạy</returns>
        [HttpGet("reports/top-dishes")]
        public async Task<IActionResult> GetTopDishes([FromQuery] int limit = 10, [FromQuery] string period = "month")
        {
            try
            {
                // TODO: Implement GetTopDishesAsync in IDashboardService
                return Ok(new { 
                    success = true, 
                    message = "GetTopDishesAsync method not implemented yet",
                    limit = limit,
                    period = period
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Owner not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading top dishes report.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy thông tin nhà hàng của owner
        /// </summary>
        /// <returns>Thông tin nhà hàng</returns>
        [HttpGet("restaurant")]
        public async Task<IActionResult> GetRestaurantInfo()
        {
            try
            {
                // TODO: Implement GetRestaurantInfoAsync in IOwnerService
                return Ok(new { 
                    success = true, 
                    message = "GetRestaurantInfoAsync method not implemented yet"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Owner not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while loading restaurant info.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Cập nhật thông tin nhà hàng
        /// </summary>
        /// <param name="request">Thông tin nhà hàng cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("restaurant")]
        public async Task<IActionResult> UpdateRestaurantInfo([FromBody] UpdateRestaurantInfoRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                // TODO: Implement UpdateRestaurantInfoAsync in IOwnerService
                return Ok(new { 
                    success = true, 
                    message = "UpdateRestaurantInfoAsync method not implemented yet"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Owner not authenticated" });
            }
            catch (Exception ex)
            {
                this.exceptionHandler.RaiseException(ex, "An error occurred while updating restaurant info.");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }

    /// <summary>
    /// Request model cho cập nhật thông tin nhà hàng
    /// </summary>
    public class UpdateRestaurantInfoRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Information { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
}
