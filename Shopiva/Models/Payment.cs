using Shopiva.Abstractions.enums;

namespace Shopiva.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public decimal Amount { get; set; }

        public PaymentMethod Method { get; set; } = PaymentMethod.COD;
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
