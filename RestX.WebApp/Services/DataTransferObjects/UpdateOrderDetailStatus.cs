namespace RestX.WebApp.Services.DataTransferObjects
{
    public class UpdateOrderDetailStatus
    {
        public Guid OrderDetailId { get; set; }
        public bool IsActive { get; set; }
    }
} 