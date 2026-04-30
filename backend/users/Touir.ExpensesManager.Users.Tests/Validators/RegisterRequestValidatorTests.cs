using FluentValidation.TestHelper;
using Touir.ExpensesManager.Users.Controllers.Requests;
using Touir.ExpensesManager.Users.Validators;

namespace Touir.ExpensesManager.Users.Tests.Validators
{
    public class RegisterRequestValidatorTests
    {
        private readonly RegisterRequestValidator _validator = new();

        [Fact]
        public void ShouldHaveError_WhenFirstNameIsEmpty()
        {
            var result = _validator.TestValidate(new RegisterRequest { FirstName = "", LastName = "Doe", Email = "john@doe.com" });
            result.ShouldHaveValidationErrorFor(x => x.FirstName).WithErrorMessage("MISSING_PARAMETERS");
        }

        [Fact]
        public void ShouldHaveError_WhenLastNameIsEmpty()
        {
            var result = _validator.TestValidate(new RegisterRequest { FirstName = "John", LastName = "", Email = "john@doe.com" });
            result.ShouldHaveValidationErrorFor(x => x.LastName).WithErrorMessage("MISSING_PARAMETERS");
        }

        [Fact]
        public void ShouldHaveError_WhenEmailIsEmpty()
        {
            var result = _validator.TestValidate(new RegisterRequest { FirstName = "John", LastName = "Doe", Email = "" });
            result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("MISSING_PARAMETERS");
        }

        [Fact]
        public void ShouldNotHaveErrors_WhenAllRequiredFieldsValid()
        {
            var result = _validator.TestValidate(new RegisterRequest { FirstName = "John", LastName = "Doe", Email = "john@doe.com" });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void ShouldNotHaveErrors_WhenApplicationCodeIsNull()
        {
            var result = _validator.TestValidate(new RegisterRequest { FirstName = "John", LastName = "Doe", Email = "john@doe.com", ApplicationCode = null });
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
