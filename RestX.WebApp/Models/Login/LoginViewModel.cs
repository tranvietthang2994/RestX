using System.ComponentModel.DataAnnotations;

namespace RestX.WebApp.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại *")]
        public string Phone { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập tên của bạn.")]
        [MaxLength(100)]
        [Display(Name = "Tên của bạn")]
        public string Name { get; set; } = null!;

        // Dùng để lưu trữ OwnerId khi tạo khách hàng mới
        public Guid OwnerId { get; set; }

        // Dùng để chuyển hướng lại trang chủ sau khi đăng nhập thành công
        public string? ReturnUrl { get; set; }
    }
}