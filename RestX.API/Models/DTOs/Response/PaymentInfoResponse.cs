namespace RestX.API.Models.DTOs.Response
{
    public class PaymentInfoResponse
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public long OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
