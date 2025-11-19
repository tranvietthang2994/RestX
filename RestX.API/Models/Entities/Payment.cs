namespace RestX.API.Models.Entities;
public partial class Payment : Entity<Guid>
{
    public Guid OrderId { get; set; }

    public Guid PaymentMethodId { get; set; }

    public DateTime? Time { get; set; }

    public decimal Cost { get; set; }

    public bool? IsActive { get; set; }
    public long? OrderCode { get; set; }       
    public string? CheckoutUrl { get; set; }   
    public string? QRBase64 { get; set; }       
    public string? PayOSStatus { get; set; }      // PENDING / PAID / CANCELLED / EXPIRED
    public string? TransactionId { get; set; }   
    public DateTime? PaidAt { get; set; }
    public virtual Order Order { get; set; } = null!;

    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
}
