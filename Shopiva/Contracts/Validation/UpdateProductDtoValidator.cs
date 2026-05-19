using FluentValidation;
using Shopiva.Contracts.Products;

namespace Shopiva.Contracts.Validation
{
    public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
    {
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
        private const long MaxImageSize = 5 * 1024 * 1024;

        public UpdateProductDtoValidator()
        {
            RuleFor(x => x.Name)
                .Length(2, 200).WithMessage("Product name must be between 2 and 200 characters.")
                .When(x => x.Name is not null);

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
                .When(x => x.Description is not null);

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.")
                .When(x => x.Price.HasValue);

            RuleFor(x => x.DiscountedPrice)
                .GreaterThan(0).WithMessage("Discounted price must be greater than 0.")
                .LessThan(x => x.Price ?? decimal.MaxValue)
                    .WithMessage("Discounted price must be less than the original price.")
                .When(x => x.DiscountedPrice.HasValue);

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative.")
                .When(x => x.Stock.HasValue);

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category ID must be a positive number.")
                .When(x => x.CategoryId.HasValue);

            RuleFor(x => x.NewImages)
                .Must(imgs => imgs!.Count <= 10)
                    .WithMessage("You can upload a maximum of 10 images at once.")
                .When(x => x.NewImages is not null && x.NewImages.Count > 0);

            RuleForEach(x => x.NewImages)
                .ChildRules(img =>
                {
                    img.RuleFor(f => f.Length)
                        .GreaterThan(0).WithMessage("Image file cannot be empty.")
                        .LessThanOrEqualTo(MaxImageSize).WithMessage("Each image must be less than 5 MB.");

                    img.RuleFor(f => f.ContentType)
                        .Must(ct => AllowedImageTypes.Contains(ct.ToLower()))
                        .WithMessage("Only JPEG, PNG, and WebP images are allowed.");
                })
                .When(x => x.NewImages is not null && x.NewImages.Count > 0);
        }
    }
}