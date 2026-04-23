# Fixes, Suggestions & Ameliorations

A living document tracking open issues, improvement ideas, and technical debt across the ExpenseManager project. Items marked ✅ are resolved; all others are pending.

---

## Frontend — Open QA Items

These are the remaining unfixed issues from the [2026-03-22 QA report](qa_test_results/2026-03-22-frontend-dashboard-qa.md).

| # | Priority | Area | Item |
|---|----------|------|------|
| QA-11 | 🟡 Moderate | Feature | Core "Expenses" feature is "Coming soon…" — the app's primary purpose is unimplemented |
| QA-22 | 🔵 Low | SEO / A11y | Browser tab title is always "Expenses Manager" — no per-page `document.title` update |
| QA-24 | 🔵 Low | Code quality | Encoding artifacts in source strings (mojibake) — ensure all files are saved as UTF-8 |
| QA-25 | 🔵 Low | DX | React Router v6 future flags (`v7_startTransition`, `v7_relativeSplatPath`) not set — console warnings on every page load |
| QA-26 | 🔵 Low | Architecture | `onUnauthorized` handler registered in component body instead of `useEffect` — no cleanup, fires on every render |
| QA-27 | 🔵 Low | Performance | Auth functions (`login`, `logout`, `register`, etc.) not wrapped in `useCallback` — memoized context value updates unnecessarily |
| QA-28 | ⚙️ Security | Feature | No brute-force / rate-limit protection on the Login form — no lockout, CAPTCHA, or progressive delay |

---

## Frontend — Additional Suggestions

### UX / Interaction
- **Toast on login error**: Replace the inline `msg-error` paragraph on `Login.tsx` with a toast notification, consistent with `RequestPasswordReset.tsx` and `ChangePassword.tsx`.
- **Form autofocus**: Auto-focus the first input on page load for Login, Register, and RequestPasswordReset so users can start typing immediately without clicking.
- **Remember me**: Add an optional "Remember me" checkbox on the Login page to control session persistence (currently all sessions behave the same).
- **Loading skeleton on Dashboard**: `HomeDashboard.tsx` shows nothing meaningful while the auth session is restoring. A skeleton or placeholder avoids layout shift.

### Accessibility
- **Landmark roles**: Wrap page content in `<main>` elements so screen readers can navigate directly to the main content area.
- **Focus trap on mobile menu**: When the hamburger menu is open, keyboard focus should be trapped within the menu overlay.
- **Error message `aria-describedby`**: Link inline error messages to their corresponding input fields via `aria-describedby` so screen readers announce the error when the field is focused.

### Code quality
- **`api.ts` — typed error responses**: The current error handling extracts a generic message; model the error shape and return typed results instead of `boolean` from auth functions so callers can distinguish error types.
- **`AuthContext.tsx` — `useCallback` on auth functions**: See QA-27. Wrap `login`, `logout`, `register`, `changePassword`, `resetPassword`, and `requestPasswordReset` in `useCallback` to stabilize the memoized context value.
- **`AuthContext.tsx` — `onUnauthorized` in `useEffect`**: See QA-26. Register the global unauthorized handler inside a `useEffect` with proper cleanup to avoid re-registration on every render.
- **Test coverage on edge-cases**: Several async flows (token expiry mid-session, network timeout) have no test coverage. Add tests that simulate failed fetch responses.

### DevX
- **Storybook**: Add Storybook for `PasswordInput`, `PasswordStrength`, `Toast`, and `NavBar` so components can be developed and reviewed in isolation.
- **Husky + lint-staged**: Add pre-commit hooks to run `typecheck` and `eslint --fix` on staged files, catching issues before they reach CI.

---

## Backend — Suggestions

### Users service
- **Refresh token**: The current design issues a single JWT with no refresh token. Long-lived sessions become stale; add a refresh-token flow (`POST /auth/refresh`) to extend sessions without re-authentication.
- **Account lockout**: After N consecutive failed login attempts for a given email, lock the account for a time window and notify the user by email.
- **Email change**: There is no endpoint to update an account's email address. Add a `PUT /users/email` flow with re-verification.
- **`GET /auth/session` performance**: The session-check endpoint is called on every SPA load. Ensure the JWT validation path is lightweight (no DB hit on happy-path).

### Expenses service
- **Implement expenses CRUD**: `HomeDashboard.tsx` already shows a "Coming soon…" placeholder. The expenses service has models and migrations but no working controllers or services — these need to be implemented.
- **Pagination**: When expenses are implemented, the list endpoint should support cursor- or offset-based pagination from day one rather than returning all records.
- **Input sanitisation**: Validate and sanitise string fields (`description`, category name, etc.) at the controller layer before they reach the database.

---

## Infrastructure — Suggestions

- **Health-check endpoints**: Add `/healthz` (liveness) and `/readyz` (readiness) endpoints to both .NET services and wire them into the Docker Compose `healthcheck` stanza and nginx upstream checks.
- **Structured logging**: Replace default ASP.NET console logging with Serilog + JSON output. Forward logs to a centralised sink (e.g., Loki) for querying via Grafana.
- **Secret management**: Credentials (DB passwords, JWT signing key, SMTP credentials) are currently passed as plain environment variables. Migrate to Docker Secrets or a vault solution for production deployments.
- **nginx rate limiting**: Add `limit_req_zone` / `limit_req` directives in `expenses-manager.conf` to protect the auth endpoints from brute-force traffic at the proxy layer.
- **Automated DB backups**: No backup job is configured for the PostgreSQL instances. Add a scheduled backup script (e.g., `pg_dump` via the jobs container) with remote storage.
