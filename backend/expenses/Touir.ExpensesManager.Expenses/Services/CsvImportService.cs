using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Models;
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
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null,
            };

            var rawRows = new List<RawCsvRowDto>();
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, config);

            await csv.ReadAsync();
            csv.ReadHeader();

            int rowNumber = 0;
            while (await csv.ReadAsync())
            {
                rowNumber++;
                if (rowNumber > MaxRows)
                    break;

                rawRows.Add(new RawCsvRowDto
                {
                    RowNumber = rowNumber,
                    Date = csv.TryGetField<string>("date", out var d) ? d?.Trim() : null,
                    Amount = csv.TryGetField<string>("amount", out var a) ? a?.Trim() : null,
                    CurrencyCode = csv.TryGetField<string>("currency_code", out var cc) ? cc?.Trim() : null,
                    Category = csv.TryGetField<string>("category", out var cat) ? cat?.Trim() : null,
                    Subcategory = csv.TryGetField<string>("subcategory", out var sub) ? sub?.Trim() : null,
                    Description = csv.TryGetField<string>("description", out var desc) ? desc?.Trim() : null,
                    Tags = csv.TryGetField<string>("tags", out var tags) ? tags?.Trim() : null,
                    Families = csv.TryGetField<string>("families", out var fam) ? fam?.Trim() : null,
                });
            }

            return await ValidateRowsAsync(rawRows, userId);
        }

        public async Task<CsvImportPreviewDto> ValidateRowsAsync(IEnumerable<RawCsvRowDto> rawRows, int userId)
        {
            var currencies = (await _currencyRepository.GetAllAsync())
                .ToDictionary(c => c.Code.ToUpperInvariant(), c => c.Id);

            // GetAllActiveAsync returns only top-level categories; subcategories live in .Children
            var allCategories = (await _categoryRepository.GetAllActiveAsync()).ToList();
            var topCategories = allCategories
                .ToDictionary(c => c.Name.ToLowerInvariant(), c => c);
            var subCategories = allCategories
                .ToDictionary(
                    cat => cat.Id,
                    cat => cat.Children
                        .Where(s => !s.IsDeleted)
                        .ToDictionary(s => s.Name.ToLowerInvariant(), s => s));

            var userFamilies = await _familyRepository.GetFamiliesByUserAsync(userId);
            var memberFamilyIds = userFamilies.Select(f => f.Family.Id).ToHashSet();

            var rows = rawRows
                .Select(raw => ValidateRow(raw, currencies, topCategories, subCategories, memberFamilyIds))
                .ToList();

            return new CsvImportPreviewDto
            {
                TotalRows = rows.Count,
                ValidCount = rows.Count(r => r.IsValid),
                ErrorCount = rows.Count(r => !r.IsValid),
                Rows = rows,
            };
        }

        private static CsvImportRowPreviewDto ValidateRow(
            RawCsvRowDto raw,
            Dictionary<string, int> currencies,
            Dictionary<string, Category> topCategories,
            Dictionary<int, Dictionary<string, Category>> subCategories,
            HashSet<int> memberFamilyIds)
        {
            var errors = new List<string>();
            DateOnly? parsedDate = null;
            decimal? parsedAmount = null;
            int? currencyId = null;
            int? categoryId = null;
            int? subcategoryId = null;
            int[]? familyIds = null;

            // date
            if (string.IsNullOrEmpty(raw.Date))
                errors.Add("DATE_INVALID");
            else if (!DateOnly.TryParseExact(raw.Date, "yyyy-MM-dd", null, DateTimeStyles.None, out var dt))
                errors.Add("DATE_INVALID");
            else if (dt > DateOnly.FromDateTime(DateTime.UtcNow))
                errors.Add("DATE_IN_FUTURE");
            else
                parsedDate = dt;

            // amount
            if (string.IsNullOrEmpty(raw.Amount))
                errors.Add("AMOUNT_INVALID");
            else if (!decimal.TryParse(raw.Amount, NumberStyles.Any, CultureInfo.InvariantCulture, out var amt))
                errors.Add("AMOUNT_INVALID");
            else if (amt <= 0)
                errors.Add("AMOUNT_MUST_BE_POSITIVE");
            else
                parsedAmount = amt;

            // currency
            if (string.IsNullOrEmpty(raw.CurrencyCode))
                errors.Add("CURRENCY_NOT_FOUND");
            else if (!currencies.TryGetValue(raw.CurrencyCode.ToUpperInvariant(), out var cid))
                errors.Add("CURRENCY_NOT_FOUND");
            else
                currencyId = cid;

            // category (optional)
            if (!string.IsNullOrEmpty(raw.Category))
            {
                if (!topCategories.TryGetValue(raw.Category.ToLowerInvariant(), out var catEntity))
                    errors.Add("CATEGORY_NOT_FOUND");
                else
                {
                    categoryId = catEntity.Id;

                    if (!string.IsNullOrEmpty(raw.Subcategory))
                    {
                        if (!subCategories.TryGetValue(catEntity.Id, out var subs) ||
                            !subs.TryGetValue(raw.Subcategory.ToLowerInvariant(), out var subEntity))
                            errors.Add("SUBCATEGORY_INVALID");
                        else
                            subcategoryId = subEntity.Id;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(raw.Subcategory))
            {
                errors.Add("SUBCATEGORY_INVALID");
            }

            // families (optional — empty = default family)
            if (!string.IsNullOrEmpty(raw.Families))
            {
                var parts = raw.Families.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var ids = new List<int>();
                var familyError = false;
                foreach (var part in parts)
                {
                    if (!int.TryParse(part, out var fid) || !memberFamilyIds.Contains(fid))
                    {
                        errors.Add("FAMILY_FORBIDDEN");
                        familyError = true;
                        break;
                    }
                    ids.Add(fid);
                }
                if (!familyError)
                    familyIds = ids.Count > 0 ? [.. ids] : null;
            }

            // tags — parsed but not validated (auto-created on confirm)
            var tagNames = string.IsNullOrEmpty(raw.Tags)
                ? null
                : raw.Tags.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var isValid = errors.Count == 0;
            return new CsvImportRowPreviewDto
            {
                RowNumber = raw.RowNumber,
                IsValid = isValid,
                Errors = [.. errors],
                DateDisplay = raw.Date,
                AmountDisplay = raw.Amount,
                CurrencyDisplay = raw.CurrencyCode,
                CategoryDisplay = raw.Category,
                SubcategoryDisplay = raw.Subcategory,
                DescriptionDisplay = raw.Description,
                TagNames = tagNames,
                FamiliesDisplay = raw.Families,
                Date = isValid ? parsedDate : null,
                Amount = isValid ? parsedAmount : null,
                CurrencyId = isValid ? currencyId : null,
                CategoryId = isValid ? categoryId : null,
                SubcategoryId = isValid ? subcategoryId : null,
                FamilyIds = isValid ? familyIds : null,
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
