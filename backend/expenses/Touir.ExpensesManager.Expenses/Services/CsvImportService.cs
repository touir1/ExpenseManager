using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System.Globalization;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Services
{
    public class CsvImportService : ICsvImportService
    {
        private const int SourceBulkWeb = 3;
        private const int MaxRows = 500;

        private readonly ICurrencyRepository _currencyRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly ITagService _tagService;
        private readonly IExpenseService _expenseService;

        public CsvImportService(
            ICurrencyRepository currencyRepository,
            ICategoryRepository categoryRepository,
            IFamilyRepository familyRepository,
            ITagService tagService,
            IExpenseService expenseService)
        {
            _currencyRepository = currencyRepository;
            _categoryRepository = categoryRepository;
            _familyRepository = familyRepository;
            _tagService = tagService;
            _expenseService = expenseService;
        }

        public async Task<CsvImportPreviewDto> ParseAndValidateAsync(Stream csvStream, int userId)
        {
            var currencies = (await _currencyRepository.GetAllAsync())
                .ToDictionary(c => c.Code.ToUpperInvariant(), c => c.Id);

            var allCategories = (await _categoryRepository.GetAllActiveAsync()).ToList();
            var topCategories = allCategories
                .Where(c => c.ParentCategoryId == null)
                .ToDictionary(c => c.Name.ToLowerInvariant(), c => c);
            var subCategories = allCategories
                .Where(c => c.ParentCategoryId != null)
                .GroupBy(c => c.ParentCategoryId!.Value)
                .ToDictionary(g => g.Key, g => g.ToDictionary(c => c.Name.ToLowerInvariant(), c => c));

            var userFamilies = await _familyRepository.GetFamiliesByUserAsync(userId);
            var memberFamilyIds = userFamilies.Select(f => f.Family.Id).ToHashSet();

            var rows = new List<CsvImportRowPreviewDto>();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null,
            };

            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, config);

            int rowNumber = 0;
            await csv.ReadAsync();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                rowNumber++;
                if (rowNumber > MaxRows)
                    break;

                var rawDate = csv.TryGetField<string>("date", out var d) ? d?.Trim() : null;
                var rawAmount = csv.TryGetField<string>("amount", out var a) ? a?.Trim() : null;
                var rawCurrency = csv.TryGetField<string>("currency_code", out var cc) ? cc?.Trim() : null;
                var rawCategory = csv.TryGetField<string>("category", out var cat) ? cat?.Trim() : null;
                var rawSubcategory = csv.TryGetField<string>("subcategory", out var sub) ? sub?.Trim() : null;
                var rawDescription = csv.TryGetField<string>("description", out var desc) ? desc?.Trim() : null;
                var rawTags = csv.TryGetField<string>("tags", out var tags) ? tags?.Trim() : null;
                var rawFamilies = csv.TryGetField<string>("families", out var fam) ? fam?.Trim() : null;

                var errors = new List<string>();
                DateOnly? parsedDate = null;
                decimal? parsedAmount = null;
                int? currencyId = null;
                int? categoryId = null;
                int? subcategoryId = null;
                int[]? familyIds = null;

                // date
                if (string.IsNullOrEmpty(rawDate))
                    errors.Add("DATE_INVALID");
                else if (!DateOnly.TryParseExact(rawDate, "yyyy-MM-dd", null, DateTimeStyles.None, out var dt))
                    errors.Add("DATE_INVALID");
                else if (dt > DateOnly.FromDateTime(DateTime.UtcNow))
                    errors.Add("DATE_IN_FUTURE");
                else
                    parsedDate = dt;

                // amount
                if (string.IsNullOrEmpty(rawAmount))
                    errors.Add("AMOUNT_INVALID");
                else if (!decimal.TryParse(rawAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var amt))
                    errors.Add("AMOUNT_INVALID");
                else if (amt <= 0)
                    errors.Add("AMOUNT_MUST_BE_POSITIVE");
                else
                    parsedAmount = amt;

                // currency
                if (string.IsNullOrEmpty(rawCurrency))
                    errors.Add("CURRENCY_NOT_FOUND");
                else if (!currencies.TryGetValue(rawCurrency.ToUpperInvariant(), out var cid))
                    errors.Add("CURRENCY_NOT_FOUND");
                else
                    currencyId = cid;

                // category (optional)
                if (!string.IsNullOrEmpty(rawCategory))
                {
                    if (!topCategories.TryGetValue(rawCategory.ToLowerInvariant(), out var catEntity))
                        errors.Add("CATEGORY_NOT_FOUND");
                    else
                    {
                        categoryId = catEntity.Id;

                        // subcategory (optional, requires category)
                        if (!string.IsNullOrEmpty(rawSubcategory))
                        {
                            if (!subCategories.TryGetValue(catEntity.Id, out var subs) ||
                                !subs.TryGetValue(rawSubcategory.ToLowerInvariant(), out var subEntity))
                                errors.Add("SUBCATEGORY_INVALID");
                            else
                                subcategoryId = subEntity.Id;
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(rawSubcategory))
                {
                    errors.Add("SUBCATEGORY_INVALID");
                }

                // families (optional — empty = default family)
                if (!string.IsNullOrEmpty(rawFamilies))
                {
                    var parts = rawFamilies.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var ids = new List<int>();
                    foreach (var part in parts)
                    {
                        if (!int.TryParse(part, out var fid))
                        {
                            errors.Add("FAMILY_FORBIDDEN");
                            break;
                        }
                        if (!memberFamilyIds.Contains(fid))
                        {
                            errors.Add("FAMILY_FORBIDDEN");
                            break;
                        }
                        ids.Add(fid);
                    }
                    if (errors.All(e => e != "FAMILY_FORBIDDEN"))
                        familyIds = ids.Count > 0 ? [.. ids] : null;
                }

                // tags — parsed but not validated (auto-created on confirm)
                var tagNames = string.IsNullOrEmpty(rawTags)
                    ? null
                    : rawTags.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var isValid = errors.Count == 0;
                rows.Add(new CsvImportRowPreviewDto
                {
                    RowNumber = rowNumber,
                    IsValid = isValid,
                    Errors = [.. errors],
                    DateDisplay = rawDate,
                    AmountDisplay = rawAmount,
                    CurrencyDisplay = rawCurrency,
                    CategoryDisplay = rawCategory,
                    SubcategoryDisplay = rawSubcategory,
                    DescriptionDisplay = rawDescription,
                    TagNames = tagNames,
                    Date = isValid ? parsedDate : null,
                    Amount = isValid ? parsedAmount : null,
                    CurrencyId = isValid ? currencyId : null,
                    CategoryId = isValid ? categoryId : null,
                    SubcategoryId = isValid ? subcategoryId : null,
                    FamilyIds = isValid ? familyIds : null,
                });
            }

            return new CsvImportPreviewDto
            {
                TotalRows = rows.Count,
                ValidCount = rows.Count(r => r.IsValid),
                ErrorCount = rows.Count(r => !r.IsValid),
                Rows = rows,
            };
        }

        public async Task<CsvImportResultDto> ConfirmImportAsync(IEnumerable<CsvImportConfirmRowDto> rows, int userId)
        {
            int imported = 0;
            int skipped = 0;

            foreach (var row in rows)
            {
                try
                {
                    var tagIds = new List<int>();
                    if (row.TagNames is { Length: > 0 })
                    {
                        foreach (var name in row.TagNames)
                        {
                            var tag = await _tagService.UseTagAsync(name, userId);
                            tagIds.Add(tag.Id);
                        }
                    }

                    var request = new CreateExpenseRequest
                    {
                        Amount = row.Amount,
                        CurrencyId = row.CurrencyId,
                        Date = row.Date,
                        CategoryId = row.CategoryId,
                        SubcategoryId = row.SubcategoryId,
                        Description = row.Description,
                        TagIds = tagIds.Count > 0 ? [.. tagIds] : null,
                        FamilyIds = row.FamilyIds,
                    };

                    await _expenseService.AddAsync(request, userId, SourceBulkWeb);
                    imported++;
                }
                catch
                {
                    skipped++;
                }
            }

            return new CsvImportResultDto { Imported = imported, Skipped = skipped };
        }
    }
}
