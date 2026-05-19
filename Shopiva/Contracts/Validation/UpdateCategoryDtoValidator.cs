namespace Shopiva.Contracts.Validation
{
    public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
    {
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
        private const long MaxImageSize = 5 * 1024 * 1024;

        public UpdateCategoryDtoValidator()
        {
            RuleFor(x => x.Name)
                .Length(2, 100).WithMessage("Category name must be between 2 and 100 characters.")
                .When(x => x.Name is not null);

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
                .When(x => x.Description is not null);

            RuleFor(x => x.Image)
                .Must(f => f!.Length > 0).WithMessage("Image file cannot be empty.")
                .Must(f => AllowedImageTypes.Contains(f!.ContentType.ToLower()))
                    .WithMessage("Only JPEG, PNG, and WebP images are allowed.")
                .Must(f => f!.Length <= MaxImageSize)
                    .WithMessage("Image must be less than 5 MB.")
                .When(x => x.Image is not null);

            RuleFor(x => x)
                .Must(x => !(x.RemoveImage && x.Image is not null))
                .WithMessage("Cannot upload a new image and remove the existing image at the same time.")
                .OverridePropertyName("Image");
        }
    }
}