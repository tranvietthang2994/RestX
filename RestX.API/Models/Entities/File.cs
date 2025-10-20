using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestX.API.Models.Entities;

[Table("File")]
public partial class File : Entity<Guid>
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string Url { get; set; } = null!;
}
