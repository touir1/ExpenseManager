# Fixes, Suggestions & Ameliorations

A living document tracking open issues, improvement ideas, and technical debt across the ExpenseManager project. Items marked ✅ are resolved; all others are pending.

---

## Frontend — Open QA Items

These are the remaining unfixed issues from the [2026-03-22 QA report](qa_test_results/2026-03-22-frontend-dashboard-qa.md).

| # | Priority | Area | Item |
|---|----------|------|------|
| ~~QA-11~~ | ~~🟡 Moderate~~ | ~~Feature~~ | ~~Core "Expenses" feature is "Coming soon…"~~ — ✅ resolved in v0.105.0: full expense CRUD UI shipped (list, add, edit, delete, filters, pagination). Dashboard card still shows "Coming soon…" placeholder text — see U-2 below. |
| ~~QA-28~~ | ~~⚙️ Security~~ | ~~Feature~~ | ~~No brute-force / rate-limit protection on the Login form~~ — ✅ resolved in v0.92.0: backend rate limiting (10 req/1 min per IP) on login + all sensitive auth routes via .NET 8 `Microsoft.AspNetCore.RateLimiting` |

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

#### Phase 2 — Expenses feature ✅ done ([v0.105.0](../../CHANGELOG.md))

- ~~**TanStack Query** (`@tanstack/react-query`): The expenses feature will need paginated lists, filters, background refetch, and cache invalidation.~~ ✅ Installed in v0.105.0 — `ExpensesPage` uses `useQuery(getExpenses)` + `refetch` after delete; `EditExpensePage` uses `useQuery(getExpenseById)`. `api.service.ts` remains the low-level fetch layer.
- ~~**Recharts** (`recharts`): An expense manager without charts is incomplete. Expected visualizations: spending over time (line), by category (pie/donut), monthly comparison (bar). Recharts is composable, TypeScript-friendly, and lightweight (~200 KB). New components go in `src/components/charts/`. *Dashboard charting (Phase 7 backend complete) awaits this frontend implementation.*~~ ✅ Installed in v0.106.0 — `SpendChart` (ComposedChart), `CategoryDonut` (PieChart), `SameMonthChart` (BarChart) all use Recharts; components live in `src/features/dashboard/components/`.

#### Phase 3 — react-i18next ✅ done ([v0.72.0](../../CHANGELOG.md))

*Moved to [fixes-and-suggestions-applied.md](../fixed/fixes-and-suggestions-applied.md).*

#### Phase 3 remaining — Conditional (only if needed)

- **react-dropzone** (`react-dropzone`): Add only when receipt or attachment upload is being implemented.
- **Motion** (`motion` / Framer Motion): Current CSS transitions via Tailwind are sufficient. Add only after the expense feature's UX design calls for page transitions, drag-and-drop reordering, or complex entrance/exit animations.

#### Existing code refactors (independent of new libraries)

*All items resolved — see [fixes-and-suggestions-applied.md](../fixed/fixes-and-suggestions-applied.md).*

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
- ✅ **Tags (Phase 5)**: Implemented in v0.98.0 — global tag table (unique by name, case-sensitive), `UserTags` junction for adoption tracking, `GET/POST/DELETE /tags`, tag visibility across family co-members, auto-adopt on expense attachment, `TagInput` component.
- **Input sanitisation**: Validate and sanitise string fields (`description`, category name, etc.) at the controller layer before they reach the database.

---

## Infrastructure — Suggestions

- **Health-check endpoints**: ⚠️ Partial — both services now expose `GET /health` (liveness + DB readiness). Remaining: wire into Docker Compose `healthcheck` stanza and nginx upstream checks.
- **Structured logging**: Replace default ASP.NET console logging with Serilog + JSON output. Forward logs to a centralised sink (e.g., Loki) for querying via Grafana.
- **Secret management**: Credentials (DB passwords, JWT signing key, SMTP credentials) are currently passed as plain environment variables. Migrate to Docker Secrets or a vault solution for production deployments.
- **nginx rate limiting**: ⚠️ Partial — backend rate limiting implemented in v0.92.0 via .NET 8 `Microsoft.AspNetCore.RateLimiting` on all sensitive routes. Remaining: add `limit_req_zone` / `limit_req` directives in `expenses-manager.conf` as an additional proxy-layer defense.
- **Automated DB backups**: No backup job is configured for the PostgreSQL instances. Add a scheduled backup script (e.g., `pg_dump` via the jobs container) with remote storage.
