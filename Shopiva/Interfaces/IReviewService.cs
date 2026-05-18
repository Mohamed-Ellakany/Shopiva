using Shopiva.Contracts.Reviews;

namespace Shopiva.Interfaces
{
    public interface IReviewService
    {
        Task<Result<ProductReviewSummaryDto>> GetProductReviewsAsync(int productId);
        Task<Result<ReviewResponseDto>> GetByIdAsync(int reviewId);
        Task<Result<ReviewResponseDto>> CreateAsync(int productId, CreateReviewDto dto, string customerId);
        Task<Result<ReviewResponseDto>> UpdateAsync(int reviewId, UpdateReviewDto dto, string customerId);
        Task<Result<bool>> DeleteAsync(int reviewId, string customerId, bool isAdmin);
    }

}
