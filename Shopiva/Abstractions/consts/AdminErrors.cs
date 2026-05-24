namespace Shopiva.Abstractions.consts
{
    public static class AdminErrors
    {
        public static readonly Error UserNotFound =
          new("Admin.UserNotFound", "User not found.");

        public static readonly Error CannotRestrictAdmin =
            new("Admin.CannotRestrictAdmin", "Admin accounts cannot be restricted.");

        public static readonly Error CannotDeleteAdmin =
            new("Admin.CannotDeleteAdmin", "Admin accounts cannot be deleted through this endpoint.");

        public static readonly Error CannotRemoveLastAdmin =
            new("Admin.CannotRemoveLastAdmin", "Cannot remove the Admin role from the last remaining admin.");

        public static readonly Error UserAlreadyInRole =
            new("Admin.UserAlreadyInRole", "The user is already assigned this role.");

        public static readonly Error UserNotInRole =
            new("Admin.UserNotInRole", "The user does not have this role.");
    }
}