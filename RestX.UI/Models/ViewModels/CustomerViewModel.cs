using System.ComponentModel.DataAnnotations;

namespace RestX.UI.Models.ViewModels
{
    public class CustomerViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Loyalty Points")]
        public int? Point { get; set; } = 0;

        [Display(Name = "Active")]
        public bool? IsActive { get; set; } = true;

        [Display(Name = "Registration Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime? ModifiedDate { get; set; }

        public Guid? OwnerId { get; set; }

        // For display purposes
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastOrderDate { get; set; }
        
        public string? ErrorMessage { get; set; }

        // Computed properties
        public string DisplayName => !string.IsNullOrEmpty(Name) ? Name : Phone;
        public string StatusText => IsActive == true ? "Active" : "Inactive";
        public string StatusBadgeClass => IsActive == true ? "badge-success" : "badge-danger";
    }

    public class CustomerManagementViewModel
    {
        public List<CustomerViewModel> Customers { get; set; } = new();
        public CustomerViewModel NewCustomer { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public decimal TotalCustomerValue { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class CustomerRequestViewModel
    {
        public List<OrderRequestViewModel> Orders { get; set; } = new();
    }

    public class OrderRequestViewModel
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public int TableNumber { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public DateTime? OrderTime { get; set; }
        public bool IsActive { get; set; }
        public List<OrderDetailRequestViewModel> OrderDetails { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }

    public class OrderDetailRequestViewModel
    {
        public Guid Id { get; set; }
        public int DishId { get; set; }
        public string DishName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public decimal SubTotal => Quantity * Price;
    }
}
