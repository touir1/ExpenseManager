using FluentValidation;
using Touir.ExpensesManager.Expenses.Controllers.Requests;

namespace Touir.ExpensesManager.Expenses.Validators
{
    public class UpdateUserConfigRequestValidator : AbstractValidator<UpdateUserConfigRequest>
    {
        public UpdateUserConfigRequestValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            When(x => x.DefaultCurrencyId.HasValue, () =>
            {
                RuleFor(x => x.DefaultCurrencyId!.Value)
                    .GreaterThan(0).WithMessage("USER_CONFIG_INVALID_CURRENCY");
            });
        }
    }
}
