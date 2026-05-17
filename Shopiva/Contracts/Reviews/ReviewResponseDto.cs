namespace Shopiva.Contracts.Reviews
{
    public class ReviewResponseDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
