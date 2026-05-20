namespace Shopiva.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public Cart Cart { get; set; } = null!;
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }  // snapshot price at add-time
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
