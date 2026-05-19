namespace Shopiva.Contracts.Validation
{
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
        private const long MaxImageSize = 5 * 1024 * 1024;

        public CreateProductDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .Length(2, 200).WithMessage("Product name must be between 2 and 200 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(x => x.DiscountedPrice)
                .GreaterThan(0).WithMessage("Discounted price must be greater than 0.")
                .LessThan(x => x.Price).WithMessage("Discounted price must be less than the original price.")
                .When(x => x.DiscountedPrice.HasValue);

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("A valid Category is required.");

            RuleFor(x => x.Images)
                .Must(imgs => imgs!.Count <= 10)
                    .WithMessage("You can upload a maximum of 10 images.")
                .When(x => x.Images is not null && x.Images.Count > 0);

            RuleForEach(x => x.Images)
                .ChildRules(img =>
                {
                    img.RuleFor(f => f.Length)
                        .GreaterThan(0).WithMessage("Image file cannot be empty.")
                        .LessThanOrEqualTo(MaxImageSize).WithMessage("Each image must be less than 5 MB.");

                    img.RuleFor(f => f.ContentType)
                        .Must(ct => AllowedImageTypes.Contains(ct.ToLower()))
                        .WithMessage("Only JPEG, PNG, and WebP images are allowed.");
                })
                .When(x => x.Images is not null && x.Images.Count > 0);
        }
    }
}