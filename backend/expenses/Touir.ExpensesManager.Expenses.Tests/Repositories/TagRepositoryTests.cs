using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Models.External;
using Touir.ExpensesManager.Expenses.Repositories;
using Touir.ExpensesManager.Expenses.Tests.TestHelpers;

namespace Touir.ExpensesManager.Expenses.Tests.Repositories
{
    public class TagRepositoryTests : IDisposable
    {
        private readonly TestExpensesDbContextWrapper _wrapper;
        private readonly TagRepository _sut;

        public TagRepositoryTests()
        {
            _wrapper = new TestExpensesDbContextWrapper();
            _sut = new TagRepository(_wrapper.Context);
        }

        public void Dispose()
        {
            _wrapper.Dispose();
            GC.SuppressFinalize(this);
        }

        private async Task SeedUserAsync(int id, string? email = null)
        {
            if (!_wrapper.Context.Users.Any(u => u.Id == id))
            {
                _wrapper.Context.Users.Add(new User { Id = id, FirstName = "T", LastName = "U", Email = email ?? $"u{id}@test.com", IsDeleted = false });
                await _wrapper.Context.SaveChangesAsync();
            }
        }

        private async Task<Family> SeedFamilyAsync(int createdById = 1, bool isDeleted = false)
        {
            await SeedUserAsync(createdById);
            var family = new Family { Name = "TestFamily", IsDefault = false, CreatedAt = DateTime.UtcNow, CreatedById = createdById, IsDeleted = isDeleted };
            _wrapper.Context.Families.Add(family);
            await _wrapper.Context.SaveChangesAsync();
            return family;
        }

        private async Task SeedMembershipAsync(int familyId, int userId, int roleId = 2)
        {
            await SeedUserAsync(userId);
            _wrapper.Context.FamilyMemberships.Add(new FamilyMembership { FamilyId = familyId, UserId = userId, RoleId = roleId, JoinedAt = DateTime.UtcNow });
            await _wrapper.Context.SaveChangesAsync();
        }

        private async Task<Tag> SeedTagAsync(string name)
        {
            var tag = new Tag { Name = name };
            _wrapper.Context.Tags.Add(tag);
            await _wrapper.Context.SaveChangesAsync();
            return tag;
        }

        private async Task SeedUserTagAsync(int userId, int tagId)
        {
            _wrapper.Context.UserTags.Add(new UserTag { UserId = userId, TagId = tagId });
            await _wrapper.Context.SaveChangesAsync();
        }

        // ── GetOwnAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task GetOwnAsync_ReturnsTagsAdoptedByUser()
        {
            await SeedUserAsync(1);
            var tag = await SeedTagAsync("food");
            await SeedUserTagAsync(1, tag.Id);

            var result = await _sut.GetOwnAsync(1);

            Assert.Single(result);
            Assert.Equal("food", result.First().Name);
        }

        [Fact]
        public async Task GetOwnAsync_DoesNotReturnTagsOfOtherUsers()
        {
            await SeedUserAsync(1);
            await SeedUserAsync(2);
            var tag = await SeedTagAsync("food");
            await SeedUserTagAsync(2, tag.Id);

            var result = await _sut.GetOwnAsync(1);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetOwnAsync_ReturnsEmpty_WhenNoUserTags()
        {
            await SeedUserAsync(1);

            var result = await _sut.GetOwnAsync(1);

            Assert.Empty(result);
        }

        // ── GetFamilyAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetFamilyAsync_ReturnsCoMemberTags()
        {
            var family = await SeedFamilyAsync(createdById: 1);
            await SeedMembershipAsync(family.Id, userId: 1);
            await SeedMembershipAsync(family.Id, userId: 2);
            var tag = await SeedTagAsync("travel");
            await SeedUserTagAsync(2, tag.Id);

            var result = await _sut.GetFamilyAsync(1);

            Assert.Single(result);
            Assert.Equal("travel", result.First().Name);
        }

        [Fact]
        public async Task GetFamilyAsync_ExcludesTagsAlsoAdoptedByRequestingUser()
        {
            var family = await SeedFamilyAsync(createdById: 1);
            await SeedMembershipAsync(family.Id, userId: 1);
            await SeedMembershipAsync(family.Id, userId: 2);
            var tag = await SeedTagAsync("shared");
            await SeedUserTagAsync(1, tag.Id);
            await SeedUserTagAsync(2, tag.Id);

            var result = await _sut.GetFamilyAsync(1);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFamilyAsync_ExcludesTagsFromDeletedFamilies()
        {
            var activeFamily = await SeedFamilyAsync(createdById: 1);
            var deletedFamily = await SeedFamilyAsync(createdById: 1, isDeleted: true);
            await SeedMembershipAsync(activeFamily.Id, userId: 1);
            await SeedMembershipAsync(deletedFamily.Id, userId: 1);
            await SeedUserAsync(3);
            await SeedMembershipAsync(deletedFamily.Id, userId: 3);
            var tag = await SeedTagAsync("deletedFamilyTag");
            await SeedUserTagAsync(3, tag.Id);

            var result = await _sut.GetFamilyAsync(1);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFamilyAsync_ExcludesTagsFromUsersWithNoSharedFamily()
        {
            await SeedUserAsync(1);
            await SeedUserAsync(2);
            var tag = await SeedTagAsync("unrelated");
            await SeedUserTagAsync(2, tag.Id);

            var result = await _sut.GetFamilyAsync(1);

            Assert.Empty(result);
        }

        // ── EnsureUserTagAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task EnsureUserTagAsync_CreatesUserTag_AndReturnsTrue()
        {
            await SeedUserAsync(1);
            var tag = await SeedTagAsync("food");

            var created = await _sut.EnsureUserTagAsync(1, tag.Id);

            Assert.True(created);
            Assert.True(_wrapper.Context.UserTags.Any(ut => ut.UserId == 1 && ut.TagId == tag.Id));
        }

        [Fact]
        public async Task EnsureUserTagAsync_ReturnsFalse_WhenAlreadyExists()
        {
            await SeedUserAsync(1);
            var tag = await SeedTagAsync("food");
            await SeedUserTagAsync(1, tag.Id);

            var created = await _sut.EnsureUserTagAsync(1, tag.Id);

            Assert.False(created);
        }

        [Fact]
        public async Task EnsureUserTagAsync_DoesNotCreateDuplicate()
        {
            await SeedUserAsync(1);
            var tag = await SeedTagAsync("food");
            await SeedUserTagAsync(1, tag.Id);

            await _sut.EnsureUserTagAsync(1, tag.Id);

            Assert.Single(_wrapper.Context.UserTags.Where(ut => ut.UserId == 1 && ut.TagId == tag.Id));
        }

        // ── RemoveUserTagAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task RemoveUserTagAsync_RemovesRow_AndReturnsTrue()
        {
            await SeedUserAsync(1);
            var tag = await SeedTagAsync("food");
            await SeedUserTagAsync(1, tag.Id);

            var removed = await _sut.RemoveUserTagAsync(1, tag.Id);

            Assert.True(removed);
            Assert.False(_wrapper.Context.UserTags.Any(ut => ut.UserId == 1 && ut.TagId == tag.Id));
        }

        [Fact]
        public async Task RemoveUserTagAsync_ReturnsFalse_WhenNotFound()
        {
            await SeedUserAsync(1);
            var tag = await SeedTagAsync("food");

            var removed = await _sut.RemoveUserTagAsync(1, tag.Id);

            Assert.False(removed);
        }

        // ── IsVisibleAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task IsVisibleAsync_ReturnsTrue_ForOwnTag()
        {
            await SeedUserAsync(1);
            var tag = await SeedTagAsync("food");
            await SeedUserTagAsync(1, tag.Id);

            var visible = await _sut.IsVisibleAsync(1, tag.Id);

            Assert.True(visible);
        }

        [Fact]
        public async Task IsVisibleAsync_ReturnsTrue_ForCoMemberTag()
        {
            var family = await SeedFamilyAsync(createdById: 1);
            await SeedMembershipAsync(family.Id, userId: 1);
            await SeedMembershipAsync(family.Id, userId: 2);
            var tag = await SeedTagAsync("shared");
            await SeedUserTagAsync(2, tag.Id);

            var visible = await _sut.IsVisibleAsync(1, tag.Id);

            Assert.True(visible);
        }

        [Fact]
        public async Task IsVisibleAsync_ReturnsFalse_ForUnrelatedTag()
        {
            await SeedUserAsync(1);
            await SeedUserAsync(2);
            var tag = await SeedTagAsync("private");
            await SeedUserTagAsync(2, tag.Id);

            var visible = await _sut.IsVisibleAsync(1, tag.Id);

            Assert.False(visible);
        }

        [Fact]
        public async Task IsVisibleAsync_ReturnsFalse_ForTagInDeletedFamily()
        {
            var deletedFamily = await SeedFamilyAsync(createdById: 1, isDeleted: true);
            await SeedMembershipAsync(deletedFamily.Id, userId: 1);
            await SeedMembershipAsync(deletedFamily.Id, userId: 2);
            var tag = await SeedTagAsync("deletedFamilyTag");
            await SeedUserTagAsync(2, tag.Id);

            var visible = await _sut.IsVisibleAsync(1, tag.Id);

            Assert.False(visible);
        }
    }
}
