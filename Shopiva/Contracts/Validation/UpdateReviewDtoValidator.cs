using FluentValidation;
using Shopiva.Contracts.Reviews;

namespace Shopiva.Contracts.Validation
{
    public class UpdateReviewDtoValidator : AbstractValidator<UpdateReviewDto>
    {
        public UpdateReviewDtoValidator()
        {
            RuleFor(x => x)
                .Must(x => x.Rating.HasValue || x.Comment is not null)
                .WithMessage("At least one field (Rating or Comment) must be provided.")
                .OverridePropertyName("UpdateReviewDto");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.")
                .When(x => x.Rating.HasValue);

            RuleFor(x => x.Comment)
                .MaximumLength(1000).WithMessage("Comment must not exceed 1000 characters.")
                .When(x => x.Comment is not null);
        }
    }
}
