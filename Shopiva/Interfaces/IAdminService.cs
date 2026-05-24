namespace Shopiva.Interfaces
{
    public interface IAdminService
    {
        // ── Dashboard ──────────────────────────────────────────────────────────
        Task<Result<DashboardSummaryDto>> GetDashboardAsync(CancellationToken ct = default);

        // ── User Management ────────────────────────────────────────────────────
        Task<Result<PaginatedResult<AdminUserSummaryDto>>> GetUsersAsync(
            UserFilterDto filter, CancellationToken ct = default);

        Task<Result<AdminUserDetailDto>> GetUserByIdAsync(
            string userId, CancellationToken ct = default);

        Task<Result<bool>> RestrictUserAsync(
            string userId, RestrictUserRequest request, CancellationToken ct = default);

        Task<Result<bool>> DeleteUserAsync(
            string userId, CancellationToken ct = default);

        Task<Result<AdminUserSummaryDto>> AssignRoleAsync(
            string userId, AssignRoleRequest request, CancellationToken ct = default);

        Task<Result<AdminUserSummaryDto>> RevokeRoleAsync(
            string userId, AssignRoleRequest request, CancellationToken ct = default);

        // ── Product Management ─────────────────────────────────────────────────
        Task<Result<PaginatedResult<AdminProductSummaryDto>>> GetProductsAsync(
            AdminProductFilterDto filter, CancellationToken ct = default);

        Task<Result<bool>> SetProductActiveAsync(
            int productId, bool isActive, CancellationToken ct = default);
    }
}
