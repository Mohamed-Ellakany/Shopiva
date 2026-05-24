namespace Shopiva.Contracts.SellerDashboard
{
    public class SellerDashboardDto
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int OutOfStockProducts { get; set; }

        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public decimal TotalRevenue { get; set; }   // seller's items in delivered orders only

        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }

    // ── Seller's product list ─────────────────────────────────────────────────

    public class SellerProductSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int TotalUnitsSold { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> ImageUrls { get; set; } = [];
    }

    // ── Seller's order list ───────────────────────────────────────────────────

    public class SellerOrderSummaryDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal SellerItemsTotal { get; set; }   // only this seller's items
        public int SellerItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ── Filters ───────────────────────────────────────────────────────────────

    public class SellerProductFilterDto
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public bool? IsActive { get; set; }
        public bool? InStock { get; set; }
        public string SortBy { get; set; } = "createdAt";
        public bool Descending { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class SellerOrderFilterDto
    {
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}