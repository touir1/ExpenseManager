
# Changelog

All notable changes to this project will be documented in this file.

## [0.13.0] - 2026-03-21
### Added
- Tailwind CSS v3 integrated into the frontend dashboard (PostCSS pipeline, `tailwind.config.ts` with custom design system: indigo brand palette, Inter font, surface tokens, custom shadows).
- Responsive `NavBar` component with auth-aware links and mobile hamburger menu.
- Shared UI primitive classes in `@layer components` (`.field-input`, `.btn-primary`, `.auth-card`, `.msg-error`, etc.) for consistent form and layout styling.
- `GET /auth/check` endpoint in users service for nginx auth subrequest validation — enables nginx to authenticate all inbound requests internally before forwarding to backend services.
- CI/CD deploy job added for the expenses service (completing full pipeline coverage for both backend services).

### Changed
- Full frontend UI redesign: modern minimalist SaaS aesthetic replacing all inline styles with Tailwind utilities.
- Toast notifications redesigned with light-themed semantic colors and SVG icons; added `aria-live` region for accessibility.
- All auth pages (Login, Register, ResetPassword, RequestPasswordReset, ChangePassword) restyled with centered card layout.
- Expenses service Dockerfile refactored for leaner image builds.
- Updated documentation: `README.md` files for backend services corrected from .NET 6 to .NET 8; `frontend/dashboard/README.md` rewritten to reflect current tech stack and structure.

## [0.12.0] - 2026-03-17
### Added
- Custom Docker Image Updater API in Python to replace Watchtower for automatic zero-downtime deployments.
- Health check endpoints to backend services (`users` and `expenses`).
- Unit tests and associated testing commands to the `frontend/dashboard`.

### Changed
- Fixed CI/CD pipelines across the repository and integrated strict SonarQube quality gates.
- Resolved various SCA security scanner vulnerabilities.
- Updated frontend URL routing logic and dashboard paths.

## [0.11.0] - 2026-03-07
### Changed
- Updated infrastructure/README.md and added README files to configs, jobs, scripts, and volumes directories for improved documentation.
- Refactored infrastructure folder structure for clarity and maintainability.
- Fixed SonarQube quality gate and coverage issues in backend services.

## [0.10.0] - 2026-03-05
### Added
- Initial workspace structure documented in README.md.
- Backend services: expenses, users (.NET 8, EF Core).
- Frontend dashboard: React + Vite + TypeScript.
- Docker Compose infrastructure for apps and tools.
- Environment variable-based configuration for secrets.
- SQL scripts for database setup.
- Quick start instructions for backend and frontend.
- Linked RabbitMQ, Grafana, Prometheus management URLs.
- Contributing and license sections.
- Nginx full configuration (virtual host, SSL, mime types, fastcgi/scgi params) and automated deployment via CI/CD.
- Expenses service GitLab CI/CD pipeline (build, test, quality, docker, deploy stages).
- Expenses backend unit tests: external user repository and RabbitMQ service coverage; in-memory test DB helper (`TestExpensesDbContextWrapper`).
- EF Core migrations for expenses service applied automatically at startup via `db.Database.MigrateAsync()`.

## [0.9.0] - 2026-02-15
### Added
- Integrated GitLab CI/CD, SonarQube, and security scanning (SAST, SCA, secrets-scan, docker-security, gitleaks, trivy).
- Added job runner, cron, and supervisor configuration for scheduled/background tasks.
- Added monitoring dashboards: Grafana, Prometheus, RabbitMQ.
- Added frontend dashboard build/test pipeline and unit tests.

### Changed
- Multiple fixes and refactors to CI/CD, SonarQube, and test pipelines.
- Improved Dockerfile and infrastructure scripts for cross-platform compatibility.

## [0.8.0] - 2026-01-31
### Added
- Frontend and backend integration improvements.
- Users backend comprehensive unit test suite: repository tests (User, Application, Authentication, Role), service tests (Application, Authentication, Role), controller tests (AuthenticationController), and infrastructure tests (CryptographyHelper, EmailHelper).
- EO (Entity Object) DTO pattern adopted in users service and controller layers.
- GitLab mirror to GitHub, CRLF/LF normalization, and improved pipeline triggers.

### Changed
- Frontend home page route renamed to `/home-auth` (dashboard).
- Refactored backend and frontend code for maintainability.
- Improved Docker build and security.

## [0.7.0] - 2025-12-31
### Added
- Switched frontend to React (Next.js, then native React, then TailwindCSS).
- Upgraded Angular and improved login/register flows.
- Replaced Dapper with EF in Users backend.

## [0.6.0] - 2024-05-29
### Added
- Users backend: register, verify email, change password, email helper.
- Expenses backend: models, RabbitMQ integration, Postgres config, migrations.

## [0.5.0] - 2024-02-19
### Added
- Initial repository structure, .gitignore, and documentation.
- Added backend and infrastructure README updates.
- Added monitoring dashboards and run scripts.
