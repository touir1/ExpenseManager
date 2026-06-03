using Moq;
using System.Text;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Tests.Services
{
    public class CsvImportServiceTests
    {
        private static readonly Currency EurCurrency = new() { Id = 1, Code = "EUR", Name = "Euro", Symbol = "€", Decimals = 2 };
        private static readonly Category RestaurantSub = new() { Id = 11, Name = "Restaurant", ParentCategoryId = 10, IsDeleted = false };

        // GetAllActiveAsync returns only top-level categories; subcategories are in .Children (mirrors real EF behaviour)
        private static readonly Category FoodCategory = new()
        {
            Id = 10, Name = "Food", ParentCategoryId = null,
            Children = [RestaurantSub]
        };
        private static readonly Category TransportCategory = new() { Id = 20, Name = "Transport", ParentCategoryId = null };

        private static CsvImportService CreateService(
            ICurrencyRepository? currencyRepo = null,
            ICategoryRepository? categoryRepo = null,
            IFamilyRepository? familyRepo = null,
            ITagService? tagService = null,
            IExpenseService? expenseService = null)
        {
            if (currencyRepo == null)
            {
                var cr = new Mock<ICurrencyRepository>();
                cr.Setup(x => x.GetAllAsync()).ReturnsAsync([EurCurrency]);
                currencyRepo = cr.Object;
            }

            if (categoryRepo == null)
            {
                var cat = new Mock<ICategoryRepository>();
                // Only top-level categories returned — same as real GetAllActiveAsync()
                cat.Setup(x => x.GetAllActiveAsync()).ReturnsAsync([FoodCategory, TransportCategory]);
                categoryRepo = cat.Object;
            }

            if (familyRepo == null)
            {
                var fam = new Mock<IFamilyRepository>();
                fam.Setup(x => x.GetFamiliesByUserAsync(It.IsAny<int>()))
                   .ReturnsAsync([]);
                familyRepo = fam.Object;
            }

            return new CsvImportService(
                currencyRepo,
                categoryRepo,
                familyRepo,
                tagService ?? Mock.Of<ITagService>(),
                expenseService ?? Mock.Of<IExpenseService>());
        }

        private static Stream MakeCsv(string content)
            => new MemoryStream(Encoding.UTF8.GetBytes(content));

        private static string ValidRow(string? date = null, string amount = "50.00", string currency = "EUR",
            string category = "", string subcategory = "", string description = "", string tags = "", string families = "")
        {
            date ??= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)).ToString("yyyy-MM-dd");
            return $"date,amount,currency_code,category,subcategory,description,tags,families\n{date},{amount},{currency},{category},{subcategory},{description},{tags},{families}";
        }

        // ── ParseAndValidateAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task ParseAndValidateAsync_ValidRow_ReturnsIsValidTrue()
        {
            var svc = CreateService();
            var stream = MakeCsv(ValidRow());

            var result = await svc.ParseAndValidateAsync(stream, userId: 1);

            Assert.Equal(1, result.TotalRows);
            Assert.Equal(1, result.ValidCount);
            Assert.Equal(0, result.ErrorCount);
            Assert.True(result.Rows.Single().IsValid);
        }

        [Fact]
        public async Task ParseAndValidateAsync_EmptyFile_ReturnsZeroRows()
        {
            var svc = CreateService();
            var stream = MakeCsv("date,amount,currency_code,category,subcategory,description,tags,families\n");

            var result = await svc.ParseAndValidateAsync(stream, userId: 1);

            Assert.Equal(0, result.TotalRows);
        }

        [Fact]
        public async Task ParseAndValidateAsync_InvalidAmount_ReturnsAmountInvalidError()
        {
            var svc = CreateService();
            var stream = MakeCsv(ValidRow(amount: "not_a_number"));

            var result = await svc.ParseAndValidateAsync(stream, userId: 1);

            Assert.False(result.Rows.Single().IsValid);
            Assert.Contains("AMOUNT_INVALID", result.Rows.Single().Errors);
        }

        [Fact]
        public async Task ParseAndValidateAsync_NegativeAmount_ReturnsAmountMustBePositiveError()
        {
            var svc = CreateService();
            var stream = MakeCsv(ValidRow(amount: "-5.00"));

            var result = await svc.ParseAndValidateAsync(stream, userId: 1);

            Assert.False(result.Rows.Single().IsValid);
            Assert.Contains("AMOUNT_MUST_BE_POSITIVE", result.Rows.Single().Errors);
        }

        [Fact]
        public async Task ParseAndValidateAsync_FutureDate_ReturnsDateInFutureError()
        {
            var svc = CreateService();
            var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)).ToString("yyyy-MM-dd");
            var stream = MakeCsv(ValidRow(date: futureDate));

            var result = await svc.ParseAndValidateAsync(stream, userId: 1);

            Assert.False(result.Rows.Single().IsValid);
            Assert.Contains("DATE_IN_FUTURE", result.Rows.Single().Errors);
        }

        [Fact]
        public async Task ParseAndValidateAsync_BadDateFormat_ReturnsDateInvalidError()
        {
            var svc = CreateService();
            var stream = MakeCsv(ValidRow(date: "15/01/2025"));

            var result = await svc.ParseAndValidateAsync(stream, userId: 1);

            Assert.False(result.Rows.Single().IsValid);
            Assert.Contains("DATE_INVALID", result.Rows.Single().Errors);
        }

        [Fact]
        public async Task ParseAndValidateAsync_UnknownCurrency_ReturnsCurrencyNotFoundError()
        {
            var svc = CreateService();
            var stream = MakeCsv(ValidRow(currency: "XYZ"));

            var result = await svc.ParseAndValidateAsync(stream, userId: 1);

            Assert.False(result.Rows.Single().IsValid);
            Assert.Contains("CURRENCY_NOT_FOUND", result.Rows.Single().Errors);
        }

        [Fact]
        public async Task ParseAndValidateAsync_UnknownCategory_ReturnsCategoryNotFoundError()
        {
            var svc = CreateService();
            var stream = MakeCsv(ValidRow(category: "NonExistent"));

            var result = await svc.ParseAndValidateAsync(stream, userId: 1);

            Assert.False(result.Rows.Single().IsValid);
            Assert.Contains("CATEGORY_NOT_FOUND", result.Rows.Single().Errors);
        }

        [Fact]
        public async Task ParseAndValidateAsync_SubcategoryWithoutCategory_ReturnsSubcategoryInvalidError()
        {
            var svc = CreateService();
            var stream = MakeCsv(ValidRow(category: "", subcategory: "Restaurant"));

            var result = await svc.ParseAndValidateAsync(stream, userId: 1);

            Assert.False(result.Rows.Single().IsValid);
            Assert.Contains("SUBCATEGORY_INVALID", result.Rows.Single().Errors);
        }

        [Fact]
        public async Task ParseAndValidateAsync_ValidCategoryAndSubcategory_ResolvesIds()
        {
            var svc = CreateService();
            var stream = MakeCsv(ValidRow(category: "Food", subcategory: "Restaurant"));

            var result = await svc.ParseAndValidateAsync(stream, userId: 1);

            var row = result.Rows.Single();
            Assert.True(row.IsValid);
            Assert.Equal(10, row.CategoryId);
            Assert.Equal(11, row.SubcategoryId);
        }

        [Fact]
        public async Task ParseAndValidateAsync_MixedRows_ReturnsCorrectCounts()
        {
            var svc = CreateService();
            var validDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)).ToString("yyyy-MM-dd");
            var csv = $"date,amount,currency_code,category,subcategory,description,tags,families\n" +
                      $"{validDate},50.00,EUR,,,,,\n" +
                      $"{validDate},bad_amount,EUR,,,,,\n" +
                      $"{validDate},30.00,EUR,,,,,";

            var result = await svc.ParseAndValidateAsync(MakeCsv(csv), userId: 1);

            Assert.Equal(3, result.TotalRows);
            Assert.Equal(2, result.ValidCount);
            Assert.Equal(1, result.ErrorCount);
        }

        [Fact]
        public async Task ParseAndValidateAsync_FamilyByName_ResolvesId_WhenMember()
        {
            var family = new Family { Id = 3, Name = "My Family", IsDefault = false, IsDeleted = false };
            var membership = new FamilyMembership { Family = family };
            var famRepo = new Mock<IFamilyRepository>();
            famRepo.Setup(x => x.GetFamiliesByUserAsync(It.IsAny<int>()))
                   .ReturnsAsync([(family, membership)]);

            var svc = CreateService(familyRepo: famRepo.Object);
            var stream = MakeCsv(ValidRow(families: "My Family"));

            var result = await svc.ParseAndValidateAsync(stream, userId: 1);

            var row = result.Rows.Single();
            Assert.True(row.IsValid);
            Assert.Equal([3], row.FamilyIds);
        }

        [Fact]
        public async Task ParseAndValidateAsync_FamilyByName_CaseInsensitive()
        {
            var family = new Family { Id = 3, Name = "My Family", IsDefault = false, IsDeleted = false };
            var membership = new FamilyMembership { Family = family };
            var famRepo = new Mock<IFamilyRepository>();
            famRepo.Setup(x => x.GetFamiliesByUserAsync(It.IsAny<int>()))
                   .ReturnsAsync([(family, membership)]);

            var svc = CreateService(familyRepo: famRepo.Object);
            var stream = MakeCsv(ValidRow(families: "MY FAMILY"));

            var result = await svc.ParseAndValidateAsync(stream, userId: 1);

            var row = result.Rows.Single();
            Assert.True(row.IsValid);
            Assert.Equal([3], row.FamilyIds);
        }

        [Fact]
        public async Task ParseAndValidateAsync_FamilyByName_ReturnsError_WhenUnknownName()
        {
            var svc = CreateService(); // no families in default mock
            var stream = MakeCsv(ValidRow(families: "Unknown Family"));

            var result = await svc.ParseAndValidateAsync(stream, userId: 1);

            Assert.False(result.Rows.Single().IsValid);
            Assert.Contains("FAMILY_FORBIDDEN", result.Rows.Single().Errors);
        }

        [Fact]
        public async Task ParseAndValidateAsync_FamilyByName_ExcludesDeletedFamilies()
        {
            var family = new Family { Id = 3, Name = "Old Family", IsDefault = false, IsDeleted = true };
            var membership = new FamilyMembership { Family = family };
            var famRepo = new Mock<IFamilyRepository>();
            famRepo.Setup(x => x.GetFamiliesByUserAsync(It.IsAny<int>()))
                   .ReturnsAsync([(family, membership)]);

            var svc = CreateService(familyRepo: famRepo.Object);
            var stream = MakeCsv(ValidRow(families: "Old Family"));

            var result = await svc.ParseAndValidateAsync(stream, userId: 1);

            Assert.False(result.Rows.Single().IsValid);
            Assert.Contains("FAMILY_FORBIDDEN", result.Rows.Single().Errors);
        }

        [Fact]
        public async Task ParseAndValidateAsync_TagsValidAndParsed()
        {
            var svc = CreateService();
            var stream = MakeCsv(ValidRow(tags: "work;client"));

            var result = await svc.ParseAndValidateAsync(stream, userId: 1);

            var row = result.Rows.Single();
            Assert.True(row.IsValid);
            Assert.Equal(["work", "client"], row.TagNames);
        }

        [Fact]
        public async Task ParseAndValidateAsync_MissingRequiredHeaders_Throws()
        {
            var svc = CreateService();
            // CSV without 'currency_code' header
            var stream = MakeCsv("date,amount,category\n2025-01-01,10.00,Food");

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.ParseAndValidateAsync(stream, userId: 1));
        }

        [Fact]
        public async Task ParseAndValidateAsync_TooManyColumns_Throws()
        {
            var svc = CreateService();
            var manyHeaders = string.Join(",", Enumerable.Range(0, 25).Select(i => $"col{i}"));
            var manyValues = string.Join(",", Enumerable.Range(0, 25).Select(_ => "x"));
            var stream = MakeCsv($"{manyHeaders}\n{manyValues}");

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.ParseAndValidateAsync(stream, userId: 1));
        }

        [Fact]
        public async Task ParseAndValidateAsync_BinaryContent_Throws()
        {
            var svc = CreateService();
            // Create stream with null bytes (binary file signature)
            var bytes = new byte[512];
            bytes[0] = 0x4D; bytes[1] = 0x5A; // PE header MZ
            bytes[3] = 0x00; // null byte
            var stream = new MemoryStream(bytes);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.ParseAndValidateAsync(stream, userId: 1));
        }

        // ── ConfirmImportAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task ConfirmImportAsync_CallsAddAsync_ForEachRow()
        {
            var expenseService = new Mock<IExpenseService>();
            expenseService
                .Setup(s => s.AddAsync(It.IsAny<CreateExpenseRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new ExpenseDto { Date = DateOnly.FromDateTime(DateTime.UtcNow) });

            var svc = CreateService(expenseService: expenseService.Object);

            var rows = new[]
            {
                new CsvImportConfirmRowDto { Amount = 10m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) },
                new CsvImportConfirmRowDto { Amount = 20m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)) },
            };

            await svc.ConfirmImportAsync(rows, userId: 1);

            expenseService.Verify(s => s.AddAsync(It.IsAny<CreateExpenseRequest>(), 1, It.IsAny<int>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ConfirmImportAsync_UsesBulkWebSourceId()
        {
            const int BulkWeb = 3;
            var expenseService = new Mock<IExpenseService>();
            expenseService
                .Setup(s => s.AddAsync(It.IsAny<CreateExpenseRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new ExpenseDto { Date = DateOnly.FromDateTime(DateTime.UtcNow) });

            var svc = CreateService(expenseService: expenseService.Object);

            var rows = new[]
            {
                new CsvImportConfirmRowDto { Amount = 10m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) },
            };

            await svc.ConfirmImportAsync(rows, userId: 1);

            expenseService.Verify(s => s.AddAsync(It.IsAny<CreateExpenseRequest>(), It.IsAny<int>(), BulkWeb), Times.Once);
        }

        [Fact]
        public async Task ConfirmImportAsync_ReturnsCorrectImportedCount()
        {
            var expenseService = new Mock<IExpenseService>();
            expenseService
                .Setup(s => s.AddAsync(It.IsAny<CreateExpenseRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new ExpenseDto { Date = DateOnly.FromDateTime(DateTime.UtcNow) });

            var svc = CreateService(expenseService: expenseService.Object);

            var rows = new[]
            {
                new CsvImportConfirmRowDto { Amount = 10m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) },
                new CsvImportConfirmRowDto { Amount = 20m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)) },
                new CsvImportConfirmRowDto { Amount = 30m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)) },
            };

            var result = await svc.ConfirmImportAsync(rows, userId: 1);

            Assert.Equal(3, result.Imported);
            Assert.Equal(0, result.Skipped);
        }

        [Fact]
        public async Task ConfirmImportAsync_SkipsRowOnException()
        {
            var expenseService = new Mock<IExpenseService>();
            expenseService
                .SetupSequence(s => s.AddAsync(It.IsAny<CreateExpenseRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new ExpenseDto { Date = DateOnly.FromDateTime(DateTime.UtcNow) })
                .ThrowsAsync(new Exception("DB error"))
                .ReturnsAsync(new ExpenseDto { Date = DateOnly.FromDateTime(DateTime.UtcNow) });

            var svc = CreateService(expenseService: expenseService.Object);

            var rows = new[]
            {
                new CsvImportConfirmRowDto { Amount = 10m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) },
                new CsvImportConfirmRowDto { Amount = 20m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)) },
                new CsvImportConfirmRowDto { Amount = 30m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)) },
            };

            var result = await svc.ConfirmImportAsync(rows, userId: 1);

            Assert.Equal(2, result.Imported);
            Assert.Equal(1, result.Skipped);
        }

        [Fact]
        public async Task ConfirmImportAsync_CallsUseTagAsync_ForEachTagName()
        {
            var tagService = new Mock<ITagService>();
            tagService.Setup(t => t.UseTagAsync(It.IsAny<string>(), It.IsAny<int>()))
                      .ReturnsAsync((string name, int _) => new TagDto { Id = 99, Name = name });

            var expenseService = new Mock<IExpenseService>();
            expenseService
                .Setup(s => s.AddAsync(It.IsAny<CreateExpenseRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new ExpenseDto { Date = DateOnly.FromDateTime(DateTime.UtcNow) });

            var svc = CreateService(tagService: tagService.Object, expenseService: expenseService.Object);

            var rows = new[]
            {
                new CsvImportConfirmRowDto
                {
                    Amount = 10m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    TagNames = ["work", "client"]
                },
            };

            await svc.ConfirmImportAsync(rows, userId: 1);

            tagService.Verify(t => t.UseTagAsync("work", 1), Times.Once);
            tagService.Verify(t => t.UseTagAsync("client", 1), Times.Once);
        }

        // ── ValidateRowsAsync ──────────────────────────────────────────────────

        private static RawCsvRowDto MakeRawRow(
            int rowNumber = 1,
            string? date = null,
            string amount = "50.00",
            string currencyCode = "EUR",
            string category = "",
            string subcategory = "",
            string description = "",
            string tags = "",
            string families = "")
        {
            date ??= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)).ToString("yyyy-MM-dd");
            return new RawCsvRowDto
            {
                RowNumber = rowNumber,
                Date = date,
                Amount = amount,
                CurrencyCode = currencyCode,
                Category = category,
                Subcategory = subcategory,
                Description = description,
                Tags = tags,
                Families = families,
            };
        }

        [Fact]
        public async Task ValidateRowsAsync_ValidRow_ReturnsIsValidTrue()
        {
            var svc = CreateService();
            var result = await svc.ValidateRowsAsync([MakeRawRow()], userId: 1);

            Assert.Equal(1, result.ValidCount);
            Assert.True(result.Rows.Single().IsValid);
        }

        [Fact]
        public async Task ValidateRowsAsync_FixedRow_ChangesFromInvalidToValid()
        {
            var svc = CreateService();

            // first pass: invalid amount
            var invalidRow = MakeRawRow(amount: "bad");
            var firstResult = await svc.ValidateRowsAsync([invalidRow], userId: 1);
            Assert.False(firstResult.Rows.Single().IsValid);

            // second pass: fixed amount
            var fixedRow = MakeRawRow(amount: "25.00");
            var secondResult = await svc.ValidateRowsAsync([fixedRow], userId: 1);
            Assert.True(secondResult.Rows.Single().IsValid);
        }

        [Fact]
        public async Task ValidateRowsAsync_PreservesRowNumbers()
        {
            var svc = CreateService();
            var rows = new[]
            {
                MakeRawRow(rowNumber: 3, amount: "10.00"),
                MakeRawRow(rowNumber: 7, amount: "20.00"),
            };

            var result = await svc.ValidateRowsAsync(rows, userId: 1);

            Assert.Equal([3, 7], result.Rows.Select(r => r.RowNumber).ToArray());
        }

        [Fact]
        public async Task ValidateRowsAsync_EmptyRows_ReturnsZeroCounts()
        {
            var svc = CreateService();
            var result = await svc.ValidateRowsAsync([], userId: 1);

            Assert.Equal(0, result.TotalRows);
            Assert.Equal(0, result.ValidCount);
            Assert.Equal(0, result.ErrorCount);
        }

        [Fact]
        public async Task ValidateRowsAsync_InvalidCurrency_ReturnsCurrencyNotFoundError()
        {
            var svc = CreateService();
            var result = await svc.ValidateRowsAsync([MakeRawRow(currencyCode: "XYZ")], userId: 1);

            Assert.False(result.Rows.Single().IsValid);
            Assert.Contains("CURRENCY_NOT_FOUND", result.Rows.Single().Errors);
        }

        [Fact]
        public async Task ValidateRowsAsync_TooManyTags_ReturnsTooManyTagsError()
        {
            var svc = CreateService();
            var manyTags = string.Join(";", Enumerable.Range(0, 21).Select(i => $"tag{i}"));
            var result = await svc.ValidateRowsAsync([MakeRawRow(tags: manyTags)], userId: 1);

            Assert.False(result.Rows.Single().IsValid);
            Assert.Contains("TOO_MANY_TAGS", result.Rows.Single().Errors);
        }

        [Fact]
        public async Task ValidateRowsAsync_TagNameTooLong_ReturnsTagNameTooLongError()
        {
            var svc = CreateService();
            var longTag = new string('a', 101);
            var result = await svc.ValidateRowsAsync([MakeRawRow(tags: longTag)], userId: 1);

            Assert.False(result.Rows.Single().IsValid);
            Assert.Contains("TAG_NAME_TOO_LONG", result.Rows.Single().Errors);
        }

        [Fact]
        public async Task ValidateRowsAsync_ValidTagsWithinLimits_ReturnsIsValidTrue()
        {
            var svc = CreateService();
            var tags = string.Join(";", Enumerable.Range(0, 5).Select(i => $"tag{i}"));
            var result = await svc.ValidateRowsAsync([MakeRawRow(tags: tags)], userId: 1);

            Assert.True(result.Rows.Single().IsValid);
        }
    }
}
