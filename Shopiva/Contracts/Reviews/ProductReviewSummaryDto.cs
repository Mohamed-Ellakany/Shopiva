namespace Shopiva.Contracts.Reviews
{
    public class ProductReviewSummaryDto
    {
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingBreakdown { get; set; } = []; // { 5: 10, 4: 5, ... }
        public List<ReviewResponseDto> Reviews { get; set; } = [];
    }
}
