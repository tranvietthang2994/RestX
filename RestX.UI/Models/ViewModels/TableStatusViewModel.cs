namespace RestX.UI.Models.ViewModels
{
    public class TableStatusViewModel
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public TableStatusDetailViewModel? TableStatus { get; set; }
    }

    public class TableStatusDetailViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public IEnumerable<TableItemViewModel>? Tables { get; set; }
    }

    public class TableItemViewModel
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public bool IsActive { get; set; }
        public string? QrCode { get; set; }
        public int TableStatusId { get; set; }
    }
}
