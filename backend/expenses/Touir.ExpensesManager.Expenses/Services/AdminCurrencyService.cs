using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Services
{
    public class AdminCurrencyService : IAdminCurrencyService
    {
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly ICurrencyRateRepository _rateRepository;

        public AdminCurrencyService(
            ICurrencyRepository currencyRepository,
            IExpenseRepository expenseRepository,
            ICurrencyRateRepository rateRepository)
        {
            _currencyRepository = currencyRepository;
            _expenseRepository = expenseRepository;
            _rateRepository = rateRepository;
        }

        public async Task<CurrencyDto> AddCurrencyAsync(string code, string name, string symbol, int decimals)
        {
            var upperCode = code.ToUpperInvariant();

            if (await _currencyRepository.ExistsByCodeAsync(upperCode))
                throw new InvalidOperationException("CURRENCY_CODE_ALREADY_EXISTS");

            var currency = new Currency { Code = upperCode, Name = name, Symbol = symbol, Decimals = decimals };
            var added = await _currencyRepository.AddAsync(currency);

            return MapToDto(added);
        }

        public async Task<CurrencyDto> UpdateCurrencyAsync(int id, string name, string symbol, int decimals)
        {
            var currency = await _currencyRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Currency {id} not found.");

            currency.Name = name;
            currency.Symbol = symbol;
            currency.Decimals = decimals;

            var updated = await _currencyRepository.UpdateAsync(currency);
            return MapToDto(updated);
        }

        public async Task DeleteCurrencyAsync(int id)
        {
            var currency = await _currencyRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Currency {id} not found.");

            var usedInExpenses = (await _expenseRepository.GetDistinctCurrencyIdsAsync()).Contains(id);
            var usedInRates = await _rateRepository.IsUsedInRatesAsync(id);

            if (usedInExpenses || usedInRates)
                throw new InvalidOperationException("CURRENCY_IN_USE");

            await _currencyRepository.DeleteAsync(currency.Id);
        }

        public async Task<IEnumerable<CurrencyDefaultRateDto>> GetCurrencyDefaultsAsync(int id)
            => await _currencyRepository.GetDefaultsForSourceAsync(id);

        private static CurrencyDto MapToDto(Currency c) =>
            new() { Id = c.Id, Code = c.Code, Name = c.Name, Symbol = c.Symbol, Decimals = c.Decimals };
    }
}
