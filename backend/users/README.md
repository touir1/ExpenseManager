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
- `POST /auth/login` — JWT login
- `POST /auth/register` — User registration
- `POST /auth/request-password-reset` — Send reset email
- `POST /auth/reset-password` — Reset password with token
- `GET  /auth/check` — Internal auth check used by nginx `auth_request`

Protected:
- `POST /auth/change-password` — Change password (authenticated)
- `GET/POST/PUT/DELETE /users` — User CRUD
- `GET /health` — Liveness/readiness probe

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
