using Touir.ExpensesManager.Users.Models;

namespace Touir.ExpensesManager.Users.Repositories.Contracts
{
    public interface INotificationPreferencesRepository
    {
        Task<IReadOnlyList<NotificationPreference>> GetByUserIdAsync(int userId);
        Task UpsertAsync(int userId, IEnumerable<NotificationPreference> preferences);
    }
}
