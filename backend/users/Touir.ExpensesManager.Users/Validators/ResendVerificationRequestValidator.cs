using FluentValidation;
using Touir.ExpensesManager.Users.Controllers.Requests;

namespace Touir.ExpensesManager.Users.Validators
{
    public class ResendVerificationRequestValidator : AbstractValidator<ResendVerificationRequest>
    {
        public ResendVerificationRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("MISSING_PARAMETERS");
            RuleFor(x => x.ApplicationCode).NotEmpty().WithMessage("MISSING_PARAMETERS");
        }
    }
}
