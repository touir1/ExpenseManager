using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Services
{
    public class AdminCurrencyService : IAdminCurrencyService
    {
        private readonly ICurrencyRepository _currencyRepository;

        public AdminCurrencyService(ICurrencyRepository currencyRepository)
        {
            _currencyRepository = currencyRepository;
        }

        public async Task<CurrencyDto> AddCurrencyAsync(string code, string name, string symbol, int decimals)
        {
            var upperCode = code.ToUpperInvariant();

            if (await _currencyRepository.ExistsByCodeAsync(upperCode))
                throw new InvalidOperationException("CURRENCY_CODE_ALREADY_EXISTS");

            var currency = new Currency { Code = upperCode, Name = name, Symbol = symbol, Decimals = decimals };
            var added = await _currencyRepository.AddAsync(currency);

            return new CurrencyDto { Id = added.Id, Code = added.Code, Name = added.Name, Symbol = added.Symbol, Decimals = added.Decimals };
        }
    }
}
