# Expenses Service

REST API for managing expenses, categories, and currencies.

## Tech Stack

- **.NET 8** — `net8.0` target framework
- **Entity Framework Core 8** + **Npgsql** — PostgreSQL via EF Core
- **FluentValidation 11** — request DTO validation (`AddFluentValidationAutoValidation`); validators in `Validators/`
- **RabbitMQ.Client 6.8.1** — event consumption from `users.events` topic exchange
- **xUnit** + **Moq** — unit and integration tests

## Usage

```bash
cd Touir.ExpensesManager.Expenses
dotnet restore
dotnet run
```

Service runs on port **9200** by default. Configuration via `appsettings.json` and environment variables.

## Key Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/categories` | Active category tree (top-level + subcategories, archived excluded) → `CategoryDto[]` |
| `GET` | `/currencies` | All currencies → `CurrencyDto[]` |
| `POST` | `/` | Create expense → `ExpenseDto` (201) |
| `PUT` | `/{id}` | Update expense → `ExpenseDto` (200) or 404 |
| `DELETE` | `/{id}` | Soft-delete expense → 204 or 404 |
| `GET` | `/{id}` | Get expense by id → `ExpenseDto` (200) or 404 |
| `GET` | `/` | Paged + filtered expense list → `ExpensePagedResponse` |
| `GET` | `/families` | List families for authenticated user → `FamilyDto[]` |
| `POST` | `/families` | Create a new family → `FamilyDto` (201); **409** if user already has a family with that name |
| `GET` | `/families/{id}` | Family detail with members → `FamilyDetailDto` (200) or 403/404 |
| `PUT` | `/families/{id}/name` | Rename family (head only) → `FamilyDto` (200) or 403/404/**409** (name already exists for user) |
| `POST` | `/families/{id}/archive` | Archive non-default family (head only) → 204 or 403/404 |
| `POST` | `/families/{id}/unarchive` | Unarchive family (head only) → 204 or 403/404 |
| `POST` | `/families/{id}/invite` | Invite user by email (head only) → `{ token }` (200) or 403/404/409 |
| `POST` | `/families/accept-invite` | Accept invitation by token → 204 or 400/403/409 |
| `DELETE` | `/families/{id}/members/{userId}` | Remove member (head only, not self) → 204 or 403/404 |
| `PUT` | `/families/{id}/members/{userId}/role` | Change member role (head only) → 204 or 403/404 |
| `GET` | `/tags` | Tags visible to user → `TagListDto { own, family }` |
| `POST` | `/tags` | Create/adopt tag by name (idempotent, case-sensitive) → `TagDto` (200) |
| `DELETE` | `/tags/{id}` | Remove user's adoption of tag → 204 or 404 (tag entity and expense history preserved) |
| `POST` | `/import/preview` | Parse + validate CSV file (multipart, max 1 MB, `.csv` ext, `text/csv`/`application/csv`/`text/plain`/`application/vnd.ms-excel` MIME); rejects binary files (null-byte probe); validates required headers (`date`, `amount`, `currency_code`) and ≤20 columns; 30s timeout → `CsvImportPreviewDto` with per-row status and error codes; `families` column accepts **semicolon-separated family names** (case-insensitive, not IDs) |
| `POST` | `/import/validate-rows` | Re-validate edited rows without re-uploading — body: `{ rows: RawCsvRowDto[] }` (validated by FluentValidation: ≤500 rows, per-field length limits); 30s timeout → `CsvImportPreviewDto`; used by frontend inline-edit + re-validate flow |
| `POST` | `/import/confirm` | Bulk-insert valid rows; tags auto-created/adopted; logged as `bulk_web` (OperationSource ID 3) → `CsvImportResultDto { imported, skipped }` |
| `GET` | `/import/template` | Download CSV template (`expenses-import-template.csv`) with header row and 2 example rows |
| `GET` | `/admin/rates/history` | **[AppAdmin]** Paged rate history → `PagedRatesResponse { rates, total, page, pageSize }` (query: `sourceCurrencyId?`, `destinationCurrencyId?`, `page`, `pageSize`) |
| `POST` | `/admin/rates` | **[AppAdmin]** Add manual rate → `RateDto` (201); conflict created if auto rate exists for that date |
| `POST` | `/admin/rates/bulk` | **[AppAdmin]** Bulk add manual rates → 204 |
| `PUT` | `/admin/rates/default` | **[AppAdmin]** Set/update global fallback rate for a currency pair → 204 |
| `GET` | `/admin/rates/conflicts` | **[AppAdmin]** List pending rate conflicts → `RateConflictDto[]` |
| `PUT` | `/admin/rates/conflicts/{id}/resolve` | **[AppAdmin]** Resolve conflict (AcceptAuto / KeepManual / Custom) → 204 or 400/404 |
| `POST` | `/admin/rates/refresh` | **[AppAdmin]** Backfill rates from provider → 204; body: `{ from, to?, sourceCurrencyId?, destinationCurrencyId? }` |
| `POST` | `/admin/categories` | **[AppAdmin]** Add top-level category → `AdminCategoryDto` (201); **400** (`CATEGORY_NAME_DUPLICATE`) if name exists at top level |
| `PUT` | `/admin/categories/{id}` | **[AppAdmin]** Edit category name/description → 200 or 404; **400** (`CATEGORY_NAME_DUPLICATE`) on name collision |
| `POST` | `/admin/categories/{id}/archive` | **[AppAdmin]** Archive category → 204 or 400/404 |
| `POST` | `/admin/categories/{id}/unarchive` | **[AppAdmin]** Unarchive category → 204 or 404 |
| `POST` | `/admin/categories/{id}/subcategories` | **[AppAdmin]** Add subcategory → `AdminCategoryDto` (201) or 400/404; **400** (`CATEGORY_NAME_DUPLICATE`) if name exists within the same parent |
| `PUT` | `/admin/categories/{id}/subcategories/{subId}` | **[AppAdmin]** Edit subcategory → 200 or 404; **400** (`CATEGORY_NAME_DUPLICATE`) on name collision within parent |
| `POST` | `/admin/categories/{id}/subcategories/{subId}/archive` | **[AppAdmin]** Archive subcategory → 204 or 404 |
| `POST` | `/admin/categories/{id}/subcategories/{subId}/unarchive` | **[AppAdmin]** Unarchive subcategory → 204 or 404 |
| `POST` | `/admin/currencies` | **[AppAdmin]** Add currency → `CurrencyDto` (201) |
| `PUT` | `/admin/currencies/{id}` | **[AppAdmin]** Update currency name/symbol/decimals → `CurrencyDto` (200) or 404 |
| `DELETE` | `/admin/currencies/{id}` | **[AppAdmin]** Delete currency → 204; 409 when currency is in use by expenses or rates |
| `GET` | `/admin/currencies/{id}/defaults` | **[AppAdmin]** Default rate table for a currency → `CurrencyDefaultRateDto[]` (destination + default rate + last auto rate + date) |
| `POST` | `/admin/currencies/defaults` | **[AppAdmin]** Set default fallback rate for a pair → 204 |
| `GET` | `/dashboard/summary` | Total amount + count + previous-period delta (% change) + top category → `DashboardSummaryDto` |
| `GET` | `/dashboard/monthly` | Per-month totals broken down by category → `MonthlyBreakdownDto[]`; default range = Jan 1 of current year → today |
| `GET` | `/dashboard/categories` | Category/subcategory breakdown with percentages → `CategoryBreakdownDto[]` |
| `GET` | `/dashboard/same-month-across-years` | Per-year totals for a given month (`?month=1–12`) → `SameMonthYearlyDto[]` |
| `GET` | `/dashboard/by-currency` | Per-currency totals + converted amount + count → `CurrencyBreakdownDto[]` |
| `GET` | `/dashboard/recent` | Last 10 expenses → `ExpensePagedResponse` |
| `GET` | `/health` | Liveness/readiness probe |

All endpoints (except `/health`) require authentication, enforced by nginx's `auth_request` subrequest to the users service before forwarding. `/admin/*` endpoints additionally require the `APP_ADMIN` role (`[AppAdmin]` filter checks the `isAdmin` JWT claim; returns 403 if absent or false).

`POST /expenses` and `PUT /expenses/{id}` return `403 Forbidden` if a provided `familyId` does not match a family the user belongs to, or if a `tagId` is not visible to the user (not in own tags or co-member tags).

**Dashboard query params:** all dashboard endpoints accept `?familyId` (scope to family-attributed expenses; verifies membership → 403 if not member), `?displayCurrencyId` (currency conversion), `?dateFrom`/`?dateTo` (defaults: first day of current month → today for summary/categories/by-currency; Jan 1 of current year → today for monthly). `same-month-across-years` uses `?month=1–12` instead of date range.

**Tag visibility:** a tag is visible if the user has adopted it (`UserTag` row exists) OR any co-member of a shared non-deleted family has adopted it. Attaching a tag to an expense auto-adopts it for the requesting user.

**Display currency conversion:** `GET /expenses/{id}` and `GET /expenses` accept an optional `?displayCurrencyId={id}` query param. When set and the expense currency differs, `ExpenseDto.convertedAmount` and `ExpenseDto.displayCurrency` are populated. The rate resolution chain: same currency → 1.0; exact date match → most-recent-before fallback → global pair default → on-demand Frankfurter API fetch (result stored as auto rate) → null (no conversion).

### Response DTOs

**`CategoryDto`** — `{ id, name, description?, subcategories: SubcategoryDto[] }`  
**`SubcategoryDto`** — `{ id, name, description? }`  
**`CurrencyDto`** — `{ id, code, name, symbol, decimals }`  
**`ExpenseDto`** — `{ id, amount, currency: CurrencyDto?, date, category: SubcategoryDto?, subcategory: SubcategoryDto?, description?, createdAt, modifiedAt?, modifiedFrom?, tags: TagDto[], convertedAmount?: decimal, displayCurrency?: CurrencyDto, families: FamilyNameDto[] }`  
**`FamilyNameDto`** — `{ id, name }` (non-default family attributions only)  
**`TagDto`** — `{ id, name }`  
**`TagListDto`** — `{ own: TagDto[], family: TagDto[] }`  
**`ExpensePagedResponse`** — `{ items: ExpenseDto[], totalCount, page, pageSize, totalPages }`  
**`RateDto`** — `{ sourceCurrencyId, destinationCurrencyId, date, rate, rateSource }`  
**`RateConflictDto`** — `{ id, sourceCurrencyId, destinationCurrencyId, date, autoRate, manualRate, status, resolvedAt? }`  
**`AdminCategoryDto`** — `{ id, name, description?, isArchived, subcategories: AdminCategoryDto[] }`  
**`DashboardSummaryDto`** — `{ totalAmount, convertedTotal?, displayCurrency?: CurrencyDto, expenseCount, previousPeriodTotal?, changePercent?, topCategory?: SubcategoryDto, topCategoryAmount? }`  
**`MonthlyBreakdownDto`** — `{ year, month, totalAmount, convertedTotal?, byCategory: CategoryAmountDto[] }`  
**`CategoryAmountDto`** — `{ category?: SubcategoryDto, amount, convertedAmount? }`  
**`CategoryBreakdownDto`** — `{ category?: SubcategoryDto, totalAmount, convertedTotal?, percentage, subcategories: CategoryAmountDto[] }`  
**`SameMonthYearlyDto`** — `{ year, totalAmount, convertedTotal? }`  
**`CurrencyBreakdownDto`** — `{ currency: CurrencyDto, totalAmount, convertedAmount?, expenseCount }`

**Query params for `GET /expenses`:** `dateFrom`, `dateTo`, `categoryId`, `subcategoryId`, `currencyId`, `amountMin`, `amountMax`, `description` (substring), `tagIds[]` (OR filter), `displayCurrencyId`, `page` (default 1), `pageSize` (default 20)

DTOs live in `Controllers/DTO/`; error envelopes (`{ message }`) in `Controllers/Responses/`.

## API Documentation

Swagger UI is available in development or when `ENABLE_SWAGGER=true`:

```
http://localhost:9200/swagger
```

All endpoints have XML `<summary>` docs and full `[ProducesResponseType]` coverage.

## Architecture

Layered structure: **Controllers → Services → Repositories → DbContext**

- Migrations are applied automatically at startup via `db.Database.MigrateAsync()`
- Reads user data from the users service's PostgreSQL database via `Repositories/External/UserRepository` (read-only, `ext.USR_Users` table)
- `UserEventConsumer` (BackgroundService) subscribes to queue `expenses.users.sync` bound to `users.events` exchange (`user.#`); retries connection on `BrokerUnreachableException` every 5 s (host starts even if RabbitMQ not yet ready); uses **inbox deduplication** via `InboxEvents` table (`IInboxRepository.ExistsAsync` checked before processing; `InboxEvent { Status=Processed }` written on success); `user.created`/`user.updated` → `SaveOrUpdateUserAsync`, `user.deleted` → `DeleteUserAsync` on `ext.USR_Users`
- `FamilyOutboxPublisherService` (BackgroundService) polls `OutboxEvents` every 5 s (max 5 retries) and publishes to `expenses.events` topic exchange via `IFamilyEventPublisher.PublishRaw`; written by `FamilyService.RemoveMemberAsync` after member + attribution removal — provides durable at-least-once delivery even if RabbitMQ is down at removal time
- RabbitMQ connects to vhost `expense_management`; `expense_expenses` user has permissions only on this vhost. Env vars override `appsettings.json` values (env var checked first in `Program.cs`).

## Testing

```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory "./coverage" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
```

Use `TestExpensesDbContextWrapper` (in-memory DB) for repository tests.

## Pipeline & Quality

GitLab CI with SonarQube quality gate, Semgrep SAST, OWASP Dependency Check, and Trivy image scanning.

## Docker

Containerized and orchestrated via Docker Compose. Image built and deployed through the CI/CD pipeline.
