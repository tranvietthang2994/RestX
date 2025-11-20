namespace RestX.API.Models.Entities
{
    public partial class PaymentTransaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string? Bank { get; set; }
        public string? CustomerName { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionTime { get; set; } = DateTime.UtcNow;
        public string? RawData { get; set; } 

        // Navigation
        public virtual Payment Payment { get; set; } = null!;
    }
}
