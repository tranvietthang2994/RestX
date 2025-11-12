namespace RestX.API.Models.DTOs.Response
{
    public class StaffProfileDTO
    {
        public Guid? OwnerId { get; set; }
        public Guid FileId { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public bool? IsActive { get; set; }
        public string? FileUrl { get; set; }
        public string? OwnerName { get; set; }
    }

}
