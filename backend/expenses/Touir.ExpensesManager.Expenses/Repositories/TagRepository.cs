using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;

namespace Touir.ExpensesManager.Expenses.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly ExpensesDbContext _db;

        public TagRepository(ExpensesDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Tag>> GetOwnAsync(int userId)
        {
            return await _db.Tags
                .Where(t => t.UserTags.Any(ut => ut.UserId == userId))
                .OrderBy(t => t.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> GetFamilyAsync(int userId)
        {
            var userFamilyIds = _db.FamilyMemberships
                .Where(fm => fm.UserId == userId && !fm.Family.IsDeleted)
                .Select(fm => fm.FamilyId);

            var coMemberIds = _db.FamilyMemberships
                .Where(fm => userFamilyIds.Contains(fm.FamilyId) && fm.UserId != userId)
                .Select(fm => fm.UserId)
                .Distinct();

            return await _db.Tags
                .Where(t =>
                    t.UserTags.Any(ut => coMemberIds.Contains(ut.UserId)) &&
                    !t.UserTags.Any(ut => ut.UserId == userId))
                .OrderBy(t => t.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var idList = ids.ToList();
            return await _db.Tags
                .Where(t => idList.Contains(t.Id))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Tag?> GetByNameAsync(string name)
        {
            return await _db.Tags
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task<Tag> AddAsync(Tag tag)
        {
            _db.Tags.Add(tag);
            await _db.SaveChangesAsync();
            return tag;
        }

        public async Task<bool> EnsureUserTagAsync(int userId, int tagId)
        {
            var exists = await _db.UserTags.AnyAsync(ut => ut.UserId == userId && ut.TagId == tagId);
            if (exists)
                return false;

            _db.UserTags.Add(new UserTag { UserId = userId, TagId = tagId });
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveUserTagAsync(int userId, int tagId)
        {
            var userTag = await _db.UserTags.FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TagId == tagId);
            if (userTag is null)
                return false;

            _db.UserTags.Remove(userTag);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsVisibleAsync(int userId, int tagId)
        {
            var isOwn = await _db.UserTags.AnyAsync(ut => ut.UserId == userId && ut.TagId == tagId);
            if (isOwn)
                return true;

            var userFamilyIds = _db.FamilyMemberships
                .Where(fm => fm.UserId == userId && !fm.Family.IsDeleted)
                .Select(fm => fm.FamilyId);

            var coMemberIds = _db.FamilyMemberships
                .Where(fm => userFamilyIds.Contains(fm.FamilyId) && fm.UserId != userId)
                .Select(fm => fm.UserId)
                .Distinct();

            return await _db.UserTags.AnyAsync(ut => coMemberIds.Contains(ut.UserId) && ut.TagId == tagId);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
