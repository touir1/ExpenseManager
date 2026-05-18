using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _repo;
        private readonly IFamilyRepository _familyRepo;
        private readonly ICurrencyRateService _rateService;
        private readonly ICurrencyRepository _currencyRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IExpenseService _expenseService;

        public DashboardService(
            IDashboardRepository repo,
            IFamilyRepository familyRepo,
            ICurrencyRateService rateService,
            ICurrencyRepository currencyRepo,
            ICategoryRepository categoryRepo,
            IExpenseService expenseService)
        {
            _repo = repo;
            _familyRepo = familyRepo;
            _rateService = rateService;
            _currencyRepo = currencyRepo;
            _categoryRepo = categoryRepo;
            _expenseService = expenseService;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(
            int userId, int? familyId, DateOnly dateFrom, DateOnly dateTo, int? displayCurrencyId)
        {
            await CheckMembershipAsync(familyId, userId);

            var currentRows = await _repo.GetTotalsAsync(userId, familyId, dateFrom, dateTo);
            var categoryRows = await _repo.GetCategoryTotalsAsync(userId, familyId, dateFrom, dateTo);

            var duration = (dateTo.DayNumber - dateFrom.DayNumber) + 1;
            var prevDateTo = dateFrom.AddDays(-1);
            var prevDateFrom = prevDateTo.AddDays(-(duration - 1));
            var prevRows = await _repo.GetTotalsAsync(userId, familyId, prevDateFrom, prevDateTo);

            var currencies = displayCurrencyId.HasValue
                ? (await _currencyRepo.GetAllAsync()).ToDictionary(c => c.Id)
                : null;

            decimal totalAmount = currentRows.Sum(r => r.Amount);
            int expenseCount = currentRows.Sum(r => r.Count);

            decimal? convertedTotal = await ConvertRowsAsync(currentRows, displayCurrencyId, dateTo);
            decimal? prevTotal = await ConvertRowsAsync(prevRows, displayCurrencyId, prevDateTo);

            decimal? changePercent = null;
            if (convertedTotal.HasValue && prevTotal.HasValue && prevTotal.Value != 0)
                changePercent = Math.Round((convertedTotal.Value - prevTotal.Value) / prevTotal.Value * 100, 2);

            var topCategory = await ResolveTopCategoryAsync(categoryRows, displayCurrencyId, dateTo);

            CurrencyDto? displayCurrencyDto = null;
            if (displayCurrencyId.HasValue && currencies is not null && currencies.TryGetValue(displayCurrencyId.Value, out var dc))
                displayCurrencyDto = MapCurrencyDto(dc);

            return new DashboardSummaryDto
            {
                TotalAmount = totalAmount,
                ConvertedTotal = convertedTotal,
                DisplayCurrency = displayCurrencyDto,
                ExpenseCount = expenseCount,
                PreviousPeriodTotal = prevTotal,
                ChangePercent = changePercent,
                TopCategory = topCategory.Category,
                TopCategoryAmount = topCategory.Amount
            };
        }

        public async Task<IEnumerable<MonthlyBreakdownDto>> GetMonthlyAsync(
            int userId, int? familyId, DateOnly dateFrom, DateOnly dateTo, int? displayCurrencyId)
        {
            await CheckMembershipAsync(familyId, userId);

            var monthlyRows = await _repo.GetMonthlyTotalsAsync(userId, familyId, dateFrom, dateTo);
            var categoryRows = await _repo.GetMonthlyCategoryTotalsAsync(userId, familyId, dateFrom, dateTo);
            var categories = (await _categoryRepo.GetAllActiveAsync()).ToDictionary(c => c.Id);

            var grouped = monthlyRows
                .GroupBy(r => new { r.Year, r.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month);

            var result = new List<MonthlyBreakdownDto>();
            foreach (var monthGroup in grouped)
            {
                var rateDate = LastDayOfMonth(monthGroup.Key.Year, monthGroup.Key.Month);
                var rows = monthGroup.ToList();
                decimal total = rows.Sum(r => r.Amount);
                decimal? converted = await ConvertRowsAsync(
                    rows.Select(r => new DashboardRowAmount(r.CurrencyId, r.Amount)),
                    displayCurrencyId, rateDate);

                var catRows = categoryRows
                    .Where(c => c.Year == monthGroup.Key.Year && c.Month == monthGroup.Key.Month)
                    .ToList();

                var byCategory = await BuildCategoryAmountsAsync(catRows, categories, displayCurrencyId, rateDate);

                result.Add(new MonthlyBreakdownDto
                {
                    Year = monthGroup.Key.Year,
                    Month = monthGroup.Key.Month,
                    TotalAmount = total,
                    ConvertedTotal = converted,
                    ByCategory = byCategory
                });
            }

            return result;
        }

        public async Task<IEnumerable<CategoryBreakdownDto>> GetCategoriesAsync(
            int userId, int? familyId, DateOnly dateFrom, DateOnly dateTo, int? displayCurrencyId)
        {
            await CheckMembershipAsync(familyId, userId);

            var rows = await _repo.GetCategoryTotalsAsync(userId, familyId, dateFrom, dateTo);
            var categories = (await _categoryRepo.GetAllActiveAsync()).ToDictionary(c => c.Id);

            var parentGroups = rows.GroupBy(r => r.CategoryId);
            var breakdowns = new List<(int? catId, decimal raw, decimal? converted, List<CategoryAmountDto> subs)>();

            foreach (var parentGroup in parentGroups)
            {
                var subGroups = parentGroup.GroupBy(r => r.SubcategoryId);
                var subcategories = new List<CategoryAmountDto>();
                decimal rawTotal = 0m;
                decimal? convertedTotal = 0m;

                foreach (var subGroup in subGroups)
                {
                    var subRows = subGroup.ToList();
                    decimal subRaw = subRows.Sum(r => r.Amount);
                    decimal? subConverted = await ConvertRowsAsync(
                        subRows.Select(r => new DashboardRowAmount(r.CurrencyId, r.Amount)),
                        displayCurrencyId, dateTo);

                    rawTotal += subRaw;
                    convertedTotal = convertedTotal.HasValue && subConverted.HasValue
                        ? convertedTotal + subConverted
                        : null;

                    SubcategoryDto? subDto = subGroup.Key.HasValue && categories.TryGetValue(subGroup.Key.Value, out var sc)
                        ? new SubcategoryDto { Id = sc.Id, Name = sc.Name, Description = sc.Description }
                        : null;

                    subcategories.Add(new CategoryAmountDto
                    {
                        Category = subDto,
                        Amount = subRaw,
                        ConvertedAmount = subConverted
                    });
                }

                breakdowns.Add((parentGroup.Key, rawTotal, convertedTotal, subcategories));
            }

            decimal grandRaw = breakdowns.Sum(b => b.raw);
            decimal? grandConverted = breakdowns.All(b => b.converted.HasValue)
                ? breakdowns.Sum(b => b.converted!.Value)
                : null;
            decimal grandBase = grandConverted ?? grandRaw;

            return breakdowns.Select(b =>
            {
                SubcategoryDto? catDto = b.catId.HasValue && categories.TryGetValue(b.catId.Value, out var c)
                    ? new SubcategoryDto { Id = c.Id, Name = c.Name, Description = c.Description }
                    : null;
                decimal thisBase = b.converted ?? b.raw;
                decimal pct = grandBase > 0 ? Math.Round(thisBase / grandBase * 100, 2) : 0;
                return new CategoryBreakdownDto
                {
                    Category = catDto,
                    TotalAmount = b.raw,
                    ConvertedTotal = b.converted,
                    Percentage = pct,
                    Subcategories = b.subs
                };
            })
            .OrderByDescending(b => b.ConvertedTotal ?? b.TotalAmount)
            .ToList();
        }

        public async Task<IEnumerable<SameMonthYearlyDto>> GetSameMonthAcrossYearsAsync(
            int userId, int? familyId, int month, int? displayCurrencyId)
        {
            await CheckMembershipAsync(familyId, userId);

            var rows = await _repo.GetYearlyTotalsForMonthAsync(userId, familyId, month);
            int currentYear = DateOnly.FromDateTime(DateTime.UtcNow).Year;

            var result = new List<SameMonthYearlyDto>();
            foreach (var yearGroup in rows.GroupBy(r => r.Year).OrderBy(g => g.Key))
            {
                var rateDate = yearGroup.Key < currentYear
                    ? new DateOnly(yearGroup.Key, 12, 31)
                    : DateOnly.FromDateTime(DateTime.UtcNow);

                var groupRows = yearGroup.ToList();
                decimal total = groupRows.Sum(r => r.Amount);
                decimal? converted = await ConvertRowsAsync(
                    groupRows.Select(r => new DashboardRowAmount(r.CurrencyId, r.Amount)),
                    displayCurrencyId, rateDate);

                result.Add(new SameMonthYearlyDto
                {
                    Year = yearGroup.Key,
                    TotalAmount = total,
                    ConvertedTotal = converted
                });
            }

            return result;
        }

        public async Task<IEnumerable<CurrencyBreakdownDto>> GetByCurrencyAsync(
            int userId, int? familyId, DateOnly dateFrom, DateOnly dateTo, int? displayCurrencyId)
        {
            await CheckMembershipAsync(familyId, userId);

            var rows = await _repo.GetTotalsAsync(userId, familyId, dateFrom, dateTo);
            var currencies = (await _currencyRepo.GetAllAsync()).ToDictionary(c => c.Id);

            var result = new List<CurrencyBreakdownDto>();
            foreach (var row in rows.OrderByDescending(r => r.Amount))
            {
                decimal? converted = displayCurrencyId.HasValue && displayCurrencyId.Value != row.CurrencyId
                    ? row.Amount * await _rateService.ResolveRateAsync(row.CurrencyId, displayCurrencyId.Value, dateTo)
                    : null;

                currencies.TryGetValue(row.CurrencyId, out var currency);
                result.Add(new CurrencyBreakdownDto
                {
                    Currency = currency is not null ? MapCurrencyDto(currency) : new CurrencyDto { Id = row.CurrencyId },
                    TotalAmount = row.Amount,
                    ConvertedAmount = converted,
                    ExpenseCount = row.Count
                });
            }

            return result;
        }

        public async Task<ExpensePagedResult> GetRecentAsync(
            int userId, int? familyId, DateOnly? dateFrom, DateOnly? dateTo)
        {
            await CheckMembershipAsync(familyId, userId);

            var filter = new ExpenseFilterDto
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                Page = 1,
                PageSize = 10
            };
            return await _expenseService.GetPagedAsync(filter, userId);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private async Task CheckMembershipAsync(int? familyId, int userId)
        {
            if (familyId.HasValue && !await _familyRepo.IsMemberAsync(familyId.Value, userId))
                throw new FamilyForbiddenException();
        }

        private async Task<decimal?> ConvertRowsAsync(
            IEnumerable<CurrencyTotalRow> rows, int? toId, DateOnly rateDate)
        {
            if (!toId.HasValue) return null;
            return await ConvertRowsAsync(
                rows.Select(r => new DashboardRowAmount(r.CurrencyId, r.Amount)), toId, rateDate);
        }

        private async Task<decimal?> ConvertRowsAsync(
            IEnumerable<DashboardRowAmount> rows, int? toId, DateOnly rateDate)
        {
            if (!toId.HasValue) return null;
            decimal total = 0m;
            foreach (var row in rows)
            {
                if (row.CurrencyId == toId.Value)
                {
                    total += row.Amount;
                    continue;
                }
                var rate = await _rateService.ResolveRateAsync(row.CurrencyId, toId.Value, rateDate);
                if (rate is null) return null;
                total += row.Amount * rate.Value;
            }
            return total;
        }

        private async Task<(SubcategoryDto? Category, decimal? Amount)> ResolveTopCategoryAsync(
            IEnumerable<CategoryTotalRow> rows, int? displayCurrencyId, DateOnly rateDate)
        {
            var categories = (await _categoryRepo.GetAllActiveAsync()).ToDictionary(c => c.Id);

            const int uncategorisedKey = -1;
            var parentTotals = new Dictionary<int, decimal>();
            foreach (var row in rows)
            {
                decimal amount;
                if (displayCurrencyId.HasValue && row.CurrencyId != displayCurrencyId.Value)
                {
                    var rate = await _rateService.ResolveRateAsync(row.CurrencyId, displayCurrencyId.Value, rateDate);
                    amount = rate.HasValue ? row.Amount * rate.Value : row.Amount;
                }
                else
                {
                    amount = row.Amount;
                }

                int key = row.CategoryId ?? uncategorisedKey;
                parentTotals[key] = parentTotals.GetValueOrDefault(key) + amount;
            }

            if (parentTotals.Count == 0) return (null, null);

            var topKey = parentTotals.MaxBy(kvp => kvp.Value).Key;
            SubcategoryDto? dto = topKey != uncategorisedKey && categories.TryGetValue(topKey, out var c)
                ? new SubcategoryDto { Id = c.Id, Name = c.Name, Description = c.Description }
                : null;

            return (dto, parentTotals[topKey]);
        }

        private async Task<IEnumerable<CategoryAmountDto>> BuildCategoryAmountsAsync(
            IEnumerable<MonthlyCategoryTotalRow> rows,
            Dictionary<int, Models.Category> categories,
            int? displayCurrencyId,
            DateOnly rateDate)
        {
            var grouped = rows.GroupBy(r => r.CategoryId);
            var result = new List<CategoryAmountDto>();

            foreach (var group in grouped)
            {
                var groupList = group.ToList();
                decimal raw = groupList.Sum(r => r.Amount);
                decimal? converted = await ConvertRowsAsync(
                    groupList.Select(r => new DashboardRowAmount(r.CurrencyId, r.Amount)),
                    displayCurrencyId, rateDate);

                SubcategoryDto? catDto = group.Key.HasValue && categories.TryGetValue(group.Key.Value, out var c)
                    ? new SubcategoryDto { Id = c.Id, Name = c.Name, Description = c.Description }
                    : null;

                result.Add(new CategoryAmountDto
                {
                    Category = catDto,
                    Amount = raw,
                    ConvertedAmount = converted
                });
            }

            return result.OrderByDescending(r => r.ConvertedAmount ?? r.Amount);
        }

        private static DateOnly LastDayOfMonth(int year, int month)
            => new DateOnly(year, month, DateTime.DaysInMonth(year, month));

        private static CurrencyDto MapCurrencyDto(Models.Currency c)
            => new() { Id = c.Id, Code = c.Code, Name = c.Name, Symbol = c.Symbol, Decimals = c.Decimals };

        private record DashboardRowAmount(int CurrencyId, decimal Amount);
    }
}
