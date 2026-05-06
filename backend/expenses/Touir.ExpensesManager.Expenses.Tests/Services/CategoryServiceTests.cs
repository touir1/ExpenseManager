using Moq;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services;

namespace Touir.ExpensesManager.Expenses.Tests.Services
{
    public class CategoryServiceTests
    {
        private static CategoryService CreateService(ICategoryRepository? repo = null)
        {
            return new CategoryService(repo ?? Mock.Of<ICategoryRepository>());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsOnlyTopLevelCategories()
        {
            var parent = new Category { Id = 1, Name = "Food", IsArchived = false, Children = [] };
            var mockRepo = new Mock<ICategoryRepository>();
            mockRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([parent]);

            var result = (await CreateService(mockRepo.Object).GetAllAsync()).ToList();

            Assert.Single(result);
            Assert.Equal("Food", result[0].Name);
        }

        [Fact]
        public async Task GetAllAsync_IncludesActiveSubcategories()
        {
            var child1 = new Category { Id = 2, Name = "Car", IsArchived = false };
            var child2 = new Category { Id = 3, Name = "Bus", IsArchived = false };
            var parent = new Category { Id = 1, Name = "Transport", IsArchived = false, Children = [child1, child2] };
            var mockRepo = new Mock<ICategoryRepository>();
            mockRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([parent]);

            var result = (await CreateService(mockRepo.Object).GetAllAsync()).ToList();

            Assert.Single(result);
            Assert.Equal(2, result[0].Subcategories.Count());
        }

        [Fact]
        public async Task GetAllAsync_ExcludesArchivedTopLevelCategories()
        {
            var active = new Category { Id = 1, Name = "Active", IsArchived = false, Children = [] };
            var mockRepo = new Mock<ICategoryRepository>();
            mockRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([active]);

            var result = (await CreateService(mockRepo.Object).GetAllAsync()).ToList();

            Assert.Single(result);
            Assert.Equal("Active", result[0].Name);
        }

        [Fact]
        public async Task GetAllAsync_ExcludesArchivedSubcategories()
        {
            var activeSub = new Category { Id = 2, Name = "Active Sub", IsArchived = false };
            var archivedSub = new Category { Id = 3, Name = "Archived Sub", IsArchived = true };
            var parent = new Category { Id = 1, Name = "Food", IsArchived = false, Children = [activeSub, archivedSub] };
            var mockRepo = new Mock<ICategoryRepository>();
            mockRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([parent]);

            var result = (await CreateService(mockRepo.Object).GetAllAsync()).ToList();

            var subs = result[0].Subcategories.ToList();
            Assert.Single(subs);
            Assert.Equal("Active Sub", subs[0].Name);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyWhenNoCategoriesExist()
        {
            var mockRepo = new Mock<ICategoryRepository>();
            mockRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([]);

            var result = await CreateService(mockRepo.Object).GetAllAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_MapsDescriptionCorrectly()
        {
            var cat = new Category { Id = 1, Name = "Health", Description = "Health expenses", IsArchived = false, Children = [] };
            var mockRepo = new Mock<ICategoryRepository>();
            mockRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([cat]);

            var result = (await CreateService(mockRepo.Object).GetAllAsync()).ToList();

            Assert.Equal("Health expenses", result[0].Description);
        }

        [Fact]
        public async Task GetAllAsync_SubcategoriesAreSubcategoryDtoType()
        {
            var child = new Category { Id = 2, Name = "Child", IsArchived = false };
            var parent = new Category { Id = 1, Name = "Parent", IsArchived = false, Children = [child] };
            var mockRepo = new Mock<ICategoryRepository>();
            mockRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([parent]);

            var result = (await CreateService(mockRepo.Object).GetAllAsync()).ToList();

            var sub = result[0].Subcategories.First();
            Assert.IsType<SubcategoryDto>(sub);
        }

        [Fact]
        public async Task GetAllAsync_CallsRepositoryOnce()
        {
            var mockRepo = new Mock<ICategoryRepository>();
            mockRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([]);

            await CreateService(mockRepo.Object).GetAllAsync();

            mockRepo.Verify(r => r.GetAllActiveAsync(), Times.Once);
        }
    }
}
