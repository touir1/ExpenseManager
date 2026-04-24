
# Changelog

All notable changes to this project will be documented in this file.

## [0.38.0] - 2026-04-24
### Refactor
- Frontend dashboard: reorganized `src/` folder structure to align with React 2025 conventions. Moved `auth/AuthContext.tsx` + route guards (`ProtectedRoute`, `PublicOnlyRoute`) into `features/auth/`; `NavBar` into `layouts/`; `api.ts` into `services/`; `index.css` into `styles/`. Extracted all `<Routes>` from `App.tsx` into a new `router.tsx`. `components/` now holds only shared reusable UI (PasswordInput, PasswordStrength, Toast). All imports updated; 205 tests continue to pass.

## [0.37.0] - 2026-04-23
### Fixed
- Per-page browser tab titles: added `usePageTitle` hook (`src/hooks/usePageTitle.ts`) and applied it to all 9 page components. Tab titles now read "Login — Expenses Manager", "Dashboard — Expenses Manager", etc. The landing page keeps "Expenses Manager". The Reset Password page switches between "Reset Password" and "Create Password" based on the `?mode=create` query param. (QA #22)

## [0.36.0] - 2026-04-23
### Fixed
- `Login.tsx`: added `submitting` state — the Login button is now disabled and shows a spinner with "Signing in…" while the request is in flight, matching the UX pattern already present in `RequestPasswordReset.tsx`. Email and password inputs are also disabled during submission.
- `Register.tsx`: same fix — the Register button is now disabled and shows a spinner with "Submitting…" during registration. All three inputs are disabled while the request is pending.

## [0.35.0] - 2026-04-23
### Security
- Expenses service `Dockerfile`: switched runtime base image from `mcr.microsoft.com/dotnet/aspnet:8.0` (Debian 12) to `mcr.microsoft.com/dotnet/aspnet:8.0-noble-chiseled` (Ubuntu 24.04, chiseled), matching the users service. Removes the package surface carrying OS-level HIGH Trivy alerts and eliminates the explicit `USER app` directive (chiseled runs non-root by default).

## [0.34.0] - 2026-04-23
### Security
- Users service `Dockerfile`: switched runtime base image from `mcr.microsoft.com/dotnet/aspnet:8.0` (Debian 12) to `mcr.microsoft.com/dotnet/aspnet:8.0-noble-chiseled` (Ubuntu 24.04, chiseled). The chiseled variant contains only the files required to run the .NET runtime — no shell, no package manager, no systemd, no ncurses — eliminating the package surface that carried the HIGH Trivy alerts (`CVE-2026-0861`, `CVE-2026-29111`, `CVE-2025-69720`). The image also runs non-root by default, making the explicit `USER app` directive redundant (removed).

## [0.33.0] - 2026-04-23
### Security
- Users service: pinned `Microsoft.Bcl.Memory` to `9.0.14` in both the main and test projects to remediate CVE-2026-26127 (HIGH — .NET denial of service via out-of-bounds read). The package was a transitive dependency pulled in at a vulnerable version; the explicit reference overrides it with the patched release.

## [0.32.0] - 2026-04-22
### Fixed
- CI Docker template (`ci-docker.yml`): added `docker builder prune -af` after each image push to reclaim build cache disk space on the CI runner.
- Docker-in-Docker (`docker-compose-tools.yml`): switched the DinD storage driver from `vfs` to `overlay2` for improved build performance and reduced disk usage.

## [0.31.0] - 2026-04-21
### Added
- `index.html`: added SEO and Open Graph meta tags — `<meta name="description">`, `<meta name="robots">`, `og:type`, `og:title`, `og:description`, `og:image`, and Twitter Card equivalents (QA #23).

## [0.30.0] - 2026-04-19
### Fixed
- `AuthProvider`, `ToastProvider`, and `PasswordInput` component props marked as `Readonly<>` (SonarQube: mark component props read-only).
- `Toast.tsx` `show` function: extracted inner `setToasts` filter callback to a named `removeById` variable, reducing function nesting depth from 5 to 4 (SonarQube: do not nest functions more than 4 levels deep).

## [0.29.0] - 2026-04-19
### Added
- `PasswordInput` component (`src/components/PasswordInput.tsx`): reusable password field with an eye-icon show/hide toggle. Clicking the button toggles `type` between `"password"` and `"text"` and updates the `aria-label` ("Show password" / "Hide password"). All six password inputs across `Login.tsx`, `ChangePassword.tsx`, and `ResetPassword.tsx` now use this component (QA #19).

## [0.28.0] - 2026-04-19
### Fixed
- Password inputs: replaced `placeholder="••••••••"` with `placeholder=""` on all six password fields across `Login.tsx`, `ChangePassword.tsx`, and `ResetPassword.tsx`. The Unicode bullet workaround displayed inconsistently across browsers; `type="password"` already handles masking (QA #18).

## [0.27.0] - 2026-04-19
### Fixed
- Landing page (`/`): hero content is now vertically centred in the viewport. `HomePublic` uses the `.auth-page` shared class (`flex-1 flex items-center justify-center`) which centres the content within the full remaining viewport height below the navbar (QA #17).

## [0.26.0] - 2026-04-19
### Fixed
- NavBar: corrected the mobile menu Settings link from `/change-password` to `/settings`, fixing a regression introduced during QA #13 where mobile users were routed to the wrong page (QA #16).

## [0.25.0] - 2026-04-19
### Added
- `PasswordStrength` component (`src/components/PasswordStrength.tsx`): live password strength indicator with a 5-segment colour bar (Weak → Fair → Good → Strong) and a checklist of five criteria — at least 8 characters, uppercase letter, lowercase letter, number, and special character. Displayed below the "New password" field on Change Password and Reset Password pages (QA #29).
### Fixed
- Change Password and Reset Password pages: added client-side minimum-length validation; passwords shorter than 8 characters are now rejected with "Password must be at least 8 characters." before the API is called (QA #29).

## [0.24.0] - 2026-04-19
### Fixed
- Request Password Reset page: added "← Back to login" link below the form, giving users a clear navigation path back without relying on the browser back button or the logo (QA #14).

## [0.23.0] - 2026-04-15
### Added
- `Settings` page (`src/pages/Settings.tsx`) at route `/settings`: a proper settings hub with a "Password" card linking to `/change-password`. The navbar "Settings" link now points to `/settings`; the active state covers both `/settings` and `/change-password` (QA #13).
### Fixed
- Change Password page: added "← Back to settings" link at the top, returning users to `/settings` (QA #15).

## [0.22.0] - 2026-04-15
### Fixed
- Dashboard: removed the redundant "Logout" button from the page body. Sign out is now available exclusively via the navbar "Sign out" link, eliminating the duplicate action and the inconsistent redirect destinations (QA #12).

## [0.21.0] - 2026-04-14
### Fixed
- Register page: replaced auto-redirect after successful registration with a dedicated success state. The form is now replaced by a full-card success message and a manual "Go to login →" link — users control when to proceed and the message is always fully visible (QA #10).
- Register page: corrected misleading success message. Registration creates an unvalidated account with no password; the user must click the verification link in their inbox (which redirects to `/reset-password`) before they can log in. The new message tells the user exactly what to do next.

## [0.20.0] - 2026-04-12
### Added
- `NotFound` page (`src/pages/NotFound.tsx`): dedicated 404 component with a "Go to home" link, replacing the silent fallback that rendered the public landing page for unknown routes (QA #8). The `*` catch-all route in `App.tsx` now uses `<NotFound />`.

## [0.19.0] - 2026-04-12
### Fixed
- `/reset-password` and `/request-password-reset` routes are now wrapped in `PublicOnlyRoute` — authenticated users are redirected to `/dashboard` instead of seeing the unauthenticated form (QA #9).
- Reset Password page: on successful reset, the form fields are cleared, the submit button is disabled, and the user is redirected to `/` after 3 seconds so the success message is readable.

## [0.18.0] - 2026-04-12
### Fixed
- Email validation redirect now correctly lands on the frontend `/reset-password` page. Root cause: `ApplicationService.GetApplicationByCodeAsync` mapped the `Application` entity to `ApplicationEo` but omitted `ResetPasswordUrlPath`, so it was always `null` and the redirect became a bare `?email=...` relative URL. Fixed by adding `ResetPasswordUrlPath = app.ResetPasswordUrlPath` to the mapping in `ApplicationService.cs`.
- `APP_ResetPasswordUrlPath` changed from absolute URL (`https://localhost/reset-password`) to a host-agnostic relative path (`/reset-password`) via `FixResetPasswordUrl` migration — the browser resolves it against the request origin automatically, removing any environment-specific host from the database.
- Email address in the validate-email redirect URL is now percent-encoded via `Uri.EscapeDataString` so `@` is safely transmitted as `%40` in the `Location` header (`AuthenticationController.cs`).

## [0.17.0] - 2026-04-12
### Fixed
- Route `/dashboard` now works for authenticated users: renamed the authenticated route from `/home` to `/dashboard` throughout the frontend (`App.tsx`, `NavBar.tsx`, `PublicOnlyRoute.tsx`, `Login.tsx`). All redirect targets, logo links, and nav links updated. Resolves QA Bug #7 and naming inconsistency #30.

## [0.16.0] - 2026-03-23
### Fixed
- Change Password and Reset Password pages now show distinct validation error messages: "All fields are required." when any field is empty, and "New passwords do not match." when the new password fields differ (Bug #6). API failures show "Incorrect current password." / "Password reset failed. Please try again." respectively. The API is no longer called when client-side validation fails.
- Trivy Docker image reference updated from `aquasec/trivy:latest` (removed from Docker Hub) to `ghcr.io/aquasecurity/trivy:latest` in `ci-docker-security.yml`; fixes `docker-security` pipeline stage failing with manifest unknown error.
- Mailpit `MP_SMTP_AUTH_ACCEPT_ANY` and `MP_SMTP_AUTH_ALLOW_INSECURE` env vars added to `docker-compose-tools.yml`; fixes `5.7.0 Authentication Required` SMTP error that silently swallowed registration emails.
- `EXPENSES_MANAGEMENT_USERS_AUTHSERVICE_VERIFY_EMAIL_URL` corrected in `.env` from `localhost:9100/api/auth/validate-email` to `localhost/api/users/auth/validate-email` (nginx route, not direct service port).
- `APP_UrlPath` and `APP_ResetPasswordUrlPath` for the `EXPENSES_MANAGER` application updated from `localhost:5173` (Vite dev server) to `localhost` (nginx) via migration `20260323120000_UpdateApplicationUrls`.

### Changed
- `Migrations/` added to `sonar.exclusions` (full analysis exclusion) in both backend `SonarQube.Analysis.xml` files; it was already in `sonar.coverage.exclusions` but SonarQube still raised issues on migration files — auto-generated code that cannot be meaningfully fixed.


## [0.15.0] - 2026-03-23
### Added
- Mailpit added to `docker-compose-tools.yml` for local email testing (SMTP on port 1025, web UI on `http://localhost:8025`).
- `EXPENSES_MANAGEMENT_USERS_EMAILAUTH_ENABLESSL` env var to control SSL on the SMTP connection; defaults to `true` to preserve existing behaviour.
- `EnableSsl` property on `EmailOptions`, wired through `Program.cs`, `EmailHelper`, and passed via `docker-compose-apps.yml`.
- `.env` updated to point at Mailpit (`host.docker.internal:1025`, `EnableSsl=false`); `.env.example` includes the new variable.
- NuGet package cache added to `ci-build.yml` and `ci-test.yml` (stored in MinIO via the S3 runner cache) to avoid redundant downloads across pipeline runs.
- `network_mode = "host"` added to the GitLab runner Docker config (`config.toml`) so job containers inherit the dind container's network and can reach external hosts (fixes `NU1301` restore failures).

### Fixed
- GitLab CI `dotnet restore` failing with `NU1301` (unable to reach `api.nuget.org`) caused by broken NAT inside nested Docker; resolved by setting `network_mode = "host"` on the runner.
- SonarQube S5332 hotspot on `client.EnableSsl` in `EmailHelper` suppressed with `// NOSONAR` and a justification comment (value is `false` only in local dev against Mailpit; all other environments use `true`).

## [0.14.0] - 2026-03-22
### Added
- HttpOnly cookie-based authentication: the backend now sets `auth_token` as an `HttpOnly; Secure; SameSite=Strict` cookie on login and clears it on logout, eliminating the JWT XSS vulnerability.
- `GET /auth/session` endpoint in users service: validates the HttpOnly cookie and returns 200/401, used by the frontend to restore session state on page load.
- `isLoading` state in `AuthContext`: prevents `ProtectedRoute` and `PublicOnlyRoute` from incorrectly redirecting while session restore is in progress.
- `PublicOnlyRoute` component: redirects authenticated users to `/home` when accessing `/`, `/login`, or `/register`.

### Fixed
- JWT token no longer stored in `localStorage` — resolves critical XSS risk (Bug #3).
- `api.ts` now passes `credentials: 'include'` on all requests to support cross-origin cookie forwarding.
- `api.ts` `request()` cognitive complexity reduced from 21 to 13 (SonarQube gate): extracted `getErrorMessage()` helper and simplified the unauthorized handler to `if/else`.
- `AuthenticationController` `auth_token` cookie now sets `Secure = true` (SonarQube gate).
- `api.ts` login redirect changed from `window.location.assign` to `globalThis.location.assign` (SonarQube gate: prefer `globalThis` over `window`).
- nginx config updated to forward `Cookie` header in `/internal/auth/check` subrequest and include `Access-Control-Allow-Credentials: true` on all API location blocks.
- `changePassword` now includes the user's email in the request body, resolving a 400 validation error from the API.

### Changed
- `AuthContext` removed all token state (`token`, `setAuthToken`, JWT decode logic); login now stores only user info in localStorage for UI display.
- All auth `post()` calls use `{ skipUnauthorized: true }` to prevent the global unauthorized handler from firing on expected 401 responses.
- `AuthenticationController`: extracted cookie name into `private const string AuthTokenCookie = "auth_token"` to eliminate magic string duplication (SonarQube gate).
- `AuthenticationControllerTests`: updated cookie assertions to use `Headers.SetCookie` and `Headers.Cookie` typed properties instead of string indexers (SonarQube recommendations).
- Dashboard welcome message now displays the user's first name instead of their email address, falling back to email if first name is unavailable.
- `AuthContext` user type extended with optional `firstName` field, populated from the login API response.

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
