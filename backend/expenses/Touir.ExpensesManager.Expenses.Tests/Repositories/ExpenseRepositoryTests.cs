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

        private Expense BuildExpense(int userId = 1, long id = 0) => new()
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
    }
}
