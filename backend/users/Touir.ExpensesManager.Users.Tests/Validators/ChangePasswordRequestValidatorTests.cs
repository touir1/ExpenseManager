using FluentValidation.TestHelper;
using Touir.ExpensesManager.Users.Controllers.Requests;
using Touir.ExpensesManager.Users.Validators;

namespace Touir.ExpensesManager.Users.Tests.Validators
{
    public class ChangePasswordRequestValidatorTests
    {
        private readonly ChangePasswordRequestValidator _validator = new();

        [Fact]
        public void ShouldHaveError_WhenEmailIsEmpty()
        {
            var result = _validator.TestValidate(new ChangePasswordRequest { Email = "", OldPassword = "old", NewPassword = "newPassword1", ConfirmPassword = "newPassword1" });
            result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("MISSING_PARAMETERS");
        }

        [Fact]
        public void ShouldHaveError_WhenOldPasswordIsEmpty()
        {
            var result = _validator.TestValidate(new ChangePasswordRequest { Email = "john@doe.com", OldPassword = "", NewPassword = "newPassword1", ConfirmPassword = "newPassword1" });
            result.ShouldHaveValidationErrorFor(x => x.OldPassword).WithErrorMessage("MISSING_PARAMETERS");
        }

        [Fact]
        public void ShouldHaveError_WhenNewPasswordIsEmpty()
        {
            var result = _validator.TestValidate(new ChangePasswordRequest { Email = "john@doe.com", OldPassword = "old", NewPassword = "", ConfirmPassword = "newPassword1" });
            result.ShouldHaveValidationErrorFor(x => x.NewPassword).WithErrorMessage("MISSING_PARAMETERS");
        }

        [Fact]
        public void ShouldHaveError_WhenNewPasswordTooShort()
        {
            var result = _validator.TestValidate(new ChangePasswordRequest { Email = "john@doe.com", OldPassword = "old", NewPassword = "short", ConfirmPassword = "short" });
            result.ShouldHaveValidationErrorFor(x => x.NewPassword).WithErrorMessage("PASSWORD_TOO_SHORT");
        }

        [Fact]
        public void ShouldNotHaveErrors_WhenAllFieldsValid()
        {
            var result = _validator.TestValidate(new ChangePasswordRequest { Email = "john@doe.com", OldPassword = "old", NewPassword = "newPassword1" });
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
