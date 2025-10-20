using System;
using System.Collections.Generic;

namespace RestX.WebApp.Models;

public partial class Supplier : Entity<Guid>
{
    public Guid OwnerId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Address { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<IngredientImport> IngredientImports { get; set; } = new List<IngredientImport>();

    public virtual Owner Owner { get; set; } = null!;
}
