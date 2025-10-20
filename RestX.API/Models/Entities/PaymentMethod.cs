namespace RestX.API.Models.Entities;

public partial class PaymentMethod
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
