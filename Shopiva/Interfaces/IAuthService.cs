

namespace Shopiva.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> getTokenAsync(string email, string password , CancellationToken cancellationToken = default);
        Task<Result<AuthResponse>> getRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
        Task<Result<bool>> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
        Task<Result<AuthResponse>> RegisterAsync(RegisterRequest registerRequest, CancellationToken cancellationToken = default);
    }
} 
