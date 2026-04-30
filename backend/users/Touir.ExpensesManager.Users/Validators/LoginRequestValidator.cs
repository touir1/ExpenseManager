using FluentValidation;
using Touir.ExpensesManager.Users.Controllers.Requests;

namespace Touir.ExpensesManager.Users.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.ApplicationCode).NotEmpty().WithMessage("MISSING_PARAMETERS");
            RuleFor(x => x.Email).NotEmpty().WithMessage("MISSING_PARAMETERS");
            RuleFor(x => x.Password).NotEmpty().WithMessage("MISSING_PARAMETERS");
        }
    }
}
