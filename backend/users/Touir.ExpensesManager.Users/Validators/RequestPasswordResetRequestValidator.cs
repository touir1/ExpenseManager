using FluentValidation;
using Touir.ExpensesManager.Users.Controllers.Requests;

namespace Touir.ExpensesManager.Users.Validators
{
    public class RequestPasswordResetRequestValidator : AbstractValidator<RequestPasswordResetRequest>
    {
        public RequestPasswordResetRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("MISSING_PARAMETERS");
            RuleFor(x => x.AppCode).NotEmpty().WithMessage("MISSING_PARAMETERS");
        }
    }
}
