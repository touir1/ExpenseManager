using FluentValidation;
using Touir.ExpensesManager.Expenses.Controllers.Requests;

namespace Touir.ExpensesManager.Expenses.Validators
{
    public abstract class RecurringExpenseRequestValidatorBase<T> : AbstractValidator<T>
        where T : IRecurringExpenseRequest
    {
        protected RecurringExpenseRequestValidatorBase()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("DESCRIPTION_REQUIRED")
                .MaximumLength(500).WithMessage("DESCRIPTION_TOO_LONG");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("AMOUNT_MUST_BE_POSITIVE");

            RuleFor(x => x.CurrencyId)
                .GreaterThan(0).WithMessage("INVALID_CURRENCY");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("INVALID_CATEGORY");

            RuleFor(x => x.FrequencyId)
                .GreaterThan(0).WithMessage("INVALID_FREQUENCY");

            RuleFor(x => x.SubcategoryId)
                .Must((req, subId) => subId == null || req.CategoryId != 0)
                .WithMessage("SUBCATEGORY_REQUIRES_CATEGORY");
        }
    }
}
