using FluentValidation;
using Touir.ExpensesManager.Users.Controllers.Requests;

namespace Touir.ExpensesManager.Users.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("MISSING_PARAMETERS")
                .MaximumLength(100).WithMessage("FIELD_TOO_LONG");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("MISSING_PARAMETERS")
                .MaximumLength(100).WithMessage("FIELD_TOO_LONG");
            RuleFor(x => x.Email).NotEmpty().WithMessage("MISSING_PARAMETERS")
                .MaximumLength(100).WithMessage("FIELD_TOO_LONG")
                .EmailAddress().WithMessage("INVALID_EMAIL_FORMAT");
        }
    }
}
