using Shopiva.Abstractions.enums;

namespace Shopiva.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;  

        public string CustomerId { get; set; } = string.Empty;
        public ApplicationUser Customer { get; set; } = null!;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Snapshot of shipping address at order time
        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;

        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; } = 0;
        public decimal Total => SubTotal + ShippingFee;

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    }
}
