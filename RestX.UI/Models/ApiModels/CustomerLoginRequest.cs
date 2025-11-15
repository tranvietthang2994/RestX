using System.ComponentModel.DataAnnotations;

namespace RestX.UI.Models.ApiModels
{
    public class CustomerLoginRequest
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "OwnerId is required")]
        public Guid OwnerId { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
