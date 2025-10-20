using System;
using System.Collections.Generic;

namespace RestX.WebApp.Models;

public partial class Ingredient : Entity<int>
{
    public Guid OwnerId { get; set; }

    public string Name { get; set; } = null!;

    public decimal? CurrentQuantity { get; set; }

    public string Unit { get; set; } = null!;

    public virtual ICollection<DishIngredient> DishIngredients { get; set; } = new List<DishIngredient>();

    public virtual ICollection<IngredientExport> IngredientExports { get; set; } = new List<IngredientExport>();

    public virtual ICollection<IngredientImport> IngredientImports { get; set; } = new List<IngredientImport>();

    public virtual Owner Owner { get; set; } = null!;
}
