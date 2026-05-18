using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Models.External;
using Touir.ExpensesManager.Expenses.Repositories;
using Touir.ExpensesManager.Expenses.Tests.TestHelpers;

namespace Touir.ExpensesManager.Expenses.Tests.Repositories
{
    public class ExpenseRepositoryTests : IDisposable
    {
        private readonly TestExpensesDbContextWrapper _wrapper;
        private readonly ExpenseRepository _sut;

        public ExpenseRepositoryTests()
        {
            _wrapper = new TestExpensesDbContextWrapper();
            _sut = new ExpenseRepository(_wrapper.Context);
        }

        public void Dispose()
        {
            _wrapper.Dispose();
            GC.SuppressFinalize(this);
        }

        private async Task SeedBaseDataAsync(int userId = 1)
        {
            _wrapper.Context.Users.Add(new User { Id = userId, FirstName = "T", LastName = "U", Email = $"u{userId}@test.com", IsDeleted = false });
            _wrapper.Context.Currencies.Add(new Currency { Id = 1000, Code = "TST", Name = "Test", Symbol = "$", Decimals = 2 });
            await _wrapper.Context.SaveChangesAsync();
        }

        private static Expense BuildExpense(int userId = 1, long id = 0) => new()
        {
            UserId = userId,
            Amount = 99m,
            CurrencyId = 1000,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = DateTime.UtcNow,
            CreatedById = userId,
            CreatedFromId = 1
        };

        [Fact]
        public async Task AddAsync_PersistsExpense()
        {
            await SeedBaseDataAsync();
            var expense = BuildExpense();

            var result = await _sut.AddAsync(expense);

            Assert.True(result.Id > 0);
            Assert.Single(_wrapper.Context.Expenses.Where(e => !e.IsDeleted));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsExpense_WhenOwned()
        {
            await SeedBaseDataAsync();
            var expense = BuildExpense();
            _wrapper.Context.Expenses.Add(expense);
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetByIdAsync(expense.Id, userId: 1);

            Assert.NotNull(result);
            Assert.Equal(expense.Id, result!.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenWrongUser()
        {
            await SeedBaseDataAsync();
            var expense = BuildExpense(userId: 1);
            _wrapper.Context.Expenses.Add(expense);
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetByIdAsync(expense.Id, userId: 99);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenSoftDeleted()
        {
            await SeedBaseDataAsync();
            var expense = BuildExpense();
            expense.IsDeleted = true;
            expense.DeletedAt = DateTime.UtcNow;
            _wrapper.Context.Expenses.Add(expense);
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetByIdAsync(expense.Id, userId: 1);

            Assert.Null(result);
        }

        [Fact]
        public async Task SoftDeleteAsync_SetsIsDeletedAndDeletedAt()
        {
            await SeedBaseDataAsync();
            var expense = BuildExpense();
            _wrapper.Context.Expenses.Add(expense);
            await _wrapper.Context.SaveChangesAsync();

            await _sut.SoftDeleteAsync(expense);

            var updated = _wrapper.Context.Expenses.Find(expense.Id);
            Assert.NotNull(updated);
            Assert.True(updated!.IsDeleted);
            Assert.NotNull(updated.DeletedAt);
        }

        [Fact]
        public async Task GetPagedAsync_ExcludesSoftDeleted()
        {
            await SeedBaseDataAsync();
            var active = BuildExpense();
            var deleted = BuildExpense();
            deleted.IsDeleted = true;
            deleted.DeletedAt = DateTime.UtcNow;
            _wrapper.Context.Expenses.AddRange(active, deleted);
            await _wrapper.Context.SaveChangesAsync();

            var (items, total) = await _sut.GetPagedAsync(new ExpenseFilterDto { Page = 1, PageSize = 20 }, userId: 1);

            Assert.Equal(1, total);
            Assert.Single(items);
        }

        [Fact]
        public async Task GetPagedAsync_FiltersOtherUsersExpenses()
        {
            await SeedBaseDataAsync(userId: 1);
            _wrapper.Context.Users.Add(new User { Id = 2, FirstName = "B", LastName = "U", Email = "u2@test.com", IsDeleted = false });
            await _wrapper.Context.SaveChangesAsync();

            _wrapper.Context.Expenses.AddRange(BuildExpense(userId: 1), BuildExpense(userId: 2));
            await _wrapper.Context.SaveChangesAsync();

            var (items, total) = await _sut.GetPagedAsync(new ExpenseFilterDto { Page = 1, PageSize = 20 }, userId: 1);

            Assert.Equal(1, total);
        }

        [Fact]
        public async Task GetPagedAsync_PaginatesCorrectly()
        {
            await SeedBaseDataAsync();
            for (int i = 0; i < 5; i++)
                _wrapper.Context.Expenses.Add(BuildExpense());
            await _wrapper.Context.SaveChangesAsync();

            var (items, total) = await _sut.GetPagedAsync(new ExpenseFilterDto { Page = 2, PageSize = 2 }, userId: 1);

            Assert.Equal(5, total);
            Assert.Equal(2, items.Count());
        }

        [Fact]
        public async Task UpdateAsync_PersistsChanges()
        {
            await SeedBaseDataAsync();
            var expense = BuildExpense();
            _wrapper.Context.Expenses.Add(expense);
            await _wrapper.Context.SaveChangesAsync();
            _wrapper.Context.ChangeTracker.Clear();

            expense.Amount = 500m;
            expense.ModifiedAt = DateTime.UtcNow;
            expense.ModifiedById = 1;
            expense.ModifiedFromId = 1;

            await _sut.UpdateAsync(expense);

            var updated = _wrapper.Context.Expenses.Find(expense.Id);
            Assert.Equal(500m, updated!.Amount);
        }

        [Fact]
        public async Task ClearExpenseTagsAsync_RemovesAllTagsForExpense()
        {
            await SeedBaseDataAsync();
            var expense = BuildExpense();
            _wrapper.Context.Expenses.Add(expense);
            var tag = new Touir.ExpensesManager.Expenses.Models.Tag { Name = "food" };
            _wrapper.Context.Tags.Add(tag);
            await _wrapper.Context.SaveChangesAsync();
            _wrapper.Context.ExpenseTags.Add(new Touir.ExpensesManager.Expenses.Models.ExpenseTag { ExpenseId = expense.Id, TagId = tag.Id });
            await _wrapper.Context.SaveChangesAsync();

            await _sut.ClearExpenseTagsAsync(expense.Id);

            Assert.Empty(_wrapper.Context.ExpenseTags.Where(et => et.ExpenseId == expense.Id));
        }

        [Fact]
        public async Task AddExpenseTagsAsync_PersistsExpenseTags()
        {
            await SeedBaseDataAsync();
            var expense = BuildExpense();
            _wrapper.Context.Expenses.Add(expense);
            var tag = new Touir.ExpensesManager.Expenses.Models.Tag { Name = "travel" };
            _wrapper.Context.Tags.Add(tag);
            await _wrapper.Context.SaveChangesAsync();

            await _sut.AddExpenseTagsAsync([new Touir.ExpensesManager.Expenses.Models.ExpenseTag { ExpenseId = expense.Id, TagId = tag.Id }]);

            Assert.Single(_wrapper.Context.ExpenseTags.Where(et => et.ExpenseId == expense.Id));
        }

        // ── GetPagedAsync filter branches ─────────────────────────────────────────

        [Fact]
        public async Task GetPagedAsync_FiltersDateFrom()
        {
            await SeedBaseDataAsync();
            var old = BuildExpense(); old.Date = new DateOnly(2020, 1, 1);
            var recent = BuildExpense(); recent.Date = new DateOnly(2024, 6, 1);
            _wrapper.Context.Expenses.AddRange(old, recent);
            await _wrapper.Context.SaveChangesAsync();

            var (items, total) = await _sut.GetPagedAsync(
                new ExpenseFilterDto { Page = 1, PageSize = 20, DateFrom = new DateOnly(2024, 1, 1) }, userId: 1);

            Assert.Equal(1, total);
        }

        [Fact]
        public async Task GetPagedAsync_FiltersDateTo()
        {
            await SeedBaseDataAsync();
            var old = BuildExpense(); old.Date = new DateOnly(2020, 1, 1);
            var recent = BuildExpense(); recent.Date = new DateOnly(2024, 6, 1);
            _wrapper.Context.Expenses.AddRange(old, recent);
            await _wrapper.Context.SaveChangesAsync();

            var (items, total) = await _sut.GetPagedAsync(
                new ExpenseFilterDto { Page = 1, PageSize = 20, DateTo = new DateOnly(2022, 1, 1) }, userId: 1);

            Assert.Equal(1, total);
        }

        [Fact]
        public async Task GetPagedAsync_FiltersCurrencyId()
        {
            await SeedBaseDataAsync();
            _wrapper.Context.Currencies.Add(new Touir.ExpensesManager.Expenses.Models.Currency { Id = 1001, Code = "EUR", Name = "Euro", Symbol = "€", Decimals = 2 });
            await _wrapper.Context.SaveChangesAsync();

            var e1 = BuildExpense(); e1.CurrencyId = 1000;
            var e2 = BuildExpense(); e2.CurrencyId = 1001;
            _wrapper.Context.Expenses.AddRange(e1, e2);
            await _wrapper.Context.SaveChangesAsync();

            var (items, total) = await _sut.GetPagedAsync(
                new ExpenseFilterDto { Page = 1, PageSize = 20, CurrencyId = 1001 }, userId: 1);

            Assert.Equal(1, total);
        }

        [Fact]
        public async Task GetPagedAsync_FiltersAmountMin()
        {
            await SeedBaseDataAsync();
            var cheap = BuildExpense(); cheap.Amount = 5m;
            var expensive = BuildExpense(); expensive.Amount = 500m;
            _wrapper.Context.Expenses.AddRange(cheap, expensive);
            await _wrapper.Context.SaveChangesAsync();

            var (items, total) = await _sut.GetPagedAsync(
                new ExpenseFilterDto { Page = 1, PageSize = 20, AmountMin = 100m }, userId: 1);

            Assert.Equal(1, total);
        }

        [Fact]
        public async Task GetPagedAsync_FiltersAmountMax()
        {
            await SeedBaseDataAsync();
            var cheap = BuildExpense(); cheap.Amount = 5m;
            var expensive = BuildExpense(); expensive.Amount = 500m;
            _wrapper.Context.Expenses.AddRange(cheap, expensive);
            await _wrapper.Context.SaveChangesAsync();

            var (items, total) = await _sut.GetPagedAsync(
                new ExpenseFilterDto { Page = 1, PageSize = 20, AmountMax = 10m }, userId: 1);

            Assert.Equal(1, total);
        }

        [Fact]
        public async Task GetPagedAsync_FiltersDescription()
        {
            await SeedBaseDataAsync();
            var withDesc = BuildExpense(); withDesc.Description = "lunch at cafe";
            var noDesc = BuildExpense(); noDesc.Description = "taxi";
            _wrapper.Context.Expenses.AddRange(withDesc, noDesc);
            await _wrapper.Context.SaveChangesAsync();

            var (items, total) = await _sut.GetPagedAsync(
                new ExpenseFilterDto { Page = 1, PageSize = 20, Description = "lunch" }, userId: 1);

            Assert.Equal(1, total);
        }

        [Fact]
        public async Task GetPagedAsync_FiltersTagIds()
        {
            await SeedBaseDataAsync();
            var tag = new Touir.ExpensesManager.Expenses.Models.Tag { Name = "work" };
            _wrapper.Context.Tags.Add(tag);
            await _wrapper.Context.SaveChangesAsync();

            var tagged = BuildExpense();
            var untagged = BuildExpense();
            _wrapper.Context.Expenses.AddRange(tagged, untagged);
            await _wrapper.Context.SaveChangesAsync();

            _wrapper.Context.ExpenseTags.Add(new Touir.ExpensesManager.Expenses.Models.ExpenseTag { ExpenseId = tagged.Id, TagId = tag.Id });
            await _wrapper.Context.SaveChangesAsync();

            var (items, total) = await _sut.GetPagedAsync(
                new ExpenseFilterDto { Page = 1, PageSize = 20, TagIds = [tag.Id] }, userId: 1);

            Assert.Equal(1, total);
        }

        [Fact]
        public async Task GetPagedAsync_FiltersCategoryId()
        {
            await SeedBaseDataAsync();
            var cat = new Touir.ExpensesManager.Expenses.Models.Category { Name = "Food" };
            _wrapper.Context.Categories.Add(cat);
            await _wrapper.Context.SaveChangesAsync();

            var withCat = BuildExpense(); withCat.CategoryId = cat.Id;
            var noCat = BuildExpense();
            _wrapper.Context.Expenses.AddRange(withCat, noCat);
            await _wrapper.Context.SaveChangesAsync();

            var (items, total) = await _sut.GetPagedAsync(
                new ExpenseFilterDto { Page = 1, PageSize = 20, CategoryId = cat.Id }, userId: 1);

            Assert.Equal(1, total);
        }

        [Fact]
        public async Task GetPagedAsync_FiltersSubcategoryId()
        {
            await SeedBaseDataAsync();
            var cat = new Touir.ExpensesManager.Expenses.Models.Category { Name = "Food" };
            _wrapper.Context.Categories.Add(cat);
            await _wrapper.Context.SaveChangesAsync();
            var sub = new Touir.ExpensesManager.Expenses.Models.Category { Name = "Lunch", ParentCategoryId = cat.Id };
            _wrapper.Context.Categories.Add(sub);
            await _wrapper.Context.SaveChangesAsync();

            var withSub = BuildExpense(); withSub.CategoryId = cat.Id; withSub.SubcategoryId = sub.Id;
            var noSub = BuildExpense();
            _wrapper.Context.Expenses.AddRange(withSub, noSub);
            await _wrapper.Context.SaveChangesAsync();

            var (items, total) = await _sut.GetPagedAsync(
                new ExpenseFilterDto { Page = 1, PageSize = 20, SubcategoryId = sub.Id }, userId: 1);

            Assert.Equal(1, total);
        }
    }
}
