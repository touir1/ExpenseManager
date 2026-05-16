using Moq;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services;

namespace Touir.ExpensesManager.Expenses.Tests.Services
{
    public class TagServiceTests
    {
        private static TagService CreateService(ITagRepository? repo = null)
            => new(repo ?? Mock.Of<ITagRepository>());

        private static Tag MakeTag(int id = 1, string name = "food") => new() { Id = id, Name = name };

        // ── GetVisibleAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetVisibleAsync_ReturnsOwnTags()
        {
            var repo = new Mock<ITagRepository>();
            repo.Setup(r => r.GetOwnAsync(1)).ReturnsAsync([MakeTag(1, "food")]);
            repo.Setup(r => r.GetFamilyAsync(1)).ReturnsAsync([]);

            var result = await CreateService(repo.Object).GetVisibleAsync(1);

            Assert.Single(result.Own);
            Assert.Equal("food", result.Own.First().Name);
            Assert.Empty(result.Family);
        }

        [Fact]
        public async Task GetVisibleAsync_ReturnsFamilyTags()
        {
            var repo = new Mock<ITagRepository>();
            repo.Setup(r => r.GetOwnAsync(1)).ReturnsAsync([]);
            repo.Setup(r => r.GetFamilyAsync(1)).ReturnsAsync([MakeTag(2, "travel")]);

            var result = await CreateService(repo.Object).GetVisibleAsync(1);

            Assert.Empty(result.Own);
            Assert.Single(result.Family);
            Assert.Equal("travel", result.Family.First().Name);
        }

        [Fact]
        public async Task GetVisibleAsync_ReturnsBothLists()
        {
            var repo = new Mock<ITagRepository>();
            repo.Setup(r => r.GetOwnAsync(1)).ReturnsAsync([MakeTag(1, "food")]);
            repo.Setup(r => r.GetFamilyAsync(1)).ReturnsAsync([MakeTag(2, "travel")]);

            var result = await CreateService(repo.Object).GetVisibleAsync(1);

            Assert.Single(result.Own);
            Assert.Single(result.Family);
        }

        [Fact]
        public async Task GetVisibleAsync_ReturnsEmptyLists_WhenNoTags()
        {
            var repo = new Mock<ITagRepository>();
            repo.Setup(r => r.GetOwnAsync(It.IsAny<int>())).ReturnsAsync([]);
            repo.Setup(r => r.GetFamilyAsync(It.IsAny<int>())).ReturnsAsync([]);

            var result = await CreateService(repo.Object).GetVisibleAsync(1);

            Assert.Empty(result.Own);
            Assert.Empty(result.Family);
        }

        // ── UseTagAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task UseTagAsync_CreatesNewTag_WhenNameNotFound()
        {
            var repo = new Mock<ITagRepository>();
            repo.Setup(r => r.GetByNameAsync("food")).ReturnsAsync((Tag?)null);
            repo.Setup(r => r.AddAsync(It.IsAny<Tag>())).ReturnsAsync(MakeTag(5, "food"));

            var result = await CreateService(repo.Object).UseTagAsync("food", userId: 1);

            repo.Verify(r => r.AddAsync(It.Is<Tag>(t => t.Name == "food")), Times.Once);
            Assert.Equal(5, result.Id);
            Assert.Equal("food", result.Name);
        }

        [Fact]
        public async Task UseTagAsync_ReturnsExistingTag_WhenNameFound()
        {
            var existing = MakeTag(3, "food");
            var repo = new Mock<ITagRepository>();
            repo.Setup(r => r.GetByNameAsync("food")).ReturnsAsync(existing);

            var result = await CreateService(repo.Object).UseTagAsync("food", userId: 1);

            repo.Verify(r => r.AddAsync(It.IsAny<Tag>()), Times.Never);
            Assert.Equal(3, result.Id);
        }

        [Fact]
        public async Task UseTagAsync_CallsEnsureUserTag()
        {
            var repo = new Mock<ITagRepository>();
            repo.Setup(r => r.GetByNameAsync("food")).ReturnsAsync(MakeTag(2, "food"));

            await CreateService(repo.Object).UseTagAsync("food", userId: 7);

            repo.Verify(r => r.EnsureUserTagAsync(7, 2), Times.Once);
        }

        [Fact]
        public async Task UseTagAsync_IsCaseSensitive()
        {
            var repo = new Mock<ITagRepository>();
            repo.Setup(r => r.GetByNameAsync("Food")).ReturnsAsync((Tag?)null);
            repo.Setup(r => r.GetByNameAsync("food")).ReturnsAsync(MakeTag(1, "food"));
            repo.Setup(r => r.AddAsync(It.IsAny<Tag>())).ReturnsAsync(MakeTag(2, "Food"));

            var lower = await CreateService(repo.Object).UseTagAsync("food", userId: 1);
            var upper = await CreateService(repo.Object).UseTagAsync("Food", userId: 1);

            Assert.Equal(1, lower.Id);
            Assert.Equal(2, upper.Id);
        }

        // ── RemoveTagAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task RemoveTagAsync_ReturnsTrueAndCallsRemove()
        {
            var repo = new Mock<ITagRepository>();
            repo.Setup(r => r.RemoveUserTagAsync(1, 5)).ReturnsAsync(true);

            var result = await CreateService(repo.Object).RemoveTagAsync(tagId: 5, userId: 1);

            Assert.True(result);
            repo.Verify(r => r.RemoveUserTagAsync(1, 5), Times.Once);
        }

        [Fact]
        public async Task RemoveTagAsync_ReturnsFalse_WhenNoUserTag()
        {
            var repo = new Mock<ITagRepository>();
            repo.Setup(r => r.RemoveUserTagAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);

            var result = await CreateService(repo.Object).RemoveTagAsync(tagId: 99, userId: 1);

            Assert.False(result);
        }
    }
}
