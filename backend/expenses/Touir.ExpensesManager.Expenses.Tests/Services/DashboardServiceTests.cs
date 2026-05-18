using Moq;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Tests.Services
{
    public class DashboardServiceTests
    {
        private const int UserId = 1;
        private const int FamilyId = 10;
        private const int CurrencyId = 100;
        private const int DisplayCurrencyId = 200;

        private static readonly DateOnly DateFrom = new(2026, 1, 1);
        private static readonly DateOnly DateTo = new(2026, 1, 31);

        private readonly Mock<IDashboardRepository> _repo = new();
        private readonly Mock<IFamilyRepository> _familyRepo = new();
        private readonly Mock<ICurrencyRateService> _rateService = new();
        private readonly Mock<ICurrencyRepository> _currencyRepo = new();
        private readonly Mock<ICategoryRepository> _categoryRepo = new();
        private readonly Mock<IExpenseService> _expenseService = new();

        private DashboardService CreateSut() => new(
            _repo.Object, _familyRepo.Object, _rateService.Object,
            _currencyRepo.Object, _categoryRepo.Object, _expenseService.Object);

        private void SetupEmptyRepo()
        {
            _repo.Setup(r => r.GetTotalsAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                 .ReturnsAsync([]);
            _repo.Setup(r => r.GetCategoryTotalsAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                 .ReturnsAsync([]);
        }

        private void SetupCurrencies()
        {
            _currencyRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(
            [
                new Currency { Id = CurrencyId, Code = "EUR", Name = "Euro", Symbol = "€", Decimals = 2 },
                new Currency { Id = DisplayCurrencyId, Code = "USD", Name = "Dollar", Symbol = "$", Decimals = 2 }
            ]);
        }

        private void SetupCategories(IEnumerable<Category>? cats = null)
        {
            _categoryRepo.Setup(r => r.GetAllActiveAsync())
                         .ReturnsAsync(cats ?? []);
        }

        // ── GetSummaryAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetSummaryAsync_ReturnsZeroes_WhenNoExpenses()
        {
            SetupEmptyRepo();
            SetupCategories();
            SetupCurrencies();

            var result = await CreateSut().GetSummaryAsync(UserId, null, DateFrom, DateTo, null);

            Assert.Equal(0m, result.TotalAmount);
            Assert.Equal(0, result.ExpenseCount);
            Assert.Null(result.ConvertedTotal);
            Assert.Null(result.TopCategory);
        }

        [Fact]
        public async Task GetSummaryAsync_ReturnsTotalAndCount_SingleCurrency_NoConversion()
        {
            _repo.Setup(r => r.GetTotalsAsync(UserId, null, DateFrom, DateTo))
                 .ReturnsAsync([new CurrencyTotalRow(CurrencyId, 300m, 3)]);
            _repo.Setup(r => r.GetTotalsAsync(UserId, null, It.Is<DateOnly>(d => d < DateFrom), It.IsAny<DateOnly>()))
                 .ReturnsAsync([]);
            _repo.Setup(r => r.GetCategoryTotalsAsync(UserId, null, DateFrom, DateTo)).ReturnsAsync([]);
            SetupCategories();
            SetupCurrencies();

            var result = await CreateSut().GetSummaryAsync(UserId, null, DateFrom, DateTo, null);

            Assert.Equal(300m, result.TotalAmount);
            Assert.Equal(3, result.ExpenseCount);
            Assert.Null(result.ConvertedTotal);
        }

        [Fact]
        public async Task GetSummaryAsync_ReturnsConvertedTotal_WhenDisplayCurrencyProvided()
        {
            _repo.Setup(r => r.GetTotalsAsync(UserId, null, DateFrom, DateTo))
                 .ReturnsAsync([new CurrencyTotalRow(CurrencyId, 100m, 1)]);
            _repo.Setup(r => r.GetTotalsAsync(UserId, null, It.Is<DateOnly>(d => d < DateFrom), It.IsAny<DateOnly>()))
                 .ReturnsAsync([]);
            _repo.Setup(r => r.GetCategoryTotalsAsync(UserId, null, DateFrom, DateTo)).ReturnsAsync([]);
            _rateService.Setup(r => r.ResolveRateAsync(CurrencyId, DisplayCurrencyId, DateTo)).ReturnsAsync(1.1m);
            SetupCategories();
            SetupCurrencies();

            var result = await CreateSut().GetSummaryAsync(UserId, null, DateFrom, DateTo, DisplayCurrencyId);

            Assert.Equal(110m, result.ConvertedTotal);
            Assert.NotNull(result.DisplayCurrency);
            Assert.Equal("USD", result.DisplayCurrency!.Code);
        }

        [Fact]
        public async Task GetSummaryAsync_ReturnsNullConvertedTotal_WhenRateUnavailable()
        {
            _repo.Setup(r => r.GetTotalsAsync(UserId, null, DateFrom, DateTo))
                 .ReturnsAsync([new CurrencyTotalRow(CurrencyId, 100m, 1)]);
            _repo.Setup(r => r.GetTotalsAsync(UserId, null, It.Is<DateOnly>(d => d < DateFrom), It.IsAny<DateOnly>()))
                 .ReturnsAsync([]);
            _repo.Setup(r => r.GetCategoryTotalsAsync(UserId, null, DateFrom, DateTo)).ReturnsAsync([]);
            _rateService.Setup(r => r.ResolveRateAsync(CurrencyId, DisplayCurrencyId, DateTo)).ReturnsAsync((decimal?)null);
            SetupCategories();
            SetupCurrencies();

            var result = await CreateSut().GetSummaryAsync(UserId, null, DateFrom, DateTo, DisplayCurrencyId);

            Assert.Null(result.ConvertedTotal);
        }

        [Fact]
        public async Task GetSummaryAsync_CalculatesPositiveDelta_WhenCurrentHigherThanPrevious()
        {
            _repo.Setup(r => r.GetTotalsAsync(UserId, null, DateFrom, DateTo))
                 .ReturnsAsync([new CurrencyTotalRow(CurrencyId, 200m, 2)]);
            _repo.Setup(r => r.GetTotalsAsync(UserId, null, It.Is<DateOnly>(d => d < DateFrom), It.IsAny<DateOnly>()))
                 .ReturnsAsync([new CurrencyTotalRow(CurrencyId, 100m, 1)]);
            _repo.Setup(r => r.GetCategoryTotalsAsync(UserId, null, DateFrom, DateTo)).ReturnsAsync([]);
            _rateService.Setup(r => r.ResolveRateAsync(CurrencyId, DisplayCurrencyId, It.IsAny<DateOnly>())).ReturnsAsync(1m);
            SetupCategories();
            SetupCurrencies();

            var result = await CreateSut().GetSummaryAsync(UserId, null, DateFrom, DateTo, DisplayCurrencyId);

            Assert.Equal(100m, result.ChangePercent);
        }

        [Fact]
        public async Task GetSummaryAsync_CalculatesNegativeDelta_WhenCurrentLowerThanPrevious()
        {
            _repo.Setup(r => r.GetTotalsAsync(UserId, null, DateFrom, DateTo))
                 .ReturnsAsync([new CurrencyTotalRow(CurrencyId, 50m, 1)]);
            _repo.Setup(r => r.GetTotalsAsync(UserId, null, It.Is<DateOnly>(d => d < DateFrom), It.IsAny<DateOnly>()))
                 .ReturnsAsync([new CurrencyTotalRow(CurrencyId, 100m, 2)]);
            _repo.Setup(r => r.GetCategoryTotalsAsync(UserId, null, DateFrom, DateTo)).ReturnsAsync([]);
            _rateService.Setup(r => r.ResolveRateAsync(CurrencyId, DisplayCurrencyId, It.IsAny<DateOnly>())).ReturnsAsync(1m);
            SetupCategories();
            SetupCurrencies();

            var result = await CreateSut().GetSummaryAsync(UserId, null, DateFrom, DateTo, DisplayCurrencyId);

            Assert.Equal(-50m, result.ChangePercent);
        }

        [Fact]
        public async Task GetSummaryAsync_ReturnsNullDelta_WhenPreviousPeriodIsZero()
        {
            _repo.Setup(r => r.GetTotalsAsync(UserId, null, DateFrom, DateTo))
                 .ReturnsAsync([new CurrencyTotalRow(CurrencyId, 100m, 1)]);
            _repo.Setup(r => r.GetTotalsAsync(UserId, null, It.Is<DateOnly>(d => d < DateFrom), It.IsAny<DateOnly>()))
                 .ReturnsAsync([]);
            _repo.Setup(r => r.GetCategoryTotalsAsync(UserId, null, DateFrom, DateTo)).ReturnsAsync([]);
            _rateService.Setup(r => r.ResolveRateAsync(CurrencyId, DisplayCurrencyId, DateTo)).ReturnsAsync(1m);
            SetupCategories();
            SetupCurrencies();

            var result = await CreateSut().GetSummaryAsync(UserId, null, DateFrom, DateTo, DisplayCurrencyId);

            Assert.Null(result.ChangePercent);
        }

        [Fact]
        public async Task GetSummaryAsync_IdentifiesTopCategory_ByConvertedAmount()
        {
            var cat10 = new Category { Id = 10, Name = "Food" };
            var cat20 = new Category { Id = 20, Name = "Travel" };

            _repo.Setup(r => r.GetTotalsAsync(UserId, null, DateFrom, DateTo))
                 .ReturnsAsync([new CurrencyTotalRow(CurrencyId, 300m, 3)]);
            _repo.Setup(r => r.GetTotalsAsync(UserId, null, It.Is<DateOnly>(d => d < DateFrom), It.IsAny<DateOnly>()))
                 .ReturnsAsync([]);
            _repo.Setup(r => r.GetCategoryTotalsAsync(UserId, null, DateFrom, DateTo))
                 .ReturnsAsync([
                     new CategoryTotalRow(10, null, CurrencyId, 100m),
                     new CategoryTotalRow(20, null, CurrencyId, 200m)
                 ]);
            _rateService.Setup(r => r.ResolveRateAsync(CurrencyId, DisplayCurrencyId, DateTo)).ReturnsAsync(1m);
            SetupCategories([cat10, cat20]);
            SetupCurrencies();

            var result = await CreateSut().GetSummaryAsync(UserId, null, DateFrom, DateTo, DisplayCurrencyId);

            Assert.NotNull(result.TopCategory);
            Assert.Equal("Travel", result.TopCategory!.Name);
            Assert.Equal(200m, result.TopCategoryAmount);
        }

        [Fact]
        public async Task GetSummaryAsync_ThrowsFamilyForbidden_WhenNotMember()
        {
            _familyRepo.Setup(r => r.IsMemberAsync(FamilyId, UserId)).ReturnsAsync(false);

            await Assert.ThrowsAsync<FamilyForbiddenException>(
                () => CreateSut().GetSummaryAsync(UserId, FamilyId, DateFrom, DateTo, null));
        }

        // ── GetMonthlyAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetMonthlyAsync_GroupsByYearMonth_OrderedAscending()
        {
            _repo.Setup(r => r.GetMonthlyTotalsAsync(UserId, null, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                 .ReturnsAsync([
                     new MonthlyTotalRow(2026, 3, CurrencyId, 300m),
                     new MonthlyTotalRow(2026, 1, CurrencyId, 100m),
                     new MonthlyTotalRow(2026, 2, CurrencyId, 200m)
                 ]);
            _repo.Setup(r => r.GetMonthlyCategoryTotalsAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                 .ReturnsAsync([]);
            SetupCategories();

            var result = (await CreateSut().GetMonthlyAsync(UserId, null, DateFrom, new DateOnly(2026, 3, 31), null)).ToList();

            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[0].Month);
            Assert.Equal(2, result[1].Month);
            Assert.Equal(3, result[2].Month);
        }

        [Fact]
        public async Task GetMonthlyAsync_UsesLastDayOfMonthAsRateDate()
        {
            DateOnly capturedRateDate = default;
            _repo.Setup(r => r.GetMonthlyTotalsAsync(UserId, null, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                 .ReturnsAsync([new MonthlyTotalRow(2026, 2, CurrencyId, 100m)]);
            _repo.Setup(r => r.GetMonthlyCategoryTotalsAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                 .ReturnsAsync([]);
            _rateService.Setup(r => r.ResolveRateAsync(CurrencyId, DisplayCurrencyId, It.IsAny<DateOnly>()))
                        .Callback<int, int, DateOnly>((_, _, d) => capturedRateDate = d)
                        .ReturnsAsync(1m);
            SetupCategories();
            SetupCurrencies();

            await CreateSut().GetMonthlyAsync(UserId, null, new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28), DisplayCurrencyId);

            Assert.Equal(new DateOnly(2026, 2, 28), capturedRateDate);
        }

        [Fact]
        public async Task GetMonthlyAsync_PopulatesCategoryBreakdown_WithinEachMonth()
        {
            var cat = new Category { Id = 5, Name = "Food" };
            _repo.Setup(r => r.GetMonthlyTotalsAsync(UserId, null, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                 .ReturnsAsync([new MonthlyTotalRow(2026, 1, CurrencyId, 150m)]);
            _repo.Setup(r => r.GetMonthlyCategoryTotalsAsync(UserId, null, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                 .ReturnsAsync([
                     new MonthlyCategoryTotalRow(2026, 1, 5, CurrencyId, 100m),
                     new MonthlyCategoryTotalRow(2026, 1, null, CurrencyId, 50m)
                 ]);
            SetupCategories([cat]);

            var result = (await CreateSut().GetMonthlyAsync(UserId, null, DateFrom, DateTo, null)).ToList();

            Assert.Single(result);
            Assert.Equal(2, result[0].ByCategory.Count());
        }

        // ── GetCategoriesAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task GetCategoriesAsync_GroupsSubcategoriesUnderParent()
        {
            var parent = new Category { Id = 1, Name = "Food" };
            var sub = new Category { Id = 2, Name = "Restaurants", ParentCategoryId = 1 };

            _repo.Setup(r => r.GetCategoryTotalsAsync(UserId, null, DateFrom, DateTo))
                 .ReturnsAsync([
                     new CategoryTotalRow(1, 2, CurrencyId, 80m),
                     new CategoryTotalRow(1, null, CurrencyId, 20m)
                 ]);
            SetupCategories([parent, sub]);

            var result = (await CreateSut().GetCategoriesAsync(UserId, null, DateFrom, DateTo, null)).ToList();

            Assert.Single(result);
            Assert.Equal("Food", result[0].Category!.Name);
            Assert.Equal(2, result[0].Subcategories.Count());
        }

        [Fact]
        public async Task GetCategoriesAsync_IncludesUncategorisedGroup_WhenNullCategoryExpensesExist()
        {
            _repo.Setup(r => r.GetCategoryTotalsAsync(UserId, null, DateFrom, DateTo))
                 .ReturnsAsync([
                     new CategoryTotalRow(null, null, CurrencyId, 50m)
                 ]);
            SetupCategories();

            var result = (await CreateSut().GetCategoriesAsync(UserId, null, DateFrom, DateTo, null)).ToList();

            Assert.Single(result);
            Assert.Null(result[0].Category);
        }

        [Fact]
        public async Task GetCategoriesAsync_CalculatesPercentages_BasedOnConvertedTotals()
        {
            _repo.Setup(r => r.GetCategoryTotalsAsync(UserId, null, DateFrom, DateTo))
                 .ReturnsAsync([
                     new CategoryTotalRow(1, null, CurrencyId, 75m),
                     new CategoryTotalRow(2, null, CurrencyId, 25m)
                 ]);
            SetupCategories([new Category { Id = 1, Name = "A" }, new Category { Id = 2, Name = "B" }]);

            var result = (await CreateSut().GetCategoriesAsync(UserId, null, DateFrom, DateTo, null)).ToList();

            var catA = result.First(r => r.Category?.Name == "A");
            var catB = result.First(r => r.Category?.Name == "B");
            Assert.Equal(75m, catA.Percentage);
            Assert.Equal(25m, catB.Percentage);
        }

        // ── GetSameMonthAcrossYearsAsync ──────────────────────────────────────────

        [Fact]
        public async Task GetSameMonthAcrossYearsAsync_GroupsByYear()
        {
            _repo.Setup(r => r.GetYearlyTotalsForMonthAsync(UserId, null, 3))
                 .ReturnsAsync([
                     new YearlyTotalRow(2024, CurrencyId, 100m),
                     new YearlyTotalRow(2025, CurrencyId, 200m)
                 ]);
            SetupCurrencies();

            var result = (await CreateSut().GetSameMonthAcrossYearsAsync(UserId, null, 3, null)).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(2024, result[0].Year);
            Assert.Equal(2025, result[1].Year);
        }

        [Fact]
        public async Task GetSameMonthAcrossYearsAsync_UsesDecember31AsRateDate_ForPastYears()
        {
            DateOnly capturedRateDate = default;
            _repo.Setup(r => r.GetYearlyTotalsForMonthAsync(UserId, null, 3))
                 .ReturnsAsync([new YearlyTotalRow(2024, CurrencyId, 100m)]);
            _rateService.Setup(r => r.ResolveRateAsync(CurrencyId, DisplayCurrencyId, It.IsAny<DateOnly>()))
                        .Callback<int, int, DateOnly>((_, _, d) => capturedRateDate = d)
                        .ReturnsAsync(1m);
            SetupCurrencies();

            await CreateSut().GetSameMonthAcrossYearsAsync(UserId, null, 3, DisplayCurrencyId);

            Assert.Equal(new DateOnly(2024, 12, 31), capturedRateDate);
        }

        // ── GetByCurrencyAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task GetByCurrencyAsync_ReturnsTotalsAndCountPerCurrency()
        {
            _repo.Setup(r => r.GetTotalsAsync(UserId, null, DateFrom, DateTo))
                 .ReturnsAsync([
                     new CurrencyTotalRow(CurrencyId, 150m, 3),
                     new CurrencyTotalRow(DisplayCurrencyId, 50m, 1)
                 ]);
            SetupCurrencies();

            var result = (await CreateSut().GetByCurrencyAsync(UserId, null, DateFrom, DateTo, null)).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(3, result.First(r => r.Currency.Id == CurrencyId).ExpenseCount);
        }

        // ── GetRecentAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task GetRecentAsync_DelegatesToExpenseService_WithPageSize10()
        {
            ExpenseFilterDto? capturedFilter = null;
            var expected = new ExpensePagedResult { Items = [], TotalCount = 0, Page = 1, PageSize = 10, TotalPages = 0 };
            _expenseService.Setup(s => s.GetPagedAsync(It.IsAny<ExpenseFilterDto>(), UserId))
                           .Callback<ExpenseFilterDto, int>((f, _) => capturedFilter = f)
                           .ReturnsAsync(expected);

            var result = await CreateSut().GetRecentAsync(UserId, null, DateFrom, DateTo);

            Assert.Same(expected, result);
            Assert.NotNull(capturedFilter);
            Assert.Equal(1, capturedFilter!.Page);
            Assert.Equal(10, capturedFilter.PageSize);
            Assert.Equal(DateFrom, capturedFilter.DateFrom);
        }

        [Fact]
        public async Task GetRecentAsync_ThrowsFamilyForbidden_WhenFamilyIdSetAndNotMember()
        {
            _familyRepo.Setup(r => r.IsMemberAsync(FamilyId, UserId)).ReturnsAsync(false);

            await Assert.ThrowsAsync<FamilyForbiddenException>(
                () => CreateSut().GetRecentAsync(UserId, FamilyId, null, null));
        }
    }
}
