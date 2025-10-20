namespace RestX.WebApp.Models.ViewModels
{
    public class OwnerProfileViewModel
    {
        public Guid? OwnerId { get; set; }
        public Guid FileId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Information { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}
