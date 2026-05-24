namespace Shopiva.Contracts.AdminPanel
{
    // ── User management ───────────────────────────────────────────────────────

    public class AdminUserSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool IsRestricted { get; set; }          // maps to LockoutEnabled + LockoutEnd
        public bool EmailConfirmed { get; set; }
        public List<string> Roles { get; set; } = [];
        public DateTime? LockoutEnd { get; set; }
    }

    public class AdminUserDetailDto : AdminUserSummaryDto
    {
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? ProfileImageUrl { get; set; }
        public int TotalOrders { get; set; }
        public int TotalReviews { get; set; }
    }

    public class UserFilterDto
    {
        public string? Search { get; set; }          // email / name
        public string? Role { get; set; }            // "Customer" | "Seller" | "Admin"
        public bool? IsRestricted { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public record AssignRoleRequest(string Role);
    public record RestrictUserRequest(bool Restrict, string? Reason, DateTime? Until);

    // ── Product management ────────────────────────────────────────────────────

    public class AdminProductSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public string SellerEmail { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminProductFilterDto
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public bool? IsActive { get; set; }          // null = all, true = active only, false = inactive only
        public string? SellerId { get; set; }
        public string SortBy { get; set; } = "createdAt";
        public bool Descending { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────

    public class DashboardSummaryDto
    {
        public int TotalUsers { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalSellers { get; set; }
        public int RestrictedUsers { get; set; }

        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int OutOfStockProducts { get; set; }

        public int TotalCategories { get; set; }

        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        public int TotalReviews { get; set; }
    }
}
