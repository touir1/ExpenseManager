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

**Keeping this file up to date:** Whenever code is added, removed, or significantly changed, update this file accordingly — including the file tree below if files are added or removed, and the Architecture/Testing sections if patterns change.

---

## File Tree

Excludes: `node_modules/`, `bin/`, `obj/`, `.git/`, `coverage/`, `dist/`, generated build artifacts.

```
ExpenseManager/
├── .claude/
│   └── settings.local.json            — Claude Code local settings
├── .vscode/
│   ├── extensions.json                — Recommended VS Code extensions
│   └── settings.json                  — VS Code workspace settings
├── .gitignore                         — Root Git ignore patterns
├── .gitlab-ci.yml                     — Root GitLab CI pipeline
├── .gitleaks.toml                     — Gitleaks secret scanning config
├── CHANGELOG.md                       — Version history and release notes
├── CLAUDE.md                          — Claude Code instructions (this file)
├── LICENSE                            — Project license
├── README.md                          — Main project README
├── frontend-dashboard-test-result.md  — Frontend test results summary
│
├── backend/
│   ├── dashboard/
│   │   ├── Dockerfile                 — Docker image for dashboard service
│   │   └── README.md                  — Dashboard service documentation
│   │
│   ├── expenses/
│   │   ├── .dockerignore
│   │   ├── .gitlab-ci.yml             — Expenses service CI/CD pipeline
│   │   ├── .trivyignore               — Trivy scanner ignore list
│   │   ├── Dockerfile                 — Docker image for expenses service
│   │   ├── README.md
│   │   ├── SonarQube.Analysis.xml     — SonarQube project settings
│   │   ├── Touir.ExpensesManager.Expenses.sln
│   │   ├── sql_database_scripts/
│   │   │   ├── create_database.sql
│   │   │   ├── create_tables.sql
│   │   │   └── create_user_and_privileges.sql
│   │   ├── Touir.ExpensesManager.Expenses/
│   │   │   ├── Program.cs             — Entry point, DI registration, migrations
│   │   │   ├── appsettings.json
│   │   │   ├── appsettings.Development.json
│   │   │   ├── Touir.ExpensesManager.Expenses.csproj
│   │   │   ├── Properties/
│   │   │   │   └── launchSettings.json
│   │   │   ├── Infrastructure/
│   │   │   │   ├── ExpensesDbContext.cs     — EF Core context; maps ext.USR_Users
│   │   │   │   └── Options/
│   │   │   │       ├── PostgresOptions.cs
│   │   │   │       └── RabbitMQOptions.cs
│   │   │   ├── Models/
│   │   │   │   ├── Category.cs
│   │   │   │   ├── Currency.cs
│   │   │   │   ├── Expense.cs
│   │   │   │   └── External/
│   │   │   │       └── User.cs              — Read-only mapping of users DB entity
│   │   │   ├── Repositories/
│   │   │   │   └── External/
│   │   │   │       ├── Contracts/
│   │   │   │       │   └── IUserRepository.cs
│   │   │   │       └── UserRepository.cs    — Read-only cross-service user access
│   │   │   ├── Services/
│   │   │   │   ├── Contracts/
│   │   │   │   │   └── IRabbitMQService.cs
│   │   │   │   └── RabbitMQService.cs       — RabbitMQ connection and messaging
│   │   │   └── Migrations/
│   │   │       ├── 20260217225816_InitialCreate.cs
│   │   │       ├── 20260217225816_InitialCreate.Designer.cs
│   │   │       └── ExpensesDbContextModelSnapshot.cs
│   │   └── Touir.ExpensesManager.Expenses.Tests/
│   │       ├── Touir.ExpensesManager.Expenses.Tests.csproj
│   │       ├── TestHelpers/
│   │       │   └── TestExpensesDbContext.cs  — In-memory DB wrapper for tests
│   │       ├── Repositories/External/
│   │       │   └── UserRepositoryTests.cs
│   │       └── Services/
│   │           └── RabbitMQServiceTests.cs
│   │
│   └── users/
│       ├── .config/
│       │   └── dotnet-tools.json
│       ├── .dockerignore
│       ├── .gitignore
│       ├── .gitlab-ci.yml             — Users service CI/CD pipeline
│       ├── .trivyignore
│       ├── Dockerfile
│       ├── README.md
│       ├── SonarQube.Analysis.xml
│       ├── Touir.ExpensesManager.Users.sln
│       ├── sql_database_scripts/
│       │   ├── create_database.sql
│       │   ├── create_tables.sql
│       │   └── create_user_and_privileges.sql
│       ├── Touir.ExpensesManager.Users/
│       │   ├── Program.cs             — Entry point, DI registration, migrations
│       │   ├── appsettings.json
│       │   ├── appsettings.Development.json
│       │   ├── Touir.ExpensesManager.Users.csproj
│       │   ├── Properties/
│       │   │   └── launchSettings.json
│       │   ├── Assets/EmailTemplates/
│       │   │   └── EMAIL_VERIFICATION_TEMPLATE.html
│       │   ├── Infrastructure/
│       │   │   ├── UsersAppDbContext.cs      — EF Core context for users schema
│       │   │   ├── CryptographyHelper.cs     — Password hashing and HMAC utilities
│       │   │   ├── EmailHelper.cs            — SMTP email sending
│       │   │   ├── EmailHTMLTemplate.cs      — HTML email template builder
│       │   │   ├── Contracts/
│       │   │   │   ├── ICryptographyHelper.cs
│       │   │   │   └── IEmailHelper.cs
│       │   │   └── Options/
│       │   │       ├── AuthenticationServiceOptions.cs
│       │   │       ├── CryptographyOptions.cs
│       │   │       ├── EmailOptions.cs
│       │   │       ├── JwtAuthOptions.cs
│       │   │       └── PostgresOptions.cs
│       │   ├── Models/
│       │   │   ├── AllowedOrigin.cs
│       │   │   ├── Application.cs
│       │   │   ├── Authentication.cs
│       │   │   ├── RequestAccess.cs
│       │   │   ├── Role.cs
│       │   │   ├── RoleRequestAccess.cs
│       │   │   ├── User.cs
│       │   │   └── UserRole.cs
│       │   ├── Controllers/
│       │   │   ├── AuthenticationController.cs  — Login (sets HttpOnly cookie), logout, session check, register, change/reset password, auth check
│       │   │   ├── EO/
│       │   │   │   ├── ApplicationEo.cs
│       │   │   │   ├── RoleEo.cs
│       │   │   │   └── UserEo.cs               — User DTO with FirstName, LastName, Email
│       │   │   ├── Requests/
│       │   │   │   ├── ChangePasswordRequest.cs        — Requires Email, OldPassword, NewPassword, ConfirmPassword
│       │   │   │   ├── ChangePasswordResetRequest.cs
│       │   │   │   ├── LoginRequest.cs
│       │   │   │   ├── RegisterRequest.cs
│       │   │   │   └── RequestPasswordResetRequest.cs
│       │   │   └── Responses/
│       │   │       ├── ErrorResponse.cs
│       │   │       ├── LoginResponse.cs        — Returns Token, User (UserEo), Roles
│       │   │       └── RegisterResponse.cs
│       │   ├── Repositories/
│       │   │   ├── ApplicationRepository.cs
│       │   │   ├── AuthenticationRepository.cs
│       │   │   ├── RoleRepository.cs
│       │   │   ├── UserRepository.cs
│       │   │   └── Contracts/
│       │   │       ├── IApplicationRepository.cs
│       │   │       ├── IAuthenticationRepository.cs
│       │   │       ├── IRoleRepository.cs
│       │   │       └── IUserRepository.cs
│       │   ├── Services/
│       │   │   ├── ApplicationService.cs
│       │   │   ├── AuthenticationService.cs    — JWT generation and validation (claims: sub, email, jti); token is delivered as HttpOnly cookie by the controller
│       │   │   ├── RoleService.cs
│       │   │   └── Contracts/
│       │   │       ├── IApplicationService.cs
│       │   │       ├── IAuthenticationService.cs
│       │   │       └── IRoleService.cs
│       │   └── Migrations/
│       │       ├── 20251227165426_InitialCreate.cs
│       │       ├── 20251231180932_SeedInitialData.cs
│       │       ├── 20251231182927_AddAllowedOrigin.cs
│       │       ├── 20260101165439_AddIsDefaultColumnRole.cs
│       │       ├── 20260101171739_SetDefaultRoles.cs
│       │       ├── 20260101174904_SetResetPasswordUrlApplication.cs
│       │       └── UsersAppDbContextModelSnapshot.cs
│       └── Touir.ExpensesManager.Users.Tests/
│           ├── Touir.ExpensesManager.Users.Tests.csproj
│           ├── TestHelpers/
│           │   └── TestDbContextWrapper.cs     — In-memory DB wrapper for tests
│           ├── Controllers/
│           │   └── AuthenticationControllerTests.cs
│           ├── Infrastructure/
│           │   ├── CryptographyHelperTests.cs
│           │   └── EmailHelperTests.cs
│           ├── Repositories/
│           │   ├── ApplicationRepositoryTests.cs
│           │   ├── AuthenticationRepositoryTests.cs
│           │   ├── RoleRepositoryTests.cs
│           │   └── UserRepositoryTests.cs
│           └── Services/
│               ├── ApplicationServiceTests.cs
│               ├── AuthenticationServiceTests.cs
│               └── RoleServiceTests.cs
│
├── frontend/
│   └── dashboard/
│       ├── .env                       — Local env vars (gitignored)
│       ├── .env.example               — Env vars template
│       ├── .gitignore
│       ├── .gitlab-ci.yml             — Frontend CI/CD pipeline
│       ├── README.md
│       ├── index.html                 — HTML entry point
│       ├── package.json
│       ├── package-lock.json
│       ├── postcss.config.cjs         — PostCSS pipeline for Tailwind
│       ├── setupTests.ts              — Vitest global test setup
│       ├── sonar-project.properties   — SonarQube project settings
│       ├── tailwind.config.ts         — Custom design system tokens
│       ├── tsconfig.json
│       ├── tsconfig.app.json
│       ├── tsconfig.node.json
│       ├── vite.config.ts             — Vite bundler config with @ alias
│       └── vitest.config.ts           — Vitest test runner config
│       └── src/
│           ├── main.tsx               — React app mount point
│           ├── App.tsx                — Root component: router, providers, routes
│           ├── index.css              — Tailwind directives + @layer components
│           ├── env.d.ts               — Vite env type declarations
│           ├── api.ts                 — Axios-based API client; error/auth handlers
│           ├── auth/
│           │   ├── AuthContext.tsx    — JWT state, login/logout/register/changePassword
│           │   └── __tests__/
│           │       └── AuthContext.test.tsx
│           ├── components/
│           │   ├── NavBar.tsx         — Auth-aware nav; desktop + mobile responsive
│           │   ├── ProtectedRoute.tsx — Redirects unauthenticated users to /login
│           │   ├── PublicOnlyRoute.tsx — Redirects authenticated users to /home
│           │   ├── Toast.tsx          — Toast notification provider and hook
│           │   └── __tests__/
│           │       ├── NavBar.test.tsx
│           │       ├── ProtectedRoute.test.tsx
│           │       ├── PublicOnlyRoute.test.tsx
│           │       └── Toast.test.tsx
│           └── pages/
│               ├── HomePublic.tsx            — Public landing page
│               ├── Login.tsx                 — Login form; redirects to /home on success
│               ├── Register.tsx              — Registration form
│               ├── HomeDashboard.tsx         — Authenticated dashboard; shows firstName
│               ├── ChangePassword.tsx        — Change password form
│               ├── RequestPasswordReset.tsx  — Request password reset email
│               ├── ResetPassword.tsx         — Reset password with token from email
│               └── __tests__/
│                   ├── HomeDashboard.test.tsx
│                   ├── Login.test.tsx
│                   ├── Register.test.tsx
│                   ├── HomePublic.test.tsx
│                   ├── ChangePassword.test.tsx
│                   ├── RequestPasswordReset.test.tsx
│                   └── ResetPassword.test.tsx
│
├── infrastructure/
│   ├── .env                           — Local infrastructure env vars (gitignored)
│   ├── .env.example
│   ├── README.md
│   ├── docker-compose-apps.yml        — App stack: nginx, users-service, expenses-service
│   ├── docker-compose-tools.yml       — Tool stack: PostgreSQL, RabbitMQ, Grafana, Prometheus, SonarQube
│   ├── run-docker-compose-apps.bat    — Start app containers (Windows)
│   ├── run-docker-compose-tools.bat   — Start tool containers (Windows)
│   ├── start-expenses-manager-apps.bat
│   ├── configs/
│   │   ├── gitlab-ci-templates/       — Reusable CI/CD job templates
│   │   │   ├── ci-init.yml
│   │   │   ├── ci-stages.yml
│   │   │   ├── ci-build.yml
│   │   │   ├── ci-test.yml
│   │   │   ├── ci-quality.yml
│   │   │   ├── ci-docker.yml
│   │   │   ├── ci-docker-security.yml
│   │   │   ├── ci-security.yml
│   │   │   └── ci-deploy.yml
│   │   ├── nginx/
│   │   │   ├── nginx.conf             — Main nginx config
│   │   │   ├── fastcgi_params / scgi_params / uwsgi_params / mime.types
│   │   │   ├── entrypoint.sh          — Docker entrypoint; injects env vars into config
│   │   │   ├── sites-available/
│   │   │   │   └── expenses-manager.conf  — Route mapping and auth subrequest config
│   │   │   └── ssl/
│   │   │       ├── ssl.conf
│   │   │       ├── expenses-manager.crt
│   │   │       └── expenses-manager.key
│   │   ├── prometheus/
│   │   │   └── prometheus.yml         — Scrape targets config
│   │   ├── rabbitmq/
│   │   │   ├── rabbitmq.conf
│   │   │   ├── enabled_plugins
│   │   │   └── management_definitions.json
│   │   └── sql_database_scripts/
│   │       ├── gitlab/
│   │       │   ├── create_database.sql
│   │       │   └── create_user_and_privileges.sql
│   │       └── sonar/
│   │           ├── create_database.sql
│   │           └── create_user_and_privileges.sql
│   ├── jobs/
│   │   ├── Dockerfile
│   │   ├── crontab                    — Scheduled job definitions
│   │   ├── supervisord.conf           — Supervisor daemon config
│   │   ├── configs/                   — Per-job configuration files
│   │   └── scripts/
│   │       ├── cron-runner.sh
│   │       ├── docker-updater-runner.sh
│   │       ├── docker_image_updater_api.py  — Zero-downtime Docker image update API
│   │       ├── github-gitlab-sync.sh
│   │       ├── minio-sync.sh
│   │       └── registry_proxy.py      — Docker registry proxy
│   └── scripts/
│       └── gitlab-register-runner.sh  — GitLab Runner registration helper
│
└── mobile/
    └── README.md                      — Mobile app placeholder
```
