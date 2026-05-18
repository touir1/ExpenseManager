using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Models.External;
using Touir.ExpensesManager.Expenses.Repositories;
using Touir.ExpensesManager.Expenses.Tests.TestHelpers;

namespace Touir.ExpensesManager.Expenses.Tests.Repositories
{
    public class DashboardRepositoryTests : IDisposable
    {
        private readonly TestExpensesDbContextWrapper _wrapper;
        private readonly DashboardRepository _sut;

        private const int UserId = 1;
        private const int OtherUserId = 2;
        private const int CurrencyId = 2000;
        private const int AltCurrencyId = 2001;
        private const int FamilyId = 100;

        public DashboardRepositoryTests()
        {
            _wrapper = new TestExpensesDbContextWrapper();
            _sut = new DashboardRepository(_wrapper.Context);
        }

        public void Dispose()
        {
            _wrapper.Dispose();
            GC.SuppressFinalize(this);
        }

        private async Task SeedBaseAsync()
        {
            _wrapper.Context.Users.AddRange(
                new User { Id = UserId, FirstName = "A", LastName = "B", Email = "a@test.com", IsDeleted = false },
                new User { Id = OtherUserId, FirstName = "C", LastName = "D", Email = "c@test.com", IsDeleted = false });
            _wrapper.Context.Currencies.AddRange(
                new Currency { Id = CurrencyId, Code = "EUR", Name = "Euro", Symbol = "€", Decimals = 2 },
                new Currency { Id = AltCurrencyId, Code = "USD", Name = "Dollar", Symbol = "$", Decimals = 2 });
            await _wrapper.Context.SaveChangesAsync();
        }

        private async Task SeedFamilyAsync()
        {
            _wrapper.Context.Families.Add(new Family
            {
                Id = FamilyId, Name = "Test", IsDefault = false, CreatedAt = DateTime.UtcNow, CreatedById = UserId
            });
            _wrapper.Context.FamilyMemberships.Add(new FamilyMembership
            {
                FamilyId = FamilyId, UserId = UserId, RoleId = 1, JoinedAt = DateTime.UtcNow
            });
            await _wrapper.Context.SaveChangesAsync();
        }

        private Expense BuildExpense(int userId = UserId, decimal amount = 50m,
            int currencyId = CurrencyId, DateOnly? date = null, int? categoryId = null, int? subId = null)
            => new()
            {
                UserId = userId,
                Amount = amount,
                CurrencyId = currencyId,
                Date = date ?? new DateOnly(2026, 1, 15),
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId,
                CreatedFromId = 1,
                CategoryId = categoryId,
                SubcategoryId = subId
            };

        private async Task<Expense> AddExpenseAsync(Expense e)
        {
            _wrapper.Context.Expenses.Add(e);
            await _wrapper.Context.SaveChangesAsync();
            return e;
        }

        private async Task AttributeToFamilyAsync(long expenseId)
        {
            _wrapper.Context.ExpenseFamilyAttributions.Add(new ExpenseFamilyAttribution
            {
                ExpenseId = expenseId, FamilyId = FamilyId, AttributedAt = DateTime.UtcNow, AttributedById = UserId
            });
            await _wrapper.Context.SaveChangesAsync();
        }

        // ── GetTotalsAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task GetTotalsAsync_ReturnsEmpty_WhenNoExpenses()
        {
            await SeedBaseAsync();
            var result = await _sut.GetTotalsAsync(UserId, null, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTotalsAsync_ReturnsTotalAndCount_FilteredByDateRange()
        {
            await SeedBaseAsync();
            await AddExpenseAsync(BuildExpense(amount: 100m, date: new DateOnly(2026, 1, 10)));
            await AddExpenseAsync(BuildExpense(amount: 200m, date: new DateOnly(2026, 2, 10)));

            var result = await _sut.GetTotalsAsync(UserId, null, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));

            Assert.Single(result);
            Assert.Equal(100m, result.First().Amount);
            Assert.Equal(1, result.First().Count);
        }

        [Fact]
        public async Task GetTotalsAsync_ExcludesSoftDeletedExpenses()
        {
            await SeedBaseAsync();
            var e = await AddExpenseAsync(BuildExpense(amount: 100m));
            e.IsDeleted = true;
            e.DeletedAt = DateTime.UtcNow;
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetTotalsAsync(UserId, null, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTotalsAsync_FiltersToUserId_WhenFamilyIdNull()
        {
            await SeedBaseAsync();
            await AddExpenseAsync(BuildExpense(userId: UserId, amount: 100m));
            await AddExpenseAsync(BuildExpense(userId: OtherUserId, amount: 200m));

            var result = await _sut.GetTotalsAsync(UserId, null, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));

            Assert.Single(result);
            Assert.Equal(100m, result.First().Amount);
        }

        [Fact]
        public async Task GetTotalsAsync_FiltersToAttribution_WhenFamilyIdProvided()
        {
            await SeedBaseAsync();
            await SeedFamilyAsync();
            var e1 = await AddExpenseAsync(BuildExpense(userId: UserId, amount: 100m));
            await AddExpenseAsync(BuildExpense(userId: UserId, amount: 200m)); // not attributed
            await AttributeToFamilyAsync(e1.Id);

            var result = await _sut.GetTotalsAsync(UserId, FamilyId, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));

            Assert.Single(result);
            Assert.Equal(100m, result.First().Amount);
        }

        [Fact]
        public async Task GetTotalsAsync_GroupsByCurrencyId()
        {
            await SeedBaseAsync();
            await AddExpenseAsync(BuildExpense(amount: 100m, currencyId: CurrencyId));
            await AddExpenseAsync(BuildExpense(amount: 50m, currencyId: AltCurrencyId));
            await AddExpenseAsync(BuildExpense(amount: 75m, currencyId: CurrencyId));

            var result = (await _sut.GetTotalsAsync(UserId, null, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31))).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(175m, result.First(r => r.CurrencyId == CurrencyId).Amount);
            Assert.Equal(50m, result.First(r => r.CurrencyId == AltCurrencyId).Amount);
        }

        // ── GetCategoryTotalsAsync ────────────────────────────────────────────────

        [Fact]
        public async Task GetCategoryTotalsAsync_GroupsByCategoryAndSubcategory()
        {
            await SeedBaseAsync();
            _wrapper.Context.Categories.AddRange(
                new Category { Id = 9010, Name = "TestFood" },
                new Category { Id = 9011, Name = "TestRestaurants", ParentCategoryId = 9010 });
            await _wrapper.Context.SaveChangesAsync();

            await AddExpenseAsync(BuildExpense(amount: 30m, categoryId: 9010, subId: 9011));
            await AddExpenseAsync(BuildExpense(amount: 20m, categoryId: 9010));

            var result = (await _sut.GetCategoryTotalsAsync(UserId, null, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31))).ToList();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.CategoryId == 9010 && r.SubcategoryId == 9011 && r.Amount == 30m);
            Assert.Contains(result, r => r.CategoryId == 9010 && r.SubcategoryId == null && r.Amount == 20m);
        }

        [Fact]
        public async Task GetCategoryTotalsAsync_IncludesNullCategory_ForUncategorised()
        {
            await SeedBaseAsync();
            await AddExpenseAsync(BuildExpense(amount: 55m)); // no category

            var result = (await _sut.GetCategoryTotalsAsync(UserId, null, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31))).ToList();

            Assert.Single(result);
            Assert.Null(result.First().CategoryId);
        }

        // ── GetMonthlyTotalsAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task GetMonthlyTotalsAsync_GroupsByYearAndMonth()
        {
            await SeedBaseAsync();
            await AddExpenseAsync(BuildExpense(amount: 100m, date: new DateOnly(2025, 11, 1)));
            await AddExpenseAsync(BuildExpense(amount: 200m, date: new DateOnly(2025, 12, 1)));
            await AddExpenseAsync(BuildExpense(amount: 300m, date: new DateOnly(2026, 1, 1)));

            var result = (await _sut.GetMonthlyTotalsAsync(UserId, null, new DateOnly(2025, 1, 1), new DateOnly(2026, 12, 31))).ToList();

            Assert.Equal(3, result.Count);
            Assert.Contains(result, r => r.Year == 2025 && r.Month == 11 && r.Amount == 100m);
            Assert.Contains(result, r => r.Year == 2025 && r.Month == 12 && r.Amount == 200m);
            Assert.Contains(result, r => r.Year == 2026 && r.Month == 1 && r.Amount == 300m);
        }

        [Fact]
        public async Task GetMonthlyTotalsAsync_DoesNotLeakExpensesOutsideDateRange()
        {
            await SeedBaseAsync();
            await AddExpenseAsync(BuildExpense(amount: 100m, date: new DateOnly(2026, 1, 15)));
            await AddExpenseAsync(BuildExpense(amount: 200m, date: new DateOnly(2026, 3, 15)));

            var result = await _sut.GetMonthlyTotalsAsync(UserId, null, new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 28));

            Assert.Single(result);
        }

        // ── GetMonthlyCategoryTotalsAsync ─────────────────────────────────────────

        [Fact]
        public async Task GetMonthlyCategoryTotalsAsync_GroupsByYearMonthAndCategory()
        {
            await SeedBaseAsync();
            _wrapper.Context.Categories.Add(new Category { Id = 9020, Name = "TestTravel" });
            await _wrapper.Context.SaveChangesAsync();

            await AddExpenseAsync(BuildExpense(amount: 100m, date: new DateOnly(2026, 1, 5), categoryId: 9020));
            await AddExpenseAsync(BuildExpense(amount: 50m, date: new DateOnly(2026, 1, 10)));

            var result = (await _sut.GetMonthlyCategoryTotalsAsync(UserId, null, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31))).ToList();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Year == 2026 && r.Month == 1 && r.CategoryId == 9020 && r.Amount == 100m);
            Assert.Contains(result, r => r.Year == 2026 && r.Month == 1 && r.CategoryId == null && r.Amount == 50m);
        }

        // ── GetYearlyTotalsForMonthAsync ──────────────────────────────────────────

        [Fact]
        public async Task GetYearlyTotalsForMonthAsync_FiltersToSpecifiedMonth()
        {
            await SeedBaseAsync();
            await AddExpenseAsync(BuildExpense(amount: 100m, date: new DateOnly(2024, 3, 15)));
            await AddExpenseAsync(BuildExpense(amount: 200m, date: new DateOnly(2025, 3, 10)));
            await AddExpenseAsync(BuildExpense(amount: 999m, date: new DateOnly(2025, 7, 1))); // different month

            var result = (await _sut.GetYearlyTotalsForMonthAsync(UserId, null, 3)).ToList();

            Assert.Equal(2, result.Count);
            Assert.DoesNotContain(result, r => r.Amount == 999m);
        }

        [Fact]
        public async Task GetYearlyTotalsForMonthAsync_GroupsByYear()
        {
            await SeedBaseAsync();
            await AddExpenseAsync(BuildExpense(amount: 100m, date: new DateOnly(2024, 3, 1)));
            await AddExpenseAsync(BuildExpense(amount: 150m, date: new DateOnly(2024, 3, 20)));
            await AddExpenseAsync(BuildExpense(amount: 200m, date: new DateOnly(2025, 3, 5)));

            var result = (await _sut.GetYearlyTotalsForMonthAsync(UserId, null, 3)).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(250m, result.First(r => r.Year == 2024).Amount);
            Assert.Equal(200m, result.First(r => r.Year == 2025).Amount);
        }
    }
}
