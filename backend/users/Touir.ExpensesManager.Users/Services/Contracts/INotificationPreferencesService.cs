using Touir.ExpensesManager.Users.Controllers.DTO;

namespace Touir.ExpensesManager.Users.Services.Contracts
{
    public interface INotificationPreferencesService
    {
        Task<IReadOnlyList<NotificationPreferenceDto>> GetAsync(int userId);
        Task<IReadOnlyList<NotificationPreferenceDto>> UpdateAsync(int userId, IEnumerable<NotificationPreferenceDto> preferences);
    }
}
