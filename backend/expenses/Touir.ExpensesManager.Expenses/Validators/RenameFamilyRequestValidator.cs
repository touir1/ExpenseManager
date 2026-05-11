using FluentValidation;
using Touir.ExpensesManager.Expenses.Controllers.Requests;

namespace Touir.ExpensesManager.Expenses.Validators
{
    public class RenameFamilyRequestValidator : AbstractValidator<RenameFamilyRequest>
    {
        public RenameFamilyRequestValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("FAMILY_NAME_REQUIRED")
                .MaximumLength(100).WithMessage("FAMILY_NAME_TOO_LONG");
        }
    }
}
