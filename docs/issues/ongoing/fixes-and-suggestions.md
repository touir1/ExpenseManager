# Fixes, Suggestions & Ameliorations

A living document tracking open issues, improvement ideas, and technical debt across the ExpenseManager project. Items marked ✅ are resolved; all others are pending.

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

#### Phase 2 — Expenses feature ✅ done ([v0.105.0](../../CHANGELOG.md) / [v0.106.0](../../CHANGELOG.md))

*Moved to [fixes-and-suggestions-applied.md](../fixed/fixes-and-suggestions-applied.md).*

#### Phase 3 — react-i18next ✅ done ([v0.72.0](../../CHANGELOG.md))

*Moved to [fixes-and-suggestions-applied.md](../fixed/fixes-and-suggestions-applied.md).*

#### Phase 3 remaining — Conditional (only if needed)

- **react-dropzone** (`react-dropzone`): Add only when receipt or attachment upload is being implemented.
- **Motion** (`motion` / Framer Motion): Current CSS transitions via Tailwind are sufficient. Add only after the expense feature's UX design calls for page transitions, drag-and-drop reordering, or complex entrance/exit animations.

#### Existing code refactors (independent of new libraries)

*All items resolved — see [fixes-and-suggestions-applied.md](../fixed/fixes-and-suggestions-applied.md).*

---

## Backend — Suggestions

### Users service
- **Account lockout**: After N consecutive failed login attempts for a given email, lock the account for a time window and notify the user by email.
- **Email change**: There is no endpoint to update an account's email address. Add a `PUT /users/email` flow with re-verification.
- **`GET /auth/session` performance**: The session-check endpoint is called on every SPA load. Ensure the JWT validation path is lightweight (no DB hit on happy-path).

### Expenses service
- **Input sanitisation**: ⚠️ Partial (v0.110.8) — CSV import now validates tag names (length/count), enforces per-field length limits via `ValidateRowsRequestValidator`, and probes for binary content. Remaining: general string field sanitisation on non-CSV endpoints (`description`, category name, etc.) at the controller layer.

---

## Infrastructure — Suggestions

- **Health-check endpoints**: ⚠️ Partial — both services now expose `GET /health` (liveness + DB readiness). Remaining: wire into Docker Compose `healthcheck` stanza and nginx upstream checks.
- **Structured logging**: Replace default ASP.NET console logging with Serilog + JSON output. Forward logs to a centralised sink (e.g., Loki) for querying via Grafana.
- **Secret management**: Credentials (DB passwords, JWT signing key, SMTP credentials) are currently passed as plain environment variables. Migrate to Docker Secrets or a vault solution for production deployments.
- **nginx rate limiting**: ⚠️ Partial — backend rate limiting implemented in v0.92.0 via .NET 8 `Microsoft.AspNetCore.RateLimiting` on all sensitive routes. Remaining: add `limit_req_zone` / `limit_req` directives in `expenses-manager.conf` as an additional proxy-layer defense.
- **Automated DB backups**: No backup job is configured for the PostgreSQL instances. Add a scheduled backup script (e.g., `pg_dump` via the jobs container) with remote storage.
