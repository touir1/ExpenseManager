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

        // ── GetAllWithArchivedAsync ───────────────────────────────────────────

        [Fact]
        public async Task GetAllWithArchivedAsync_ReturnsAllTopLevelIncludingArchived()
        {
            var active = new Category { Name = "TestActiveTop", IsDeleted = false };
            var archived = new Category { Name = "TestArchivedTop", IsDeleted = true, DeletedAt = DateTime.UtcNow };
            _wrapper.Context.Categories.AddRange(active, archived);
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetAllWithArchivedAsync()).ToList();

            Assert.Contains(result, c => c.Name == "TestActiveTop");
            Assert.Contains(result, c => c.Name == "TestArchivedTop");
        }

        [Fact]
        public async Task GetAllWithArchivedAsync_ExcludesChildCategories()
        {
            var parent = new Category { Name = "TestParentWithArchived", IsDeleted = false };
            var child = new Category { Name = "TestChildHidden", IsDeleted = false, ParentCategory = parent };
            _wrapper.Context.Categories.AddRange(parent, child);
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetAllWithArchivedAsync()).ToList();

            Assert.Contains(result, c => c.Name == "TestParentWithArchived");
            Assert.DoesNotContain(result, c => c.Name == "TestChildHidden");
        }

        // ── ExistsWithNameAsync ───────────────────────────────────────────────

        [Fact]
        public async Task ExistsWithNameAsync_ReturnsTrue_WhenNameExists()
        {
            _wrapper.Context.Categories.Add(new Category { Name = "TestDupCheck", IsDeleted = false });
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.ExistsWithNameAsync("testdupcheck", null);

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsWithNameAsync_ReturnsFalse_WhenNameNotFound()
        {
            var result = await _sut.ExistsWithNameAsync("NoSuchCategory_XYZ", null);

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsWithNameAsync_ReturnsFalse_WhenExcludeIdMatches()
        {
            var cat = new Category { Name = "TestExcluded", IsDeleted = false };
            _wrapper.Context.Categories.Add(cat);
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.ExistsWithNameAsync("TestExcluded", null, cat.Id);

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsWithNameAsync_ReturnsFalse_WhenDeleted()
        {
            _wrapper.Context.Categories.Add(new Category { Name = "TestDeletedName", IsDeleted = true });
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.ExistsWithNameAsync("TestDeletedName", null);

            Assert.False(result);
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_ReturnsCategory_WhenExists()
        {
            var cat = new Category { Name = "TestGetById", IsDeleted = false };
            _wrapper.Context.Categories.Add(cat);
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetByIdAsync(cat.Id);

            Assert.NotNull(result);
            Assert.Equal(cat.Id, result!.Id);
            Assert.Equal("TestGetById", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            var result = await _sut.GetByIdAsync(99999);

            Assert.Null(result);
        }

        // ── AddAsync ─────────────────────────────────────────────────────────

        [Fact]
        public async Task AddAsync_PersistsAndReturnsCategory()
        {
            var cat = new Category { Name = "TestAdd", IsDeleted = false };

            var result = await _sut.AddAsync(cat);

            Assert.True(result.Id > 0);
            Assert.NotNull(_wrapper.Context.Categories.Find(result.Id));
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_PersistsChanges()
        {
            var cat = new Category { Name = "TestUpdateOld", IsDeleted = false };
            _wrapper.Context.Categories.Add(cat);
            await _wrapper.Context.SaveChangesAsync();

            cat.Name = "TestUpdateNew";
            await _sut.UpdateAsync(cat);

            var updated = _wrapper.Context.Categories.Find(cat.Id);
            Assert.Equal("TestUpdateNew", updated!.Name);
        }

        // ── ArchiveAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task ArchiveAsync_SetsIsDeletedTrue_WhenFound()
        {
            var cat = new Category { Name = "TestArchive", IsDeleted = false };
            _wrapper.Context.Categories.Add(cat);
            await _wrapper.Context.SaveChangesAsync();

            await _sut.ArchiveAsync(cat.Id);

            var archived = _wrapper.Context.Categories.Find(cat.Id);
            Assert.True(archived!.IsDeleted);
            Assert.NotNull(archived.DeletedAt);
        }

        [Fact]
        public async Task ArchiveAsync_DoesNotThrow_WhenNotFound()
        {
            var ex = await Record.ExceptionAsync(() => _sut.ArchiveAsync(99999));
            Assert.Null(ex);
        }

        // ── UnarchiveAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task UnarchiveAsync_SetsIsDeletedFalse_WhenFound()
        {
            var cat = new Category { Name = "TestUnarchive", IsDeleted = true, DeletedAt = DateTime.UtcNow };
            _wrapper.Context.Categories.Add(cat);
            await _wrapper.Context.SaveChangesAsync();

            await _sut.UnarchiveAsync(cat.Id);

            var result = _wrapper.Context.Categories.Find(cat.Id);
            Assert.False(result!.IsDeleted);
            Assert.Null(result.DeletedAt);
        }

        [Fact]
        public async Task UnarchiveAsync_DoesNotThrow_WhenNotFound()
        {
            var ex = await Record.ExceptionAsync(() => _sut.UnarchiveAsync(99999));
            Assert.Null(ex);
        }
    }
}
