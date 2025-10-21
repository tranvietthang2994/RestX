namespace RestX.UI.Models.ApiModels
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public UserInfo? User { get; set; }
    }

    public class UserInfo
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Owner", "Staff", "Customer"
        public Guid? OwnerId { get; set; } // For Staff and Owner
        public Guid? StaffId { get; set; } // For Staff
        public Guid? CustomerId { get; set; } // For Customer
    }
}
