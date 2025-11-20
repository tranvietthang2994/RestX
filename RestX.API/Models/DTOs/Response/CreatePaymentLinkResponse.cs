namespace RestX.API.Models.DTOs.Response
{
    public class CreatePaymentLinkResponse
    {
        public Guid PaymentId { get; set; }
        public long OrderCode { get; set; }
        public string CheckoutUrl { get; set; } = string.Empty;
        public string QRBase64 { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "PENDING";
    }
}
