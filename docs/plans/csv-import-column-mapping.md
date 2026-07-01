# Plan: CSV Import — Column Mapping Step (+ per-user default mapping)

Backlog item: `docs/plans/ux-ui-improvements.md` §4, "🟢 Web: No column mapping step" (line 238).

## Context

CSV import (`CsvImportService.ParseAndValidateAsync`) requires exact (case-insensitive) headers `date`, `amount`, `currency_code`. Real-world exports often use `sum`/`cur`/`cat` instead — import fails outright with `MISSING_HEADERS`, no recovery path. Goal: let the user remap their columns instead of forcing a re-export, without adding friction to already-well-formed CSVs — **and** let a user save their own export tool's column names as a personal default so recurring imports skip the mapping step entirely on subsequent uploads.

**Found while planning:** `MISSING_HEADERS` is currently swallowed by a generic `catch (Exception)` in `ExpenseImportController.PreviewAsync`, collapsing it to a generic `ServerError`. Fixing this is required — the new flow depends on the client detecting this specific failure.

---

## 1. Backend — alias table + mapping-aware parsing

**New helper:** `backend/expenses/Touir.ExpensesManager.Expenses/Services/CsvHeaderAliasResolver.cs` (static, not DI-registered — mirrors existing `RequiredHeaders` pattern).
- Canonical fields: `date, amount, currency_code, category, subcategory, description, tags, families`.
- Alias table, e.g. `amount -> [amt, sum, value, price]`, `currency_code -> [currency, cur, ccy]`, `category -> [cat]`, `subcategory -> [subcat, sub_category]`, `description -> [desc, note, memo]`, `tags -> [tag]`, `families -> [family, fam]`, `date -> [transaction_date, txn_date]`.
- `SuggestMapping(rawHeaders) -> Dictionary<string,string>` (rawHeader→canonical), case-insensitive/trimmed, first-match-wins on ambiguity.
- `IsExactHeaderMatch(rawHeaders) -> bool`.

**`CsvImportService.ParseAndValidateAsync`** (and `ICsvImportService`) gains optional param:
`Dictionary<string, string>? columnMapping = null` (rawHeader→canonicalField).
- `null`/empty: unchanged behavior (exact-name check) **unless** the user has a saved default mapping — see §1.7. Fully backward compatible for users with no saved default.
- Supplied: resolve canonical→raw via mapping; still throw `MISSING_HEADERS:<fields>` if `date`/`amount`/`currency_code` don't resolve (never trust client for required fields); new `INVALID_COLUMN_MAPPING` if a mapped raw header doesn't exist in the file. Cap mapping size at existing `MaxColumns`.
- Row-reading loop: replace hardcoded `TryGetField("date", ...)` etc. with a `Field(canonicalName)` helper that resolves through the mapping when present, else uses the canonical name directly (today's behavior). Unmapped ("Ignore") canonical fields return null, same as an absent optional column today.
- `ValidateRowsAsync`/`ConfirmImportAsync` untouched — mapping only affects raw→DTO parsing.

**New DTO** `Controllers/DTO/CsvHeaderMappingDto.cs`:
```csharp
public class CsvHeaderDetectionDto {
    public string[] RawHeaders { get; set; } = [];
    public Dictionary<string, string> SuggestedMapping { get; set; } = new();
    public bool HeadersMatchExactly { get; set; }
}
```

**New service method:** `DetectHeadersAsync(Stream, int userId, CancellationToken)` — reads header row only (share the "open reader / read header / TOO_MANY_COLUMNS guard" logic with `ParseAndValidateAsync` via a small private helper), never throws `MISSING_HEADERS`, never reads data rows. `SuggestedMapping` is built by merging the alias table with the user's saved default mapping (§1.7) — **saved mapping takes priority over generic aliases** when both apply to the same raw header, since it reflects the user's own confirmed choice.

### 1.7 Per-user default column mapping — storage

Follows the existing `UserConfig` pattern (`UserConfigs` table, `expenses` service — see `UserConfigService`/`IUserConfigRepository` in constraints.md) rather than inventing a new subsystem.

- **Migration:** add nullable column `DefaultCsvColumnMappingJson nvarchar(max)` to `UserConfigs` (stores `Dictionary<string,string>` — rawHeader→canonicalField — as JSON; `null` = no saved default). Use `CURRENT_TIMESTAMP`-style SQLite-compatible migration conventions already established in this codebase.
- **`IUserConfigRepository`:** add `GetDefaultCsvColumnMappingAsync(userId) -> Dictionary<string,string>?` and `UpsertCsvColumnMappingAsync(userId, Dictionary<string,string>? mapping)` (passing `null` clears it — same upsert-or-clear shape as other config fields).
- **`UserConfigService`:** add `UpdateCsvColumnMappingAsync(userId, Dictionary<string,string>? mapping)` — validates every mapping value is one of `CsvHeaderAliasResolver`'s canonical fields (reject unknown values with a new `INVALID_COLUMN_MAPPING` service error, same code reused from §1.5), validates raw header keys are non-empty/trimmed, caps count at `MaxColumns`. No uniqueness enforcement server-side (defensive only — UI already enforces it); duplicate canonical targets simply mean "last one wins" when applied.
- **`UserConfigDto`:** add `DefaultCsvColumnMapping: Dictionary<string,string>?` alongside existing `defaultCurrencyId`/`defaultCategoryId` fields.

### 1.8 Auto-apply saved mapping — transparent recurring imports

In `ParseAndValidateAsync`, when the caller passes no explicit `columnMapping` and the exact-header check fails:
1. Look up the user's saved default mapping via `IUserConfigRepository.GetDefaultCsvColumnMappingAsync(userId)`.
2. If present and every one of its raw-header keys exists in `csv.HeaderRecord`, **and** it resolves all three required canonical fields, silently use it as the effective mapping — no `MISSING_HEADERS` thrown, no mapping step surfaced to the client at all. This is the actual value of "configure default column names": a user importing from the same export tool repeatedly never sees the mapping step again after the first save.
3. Otherwise, fall through to today's behavior (throw `MISSING_HEADERS`, client detects and shows the mapping step, prefilled per §1.6/§1.7's merge).

### 1.9 Controller changes

`backend/expenses/Touir.ExpensesManager.Expenses/Controllers/ExpenseImportController.cs`:
- New `POST /import/detect-headers` — same file validation as `/import/preview` (extract shared `ValidateUploadedFile` helper), calls `DetectHeadersAsync`, same rate limit.
- `POST /import/preview` accepts an additional `string? columnMapping` form field (JSON-encoded `Record<string,string>`), deserialized via `System.Text.Json`; malformed JSON → `400 INVALID_COLUMN_MAPPING`. Consider a `CsvImportPreviewRequest` (`IFormFile? File`, `string? ColumnMapping`) for symmetry with `CsvImportConfirmRequest`/`ValidateRowsRequest`.
- **Fix the swallowed exception:** catch `InvalidOperationException` with message prefix `MISSING_HEADERS`/`INVALID_COLUMN_MAPPING`/`TOO_MANY_COLUMNS`/`INVALID_FILE_CONTENT` *before* the generic `catch (Exception)`, return `BadRequest(new ErrorResponse { Message = ex.Message })`.

`UserConfigController` (`[Route("config")]`, expenses service): extend existing config read/write to include the new field —
- `GET /config` response (`UserConfigDto`) includes `defaultCsvColumnMapping`.
- New `PUT /config/csv-column-mapping` body `{ mapping: Record<string,string> }` → `UpdateCsvColumnMappingAsync`. Separate endpoint (not folded into the existing currency/category PUT) since the shape differs materially (dictionary vs two ints) and this keeps the existing update DTO/validator untouched.
- New `DELETE /config/csv-column-mapping` → clears the saved default (`UpsertCsvColumnMappingAsync(userId, null)`).

---

## 2. Frontend UX/UI design — use `/ui-ux-pro-max`

Before building the mapping-step UI and the new Settings card, run `/ui-ux-pro-max` to design both against the existing **Hearth** design system (see §16 of `ux-ui-improvements.md`):
- Mapping step: inline step (not a modal, consistent with the page's dropzone→preview flow), reusing `StringCombobox`, `bg-surface-card`/`border-surface-border`/`rounded-2xl`/`shadow-card` conventions, the "Edited"-badge visual language for a "Suggested" badge.
- Settings card: match the existing `DefaultCurrencyCard`/`DefaultCategoryCard` visual pattern (card header, editable list, "Saved ✓" confirmation state) so "Default CSV Columns" doesn't look bolted on.

---

## 3. Frontend — types & API

`features/expenses/types/expenses.type.ts`: add `CsvHeaderDetectionDto` type + `CSV_CANONICAL_FIELDS` const array.

`features/expenses/services/expensesApi.service.ts`:
- new `detectCsvHeaders(file)` → `POST /import/detect-headers`.
- `previewCsvImport(file, columnMapping?)` — optional 2nd param, appends `columnMapping` as JSON string form field when present. Backward compatible.

`services/api.service.ts` / `types/api.type.ts`: add optional `rawCode` to `ApiResponse`, populated in `buildErrorResponse` alongside the translated `error` string — needed because `MISSING_HEADERS:date,amount` can't be distinguished from any other 400 via the translated message alone. Keep `BACKEND_ERROR_CODES` lookup working for the fallback path by stripping everything after the first `:`.

**User config types/API** (wherever `UserConfigDto`/settings API calls currently live, e.g. `features/settings/...` — verify exact path before implementing): add `defaultCsvColumnMapping: Record<string,string> | null` to the config type, and `updateDefaultCsvColumnMapping(mapping)` / `clearDefaultCsvColumnMapping()` API functions calling the new `PUT`/`DELETE /config/csv-column-mapping` endpoints.

---

## 4. Frontend — mapping step in `CsvImportPage.tsx`

New state: `pendingFile`, `headerDetection`, `columnMapping` (`rawHeader → canonical | 'ignore'`), `mappingError`, `detecting`, `rememberMapping` (boolean, default `true`).

Flow change in `handleFile`:
1. Client-side size/extension checks unchanged (fail fast).
2. Call `previewCsvImport(file)` (no mapping) first — with the backend auto-apply from §1.8, this transparently succeeds on repeat imports for users with a matching saved default, with **no visible change to this step**.
3. Success → proceed exactly as today.
4. Failure with `res.rawCode?.startsWith('MISSING_HEADERS')` → `setPendingFile(file)`, call `detectCsvHeaders(file)`, seed `columnMapping` from `suggestedMapping` (unmatched raw headers default to `'ignore'`), render the mapping step.
5. Any other error → existing `setError` path, unchanged.

Render branch: `!preview && !headerDetection` → dropzone; `!preview && headerDetection` → mapping step; else → preview table.

**Mapping step UI:**
- Title + one-line explanation of why the step appeared.
- One row per `headerDetection.rawHeaders` entry: raw header name (monospace) + "Suggested" badge (derived by comparing current value to `suggestedMapping`, no extra state) | `StringCombobox` with `CSV_CANONICAL_FIELDS` (translated) + leading "Ignore" option.
- Enforce uniqueness by filtering out canonical fields already chosen in other rows from each combobox's options (no silent reassignment).
- Inline `role="alert"` banner listing unmapped required fields (`date`/`amount`/`currency_code`); disables "Continue" while any are missing.
- **New: "Remember this mapping for future imports" checkbox** (checked by default), bound to `rememberMapping`. Short helper text: "We'll skip this step next time you import a file with these same column names."
- "Cancel" → reset mapping state, return to dropzone. "Continue" →
  1. `previewCsvImport(pendingFile, columnMapping)` (filter out `'ignore'` entries first).
  2. On success: if `rememberMapping` is checked, fire-and-forget (non-blocking, same non-fatal pattern used for outbox/audit writes elsewhere) call `updateDefaultCsvColumnMapping(columnMapping)` so it's saved for next time; clear mapping-step state and show the preview table regardless of whether the save call succeeds (import flow must not be blocked by a settings-save failure).
  3. On preview failure: `setError`, stay on the mapping step.

Accessibility: `aria-label` per combobox (`Map column "{rawHeader}"`), real `<table>` structure (not a div-grid), alert region names exactly which fields are missing, checkbox has an associated `<label>`.

---

## 5. Frontend — Settings screen: "Default CSV Columns" card

New card on `SettingsPage.tsx`, alongside `DefaultCurrencyCard`/`DefaultCategoryCard` (same file/folder — check exact component location before implementing), e.g. `DefaultCsvColumnMappingCard.tsx`:
- Reads `defaultCsvColumnMapping` from user config (loaded the same way currency/category defaults are).
- If unset: empty-state text ("No default column mapping saved yet — one will be created automatically next time you confirm a mapping during CSV import.") plus no destructive action available.
- If set: a read-only list of `rawHeader → canonicalField` pairs (reuse the same two-column row layout as the CsvImportPage mapping step for visual consistency) with per-row edit (via `StringCombobox`, same options as the import mapping step) and delete, plus an "Add column" row to append a new raw-header/canonical pair manually.
- "Save" button → `updateDefaultCsvColumnMapping(mapping)`; on success, same "Saved ✓" inline confirmation pattern (sage background + checkmark + `aria-live="polite"` announcement, reverting after 2s) as `DefaultCurrencyCard`/`DefaultCategoryCard`.
- "Clear default mapping" button (secondary/destructive style) → confirmation-free (low-risk, reversible by re-saving) call to `clearDefaultCsvColumnMapping()`; reverts card to the empty state.

---

## 6. New/changed DTOs and types — summary

| Layer | Item | Change |
|---|---|---|
| Backend DTO | `CsvHeaderDetectionDto` (new file `CsvHeaderMappingDto.cs`) | `RawHeaders`, `SuggestedMapping`, `HeadersMatchExactly` |
| Backend Request | `CsvImportPreviewRequest` (new, optional) | `IFormFile? File`, `string? ColumnMapping` |
| Backend Service | `ICsvImportService`/`CsvImportService` | new `DetectHeadersAsync`; `ParseAndValidateAsync` gains `columnMapping` param + saved-default auto-apply (§1.8) |
| Backend Helper | `CsvHeaderAliasResolver` (new file) | alias table + `SuggestMapping` + `IsExactHeaderMatch` |
| Backend Controller | `ExpenseImportController` | new `POST /import/detect-headers`; `PreviewAsync` accepts mapping; fixes swallowed `MISSING_HEADERS`; new `INVALID_COLUMN_MAPPING` |
| Backend DB | `UserConfigs` migration | new nullable `DefaultCsvColumnMappingJson` column |
| Backend Repo/Service | `IUserConfigRepository`/`UserConfigService` | new get/upsert/clear for the saved mapping, validation against canonical fields |
| Backend Controller | `UserConfigController` | `GET /config` includes new field; new `PUT`/`DELETE /config/csv-column-mapping` |
| Frontend type | `expenses.type.ts` | `CsvHeaderDetectionDto`, `CSV_CANONICAL_FIELDS` |
| Frontend type | user-config type (settings feature) | `defaultCsvColumnMapping: Record<string,string> \| null` |
| Frontend API | `expensesApi.service.ts` | new `detectCsvHeaders`; `previewCsvImport` gains optional `columnMapping` param |
| Frontend API | settings/user-config API service | new `updateDefaultCsvColumnMapping`/`clearDefaultCsvColumnMapping` |
| Frontend API core | `api.service.ts`/`api.type.ts` | optional `rawCode` on `ApiResponse` for raw backend error code passthrough |
| Frontend page | `CsvImportPage.tsx` | mapping-step state + inline step UI + "remember this mapping" checkbox |
| Frontend page | `SettingsPage.tsx` | new `DefaultCsvColumnMappingCard` |

---

## 7. i18n — all 4 locale files

`frontend/dashboard/src/i18n/locales/{en,fr,es,de}/translation.json`, under `expenses.import`:
```json
"mapping": {
  "title": "...", "description": "...", "rawColumn": "...", "mapsTo": "...",
  "ignore": "...", "suggested": "...", "missingRequired": "...", "continue": "...",
  "remember": "...", "rememberHint": "..."
}
```
Plus `expenses.import.errors.INVALID_COLUMN_MAPPING`. Verify `MISSING_HEADERS` exists in fr/es/de (not just en) before assuming reuse — add if missing.

Under `settings` (new section, sibling to existing `defaultCurrency`/`defaultCategory` keys):
```json
"csvColumnMapping": {
  "title": "...", "empty": "...", "rawColumn": "...", "mapsTo": "...",
  "addColumn": "...", "save": "...", "saved": "...", "clear": "...", "cleared": "..."
}
```

English copy suggestions only (translate for fr/es/de following the tone of neighboring strings in each file):
- mapping.title: "Match your CSV columns"
- mapping.description: "We couldn't find some required columns automatically. Match each column from your file to the right field below."
- mapping.missingRequired: "Please map the following required fields: {{fields}}"
- mapping.remember: "Remember this mapping for future imports"
- mapping.rememberHint: "We'll skip this step next time you import a file with these same column names."
- settings.csvColumnMapping.title: "Default CSV Columns"
- settings.csvColumnMapping.empty: "No default column mapping saved yet — one will be created automatically next time you confirm a mapping during CSV import."

---

## 8. Unit tests — implement/update alongside code (not deferred)

### 8.1 Backend — `CsvImportServiceTests.cs`

- No-mapping missing-headers behavior unchanged when the user has no saved default (regression).
- Mapping with aliased headers parses correctly.
- Mapping provided but a required canonical field absent → still `MISSING_HEADERS`.
- Mapping references a raw header not in the file → `INVALID_COLUMN_MAPPING`.
- Raw header present but unmapped ("Ignore") → excluded from parsed row.
- **New:** no explicit mapping passed, exact headers don't match, but user has a saved default mapping covering all required fields and every raw header in the file → parses successfully, no exception thrown (auto-apply, §1.8).
- **New:** saved default mapping exists but references a raw header *not* present in this particular file → falls through to `MISSING_HEADERS` (saved mapping only auto-applies when it fully covers the file's actual headers).
- **New:** saved default mapping exists and covers required fields but the caller *also* passes an explicit `columnMapping` → explicit mapping takes precedence (client override always wins over stored default).

New `CsvHeaderAliasResolverTests.cs`:
- `SuggestMapping` exact-header identity mapping, known aliases, unknown columns excluded, ambiguous duplicate aliases (first-match-wins).
- `IsExactHeaderMatch` true/false.

`DetectHeadersAsync` tests (same file or adjacent):
- Well-formed CSV → `HeadersMatchExactly=true`.
- Aliased CSV, no saved user mapping → suggestions from alias table only, `HeadersMatchExactly=false`.
- **New:** aliased CSV, user has a saved default mapping for some of the same raw headers → `SuggestedMapping` prefers the saved mapping's value over the generic alias table for those headers.
- Doesn't choke on malformed data rows (only reads header line).

### 8.2 Backend — `UserConfigServiceTests.cs` (extend existing file)

- `UpdateCsvColumnMappingAsync` with valid mapping (all values are known canonical fields) → persists successfully.
- `UpdateCsvColumnMappingAsync` with an unknown canonical value → throws/returns `INVALID_COLUMN_MAPPING`.
- `UpdateCsvColumnMappingAsync(userId, null)` (clear) → persisted mapping becomes null, subsequent `GetDefaultCsvColumnMappingAsync` returns null.
- Mapping exceeding `MaxColumns` count → rejected.

### 8.3 Backend — `UserConfigRepositoryTests.cs` (extend existing file, if present — verify path first)

- `UpsertCsvColumnMappingAsync` round-trips a `Dictionary<string,string>` through the JSON column correctly (serialize → store → deserialize).
- `GetDefaultCsvColumnMappingAsync` returns `null` for a user who never saved one (default state after migration).

### 8.4 Backend — controller tests

Check for/update an `ExpenseImportControllerTests.cs`/`UserConfigControllerTests.cs` if either exists (verify path via Glob before implementing):
- `POST /import/detect-headers` happy path, no file, wrong type.
- `POST /import/preview` now returns `400 MISSING_HEADERS:...` instead of generic `ServerError` (regression test for the swallowed-exception fix).
- **New:** `PUT /config/csv-column-mapping` happy path, invalid canonical value → 400.
- **New:** `DELETE /config/csv-column-mapping` clears the value; subsequent `GET /config` reflects `null`.

### 8.5 Frontend — `CsvImportPage.test.tsx`

Add a `mockDetectCsvHeaders` to the mocked service module, following the same pattern as `mockValidateCsvRows`.

- "shows column mapping step when preview fails with `MISSING_HEADERS` and detect-headers succeeds" — prefilled comboboxes from mocked `suggestedMapping`.
- "does not show mapping step when headers match (normal upload flow unaffected)" — existing happy-path tests continue to pass unmodified; assert `mockDetectCsvHeaders` never called.
- "disables Continue button when a required canonical field is unmapped".
- "clicking Continue resubmits file with confirmed `columnMapping` and shows preview table on success".
- "Cancel on mapping step returns to the dropzone and clears mapping state".
- "choosing a canonical field already used by another row removes it from that row's options".
- **New:** "'Remember this mapping' checked by default; on successful Continue, calls `updateDefaultCsvColumnMapping` with the confirmed (non-'ignore') mapping".
- **New:** "unchecking 'Remember this mapping' before Continue does not call `updateDefaultCsvColumnMapping`".
- **New:** "if `updateDefaultCsvColumnMapping` rejects, the preview table still renders (save failure doesn't block import)".

### 8.6 Frontend — `SettingsPage.test.tsx` (extend existing file — already has patterns for `DefaultCurrencyCard`/`DefaultCategoryCard`)

- Renders empty state when `defaultCsvColumnMapping` is `null`.
- Renders existing mapping rows when set; edit + Save shows "Saved ✓" confirmation (same `aria-live` assertion pattern as the currency/category cards) and reverts after 2s.
- "Add column" appends an editable row.
- "Clear default mapping" calls `clearDefaultCsvColumnMapping` and reverts the card to empty state.

---

## Implementation order

1. Backend: `CsvHeaderAliasResolver` + its tests (pure, no deps).
2. Backend: `UserConfigs` migration + repository/service changes for the saved mapping + their tests.
3. Backend: `ParseAndValidateAsync` mapping support + saved-default auto-apply (§1.8) + `DetectHeadersAsync` + DTOs + service tests.
4. Backend: controller wiring (`/import/detect-headers`, preview mapping param, swallowed-exception fix, `UserConfigController` new endpoints) + controller tests.
5. Run `/ui-ux-pro-max` to finalize both the mapping-step and Settings-card visual design against Hearth tokens.
6. Frontend: types + API functions (`detectCsvHeaders`, `previewCsvImport`, user-config mapping API) + `ApiResponse.rawCode` plumbing.
7. Frontend: `CsvImportPage.tsx` mapping-step UI + "remember this mapping" checkbox + state wiring.
8. Frontend: `SettingsPage.tsx` new `DefaultCsvColumnMappingCard`.
9. Frontend: i18n keys, all 4 locales.
10. Frontend: `CsvImportPage.test.tsx` + `SettingsPage.test.tsx` new cases.
11. Manual verification: upload a CSV with `sum,cur,cat` headers, confirm mapping with "Remember" checked; re-upload the same-shaped file and confirm the mapping step is skipped entirely; check Settings shows the saved mapping and that "Clear" reverts to asking again on next import.
12. Run `/done` to update docs (CLAUDE.md, FILE-TREE.md, CHANGELOG.md, expenses README, `ux-ui-improvements.md` — mark item done).

---

### Critical files

- `backend/expenses/Touir.ExpensesManager.Expenses/Services/CsvImportService.cs`
- `backend/expenses/Touir.ExpensesManager.Expenses/Controllers/ExpenseImportController.cs`
- `backend/expenses/Touir.ExpensesManager.Expenses/Services/Contracts/ICsvImportService.cs`
- `backend/expenses/Touir.ExpensesManager.Expenses/Services/UserConfigService.cs` (and its interface/repository)
- `backend/expenses/Touir.ExpensesManager.Expenses/Controllers/UserConfigController.cs`
- `backend/expenses/Touir.ExpensesManager.Expenses.Tests/Services/CsvImportServiceTests.cs`
- `backend/expenses/Touir.ExpensesManager.Expenses.Tests/Services/UserConfigServiceTests.cs`
- `frontend/dashboard/src/features/expenses/pages/CsvImportPage.tsx`
- `frontend/dashboard/src/features/expenses/pages/__tests__/CsvImportPage.test.tsx`
- `frontend/dashboard/src/features/expenses/services/expensesApi.service.ts`
- `frontend/dashboard/src/services/api.service.ts`
- `frontend/dashboard/src/features/settings/pages/SettingsPage.tsx` (verify exact path)
- `frontend/dashboard/src/features/settings/pages/__tests__/SettingsPage.test.tsx` (verify exact path)
