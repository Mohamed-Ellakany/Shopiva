namespace Shopiva.Contracts.Products
{
    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public List<string> ImageUrls { get; set; } = [];
        public DateTime CreatedAt { get; set; }

    }
}
