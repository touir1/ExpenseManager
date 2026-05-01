# Backend — Expenses Service

← [Wiki Index](./index.md)

---

## Overview

The Expenses Service manages the core expense tracking domain: expenses, categories, and currencies. Authentication is not handled by this service — it is enforced at the nginx layer before any request reaches this service. User data is accessed read-only from the users database via a cross-schema view.

**Port:** `9200`  
**Base path (via nginx):** `/api/expenses/`  
**Solution:** `backend/expenses/Touir.ExpensesManager.Expenses.sln`

---

## Tech Stack

| Component | Technology |
|---|---|
| Framework | .NET 8 / ASP.NET Core |
| ORM | Entity Framework Core 8 + Npgsql |
| Messaging | RabbitMQ via custom `IRabbitMQService` |
| Testing | xUnit + Moq |
| Code quality | SonarQube, Semgrep, OWASP Dependency Check, Trivy |

---

## Project Structure

```
Touir.ExpensesManager.Expenses/
├── Program.cs                     ← Entry point, DI, migrations
├── Infrastructure/
│   ├── ExpensesDbContext.cs        ← EF Core context; maps ext.USR_Users cross-schema view
│   └── Options/
│       ├── PostgresOptions.cs
│       └── RabbitMQOptions.cs
├── Models/
│   ├── Expense.cs                  ← Core expense entity
│   ├── Category.cs                 ← Expense category
│   ├── Currency.cs                 ← Currency (e.g. USD, EUR)
│   └── External/
│       └── User.cs                 ← Read-only user entity mapped to ext.USR_Users
├── Repositories/
│   └── External/
│       ├── UserRepository.cs       ← Read-only cross-service user access
│       └── Contracts/
│           └── IUserRepository.cs
├── Services/
│   ├── RabbitMQService.cs          ← RabbitMQ connection and messaging
│   └── Contracts/
│       └── IRabbitMQService.cs
└── Migrations/
    ├── 20260217225816_InitialCreate.cs
    └── ExpensesDbContextModelSnapshot.cs
```

---

## Domain Models

### Expense

The primary entity. Belongs to a user, a category, and a currency.

```csharp
public class Expense
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedDate { get; set; }
    public bool IsHidden { get; set; }
    public double Amount { get; set; }
    public External.User User { get; set; }
    public Category Category { get; set; }
    public Currency Currency { get; set; }
}
```

### Category

Classifies expenses (e.g. Food, Transport, Utilities).

```csharp
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    // ... additional fields per migration
}
```

### Currency

Represents a currency used for an expense amount.

```csharp
public class Currency
{
    public int Id { get; set; }
    public string Code { get; set; }  // e.g. "USD", "EUR"
    public string Name { get; set; }
    // ... additional fields per migration
}
```

### External User (read-only)

A read-only mapping of the users service's `USR_Users` table, accessed via the `ext.USR_Users` view. The expenses service never writes to this entity.

```csharp
// External/User.cs - maps to ext.USR_Users schema view
public class User
{
    public int Id { get; set; }
    // ... fields from USR_Users (read-only)
}
```

---

## Endpoints

All endpoints (except `/health`) require authentication enforced by nginx's `auth_request` mechanism.

| Method | Route | Description |
|---|---|---|
| GET | `/expenses` | List expenses for the authenticated user |
| POST | `/expenses` | Create a new expense |
| PUT | `/expenses/{id}` | Update an existing expense |
| DELETE | `/expenses/{id}` | Delete an expense |
| GET | `/categories` | List categories |
| POST | `/categories` | Create a category |
| PUT | `/categories/{id}` | Update a category |
| DELETE | `/categories/{id}` | Delete a category |
| GET | `/currencies` | List currencies |
| GET | `/health` | Liveness/readiness probe (no auth) |

*Note: Controller implementation details are not yet documented in source — refer to the codebase for full request/response shapes.*

---

## Authentication Enforcement

This service does **not** validate JWTs. Authentication is entirely handled by nginx:

```
Client request → nginx
  nginx sends auth_request to GET /internal/auth/check (users-service)
    ├─ 200 → nginx proxies to expenses-service (request forwarded)
    └─ 401 → nginx returns 401 to client (expenses-service never sees request)
```

The service trusts that any request it receives has already been authenticated. To identify the requesting user, the service reads user identity from the forwarded cookie or JWT claims — or from the cross-schema `ext.USR_Users` view by correlating user ID.

---

## Cross-Service User Access

The expenses service reads user data via `Repositories/External/UserRepository`:

- Connects to the **same PostgreSQL instance** as the users service
- Reads from `ext.USR_Users` — a schema-isolated view (not a direct table write)
- Used to associate expenses with user records and for user lookups

This is a **one-way, read-only dependency**. The expenses service never writes user data.

```
ExpensesDbContext
  └── Reads: ext.USR_Users  (users service's schema, read-only view)
  └── Writes: expenses schema tables (EXP_Expenses, CAT_Categories, CUR_Currencies)
```

---

## RabbitMQ Integration

The expenses service uses `IRabbitMQService` for async event messaging.

```csharp
public interface IRabbitMQService
{
    IConnection GetConnection();
}
```

`RabbitMQService` creates and manages a RabbitMQ connection using `RabbitMQOptions` (hostname, port, credentials). The specific events published or consumed are defined by individual use cases as they are implemented.

**Configuration (`RabbitMQOptions`):**

| Environment Variable | Description |
|---|---|
| `EXPENSES_MANAGEMENT_EXPENSES_RABBITMQ_HOSTNAME` | RabbitMQ server hostname |
| `EXPENSES_MANAGEMENT_EXPENSES_RABBITMQ_PORT` | RabbitMQ port (default 5672) |
| `EXPENSES_MANAGEMENT_EXPENSES_RABBITMQ_USERNAME` | RabbitMQ username |
| `EXPENSES_MANAGEMENT_EXPENSES_RABBITMQ_PASSWORD` | RabbitMQ password |

---

## Configuration

### PostgreSQL (`PostgresOptions`)

| Environment Variable | Description |
|---|---|
| `EXPENSES_MANAGEMENT_EXPENSES_DATABASE_SERVER` | DB host |
| `EXPENSES_MANAGEMENT_EXPENSES_DATABASE_PORT` | DB port |
| `EXPENSES_MANAGEMENT_EXPENSES_DATABASE_USERNAME` | DB user |
| `EXPENSES_MANAGEMENT_EXPENSES_DATABASE_PASSWORD` | DB password |
| `EXPENSES_MANAGEMENT_EXPENSES_DATABASE_DATABASE` | Database name |

---

## Database

Migrations are applied automatically at startup via `db.Database.MigrateAsync()`.

**Tables:**

| Table | Description |
|---|---|
| `EXP_Expenses` | Expense records |
| `CAT_Categories` | Expense categories |
| `CUR_Currencies` | Supported currencies |

**External (read-only):**

| View | Description |
|---|---|
| `ext.USR_Users` | Cross-schema read of users service user table |

SQL scripts for initial database setup are in `backend/expenses/sql_database_scripts/`:
- `create_database.sql`
- `create_tables.sql`
- `create_user_and_privileges.sql`

---

## Testing

```bash
cd backend/expenses
dotnet test --collect:"XPlat Code Coverage" --results-directory "./coverage" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
```

**Test organization:**
```
Touir.ExpensesManager.Expenses.Tests/
├── TestHelpers/
│   ├── TestExpensesDbContext.cs      ← In-memory EF Core context for repository tests
│   └── TestDbContextWrapper.cs       ← IDisposable wrapper; handles context lifecycle
├── Repositories/External/
│   └── UserRepositoryTests.cs        ← Tests for cross-service user access
└── Services/
    └── RabbitMQServiceTests.cs        ← Tests for RabbitMQ service initialization
```

`TestDbContextWrapper` implements `IDisposable` and wraps `TestExpensesDbContext` to ensure each test gets a clean database state.

---

## Running Locally

```bash
cd backend/expenses/Touir.ExpensesManager.Expenses
dotnet restore
dotnet run
```

The service starts on port **9200**. Swagger UI is available at `http://localhost:9200/swagger` when `ENABLE_SWAGGER=true`.

---

## Planned Development

The expenses backend is architecturally complete but the frontend UI is not yet implemented. Planned next steps per the library adoption roadmap:

- **TanStack Query** — for paginated expense lists, background refresh, and cache invalidation in the frontend
- **Recharts** — for spending charts (by category, over time, monthly comparison)

See [Use Cases](./use-cases.md) and the [ongoing issues doc](../issues/ongoing/fixes-and-suggestions.md) for the full roadmap.
