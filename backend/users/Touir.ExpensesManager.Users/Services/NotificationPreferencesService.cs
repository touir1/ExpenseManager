using Touir.ExpensesManager.Users.Controllers.DTO;
using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Services.Contracts;

namespace Touir.ExpensesManager.Users.Services
{
    public class NotificationPreferencesService : INotificationPreferencesService
    {
        private readonly INotificationPreferencesRepository _repo;

        public NotificationPreferencesService(INotificationPreferencesRepository repo)
        {
            _repo = repo;
        }

        public async Task<IReadOnlyList<NotificationPreferenceDto>> GetAsync(int userId)
        {
            var prefs = await _repo.GetByUserIdAsync(userId);
            return prefs.Select(MapToDto).ToList();
        }

        public async Task<IReadOnlyList<NotificationPreferenceDto>> UpdateAsync(int userId, IEnumerable<NotificationPreferenceDto> preferences)
        {
            var models = preferences.Select(p => new NotificationPreference
            {
                UserId = userId,
                EventType = p.EventType,
                EmailEnabled = p.EmailEnabled
            }).ToList();

            await _repo.UpsertAsync(userId, models);
            return await GetAsync(userId);
        }

        private static NotificationPreferenceDto MapToDto(NotificationPreference p) => new()
        {
            EventType = p.EventType,
            EmailEnabled = p.EmailEnabled
        };
    }
}
