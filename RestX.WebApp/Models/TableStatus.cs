using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestX.WebApp.Models;

[Table("TableStatus")]
public partial class TableStatus : Entity<int>
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;

    public virtual ICollection<Table> Tables { get; set; } = new List<Table>();
}
