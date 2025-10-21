namespace RestX.UI.Models.ViewModels
{
    public class TableListViewModel
    {
        public List<TableApiModel> Tables { get; set; } = new List<TableApiModel>();
        public int TotalTables { get; set; }
        public int AvailableTables { get; set; }
        public int OccupiedTables { get; set; }
    }

    public class TableApiModel
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? ReservedAt { get; set; }
        public string? QrCodeUrl { get; set; }
    }
}
