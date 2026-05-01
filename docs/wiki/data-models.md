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
| `Email` | `varchar` | NOT NULL, unique | Email address (stored lowercase) |
| `FamilyId` | `int?` | FK, nullable | Family group (reserved for future use) |
| `CreatedAt` | `datetime` | NOT NULL | Account creation timestamp |
| `LastUpdatedAt` | `datetime` | NOT NULL | Last update timestamp |
| `IsEmailValidated` | `bool` | NOT NULL, default false | Whether email has been verified |
| `EmailValidationHash` | `varchar?` | nullable | Hash sent in verification email |
| `IsDisabled` | `bool` | NOT NULL, default false | Soft-delete / account disable flag |
| `CreatedById` | `int?` | FK → USR_Users, nullable | Who created this user |
| `LastUpdatedById` | `int?` | FK → USR_Users, nullable | Who last updated this user |

**Relationships:**
- One `Authentication` record (0..1)
- Many `UserRoles` (0..*)

**Notes:**
- Email is always stored lowercase (`ToLowerInvariant()` applied in controllers)
- `IsEmailValidated` must be `true` before a user can log in
- `EmailValidationHash` is cleared after use

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

Core expense table.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `int` | PK | Expense identifier |
| `Description` | `varchar?` | nullable | Optional description |
| `CreatedDate` | `datetime?` | nullable | When expense occurred |
| `IsHidden` | `bool` | NOT NULL | Soft-hide flag |
| `Amount` | `double` | NOT NULL | Monetary amount |
| `UserId` | `int` | FK → ext.USR_Users | Owner (read from users DB) |
| `CategoryId` | `int` | FK → CAT_Categories | Expense category |
| `CurrencyId` | `int` | FK → CUR_Currencies | Currency |

---

### CAT_Categories

Expense categories.

| Column | Type | Description |
|---|---|---|
| `Id` | `int` | PK |
| `Name` | `varchar` | Category name (e.g. "Food", "Transport") |

---

### CUR_Currencies

Currencies.

| Column | Type | Description |
|---|---|---|
| `Id` | `int` | PK |
| `Code` | `varchar` | ISO currency code (e.g. `USD`, `EUR`) |
| `Name` | `varchar` | Full name (e.g. "US Dollar") |

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

### Expenses Service Migrations

| Migration | Date | Description |
|---|---|---|
| `20260217225816_InitialCreate` | 2026-02-17 | Initial schema: expenses, categories, currencies |
