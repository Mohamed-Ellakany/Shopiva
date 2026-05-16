namespace Shopiva.Abstractions.consts
{
    public static class Regex
    {
        public const string Password = "(?=(.*[0-9]))(?=.*[\\!@#$%^&*()\\\\[\\]{}\\-_+=~`:;\"'<>.,/?])(?=.*[a-z])(?=(.*[A-Z]))(?=(.*)).{8,}";
    }
}


