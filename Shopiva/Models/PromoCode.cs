namespace Shopiva.Models
{
    public class PromoCode
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public decimal DiscountPercent { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
