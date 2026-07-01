using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text.Json;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Messaging.Messages;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Services
{
    public class CsvImportService : ICsvImportService
    {
        private const int SourceBulkWeb = 3;
        private const int MaxRows = 500;
        private const int MaxColumns = CsvHeaderAliasResolver.MaxColumns;
        private const int MaxTagsPerRow = 20;
        private const int MaxTagNameLength = 100;

        private static readonly string[] RequiredHeaders = CsvHeaderAliasResolver.RequiredCanonicalFields;

        private readonly ICurrencyRepository _currencyRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly ITagService _tagService;
        private readonly IExpenseService _expenseService;
        private readonly IExpensesOutboxRepository _outboxRepo;
        private readonly IUserConfigRepository _userConfigRepository;

        public CsvImportService(
            ICurrencyRepository currencyRepository,
            ICategoryRepository categoryRepository,
            IFamilyRepository familyRepository,
            ITagService tagService,
            IExpenseService expenseService,
            IExpensesOutboxRepository outboxRepo,
            IUserConfigRepository userConfigRepository)
        {
            _currencyRepository = currencyRepository;
            _categoryRepository = categoryRepository;
            _familyRepository = familyRepository;
            _tagService = tagService;
            _expenseService = expenseService;
            _outboxRepo = outboxRepo;
            _userConfigRepository = userConfigRepository;
        }

        public async Task<CsvImportPreviewDto> ParseAndValidateAsync(
            Stream csvStream,
            int userId,
            Dictionary<string, string>? columnMapping = null,
            CancellationToken cancellationToken = default)
        {
            using var csv = await OpenReaderAsync(csvStream, cancellationToken);
            var rawHeaders = csv.HeaderRecord!;

            var effectiveMapping = columnMapping;
            if (effectiveMapping is null or { Count: 0 } && !CsvHeaderAliasResolver.IsExactHeaderMatch(rawHeaders))
            {
                var savedMapping = await _userConfigRepository.GetDefaultCsvColumnMappingAsync(userId);
                if (savedMapping is not null &&
                    savedMapping.Keys.All(h => rawHeaders.Contains(h, StringComparer.OrdinalIgnoreCase)) &&
                    CsvHeaderAliasResolver.RequiredCanonicalFields.All(f => savedMapping.ContainsValue(f)))
                {
                    effectiveMapping = savedMapping;
                }
            }

            Dictionary<string, string>? rawHeaderByCanonical = null;
            if (effectiveMapping is { Count: > 0 })
            {
                rawHeaderByCanonical = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var (raw, canonical) in effectiveMapping)
                {
                    if (!rawHeaders.Contains(raw, StringComparer.OrdinalIgnoreCase))
                        throw new InvalidOperationException("INVALID_COLUMN_MAPPING");
                    rawHeaderByCanonical[canonical] = raw;
                }

                var missingRequired = CsvHeaderAliasResolver.RequiredCanonicalFields
                    .Where(f => !rawHeaderByCanonical.ContainsKey(f))
                    .ToList();
                if (missingRequired.Count > 0)
                    throw new InvalidOperationException($"MISSING_HEADERS:{string.Join(',', missingRequired)}");
            }
            else
            {
                // No mapping — validate required headers exist verbatim (original behavior)
                var missing = RequiredHeaders
                    .Where(h => !rawHeaders.Contains(h, StringComparer.OrdinalIgnoreCase))
                    .ToList();
                if (missing.Count > 0)
                    throw new InvalidOperationException($"MISSING_HEADERS:{string.Join(',', missing)}");
            }

            string? Field(string canonicalName)
            {
                string rawName;
                if (rawHeaderByCanonical is not null)
                {
                    if (!rawHeaderByCanonical.TryGetValue(canonicalName, out rawName!))
                        return null; // user chose "Ignore" for this canonical field
                }
                else
                {
                    rawName = canonicalName;
                }

                return csv.TryGetField<string>(rawName, out var v) ? v?.Trim() : null;
            }

            var rawRows = new List<RawCsvRowDto>();
            int rowNumber = 0;
            while (await csv.ReadAsync())
            {
                rowNumber++;
                if (rowNumber > MaxRows)
                    break;

                rawRows.Add(new RawCsvRowDto
                {
                    RowNumber = rowNumber,
                    Date = Field("date"),
                    Amount = Field("amount"),
                    CurrencyCode = Field("currency_code"),
                    Category = Field("category"),
                    Subcategory = Field("subcategory"),
                    Description = Field("description"),
                    Tags = Field("tags"),
                    Families = Field("families"),
                });
            }

            return await ValidateRowsAsync(rawRows, userId, cancellationToken);
        }

        public async Task<CsvHeaderDetectionDto> DetectHeadersAsync(Stream csvStream, int userId, CancellationToken cancellationToken = default)
        {
            using var csv = await OpenReaderAsync(csvStream, cancellationToken);
            var rawHeaders = csv.HeaderRecord!;

            var suggestions = CsvHeaderAliasResolver.SuggestMapping(rawHeaders);

            var savedMapping = await _userConfigRepository.GetDefaultCsvColumnMappingAsync(userId);
            if (savedMapping is not null)
            {
                foreach (var (raw, canonical) in savedMapping)
                {
                    var matchingHeader = rawHeaders.FirstOrDefault(h => string.Equals(h, raw, StringComparison.OrdinalIgnoreCase));
                    if (matchingHeader is not null)
                        suggestions[matchingHeader] = canonical; // saved default takes priority over generic aliases
                }
            }

            return new CsvHeaderDetectionDto
            {
                RawHeaders = rawHeaders,
                SuggestedMapping = suggestions,
                HeadersMatchExactly = CsvHeaderAliasResolver.IsExactHeaderMatch(rawHeaders),
            };
        }

        private static async Task<CsvReader> OpenReaderAsync(Stream csvStream, CancellationToken cancellationToken)
        {
            // Magic bytes check — binary files always contain null bytes
            var probe = new byte[512];
            var read = await csvStream.ReadAsync(probe.AsMemory(0, probe.Length), cancellationToken);
            if (probe.Take(read).Any(b => b == 0x00))
                throw new InvalidOperationException("INVALID_FILE_CONTENT");
            csvStream.Position = 0;

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null,
            };

            var reader = new StreamReader(csvStream);
            var csv = new CsvReader(reader, config);

            await csv.ReadAsync();
            csv.ReadHeader();

            if (csv.HeaderRecord!.Length > MaxColumns)
                throw new InvalidOperationException("TOO_MANY_COLUMNS");

            return csv;
        }

        public async Task<CsvImportPreviewDto> ValidateRowsAsync(IEnumerable<RawCsvRowDto> rawRows, int userId, CancellationToken cancellationToken = default)
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
            var memberFamiliesByName = userFamilies
                .Where(f => !f.Family.IsDeleted)
                .ToDictionary(f => f.Family.Name.ToLowerInvariant(), f => f.Family.Id);

            var rows = rawRows
                .Select(raw => ValidateRow(raw, currencies, topCategories, subCategories, memberFamiliesByName))
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
            Dictionary<string, int> memberFamiliesByName)
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

            // families (optional — empty = default family; column accepts names, resolved case-insensitively)
            if (!string.IsNullOrEmpty(raw.Families))
            {
                var parts = raw.Families.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var ids = new List<int>();
                var familyError = false;
                foreach (var part in parts)
                {
                    if (!memberFamiliesByName.TryGetValue(part.ToLowerInvariant(), out var fid))
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

            // tags — validate count and name length, auto-created on confirm
            string[]? tagNames = null;
            if (!string.IsNullOrEmpty(raw.Tags))
            {
                var tagParts = raw.Tags.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (tagParts.Length > MaxTagsPerRow)
                    errors.Add("TOO_MANY_TAGS");
                else
                {
                    foreach (var tag in tagParts)
                    {
                        if (tag.Length > MaxTagNameLength)
                        {
                            errors.Add("TAG_NAME_TOO_LONG");
                            break;
                        }
                    }
                    tagNames = tagParts;
                }
            }

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
            var rowList = rows.ToList();

            foreach (var row in rowList)
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

            try
            {
                var msgId = Guid.NewGuid().ToString();
                await _outboxRepo.EnqueueAsync(new OutboxEvent
                {
                    MessageId = msgId,
                    EventType = FamilyEventType.ImportCompleted,
                    Payload = JsonSerializer.Serialize(new ImportCompletedEventMessage
                    {
                        MessageId = msgId,
                        EventType = FamilyEventType.ImportCompleted,
                        UserId = userId,
                        TotalRows = rowList.Count,
                        ImportedCount = imported,
                        SkippedCount = skipped
                    }),
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }

            return new CsvImportResultDto { Imported = imported, Skipped = skipped };
        }
    }
}
