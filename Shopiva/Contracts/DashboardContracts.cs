namespace Shopiva.Contracts.Dashboard
{
    // ────────── Store Overview ──────────
    public record StoreOverviewResponse(
        decimal TotalRevenue,
        decimal RevenueGrowthPercent,
        int TotalOrders,
        decimal OrdersGrowthPercent,
        int NewCustomers,
        decimal CustomersGrowthPercent,
        double ConversionRate,
        string ConversionRateStatus  // "High", "Medium", "Low"
    );

    // ────────── Recent Users ──────────
    public record RecentUserResponse(
        string Id,
        string FullName,
        string Email,
        string Role,
        string Status,        // "Active", "Pending"
        string? ProfileImageUrl,
        DateTime JoinedAt
    );

    public record RecentUsersResponse(
        int TotalCount,
        List<RecentUserResponse> Users
    );

    // ────────── Promo Codes ──────────
    public record PromoCodeResponse(
        int Id,
        string Code,
        string Description,
        decimal DiscountPercent,
        bool IsActive,
        DateTime? ExpiresAt,
        string ExpiryStatus,   // "Active", "Expired", "No Expiry"
        DateTime CreatedAt
    );

    public record CreatePromoCodeRequest(
        string Code,
        string Description,
        decimal DiscountPercent,
        DateTime? ExpiresAt
    );

    public record UpdatePromoCodeRequest(
        string? Description,
        decimal? DiscountPercent,
        bool? IsActive,
        DateTime? ExpiresAt
    );

    // ────────── Banner ──────────
    public record BannerResponse(
        int Id,
        string Title,
        string? SubTitle,
        string ImageUrl,
        bool IsLive,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );

    public record CreateBannerRequest(
        string Title,
        string? SubTitle,
        string ImageUrl
    );

    public record UpdateBannerRequest(
        string? Title,
        string? SubTitle,
        string? ImageUrl
    );
}
