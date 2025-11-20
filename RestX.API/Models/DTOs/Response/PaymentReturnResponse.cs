namespace RestX.API.Models.DTOs.Response
{
    public class PaymentReturnResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? OrderId { get; set; }
        public long? OrderCode { get; set; }
        public decimal? Amount { get; set; }
        public string? Status { get; set; }
    }
}
