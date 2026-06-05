# Users Service

REST API for user management, authentication (JWT), and email verification.

## Tech Stack

- **.NET 8** — `net8.0` target framework
- **Entity Framework Core 8** + **Npgsql** — PostgreSQL via EF Core
- **FluentValidation 11** — request DTO validation with auto-validation middleware
- **RabbitMQ.Client 6.8.1** — event publishing to `users.events` topic exchange
- **xUnit** + **Moq** — unit and integration tests

## Usage

```bash
cd Touir.ExpensesManager.Users
dotnet restore
dotnet run
```

Service runs on port **9100** by default. Configuration via `appsettings.json` and environment variables.

## Key Endpoints

Public (no auth required, accessible via `/api/users/auth/` through nginx):
- `POST /auth/login` — Authenticate user; sets `auth_token` + `refresh_token` as `HttpOnly; Secure; SameSite=Strict` cookies; `RememberMe=true` → persistent cookies with `Expires`, `false` → session cookies; response body: `{ user: { firstName, lastName, email }, roles: RoleDto[], isAdmin: bool }`
- `POST /auth/logout` — Revoke refresh token and delete both `auth_token` + `refresh_token` cookies
- `GET  /auth/session` — Validate `auth_token` cookie; returns `{ email, firstName, lastName, isAdmin }` from JWT claims if valid, 401 otherwise (used for session restore on page load)
- `POST /auth/refresh` — Validate `refresh_token` cookie, issue new `auth_token`, rotate `refresh_token` (used transparently by the frontend on 401)
- `POST /auth/register` — User registration; if email exists but is unverified, silently resends a new verification link (old link invalidated) and returns success
- `GET  /auth/validate-email` — Verify email from link; link expires after 24 h; on success redirects to `{app.ResetPasswordUrlPath}?email=…&h=…&mode=create`; on failure redirects to `{app.UrlPath}{app.VerifyEmailErrorUrlPath}?email=…&app_code=…` if configured, otherwise returns `{"message":"EMAIL_VERIFICATION_FAILED"}`
- `POST /auth/resend-verification` — Resend verification email; generates new hash (invalidates old link); always returns `200 OK` regardless of account existence; body: `{ email, applicationCode }`
- `POST /auth/create-password` — Initial password setup after email verification; body: `{ email, verificationHash, newPassword }` — validates via `EmailValidationHash`; creates auth record if none exists
- `POST /auth/request-password-reset` — Send reset email; body: `{ email, appCode }` — link points to `ResetPasswordBaseUrl?email=…&h=…`
- `POST /auth/change-password-reset` — Reset password using a `PasswordResetHash` from `request-password-reset`; body: `{ email, verificationHash, newPassword }` — hash must be valid and issued within the last 24 hours
- `GET  /auth/check` — Internal auth check used by nginx `auth_request`; accepts Bearer token header or `auth_token` cookie

Protected:
- `POST /auth/change-password` — Change password (requires email + old password)
- `GET/POST/PUT/DELETE /users` — User CRUD
- `GET /health` — Liveness/readiness probe

Admin (require `APP_ADMIN` role; `[AdminAuthorize]` filter checks `isAdmin` JWT claim → 403 if absent/false):
- `GET  /admin/users` — Paged user list; query params: `search`, `page`, `pageSize`; returns `{ users: AdminUserDto[], total, page, pageSize }`
- `PATCH /admin/users/{id}/disable` — Disable user account → 204, 403 if self (`CANNOT_SELF_DISABLE`), 404
- `PATCH /admin/users/{id}/enable` — Enable user account → 204, 404
- `PUT  /admin/users/{id}/roles` — Replace user role list; body: `{ roleIds: int[] }` → 204, 403 if self removes APP_ADMIN (`CANNOT_REMOVE_OWN_ADMIN_ROLE`), 404

## API Documentation

Swagger UI is available in development or when `ENABLE_SWAGGER=true`:

```
http://localhost:9100/swagger
```

All 14 endpoints have XML `<summary>` docs and full `[ProducesResponseType]` coverage.

## Email

Email is **not sent by this service**. All outbound email is handled by the notifications service. The users service writes outbox events that the notifications service consumes and dispatches via SMTP.

`AuthenticationServiceOptions` still configures the base URLs embedded in email links:

| Variable | Description | Default |
|---|---|---|
| `EXPENSES_MANAGEMENT_USERS_AUTHSERVICE_VERIFY_EMAIL_URL` | Base URL for email verification links | `https://localhost:7114/api/auth/verifyEmail` |
| `EXPENSES_MANAGEMENT_USERS_AUTHSERVICE_RESET_PASSWORD_URL` | Base URL for password-reset links | `https://localhost/reset-password` |

## Messaging

Publishes to RabbitMQ exchange `users.events` (topic, durable) via the **outbox pattern** — events are first written to `MSG_OutboxEvents` in the same DB transaction as the business operation, then `OutboxPublisherService` (BackgroundService) polls and publishes with up to 5 retries.

| Routing key | Trigger |
|---|---|
| `user.created` | Email validation succeeds (`GET /auth/validate-email`) |
| `user.updated` | User profile update (future) |
| `user.deleted` | Validated user deletion (future) |
| `user.email.verification.requested` | Registration or resend-verification → notifications service sends the email |
| `user.password.reset.requested` | Password reset requested → notifications service sends the reset email |
| `user.password.changed` | Password changed or reset → notifications service sends a security confirmation email |

Config via `EXPENSES_MANAGEMENT_USERS_RABBITMQ_*` env vars (HostName, Port, UserName, Password).

### Messaging Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/messaging/replay` | Requeue failed/undelivered outbox events; query params: `eventType`, `from` (ISO datetime), `forceAll` (bool) |
| `GET`  | `/messaging/outbox/stats` | Counts of pending, published, and failed outbox events |

## Rate Limiting

All sensitive routes are protected by built-in .NET 8 `Microsoft.AspNetCore.RateLimiting` (fixed window, per client IP):

| Route | Limit | Window |
|-------|-------|--------|
| `POST /auth/login` | 10 req | 1 min |
| `POST /auth/register` | 5 req | 10 min |
| `POST /auth/resend-verification` | 3 req | 10 min |
| `GET  /auth/validate-email` | 10 req | 5 min |
| `POST /auth/request-password-reset` | 5 req | 10 min |
| `POST /auth/change-password-reset` | 5 req | 10 min |
| `POST /auth/create-password` | 5 req | 10 min |
| `POST /auth/change-password` | 10 req | 5 min |
| `POST /auth/refresh` | 20 req | 1 min |
| `POST /messaging/replay` | 5 req | 1 min |

Exceeding the limit returns `429 Too Many Requests`.

## Database Schema

Manages tables: `USR_Users`, `ATH_Authentications`, `RLE_Roles`, `APP_Applications`, `RQA_RequestAccesses`, `RRA_RoleRequestAccesses`, `URR_UserRoles`, `ALW_AllowedOrigins`, `RTK_RefreshTokens`

Migrations are applied automatically at startup via `db.Database.MigrateAsync()`.

## Testing

```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory "./coverage" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
```

## Pipeline & Quality

GitLab CI with SonarQube quality gate, Semgrep SAST, OWASP Dependency Check, and Trivy image scanning.

## Docker

Containerized and orchestrated via Docker Compose. Image built and deployed through the CI/CD pipeline.
