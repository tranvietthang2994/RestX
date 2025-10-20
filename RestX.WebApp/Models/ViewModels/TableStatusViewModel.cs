namespace RestX.WebApp.Models.ViewModels
{
    public class TableStatusViewModel
    {
        public int Id { get; set; }

        public int TableNumber { get; set; }

        public virtual TableStatus TableStatus { get; set; } = null!;
    }
}
