# Expenses Service

REST API for managing expenses, categories, and currencies.

## Tech Stack

- **.NET 8** — `net8.0` target framework
- **Entity Framework Core 8** + **Npgsql** — PostgreSQL via EF Core
- **xUnit** + **Moq** — unit and integration tests

## Usage

```bash
cd Touir.ExpensesManager.Expenses
dotnet restore
dotnet run
```

Service runs on port **9200** by default. Configuration via `appsettings.json` and environment variables.

## Key Endpoints

- `GET/POST/PUT/DELETE /expenses` — Expense CRUD
- `GET/POST/PUT/DELETE /categories` — Expense categories
- `GET /health` — Liveness/readiness probe

All endpoints (except health) require authentication, enforced by nginx's `auth_request` subrequest to the users service before forwarding.

## Architecture

Layered structure: **Controllers → Services → Repositories → DbContext**

- Migrations are applied automatically at startup via `db.Database.MigrateAsync()`
- Reads user data from the users service's PostgreSQL database via `Repositories/External/UserRepository` (read-only, `ext.USR_Users` table)
- Async event publishing/consuming via RabbitMQ (`IRabbitMQService`)

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
