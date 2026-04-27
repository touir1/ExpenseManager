## Key Constraints

Non-obvious things Claude cannot derive from reading files:

**nginx auth:** subrequest `/internal/auth/check` → `users-service:9100/auth/check` before every request; only `/api/users/auth/` is public.
**cross-service DB:** expenses reads `ext.USR_Users` from users DB via `Repositories/External/UserRepository` (read-only, mapped in `ExpensesDbContext`).
**messaging:** RabbitMQ singleton `IRabbitMQService.GetConnection()`; config env prefix: `EXPENSES_MANAGEMENT_EXPENSES_RABBITMQ_*`.
**DB migrations:** auto-run at startup via `MigrateAsync()`.
**frontend auth:** `credentials: 'include'` on all requests; session restored via `GET /auth/session`; all HTTP in `features/auth/services/authApi.service.ts`, state in `features/auth/AuthContext.tsx`.
**frontend forms:** all auth forms use `useForm<T>({ resolver: zodResolver(...) })` from React Hook Form; Zod schemas and inferred types live in `src/features/auth/auth.schemas.ts`; per-field errors use `aria-describedby` with field-scoped IDs (e.g. `email-error`); `PasswordInput` uses `forwardRef` so RHF's ref callback reaches the underlying `<input>`.
**frontend CSS:** shared primitives in `@layer components` in `index.css` — extend before adding new classes; brand `brand-600`; cards `bg-white shadow-card border border-slate-200 rounded-2xl`.
**backend tests:** use `TestExpensesDbContextWrapper` (in-memory DB); never mock DbContext.
**frontend tests:** `toHaveClass()` not `toHaveStyle()` — Tailwind doesn't produce inline styles in jsdom.
