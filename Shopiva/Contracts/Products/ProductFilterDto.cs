namespace Shopiva.Contracts.Products
{
    public class ProductFilterDto
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStock { get; set; }
        public string SortBy { get; set; } = "createdAt"; 
        public bool Descending { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

    }
}
