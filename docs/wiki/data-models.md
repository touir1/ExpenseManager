# Data Models

← [Wiki Index](./index.md)

---

## Overview

ExpenseManager uses two PostgreSQL databases — one for the users service and one for the expenses service — managed by EF Core migrations applied automatically at startup. The expenses service reads user data from the users database via a read-only cross-schema view.

---

## Users Service Database

### USR_Users

Core user account table.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `int` | PK, identity | User identifier |
| `FirstName` | `varchar` | NOT NULL | User's first name (max 100) |
| `LastName` | `varchar` | NOT NULL | User's last name (max 100) |
| `Email` | `varchar` | NOT NULL | Email address (stored lowercase) |
| `FamilyId` | `int?` | FK, nullable | Family group (reserved for future use) |
| `CreatedAt` | `datetime` | NOT NULL | Account creation timestamp |
| `LastUpdatedAt` | `datetime` | NOT NULL | Last update timestamp |
| `IsEmailValidated` | `bool` | NOT NULL, default false | Whether email has been verified |
| `EmailValidationHash` | `varchar?` | nullable | Hash sent in verification email |
| `EmailValidationHashExpiresAt` | `datetime?` | nullable | Expiry for verification link (null = no expiry for legacy rows); set to UtcNow + 24 h on register/resend |
| `IsDisabled` | `bool` | NOT NULL, default false | Account suspension flag (separate from soft delete) |
| `IsDeleted` | `bool` | NOT NULL, default false | Soft-delete flag |
| `DeletedAt` | `datetime?` | nullable | Soft-delete timestamp |
| `CreatedById` | `int?` | FK → USR_Users, nullable | Who created this user |
| `LastUpdatedById` | `int?` | FK → USR_Users, nullable | Who last updated this user |

**Relationships:**
- One `Authentication` record (0..1)
- Many `UserRoles` (0..*)

**Notes:**
- Email is always stored lowercase (`ToLowerInvariant()` applied in controllers)
- `IsEmailValidated` must be `true` before a user can log in
- `EmailValidationHash` is cleared after use; old link immediately invalidated when `UpdateEmailValidationHashAsync` is called
- Partial unique index `ux_usr_email_active` on `USR_Email WHERE USR_IsDeleted = FALSE` — same email can be reused after soft delete
- `UserRepository.DeleteUserAsync` sets `IsDeleted`/`DeletedAt` — never calls `.Remove()`; all query methods filter `!u.IsDeleted`

---

### ATH_Authentications

Stores hashed passwords. Linked 1-to-1 with `USR_Users`.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `UserId` | `int` | PK, FK → USR_Users | The user this authentication belongs to |
| `HashPassword` | `varchar` | NOT NULL | HMAC-SHA256 password hash (Base64) |
| `HashSalt` | `varchar` | NOT NULL | Random salt (Base64) |
| `IsTemporaryPassword` | `bool` | NOT NULL | Whether this is a temporary/initial password |
| `PasswordResetHash` | `varchar?` | nullable | Hash issued by `request-password-reset` |
| `PasswordResetRequestedAt` | `datetime?` | nullable | When reset was requested (24h TTL) |

**Security notes:**
- `HashPassword` is an HMAC-SHA256 digest, not bcrypt. The salt is stored separately.
- `PasswordResetHash` expires 24 hours after `PasswordResetRequestedAt`
- Both fields are cleared on successful password reset

---

### RTK_RefreshTokens

Opaque refresh token store.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `int` | PK | Token identifier |
| `UserId` | `int` | FK → USR_Users | Token owner |
| `TokenHash` | `varchar` | NOT NULL | Hash of the opaque token value |
| `ExpiresAt` | `datetime` | NOT NULL | Token expiry |
| `IsRevoked` | `bool` | NOT NULL, default false | Whether explicitly revoked |
| `CreatedAt` | `datetime` | NOT NULL | Issue timestamp |

**Notes:**
- The raw token value is never stored — only its hash
- Tokens are rotated on every `POST /auth/refresh` call
- All tokens for a user are revoked on `POST /auth/logout`

---

### RLE_Roles

Application roles.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `int` | PK | Role identifier |
| `Code` | `varchar` | NOT NULL | Role code (e.g. `USER`, `ADMIN`) |
| `Name` | `varchar` | NOT NULL | Human-readable name |
| `Description` | `varchar?` | nullable | Optional description |
| `IsDefault` | `bool` | NOT NULL, default false | Assigned automatically on registration |
| `ApplicationId` | `int` | FK → APP_Applications | Application this role belongs to |

---

### APP_Applications

Application registry (multi-tenant support).

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `int` | PK | Application identifier |
| `Code` | `varchar` | NOT NULL, unique | Application code (e.g. `EXPENSES_MANAGER`) |
| `Name` | `varchar` | NOT NULL | Display name |
| `Description` | `varchar?` | nullable | Optional description |
| `UrlPath` | `varchar?` | nullable | App base URL path (e.g. `/`) |
| `ResetPasswordUrlPath` | `varchar?` | nullable | Path for the create-password redirect after email verification |
| `VerifyEmailErrorUrlPath` | `varchar?` | nullable | Path to redirect on failed email verification (e.g. `/verify-error`) |

**Notes:**
- Seeded during migrations with the `EXPENSES_MANAGER` application
- `VerifyEmailErrorUrlPath` was added in migration `20260429200824_AddVerifyEmailErrorUrlPath`

---

### URR_UserRoles

User-to-role assignment join table.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `UserId` | `int` | FK → USR_Users | User |
| `RoleId` | `int` | FK → RLE_Roles | Role |
| `CreatedAt` | `datetime` | NOT NULL | Assignment timestamp |
| `CreatedById` | `int?` | FK → USR_Users, nullable | Who assigned the role |

---

### RQA_RequestAccesses

Request access rules per application (e.g. which routes/methods are accessible to which roles).

| Column | Type | Description |
|---|---|---|
| `Id` | `int` | PK |
| `ApplicationId` | `int` | FK → APP_Applications |
| Additional rule columns | — | Defined per migration |

---

### RRA_RoleRequestAccesses

Maps roles to request access rules.

| Column | Type | Description |
|---|---|---|
| `RoleId` | `int` | FK → RLE_Roles |
| `RequestAccessId` | `int` | FK → RQA_RequestAccesses |

---

### ALW_AllowedOrigins

Allowed CORS origins per application.

| Column | Type | Description |
|---|---|---|
| `Id` | `int` | PK |
| `ApplicationId` | `int` | FK → APP_Applications |
| `Origin` | `varchar` | Allowed origin URL |

---

### MSG_OutboxEvents

Outbox pattern — stores unpublished events before RabbitMQ delivery.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `long` | PK, GENERATED ALWAYS (Postgres) / INTEGER PK (SQLite) | Event identifier |
| `EventType` | `varchar` | NOT NULL | e.g. `user.created`, `user.updated`, `user.deleted` |
| `Payload` | `text` | NOT NULL | JSON payload |
| `MessageId` | `varchar` | NOT NULL, unique | Idempotency key for inbox deduplication |
| `Status` | `varchar` | NOT NULL | `Pending` / `Sent` / `Failed` |
| `RetryCount` | `int` | NOT NULL, default 0 | Publish attempt count (max 5) |
| `CreatedAt` | `datetime` | NOT NULL | Event creation timestamp |
| `PublishedAt` | `datetime?` | nullable | When successfully published |

**Notes:**
- Written by `IOutboxRepository.EnqueueAsync` — no shared transaction with the user insert
- `OutboxPublisherService` polls every 5 s, max 5 retries, then publishes via `IUserEventPublisher`
- `OutboxEvent.Id` uses `UseIdentityAlwaysColumn()` only when `Database.IsNpgsql()` so SQLite test runner emits `INTEGER PRIMARY KEY` (auto-increment). Never call `Migrate()` for `OutboxRepositoryTests` — use `EnsureCreated`.
- Replay via `POST /messaging/replay?eventType=&from=&forceAll=`; stats via `GET /messaging/outbox/stats`

---

## Users Service — Entity Relationship Diagram

```
USR_Users ──────────────────┐
  │                         │ (self-referential: CreatedBy, LastUpdatedBy)
  │ 1:1                     │
ATH_Authentications         │
                            │
  │ 1:*                     │
RTK_RefreshTokens           │
                            │
  │ *:*  (via URR_UserRoles)│
RLE_Roles ──────────────────┘
  │
  │ *:1
APP_Applications
  │
  │ 1:*
ALW_AllowedOrigins
RQA_RequestAccesses
  │
  │ *:*  (via RRA_RoleRequestAccesses)
RLE_Roles
```

---

## Expenses Service Database

### EXP_Expenses

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `long` | PK, identity | Expense identifier |
| `Description` | `varchar?` | nullable | Optional description |
| `CreatedDate` | `datetime?` | nullable | When expense occurred |
| `IsHidden` | `bool` | NOT NULL | Hide flag |
| `Amount` | `decimal` | NOT NULL | Monetary amount |
| `IsDeleted` | `bool` | NOT NULL, default false | Soft-delete flag |
| `DeletedAt` | `datetime?` | nullable | Soft-delete timestamp |
| `UserId` | `int` | FK → ext.USR_Users | Owner |
| `CategoryId` | `int` | FK → CAT_Categories | Top-level category |
| `SubcategoryId` | `int?` | FK → CAT_Categories, nullable | Subcategory (requires CategoryId) |
| `CurrencyId` | `int` | FK → CUR_Currencies | Currency |
| `CreatedFromId` | `int` | FK → OperationSource | How expense was created (1=SingleWeb) |

`ExpenseRepository` filters `!e.IsDeleted` in both `GetByIdAsync` and `GetPagedAsync`.

---

### CAT_Categories

Hierarchical — top-level categories have subcategories via self-referential `ParentId`.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `int` | PK | |
| `Name` | `varchar` | NOT NULL | Category name |
| `Description` | `varchar?` | nullable | Optional description |
| `ParentId` | `int?` | FK → CAT_Categories, nullable | Null = top-level |
| `IsDeleted` | `bool` | NOT NULL, default false | Soft-delete flag |
| `DeletedAt` | `datetime?` | nullable | Soft-delete timestamp |

`CategoryRepository.GetAllActiveAsync` filters `!c.IsDeleted` at top level; `CategoryService.GetAllAsync` also filters subcategories.

---

### CUR_Currencies

Seeded at migration time.

| Column | Type | Description |
|---|---|---|
| `Id` | `int` | PK |
| `Code` | `varchar` | ISO code (e.g. `USD`, `EUR`) |
| `Name` | `varchar` | Full name (e.g. "US Dollar") |

---

### FAM_Families

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `int` | PK | |
| `Name` | `varchar` | NOT NULL | Family display name |
| `IsDefault` | `bool` | NOT NULL | Created automatically on `user.created` |
| `IsDeleted` | `bool` | NOT NULL, default false | Soft-delete (archive) |
| `DeletedAt` | `datetime?` | nullable | Archive timestamp |
| `OwnerId` | `int` | FK → ext.USR_Users | Family owner (Head role) |

Only non-default families can be archived. `FamilyService.CreateDefaultAsync` is idempotent.

---

### FAM_FamilyMembers

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `int` | PK | |
| `FamilyId` | `int` | FK → FAM_Families | |
| `UserId` | `int` | FK → ext.USR_Users | |
| `RoleId` | `int` | FK → FamilyRole lookup | Head (1) or Member (2) |
| `JoinedAt` | `datetime` | NOT NULL | |

---

### FAM_FamilyInvitations

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `int` | PK | |
| `FamilyId` | `int` | FK → FAM_Families | |
| `InviteeEmail` | `varchar` | NOT NULL | Must match acceptor's email |
| `Token` | `varchar` | NOT NULL, unique | GUID token sent to invitee |
| `ExpiresAt` | `datetime` | NOT NULL | Configurable via `FamilyOptions.InviteExpiryInDays` (default 7 days) |
| `IsAccepted` | `bool` | NOT NULL, default false | |
| `CreatedAt` | `datetime` | NOT NULL | |

`AcceptInviteAsync` validates: token not expired, not already accepted, acceptor email = `InviteeEmail`.

---

### FAM_ExpenseFamilyAttributions

Join table — expense to family associations.

| Column | Type | Description |
|---|---|---|
| `ExpenseId` | `long` | FK → EXP_Expenses |
| `FamilyId` | `int` | FK → FAM_Families |

`RemoveMemberAsync` calls `RemoveMemberAttributionsAsync(familyId, ownerId)` to purge that member's expense attributions.

---

### EXP_AuditLog

| Column | Type | Description |
|---|---|---|
| `Id` | `long` | PK, identity |
| `ExpenseId` | `long` | FK → EXP_Expenses |
| `OperationId` | `int` | FK → AuditOperation lookup |
| `PerformedById` | `int` | User who made the change |
| `PerformedFromId` | `int` | FK → OperationSource lookup |
| `PerformedAt` | `datetime` | Timestamp |

---

### EXP_AuditSnapshots

| Column | Type | Description |
|---|---|---|
| `Id` | `long` | PK, identity |
| `AuditLogId` | `long` | FK → EXP_AuditLog |
| `SnapshotTypeId` | `int` | FK → SnapshotType lookup (1=Before, 2=After) |
| `FieldName` | `varchar` | Field that changed |
| `OldValue` | `varchar?` | Value before (null for After snapshots on Add) |
| `NewValue` | `varchar?` | Value after (null for Before snapshots on Delete) |
| `TagIds` | `varchar` | Comma-separated tag IDs (empty currently) |
| `FamilyIds` | `varchar` | Comma-separated family IDs (empty currently) |

---

### InboxEvents

RabbitMQ message deduplication for the expenses consumer.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `int` | PK | |
| `MessageId` | `varchar` | NOT NULL, unique | From `ea.BasicProperties.MessageId` (fallback: new GUID) |
| `Status` | `varchar` | NOT NULL | `Processed` |
| `ProcessedAt` | `datetime` | NOT NULL | |

---

### Lookup Tables

All seeded at migration time. Accessed via `ILookupCacheService` — never hardcode IDs.

| Table | Values |
|---|---|
| `OperationSource` | 1=SingleWeb, 2=SingleMobile, 3=BulkWeb |
| `ModifiedSource` | 1=Web, 2=Mobile |
| `FamilyRole` | 1=Head, 2=Member |
| `RateSource` | 1=Auto, 2=Manual |
| `ConflictStatus` | 1=Pending, 2=Resolved |
| `ConflictResolution` | 1=AcceptAuto, 2=KeepManual, 3=Custom |
| `AuditOperation` | 1=Add, 2=Update, 3=Delete |
| `SnapshotType` | 1=Before, 2=After |

---

## Cross-Service Data Access

The expenses service accesses user data from the users database via a PostgreSQL schema view:

```
expenses DB ──── ext.USR_Users (read-only view) ──── users DB (USR_Users table)
```

This is mapped in `ExpensesDbContext` and accessed via `External/UserRepository`. The expenses service never writes to this view or the underlying table.

---

## Migration History

### Users Service Migrations

| Migration | Date | Description |
|---|---|---|
| `20251227165426_InitialCreate` | 2025-12-27 | Initial schema: users, auth, roles, applications |
| `20251231180932_SeedInitialData` | 2025-12-31 | Seed EXPENSES_MANAGER application and default roles |
| `20251231182927_AddAllowedOrigin` | 2025-12-31 | Add `ALW_AllowedOrigins` table |
| `20260101165439_AddIsDefaultColumnRole` | 2026-01-01 | Add `IsDefault` to `RLE_Roles` |
| `20260101171739_SetDefaultRoles` | 2026-01-01 | Seed default role for EXPENSES_MANAGER |
| `20260101174904_SetResetPasswordUrlApplication` | 2026-01-01 | Add `ResetPasswordUrlPath` to `APP_Applications` |
| `20260323120000_UpdateApplicationUrls` | 2026-03-23 | Update application URL paths |
| `20260412165435_FixResetPasswordUrl` | 2026-04-12 | Fix `ResetPasswordUrlPath` value |
| `20260427220653_AddRefreshTokens` | 2026-04-27 | Add `RTK_RefreshTokens` table |
| `20260429200824_AddVerifyEmailErrorUrlPath` | 2026-04-29 | Add `VerifyEmailErrorUrlPath` to `APP_Applications` |

### Users Service Migrations (continued)

| Migration | Date | Description |
|---|---|---|
| `20260506224929_AddOutboxEvents` | 2026-05-06 | Add `MSG_OutboxEvents` table |
| `20260509140937_AddUserSoftDelete` | 2026-05-09 | Add `IsDeleted`, `DeletedAt` to `USR_Users`; partial unique index |
| `20260510122007_AddEmailValidationHashExpiry` | 2026-05-10 | Add `EmailValidationHashExpiresAt` to `USR_Users` |

### Expenses Service Migrations

| Migration | Date | Description |
|---|---|---|
| `20260217225816_InitialCreate` | 2026-02-17 | Initial schema: expenses, categories, currencies |
| `20260505144220_SchemaFoundation` | 2026-05-05 | Full schema rebuild: families, members, attributions, audit, lookups |
| `20260505145359_LongIdsForExpenseAndAudit` | 2026-05-05 | Change `EXP_Expenses.Id` and audit IDs to `long` |
| `20260506203552_SeedCurrencies` | 2026-05-06 | Seed currency lookup data |
| `20260506204543_SeedCategories` | 2026-05-06 | Seed category data |
| `20260506224942_AddInboxEvents` | 2026-05-06 | Add `InboxEvents` table |
| `20260509155613_ReplaceCategoryFamilyIsArchivedWithSoftDelete` | 2026-05-09 | Replace `IsArchived` with `IsDeleted`/`DeletedAt` on Category and Family |
| `20260509163919_AddExpenseSoftDelete` | 2026-05-09 | Add `IsDeleted`/`DeletedAt` to `EXP_Expenses` |
| `20260511130345_Phase4_FamilyInvitation` | 2026-05-11 | Add `FAM_FamilyInvitations` table |
