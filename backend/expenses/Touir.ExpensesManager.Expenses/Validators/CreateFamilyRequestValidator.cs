using FluentValidation;
using Touir.ExpensesManager.Expenses.Controllers.Requests;

namespace Touir.ExpensesManager.Expenses.Validators
{
    public class CreateFamilyRequestValidator : AbstractValidator<CreateFamilyRequest>
    {
        public CreateFamilyRequestValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("FAMILY_NAME_REQUIRED")
                .MaximumLength(30).WithMessage("FAMILY_NAME_TOO_LONG");
        }
    }
}
