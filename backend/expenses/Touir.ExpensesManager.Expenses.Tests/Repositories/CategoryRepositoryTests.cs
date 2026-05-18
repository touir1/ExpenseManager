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
            var parent = new Category { Name = "TestParentOnly", IsDeleted = false };
            var child = new Category { Name = "TestChildOnly", IsDeleted = false, ParentCategory = parent };
            _wrapper.Context.Categories.AddRange(parent, child);
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetAllActiveAsync()).ToList();

            Assert.Contains(result, c => c.Name == "TestParentOnly");
            Assert.DoesNotContain(result, c => c.Name == "TestChildOnly");
        }

        [Fact]
        public async Task GetAllActiveAsync_IncludesChildren()
        {
            var parent = new Category { Name = "TestTransportGroup", IsDeleted = false };
            var child1 = new Category { Name = "TestCar", IsDeleted = false, ParentCategory = parent };
            var child2 = new Category { Name = "TestBus", IsDeleted = false, ParentCategory = parent };
            _wrapper.Context.Categories.AddRange(parent, child1, child2);
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetAllActiveAsync()).ToList();

            var found = result.First(c => c.Name == "TestTransportGroup");
            Assert.Equal(2, found.Children.Count);
        }

        [Fact]
        public async Task GetAllActiveAsync_ExcludesArchivedCategories()
        {
            var active = new Category { Name = "TestActiveCategory", IsDeleted = false };
            var archived = new Category { Name = "TestArchivedCategory", IsDeleted = true };
            _wrapper.Context.Categories.AddRange(active, archived);
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetAllActiveAsync()).ToList();

            Assert.Contains(result, c => c.Name == "TestActiveCategory");
            Assert.DoesNotContain(result, c => c.Name == "TestArchivedCategory");
        }

        [Fact]
        public async Task GetAllActiveAsync_ReturnsSeededTopLevelCategories()
        {
            var result = (await _sut.GetAllActiveAsync()).ToList();

            Assert.Equal(17, result.Count); // 17 seeded top-level categories
        }

        [Fact]
        public async Task GetAllActiveAsync_IncludesArchivedChildrenInCollection()
        {
            var parent = new Category { Name = "TestFoodGroup", IsDeleted = false };
            var activeSub = new Category { Name = "TestActiveSub", IsDeleted = false, ParentCategory = parent };
            var archivedSub = new Category { Name = "TestArchivedSub", IsDeleted = true, ParentCategory = parent };
            _wrapper.Context.Categories.AddRange(parent, activeSub, archivedSub);
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetAllActiveAsync()).ToList();

            // Repository returns all children; service filters archived ones
            var found = result.First(c => c.Name == "TestFoodGroup");
            Assert.Equal(2, found.Children.Count);
        }

        [Fact]
        public void Category_DeletedAt_Setter()
        {
            var cat = new Category { Name = "x" };
            var ts = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            cat.DeletedAt = ts;
            Assert.Equal(ts, cat.DeletedAt);
        }
    }
}
