# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ExpenseManager is a microservices application with two .NET 8 backend services (expenses, users), a React/TypeScript frontend dashboard, and a Docker-based infrastructure stack using nginx, PostgreSQL, RabbitMQ, Prometheus, and Grafana.

---

## Commands

### Backend (.NET)

Run from the solution directory (e.g., `backend/expenses/` or `backend/users/`):

```bash
# Build
dotnet build --configuration Release

# Run all tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory "./coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

# Run a single test
dotnet test --filter FullyQualifiedName~ClassName.MethodName

# Run the service locally
cd backend/expenses/Touir.ExpensesManager.Expenses
dotnet run

cd backend/users/Touir.ExpensesManager.Users
dotnet run
```

### Frontend (`frontend/dashboard`)

```bash
npm ci
npm run dev          # Development server
npm run build:prod   # Production build
npm run typecheck    # TypeScript type checking
npm test             # Run tests with coverage
npm run test:watch   # Watch mode
```

### Infrastructure

```bash
cd infrastructure
./run-docker-compose-tools.bat   # Start supporting tools (RabbitMQ, Grafana, Prometheus)
./run-docker-compose-apps.bat    # Start app containers (nginx, users-service, expenses-service)
```

---

## Architecture

### Services

| Service | Port | Responsibility |
|---|---|---|
| `users-service` | 9100 | Authentication (JWT), user management, email verification |
| `expenses-service` | 9200 | Expense CRUD, categories, currencies |
| `nginx` | 80/443 | Reverse proxy, TLS termination, auth gating |

### Request Flow

All traffic enters through **nginx**, which enforces authentication via a subrequest to `/internal/auth/check` (proxied internally to `users-service:9100/auth/check`) before forwarding to backend services. Public endpoints (e.g., `/api/users/auth/`) bypass this check.

Route mapping in `infrastructure/configs/nginx/sites-available/expenses-manager.conf`:
- `/api/users/auth/` → `users-service:9100/auth/` (public)
- `/api/users/` → `users-service:9100/` (auth-gated)
- `/api/expenses/` → `expenses-service:9200/` (auth-gated)

### Backend Layer Pattern

Both .NET services share the same layered structure:
- **Controllers/** — HTTP endpoints; use request/response DTOs (`Requests/`, `Responses/`, `EO/`)
- **Services/** — Business logic; injected via interfaces from `Services/Contracts/`
- **Repositories/** — Data access via EF Core; injected via interfaces from `Repositories/Contracts/`
- **Infrastructure/** — `DbContext`, config `Options/`, helpers (crypto, email)
- **Models/** — EF Core entity models (not used directly in controllers)
- **Messaging/** — RabbitMQ consumers/publishers
- **Migrations/** — EF Core migrations, applied automatically at startup via `db.Database.MigrateAsync()`

### Cross-Service Data Access

The expenses service reads user data directly from the users PostgreSQL database via an external repository (`Repositories/External/UserRepository`), querying the `ext.USR_Users` table. This is a read-only relationship mapped in `ExpensesDbContext`.

### Inter-Service Messaging

RabbitMQ is used for async messaging between services. The `IRabbitMQService` (registered as singleton) exposes `GetConnection()` for publishing/consuming. Configuration via env vars: `EXPENSES_MANAGEMENT_EXPENSES_RABBITMQ_*`.

### Frontend

React 18 + TypeScript + Vite SPA, styled with **Tailwind CSS v3**. Key files:
- `src/api.ts` — Centralized API client; uses `credentials: 'include'` for cookie auth; supports `skipUnauthorized` option
- `src/auth/AuthContext.tsx` — Cookie-based auth state and context; session restored via `GET /auth/session` on load
- `src/components/ProtectedRoute.tsx` — Route guard
- `src/components/NavBar.tsx` — Auth-aware responsive navigation bar
- `src/index.css` — Tailwind directives + `@layer components` for shared UI primitives (`.field-label`, `.field-input`, `.btn-primary`, `.btn-secondary`, `.auth-page`, `.auth-card`, `.msg-error`, `.msg-success`, `.msg-info`)
- `tailwind.config.ts` — Custom design system: `brand` color scale (indigo), `surface` tokens, custom shadows; Inter font via Google Fonts

Path alias `@` maps to `src/`. The SPA is served through nginx, which rewrites all non-asset paths to `/index.html`.

**Styling conventions:**
- Use Tailwind utility classes; avoid inline styles
- Reusable UI primitives live in `@layer components` in `index.css` — prefer extending those over adding new one-off utility chains
- Brand color: `brand-600` (`#4f46e5`, indigo). Background: `slate-50`. Cards: `bg-white shadow-card border border-slate-200 rounded-2xl`
- Test assertions for styling: use `toHaveClass('...')` not `toHaveStyle(...)` — Tailwind classes don't produce inline styles in jsdom

### Database

- Both services use **PostgreSQL** via `Npgsql.EntityFrameworkCore.PostgreSQL`
- Users service manages schema: `USR_Users`, `ATH_Authentications`, `RLE_Roles`, `APP_Applications`, `RQA_RequestAccesses`, `RRA_RoleRequestAccesses`, `URR_UserRoles`, `ALW_AllowedOrigins`
- Expenses service has its own schema plus read access to `ext.USR_Users`

---

## CI/CD (GitLab CI)

The pipeline is defined in `.gitlab-ci.yml` at the root, with reusable templates in `infrastructure/configs/gitlab-ci-templates/`. Each service has its own `.gitlab-ci.yml`.

**Pipeline stages:** `build → test → quality → docker → security → deploy`

- **Quality gate:** SonarQube analysis blocks on gate failure (`sonar.qualitygate.wait=true`)
- **Security:** Semgrep (SAST), OWASP Dependency Check (SCA), Gitleaks (secrets), Trivy (Docker image scanning)
- **Coverage:** OpenCover format for .NET; V8 for frontend; reports consumed by SonarQube

SonarQube exclusions (no coverage required): `Migrations/`, `Models/`, `Options/`, `EO/`, `Requests/`, `Responses/`, `Program.cs`

---

## Testing Conventions

- **Backend:** xUnit + Moq. Use `TestExpensesDbContextWrapper` (in-memory DB) for repository tests; avoid mocking the DbContext directly.
- **Frontend:** Vitest + React Testing Library. Tests live in `__tests__/` subdirectories next to the code they test. Use `userEvent` (from `@testing-library/user-event`) for interactions; `fireEvent` only when needed. Mock `useAuth` via `vi.mock('@/auth/AuthContext')`. Wrap renders in `<MemoryRouter>` (or `<MemoryRouter initialEntries={[...]}>` + `<Routes>` when testing navigation). Assert Tailwind styling with `toHaveClass()`, not `toHaveStyle()`.

---

## Maintenance

Whenever code is added, removed, or significantly changed, keep the following documentation files up to date:

| File | Purpose |
|---|---|
| [CLAUDE.md](CLAUDE.md) | Claude Code instructions — architecture, commands, testing conventions |
| [FILE-TREE.md](FILE-TREE.md) | Annotated project file tree — update when files/folders are added or removed |
| [CHANGELOG.md](CHANGELOG.md) | Version history — add an entry for every notable change |
| [README.md](README.md) | Main project README — update for infrastructure or setup changes |
| [backend/users/README.md](backend/users/README.md) | Users service API reference |
| [backend/expenses/README.md](backend/expenses/README.md) | Expenses service API reference |
| [qa/](qa/) | QA reports — add a dated report file for each testing session |

---

## File Tree

See [FILE-TREE.md](FILE-TREE.md) for the full annotated project structure.
