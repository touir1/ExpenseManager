using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Models.Lookups;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Services
{
    public class RecurringExpenseService : IRecurringExpenseService
    {
        private readonly IRecurringExpenseRepository _repository;
        private readonly ILookupCacheService _lookupCache;

        public RecurringExpenseService(IRecurringExpenseRepository repository, ILookupCacheService lookupCache)
        {
            _repository = repository;
            _lookupCache = lookupCache;
        }

        public async Task<IEnumerable<RecurringExpenseDto>> GetUpcomingAsync(int userId, int take)
        {
            var recurring = await _repository.GetUpcomingAsync(userId, take);

            var result = new List<RecurringExpenseDto>();
            foreach (var r in recurring)
            {
                result.Add(await MapToDtoAsync(r));
            }
            return result;
        }

        private async Task<RecurringExpenseDto> MapToDtoAsync(RecurringExpense r)
        {
            return new RecurringExpenseDto
            {
                Id = r.Id,
                Description = r.Description,
                Amount = r.Amount,
                Currency = r.Currency is null ? null : new CurrencyDto
                {
                    Id = r.Currency.Id,
                    Code = r.Currency.Code,
                    Name = r.Currency.Name,
                    Symbol = r.Currency.Symbol,
                    Decimals = r.Currency.Decimals
                },
                Category = r.Category is null ? null : new SubcategoryDto
                {
                    Id = r.Category.Id,
                    Name = r.Category.Name,
                    Description = r.Category.Description,
                    Icon = r.Category.Icon
                },
                Subcategory = r.Subcategory is null ? null : new SubcategoryDto
                {
                    Id = r.Subcategory.Id,
                    Name = r.Subcategory.Name,
                    Description = r.Subcategory.Description,
                    Icon = r.Subcategory.Icon
                },
                NextDueDate = r.NextDueDate,
                Frequency = await _lookupCache.GetNameAsync<RecurrenceFrequency>(r.FrequencyId)
            };
        }
    }
}
