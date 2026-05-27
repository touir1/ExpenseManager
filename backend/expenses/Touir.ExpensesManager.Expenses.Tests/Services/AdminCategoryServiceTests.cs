using Moq;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services;

namespace Touir.ExpensesManager.Expenses.Tests.Services
{
    public class AdminCategoryServiceTests
    {
        private static AdminCategoryService CreateService(ICategoryRepository? repo = null)
            => new(repo ?? Mock.Of<ICategoryRepository>());

        private static Category MakeCategory(int id, string name, bool isDeleted = false, int? parentId = null, List<Category>? children = null)
            => new() { Id = id, Name = name, IsDeleted = isDeleted, ParentCategoryId = parentId, Children = children ?? [] };

        [Fact]
        public async Task GetAllAsync_ReturnsAllCategories_IncludingArchived()
        {
            var categories = new List<Category>
            {
                MakeCategory(1, "Food"),
                MakeCategory(2, "Old", isDeleted: true)
            };
            var repo = new Mock<ICategoryRepository>();
            repo.Setup(r => r.GetAllWithArchivedAsync()).ReturnsAsync(categories);

            var result = (await CreateService(repo.Object).GetAllAsync()).ToList();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task AddCategoryAsync_ReturnsDto_WithCorrectName()
        {
            var added = MakeCategory(10, "Travel");
            var repo = new Mock<ICategoryRepository>();
            repo.Setup(r => r.AddAsync(It.IsAny<Category>())).ReturnsAsync(added);

            var result = await CreateService(repo.Object).AddCategoryAsync("Travel", null);

            Assert.Equal(10, result.Id);
            Assert.Equal("Travel", result.Name);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ReturnsUpdatedDto()
        {
            var cat = MakeCategory(5, "Food");
            var repo = new Mock<ICategoryRepository>();
            repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(cat);
            repo.Setup(r => r.UpdateAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);

            var result = await CreateService(repo.Object).UpdateCategoryAsync(5, "Groceries", "desc");

            Assert.Equal("Groceries", result.Name);
        }

        [Fact]
        public async Task UpdateCategoryAsync_Throws_WhenNotFound()
        {
            var repo = new Mock<ICategoryRepository>();
            repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                CreateService(repo.Object).UpdateCategoryAsync(99, "X", null));
        }

        [Fact]
        public async Task ArchiveCategoryAsync_CallsArchive_WhenNoActiveChildren()
        {
            var cat = MakeCategory(1, "Food", children: []);
            var repo = new Mock<ICategoryRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cat);
            repo.Setup(r => r.ArchiveAsync(1)).Returns(Task.CompletedTask);

            await CreateService(repo.Object).ArchiveCategoryAsync(1);

            repo.Verify(r => r.ArchiveAsync(1), Times.Once);
        }

        [Fact]
        public async Task ArchiveCategoryAsync_Throws_WhenHasActiveChildren()
        {
            var cat = MakeCategory(1, "Food", children: [MakeCategory(2, "Sub", isDeleted: false)]);
            var repo = new Mock<ICategoryRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cat);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateService(repo.Object).ArchiveCategoryAsync(1));
        }

        [Fact]
        public async Task UnarchiveCategoryAsync_CallsUnarchive_WhenArchived()
        {
            var cat = MakeCategory(1, "Food", isDeleted: true);
            var repo = new Mock<ICategoryRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cat);
            repo.Setup(r => r.UnarchiveAsync(1)).Returns(Task.CompletedTask);

            await CreateService(repo.Object).UnarchiveCategoryAsync(1);

            repo.Verify(r => r.UnarchiveAsync(1), Times.Once);
        }

        [Fact]
        public async Task AddSubcategoryAsync_ReturnsDto_WhenParentActive()
        {
            var parent = MakeCategory(1, "Food");
            var added = MakeCategory(10, "Organic", parentId: 1);
            var repo = new Mock<ICategoryRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(parent);
            repo.Setup(r => r.AddAsync(It.IsAny<Category>())).ReturnsAsync(added);

            var result = await CreateService(repo.Object).AddSubcategoryAsync(1, "Organic", null);

            Assert.Equal(10, result.Id);
            Assert.Equal("Organic", result.Name);
        }

        [Fact]
        public async Task AddSubcategoryAsync_Throws_WhenParentArchived()
        {
            var parent = MakeCategory(1, "Food", isDeleted: true);
            var repo = new Mock<ICategoryRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(parent);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateService(repo.Object).AddSubcategoryAsync(1, "Sub", null));
        }

        [Fact]
        public async Task AddSubcategoryAsync_Throws_WhenParentIsSubcategory()
        {
            var parent = MakeCategory(2, "Sub", parentId: 1); // already has a parent
            var repo = new Mock<ICategoryRepository>();
            repo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(parent);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateService(repo.Object).AddSubcategoryAsync(2, "NestedSub", null));
        }

        [Fact]
        public async Task ArchiveSubcategoryAsync_CallsArchive_WhenSubcategory()
        {
            var sub = MakeCategory(5, "Sub", parentId: 1);
            var repo = new Mock<ICategoryRepository>();
            repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(sub);
            repo.Setup(r => r.ArchiveAsync(5)).Returns(Task.CompletedTask);

            await CreateService(repo.Object).ArchiveSubcategoryAsync(5);

            repo.Verify(r => r.ArchiveAsync(5), Times.Once);
        }

        [Fact]
        public async Task ArchiveSubcategoryAsync_Throws_WhenNotSubcategory()
        {
            var top = MakeCategory(1, "Food"); // no parentId
            var repo = new Mock<ICategoryRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(top);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateService(repo.Object).ArchiveSubcategoryAsync(1));
        }
    }
}
