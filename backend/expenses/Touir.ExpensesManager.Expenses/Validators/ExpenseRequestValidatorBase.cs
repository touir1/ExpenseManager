using FluentValidation;
using Touir.ExpensesManager.Expenses.Controllers.Requests;

namespace Touir.ExpensesManager.Expenses.Validators
{
    public abstract class ExpenseRequestValidatorBase<T> : AbstractValidator<T>
        where T : IExpenseRequest
    {
        protected ExpenseRequestValidatorBase()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("AMOUNT_MUST_BE_POSITIVE");

            RuleFor(x => x.CurrencyId)
                .GreaterThan(0).WithMessage("INVALID_CURRENCY");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("DATE_REQUIRED")
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("DATE_IN_FUTURE");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("DESCRIPTION_TOO_LONG")
                .When(x => x.Description != null);

            RuleFor(x => x.SubcategoryId)
                .Must((req, subId) => subId == null || req.CategoryId != null)
                .WithMessage("SUBCATEGORY_REQUIRES_CATEGORY");
        }
    }
}
