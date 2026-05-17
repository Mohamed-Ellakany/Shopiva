using Shopiva.Contracts.Reviews;

namespace Shopiva.Services
{
    public class ReviewService(AppDbContext context) : IReviewService
    {
        private readonly AppDbContext _context = context;
        public async Task<Result<ProductReviewSummaryDto>> GetProductReviewsAsync(int productId)
        {
            var productExists = await _context.Products
                .AnyAsync(p => p.Id == productId && p.IsActive);

            if (!productExists)
                return Result.Failure<ProductReviewSummaryDto>(new Error("ProductNotFound", "Product not found"));

            var reviews = await _context.Reviews
                .Include(r => r.Customer)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var breakdown = Enumerable.Range(1, 5)
                .ToDictionary(star => star, star => reviews.Count(r => r.Rating == star));

            var summary = new ProductReviewSummaryDto
            {
                TotalReviews = reviews.Count,
                AverageRating = reviews.Count > 0
                    ? Math.Round(reviews.Average(r => r.Rating), 1)
                    : 0,
                RatingBreakdown = breakdown,
                Reviews = reviews.Select(MapToDto).ToList()
            };

            return Result<ProductReviewSummaryDto>.Success(summary);
        }

        public async Task<Result<ReviewResponseDto>> GetByIdAsync(int reviewId)
        {
            var review = await _context.Reviews
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review is null)
                return Result.Failure<ReviewResponseDto>(new Error("ReviewNotFound", "Review not found"));

            return Result<ReviewResponseDto>.Success(MapToDto(review));
        }

        public async Task<Result<ReviewResponseDto>> CreateAsync(int productId, CreateReviewDto dto, string customerId)
        {
            var productExists = await _context.Products
                .AnyAsync(p => p.Id == productId && p.IsActive);

            if (!productExists)
                return Result.Failure<ReviewResponseDto>(new Error("ProductNotFound", "Product not found"));

            // One review per customer per product
            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.ProductId == productId && r.CustomerId == customerId);

            if (alreadyReviewed)
                return Result.Failure<ReviewResponseDto>(new Error("ReviewAlreadyExists", "You have already reviewed this product"));

            // Must have purchased the product to review it
            //var hasPurchased = await _context.Orders
            //    .AnyAsync(o => o.CustomerId == customerId
            //                && o.Status == OrderStatus.Delivered
            //                && o.OrderItems.Any(i => i.ProductId == productId));

            //if (!hasPurchased)
            //    return Result.Failure<ReviewResponseDto>(new Error("Unauthorized", "You can only review products you have purchased");

            var review = new Review
            {
                ProductId = productId,
                CustomerId = customerId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            await _context.Entry(review).Reference(r => r.Customer).LoadAsync();

            return Result<ReviewResponseDto>.Success(MapToDto(review));
        }

        public async Task<Result<ReviewResponseDto>> UpdateAsync(int reviewId, UpdateReviewDto dto, string customerId)
        {
            var review = await _context.Reviews
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review is null)
                return Result.Failure<ReviewResponseDto>(new Error("ReviewNotFound", "Review not found"));

            if (review.CustomerId != customerId)
                return Result.Failure<ReviewResponseDto>(new Error("Unauthorized", "Unauthorized"));

            if (dto.Rating.HasValue) review.Rating = dto.Rating.Value;
            if (dto.Comment is not null) review.Comment = dto.Comment;

            review.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Result<ReviewResponseDto>.Success(MapToDto(review));
        }

        public async Task<Result<bool>> DeleteAsync(int reviewId, string customerId, bool isAdmin)
        {
            var review = await _context.Reviews.FindAsync(reviewId);

            if (review is null)
                return Result.Failure<bool>(new Error("ReviewNotFound", "Review not found"));

            if (!isAdmin && review.CustomerId != customerId)
                return Result.Failure<bool>(new Error("Unauthorized", "Unauthorized"));

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        private static ReviewResponseDto MapToDto(Review r) => new()
        {
            Id = r.Id,
            Rating = r.Rating,
            Comment = r.Comment,
            CustomerId = r.CustomerId,
            CustomerName = r.Customer?.UserName ?? "",
            ProductId = r.ProductId,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
    }
}
