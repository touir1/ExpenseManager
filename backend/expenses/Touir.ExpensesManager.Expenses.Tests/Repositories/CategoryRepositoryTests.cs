using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories;
using Touir.ExpensesManager.Expenses.Tests.TestHelpers;

namespace Touir.ExpensesManager.Expenses.Tests.Repositories
{
    public class CategoryRepositoryTests : IDisposable
    {
        private readonly TestExpensesDbContextWrapper _wrapper;
        private readonly CategoryRepository _sut;

        public CategoryRepositoryTests()
        {
            _wrapper = new TestExpensesDbContextWrapper();
            _sut = new CategoryRepository(_wrapper.Context);
        }

        public void Dispose()
        {
            _wrapper.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task GetAllActiveAsync_ReturnsOnlyTopLevelCategories()
        {
            var parent = new Category { Name = "Food", IsArchived = false };
            var child = new Category { Name = "Groceries", IsArchived = false, ParentCategory = parent };
            _wrapper.Context.Categories.AddRange(parent, child);
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetAllActiveAsync()).ToList();

            Assert.Single(result);
            Assert.Equal("Food", result[0].Name);
        }

        [Fact]
        public async Task GetAllActiveAsync_IncludesChildren()
        {
            var parent = new Category { Name = "Transport", IsArchived = false };
            var child1 = new Category { Name = "Car", IsArchived = false, ParentCategory = parent };
            var child2 = new Category { Name = "Bus", IsArchived = false, ParentCategory = parent };
            _wrapper.Context.Categories.AddRange(parent, child1, child2);
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetAllActiveAsync()).ToList();

            Assert.Single(result);
            Assert.Equal(2, result[0].Children.Count);
        }

        [Fact]
        public async Task GetAllActiveAsync_ExcludesArchivedCategories()
        {
            var active = new Category { Name = "Active", IsArchived = false };
            var archived = new Category { Name = "Archived", IsArchived = true };
            _wrapper.Context.Categories.AddRange(active, archived);
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetAllActiveAsync()).ToList();

            Assert.Single(result);
            Assert.Equal("Active", result[0].Name);
        }

        [Fact]
        public async Task GetAllActiveAsync_ReturnsEmptyWhenNoCategoriesExist()
        {
            var result = await _sut.GetAllActiveAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllActiveAsync_IncludesArchivedChildrenInCollection()
        {
            var parent = new Category { Name = "Food", IsArchived = false };
            var activeSub = new Category { Name = "Active Sub", IsArchived = false, ParentCategory = parent };
            var archivedSub = new Category { Name = "Archived Sub", IsArchived = true, ParentCategory = parent };
            _wrapper.Context.Categories.AddRange(parent, activeSub, archivedSub);
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetAllActiveAsync()).ToList();

            // Repository returns all children; service filters archived ones
            Assert.Equal(2, result[0].Children.Count);
        }
    }
}
