using FluentValidation.TestHelper;
using Touir.ExpensesManager.Users.Controllers.Requests;
using Touir.ExpensesManager.Users.Validators;

namespace Touir.ExpensesManager.Users.Tests.Validators
{
    public class ResendVerificationRequestValidatorTests
    {
        private readonly ResendVerificationRequestValidator _validator = new();

        [Fact]
        public void ShouldHaveError_WhenEmailIsEmpty()
        {
            var result = _validator.TestValidate(new ResendVerificationRequest { Email = "", ApplicationCode = "APP1" });
            result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("MISSING_PARAMETERS");
        }

        [Fact]
        public void ShouldHaveError_WhenApplicationCodeIsEmpty()
        {
            var result = _validator.TestValidate(new ResendVerificationRequest { Email = "john@doe.com", ApplicationCode = "" });
            result.ShouldHaveValidationErrorFor(x => x.ApplicationCode).WithErrorMessage("MISSING_PARAMETERS");
        }

        [Fact]
        public void ShouldNotHaveErrors_WhenAllFieldsValid()
        {
            var result = _validator.TestValidate(new ResendVerificationRequest { Email = "john@doe.com", ApplicationCode = "APP1" });
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
