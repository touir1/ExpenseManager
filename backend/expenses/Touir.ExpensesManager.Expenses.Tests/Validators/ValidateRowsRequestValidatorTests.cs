using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Validators;

namespace Touir.ExpensesManager.Expenses.Tests.Validators
{
    public class ValidateRowsRequestValidatorTests
    {
        private static RawCsvRowDto ValidRawRow(int rowNumber = 1) => new()
        {
            RowNumber = rowNumber,
            Date = "2025-01-01",
            Amount = "50.00",
            CurrencyCode = "EUR",
        };

        [Fact]
        public async Task Valid_PassesValidation()
        {
            var result = await new ValidateRowsRequestValidator().ValidateAsync(
                new ValidateRowsRequest { Rows = [ValidRawRow()] });
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task NullRows_FailsWithImportNoRows()
        {
            var result = await new ValidateRowsRequestValidator().ValidateAsync(
                new ValidateRowsRequest { Rows = null! });
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "IMPORT_NO_ROWS");
        }

        [Fact]
        public async Task TooManyRows_FailsWithImportTooManyRows()
        {
            var rows = Enumerable.Range(1, 501).Select(i => ValidRawRow(i));
            var result = await new ValidateRowsRequestValidator().ValidateAsync(
                new ValidateRowsRequest { Rows = rows });
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "IMPORT_TOO_MANY_ROWS");
        }

        [Fact]
        public async Task DateTooLong_FailsValidation()
        {
            var row = ValidRawRow();
            row.Date = new string('x', 11);
            var result = await new ValidateRowsRequestValidator().ValidateAsync(
                new ValidateRowsRequest { Rows = [row] });
            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task AmountTooLong_FailsValidation()
        {
            var row = ValidRawRow();
            row.Amount = new string('9', 31);
            var result = await new ValidateRowsRequestValidator().ValidateAsync(
                new ValidateRowsRequest { Rows = [row] });
            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task DescriptionTooLong_FailsValidation()
        {
            var row = ValidRawRow();
            row.Description = new string('x', 501);
            var result = await new ValidateRowsRequestValidator().ValidateAsync(
                new ValidateRowsRequest { Rows = [row] });
            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task TagsTooLong_FailsValidation()
        {
            var row = ValidRawRow();
            row.Tags = new string('x', 1001);
            var result = await new ValidateRowsRequestValidator().ValidateAsync(
                new ValidateRowsRequest { Rows = [row] });
            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task FamiliesTooLong_FailsValidation()
        {
            var row = ValidRawRow();
            row.Families = new string('x', 501);
            var result = await new ValidateRowsRequestValidator().ValidateAsync(
                new ValidateRowsRequest { Rows = [row] });
            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task MaxAllowedRows_PassesValidation()
        {
            var rows = Enumerable.Range(1, 500).Select(i => ValidRawRow(i));
            var result = await new ValidateRowsRequestValidator().ValidateAsync(
                new ValidateRowsRequest { Rows = rows });
            Assert.True(result.IsValid);
        }
    }
}
