using FluentValidation;
using Touir.ExpensesManager.Expenses.Controllers.Requests;

namespace Touir.ExpensesManager.Expenses.Validators
{
    public class AdminAddCurrencyRequestValidator : AbstractValidator<AdminAddCurrencyRequest>
    {
        public AdminAddCurrencyRequestValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("CURRENCY_CODE_REQUIRED")
                .Length(3).WithMessage("CURRENCY_CODE_MUST_BE_3_CHARS");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("CURRENCY_NAME_REQUIRED")
                .MaximumLength(50).WithMessage("CURRENCY_NAME_TOO_LONG");

            RuleFor(x => x.Symbol)
                .NotEmpty().WithMessage("CURRENCY_SYMBOL_REQUIRED")
                .MaximumLength(10).WithMessage("CURRENCY_SYMBOL_TOO_LONG");
        }
    }
}
