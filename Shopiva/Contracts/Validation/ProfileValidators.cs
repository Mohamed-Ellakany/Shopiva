using Shopiva.Contracts.Profiles;

namespace Shopiva.Contracts.Validation
{
    public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
    {
        public UpdateProfileDtoValidator()
        {
            // At least one field must be provided
            RuleFor(x => x)
                .Must(x => x.FirstName is not null || x.LastName is not null
                        || x.PhoneNumber is not null || x.Address is not null
                        || x.City is not null || x.Country is not null)
                .WithMessage("At least one field must be provided.")
                .OverridePropertyName("UpdateProfileDto");

            RuleFor(x => x.FirstName)
                .Length(2, 100).WithMessage("First name must be between 2 and 100 characters.")
                .When(x => x.FirstName is not null);

            RuleFor(x => x.LastName)
                .Length(2, 100).WithMessage("Last name must be between 2 and 100 characters.")
                .When(x => x.LastName is not null);

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[1-9]\d{6,14}$")
                .WithMessage("Phone number is not valid. Use E.164 format e.g. +201012345678.")
                .When(x => x.PhoneNumber is not null);

            RuleFor(x => x.Address)
                .MaximumLength(300).WithMessage("Address must not exceed 300 characters.")
                .When(x => x.Address is not null);

            RuleFor(x => x.City)
                .MaximumLength(100).WithMessage("City must not exceed 100 characters.")
                .When(x => x.City is not null);

            RuleFor(x => x.Country)
                .MaximumLength(100).WithMessage("Country must not exceed 100 characters.")
                .When(x => x.Country is not null);
        }
    }

    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .Matches(Regex.Password)
                .WithMessage("Password must be at least 8 characters and contain uppercase, lowercase, digit, and special character.")
                .NotEqual(x => x.CurrentPassword)
                .WithMessage("New password must be different from the current password.");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("Password confirmation is required.")
                .Equal(x => x.NewPassword)
                .WithMessage("Passwords do not match.");
        }
    }

    public class UpdateProfileImageDtoValidator : AbstractValidator<UpdateProfileImageDto>
    {
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
        private const long MaxImageSize = 5 * 1024 * 1024;

        public UpdateProfileImageDtoValidator()
        {
            // Cannot both upload and remove at the same time
            RuleFor(x => x)
                .Must(x => !(x.Image is not null && x.RemoveImage))
                .WithMessage("Cannot upload a new image and remove the existing image at the same time.")
                .OverridePropertyName("Image");

            // At least one action required
            RuleFor(x => x)
                .Must(x => x.Image is not null || x.RemoveImage)
                .WithMessage("Provide an image to upload or set RemoveImage to true.")
                .OverridePropertyName("UpdateProfileImageDto");

            RuleFor(x => x.Image)
                .Must(f => f!.Length > 0).WithMessage("Image file cannot be empty.")
                .Must(f => AllowedImageTypes.Contains(f!.ContentType.ToLower()))
                    .WithMessage("Only JPEG, PNG, and WebP images are allowed.")
                .Must(f => f!.Length <= MaxImageSize)
                    .WithMessage("Image must be less than 5 MB.")
                .When(x => x.Image is not null);
        }
    }
}
