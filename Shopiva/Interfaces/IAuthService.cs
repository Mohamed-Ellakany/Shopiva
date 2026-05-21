

namespace Shopiva.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default);
        Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
        Task<Result<bool>> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
        Task<Result<AuthResponse>> RegisterAsync(RegisterRequest registerRequest, CancellationToken cancellationToken = default);


        // ── Password reset via OTP ──────────────────────────────────
        Task<Result<bool>> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<bool>> ResetPasswordAsync(string email, string otp, string newPassword, CancellationToken cancellationToken = default);

        // ── Email confirmation via OTP ──────────────────────────────
        Task<Result<bool>> SendEmailConfirmationOtpAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<bool>> ConfirmEmailAsync(string email, string otp, CancellationToken cancellationToken = default);
    }
}
    