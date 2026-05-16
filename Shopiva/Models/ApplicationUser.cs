
namespace Shopiva.Models
{

        public sealed class ApplicationUser : IdentityUser
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public List<RefreshTokens> RefreshTokens { get; set; } = [];

        }
    }

