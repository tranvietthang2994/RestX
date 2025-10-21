using System.ComponentModel.DataAnnotations;

namespace RestX.UI.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; } = false;

        public string? ErrorMessage { get; set; }
        public string? ReturnUrl { get; set; }

        // For AuthCustomer compatibility
        public string? OwnerId { get; set; }
        public string? Phone { get; set; }
        public string? Name { get; set; }
    }

    public class CustomerLoginViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        public Guid OwnerId { get; set; }
        public int TableId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
