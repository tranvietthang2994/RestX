using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestX.WebApp.Models;

[Table("Account")]
public partial class Account : Entity<Guid>
{
    [ForeignKey("Admin")]
    public Guid? AdminId { get; set; }

    [ForeignKey("Owner")]
    public Guid? OwnerId { get; set; }

    [ForeignKey("Staff")]
    public Guid? StaffId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string Password { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = null!;

    public virtual Admin? Admin { get; set; }

    public virtual Owner? Owner { get; set; }

    public virtual Staff? Staff { get; set; }
}
