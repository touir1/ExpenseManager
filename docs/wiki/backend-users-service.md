# Backend — Users Service

← [Wiki Index](./index.md)

---

## Overview

The Users Service is the identity provider for the entire ExpenseManager system. It manages user accounts, authentication tokens, email verification, password lifecycle, role assignments, and application registry. All other services rely on it for auth enforcement via nginx's `auth_request` mechanism.

**Port:** `9100`  
**Base path (via nginx):** `/api/users/`  
**Solution:** `backend/users/Touir.ExpensesManager.Users.sln`

---

## Tech Stack

| Component | Technology |
|---|---|
| Framework | .NET 8 / ASP.NET Core |
| ORM | Entity Framework Core 8 + Npgsql |
| Validation | FluentValidation 11 (auto-validation middleware) |
| JWT | `System.IdentityModel.Tokens.Jwt` |
| Testing | xUnit + Moq + FluentValidation.TestHelper |
| Code quality | SonarQube, Semgrep, OWASP Dependency Check, Trivy |

---

## Project Structure

```
Touir.ExpensesManager.Users/
├── Program.cs                  ← Entry point, DI, migrations, FluentValidation config
├── Controllers/
│   ├── AuthenticationController.cs   ← Login, logout, session, refresh, check
│   ├── RegistrationController.cs     ← Register, validate-email
│   ├── PasswordController.cs         ← change-password, request-reset, reset, create-password
│   ├── EO/                           ← Output DTOs (ApplicationEo, RoleEo, UserEo)
│   ├── Requests/                     ← Input DTOs
│   └── Responses/                    ← Response shapes (ErrorResponse, LoginResponse, etc.)
├── Validators/                 ← FluentValidation validators (one per request DTO)
├── Services/
│   ├── AuthenticationService.cs      ← Credential verification
│   ├── JwtTokenService.cs            ← JWT generation and validation
│   ├── RefreshTokenService.cs        ← Opaque refresh token CRUD
│   ├── RegistrationService.cs        ← User creation, email hash generation
│   ├── PasswordManagementService.cs  ← Change/reset/create password
│   ├── RoleService.cs                ← Role queries per application
│   ├── ApplicationService.cs         ← Application registry queries
│   ├── UserRoleAssignmentService.cs  ← Default role assignment
│   └── Contracts/                    ← Service interfaces
├── Repositories/               ← EF Core data access
│   └── Contracts/              ← Repository interfaces
├── Models/                     ← Domain entities (User, Authentication, Role, etc.)
├── Infrastructure/
│   ├── UsersAppDbContext.cs     ← EF Core context
│   ├── CryptographyHelper.cs   ← HMAC password hashing
│   ├── EmailHelper.cs          ← Template loading, email dispatch
│   ├── SmtpEmailService.cs     ← SMTP implementation
│   ├── EmailHtmlTemplate.cs    ← Template key constants
│   ├── Contracts/              ← Infrastructure interfaces
│   └── Options/                ← Strongly-typed config sections
├── Assets/EmailTemplates/
│   ├── EMAIL_VERIFICATION_TEMPLATE.html
│   └── PASSWORD_RESET_TEMPLATE.html
└── Migrations/                 ← EF Core migrations (auto-applied at startup)
```

---

## Controllers

### AuthenticationController

Routes: `[Route("auth")]`

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/auth/login` | None | Authenticate user; issue `auth_token` + `refresh_token` cookies |
| POST | `/auth/logout` | Cookie | Revoke refresh token; delete both cookies |
| GET | `/auth/session` | Cookie | Validate `auth_token`; return user claims |
| POST | `/auth/refresh` | Cookie | Validate `refresh_token`; issue new `auth_token`; rotate refresh token |
| GET | `/auth/check` | Bearer or Cookie | Auth gate used by nginx `auth_request` |

**Cookie behavior:**
- Both `auth_token` and `refresh_token` are set as `HttpOnly; Secure; SameSite=Strict`
- `RememberMe = true` → `Expires` set (persistent)
- `RememberMe = false` → no `Expires` (session cookie)
- `BuildCookieOptions()` centralizes cookie configuration

### RegistrationController

Routes: `[Route("auth")]`

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/auth/register` | None | Create user; send verification email |
| GET | `/auth/validate-email` | None | Verify email hash; redirect to create-password or error page |

### PasswordController

Routes: `[Route("auth")]`

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/auth/change-password` | Cookie | Change password (requires old password) |
| POST | `/auth/request-password-reset` | None | Send reset email |
| POST | `/auth/change-password-reset` | None | Reset password via email hash (24h TTL) |
| POST | `/auth/create-password` | None | Create initial password after email verification |

---

## Services

### AuthenticationService

Responsibility: credential verification only.

- `AuthenticateAsync(email, password)` — loads user + authentication record; verifies HMAC hash; returns `UserEo` or `null`

### JwtTokenService

Responsibility: JWT token operations.

- `GenerateJwtToken(userId, email, firstName, lastName)` — creates signed JWT with claims: `sub`, `email`, `given_name`, `family_name`, `jti`
- `ValidateJwtToken(token)` — validates signature and expiry; returns `ClaimsPrincipal`

**Configuration** (`JwtAuthOptions`):
- `SecretKey` — HMAC-SHA256 signing key
- `ExpiryInMinutes` — token lifetime
- `Audience`, `Issuer`

### RefreshTokenService

Responsibility: opaque DB-backed refresh token lifecycle.

- `GenerateAsync(userId)` — creates random token; stores hash in `RTK_RefreshTokens`
- `ValidateAsync(token, userId)` — looks up by hash; validates not expired or revoked
- `RevokeAsync(token)` — marks token as revoked

### RegistrationService

Responsibility: user creation and email verification.

- `RegisterNewUserAsync(firstName, lastName, email, appCode)` — checks for duplicates; creates `USR_Users` record; generates `EmailValidationHash`; dispatches verification email; assigns default role; returns validation errors list
- `ValidateEmailAsync(hash, email, appCode)` — validates hash; marks `IsEmailValidated = true`; returns application for redirect URL construction
- `GenerateUniqueEmailValidationHashAsync()` — generates collision-resistant hash

### PasswordManagementService

Responsibility: full password lifecycle except login.

- `CreatePasswordAsync(email, verificationHash, newPassword)` — validates `EmailValidationHash`; creates or updates `ATH_Authentications`
- `ChangePasswordAsync(email, oldPassword, newPassword)` — verifies old password; updates hash/salt
- `ResetPasswordAsync(email, resetHash, newPassword)` — validates `PasswordResetHash` (≤ 24h); updates hash/salt; clears reset fields
- `RequestPasswordResetAsync(email, appCode)` — generates `PasswordResetHash`; sends reset email

### UserRoleAssignmentService

Responsibility: default role assignment after registration.

- `TryAssignDefaultRoleAsync(userId, appCode)` — finds default role for application; creates `URR_UserRoles` record

### RoleService

- `GetUserRolesByApplicationCodeAsync(appCode, userId)` — returns list of `RoleEo` for the given user and application

### ApplicationService

- `GetApplicationByCodeAsync(appCode)` — returns `ApplicationEo` (includes `UrlPath`, `ResetPasswordUrlPath`, `VerifyEmailErrorUrlPath`)

---

## Infrastructure

### CryptographyHelper

Implements HMAC-SHA256 password hashing:

- `GenerateRandomSalt()` — cryptographically random bytes, max size from `CryptographyOptions`
- `GeneratePasswordHash(password, salt)` — HMAC-SHA256 of password with salt
- `VerifyPasswordHash(password, storedHash, salt)` — constant-time comparison

### EmailHelper

Template loading and email dispatch:

- `GetEmailTemplate(templateKey, parameters)` — loads HTML template from `Assets/EmailTemplates/`; replaces `{{KEY}}` placeholders
- `SendEmail(to, subject, body)` — delegates to `IEmailService`
- `VerifyEmail(user, app)` — builds verification email content; sends via `SendEmail`

### SmtpEmailService

Implements `IEmailService` using `System.Net.Mail.SmtpClient`:

- `SendEmail(to, subject, body, attachments?)` — sends HTML email; returns bool success
- Configured via `EmailOptions` (host, port, credentials, SSL)

---

## Validators

All request DTOs have a corresponding FluentValidation validator. The auto-validation middleware returns `401` with `{ message: "FIRST_ERROR" }` on validation failure (preserving the existing wire format).

| Validator | Rules |
|---|---|
| `LoginRequestValidator` | `ApplicationCode`, `Email`, `Password` — NotEmpty → `MISSING_PARAMETERS` |
| `RegisterRequestValidator` | `FirstName`, `LastName`, `Email` — NotEmpty → `MISSING_PARAMETERS`; MaxLength(100) → `FIELD_TOO_LONG` |
| `ChangePasswordRequestValidator` | `Email`, `OldPassword` — NotEmpty; `NewPassword` — NotEmpty + MinLength(8) → `PASSWORD_TOO_SHORT` (CascadeMode.Stop) |
| `ChangePasswordResetRequestValidator` | `Email`, `VerificationHash` — NotEmpty; `NewPassword` — NotEmpty + MinLength(8) |
| `CreatePasswordRequestValidator` | Same as `ChangePasswordResetRequestValidator` |
| `RequestPasswordResetRequestValidator` | `Email`, `AppCode` — NotEmpty → `MISSING_PARAMETERS` |

---

## Configuration

All configuration is via `appsettings.json` and environment variables. Environment variables override `appsettings.json`.

### PostgreSQL (`PostgresOptions`)

| Environment Variable | Description |
|---|---|
| `EXPENSES_MANAGEMENT_USERS_DATABASE_SERVER` | DB host |
| `EXPENSES_MANAGEMENT_USERS_DATABASE_PORT` | DB port |
| `EXPENSES_MANAGEMENT_USERS_DATABASE_USERNAME` | DB user |
| `EXPENSES_MANAGEMENT_USERS_DATABASE_PASSWORD` | DB password |
| `EXPENSES_MANAGEMENT_USERS_DATABASE_DATABASE` | Database name |

### JWT (`JwtAuthOptions`)

| Environment Variable | Description |
|---|---|
| `EXPENSES_MANAGEMENT_USERS_JWT_SECRET_KEY` | HMAC-SHA256 signing key |
| `EXPENSES_MANAGEMENT_USERS_JWT_EXPIRY_IN_MINUTES` | Token lifetime |
| `EXPENSES_MANAGEMENT_USERS_JWT_AUDIENCE` | JWT audience claim |
| `EXPENSES_MANAGEMENT_USERS_JWT_ISSUER` | JWT issuer claim |

### Email (`EmailOptions`)

| Environment Variable | Default | Description |
|---|---|---|
| `EXPENSES_MANAGEMENT_USERS_EMAILAUTH_HOST` | `smtp.gmail.com` | SMTP host |
| `EXPENSES_MANAGEMENT_USERS_EMAILAUTH_PORT` | `587` | SMTP port |
| `EXPENSES_MANAGEMENT_USERS_EMAILAUTH_EMAIL` | — | Sender address |
| `EXPENSES_MANAGEMENT_USERS_EMAILAUTH_PASSWORD` | — | SMTP password |
| `EXPENSES_MANAGEMENT_USERS_EMAILAUTH_ENABLESSL` | `true` | TLS on SMTP |

### Authentication Service (`AuthenticationServiceOptions`)

| Environment Variable | Description |
|---|---|
| `EXPENSES_MANAGEMENT_USERS_AUTHSERVICE_VERIFY_EMAIL_URL` | Base URL for verification links |
| `EXPENSES_MANAGEMENT_USERS_AUTHSERVICE_RESET_PASSWORD_URL` | Base URL for password reset links |

### Cryptography (`CryptographyOptions`)

| Environment Variable | Description |
|---|---|
| `EXPENSES_MANAGEMENT_CRYPTOGRAPHY_MAXIMUM_SALT_SIZE` | Max size for random salt generation |

---

## Database

Migrations are applied automatically at startup via `db.Database.MigrateAsync()`.

**Tables managed by this service:**

| Table | Description |
|---|---|
| `USR_Users` | User accounts (name, email, validation state) |
| `ATH_Authentications` | Password hash + salt; reset hash |
| `RLE_Roles` | Application roles |
| `APP_Applications` | Registered applications (multi-tenant) |
| `RQA_RequestAccesses` | Request access rules per application |
| `RRA_RoleRequestAccesses` | Role-to-request-access mapping |
| `URR_UserRoles` | User-to-role assignments |
| `ALW_AllowedOrigins` | Allowed CORS origins per application |
| `RTK_RefreshTokens` | Opaque refresh token store |

See [Data Models](./data-models.md) for full entity details.

---

## Testing

```bash
cd backend/users
dotnet test --collect:"XPlat Code Coverage" --results-directory "./coverage" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
```

**Test count:** ~238 tests (as of v0.69.0)

**Test organization:**
```
Touir.ExpensesManager.Users.Tests/
├── Controllers/
│   ├── AuthenticationControllerTests.cs
│   ├── RegistrationControllerTests.cs
│   └── PasswordControllerTests.cs
├── Repositories/
│   ├── ApplicationRepositoryTests.cs
│   ├── AuthenticationRepositoryTests.cs
│   ├── RefreshTokenRepositoryTests.cs
│   ├── RoleRepositoryTests.cs
│   └── UserRepositoryTests.cs
├── Services/
│   ├── ApplicationServiceTests.cs
│   ├── AuthenticationServiceTests.cs
│   ├── PasswordManagementServiceTests.cs
│   ├── RegistrationServiceTests.cs
│   ├── RoleServiceTests.cs
│   └── UserRoleAssignmentServiceTests.cs
├── Infrastructure/
│   ├── CryptographyHelperTests.cs
│   ├── EmailHelperTests.cs
│   └── SmtpEmailServiceTests.cs
└── Validators/
    ├── LoginRequestValidatorTests.cs
    ├── RegisterRequestValidatorTests.cs
    ├── ChangePasswordRequestValidatorTests.cs
    ├── ChangePasswordResetRequestValidatorTests.cs
    ├── CreatePasswordRequestValidatorTests.cs
    └── RequestPasswordResetRequestValidatorTests.cs
```

All repository tests use a real PostgreSQL test database via `TestExpensesDbContext` (in-memory EF Core provider). Service tests use Moq for repository/infrastructure dependencies.

---

## Running Locally

```bash
cd backend/users/Touir.ExpensesManager.Users
dotnet restore
dotnet run
```

The service starts on port **9100**. Swagger UI is available at `http://localhost:9100/swagger` when `ENABLE_SWAGGER=true`.
