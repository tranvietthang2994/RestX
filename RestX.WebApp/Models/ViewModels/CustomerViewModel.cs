namespace RestX.WebApp.Models.ViewModels
{
    public class CustomerViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public int? Point { get; set; }
        public bool? IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
    public class CustomersManagementViewModel
    {
        public List<CustomerViewModel> Customers { get; set; } = new();
    }
}
