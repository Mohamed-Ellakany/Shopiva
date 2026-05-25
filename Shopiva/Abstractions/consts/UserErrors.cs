namespace Shopiva.Abstractions.consts
{
    public static class UserErrors
    {
        // Auth Errors
        public static readonly Error InvalidCredentials = new Error("User.InvalidCredentials", "The provided credentials are invalid.");
        public static readonly Error InvalidToken = new Error("Token.InvalidToken", "Invalid Token .");
        public static readonly Error EmailAlreadyInUse = new Error("User.EmailAlreadyInUse", "The email address is already in use.");
        public static readonly Error InvalidRefreshToken = new Error("Invalid.RefreshToken", "Refresh token is not founded.");
        public static readonly Error InvalidOperation = new Error("Invalid.Operation", "Invaild operation try again.");
        public static readonly Error WeakPassword = new Error("User.WeakPassword", "The provided password does not meet the security requirements.");
        public static readonly Error UnauthorizedAccess = new Error("Access.Unauthorized", "You do not have permission to access this resource.");
        public static readonly Error EmailNotConfirmed = new Error("Access.Denied", "The email is not confirmed yet.");
        public static readonly Error AccountLockedOut = new Error("Access.Denied", "The account is locked out.");



        // Category Errors
        public static readonly Error CategoryNotFound = new Error("Category.NotFound", "Category not found");
        public static readonly Error CategoryAlreadyExists = new Error("Category.Exists", "A category with this name already exists");
        public static readonly Error CategoryHasActiveProducts = new Error("Category.HasActiveProducts", "Cannot delete a category that has active products. Reassign or remove them first");



        // Image Errors
        public static readonly Error ImageUploadFailed = new Error("ImageUpload.Failed", "Failed to upload category image");
        public static readonly Error NoImages = new Error("NoImages", "No files provided");
        public static readonly Error MaxImages = new Error("MaxImages", "Maximum 10 images allowed");

        // Product Errors
        public static readonly Error ProductNotFound = new Error("Product.NotFound", "Product not found");
        public static readonly Error InsufficientStock = new Error("Product.InsufficientStock", "Insufficient stock");


        //Cart Errors
        public static readonly Error CartNotFound = new Error("Cart NotFound", "Cart not found");
        public static readonly Error CartItemNotFound = new Error("CartItem NotFound", "Cart item not found");
        public static readonly Error CartItemQuantityLessThanZero = new Error("CartItem Invalid", "Quantity must be greater than zero");

        // General
        public static readonly Error ProblemOccured = new Error("Error", "An error occured");


        // OTP
        public static readonly Error OtpInvalidOrExpired = new("Otp.InvalidOrExpired", "OTP is invalid or has expired.");
        public static readonly Error OtpTooManyAttempts = new("Otp.TooManyAttempts", "Too many failed attempts. Please request a new OTP.");




        // Profile Errors
        public static readonly Error PhoneAlreadyInUse =
       new("User.PhoneAlreadyInUse", "This phone number is already associated with another account.");

        public static readonly Error WrongCurrentPassword =
            new("User.WrongCurrentPassword", "The current password you entered is incorrect.");

    }
}
