using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Models.Lookups;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Services
{
    public class RecurringExpenseService : IRecurringExpenseService
    {
        // OperationSource seed: 1=SingleWeb, 2=SingleMobile, 3=BulkWeb (constraints.md)
        private const int SourceSingleWeb = 1;
        private const int SourceBulkWeb = 3;

        private readonly IRecurringExpenseRepository _repository;
        private readonly ILookupCacheService _lookupCache;
        private readonly IExpenseService _expenseService;

        public RecurringExpenseService(
            IRecurringExpenseRepository repository,
            ILookupCacheService lookupCache,
            IExpenseService expenseService)
        {
            _repository = repository;
            _lookupCache = lookupCache;
            _expenseService = expenseService;
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

        public async Task<IEnumerable<RecurringExpenseDto>> GetAllAsync(int userId, bool includeInactive)
        {
            var recurring = await _repository.GetPagedAsync(userId, includeInactive);

            var result = new List<RecurringExpenseDto>();
            foreach (var r in recurring)
            {
                result.Add(await MapToDtoAsync(r));
            }
            return result;
        }

        public async Task<RecurringExpenseDto?> GetByIdAsync(int id, int userId)
        {
            var recurring = await _repository.GetByIdAsync(id, userId);
            if (recurring is null)
                return null;

            return await MapToDtoAsync(recurring);
        }

        public async Task<RecurringExpenseDto> CreateAsync(CreateRecurringExpenseRequest request, int userId)
        {
            var recurring = new RecurringExpense
            {
                UserId = userId,
                Description = request.Description,
                Amount = request.Amount,
                CurrencyId = request.CurrencyId,
                CategoryId = request.CategoryId,
                SubcategoryId = request.SubcategoryId,
                FamilyId = request.FamilyId,
                FrequencyId = request.FrequencyId,
                NextDueDate = request.NextDueDate,
                AutoCreate = request.AutoCreate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(recurring);

            var created = await _repository.GetByIdAsync(recurring.Id, userId);
            return await MapToDtoAsync(created!);
        }

        public async Task<RecurringExpenseDto?> UpdateAsync(int id, UpdateRecurringExpenseRequest request, int userId)
        {
            var existing = await _repository.GetByIdAsync(id, userId);
            if (existing is null)
                return null;

            existing.Description = request.Description;
            existing.Amount = request.Amount;
            existing.CurrencyId = request.CurrencyId;
            existing.CategoryId = request.CategoryId;
            existing.SubcategoryId = request.SubcategoryId;
            existing.FamilyId = request.FamilyId;
            existing.FrequencyId = request.FrequencyId;
            existing.NextDueDate = request.NextDueDate;
            existing.AutoCreate = request.AutoCreate;
            existing.IsActive = request.IsActive;

            await _repository.UpdateAsync(existing);

            var updated = await _repository.GetByIdAsync(id, userId);
            return await MapToDtoAsync(updated!);
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            return await _repository.SoftDeleteAsync(id, userId);
        }

        public async Task<ExpenseDto?> ConfirmAsync(int id, int userId)
        {
            var existing = await _repository.GetByIdAsync(id, userId);
            if (existing is null)
                return null;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (existing.NextDueDate > today)
                throw new RecurringExpenseNotDueException();

            var createRequest = new CreateExpenseRequest
            {
                Amount = existing.Amount,
                CurrencyId = existing.CurrencyId,
                Date = existing.NextDueDate,
                CategoryId = existing.CategoryId,
                SubcategoryId = existing.SubcategoryId,
                Description = existing.Description,
                FamilyIds = existing.FamilyId.HasValue ? [existing.FamilyId.Value] : null,
                TagIds = null
            };

            var expenseDto = await _expenseService.AddAsync(createRequest, userId, SourceSingleWeb);

            existing.LastGeneratedDate = existing.NextDueDate;
            existing.NextDueDate = RecurringExpenseScheduler.AdvanceNextDueDate(existing.NextDueDate, existing.FrequencyId);
            await _repository.UpdateAsync(existing);

            return expenseDto;
        }

        public async Task GenerateDueAsync(DateOnly asOfDate, CancellationToken cancellationToken = default)
        {
            var due = await _repository.GetDueForGenerationAsync(asOfDate);

            foreach (var recurring in due)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!recurring.AutoCreate)
                    continue;

                try
                {
                    var request = new CreateExpenseRequest
                    {
                        Amount = recurring.Amount,
                        CurrencyId = recurring.CurrencyId,
                        Date = recurring.NextDueDate,
                        CategoryId = recurring.CategoryId,
                        SubcategoryId = recurring.SubcategoryId,
                        Description = recurring.Description,
                        FamilyIds = recurring.FamilyId.HasValue ? [recurring.FamilyId.Value] : null,
                        TagIds = null
                    };

                    await _expenseService.AddAsync(request, recurring.UserId, SourceBulkWeb);

                    recurring.LastGeneratedDate = recurring.NextDueDate;
                    recurring.NextDueDate = RecurringExpenseScheduler.AdvanceNextDueDate(recurring.NextDueDate, recurring.FrequencyId);
                    await _repository.UpdateAsync(recurring);
                }
                catch
                {
                    // Best-effort: one bad template must not block the rest of the batch.
                }
            }
        }

        private async Task<RecurringExpenseDto> MapToDtoAsync(RecurringExpense r)
        {
            return new RecurringExpenseDto
            {
                Id = r.Id,
                Description = r.Description,
                Amount = r.Amount,
                CurrencyId = r.CurrencyId,
                Currency = r.Currency is null ? null : new CurrencyDto
                {
                    Id = r.Currency.Id,
                    Code = r.Currency.Code,
                    Name = r.Currency.Name,
                    Symbol = r.Currency.Symbol,
                    Decimals = r.Currency.Decimals
                },
                CategoryId = r.CategoryId,
                Category = r.Category is null ? null : new SubcategoryDto
                {
                    Id = r.Category.Id,
                    Name = r.Category.Name,
                    Description = r.Category.Description,
                    Icon = r.Category.Icon
                },
                SubcategoryId = r.SubcategoryId,
                Subcategory = r.Subcategory is null ? null : new SubcategoryDto
                {
                    Id = r.Subcategory.Id,
                    Name = r.Subcategory.Name,
                    Description = r.Subcategory.Description,
                    Icon = r.Subcategory.Icon
                },
                FamilyId = r.FamilyId,
                FrequencyId = r.FrequencyId,
                NextDueDate = r.NextDueDate,
                Frequency = await _lookupCache.GetNameAsync<RecurrenceFrequency>(r.FrequencyId),
                IsActive = r.IsActive,
                AutoCreate = r.AutoCreate
            };
        }
    }
}
