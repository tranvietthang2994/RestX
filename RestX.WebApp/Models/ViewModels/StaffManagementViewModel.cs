using System.ComponentModel.DataAnnotations;

namespace RestX.WebApp.Models.ViewModels
{
    public class StaffManagementViewModel
    {
        public List<StaffViewModel> Staffs { get; set; } = new();
    }

    public class StaffViewModel
    {
        public Guid Id { get; set; }
        public Guid? FileId { get; set; }
        public string? ImageUrl { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
    }

    public class StaffRequest
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Staff name is required")]
        [MaxLength(100, ErrorMessage = "Staff name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [MaxLength(255, ErrorMessage = "Password cannot exceed 255 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone format")]
        [MaxLength(15, ErrorMessage = "Phone cannot exceed 15 characters")]
        public string Phone { get; set; } = string.Empty;

       
        public IFormFile? ImageFile { get; set; }

        public bool IsActive { get; set; } = true;
    }
}