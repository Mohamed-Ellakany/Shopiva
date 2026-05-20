
namespace Shopiva.Models
{

    public sealed class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? ProfileImageUrl { get; set; }
        public List<RefreshTokens> RefreshTokens { get; set; } = [];

        public Cart? Cart { get; set; }
    }
}

