using FluentValidation;
using Touir.ExpensesManager.Expenses.Controllers.Requests;

namespace Touir.ExpensesManager.Expenses.Validators
{
    public class AdminCategoryRequestValidator : AbstractValidator<AdminCategoryRequest>
    {
        public AdminCategoryRequestValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("CATEGORY_NAME_REQUIRED")
                .MaximumLength(100).WithMessage("CATEGORY_NAME_TOO_LONG");
        }
    }
}
