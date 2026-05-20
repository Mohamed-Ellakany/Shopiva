using Shopiva.Abstractions.enums;

namespace Shopiva.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public CartStatus Status { get; set; } = CartStatus.Active;
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
