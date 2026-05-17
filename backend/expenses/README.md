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
| `POST` | `/expenses` | Create expense → `ExpenseDto` (201) |
| `PUT` | `/expenses/{id}` | Update expense → `ExpenseDto` (200) or 404 |
| `DELETE` | `/expenses/{id}` | Soft-delete expense → 204 or 404 |
| `GET` | `/expenses/{id}` | Get expense by id → `ExpenseDto` (200) or 404 |
| `GET` | `/expenses` | Paged + filtered expense list → `ExpensePagedResponse` |
| `GET` | `/families` | List families for authenticated user → `FamilyDto[]` |
| `POST` | `/families` | Create a new family → `FamilyDto` (201) |
| `GET` | `/families/{id}` | Family detail with members → `FamilyDetailDto` (200) or 403/404 |
| `PUT` | `/families/{id}` | Rename family (head only) → `FamilyDto` (200) or 403/404 |
| `POST` | `/families/{id}/archive` | Archive non-default family (head only) → 204 or 403/404 |
| `POST` | `/families/{id}/unarchive` | Unarchive family (head only) → 204 or 403/404 |
| `POST` | `/families/{id}/invite` | Invite user by email (head only) → `{ token }` (200) or 403/404/409 |
| `POST` | `/families/accept-invite` | Accept invitation by token → 204 or 400/403/409 |
| `DELETE` | `/families/{id}/members/{userId}` | Remove member (head only, not self) → 204 or 403/404 |
| `PUT` | `/families/{id}/members/{userId}/role` | Change member role (head only) → 204 or 403/404 |
| `GET` | `/tags` | Tags visible to user → `TagListDto { own, family }` |
| `POST` | `/tags` | Create/adopt tag by name (idempotent, case-sensitive) → `TagDto` (200) |
| `DELETE` | `/tags/{id}` | Remove user's adoption of tag → 204 or 404 (tag entity and expense history preserved) |
| `GET` | `/rates/history` | Rate history for a currency pair → `RateDto[]` (query: `sourceCurrencyId`, `destinationCurrencyId`) |
| `POST` | `/rates` | Add manual rate → `RateDto` (201); if auto rate exists for that date, creates a conflict instead |
| `POST` | `/rates/bulk` | Bulk add manual rates → 204 |
| `PUT` | `/rates/default` | Set/update global fallback rate for a currency pair → 204 |
| `GET` | `/rates/conflicts` | List pending rate conflicts → `RateConflictDto[]` |
| `POST` | `/rates/conflicts/{id}/resolve` | Resolve conflict (AcceptAuto / KeepManual / Custom) → 204 or 400/404 |
| `POST` | `/rates/refresh` | Backfill rates from provider → 204; body: `{ from: "YYYY-MM-DD", sourceCurrencyId?: int, destinationCurrencyId?: int }`; omit filters to refresh all pairs |
| `GET` | `/health` | Liveness/readiness probe |

All endpoints (except `/health`) require authentication, enforced by nginx's `auth_request` subrequest to the users service before forwarding.

`POST /expenses` and `PUT /expenses/{id}` return `403 Forbidden` if a provided `familyId` does not match a family the user belongs to, or if a `tagId` is not visible to the user (not in own tags or co-member tags).

**Tag visibility:** a tag is visible if the user has adopted it (`UserTag` row exists) OR any co-member of a shared non-deleted family has adopted it. Attaching a tag to an expense auto-adopts it for the requesting user.

**Display currency conversion:** `GET /expenses/{id}` and `GET /expenses` accept an optional `?displayCurrencyId={id}` query param. When set and the expense currency differs, `ExpenseDto.convertedAmount` and `ExpenseDto.displayCurrency` are populated. The rate resolution chain: same currency → 1.0; exact date match → most-recent-before fallback → global pair default → null (no conversion).

### Response DTOs

**`CategoryDto`** — `{ id, name, description?, subcategories: SubcategoryDto[] }`  
**`SubcategoryDto`** — `{ id, name, description? }`  
**`CurrencyDto`** — `{ id, code, name, symbol, decimals }`  
**`ExpenseDto`** — `{ id, amount, currency: CurrencyDto?, date, category: SubcategoryDto?, subcategory: SubcategoryDto?, description?, createdAt, modifiedAt?, modifiedFrom?, tags: TagDto[], convertedAmount?: decimal, displayCurrency?: CurrencyDto }`  
**`TagDto`** — `{ id, name }`  
**`TagListDto`** — `{ own: TagDto[], family: TagDto[] }`  
**`ExpensePagedResponse`** — `{ items: ExpenseDto[], totalCount, page, pageSize, totalPages }`  
**`RateDto`** — `{ sourceCurrencyId, destinationCurrencyId, date, rate, rateSource }`  
**`RateConflictDto`** — `{ id, sourceCurrencyId, destinationCurrencyId, date, automaticRate, manualRate, status, resolvedAt? }`

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
