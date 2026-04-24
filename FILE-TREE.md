# File Tree

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
├── CLAUDE.md                          — Claude Code instructions
├── FILE-TREE.md                       — Project file tree (this file)
├── LICENSE                            — Project license
├── README.md                          — Main project README
├── docs/
│   └── issues/
│       ├── ongoing/
│       │   ├── fixes-and-suggestions.md          — Open improvement ideas and technical debt backlog
│       │   └── qa_test_results/
│       │       └── 2026-03-22-frontend-dashboard-qa.md  — Frontend dashboard QA (open items only)
│       └── fixed/
│           ├── fixes-and-suggestions-applied.md  — Applied suggestions (moved here from ongoing once shipped)
│           └── qa/
│               └── 2026-03-22-frontend-dashboard-fixes.md  — Resolved issues from the 2026-03-22 QA session
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
│       │       ├── 20260323120000_UpdateApplicationUrls.cs — Updates APP_UrlPath and APP_ResetPasswordUrlPath from localhost:5173 to localhost (nginx)
│       │       ├── 20260412165435_FixResetPasswordUrl.cs — Sets APP_ResetPasswordUrlPath to host-agnostic relative path /reset-password
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
│           ├── App.tsx                — Root component: providers composition
│           ├── router.tsx             — All <Routes> definitions
│           ├── env.d.ts               — Vite env type declarations
│           ├── vitest.d.ts            — Vitest type declarations
│           ├── components/            — Shared reusable UI components
│           │   ├── PasswordInput.tsx   — Reusable password input with show/hide toggle button
│           │   ├── PasswordStrength.tsx — Live password strength indicator (5-segment bar + checklist)
│           │   ├── Toast.tsx           — Toast notification provider and hook
│           │   └── __tests__/
│           │       ├── PasswordInput.test.tsx
│           │       ├── PasswordStrength.test.tsx
│           │       └── Toast.test.tsx
│           ├── features/
│           │   └── auth/              — Authentication feature (context + route guards)
│           │       ├── AuthContext.tsx    — Cookie-based auth state; session restored via GET /auth/session on load
│           │       ├── ProtectedRoute.tsx  — Redirects unauthenticated users to /login
│           │       ├── PublicOnlyRoute.tsx — Redirects authenticated users to /dashboard
│           │       └── __tests__/
│           │           ├── AuthContext.test.tsx
│           │           ├── ProtectedRoute.test.tsx
│           │           └── PublicOnlyRoute.test.tsx
│           ├── hooks/
│           │   └── usePageTitle.ts    — Sets document.title per page
│           ├── layouts/               — Layout-level components
│           │   ├── NavBar.tsx          — Auth-aware nav; desktop + mobile responsive
│           │   └── __tests__/
│           │       └── NavBar.test.tsx
│           ├── pages/
│           │   ├── HomePublic.tsx            — Public landing page
│           │   ├── Login.tsx                 — Login form; redirects to /dashboard on success
│           │   ├── Register.tsx              — Registration form
│           │   ├── HomeDashboard.tsx         — Authenticated dashboard; shows firstName
│           │   ├── ChangePassword.tsx        — Change password form
│           │   ├── RequestPasswordReset.tsx  — Request password reset email
│           │   ├── ResetPassword.tsx         — Reset password with token from email
│           │   ├── Settings.tsx              — Settings hub page; links to sub-sections
│           │   ├── NotFound.tsx              — 404 page; shown for any unmatched route
│           │   └── __tests__/
│           │       ├── HomeDashboard.test.tsx
│           │       ├── Login.test.tsx
│           │       ├── Register.test.tsx
│           │       ├── HomePublic.test.tsx
│           │       ├── ChangePassword.test.tsx
│           │       ├── Settings.test.tsx
│           │       ├── RequestPasswordReset.test.tsx
│           │       ├── ResetPassword.test.tsx
│           │       └── NotFound.test.tsx
│           ├── services/              — API layer
│           │   ├── api.ts             — API client; uses credentials: include for cookie auth; supports skipUnauthorized option
│           │   └── __tests__/
│           │       └── api.test.ts
│           └── styles/
│               └── index.css          — Tailwind directives + @layer components
│
├── infrastructure/
│   ├── .env                           — Local infrastructure env vars (gitignored)
│   ├── .env.example
│   ├── README.md
│   ├── docker-compose-apps.yml        — App stack: nginx, users-service, expenses-service
│   ├── docker-compose-tools.yml       — Tool stack: PostgreSQL, RabbitMQ, Grafana, Prometheus, SonarQube, Mailpit
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
