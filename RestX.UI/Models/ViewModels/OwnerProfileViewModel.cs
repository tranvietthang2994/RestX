using System.ComponentModel.DataAnnotations;

namespace RestX.UI.Models.ViewModels
{
    public class OwnerProfileViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Restaurant name is required")]
        [Display(Name = "Restaurant Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Restaurant Information")]
        public string? Information { get; set; }

        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [Display(Name = "Website")]
        public string? Website { get; set; }

        [Display(Name = "Opening Hours")]
        public string? OpeningHours { get; set; }

        [Display(Name = "Active")]
        public bool? IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Account information
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

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }
}
