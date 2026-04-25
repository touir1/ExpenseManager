# Fixes, Suggestions & Ameliorations

A living document tracking open issues, improvement ideas, and technical debt across the ExpenseManager project. Items marked ✅ are resolved; all others are pending.

---

## Frontend — Open QA Items

These are the remaining unfixed issues from the [2026-03-22 QA report](qa_test_results/2026-03-22-frontend-dashboard-qa.md).

| # | Priority | Area | Item |
|---|----------|------|------|
| QA-11 | 🟡 Moderate | Feature | Core "Expenses" feature is "Coming soon…" — the app's primary purpose is unimplemented |
| QA-28 | ⚙️ Security | Feature | No brute-force / rate-limit protection on the Login form — no lockout, CAPTCHA, or progressive delay |

---

## Frontend — Additional Suggestions

### UX / Interaction

*All four items resolved — see [fixes-and-suggestions-applied.md](../fixed/fixes-and-suggestions-applied.md).*

### Accessibility
- **Landmark roles**: Wrap page content in `<main>` elements so screen readers can navigate directly to the main content area.
- **Focus trap on mobile menu**: When the hamburger menu is open, keyboard focus should be trapped within the menu overlay.
- **Error message `aria-describedby`**: Link inline error messages to their corresponding input fields via `aria-describedby` so screen readers announce the error when the field is focused.

### Code quality
- **`src/services/api.ts` — typed error responses**: The current error handling extracts a generic message; model the error shape and return typed results instead of `boolean` from auth functions so callers can distinguish error types.
- **Test coverage on edge-cases**: Several async flows (token expiry mid-session, network timeout) have no test coverage. Add tests that simulate failed fetch responses.

### DevX
- **Storybook**: Add Storybook for `PasswordInput`, `PasswordStrength`, `Toast`, and `NavBar` so components can be developed and reviewed in isolation.
- **Husky + lint-staged**: Add pre-commit hooks to run `typecheck` and `eslint --fix` on staged files, catching issues before they reach CI.

### Library Adoption Roadmap

The dashboard currently uses vanilla `fetch`, manual `useState` forms, inline validation, Context API for state, Tailwind for styling, and no charts, i18n, file upload, or animation library. The following adoption plan is recommended as the project grows:

#### Phase 1 — Auth refactor (do now)

- **React Hook Form** (`react-hook-form`): Every auth form page duplicates the same `useState`-per-field + manual `setError` + `setSubmitting` pattern. React Hook Form eliminates this boilerplate with `useForm<T>()`, `register()`, and automatic `isSubmitting` management. Affects `Login.tsx`, `Register.tsx`, `ChangePassword.tsx`, `ResetPassword.tsx`, `RequestPasswordReset.tsx`.
- **Zod** (`zod` + `@hookform/resolvers`): Validation logic is currently scattered inline per form. Zod schemas centralize rules, provide TypeScript inference via `z.infer<>`, and integrate directly with React Hook Form via `zodResolver()`. Replaces all manual email-regex, length, and password-match checks.

#### Phase 2 — Expenses feature (when expenses is built)

- **TanStack Query** (`@tanstack/react-query`): The expenses feature will need paginated lists, filters, background refetch, and cache invalidation. TanStack Query handles all of this and eliminates per-component `[loading, setLoading]` / `[data, setData]` patterns. `api.ts` stays as the low-level fetch layer; TanStack Query calls it. Auth mutations in `AuthContext` can remain as-is.
- **Recharts** (`recharts`): An expense manager without charts is incomplete. Expected visualizations: spending over time (line), by category (pie/donut), monthly comparison (bar). Recharts is composable, TypeScript-friendly, and lightweight (~200 KB). New components go in `src/components/charts/`.

#### Phase 3 — Conditional (only if needed)

- **react-i18next** (`react-i18next`): All text is currently hardcoded in English. Add only if multi-language support is an actual requirement. Prefer `react-i18next` over FormatJS for its simpler API; FormatJS adds value only if rich locale-aware number/date/plural formatting is needed (which can be layered on top via `i18next-icu`). Do not add both.
- **react-dropzone** (`react-dropzone`): Add only when receipt or attachment upload is being implemented.
- **Motion** (`motion` / Framer Motion): Current CSS transitions via Tailwind are sufficient. Add only after the expense feature's UX design calls for page transitions, drag-and-drop reordering, or complex entrance/exit animations.

#### Existing code refactors (independent of new libraries)

| Priority | Issue | Note |
|---|---|---|
| Medium | `HomeDashboard.tsx` is a placeholder with no real content | Needs design before implementing |

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
