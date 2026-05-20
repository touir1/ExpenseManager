# Expenses Manager — Frontend Dashboard: Fixed Issues
**QA date:** 2026-04-29 | **Source:** [Ongoing QA report](../../ongoing/qa_test_results/2026-04-29-frontend-dashboard-qa.md)

Items below were identified in the 2026-04-29 QA session and subsequently resolved.

---

## U-2 — ~~Expenses card on dashboard shows "Coming soon…" with no context~~ ✅ FIXED

**Severity:** Info  
**Affected route:** `/dashboard`

**Root cause:** `HomeDashboardPage.tsx` was a placeholder; Phase 7 backend dashboard endpoints existed but were not wired to any frontend.

**Fix applied (v0.106.0 — Phase 9 Hearth dashboard):**
- Full `HomeDashboardPage` replaced with the Hearth 2-col layout wiring all 6 Phase 7 endpoints (`/dashboard/summary`, `/monthly`, `/categories`, `/same-month-across-years`, `/by-currency`, `/recent`) via TanStack React Query.
- New components: `MonthHero`, `SpendChart`, `CategoryDonut`, `SameMonthChart`, `CurrenciesPanel`, `RecentExpenses`, `DashboardFilters`.
- `recharts` installed; charts render real data.
- "Coming soon…" placeholder text completely removed.

---

## F-4 — ~~No max-length validation on registration name fields~~ ✅ FIXED

**Severity:** Medium  
**Affected route:** `/register`

**Root cause:** `RegisterRequestValidator` had no `MaximumLength` rule; `registerSchema` had no `.max()` constraint. Oversized payloads were accepted and written to the DB.

**Fix applied (v0.71.0):**
- `RegisterRequestValidator.cs`: `.MaximumLength(100).WithMessage("FIELD_TOO_LONG")` added to `FirstName`, `LastName`, and `Email`; 3 new backend tests.
- `auth.schemas.ts`: `.max(100, ...)` added to `firstName`, `lastName`, and `email` in `registerSchema`.
- `RegisterPage.tsx`: `maxLength={100}` on all three inputs as a browser-level guard.
- `RegisterPage.test.tsx`: 3 new tests asserting per-field max-length error messages.

---

## F-1 — ~~Duplicate "Authentication token is missing" toasts on every page load~~ ✅ FIXED

**Severity:** Medium  
**Affected routes:** All public pages on every cold load while unauthenticated

**Root cause:** `AuthContext` fires `GET /auth/session` → 401, then `POST /auth/refresh` → 401 on startup. Each 401 fell through to `buildErrorResponse` in `api.service.ts` which called `errorHandler` (toast), even with `skipUnauthorized: true`. Result: two error toasts for fresh unauthenticated visitors.

**Fix applied:**
- Added `silent?: boolean` option to `request()`, `get()`, and `post()` in `api.service.ts`. When `true`, `buildErrorResponse` skips the `errorHandler` call while still returning the `ApiResponse` with the error for callers.
- Applied `{ skipUnauthorized: true, silent: true }` to `sessionCheck()` and `refreshRequest()` in `authApi.service.ts` — these are startup probes expected to fail; toasts are suppressed.
- Two new unit tests added in `api.service.test.ts` covering `silent` flag behaviour.
- `AuthContext.test.tsx` assertion updated to match the new opts (`SKIP_SILENT` constant).

---

## F-3 — ~~Invalid/expired verification link shows raw JSON~~ ✅ FIXED

**Severity:** Medium
**Affected flow:** Email verification → expired/reused token path

**Root cause:** `RegistrationController.ValidateEmail` returned `Unauthorized(new ErrorResponse { Message = "EMAIL_VERIFICATION_FAILED" })` as raw JSON when the token was invalid or already used. The browser rendered the JSON directly with no UI context.

**Fix applied:**
- Added `VerifyEmailErrorUrlPath` property to `Application` model and `ApplicationEo`.
- Added `APP_VerifyEmailErrorUrlPath` column mapping in `UsersAppDbContext.OnModelCreating`.
- Added EF migration `20260429200824_AddVerifyEmailErrorUrlPath` seeding `/verify-error` for the EXPENSES_MANAGER app.
- Updated `ApplicationService.GetApplicationByCodeAsync` to map the new field.
- Updated `RegistrationController.ValidateEmail`: when validation fails and `VerifyEmailErrorUrlPath` is configured, redirect to `{app.UrlPath}{app.VerifyEmailErrorUrlPath}` instead of returning JSON. Falls back to JSON response if path is not configured.
- Created `VerifyErrorPage.tsx` (`/verify-error` route) with friendly error message and "Back to register" CTA.
- Added `/verify-error` route in `router.tsx` (public, no auth guard).
- Added new test `ValidateEmail_ReturnsRedirectToErrorPage_WhenValidationFails_AndErrorUrlConfigured`; renamed existing test to clarify it covers the no-URL-configured fallback. All 15 controller tests pass.

---

## U-1 — ~~"Authentication token is missing" misleading for fresh visitors~~ ✅ FIXED

**Severity:** Medium  
**Affected routes:** All public/auth pages on cold load while unauthenticated

**Root cause:** Same root as F-1. `sessionCheck()` and `refreshRequest()` fired on startup, each 401 triggered the `errorHandler` toast with "Authentication token is missing." This message implies a prior session expired, which is false for first-time visitors — eroding trust.

**Fix applied:**  
Resolved by F-1's fix: `silent: true` on `sessionCheck()` and `refreshRequest()` suppresses the toast entirely. No toast fires at all for unauthenticated visitors on startup — the misleading message never shows.

---

## F-2 — ~~Duplicate error toasts on failed login~~ ✅ FIXED

**Severity:** Medium  
**Affected route:** `/login`

**Root cause:** On a failed login (401), two toasts fired simultaneously:
1. `api.service.ts` `buildErrorResponse` called `errorHandler` → "Invalid email or password."
2. `LoginPage.onSubmit` called `show(...)` → "Invalid credentials. Please try again."

`loginRequest()` used `skipUnauthorized: true` (suppresses retry + unauthorizedHandler) but not `silent: true` (which suppresses the generic errorHandler toast).

**Fix applied:**
- Added `silent: true` to `loginRequest()` opts in `authApi.service.ts`. The api.service generic toast is suppressed; `LoginPage` continues to show its own contextual message.
- `AuthContext.test.tsx`: two `toHaveBeenCalledWith` assertions for the login endpoint updated from `SKIP` to `SKIP_SILENT`. All 267 tests pass.

---

## U-3 — ~~Email footer shows stale copyright year "© 2024"~~ ✅ FIXED (v0.103.0)

**Severity:** Low  
**Affected:** All email templates (verification email, password reset email, family invitation email)

**Root cause:** Year was hardcoded as `2024` in `EMAIL_VERIFICATION_TEMPLATE.html` and `PASSWORD_RESET_TEMPLATE.html` in the users service. New expenses service templates also used a static year.

**Fix applied (v0.103.0):**
- `EMAIL_VERIFICATION_TEMPLATE.html` and `PASSWORD_RESET_TEMPLATE.html` (users service): replaced `© 2024` with `© @@YEAR@@`.
- `FAMILY_INVITATION_TEMPLATE.html` (expenses service): uses `@@YEAR@@` from creation.
- `EmailHelper.GetEmailTemplate` (both services): auto-substitutes `@@YEAR@@` → `DateTime.UtcNow.Year.ToString()` before processing any caller-provided parameters. No caller changes required — single point of change.
- 2 new tests in users `EmailHelperTests.cs` verifying auto-substitution.

---

## S-1 — ~~No rate limiting on login endpoint~~ ✅ FIXED (v0.92.0)

**Severity:** High  
**Affected endpoint:** `POST /api/users/auth/login`

**Root cause:** No rate limiting existed at any layer — neither nginx nor application level. Unlimited concurrent login attempts returned HTTP 401 indefinitely.

**Fix applied (v0.92.0):**
- Added `Microsoft.AspNetCore.RateLimiting` to the users service with fixed-window per-client-IP policies on all sensitive routes:
  - `login`: 10 req / 1 min
  - `register`: 5 req / 10 min
  - `resend_verification`: 3 req / 10 min
  - `validate_email`: 10 req / 5 min
  - `request_password_reset` / `change_password_reset` / `create_password`: 5 req / 10 min
  - `change_password`: 10 req / 5 min
  - `refresh`: 20 req / 1 min
  - `messaging_replay`: 5 req / 1 min
- Endpoints without rate limiting: `GET /auth/check`, `GET /auth/session`, `POST /auth/logout`, `GET /messaging/outbox/stats`.
- Exceeding any limit returns HTTP 429.
