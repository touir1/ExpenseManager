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

*All three items resolved — see [fixes-and-suggestions-applied.md](../fixed/fixes-and-suggestions-applied.md).*

### Code quality

*Both items resolved — see [fixes-and-suggestions-applied.md](../fixed/fixes-and-suggestions-applied.md).*

### DevX
- **Storybook**: Add Storybook for `PasswordInput`, `PasswordStrength`, `Toast`, and `NavBar` so components can be developed and reviewed in isolation.
- **Husky + lint-staged**: Add pre-commit hooks to run `typecheck` and `eslint --fix` on staged files, catching issues before they reach CI.

### Library Adoption Roadmap

The dashboard currently uses vanilla `fetch`, React Hook Form + Zod for auth forms, Context API for state, Tailwind for styling, and no charts, i18n, file upload, or animation library. The following adoption plan is recommended as the project grows:

#### Phase 1 — Auth refactor ✅ done ([v0.48.0](../../CHANGELOG.md))

*Moved to [fixes-and-suggestions-applied.md](../fixed/fixes-and-suggestions-applied.md).*

#### Phase 2 — Expenses feature (when expenses is built)

- **TanStack Query** (`@tanstack/react-query`): The expenses feature will need paginated lists, filters, background refetch, and cache invalidation. TanStack Query handles all of this and eliminates per-component `[loading, setLoading]` / `[data, setData]` patterns. `api.service.ts` stays as the low-level fetch layer; TanStack Query calls it. Auth mutations in `AuthContext` can remain as-is.
- **Recharts** (`recharts`): An expense manager without charts is incomplete. Expected visualizations: spending over time (line), by category (pie/donut), monthly comparison (bar). Recharts is composable, TypeScript-friendly, and lightweight (~200 KB). New components go in `src/components/charts/`.

#### Phase 3 — react-i18next ✅ done ([v0.72.0](../../CHANGELOG.md))

*Moved to [fixes-and-suggestions-applied.md](../fixed/fixes-and-suggestions-applied.md).*

#### Phase 3 remaining — Conditional (only if needed)

- **react-dropzone** (`react-dropzone`): Add only when receipt or attachment upload is being implemented.
- **Motion** (`motion` / Framer Motion): Current CSS transitions via Tailwind are sufficient. Add only after the expense feature's UX design calls for page transitions, drag-and-drop reordering, or complex entrance/exit animations.

#### Existing code refactors (independent of new libraries)

| Priority | Issue | Note |
|---|---|---|
| Medium | `HomeDashboardPage.tsx` is a placeholder with no real content | Needs design before implementing |

---

## Backend — SOLID / Architecture Refactors

*SRP and OCP refactors completed — see [fixes-and-suggestions-applied.md](../fixed/fixes-and-suggestions-applied.md).*

### LSP — Fix nullable contract mismatches

*Both items resolved — see [fixes-and-suggestions-applied.md](../fixed/fixes-and-suggestions-applied.md).*

### ISP — Split fat interfaces

*Both items obsolete — see [fixes-and-suggestions-applied.md](../fixed/fixes-and-suggestions-applied.md).*

### DIP — Replace concrete dependencies with abstractions

*Both items obsolete — see [fixes-and-suggestions-applied.md](../fixed/fixes-and-suggestions-applied.md).*

---

## Validation Parity — Frontend vs Backend Mismatches

Discrepancies found by cross-checking Zod schemas (`auth.schemas.ts`) against controller/service logic in `backend/users/`.

*VAL-03 and VAL-04 resolved — see [fixes-and-suggestions-applied.md](../fixed/fixes-and-suggestions-applied.md).*

---

## Backend — Suggestions

### Users service
- ✅ **Refresh token**: Implemented in v0.52.0 — `POST /auth/refresh` endpoint with DB-backed opaque tokens, cookie rotation, and transparent frontend retry on 401.
- **Account lockout**: After N consecutive failed login attempts for a given email, lock the account for a time window and notify the user by email.
- **Email change**: There is no endpoint to update an account's email address. Add a `PUT /users/email` flow with re-verification.
- **`GET /auth/session` performance**: The session-check endpoint is called on every SPA load. Ensure the JWT validation path is lightweight (no DB hit on happy-path).

### Expenses service
- ✅ **Implement expenses CRUD**: Phase 3 complete in v0.91.0 — `POST/PUT/DELETE/GET /expenses` + paged `GET /expenses` live; soft-delete, audit trail, ownership enforcement all implemented.
- ✅ **Pagination**: Implemented in v0.91.0 — `GetPagedAsync` supports offset pagination with `Page`/`PageSize` and full filter set (date range, category, currency, amount range, description).
- **Input sanitisation**: Validate and sanitise string fields (`description`, category name, etc.) at the controller layer before they reach the database.

---

## Infrastructure — Suggestions

- **Health-check endpoints**: ⚠️ Partial — both services now expose `GET /health` (liveness + DB readiness). Remaining: wire into Docker Compose `healthcheck` stanza and nginx upstream checks.
- **Structured logging**: Replace default ASP.NET console logging with Serilog + JSON output. Forward logs to a centralised sink (e.g., Loki) for querying via Grafana.
- **Secret management**: Credentials (DB passwords, JWT signing key, SMTP credentials) are currently passed as plain environment variables. Migrate to Docker Secrets or a vault solution for production deployments.
- **nginx rate limiting**: Add `limit_req_zone` / `limit_req` directives in `expenses-manager.conf` to protect the auth endpoints from brute-force traffic at the proxy layer.
- **Automated DB backups**: No backup job is configured for the PostgreSQL instances. Add a scheduled backup script (e.g., `pg_dump` via the jobs container) with remote storage.
