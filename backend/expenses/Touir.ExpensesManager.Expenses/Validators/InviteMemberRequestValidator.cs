using FluentValidation;
using Touir.ExpensesManager.Expenses.Controllers.Requests;

namespace Touir.ExpensesManager.Expenses.Validators
{
    public class InviteMemberRequestValidator : AbstractValidator<InviteMemberRequest>
    {
        public InviteMemberRequestValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("EMAIL_REQUIRED")
                .EmailAddress().WithMessage("EMAIL_INVALID")
                .MaximumLength(256).WithMessage("EMAIL_TOO_LONG");
        }
    }
}
