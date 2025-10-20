using System;

namespace RestX.WebApp.Models.ViewModels
{
    public class TableViewModel
    {
        public int Id { get; set; }
        public Guid OwnerId { get; set; }
        public int TableStatusId { get; set; }
        public int TableNumber { get; set; }
        public string Qrcode { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public TableStatus TableStatus { get; set; } = null;
    }

    public class TableListViewModel
    {
        public List<TableViewModel> Tables { get; set; } = new();
    }
}