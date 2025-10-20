namespace RestX.API.Models.DTOs.Request
{
    public class UpdateOrderDetailStatusRequest
    {
        public Guid OrderDetailId { get; set; }
        public bool IsActive { get; set; }
    }
} 