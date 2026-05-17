using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Validators;

namespace Touir.ExpensesManager.Expenses.Tests.Validators
{
    public class CreateTagRequestValidatorTests
    {
        [Fact]
        public async Task Valid_PassesValidation()
        {
            var result = await new CreateTagRequestValidator().ValidateAsync(new CreateTagRequest { Name = "Food" });
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task EmptyName_FailsWithRequired()
        {
            var result = await new CreateTagRequestValidator().ValidateAsync(new CreateTagRequest { Name = "" });
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "TAG_NAME_REQUIRED");
        }

        [Fact]
        public async Task NameTooLong_FailsWithTooLong()
        {
            var result = await new CreateTagRequestValidator().ValidateAsync(new CreateTagRequest { Name = new string('a', 51) });
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "TAG_NAME_TOO_LONG");
        }

        [Fact]
        public async Task NameExactlyMaxLength_PassesValidation()
        {
            var result = await new CreateTagRequestValidator().ValidateAsync(new CreateTagRequest { Name = new string('a', 50) });
            Assert.True(result.IsValid);
        }
    }
}
