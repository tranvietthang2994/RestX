namespace RestX.WebApp.Models.ViewModels
{
    public class ActivityViewModel
    {
        public DateTime Time { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
        public decimal TotalAmount { get; set; }
        public List<DishViewModel> Dishes { get; set; } = new();

    }
}
