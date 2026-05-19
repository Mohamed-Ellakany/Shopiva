
namespace Shopiva.Contracts.Validation
{
    public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
    {
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
        private const long MaxImageSize = 5 * 1024 * 1024; // 5 MB

        public CreateCategoryDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .Length(2, 100).WithMessage("Category name must be between 2 and 100 characters.");

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
        }
    }
}
