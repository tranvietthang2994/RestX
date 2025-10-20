using System;

namespace RestX.WebApp.Models.Home
{
    public class HomeViewModel
    {
        // Các thuộc tính cũ
        public Guid OwnerId { get; set; }
        public int TableId { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string? FileName { get; set; }
        public string? FileUrl { get; set; }
        public int TableNumber { get; set; }

        // --- CÁC THUỘC TÍNH MỚI ĐƯỢC THÊM VÀO ---
        /// <summary>
        /// Cho biết khách hàng đã đăng nhập hay chưa.
        /// </summary>
        public bool IsCustomerLoggedIn { get; set; }

        /// <summary>
        /// Tên của khách hàng nếu đã đăng nhập.
        /// </summary>
        public string? CustomerName { get; set; }

        /// <summary>
        /// Điểm tích lũy của khách hàng nếu đã đăng nhập.
        /// </summary>
        public int CustomerPoints { get; set; }
    }
}
