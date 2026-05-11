using FluentValidation;
using Touir.ExpensesManager.Expenses.Controllers.Requests;

namespace Touir.ExpensesManager.Expenses.Validators
{
    public class ChangeMemberRoleRequestValidator : AbstractValidator<ChangeMemberRoleRequest>
    {
        private static readonly HashSet<string> ValidRoles = new(StringComparer.OrdinalIgnoreCase) { "Head", "Member" };

        public ChangeMemberRoleRequestValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("ROLE_REQUIRED")
                .Must(r => ValidRoles.Contains(r)).WithMessage("ROLE_INVALID");
        }
    }
}
