namespace Shopiva.Interfaces
{
    public interface IEmailService
    {
        Task<Result<bool>> SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);

        Task<Result<bool>> SendPasswordResetOtpAsync(string toEmail, string userName, string otp, CancellationToken cancellationToken = default);

        Task<Result<bool>> SendEmailConfirmationOtpAsync(string toEmail, string userName, string otp, CancellationToken cancellationToken = default);
    }
}
