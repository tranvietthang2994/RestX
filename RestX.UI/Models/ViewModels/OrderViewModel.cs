using System.ComponentModel.DataAnnotations;

namespace RestX.UI.Models.ViewModels
{
    public class OrderViewModel
    {
        public Guid Id { get; set; }
        
        public Guid CustomerId { get; set; }
        
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;
        
        [Display(Name = "Customer Phone")]
        public string CustomerPhone { get; set; } = string.Empty;
        
        public int TableId { get; set; }
        
        [Display(Name = "Table Name")]
        public string TableName { get; set; } = string.Empty;
        
        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal TotalAmount { get; set; }
        
        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;
        
        [Display(Name = "Order Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime OrderDate { get; set; }
        
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
        
        [Display(Name = "Payment Status")]
        public string? PaymentStatus { get; set; }
        
        [Display(Name = "Payment Method")]
        public string? PaymentMethod { get; set; }
        
        public List<OrderDetailViewModel> OrderDetails { get; set; } = new();
        
        public string? ErrorMessage { get; set; }

        // Computed properties for display
        public string StatusBadgeClass => Status?.ToLower() switch
        {
            "pending" => "badge-warning",
            "confirmed" => "badge-info",
            "preparing" => "badge-primary",
            "ready" => "badge-success",
            "delivered" => "badge-success",
            "cancelled" => "badge-danger",
            _ => "badge-secondary"
        };

        public bool CanUpdateStatus => Status?.ToLower() is "pending" or "confirmed" or "preparing";
        public bool CanCancel => Status?.ToLower() is "pending" or "confirmed";
    }

    public class OrderDetailViewModel
    {
        public Guid Id { get; set; }
        
        public Guid OrderId { get; set; }
        
        public int DishId { get; set; }
        
        [Display(Name = "Dish Name")]
        public string DishName { get; set; } = string.Empty;
        
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }
        
        [Display(Name = "Price")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal Price { get; set; }
        
        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;
        
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
        
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
        
        [Display(Name = "Updated Date")]
        public DateTime? UpdatedDate { get; set; }

        // Computed properties
        [Display(Name = "Subtotal")]
        public decimal Subtotal => Price * Quantity;

        public string StatusBadgeClass => Status?.ToLower() switch
        {
            "pending" => "badge-warning",
            "preparing" => "badge-primary",
            "ready" => "badge-success",
            "served" => "badge-success",
            "cancelled" => "badge-danger",
            _ => "badge-secondary"
        };

        public bool CanUpdateStatus => Status?.ToLower() is "pending" or "preparing";
    }

    public class OrderHistoryViewModel
    {
        public List<OrderViewModel> Orders { get; set; } = new();
        public string? CustomerName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? StatusFilter { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class KitchenOrderViewModel
    {
        public List<OrderViewModel> PendingOrders { get; set; } = new();
        public List<OrderDetailViewModel> PreparingItems { get; set; } = new();
        public List<OrderDetailViewModel> ReadyItems { get; set; } = new();
        public int TotalPendingOrders { get; set; }
        public int TotalPreparingItems { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class OrderStatusRequest
    {
        [Required]
        public Guid OrderId { get; set; }
        
        [Required]
        public string Status { get; set; } = string.Empty;
        
        public string? Notes { get; set; }
    }

    public class OrderDetailStatusRequest
    {
        [Required]
        public Guid OrderDetailId { get; set; }
        
        [Required]
        public string Status { get; set; } = string.Empty;
        
        public string? Notes { get; set; }
    }
}
