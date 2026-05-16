namespace Shopiva.Abstractions.consts
{
    public static class UserErrors
    {
        public static readonly Error InvalidCredentials = new Error("User.InvalidCredentials", "The provided credentials are invalid.");
        public static readonly Error InvalidToken = new Error("Token.InvalidToken", "Invalid Token .");
        public static readonly Error EmailAlreadyInUse = new Error("User.EmailAlreadyInUse", "The email address is already in use.");
        public static readonly Error InvalidRefreshToken = new Error("Invalid.RefreshToken", "Refresh token is not founded.");
        public static readonly Error InvalidOperation = new Error("Invalid.Operation", "Invaild operation try again.");
        public static readonly Error WeakPassword = new Error("User.WeakPassword", "The provided password does not meet the security requirements.");
        public static readonly Error UnauthorizedAccess = new Error("Access.Unauthorized", "You do not have permission to access this resource.");
    }
}
