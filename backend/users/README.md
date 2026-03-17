# Users Service

This service provides the API for user management and authentication.

## Tech Stack
- .NET 6+
- Entity Framework Core
- RESTful API

## Usage
- Run with `dotnet run` from `Touir.ExpensesManager.Users`.
- Configuration via `appsettings.json` and environment variables.

## Endpoints
- `/api/users` - CRUD operations for users
- `/api/auth` - Authentication endpoints
- Health check endpoints configured for infrastructure liveness/readiness probes

## Pipeline & Quality
- Integrated with GitLab CI, including automated unit testing and rigorous SonarQube Quality Gates before deployment.

## Database
- SQL scripts in `Touir.ExpensesManager.Users/sql_database_scripts/` for setup

## Docker
- Service can be containerized and orchestrated via Docker Compose.
