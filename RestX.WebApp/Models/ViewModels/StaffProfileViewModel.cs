namespace RestX.WebApp.Models.ViewModels
{
    public class StaffProfileViewModel
    {
        public Guid? OwnerId { get; set; }

        public Guid FileId { get; set; }

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Phone { get; set; } = null!;

        public bool? IsActive { get; set; }

        public virtual File File { get; set; } = null!;

        public virtual Owner? Owner { get; set; }

    }
}
