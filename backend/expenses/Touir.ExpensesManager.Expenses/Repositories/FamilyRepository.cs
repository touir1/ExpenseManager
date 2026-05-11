using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;

namespace Touir.ExpensesManager.Expenses.Repositories
{
    public class FamilyRepository : IFamilyRepository
    {
        private readonly ExpensesDbContext _db;

        public FamilyRepository(ExpensesDbContext db)
        {
            _db = db;
        }

        public async Task<Family> CreateAsync(Family family, FamilyMembership headMembership)
        {
            _db.Families.Add(family);
            await _db.SaveChangesAsync();

            headMembership.FamilyId = family.Id;
            _db.FamilyMemberships.Add(headMembership);
            await _db.SaveChangesAsync();

            return family;
        }

        public async Task<Family?> GetByIdAsync(int id)
            => await _db.Families.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);

        public async Task<(Family? Family, IEnumerable<FamilyMembership> Members)> GetByIdWithMembersAsync(int id)
        {
            var family = await _db.Families.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
            if (family is null)
                return (null, []);

            var members = await _db.FamilyMemberships
                .Include(m => m.User)
                .Include(m => m.Role)
                .Where(m => m.FamilyId == id)
                .AsNoTracking()
                .ToListAsync();

            return (family, members);
        }

        public async Task<IEnumerable<(Family Family, FamilyMembership Membership)>> GetFamiliesByUserAsync(int userId)
        {
            var memberships = await _db.FamilyMemberships
                .Include(m => m.Family)
                .Include(m => m.Role)
                .Where(m => m.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            return memberships.Select(m => (m.Family, m));
        }

        public async Task<Family?> GetDefaultFamilyForUserAsync(int userId)
        {
            var membership = await _db.FamilyMemberships
                .Include(m => m.Family)
                .Where(m => m.UserId == userId && m.Family.IsDefault)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return membership?.Family;
        }

        public async Task<bool> HasDefaultFamilyAsync(int userId)
            => await _db.FamilyMemberships
                .Include(m => m.Family)
                .AnyAsync(m => m.UserId == userId && m.Family.IsDefault);

        public async Task<FamilyMembership?> GetMembershipAsync(int familyId, int userId)
            => await _db.FamilyMemberships
                .Include(m => m.Role)
                .FirstOrDefaultAsync(m => m.FamilyId == familyId && m.UserId == userId);

        public async Task<bool> IsMemberAsync(int familyId, int userId)
            => await _db.FamilyMemberships.AnyAsync(m => m.FamilyId == familyId && m.UserId == userId);

        public async Task AddMemberAsync(FamilyMembership membership)
        {
            _db.FamilyMemberships.Add(membership);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateMemberAsync(FamilyMembership membership)
        {
            _db.FamilyMemberships.Update(membership);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveMemberAsync(FamilyMembership membership)
        {
            _db.FamilyMemberships.Remove(membership);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateFamilyAsync(Family family)
        {
            _db.Families.Update(family);
            await _db.SaveChangesAsync();
        }

        public async Task AddInvitationAsync(FamilyInvitation invitation)
        {
            _db.FamilyInvitations.Add(invitation);
            await _db.SaveChangesAsync();
        }

        public async Task<FamilyInvitation?> GetInvitationByTokenAsync(string token)
            => await _db.FamilyInvitations
                .Include(i => i.Family)
                .FirstOrDefaultAsync(i => i.Token == token);

        public async Task UpdateInvitationAsync(FamilyInvitation invitation)
        {
            _db.FamilyInvitations.Update(invitation);
            await _db.SaveChangesAsync();
        }

        public async Task AddAttributionsAsync(IEnumerable<ExpenseFamilyAttribution> attributions)
        {
            _db.ExpenseFamilyAttributions.AddRange(attributions);
            await _db.SaveChangesAsync();
        }

        public async Task ClearAttributionsAsync(long expenseId)
        {
            var existing = await _db.ExpenseFamilyAttributions
                .Where(a => a.ExpenseId == expenseId)
                .ToListAsync();
            _db.ExpenseFamilyAttributions.RemoveRange(existing);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveMemberAttributionsAsync(int familyId, int ownerId)
        {
            var attributions = await _db.ExpenseFamilyAttributions
                .Include(a => a.Expense)
                .Where(a => a.FamilyId == familyId && a.Expense.UserId == ownerId)
                .ToListAsync();
            _db.ExpenseFamilyAttributions.RemoveRange(attributions);
            await _db.SaveChangesAsync();
        }
    }
}
