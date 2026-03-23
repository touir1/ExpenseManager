# Users Service

REST API for user management, authentication (JWT), and email verification.

## Tech Stack

- **.NET 8** — `net8.0` target framework
- **Entity Framework Core 8** + **Npgsql** — PostgreSQL via EF Core
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
- `POST /auth/login` — Authenticate user; sets `auth_token` as an `HttpOnly; Secure; SameSite=Strict` cookie and returns user info
- `POST /auth/logout` — Clear the `auth_token` cookie
- `GET  /auth/session` — Validate the `auth_token` cookie; returns 200 if valid, 401 otherwise (used for session restore on page load)
- `POST /auth/register` — User registration
- `GET  /auth/validate-email` — Verify email from link
- `POST /auth/request-password-reset` — Send reset email
- `POST /auth/change-password-reset` — Reset password with verification hash
- `GET  /auth/check` — Internal auth check used by nginx `auth_request`; accepts Bearer token header or `auth_token` cookie

Protected:
- `POST /auth/change-password` — Change password (requires email + old password)
- `GET/POST/PUT/DELETE /users` — User CRUD
- `GET /health` — Liveness/readiness probe

## Email Configuration

Configured via environment variables:

| Variable | Description | Default |
|---|---|---|
| `EXPENSES_MANAGEMENT_USERS_EMAILAUTH_HOST` | SMTP server hostname | `smtp.gmail.com` |
| `EXPENSES_MANAGEMENT_USERS_EMAILAUTH_PORT` | SMTP server port | `587` |
| `EXPENSES_MANAGEMENT_USERS_EMAILAUTH_EMAIL` | Sender address | — |
| `EXPENSES_MANAGEMENT_USERS_EMAILAUTH_PASSWORD` | SMTP password | — |
| `EXPENSES_MANAGEMENT_USERS_EMAILAUTH_ENABLESSL` | Enable SSL/TLS on the SMTP connection | `true` |

For local development, [Mailpit](https://mailpit.axllent.org/) is included in the tools Docker Compose stack (`host.docker.internal:1025`, `EnableSsl=false`). The web UI is available at `http://localhost:8025`.

## Database Schema

Manages tables: `USR_Users`, `ATH_Authentications`, `RLE_Roles`, `APP_Applications`, `RQA_RequestAccesses`, `RRA_RoleRequestAccesses`, `URR_UserRoles`, `ALW_AllowedOrigins`

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
