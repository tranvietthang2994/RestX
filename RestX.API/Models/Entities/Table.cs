using System.ComponentModel.DataAnnotations.Schema;

namespace RestX.API.Models.Entities;

[Table("Table")]
public partial class Table : Entity<int>
{
    public Guid OwnerId { get; set; }

    public int TableStatusId { get; set; }

    public int TableNumber { get; set; }

    public string Qrcode { get; set; } = null!;

    [Column(TypeName = "bit")]
    public bool? IsActive { get; set; } = true;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Owner Owner { get; set; } = null!;

    public virtual TableStatus TableStatus { get; set; } = null!;
}
