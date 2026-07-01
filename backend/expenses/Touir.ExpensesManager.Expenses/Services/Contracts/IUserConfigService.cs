using Touir.ExpensesManager.Expenses.Controllers.DTO;

namespace Touir.ExpensesManager.Expenses.Services.Contracts
{
    public interface IUserConfigService
    {
        Task<UserConfigDto> GetAsync(int userId);
        Task<UserConfigDto?> UpdateAsync(int userId, int? currencyId, int? categoryId);
        Task<UserConfigDto?> UpdateCsvColumnMappingAsync(int userId, Dictionary<string, string>? mapping);
    }
}
