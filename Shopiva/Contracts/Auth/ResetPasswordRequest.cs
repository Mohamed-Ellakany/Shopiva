namespace Shopiva.Contracts.Auth
{
    public record ResetPasswordRequest(string Email, string Otp, string NewPassword);

}
