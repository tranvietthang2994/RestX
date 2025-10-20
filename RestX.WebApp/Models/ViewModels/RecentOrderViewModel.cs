namespace RestX.WebApp.Models.ViewModels
{
    public class RecentOrderViewModel
    {
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public string TableName { get; set; }
        public List<DishViewModel> Dishes { get; set; } = new();
        public string Status { get; set; }
        public decimal Amount { get; set; }
    }
}
