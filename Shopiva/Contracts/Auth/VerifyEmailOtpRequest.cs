namespace Shopiva.Contracts.Auth
{
    public record VerifyEmailOtpRequest(string Email, string Otp);

}
