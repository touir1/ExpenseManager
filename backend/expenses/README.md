# Expenses Service

This service provides the API for managing expenses.

## Tech Stack
- .NET 6+
- Entity Framework Core
- RESTful API

## Usage
- Run with `dotnet run` from `Touir.ExpensesManager.Expenses`.
- Configuration via `appsettings.json` and environment variables.

## Endpoints
- `/api/expenses` - CRUD operations for expenses
- `/api/categories` - Expense categories
- Health check endpoints configured for infrastructure liveness/readiness probes

## Pipeline & Quality
- Integrated with GitLab CI, including automated unit testing and rigorous SonarQube Quality Gates before deployment.

## Database
- SQL scripts in `sql_database_scripts/` for setup

## Docker
- Service can be containerized and orchestrated via Docker Compose.
