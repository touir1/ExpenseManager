# File Tree

Excludes: `node_modules/`, `bin/`, `obj/`, `.git/`, `coverage/`, `dist/`, generated build artifacts.

```
ExpenseManager/
├── .claude/
│   ├── commands/
│   │   ├── cicd.md                    — `/cicd` skill definition
│   │   ├── done.md                    — `/done` skill definition
│   │   ├── qa.md                      — `/qa` skill definition
│   │   └── test.md                    — `/test` skill definition
│   ├── cicd.md                        — CI/CD skill reference
│   ├── commands.md                    — All shell commands (imported by CLAUDE.md)
│   ├── constraints.md                 — Non-obvious architectural constraints (imported by CLAUDE.md)
│   ├── maintenance.md                 — Doc update table (imported by CLAUDE.md)
│   └── settings.local.json            — Claude Code local settings (git-ignored)
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
│   ├── issues/
│   │   ├── ongoing/
│   │   │   ├── fixes-and-suggestions.md          — Open improvement ideas and technical debt backlog
│   │   │   └── qa_test_results/
│   │   │       └── 2026-03-22-frontend-dashboard-qa.md  — Frontend dashboard QA (open items only)
│   │   └── fixed/
│   │       ├── fixes-and-suggestions-applied.md  — Applied suggestions (moved here from ongoing once shipped)
│   │       └── qa/
│   │           └── 2026-03-22-frontend-dashboard-fixes.md  — Resolved issues from the 2026-03-22 QA session
│   └── plans/
│       ├── application-description.md  — Full product specification (roles, families, audit, rate resolution, all screens)
│       └── implementation-plan.md      — 15-phase implementation plan
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
│   │   │   │   ├── ExpensesDbContext.cs     — EF Core context; all 13 DbSets with full Fluent API config
│   │   │   │   └── Options/
│   │   │   │       ├── PostgresOptions.cs
│   │   │   │       └── RabbitMQOptions.cs
│   │   │   ├── Controllers/
│   │   │   │   └── Responses/
│   │   │   │       └── ErrorResponse.cs     — Uniform error envelope (matches users service pattern)
│   │   │   ├── Models/
│   │   │   │   ├── Category.cs              — Updated: IsArchived, ParentCategoryId, Children
│   │   │   │   ├── Currency.cs
│   │   │   │   ├── Expense.cs               — Rewritten: owner, amount, date, category, audit fields; FK int columns
│   │   │   │   ├── Family.cs
│   │   │   │   ├── FamilyMembership.cs      — RoleId (int FK) instead of enum
│   │   │   │   ├── ExpenseFamilyAttribution.cs
│   │   │   │   ├── Tag.cs
│   │   │   │   ├── ExpenseTag.cs
│   │   │   │   ├── CurrencyDailyRate.cs     — RateSourceId (int FK) instead of enum
│   │   │   │   ├── CurrencyPairDefault.cs
│   │   │   │   ├── CurrencyRateConflict.cs  — StatusId/ResolutionId (int FK) instead of enums
│   │   │   │   ├── ExpenseAuditLog.cs       — OperationId/PerformedFromId (int FK) instead of enums
│   │   │   │   ├── ExpenseAuditSnapshot.cs  — SnapshotTypeId (int FK) instead of enum
│   │   │   │   ├── Lookups/
│   │   │   │   │   ├── ILookupEntity.cs     — Common interface: Id, Name
│   │   │   │   │   ├── OperationSource.cs   — 1=SingleWeb, 2=SingleMobile, 3=BulkWeb
│   │   │   │   │   ├── ModifiedSource.cs    — 1=Web, 2=Mobile
│   │   │   │   │   ├── FamilyRole.cs        — 1=Head, 2=Member
│   │   │   │   │   ├── RateSource.cs        — 1=Auto, 2=Manual
│   │   │   │   │   ├── ConflictStatus.cs    — 1=Pending, 2=Resolved
│   │   │   │   │   ├── ConflictResolution.cs — 1=AcceptAuto, 2=KeepManual, 3=Custom
│   │   │   │   │   ├── AuditOperation.cs    — 1=Add, 2=Update, 3=Delete
│   │   │   │   │   └── SnapshotType.cs      — 1=Before, 2=After
│   │   │   │   └── External/
│   │   │   │       └── User.cs              — Read-only mapping of users DB entity
│   │   │   ├── Repositories/
│   │   │   │   └── External/
│   │   │   │       ├── Contracts/
│   │   │   │       │   └── IUserRepository.cs
│   │   │   │       └── UserRepository.cs    — Read-only cross-service user access
│   │   │   ├── Services/
│   │   │   │   ├── Contracts/
│   │   │   │   │   ├── IRabbitMQService.cs
│   │   │   │   │   └── ILookupCacheService.cs — GetIdAsync<T>(name) / GetNameAsync<T>(id)
│   │   │   │   ├── RabbitMQService.cs       — RabbitMQ connection and messaging
│   │   │   │   └── LookupCacheService.cs    — IMemoryCache-backed lookup; NeverRemove priority; loads entire table on first access
│   │   │   └── Migrations/
│   │   │       ├── 20260217225816_InitialCreate.cs
│   │   │       ├── 20260217225816_InitialCreate.Designer.cs
│   │   │       ├── 20260505144048_SchemaFoundation.cs   — Phase 1: all domain + 8 lookup tables with seed data
│   │   │       ├── 20260505144048_SchemaFoundation.Designer.cs
│   │   │       ├── 20260505145359_LongIdsForExpenseAndAudit.cs   — bigint PKs/FKs for Expense, AuditLog, AuditSnapshot, FamilyAttribution
│   │   │       ├── 20260505145359_LongIdsForExpenseAndAudit.Designer.cs
│   │   │       └── ExpensesDbContextModelSnapshot.cs
│   │   └── Touir.ExpensesManager.Expenses.Tests/
│   │       ├── Touir.ExpensesManager.Expenses.Tests.csproj
│   │       ├── TestHelpers/
│   │       │   └── TestExpensesDbContext.cs  — In-memory DB wrapper for tests
│   │       ├── Repositories/External/
│   │       │   └── UserRepositoryTests.cs
│   │       ├── Infrastructure/
│   │       │   └── ExpensesDbContextSchemaTests.cs  — 23 tests: all Phase 1 entities, composite PKs, unique constraints, cascades
│   │       └── Services/
│   │           ├── RabbitMQServiceTests.cs
│   │           └── LookupCacheServiceTests.cs       — 7 tests: GetId/Name, KeyNotFoundException, cache hit, all 8 types
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
│       │   │   ├── EMAIL_VERIFICATION_TEMPLATE.html
│       │   │   └── PASSWORD_RESET_TEMPLATE.html
│       │   ├── Infrastructure/
│       │   │   ├── UsersAppDbContext.cs      — EF Core context for users schema
│       │   │   ├── CryptographyHelper.cs     — Password hashing and HMAC utilities
│       │   │   ├── EmailHelper.cs            — Email helper: validation, template loading; delegates send to IEmailService
│       │   │   ├── SmtpEmailService.cs       — IEmailService implementation using System.Net.Mail SMTP
│       │   │   ├── EmailHtmlTemplate.cs      — HTML email template keys and variable name constants
│       │   │   ├── Contracts/
│       │   │   │   ├── ICryptographyHelper.cs
│       │   │   │   ├── IEmailHelper.cs
│       │   │   │   └── IEmailService.cs      — Abstraction for email dispatch (OCP boundary)
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
│       │   │   ├── RefreshToken.cs              — Opaque refresh token entity (RTK_RefreshTokens)
│       │   │   ├── RequestAccess.cs
│       │   │   ├── Role.cs
│       │   │   ├── RoleRequestAccess.cs
│       │   │   ├── User.cs
│       │   │   └── UserRole.cs
│       │   ├── Controllers/
│       │   │   ├── AuthenticationController.cs  — Login, logout, session, refresh, auth check (token ops via IJwtTokenService)
│       │   │   ├── RegistrationController.cs    — Register, validate-email
│       │   │   ├── PasswordController.cs        — Change-password, request-password-reset, change-password-reset
│       │   │   ├── DTO/
│       │   │   │   ├── ApplicationDto.cs
│       │   │   │   ├── RoleDto.cs
│       │   │   │   └── UserDto.cs              — User DTO with FirstName, LastName, Email
│       │   │   ├── Requests/
│       │   │   │   ├── ChangePasswordRequest.cs        — Requires Email, OldPassword, NewPassword
│       │   │   │   ├── ChangePasswordResetRequest.cs
│       │   │   │   ├── CreatePasswordRequest.cs
│       │   │   │   ├── LoginRequest.cs          — Email, Password, ApplicationCode, RememberMe
│       │   │   │   ├── RegisterRequest.cs
│       │   │   │   └── RequestPasswordResetRequest.cs
│       │   │   └── Responses/
│       │   │       ├── ErrorResponse.cs
│       │   │       ├── LoginResponse.cs        — Returns User (UserDto) and Roles (token is cookie-only)
│       │   │       ├── SessionResponse.cs      — Returns Email, FirstName, LastName from JWT claims
│       │   │       └── RegisterResponse.cs
│       │   ├── Validators/
│       │   │   ├── LoginRequestValidator.cs             — ApplicationCode, Email, Password NotEmpty → MISSING_PARAMETERS
│       │   │   ├── RegisterRequestValidator.cs          — FirstName, LastName, Email NotEmpty → MISSING_PARAMETERS
│       │   │   ├── ChangePasswordRequestValidator.cs    — Email, OldPassword NotEmpty; NewPassword NotEmpty + MinLength(8) → PASSWORD_TOO_SHORT
│       │   │   ├── ChangePasswordResetRequestValidator.cs — Email, VerificationHash NotEmpty; NewPassword NotEmpty + MinLength(8)
│       │   │   ├── CreatePasswordRequestValidator.cs    — Email, VerificationHash NotEmpty; NewPassword NotEmpty + MinLength(8)
│       │   │   └── RequestPasswordResetRequestValidator.cs — Email, AppCode NotEmpty → MISSING_PARAMETERS
│       │   ├── Repositories/
│       │   │   ├── ApplicationRepository.cs
│       │   │   ├── AuthenticationRepository.cs
│       │   │   ├── RefreshTokenRepository.cs    — CRUD for RTK_RefreshTokens
│       │   │   ├── RoleRepository.cs
│       │   │   ├── UserRepository.cs
│       │   │   └── Contracts/
│       │   │       ├── IApplicationRepository.cs
│       │   │       ├── IAuthenticationRepository.cs
│       │   │       ├── IRefreshTokenRepository.cs
│       │   │       ├── IRoleRepository.cs
│       │   │       └── IUserRepository.cs
│       │   ├── Services/
│       │   │   ├── ApplicationService.cs
│       │   │   ├── AuthenticationService.cs        — Credential verification only (AuthenticateAsync)
│       │   │   ├── JwtTokenService.cs              — JWT generation (claims: sub, email, givenName, surname, jti) and validation
│       │   │   ├── RegistrationService.cs          — User registration and email validation
│       │   │   ├── PasswordManagementService.cs    — Change password, reset password, request password reset
│       │   │   ├── RefreshTokenService.cs          — Generates and validates opaque refresh tokens (DB-backed)
│       │   │   ├── RoleService.cs
│       │   │   ├── UserRoleAssignmentService.cs    — Assigns default application role to a newly registered user
│       │   │   └── Contracts/
│       │   │       ├── IApplicationService.cs
│       │   │       ├── IAuthenticationService.cs
│       │   │       ├── IJwtTokenService.cs
│       │   │       ├── IPasswordManagementService.cs
│       │   │       ├── IRefreshTokenService.cs
│       │   │       ├── IRegistrationService.cs
│       │   │       ├── IRoleService.cs
│       │   │       └── IUserRoleAssignmentService.cs
│       │   └── Migrations/
│       │       ├── 20251227165426_InitialCreate.cs
│       │       ├── 20251231180932_SeedInitialData.cs
│       │       ├── 20251231182927_AddAllowedOrigin.cs
│       │       ├── 20260101165439_AddIsDefaultColumnRole.cs
│       │       ├── 20260101171739_SetDefaultRoles.cs
│       │       ├── 20260101174904_SetResetPasswordUrlApplication.cs
│       │       ├── 20260323120000_UpdateApplicationUrls.cs — Updates APP_UrlPath and APP_ResetPasswordUrlPath from localhost:5173 to localhost (nginx)
│       │       ├── 20260412165435_FixResetPasswordUrl.cs — Sets APP_ResetPasswordUrlPath to host-agnostic relative path /reset-password
│       │       ├── AddRefreshTokens.cs          — Creates RTK_RefreshTokens table
│       │       ├── 20260429200824_AddVerifyEmailErrorUrlPath.cs — Adds APP_VerifyEmailErrorUrlPath; seeds /verify-error for EXPENSES_MANAGER app
│       │       └── UsersAppDbContextModelSnapshot.cs
│       └── Touir.ExpensesManager.Users.Tests/
│           ├── Touir.ExpensesManager.Users.Tests.csproj
│           ├── TestHelpers/
│           │   └── TestDbContextWrapper.cs     — In-memory DB wrapper for tests
│           ├── Controllers/
│           │   ├── AuthenticationControllerTests.cs
│           │   ├── PasswordControllerTests.cs
│           │   └── RegistrationControllerTests.cs
│           ├── Validators/
│           │   ├── LoginRequestValidatorTests.cs
│           │   ├── RegisterRequestValidatorTests.cs
│           │   ├── ChangePasswordRequestValidatorTests.cs
│           │   ├── ChangePasswordResetRequestValidatorTests.cs
│           │   ├── CreatePasswordRequestValidatorTests.cs
│           │   └── RequestPasswordResetRequestValidatorTests.cs
│           ├── Infrastructure/
│           │   ├── CryptographyHelperTests.cs
│           │   ├── EmailHelperTests.cs
│           │   └── SmtpEmailServiceTests.cs
│           ├── Repositories/
│           │   ├── ApplicationRepositoryTests.cs
│           │   ├── AuthenticationRepositoryTests.cs
│           │   ├── RefreshTokenRepositoryTests.cs
│           │   ├── RoleRepositoryTests.cs
│           │   └── UserRepositoryTests.cs
│           └── Services/
│               ├── ApplicationServiceTests.cs
│               ├── AuthenticationServiceTests.cs
│               ├── JwtTokenServiceTests.cs
│               ├── PasswordManagementServiceTests.cs
│               ├── RefreshTokenServiceTests.cs
│               ├── RegistrationServiceTests.cs
│               ├── RoleServiceTests.cs
│               └── UserRoleAssignmentServiceTests.cs
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
│           ├── components/            — Shared UI primitives (generic, cross-feature)
│           │   ├── BackLink.tsx        — Back-arrow link with chevron SVG
│           │   ├── FieldError.tsx      — Per-field error paragraph with role="alert"
│           │   ├── LanguageSwitcher.tsx — Language selector dropdown wired to i18n.changeLanguage
│           │   ├── PasswordInput.tsx   — Password input with show/hide toggle
│           │   ├── PasswordStrength.tsx — Live password strength indicator (5-segment bar + checklist)
│           │   ├── SubmitButton.tsx    — Submit button with spinner SVG and configurable labels
│           │   ├── Toast.tsx           — Toast notification provider and hook
│           │   └── __tests__/
│           │       ├── LanguageSwitcher.test.tsx
│           │       ├── PasswordInput.test.tsx
│           │       ├── PasswordStrength.test.tsx
│           │       └── Toast.test.tsx
│           ├── i18n/                  — Internationalisation (react-i18next)
│           │   ├── index.ts           — i18next singleton config; language detection via localStorage → navigator
│           │   ├── locales/
│           │   │   ├── en/translation.json
│           │   │   ├── fr/translation.json
│           │   │   ├── es/translation.json
│           │   │   └── de/translation.json
│           │   └── __tests__/
│           │       └── i18n.test.ts   — Verifies supported languages, fallback, resources, interpolation config
│           ├── constants/             — App-wide typed constants (used by shared services)
│           │   └── apiErrors.constant.ts — API_ERRORS + BACKEND_ERROR_CODES
│           ├── features/
│           │   ├── auth/              — Authentication feature
│           │   │   ├── components/
│           │   │   │   ├── AuthCard.tsx         — Wraps auth pages in auth-page/auth-card divs
│           │   │   │   ├── AuthPageHeader.tsx   — Page title + subtitle header
│           │   │   │   ├── EmailField.tsx       — Shared email input field for auth forms
│           │   │   │   ├── ProtectedRoute.tsx   — Redirects unauthenticated users to /login
│           │   │   │   └── PublicOnlyRoute.tsx  — Redirects authenticated users to /dashboard
│           │   │   ├── pages/
│           │   │   │   ├── LoginPage.tsx
│           │   │   │   ├── RegisterPage.tsx
│           │   │   │   ├── ChangePasswordPage.tsx
│           │   │   │   ├── ResetPasswordPage.tsx
│           │   │   │   ├── RequestPasswordResetPage.tsx
│           │   │   │   └── __tests__/
│           │   │   │       ├── LoginPage.test.tsx
│           │   │   │       ├── RegisterPage.test.tsx
│           │   │   │       ├── ChangePasswordPage.test.tsx
│           │   │   │       ├── ResetPasswordPage.test.tsx
│           │   │   │       └── RequestPasswordResetPage.test.tsx
│           │   │   ├── services/
│           │   │   │   └── authApi.service.ts   — Auth HTTP functions (login, logout, register, create/change/reset password)
│           │   │   ├── types/
│           │   │   │   └── auth.type.ts         — User, AuthResult, AuthContextValue
│           │   │   ├── AuthContext.tsx           — Cookie-based auth state; session restored via GET /auth/session (falls back to POST /auth/refresh); no localStorage/sessionStorage
│           │   │   ├── auth.schemas.ts           — Zod schemas and inferred types for all five auth forms
│           │   │   └── __tests__/
│           │   │       ├── AuthContext.test.tsx
│           │   │       ├── ProtectedRoute.test.tsx
│           │   │       └── PublicOnlyRoute.test.tsx
│           │   ├── dashboard/         — Authenticated dashboard feature
│           │   │   └── pages/
│           │   │       ├── HomeDashboardPage.tsx — Dashboard home; shows user greeting and cards
│           │   │       ├── SettingsPage.tsx       — Settings hub; links to sub-sections
│           │   │       └── __tests__/
│           │   │           ├── HomeDashboardPage.test.tsx
│           │   │           └── SettingsPage.test.tsx
│           │   └── public/            — Public (unauthenticated) pages
│           │       └── pages/
│           │           ├── HomePublicPage.tsx    — Public landing page
│           │           ├── NotFoundPage.tsx      — 404 page; shown for any unmatched route
│           │           ├── VerifyErrorPage.tsx   — Friendly error page for expired/used email verification links (/verify-error)
│           │           └── __tests__/
│           │               ├── HomePublicPage.test.tsx
│           │               └── NotFoundPage.test.tsx
│           ├── hooks/                 — Shared hooks
│           │   └── usePageTitle.ts    — Sets document.title per page
│           ├── layouts/               — App-wide layout components
│           │   ├── NavBar.tsx          — Auth-aware nav; desktop + mobile responsive
│           │   └── __tests__/
│           │       └── NavBar.test.tsx
│           ├── services/              — Shared base services
│           │   ├── api.service.ts     — Base fetch wrapper with cookie auth, transparent refresh-and-retry on 401, and skipUnauthorized option
│           │   └── __tests__/
│           │       ├── api.service.test.ts     — Full coverage: normal flows, 401 refresh-and-retry, deduplication, skipUnauthorized, network errors
│           │       ├── api.service.env.test.ts — Isolated env tests: VITE_API_BASE prefix and trailing-slash strip
│           │       └── api.test.ts
│           ├── styles/
│           │   └── index.css          — Tailwind directives + @layer components
│           └── types/                 — Shared TypeScript type definitions
│               └── api.type.ts         — ApiResponse<T>
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
