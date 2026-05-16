
namespace Shopiva.Interfaces
{
    public interface IJwtProvider
    {
        (string token , int expireIn) GenerateJwtToken(ApplicationUser user);
        string? ValidateToken(string token);
    }
}
