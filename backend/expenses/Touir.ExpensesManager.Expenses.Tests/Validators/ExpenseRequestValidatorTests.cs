using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Validators;

namespace Touir.ExpensesManager.Expenses.Tests.Validators
{
    public class ExpenseRequestValidatorTests
    {
        private static CreateExpenseRequest ValidCreate() => new()
        {
            Amount = 10m,
            CurrencyId = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        private static UpdateExpenseRequest ValidUpdate() => new()
        {
            Amount = 10m,
            CurrencyId = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // ── CreateExpenseRequestValidator ─────────────────────────────────────

        [Fact]
        public async Task Create_Valid_PassesValidation()
        {
            var result = await new CreateExpenseRequestValidator().ValidateAsync(ValidCreate());
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Create_InvalidAmount_FailsWithMessage(decimal amount)
        {
            var req = ValidCreate();
            req.Amount = amount;
            var result = await new CreateExpenseRequestValidator().ValidateAsync(req);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "AMOUNT_MUST_BE_POSITIVE");
        }

        [Fact]
        public async Task Create_InvalidCurrency_FailsWithMessage()
        {
            var req = ValidCreate();
            req.CurrencyId = 0;
            var result = await new CreateExpenseRequestValidator().ValidateAsync(req);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "INVALID_CURRENCY");
        }

        [Fact]
        public async Task Create_FutureDate_FailsWithMessage()
        {
            var req = ValidCreate();
            req.Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
            var result = await new CreateExpenseRequestValidator().ValidateAsync(req);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "DATE_IN_FUTURE");
        }

        [Fact]
        public async Task Create_DescriptionTooLong_FailsWithMessage()
        {
            var req = ValidCreate();
            req.Description = new string('x', 501);
            var result = await new CreateExpenseRequestValidator().ValidateAsync(req);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "DESCRIPTION_TOO_LONG");
        }

        [Fact]
        public async Task Create_SubcategoryWithoutCategory_FailsWithMessage()
        {
            var req = ValidCreate();
            req.SubcategoryId = 5;
            req.CategoryId = null;
            var result = await new CreateExpenseRequestValidator().ValidateAsync(req);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "SUBCATEGORY_REQUIRES_CATEGORY");
        }

        [Fact]
        public async Task Create_SubcategoryWithCategory_Passes()
        {
            var req = ValidCreate();
            req.SubcategoryId = 5;
            req.CategoryId = 2;
            var result = await new CreateExpenseRequestValidator().ValidateAsync(req);
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task Create_NullSubcategoryNoCategory_Passes()
        {
            var req = ValidCreate();
            req.SubcategoryId = null;
            req.CategoryId = null;
            var result = await new CreateExpenseRequestValidator().ValidateAsync(req);
            Assert.True(result.IsValid);
        }

        // ── UpdateExpenseRequestValidator ─────────────────────────────────────

        [Fact]
        public async Task Update_Valid_PassesValidation()
        {
            var result = await new UpdateExpenseRequestValidator().ValidateAsync(ValidUpdate());
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task Update_InvalidAmount_FailsWithMessage(decimal amount)
        {
            var req = ValidUpdate();
            req.Amount = amount;
            var result = await new UpdateExpenseRequestValidator().ValidateAsync(req);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "AMOUNT_MUST_BE_POSITIVE");
        }

        [Fact]
        public async Task Update_SubcategoryWithoutCategory_FailsWithMessage()
        {
            var req = ValidUpdate();
            req.SubcategoryId = 3;
            req.CategoryId = null;
            var result = await new UpdateExpenseRequestValidator().ValidateAsync(req);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "SUBCATEGORY_REQUIRES_CATEGORY");
        }
    }
}
