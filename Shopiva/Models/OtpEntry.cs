namespace Shopiva.Models
{
    public class OtpEntry
    {
        public string OtpHash { get; set; } = string.Empty;

        public int Attempts { get; set; } = 0;

        public const int MaxAttempts = 5;
    }
}
