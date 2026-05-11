using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IExpenseAuditService _auditService;
        private readonly IFamilyRepository _familyRepository;

        public ExpenseService(
            IExpenseRepository expenseRepository,
            IExpenseAuditService auditService,
            IFamilyRepository familyRepository)
        {
            _expenseRepository = expenseRepository;
            _auditService = auditService;
            _familyRepository = familyRepository;
        }

        public async Task<ExpenseDto> AddAsync(CreateExpenseRequest request, int userId, int sourceId)
        {
            var expense = new Expense
            {
                UserId = userId,
                Amount = request.Amount,
                CurrencyId = request.CurrencyId,
                Date = request.Date,
                CategoryId = request.CategoryId,
                SubcategoryId = request.SubcategoryId,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId,
                CreatedFromId = sourceId
            };

            await _expenseRepository.AddAsync(expense);
            await _auditService.WriteAddAuditAsync(expense, userId, sourceId);
            await WriteAttributionsAsync(expense.Id, request.FamilyIds, userId);

            return MapToDto(expense);
        }

        public async Task<ExpenseDto?> UpdateAsync(long id, UpdateExpenseRequest request, int userId, int sourceId)
        {
            var existing = await _expenseRepository.GetByIdAsync(id, userId);
            if (existing is null)
                return null;

            var before = CloneExpense(existing);

            existing.Amount = request.Amount;
            existing.CurrencyId = request.CurrencyId;
            existing.Date = request.Date;
            existing.CategoryId = request.CategoryId;
            existing.SubcategoryId = request.SubcategoryId;
            existing.Description = request.Description;
            existing.ModifiedAt = DateTime.UtcNow;
            existing.ModifiedById = userId;
            existing.ModifiedFromId = sourceId;

            await _expenseRepository.UpdateAsync(existing);
            await _auditService.WriteUpdateAuditAsync(before, existing, userId, sourceId);

            await _familyRepository.ClearAttributionsAsync(existing.Id);
            await WriteAttributionsAsync(existing.Id, request.FamilyIds, userId);

            return MapToDto(existing);
        }

        public async Task<bool> DeleteAsync(long id, int userId, int sourceId)
        {
            var existing = await _expenseRepository.GetByIdAsync(id, userId);
            if (existing is null)
                return false;

            await _auditService.WriteDeleteAuditAsync(existing, userId, sourceId);
            await _expenseRepository.SoftDeleteAsync(existing);

            return true;
        }

        public async Task<ExpenseDto?> GetByIdAsync(long id, int userId)
        {
            var expense = await _expenseRepository.GetByIdAsync(id, userId);
            return expense is null ? null : MapToDto(expense);
        }

        public async Task<ExpensePagedResult> GetPagedAsync(ExpenseFilterDto filter, int userId)
        {
            var (items, totalCount) = await _expenseRepository.GetPagedAsync(filter, userId);
            var pageSize = Math.Max(1, filter.PageSize);
            var page = Math.Max(1, filter.Page);

            return new ExpensePagedResult
            {
                Items = items.Select(MapToDto),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        private async Task WriteAttributionsAsync(long expenseId, int[]? familyIds, int userId)
        {
            IEnumerable<int> targetIds;

            if (familyIds == null)
            {
                var defaultFamily = await _familyRepository.GetDefaultFamilyForUserAsync(userId);
                if (defaultFamily is null)
                    return;
                targetIds = [defaultFamily.Id];
            }
            else
            {
                targetIds = familyIds;
            }

            var now = DateTime.UtcNow;
            var attributions = new List<ExpenseFamilyAttribution>();

            foreach (var familyId in targetIds)
            {
                if (!await _familyRepository.IsMemberAsync(familyId, userId))
                    throw new FamilyForbiddenException("FAMILY_FORBIDDEN");

                attributions.Add(new ExpenseFamilyAttribution
                {
                    ExpenseId = expenseId,
                    FamilyId = familyId,
                    AttributedAt = now,
                    AttributedById = userId
                });
            }

            if (attributions.Count > 0)
                await _familyRepository.AddAttributionsAsync(attributions);
        }

        private static ExpenseDto MapToDto(Expense e) => new()
        {
            Id = e.Id,
            Amount = e.Amount,
            Currency = e.Currency is null ? null : new CurrencyDto
            {
                Id = e.Currency.Id,
                Code = e.Currency.Code,
                Name = e.Currency.Name,
                Symbol = e.Currency.Symbol,
                Decimals = e.Currency.Decimals
            },
            Date = e.Date,
            Category = e.Category is null ? null : new SubcategoryDto
            {
                Id = e.Category.Id,
                Name = e.Category.Name,
                Description = e.Category.Description
            },
            Subcategory = e.Subcategory is null ? null : new SubcategoryDto
            {
                Id = e.Subcategory.Id,
                Name = e.Subcategory.Name,
                Description = e.Subcategory.Description
            },
            Description = e.Description,
            CreatedAt = e.CreatedAt,
            ModifiedAt = e.ModifiedAt,
            ModifiedFrom = e.ModifiedFrom?.Name
        };

        private static Expense CloneExpense(Expense e) => new()
        {
            Id = e.Id,
            UserId = e.UserId,
            Amount = e.Amount,
            CurrencyId = e.CurrencyId,
            Date = e.Date,
            CategoryId = e.CategoryId,
            SubcategoryId = e.SubcategoryId,
            Description = e.Description,
            CreatedAt = e.CreatedAt,
            CreatedById = e.CreatedById,
            CreatedFromId = e.CreatedFromId,
            ModifiedAt = e.ModifiedAt,
            ModifiedById = e.ModifiedById,
            ModifiedFromId = e.ModifiedFromId
        };
    }
}
