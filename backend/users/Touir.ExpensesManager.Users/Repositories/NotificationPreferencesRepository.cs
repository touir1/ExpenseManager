using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Users.Infrastructure;
using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories.Contracts;

namespace Touir.ExpensesManager.Users.Repositories
{
    public class NotificationPreferencesRepository : INotificationPreferencesRepository
    {
        private readonly UsersAppDbContext _db;

        public NotificationPreferencesRepository(UsersAppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<NotificationPreference>> GetByUserIdAsync(int userId)
        {
            return await _db.NotificationPreferences
                .Where(p => p.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpsertAsync(int userId, IEnumerable<NotificationPreference> preferences)
        {
            var existing = await _db.NotificationPreferences
                .Where(p => p.UserId == userId)
                .ToListAsync();

            foreach (var pref in preferences)
            {
                var row = existing.FirstOrDefault(e => e.EventType == pref.EventType);
                if (row is null)
                {
                    _db.NotificationPreferences.Add(new NotificationPreference
                    {
                        UserId = userId,
                        EventType = pref.EventType,
                        EmailEnabled = pref.EmailEnabled
                    });
                }
                else
                {
                    row.EmailEnabled = pref.EmailEnabled;
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}
