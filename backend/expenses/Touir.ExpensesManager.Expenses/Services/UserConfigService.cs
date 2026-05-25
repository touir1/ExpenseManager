using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Services
{
    public class UserConfigService : IUserConfigService
    {
        private readonly IUserConfigRepository _configRepo;
        private readonly ICurrencyRepository _currencyRepo;

        public UserConfigService(IUserConfigRepository configRepo, ICurrencyRepository currencyRepo)
        {
            _configRepo = configRepo;
            _currencyRepo = currencyRepo;
        }

        public async Task<UserConfigDto> GetAsync(int userId)
        {
            var config = await _configRepo.GetByUserIdAsync(userId);
            return MapToDto(config);
        }

        public async Task<UserConfigDto?> UpdateAsync(int userId, int? currencyId)
        {
            if (currencyId.HasValue)
            {
                var currency = await _currencyRepo.GetByIdAsync(currencyId.Value);
                if (currency is null)
                    return null;
            }

            var config = await _configRepo.UpsertAsync(userId, currencyId);
            return MapToDto(config);
        }

        private static UserConfigDto MapToDto(Models.UserConfig? config)
        {
            if (config is null)
                return new UserConfigDto();

            return new UserConfigDto
            {
                DefaultCurrencyId = config.DefaultCurrencyId,
                DefaultCurrency = config.DefaultCurrency is null ? null : new CurrencyDto
                {
                    Id = config.DefaultCurrency.Id,
                    Code = config.DefaultCurrency.Code,
                    Name = config.DefaultCurrency.Name,
                    Symbol = config.DefaultCurrency.Symbol,
                    Decimals = config.DefaultCurrency.Decimals,
                }
            };
        }
    }
}
