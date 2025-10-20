namespace RestX.WebApp.Models 
{
    public class RestaurantContext
    {
        public Guid OwnerId { get; set; } 
        public Guid? StaffId { get; set; }
        public int TableId { get; set; }
    }
}