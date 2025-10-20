using RestX.API.Models.Entities;

namespace RestX.API.Models.ViewModels
{
    public class TableStatusViewModel
    {
        public int Id { get; set; }

        public int TableNumber { get; set; }

        public virtual TableStatus TableStatus { get; set; } = null!;
    }
}
