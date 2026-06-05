using System.Text.Json;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Messaging.Messages;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Repositories.External.Contracts;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IExpenseAuditService _auditService;
        private readonly IFamilyRepository _familyRepository;
        private readonly ITagRepository _tagRepository;
        private readonly ICurrencyRateService _currencyRateService;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IExpensesOutboxRepository _outboxRepo;
        private readonly IUserRepository _userRepo;

        public ExpenseService(
            IExpenseRepository expenseRepository,
            IExpenseAuditService auditService,
            IFamilyRepository familyRepository,
            ITagRepository tagRepository,
            ICurrencyRateService currencyRateService,
            ICurrencyRepository currencyRepository,
            IExpensesOutboxRepository outboxRepo,
            IUserRepository userRepo)
        {
            _expenseRepository = expenseRepository;
            _auditService = auditService;
            _familyRepository = familyRepository;
            _tagRepository = tagRepository;
            _currencyRateService = currencyRateService;
            _currencyRepository = currencyRepository;
            _outboxRepo = outboxRepo;
            _userRepo = userRepo;
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

            var tagDtos = await WriteExpenseTagsAsync(expense.Id, request.TagIds, userId);
            var tagsSnapshot = string.Join(",", tagDtos.Select(t => t.Id));

            await _auditService.WriteAddAuditAsync(expense, userId, sourceId, tagsSnapshot);
            var (attributedFamilyIds, attributedFamilyDtos) = await WriteAttributionsAsync(expense.Id, request.FamilyIds, userId);

            await EnqueueExpenseFamilyNotificationsAsync(expense, attributedFamilyIds, userId, FamilyEventType.ExpenseAdded);

            return MapToDto(expense, tagDtos, attributedFamilyDtos);
        }

        public async Task<ExpenseDto?> UpdateAsync(long id, UpdateExpenseRequest request, int userId, int sourceId)
        {
            var existing = await _expenseRepository.GetByIdAsync(id, userId);
            if (existing is null)
                return null;

            var before = CloneExpense(existing);
            var beforeTags = string.Join(",", existing.ExpenseTags.Select(et => et.TagId));

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
            await _expenseRepository.ClearExpenseTagsAsync(existing.Id);
            var afterTagDtos = await WriteExpenseTagsAsync(existing.Id, request.TagIds, userId);
            var afterTags = string.Join(",", afterTagDtos.Select(t => t.Id));

            await _auditService.WriteUpdateAuditAsync(before, existing, userId, sourceId, beforeTags, afterTags);
            await _familyRepository.ClearAttributionsAsync(existing.Id);
            var (_, updatedFamilyDtos) = await WriteAttributionsAsync(existing.Id, request.FamilyIds, userId);

            return MapToDto(existing, afterTagDtos, updatedFamilyDtos);
        }

        public async Task<bool> DeleteAsync(long id, int userId, int sourceId)
        {
            var existing = await _expenseRepository.GetByIdAsync(id, userId);
            if (existing is null)
                return false;

            var sharedFamilyIds = existing.ExpenseFamilyAttributions
                .Where(a => !a.Family.IsDeleted && !a.Family.IsDefault)
                .Select(a => a.FamilyId)
                .ToList();

            var tags = string.Join(",", existing.ExpenseTags.Select(et => et.TagId));
            await _auditService.WriteDeleteAuditAsync(existing, userId, sourceId, tags);
            await _expenseRepository.SoftDeleteAsync(existing);

            await EnqueueExpenseFamilyNotificationsAsync(existing, sharedFamilyIds, userId, FamilyEventType.ExpenseDeleted);

            return true;
        }

        public async Task<ExpenseDto?> GetByIdAsync(long id, int userId, int? displayCurrencyId = null)
        {
            var expense = await _expenseRepository.GetByIdAsync(id, userId);
            if (expense is null)
                return null;

            var (convertedAmount, displayCurrency) = await ResolveConversionAsync(expense.CurrencyId, expense.Date, expense.Amount, displayCurrencyId);
            return MapToDto(expense, convertedAmount: convertedAmount, displayCurrency: displayCurrency);
        }

        public async Task<ExpensePagedResult> GetPagedAsync(ExpenseFilterDto filter, int userId)
        {
            var (items, totalCount) = await _expenseRepository.GetPagedAsync(filter, userId);
            var pageSize = Math.Max(1, filter.PageSize);
            var page = Math.Max(1, filter.Page);

            var dtos = new List<ExpenseDto>();
            foreach (var e in items)
            {
                var (convertedAmount, displayCurrency) = await ResolveConversionAsync(e.CurrencyId, e.Date, e.Amount, filter.DisplayCurrencyId);
                dtos.Add(MapToDto(e, convertedAmount: convertedAmount, displayCurrency: displayCurrency));
            }

            return new ExpensePagedResult
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        private async Task<(decimal? convertedAmount, CurrencyDto? displayCurrency)> ResolveConversionAsync(
            int expenseCurrencyId, DateOnly date, decimal amount, int? displayCurrencyId)
        {
            if (displayCurrencyId is null || displayCurrencyId == expenseCurrencyId)
                return (null, null);

            var rate = await _currencyRateService.ResolveRateAsync(expenseCurrencyId, displayCurrencyId.Value, date);
            if (rate is null)
                return (null, null);

            var currency = await _currencyRepository.GetByIdAsync(displayCurrencyId.Value);
            if (currency is null)
                return (null, null);

            var currencyDto = new CurrencyDto
            {
                Id = currency.Id,
                Code = currency.Code,
                Name = currency.Name,
                Symbol = currency.Symbol,
                Decimals = currency.Decimals
            };
            return (Math.Round(amount * rate.Value, 4), currencyDto);
        }

        private async Task<IEnumerable<TagDto>> WriteExpenseTagsAsync(long expenseId, int[]? tagIds, int userId)
        {
            if (tagIds is null || tagIds.Length == 0)
                return [];

            foreach (var tagId in tagIds)
                if (!await _tagRepository.IsVisibleAsync(userId, tagId))
                    throw new FamilyForbiddenException(ServiceErrors.TagNotVisible);

            foreach (var tagId in tagIds)
                await _tagRepository.EnsureUserTagAsync(userId, tagId);

            await _expenseRepository.AddExpenseTagsAsync(
                tagIds.Select(tagId => new ExpenseTag { ExpenseId = expenseId, TagId = tagId }));

            var tags = await _tagRepository.GetByIdsAsync(tagIds);
            return tags.Select(t => new TagDto { Id = t.Id, Name = t.Name });
        }

        private async Task<(IReadOnlyList<int> AllIds, IReadOnlyList<FamilyNameDto> NonDefaultFamilies)> WriteAttributionsAsync(long expenseId, int[]? familyIds, int userId)
        {
            IReadOnlyList<int> targetIds;
            IReadOnlyList<FamilyNameDto> nonDefaultFamilies;

            if (familyIds == null)
            {
                var defaultFamily = await _familyRepository.GetDefaultFamilyForUserAsync(userId);
                if (defaultFamily is null)
                    return ([], []);
                targetIds = [defaultFamily.Id];
                nonDefaultFamilies = [];
            }
            else
            {
                var userFamilies = (await _familyRepository.GetFamiliesByUserAsync(userId))
                    .Where(x => !x.Family.IsDeleted)
                    .ToDictionary(x => x.Family.Id, x => x.Family);

                var ids = new List<int>(familyIds.Length);
                var dtos = new List<FamilyNameDto>(familyIds.Length);

                foreach (var familyId in familyIds)
                {
                    if (!userFamilies.TryGetValue(familyId, out var family))
                        throw new FamilyForbiddenException(ServiceErrors.FamilyForbidden);

                    ids.Add(familyId);
                    if (!family.IsDefault)
                        dtos.Add(new FamilyNameDto { Id = family.Id, Name = family.Name });
                }

                targetIds = ids;
                nonDefaultFamilies = dtos;
            }

            var now = DateTime.UtcNow;
            var attributions = targetIds.Select(familyId => new ExpenseFamilyAttribution
            {
                ExpenseId = expenseId,
                FamilyId = familyId,
                AttributedAt = now,
                AttributedById = userId
            }).ToList();

            if (attributions.Count > 0)
                await _familyRepository.AddAttributionsAsync(attributions);

            return (targetIds, nonDefaultFamilies);
        }

        private async Task EnqueueExpenseFamilyNotificationsAsync(
            Expense expense, IReadOnlyList<int> familyIds, int actorUserId, string eventType)
        {
            if (familyIds.Count == 0)
                return;

            try
            {
                var actor = await _userRepo.GetUserByIdAsync(actorUserId);
                var actorName = actor is not null ? $"{actor.FirstName} {actor.LastName}".Trim() : string.Empty;
                var currency = await _currencyRepository.GetByIdAsync(expense.CurrencyId);
                var currencyCode = currency?.Code ?? string.Empty;

                foreach (var familyId in familyIds)
                {
                    var (family, members) = await _familyRepository.GetByIdWithMembersAsync(familyId);
                    if (family is null || family.IsDefault) continue;

                    var coMemberIds = members
                        .Select(m => m.UserId)
                        .Where(id => id != actorUserId)
                        .ToList();

                    if (coMemberIds.Count == 0) continue;

                    var msgId = Guid.NewGuid().ToString();
                    await _outboxRepo.EnqueueAsync(new OutboxEvent
                    {
                        MessageId = msgId,
                        EventType = eventType,
                        Payload = JsonSerializer.Serialize(new FamilyExpenseEventMessage
                        {
                            MessageId = msgId,
                            EventType = eventType,
                            FamilyId = familyId,
                            FamilyName = family.Name,
                            ExpenseId = expense.Id,
                            Amount = expense.Amount,
                            CurrencyCode = currencyCode,
                            ActorName = actorName,
                            ActorUserId = actorUserId,
                            MemberUserIds = coMemberIds
                        }),
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        private static ExpenseDto MapToDto(Expense e, IEnumerable<TagDto>? explicitTags = null, IEnumerable<FamilyNameDto>? explicitFamilies = null, decimal? convertedAmount = null, CurrencyDto? displayCurrency = null) => new()
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
                Description = e.Category.Description,
                Icon = e.Category.Icon
            },
            Subcategory = e.Subcategory is null ? null : new SubcategoryDto
            {
                Id = e.Subcategory.Id,
                Name = e.Subcategory.Name,
                Description = e.Subcategory.Description,
                Icon = e.Subcategory.Icon
            },
            Description = e.Description,
            CreatedAt = e.CreatedAt,
            ModifiedAt = e.ModifiedAt,
            ModifiedFrom = e.ModifiedFrom?.Name,
            Tags = explicitTags ?? e.ExpenseTags.Select(et => new TagDto { Id = et.Tag.Id, Name = et.Tag.Name }),
            Families = explicitFamilies ?? e.ExpenseFamilyAttributions
                .Where(a => a.Family != null && !a.Family.IsDeleted && !a.Family.IsDefault)
                .Select(a => new FamilyNameDto { Id = a.Family.Id, Name = a.Family.Name }),
            ConvertedAmount = convertedAmount,
            DisplayCurrency = displayCurrency
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
