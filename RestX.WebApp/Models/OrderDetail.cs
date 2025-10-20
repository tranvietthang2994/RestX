using System;
using System.Collections.Generic;

namespace RestX.WebApp.Models;

public partial class OrderDetail : Entity<Guid>
{
    public Guid OrderId { get; set; }

    public int DishId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public bool? IsActive { get; set; }

    public virtual Dish Dish { get; set; } = null!;

    public virtual ICollection<IngredientExport> IngredientExports { get; set; } = new List<IngredientExport>();

    public virtual Order Order { get; set; } = null!;
}
