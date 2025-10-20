using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestX.WebApp.Models;

[Table("Customer")]
public partial class Customer : Entity<Guid>
{
    [ForeignKey("Owner")]
    public Guid? OwnerId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    [Phone]
    public string Phone { get; set; } = null!;

    public int? Point { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Owner? Owner { get; set; }
}
