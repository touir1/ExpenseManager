using FluentValidation.TestHelper;
using Touir.ExpensesManager.Users.Controllers.Requests;
using Touir.ExpensesManager.Users.Validators;

namespace Touir.ExpensesManager.Users.Tests.Validators
{
    public class RequestPasswordResetRequestValidatorTests
    {
        private readonly RequestPasswordResetRequestValidator _validator = new();

        [Fact]
        public void ShouldHaveError_WhenEmailIsEmpty()
        {
            var result = _validator.TestValidate(new RequestPasswordResetRequest { Email = "", AppCode = "APP1" });
            result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("MISSING_PARAMETERS");
        }

        [Fact]
        public void ShouldHaveError_WhenAppCodeIsEmpty()
        {
            var result = _validator.TestValidate(new RequestPasswordResetRequest { Email = "john@doe.com", AppCode = "" });
            result.ShouldHaveValidationErrorFor(x => x.AppCode).WithErrorMessage("MISSING_PARAMETERS");
        }

        [Fact]
        public void ShouldNotHaveErrors_WhenAllFieldsValid()
        {
            var result = _validator.TestValidate(new RequestPasswordResetRequest { Email = "john@doe.com", AppCode = "APP1" });
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
