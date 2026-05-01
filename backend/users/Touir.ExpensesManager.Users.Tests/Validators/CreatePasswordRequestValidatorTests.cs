using FluentValidation.TestHelper;
using Touir.ExpensesManager.Users.Controllers.Requests;
using Touir.ExpensesManager.Users.Validators;

namespace Touir.ExpensesManager.Users.Tests.Validators
{
    public class CreatePasswordRequestValidatorTests
    {
        private readonly CreatePasswordRequestValidator _validator = new();

        [Fact]
        public void ShouldHaveError_WhenEmailIsEmpty()
        {
            var result = _validator.TestValidate(new CreatePasswordRequest { Email = "", VerificationHash = "hash", NewPassword = "newPassword1" });
            result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("MISSING_PARAMETERS");
        }

        [Fact]
        public void ShouldHaveError_WhenVerificationHashIsEmpty()
        {
            var result = _validator.TestValidate(new CreatePasswordRequest { Email = "john@doe.com", VerificationHash = "", NewPassword = "newPassword1" });
            result.ShouldHaveValidationErrorFor(x => x.VerificationHash).WithErrorMessage("MISSING_PARAMETERS");
        }

        [Fact]
        public void ShouldHaveError_WhenVerificationHashIsNull()
        {
            var result = _validator.TestValidate(new CreatePasswordRequest { Email = "john@doe.com", VerificationHash = null, NewPassword = "newPassword1" });
            result.ShouldHaveValidationErrorFor(x => x.VerificationHash).WithErrorMessage("MISSING_PARAMETERS");
        }

        [Fact]
        public void ShouldHaveError_WhenNewPasswordIsEmpty()
        {
            var result = _validator.TestValidate(new CreatePasswordRequest { Email = "john@doe.com", VerificationHash = "hash", NewPassword = "" });
            result.ShouldHaveValidationErrorFor(x => x.NewPassword).WithErrorMessage("MISSING_PARAMETERS");
        }

        [Fact]
        public void ShouldHaveError_WhenNewPasswordTooShort()
        {
            var result = _validator.TestValidate(new CreatePasswordRequest { Email = "john@doe.com", VerificationHash = "hash", NewPassword = "short" });
            result.ShouldHaveValidationErrorFor(x => x.NewPassword).WithErrorMessage("PASSWORD_TOO_SHORT");
        }

        [Fact]
        public void ShouldNotHaveErrors_WhenAllFieldsValid()
        {
            var result = _validator.TestValidate(new CreatePasswordRequest { Email = "john@doe.com", VerificationHash = "hash", NewPassword = "newPassword1" });
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
