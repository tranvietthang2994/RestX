using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestX.API.Models.Entities;

[Table("Category")]
public partial class Category : Entity<int>
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    public virtual ICollection<Dish> Dishes { get; set; } = new List<Dish>();
}
