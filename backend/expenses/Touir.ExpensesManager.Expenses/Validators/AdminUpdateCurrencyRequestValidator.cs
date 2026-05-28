using FluentValidation;
using Touir.ExpensesManager.Expenses.Controllers.Requests;

namespace Touir.ExpensesManager.Expenses.Validators
{
    public class AdminUpdateCurrencyRequestValidator : AbstractValidator<AdminUpdateCurrencyRequest>
    {
        public AdminUpdateCurrencyRequestValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("CURRENCY_NAME_REQUIRED")
                .MaximumLength(50).WithMessage("CURRENCY_NAME_TOO_LONG");

            RuleFor(x => x.Symbol)
                .NotEmpty().WithMessage("CURRENCY_SYMBOL_REQUIRED")
                .MaximumLength(10).WithMessage("CURRENCY_SYMBOL_TOO_LONG");

            RuleFor(x => x.Decimals)
                .InclusiveBetween(0, 8).WithMessage("CURRENCY_DECIMALS_OUT_OF_RANGE");
        }
    }
}
