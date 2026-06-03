# CSV Upload Security Hardening Plan

Audit date: 2026-06-03  
Implemented: 2026-06-03  
Status: ✅ All 10 fixes implemented and tested  
Scope: `POST /import/preview`, `POST /import/validate-rows`, `POST /import/confirm`  
Frontend: `CsvImportPage.tsx`

---

## Summary of Gaps

| # | Gap | Layer | Priority |
|---|-----|-------|----------|
| 1 | Missing validator on `ValidateRowsRequest` | Backend | High |
| 2 | No client-side file size check before upload | Frontend | High |
| 3 | No MIME type / extension check on backend | Backend | High |
| 4 | No tag name validation (length, chars, count) | Backend | High |
| 5 | No CSV header validation after `ReadHeader()` | Backend | High |
| 6 | No magic bytes / printability check | Backend | Medium |
| 7 | No tag count limit per row | Backend | Medium |
| 8 | No column count limit (DoS via wide CSV) | Backend | Medium |
| 9 | No async timeout on `ParseAndValidateAsync` | Backend | Medium |
| 10 | No `maxLength` on description textarea | Frontend | Low |

---

## Fix 1 — Validator for `ValidateRowsRequest`

**File:** `backend/expenses/Touir.ExpensesManager.Expenses/Validators/ValidateRowsRequestValidator.cs` *(new)*

`/import/validate-rows` currently has no FluentValidation guard. `RawCsvRowDto` fields arrive raw (strings) with no size or format constraints.

**Rules to add:**

```csharp
public class ValidateRowsRequestValidator : AbstractValidator<ValidateRowsRequest>
{
    private const int MaxRows = 500;

    public ValidateRowsRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Rows)
            .NotNull()
            .Must(r => r.Count() <= MaxRows)
            .WithMessage($"MAX_{MaxRows}_ROWS");

        RuleForEach(x => x.Rows).ChildRules(row =>
        {
            row.RuleFor(r => r.Date).NotEmpty().MaximumLength(10);
            row.RuleFor(r => r.Amount).NotEmpty().MaximumLength(30);
            row.RuleFor(r => r.CurrencyCode).NotEmpty().MaximumLength(10);
            row.RuleFor(r => r.Category).MaximumLength(200);
            row.RuleFor(r => r.Subcategory).MaximumLength(200);
            row.RuleFor(r => r.Description).MaximumLength(500);
            row.RuleFor(r => r.Tags).MaximumLength(1000);
            row.RuleFor(r => r.Families).MaximumLength(500);
        });
    }
}
```

No other changes needed — FluentValidation auto-discovery picks it up via `AddValidatorsFromAssemblyContaining<Program>()`.

---

## Fix 2 — Client-side file size check

**File:** `frontend/dashboard/src/features/expenses/pages/CsvImportPage.tsx`

Currently `handleFile(file)` sends the file immediately with no pre-check. Add size guard before the API call:

```typescript
const MAX_FILE_SIZE = 1 * 1024 * 1024; // 1 MB — must match backend

const handleFile = async (file: File) => {
  setError(null);
  // --- ADD THIS BLOCK ---
  if (file.size > MAX_FILE_SIZE) {
    setError(t('import.errors.fileTooLarge'));
    return;
  }
  if (!file.name.toLowerCase().endsWith('.csv')) {
    setError(t('import.errors.invalidFileType'));
    return;
  }
  // --- END BLOCK ---
  // ... rest of existing function
```

Also add i18n keys:
- `import.errors.fileTooLarge` → "File exceeds 1 MB limit"
- `import.errors.invalidFileType` → "Only CSV files are accepted"

---

## Fix 3 — MIME type and extension check on backend

**File:** `backend/expenses/Touir.ExpensesManager.Expenses/Controllers/ExpenseImportController.cs`

Add after the null/empty check (currently line ~42):

```csharp
private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
{
    "text/csv",
    "application/csv",
    "application/vnd.ms-excel",   // some browsers send this for .csv
    "text/plain"                   // some OS/browser combos
};

// In PreviewAsync, after file null check:
var extension = Path.GetExtension(file.FileName);
if (!extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
    return BadRequest(new ErrorResponse { Message = "INVALID_FILE_TYPE" });

if (!AllowedContentTypes.Contains(file.ContentType))
    return BadRequest(new ErrorResponse { Message = "INVALID_FILE_TYPE" });
```

Note: `ContentType` comes from the browser's multipart declaration — it is NOT cryptographic proof. This is defense-in-depth, not a security guarantee. Fix 6 (magic bytes) is the stronger check.

---

## Fix 4 — Tag name validation

**File:** `backend/expenses/Touir.ExpensesManager.Expenses/Services/CsvImportService.cs`

Tags are parsed and collected but never validated in the preview/validate path (only auto-created on confirm).

**Changes to `ValidateRow()` static method:**

```csharp
private const int MaxTagsPerRow = 20;
private const int MaxTagNameLength = 100;

// In ValidateRow(), where tags are parsed (after splitting by ';'):
var tagParts = raw.Tags?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? Array.Empty<string>();

if (tagParts.Length > MaxTagsPerRow)
{
    errors.Add("TOO_MANY_TAGS");
}
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
}
```

Add `TOO_MANY_TAGS` and `TAG_NAME_TOO_LONG` to the frontend error code → message map in `CsvImportPage.tsx`.

---

## Fix 5 — CSV header validation

**File:** `backend/expenses/Touir.ExpensesManager.Expenses/Services/CsvImportService.cs`

After `csv.ReadHeader()` (currently line ~51), validate required columns exist:

```csharp
private static readonly string[] RequiredHeaders =
{
    "Date", "Amount", "CurrencyCode", "Category", "Description"
};

// After csv.ReadHeader():
var missing = RequiredHeaders
    .Where(h => !csv.HeaderRecord!.Contains(h, StringComparer.OrdinalIgnoreCase))
    .ToList();

if (missing.Any())
    throw new InvalidOperationException($"MISSING_HEADERS:{string.Join(',', missing)}");
```

Controller catch block already returns 400 on `InvalidOperationException`, so the error bubbles correctly.  
Frontend should surface `MISSING_HEADERS` as "CSV file is missing required columns: …".

Optional columns (`Subcategory`, `Tags`, `Families`) should NOT be in `RequiredHeaders` — their absence is valid.

---

## Fix 6 — Magic bytes / printability check

**File:** `backend/expenses/Touir.ExpensesManager.Expenses/Services/CsvImportService.cs`

Read first 512 bytes of the stream before CsvHelper takes over. If null bytes exist, reject — binary files (PE, PDF, ZIP) always contain null bytes.

```csharp
// At top of ParseAndValidateAsync, before creating CsvReader:
var probe = new byte[512];
var read = await stream.ReadAsync(probe, 0, probe.Length);
if (probe.Take(read).Any(b => b == 0x00))
    throw new InvalidOperationException("INVALID_FILE_CONTENT");
stream.Position = 0; // reset for CsvHelper (stream must be seekable)
```

Note: `IFormFile.OpenReadStream()` returns a non-seekable stream in ASP.NET Core — copy to `MemoryStream` first in the controller before passing to the service, or use `file.CopyToAsync(ms)` (already needed for the Position reset).

**Controller change:**

```csharp
// In PreviewAsync, replace: var stream = file.OpenReadStream();
using var ms = new MemoryStream();
await file.CopyToAsync(ms);
ms.Position = 0;
var result = await _csvImportService.ParseAndValidateAsync(ms, userId.Value);
```

---

## Fix 7 — Tag count limit (already covered in Fix 4)

`MaxTagsPerRow = 20` constant in Fix 4 handles this.  
No separate change needed.

---

## Fix 8 — Column count limit

**File:** `backend/expenses/Touir.ExpensesManager.Expenses/Services/CsvImportService.cs`

After `ReadHeader()` and Fix 5's header validation:

```csharp
private const int MaxColumns = 20;

if (csv.HeaderRecord!.Length > MaxColumns)
    throw new InvalidOperationException("TOO_MANY_COLUMNS");
```

Prevents memory amplification from CSVs with thousands of columns.

---

## Fix 9 — Async timeout on `ParseAndValidateAsync`

**File:** `backend/expenses/Touir.ExpensesManager.Expenses/Controllers/ExpenseImportController.cs`

Use `CancellationTokenSource` with a timeout around the parse call:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
try
{
    var result = await _csvImportService.ParseAndValidateAsync(ms, userId.Value, cts.Token);
    return Ok(result);
}
catch (OperationCanceledException)
{
    return BadRequest(new ErrorResponse { Message = "IMPORT_TIMEOUT" });
}
```

Pass `CancellationToken` through `ParseAndValidateAsync` and `ValidateRowsAsync` signatures. Pass it to `ReadAsync` calls and any async repository calls.

---

## Fix 10 — Description maxLength on frontend

**File:** `frontend/dashboard/src/features/expenses/pages/CsvImportPage.tsx`

In the inline edit row, find the description `<input>` and add:

```tsx
<input
  type="text"
  maxLength={500}
  // ... existing props
/>
```

Matches the `MaximumLength(500)` in `CsvImportConfirmRequestValidator`.

---

## Implementation Order

| Step | Fix(es) | Effort | Risk |
|------|---------|--------|------|
| 1 | Fix 2 (client size check) | 15 min | None — pure addition |
| 2 | Fix 1 (ValidateRowsRequest validator) | 20 min | None — new file |
| 3 | Fix 5 + 8 (header + column count) | 20 min | Low — throws before parsing |
| 4 | Fix 4 + 7 (tag validation) | 20 min | Low — adds error codes |
| 5 | Fix 3 (MIME + extension) | 15 min | Low — rejects bad input earlier |
| 6 | Fix 6 (magic bytes) | 30 min | Medium — requires stream copy refactor |
| 7 | Fix 9 (timeout) | 30 min | Medium — signature changes |
| 8 | Fix 10 (maxLength) | 5 min | None |

**Total estimated effort: ~2.5 hours**

---

## Tests to Add / Update

- `ExpenseImportController` tests: assert 400 on wrong extension, wrong MIME type, oversized file
- `CsvImportService` tests: assert error on missing required headers, too many columns, tag count exceeded, tag name too long, binary content
- `CsvImportPage` tests: assert error message shown when file > 1 MB, non-CSV extension dropped
- `ValidateRowsRequestValidator` tests: row count > 500, field length overflow
