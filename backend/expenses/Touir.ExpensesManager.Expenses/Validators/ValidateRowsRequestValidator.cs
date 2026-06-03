using FluentValidation;
using Touir.ExpensesManager.Expenses.Controllers.Requests;

namespace Touir.ExpensesManager.Expenses.Validators
{
    public class ValidateRowsRequestValidator : AbstractValidator<ValidateRowsRequest>
    {
        private const int MaxRows = 500;

        public ValidateRowsRequestValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Rows)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("IMPORT_NO_ROWS")
                .Must(r => r.Count() <= MaxRows).WithMessage("IMPORT_TOO_MANY_ROWS");

            RuleForEach(x => x.Rows).ChildRules(row =>
            {
                row.RuleFor(r => r.Date).MaximumLength(10);
                row.RuleFor(r => r.Amount).MaximumLength(30);
                row.RuleFor(r => r.CurrencyCode).MaximumLength(10);
                row.RuleFor(r => r.Category).MaximumLength(200);
                row.RuleFor(r => r.Subcategory).MaximumLength(200);
                row.RuleFor(r => r.Description).MaximumLength(500);
                row.RuleFor(r => r.Tags).MaximumLength(1000);
                row.RuleFor(r => r.Families).MaximumLength(500);
            });
        }
    }
}
