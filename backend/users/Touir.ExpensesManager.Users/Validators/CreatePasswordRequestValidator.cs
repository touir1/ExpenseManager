using FluentValidation;
using Touir.ExpensesManager.Users.Controllers.Requests;

namespace Touir.ExpensesManager.Users.Validators
{
    public class CreatePasswordRequestValidator : AbstractValidator<CreatePasswordRequest>
    {
        public CreatePasswordRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("MISSING_PARAMETERS");
            RuleFor(x => x.VerificationHash).NotEmpty().WithMessage("MISSING_PARAMETERS");
            RuleFor(x => x.NewPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("MISSING_PARAMETERS")
                .MinimumLength(8).WithMessage("PASSWORD_TOO_SHORT");
        }
    }
}
