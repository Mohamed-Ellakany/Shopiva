using System.ComponentModel.DataAnnotations;

namespace Shopiva.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string CustomerId { get; set; } = string.Empty;
        public ApplicationUser Customer { get; set; } = null!;
    }
}
