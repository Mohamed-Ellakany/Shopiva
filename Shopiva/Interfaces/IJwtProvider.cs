
namespace Shopiva.Interfaces
{
    public interface IJwtProvider
    {
        Task<(string token, int expireIn)> GenerateJwtTokenAsync(ApplicationUser user);
        string? ValidateToken(string token);
    }
}
