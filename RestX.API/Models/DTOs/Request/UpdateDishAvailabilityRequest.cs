namespace RestX.API.Models.DTOs.Request
{
    public class UpdateDishAvailabilityRequest
    {
        public int DishId { get; set; }
        public bool IsActive { get; set; }
    }
}
