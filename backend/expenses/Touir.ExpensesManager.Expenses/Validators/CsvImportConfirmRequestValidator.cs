using FluentValidation;
using Touir.ExpensesManager.Expenses.Controllers.Requests;

namespace Touir.ExpensesManager.Expenses.Validators
{
    public class CsvImportConfirmRequestValidator : AbstractValidator<CsvImportConfirmRequest>
    {
        private const int MaxRows = 500;

        public CsvImportConfirmRequestValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Rows)
                .NotNull().WithMessage("IMPORT_NO_ROWS")
                .Must(r => r.Any()).WithMessage("IMPORT_NO_ROWS")
                .Must(r => r.Count() <= MaxRows).WithMessage("IMPORT_TOO_MANY_ROWS");

            RuleForEach(x => x.Rows).ChildRules(row =>
            {
                row.RuleFor(r => r.Amount)
                    .GreaterThan(0).WithMessage("AMOUNT_MUST_BE_POSITIVE");

                row.RuleFor(r => r.CurrencyId)
                    .GreaterThan(0).WithMessage("INVALID_CURRENCY");

                row.RuleFor(r => r.Date)
                    .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                    .WithMessage("DATE_IN_FUTURE");

                row.RuleFor(r => r.Description)
                    .MaximumLength(500).WithMessage("DESCRIPTION_TOO_LONG")
                    .When(r => r.Description != null);
            });
        }
    }
}
