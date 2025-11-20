namespace RestX.API.Models.DTOs.Request
{
    public class CreatePaymentLinkRequest
    {
        public Guid OrderId { get; set; }
        public string? Description { get; set; }
    }
}
