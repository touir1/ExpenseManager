using FluentValidation;
using Touir.ExpensesManager.Expenses.Controllers.Requests;

namespace Touir.ExpensesManager.Expenses.Validators
{
    public class CreateTagRequestValidator : AbstractValidator<CreateTagRequest>
    {
        public CreateTagRequestValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("TAG_NAME_REQUIRED")
                .MaximumLength(50).WithMessage("TAG_NAME_TOO_LONG");
        }
    }
}
