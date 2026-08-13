using FluentValidation;
using Touir.ExpensesManager.Expenses.Controllers.Requests;

namespace Touir.ExpensesManager.Expenses.Validators
{
    public class CreateRecurringExpenseRequestValidator : RecurringExpenseRequestValidatorBase<CreateRecurringExpenseRequest>
    {
        public CreateRecurringExpenseRequestValidator()
        {
            RuleFor(x => x.NextDueDate)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("NEXT_DUE_DATE_IN_PAST");
        }
    }
}
