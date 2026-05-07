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
| `GET` | `/health` | Liveness/readiness probe |

All endpoints (except `/health`) require authentication, enforced by nginx's `auth_request` subrequest to the users service before forwarding.

### Response DTOs

**`CategoryDto`** — `{ id, name, description?, subcategories: SubcategoryDto[] }`  
**`SubcategoryDto`** — `{ id, name, description? }`  
**`CurrencyDto`** — `{ id, code, name, symbol, decimals }`

DTOs live in `Controllers/DTO/`; error envelopes (`{ message }`) in `Controllers/Responses/`.

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
