namespace RestX.API.Models.DTOs.Request
{
    public class UpdateOrderStatusByIdRequest
    {
        public Guid OrderId { get; set; }
        public int NewStatusId { get; set; }
    }
}
