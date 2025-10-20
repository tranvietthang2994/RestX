using System;
using System.Collections.Generic;

namespace RestX.WebApp.Models;

public partial class Payment : Entity<Guid>
{
    public Guid OrderId { get; set; }

    public Guid PaymentMethodId { get; set; }

    public DateTime? Time { get; set; }

    public decimal Cost { get; set; }

    public bool? IsActive { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
}
