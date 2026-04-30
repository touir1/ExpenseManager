using FluentValidation.TestHelper;
using Touir.ExpensesManager.Users.Controllers.Requests;
using Touir.ExpensesManager.Users.Validators;

namespace Touir.ExpensesManager.Users.Tests.Validators
{
    public class LoginRequestValidatorTests
    {
        private readonly LoginRequestValidator _validator = new();

        [Fact]
        public void ShouldHaveError_WhenApplicationCodeIsEmpty()
        {
            var result = _validator.TestValidate(new LoginRequest { ApplicationCode = "", Email = "john@doe.com", Password = "password" });
            result.ShouldHaveValidationErrorFor(x => x.ApplicationCode).WithErrorMessage("MISSING_PARAMETERS");
        }

        [Fact]
        public void ShouldHaveError_WhenApplicationCodeIsNull()
        {
            var result = _validator.TestValidate(new LoginRequest { ApplicationCode = null, Email = "john@doe.com", Password = "password" });
            result.ShouldHaveValidationErrorFor(x => x.ApplicationCode).WithErrorMessage("MISSING_PARAMETERS");
        }

        [Fact]
        public void ShouldHaveError_WhenEmailIsEmpty()
        {
            var result = _validator.TestValidate(new LoginRequest { ApplicationCode = "APP1", Email = "", Password = "password" });
            result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("MISSING_PARAMETERS");
        }

        [Fact]
        public void ShouldHaveError_WhenEmailIsNull()
        {
            var result = _validator.TestValidate(new LoginRequest { ApplicationCode = "APP1", Email = null, Password = "password" });
            result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("MISSING_PARAMETERS");
        }

        [Fact]
        public void ShouldHaveError_WhenPasswordIsEmpty()
        {
            var result = _validator.TestValidate(new LoginRequest { ApplicationCode = "APP1", Email = "john@doe.com", Password = "" });
            result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage("MISSING_PARAMETERS");
        }

        [Fact]
        public void ShouldHaveError_WhenPasswordIsNull()
        {
            var result = _validator.TestValidate(new LoginRequest { ApplicationCode = "APP1", Email = "john@doe.com", Password = null });
            result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage("MISSING_PARAMETERS");
        }

        [Fact]
        public void ShouldNotHaveErrors_WhenAllFieldsValid()
        {
            var result = _validator.TestValidate(new LoginRequest { ApplicationCode = "APP1", Email = "john@doe.com", Password = "password" });
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
