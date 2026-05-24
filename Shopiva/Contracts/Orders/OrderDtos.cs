using Shopiva.Abstractions.enums;

namespace Shopiva.Contracts.Orders
{
    public class OrderDtos
    {  // ── Requests ─────────────────────────────────────────────────────────────

        /// <summary>Customer places an order from their active cart.</summary>
        public record PlaceOrderRequest(
            string ShippingAddress,
            string ShippingCity,
            string ShippingCountry,
            string? Notes
        );

        /// <summary>Admin/Seller updates the order status.</summary>
        public record UpdateOrderStatusRequest(
            OrderStatus NewStatus,
            string? Note          // optional tracking note shown in history
        );

        // ── Responses ─────────────────────────────────────────────────────────────

        public class OrderItemDto
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
            public decimal Subtotal { get; set; }
        }

        public class OrderStatusHistoryDto
        {
            public OrderStatus Status { get; set; }
            public string StatusLabel => Status.ToString();
            public string? Note { get; set; }
            public DateTime ChangedAt { get; set; }
        }

        public class OrderResponseDto
        {
            public int Id { get; set; }
            public string OrderNumber { get; set; } = string.Empty;
            public string CustomerId { get; set; } = string.Empty;
            public string CustomerEmail { get; set; } = string.Empty;
            public OrderStatus Status { get; set; }
            public string StatusLabel => Status.ToString();
            public string ShippingAddress { get; set; } = string.Empty;
            public string ShippingCity { get; set; } = string.Empty;
            public string ShippingCountry { get; set; } = string.Empty;
            public decimal SubTotal { get; set; }
            public decimal ShippingFee { get; set; }
            public decimal Total { get; set; }
            public string? Notes { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public List<OrderItemDto> Items { get; set; } = [];
            public List<OrderStatusHistoryDto> StatusHistory { get; set; } = [];
        }

        /// <summary>Lightweight summary used in list responses.</summary>
        public class OrderSummaryDto
        {
            public int Id { get; set; }
            public string OrderNumber { get; set; } = string.Empty;
            public OrderStatus Status { get; set; }
            public string StatusLabel => Status.ToString();
            public decimal Total { get; set; }
            public int ItemCount { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}