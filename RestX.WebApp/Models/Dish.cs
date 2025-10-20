using System;
using System.Collections.Generic;

namespace RestX.WebApp.Models;

public partial class Dish : Entity<int>
{
    public Guid OwnerId { get; set; }

    public Guid? FileId { get; set; }

    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public bool? IsActive { get; set; }

    public virtual Owner Owner { get; set; } = null!;

    public virtual File? File { get; set; } = null!;

    public virtual Category? Category { get; set; } = null!;

    public virtual ICollection<DishIngredient>? DishIngredients { get; set; } = new List<DishIngredient>();

    public virtual ICollection<OrderDetail>? OrderDetails { get; set; } = new List<OrderDetail>();
}
