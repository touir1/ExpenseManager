# Backend — Expenses Service

← [Wiki Index](./index.md)

---

## Overview

The Expenses Service manages the core expense tracking domain: expenses, categories, currencies, and family groups. Authentication is not handled by this service — it is enforced at the nginx layer before any request reaches this service. User identity is extracted from the forwarded `auth_token` cookie by `JwtCookieReader` (no signature validation — nginx already validated). User data is also accessible read-only from the users database via a cross-schema view.

**Port:** `9200`  
**Base path (via nginx):** `/api/expenses/`  
**Solution:** `backend/expenses/Touir.ExpensesManager.Expenses.sln`

---

## Tech Stack

| Component | Technology |
|---|---|
| Framework | .NET 8 / ASP.NET Core |
| ORM | Entity Framework Core 8 + Npgsql |
| Validation | FluentValidation 11 (auto-validation middleware) |
| Messaging | RabbitMQ via custom `IRabbitMQService` |
| Rate limiting | .NET 8 `Microsoft.AspNetCore.RateLimiting` — sliding window |
| Testing | xUnit + Moq |
| Code quality | SonarQube, Semgrep, OWASP Dependency Check, Trivy |

---

## Project Structure

```
Touir.ExpensesManager.Expenses/
├── Program.cs                          ← Entry point, DI, migrations, FluentValidation config
├── Controllers/
│   ├── ExpenseController.cs            ← Expense CRUD (paginated list, add, update, soft-delete)
│   ├── CategoryController.cs           ← Category CRUD
│   ├── CurrencyController.cs           ← Currency list
│   ├── FamilyController.cs             ← Family management (10 endpoints)
│   ├── DTO/                            ← Reusable data objects (CategoryDto, CurrencyDto, SubcategoryDto)
│   ├── Requests/                       ← Request bodies
│   ├── Responses/                      ← Response envelopes and ErrorResponse
│   └── ControllerErrors.cs             ← Consolidated error string constants (same namespace, no using)
├── Assets/
│   └── EmailTemplates/
│       └── FAMILY_INVITATION_TEMPLATE.html   ← Invitation email; placeholders @@INVITER_NAME@@ @@FAMILY_NAME@@ @@INVITE_LINK@@ @@YEAR@@
├── Infrastructure/
│   ├── ExpensesDbContext.cs             ← EF Core context; maps ext.USR_Users cross-schema view
│   ├── JwtCookieReader.cs              ← Reads auth_token cookie, extracts sub claim as int (no sig check)
│   ├── LookupCacheService.cs           ← IMemoryCache-backed lookup table resolver
│   ├── EmailHelper.cs                  ← Template loading (auto-substitutes @@YEAR@@) + send delegation
│   ├── SmtpEmailService.cs             ← SMTP send via System.Net.Mail; returns bool success
│   ├── EmailHtmlTemplate.cs            ← Template key + variable name constants
│   ├── Contracts/
│   │   ├── IEmailService.cs
│   │   └── IEmailHelper.cs
│   └── Options/
│       ├── PostgresOptions.cs
│       ├── RabbitMQOptions.cs
│       ├── EmailOptions.cs             ← SMTP config (env prefix: EXPENSES_MANAGEMENT_EXPENSES_EMAILAUTH_*)
│       └── FamilyOptions.cs            ← InviteExpiryInDays + InviteBaseUrl
├── Models/
│   ├── Expense.cs
│   ├── Category.cs
│   ├── Currency.cs
│   ├── Family.cs
│   ├── FamilyMember.cs
│   ├── FamilyInvitation.cs
│   ├── ExpenseFamilyAttribution.cs
│   ├── ExpenseAuditLog.cs
│   ├── ExpenseAuditSnapshot.cs
│   ├── InboxEvent.cs
│   ├── Lookups/                        ← Enum-like domain values (FamilyRole, OperationSource, etc.)
│   └── External/
│       └── User.cs                     ← Read-only entity mapped to ext.USR_Users
├── Repositories/
│   ├── ExpenseRepository.cs
│   ├── CategoryRepository.cs
│   ├── CurrencyRepository.cs
│   ├── FamilyRepository.cs
│   ├── InboxRepository.cs
│   ├── Contracts/
│   └── External/
│       └── UserRepository.cs           ← GetUserByIdAsync (filters soft-deleted), GetUserByEmailAsync
├── Services/
│   ├── ExpenseService.cs               ← Uses IFamilyRepository for attribution writes
│   ├── CategoryService.cs
│   ├── FamilyService.cs
│   ├── ExpenseAuditService.cs          ← Audit log + snapshots (injected into ExpenseService)
│   ├── LookupCacheService.cs
│   ├── RabbitMQService.cs
│   ├── UserEventConsumer.cs            ← BackgroundService; inbox dedup; calls FamilyService.CreateDefaultAsync on user.created
│   └── Contracts/
└── Migrations/                         ← EF Core migrations (auto-applied at startup)
```

---

## Controllers

### ExpenseController

Rate-limited: `[EnableRateLimiting("expenses_global")]`

| Method | Route | Description |
|---|---|---|
| GET | `/expenses` | Paginated list of expenses for authenticated user (filters by userId) |
| POST | `/expenses` | Create expense; `FamilyIds` null → default family attribution |
| PUT | `/expenses/{id}` | Update expense |
| DELETE | `/expenses/{id}` | Soft-delete expense (sets `IsDeleted=true`, `DeletedAt`) |
| GET | `/health` | Liveness/readiness probe (no auth, no rate limit) |

**User identification:** `JwtCookieReader.GetUserId(Request)` — reads `auth_token` cookie, base64url-decodes JWT payload, extracts `sub` as `int`. Returns `null` → 401.

**Family attribution on create/update:** `FamilyIds int[]?` in request body:
- `null` → auto-attribute to user's default family
- `[]` → no attribution
- `[id1, id2]` → validate membership per familyId; `FamilyForbiddenException` → 403

**Audit:** Every create/update/delete writes to `ExpenseAuditLog` + `ExpenseAuditSnapshot(s)` via `ExpenseAuditService`.

### CategoryController

Rate-limited: `[EnableRateLimiting("expenses_global")]`

| Method | Route | Description |
|---|---|---|
| GET | `/categories` | List all active top-level categories with active subcategories |
| POST | `/categories` | Create category |
| PUT | `/categories/{id}` | Update category |
| DELETE | `/categories/{id}` | Soft-delete category |

### CurrencyController

Rate-limited: `[EnableRateLimiting("expenses_global")]`

| Method | Route | Description |
|---|---|---|
| GET | `/currencies` | List all currencies |

### FamilyController

| Method | Route | Description |
|---|---|---|
| GET | `/families` | List families for authenticated user |
| GET | `/families/{id}` | Get family detail with members |
| POST | `/families` | Create named family |
| PUT | `/families/{id}/rename` | Rename family |
| POST | `/families/{id}/archive` | Soft-delete non-default family |
| POST | `/families/{id}/unarchive` | Restore archived family |
| POST | `/families/{id}/invite` | Send invitation (GUID token, configurable expiry default 7 days); sends HTML email with inviter name |
| POST | `/families/{id}/accept-invite` | Accept invitation (validates token + email match) |
| DELETE | `/families/{id}/members/{memberId}` | Remove member + purge their expense attributions |
| PUT | `/families/{id}/members/{memberId}/role` | Change member role (Head/Member) |

**Exceptions → HTTP:**

| Exception | Status |
|---|---|
| `FamilyForbiddenException` | 403 |
| `FamilyNotFoundException` | 404 |
| `FamilyConflictException` | 409 |
| `FamilyInvitationException` | 400 |

---

## Domain Models

### Expense

```csharp
public class Expense
{
    public long Id { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedDate { get; set; }
    public bool IsHidden { get; set; }
    public decimal Amount { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public int? SubcategoryId { get; set; }
    public int CurrencyId { get; set; }
    public int CreatedFromId { get; set; }   // OperationSource lookup
    // Navigation properties: Currency, Category, Subcategory, User
}
```

**`ExpenseDto` shape:** nested objects — `Currency: CurrencyDto?`, `Category: SubcategoryDto?`, `Subcategory: SubcategoryDto?`. Built by `ExpenseService.MapToDto` from navigation properties (loaded via `Include` in the repository query).

### Category

Hierarchical: a top-level category has subcategories.

```csharp
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public List<Category> Subcategories { get; set; }
}
```

`CategoryRepository.GetAllActiveAsync` filters `!c.IsDeleted` on top-level; `CategoryService.GetAllAsync` filters subcategories by `!s.IsDeleted`.

### Family

```csharp
public class Family
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsDefault { get; set; }
    public bool IsDeleted { get; set; }      // soft-delete (archive)
    public DateTime? DeletedAt { get; set; }
    public int OwnerId { get; set; }
    public List<FamilyMember> Members { get; set; }
}
```

`FamilyService.CreateDefaultAsync(userId)` is idempotent — checks `HasDefaultFamilyAsync` before creating. Called by `UserEventConsumer` on `user.created`.

### FamilyInvitation

```csharp
public class FamilyInvitation
{
    public int Id { get; set; }
    public int FamilyId { get; set; }
    public string InviteeEmail { get; set; }
    public string Token { get; set; }        // GUID
    public DateTime ExpiresAt { get; set; }  // configurable via FamilyOptions.InviteExpiryInDays
    public bool IsAccepted { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

`AcceptInviteAsync` validates token not expired/already accepted and that acceptor's email matches `InviteeEmail`.

### Lookup Tables

All enum-like domain values are DB lookup tables in `Models/Lookups/`, not C# enums. Use `ILookupCacheService.GetIdAsync<T>(name)` / `GetNameAsync<T>(id)` to resolve. Never hardcode IDs.

| Model | Seed IDs |
|---|---|
| `OperationSource` | 1=SingleWeb, 2=SingleMobile, 3=BulkWeb |
| `ModifiedSource` | 1=Web, 2=Mobile |
| `FamilyRole` | 1=Head, 2=Member |
| `RateSource` | 1=Auto, 2=Manual |
| `ConflictStatus` | 1=Pending, 2=Resolved |
| `ConflictResolution` | 1=AcceptAuto, 2=KeepManual, 3=Custom |
| `AuditOperation` | 1=Add, 2=Update, 3=Delete |
| `SnapshotType` | 1=Before, 2=After |

Role name→ID resolved via `ILookupCacheService.GetIdAsync<FamilyRole>` — normalised to title case before lookup.

---

## RabbitMQ Integration

### UserEventConsumer (BackgroundService)

Consumes queue `expenses.users.sync` (binding `user.#` on `users.events` topic exchange):

1. Extract `MessageId` from `ea.BasicProperties.MessageId` (fallback: new GUID)
2. Check `IInboxRepository.ExistsAsync` — duplicate → ack and skip
3. Process: `SaveOrUpdateUserAsync` or `DeleteUserAsync` on `IUserRepository`
4. On `user.created`: also calls `FamilyService.CreateDefaultAsync(userId)` (idempotent)
5. On success: write `InboxEvent { Status="Processed" }` then ack
6. On failure: nack without inbox write

Retries on `BrokerUnreachableException` every 5 s — host starts even if RabbitMQ not yet ready.  
`RabbitMQService` requires `DispatchConsumersAsync = true` for async consumer.

**Configuration:**

| Environment Variable | Description |
|---|---|
| `EXPENSES_MANAGEMENT_EXPENSES_RABBITMQ_HOSTNAME` | RabbitMQ server hostname |
| `EXPENSES_MANAGEMENT_EXPENSES_RABBITMQ_PORT` | RabbitMQ port (default 5672) |
| `EXPENSES_MANAGEMENT_EXPENSES_RABBITMQ_USERNAME` | RabbitMQ username |
| `EXPENSES_MANAGEMENT_EXPENSES_RABBITMQ_PASSWORD` | RabbitMQ password |

Both services connect to vhost `expense_management`. Expenses service uses credentials with perms only on this vhost (not `/`).

---

## Expense Audit

`ExpenseAuditService` (injected into `ExpenseService`, never exposed via controller):

- **Add** → 1 after-snapshot
- **Update** → before + after snapshots
- **Delete** → 1 before-snapshot

Snapshots store comma-separated tag/family IDs. `PerformedFromId` uses `OperationSource` seed ID 1 (SingleWeb, hardcoded for web controller).

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

### Family (`FamilyOptions`)

| Environment Variable | Default | Description |
|---|---|---|
| `EXPENSES_MANAGEMENT_EXPENSES_FAMILY_INVITE_EXPIRY_IN_DAYS` | `7` | Invitation token TTL |
| `EXPENSES_MANAGEMENT_EXPENSES_FAMILY_INVITE_BASE_URL` | `https://localhost/families/accept-invite` | Base URL for invitation link in email |

### Email (`EmailOptions`)

| Environment Variable | Default | Description |
|---|---|---|
| `EXPENSES_MANAGEMENT_EXPENSES_EMAILAUTH_HOST` | `smtp.gmail.com` | SMTP server hostname |
| `EXPENSES_MANAGEMENT_EXPENSES_EMAILAUTH_PORT` | `587` | SMTP port |
| `EXPENSES_MANAGEMENT_EXPENSES_EMAILAUTH_EMAIL` | — | Sender email address |
| `EXPENSES_MANAGEMENT_EXPENSES_EMAILAUTH_PASSWORD` | — | SMTP password |
| `EXPENSES_MANAGEMENT_EXPENSES_EMAILAUTH_ENABLE_SSL` | `true` | Enable TLS; set `false` for local Mailpit |

---

## Database

Migrations are applied automatically at startup via `db.Database.MigrateAsync()`. Raw SQL migrations use `CURRENT_TIMESTAMP` (not `NOW()`) to remain compatible with the SQLite test runner.

**Tables:**

| Table | Description |
|---|---|
| `EXP_Expenses` | Expense records (soft-delete) |
| `CAT_Categories` | Expense categories and subcategories (soft-delete) |
| `CUR_Currencies` | Supported currencies (seeded) |
| `FAM_Families` | Family groups (soft-delete / archive) |
| `FAM_FamilyMembers` | Family membership with role |
| `FAM_FamilyInvitations` | Pending/accepted invitations |
| `FAM_ExpenseFamilyAttributions` | Expense↔family association rows |
| `EXP_AuditLog` | Expense audit log entries |
| `EXP_AuditSnapshots` | Before/after field snapshots |
| `InboxEvents` | Consumed RabbitMQ message deduplication |
| Lookup tables | `OperationSource`, `ModifiedSource`, `FamilyRole`, `RateSource`, `ConflictStatus`, `ConflictResolution`, `AuditOperation`, `SnapshotType` |

**External (read-only):**

| View | Description |
|---|---|
| `ext.USR_Users` | Cross-schema read of users service user table |

---

## Testing

```bash
cd backend/expenses
dotnet test --collect:"XPlat Code Coverage" --results-directory "./coverage" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
```

**Test count:** ~454 tests (as of v0.103.1)

**Test organization:**
```
Touir.ExpensesManager.Expenses.Tests/
├── TestHelpers/
│   ├── TestExpensesDbContext.cs         ← In-memory EF Core context for repository tests
│   └── TestExpensesDbContextWrapper.cs  ← IDisposable wrapper
├── Controllers/
│   ├── ExpenseControllerTests.cs
│   ├── CategoryControllerTests.cs
│   ├── CurrencyControllerTests.cs
│   └── FamilyControllerTests.cs
├── Repositories/
│   ├── ExpenseRepositoryTests.cs
│   ├── CategoryRepositoryTests.cs
│   ├── FamilyRepositoryTests.cs
│   └── External/
│       └── UserRepositoryTests.cs
├── Services/
│   ├── ExpenseServiceTests.cs
│   ├── CategoryServiceTests.cs
│   ├── FamilyServiceTests.cs
│   └── RabbitMQServiceTests.cs
└── Validators/
    ├── ExpenseRequestValidatorTests.cs
    └── FamilyValidatorTests.cs
```

All repository tests use `TestExpensesDbContextWrapper` (in-memory EF Core). Service tests use Moq. Never mock DbContext.

---

## Running Locally

```bash
cd backend/expenses/Touir.ExpensesManager.Expenses
dotnet restore
dotnet run
```

The service starts on port **9200**. Swagger UI is available at `http://localhost:9200/swagger` when `ENABLE_SWAGGER=true`.
