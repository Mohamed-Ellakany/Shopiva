namespace Shopiva.Contracts.Validation
{
    public class AssignRoleRequestValidator : AbstractValidator<AssignRoleRequest>
    {
        private static readonly string[] AllowedRoles = ["Admin", "Customer", "Seller"];

        public AssignRoleRequestValidator()
        {
            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required.")
                .Must(r => AllowedRoles.Contains(r))
                .WithMessage($"Role must be one of: {string.Join(", ", AllowedRoles)}.");
        }
    }

    public class RestrictUserRequestValidator : AbstractValidator<RestrictUserRequest>
    {
        public RestrictUserRequestValidator()
        {
            RuleFor(x => x.Reason)
                .MaximumLength(300).WithMessage("Reason must not exceed 300 characters.")
                .When(x => x.Reason is not null);

            RuleFor(x => x.Until)
                .GreaterThan(DateTime.UtcNow).WithMessage("Lockout end date must be in the future.")
                .When(x => x.Until.HasValue && x.Restrict);
        }
    }
}
