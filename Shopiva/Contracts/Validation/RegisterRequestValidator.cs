
using FluentValidation;

namespace Shopiva.Contracts.Validation
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator() {



            RuleFor(x => x.FirstName).Length(2, 100)
                .NotEmpty().WithMessage("First name is required");
                

            RuleFor(x => x.LastName).Length(2, 100)
                .NotEmpty().WithMessage("Last name is required");
                

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .Matches(Regex.Password)
               .WithMessage("Password must be at least 8 characters and should contains special character and lowercase character and uppercase character.");
            
            


        }
    }
}
