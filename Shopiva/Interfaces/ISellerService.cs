using Shopiva.Contracts.SellerDashboard;

namespace Shopiva.Interfaces
{
    public interface ISellerService
    {
        Task<Result<SellerDashboardDto>> GetDashboardAsync(string sellerId, CancellationToken ct = default);

        Task<Result<PaginatedResult<SellerProductSummaryDto>>> GetMyProductsAsync(
            string sellerId, SellerProductFilterDto filter, CancellationToken ct = default);

        Task<Result<PaginatedResult<SellerOrderSummaryDto>>> GetMyOrdersAsync(
            string sellerId, SellerOrderFilterDto filter, CancellationToken ct = default);
    }
}
