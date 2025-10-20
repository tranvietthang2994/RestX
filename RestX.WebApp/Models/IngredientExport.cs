using System;
using System.Collections.Generic;

namespace RestX.WebApp.Models;

public partial class IngredientExport : Entity<Guid>
{
    public int IngredientId { get; set; }

    public Guid OrderDetailId { get; set; }

    public decimal Quantity { get; set; }

    public DateTime? Time { get; set; }

    public virtual Ingredient Ingredient { get; set; } = null!;

    public virtual OrderDetail OrderDetail { get; set; } = null!;
}
