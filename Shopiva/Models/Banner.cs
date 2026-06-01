namespace Shopiva.Models
{
    public class Banner
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? SubTitle { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsLive { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
