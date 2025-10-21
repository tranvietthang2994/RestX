using System.ComponentModel.DataAnnotations;

namespace RestX.UI.Models.ViewModels
{
    public class StaffViewModel
    {
        public Guid Id { get; set; }
        
        public Guid? OwnerId { get; set; }

        [Required(ErrorMessage = "Staff name is required")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [Display(Name = "Position")]
        public string? Position { get; set; }

        [Display(Name = "Salary")]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive number")]
        public decimal? Salary { get; set; }

        [Display(Name = "Hire Date")]
        public DateTime? HireDate { get; set; }

        [Display(Name = "Active")]
        public bool? IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Account information
        [Display(Name = "Username")]
        public string? Username { get; set; }

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        public string? Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }

        public string? ErrorMessage { get; set; }
    }

    public class StaffProfileViewModel
    {
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [Display(Name = "Position")]
        public string? Position { get; set; }

        [Display(Name = "Username")]
        public string? Username { get; set; }

        [Display(Name = "Current Password")]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        public string? NewPassword { get; set; }

        [Display(Name = "Confirm New Password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }

    public class StaffManagementViewModel
    {
        public List<StaffViewModel> StaffList { get; set; } = new();
        public StaffViewModel NewStaff { get; set; } = new();
        public int TotalStaff { get; set; }
        public int ActiveStaff { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
