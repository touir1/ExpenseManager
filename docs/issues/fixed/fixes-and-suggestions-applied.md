# Applied Fixes & Suggestions

A record of improvement ideas from [fixes-and-suggestions.md](../ongoing/fixes-and-suggestions.md) that have been implemented. Move items here from the ongoing file once they are shipped, and record the version and date they landed.

---

## Frontend

### QA-11: Core Expenses feature — 2026-05-19 (v0.105.0)

| Item | Resolution |
|------|------------|
| QA-11: "Coming soon…" placeholder | Full expense CRUD UI shipped: `ExpensesPage` (paginated list, filters, delete confirm), `AddExpensePage`, `EditExpensePage`, `ExpenseForm`, `ExpenseFilters`. Routes `/expenses`, `/expenses/add`, `/expenses/:id/edit` added. "Expenses" NavLink in desktop + mobile nav. 56 new tests, 644 total passing. |

### Library adoption: Phase 2 (TanStack Query) — 2026-05-19 (v0.105.0)

| Item | Resolution |
|------|------------|
| `@tanstack/react-query` | Installed v5.x; `QueryClientProvider` added to `AppProviders`. `ExpensesPage` uses `useQuery(getExpenses, filter)` + `refetch` after delete. `EditExpensePage` uses `useQuery(getExpenseById)`. Auth mutations in `AuthContext` remain unchanged. |
| Zod v4 migration | `expense.schemas.ts` updated: `required_error`/`invalid_type_error` → `error:`; `z.preprocess` NaN coercion → `.catch(undefined)` to avoid type inference loss in Zod v4. |

### Library adoption: Phase 3 (react-i18next) — 2026-05-02 (v0.72.0)

| Item | Resolution |
|------|------------|
| react-i18next | `react-i18next`, `i18next`, `i18next-browser-languagedetector` installed; `src/i18n/index.ts` singleton; detection order: localStorage → browser navigator |
| 4 languages | English, French, Spanish, German translation JSON files under `src/i18n/locales/{en,fr,es,de}/translation.json` — full UI coverage |
| LanguageSwitcher | `src/components/LanguageSwitcher.tsx` dropdown added to NavBar (desktop + mobile) |
| Schema factory functions | All 5 auth schemas in `auth.schemas.ts` converted to `makeXxxSchema(t: TFunction)`; consumers use `useMemo(() => makeXxxSchema(t), [t])` + `zodResolver(schema)` |
| Dynamic API errors | `apiErrors.constant.ts` uses getter properties + Proxy so error strings resolve via `i18next.t()` at access time |
| All pages/components | `LoginPage`, `RegisterPage`, `ChangePasswordPage`, `RequestPasswordResetPage`, `ResetPasswordPage`, `HomeDashboardPage`, `SettingsPage`, `HomePublicPage`, `NotFoundPage`, `VerifyErrorPage`, `NavBar`, `EmailField`, `PasswordStrength` all use `useTranslation` + `t()` |

### Library adoption: Phase 1 (React Hook Form + Zod) — 2026-04-26 (v0.48.0)

| Item | Resolution |
|------|------------|
| React Hook Form | All five auth pages (`LoginPage`, `RegisterPage`, `ChangePasswordPage`, `ResetPasswordPage`, `RequestPasswordResetPage`) refactored from per-field `useState` + manual `setSubmitting` to `useForm<T>({ resolver: zodResolver(...) })`. `isSubmitting` from `formState` drives loading/disabled state automatically |
| Zod schemas | `src/features/auth/auth.schemas.ts` centralises all five form schemas and exports inferred TypeScript types. `zodResolver()` wires schemas into React Hook Form, replacing scattered inline validation logic |
| Per-field errors | Each form input now shows its own error message immediately below it. Error elements carry stable IDs (`{field}-error`); inputs link via `aria-describedby`. Server-level errors via `setError('root', ...)` or local `serverMsg` state |
| `PasswordInput` `forwardRef` | Component updated to `forwardRef<HTMLInputElement, Props>` so React Hook Form's `ref` callback reaches the underlying `<input>` |
| `.field-error` CSS class | Added to `@layer components` in `index.css` for per-field error text styling |

### Code quality — 2026-04-25 (v0.45.0)

| Item | Resolution |
|------|------------|
| Typed error responses | Auth context functions return `AuthResult` (`{ ok: boolean; error?: string }`) instead of `boolean`. Callers can now inspect `.error` to distinguish network errors, backend validation errors, and other failure types. `AuthResult` exported from `src/types/auth.type.ts` |
| Edge-case test coverage | Added tests for: network error during session restore (status 0), network error during login with error propagation, sessionStorage token-expiry flow (unauthorised handler clears sessionStorage + redirects), and session restore from sessionStorage without localStorage |

### Accessibility — 2026-04-25 (v0.44.0)

| Item | Resolution |
|------|------------|
| Landmark roles | `App.tsx` already wraps all page routes in `<main className="flex-1 flex flex-col">` and `NavBar` returns a `<header>` — the correct landmark structure was already in place |
| Focus trap on mobile menu | `NavBar.tsx`: hamburger button gains `aria-expanded` + `aria-controls="mobile-menu"`. Menu div gains `id`, `role="navigation"`, and `aria-label`. A `useEffect` on `mobileOpen` moves focus to the first menu item on open, traps Tab/Shift-Tab within the menu, closes on Escape, and restores focus to the hamburger button on close |
| Error message `aria-describedby` | Error `<p>` elements in `Register.tsx`, `ChangePassword.tsx`, and `ResetPassword.tsx` given stable IDs (`register-msg`, `change-password-msg`, `reset-password-msg`). All inputs in each form link to the error via `aria-describedby` when a message is present, so screen readers announce the error when an input is focused |

### UX / Interaction improvements — 2026-04-25 (v0.43.0)

| Item | Resolution |
|------|------------|
| Toast on login error | Replaced inline `msg-error` paragraph in `Login.tsx` with `useToast` — error now appears as a toast consistent with `RequestPasswordReset.tsx` and `ChangePassword.tsx` |
| Form autofocus | Added `autoFocus` to the first input on `Login.tsx` (email), `Register.tsx` (first name), and `RequestPasswordReset.tsx` (email) |
| Remember me | Added "Remember me" checkbox to `Login.tsx`; unchecked (default) stores session in `sessionStorage` (clears on tab close), checked stores in `localStorage` (persists across sessions). `AuthProvider` login, logout, and session-restore all updated accordingly |
| Loading skeleton on Dashboard | `HomeDashboard.tsx` renders animated skeleton cards while `isLoading` is `true`, avoiding layout shift during session restore |

### QA-28: Backend rate limiting — 2026-05-08 (v0.92.0)

| Item | Resolution |
|------|------------|
| No brute-force / rate-limit protection on login or any auth route | Added `Microsoft.AspNetCore.RateLimiting` (fixed-window, per-client-IP) to the users service. Policies: `login` 10 req/1 min, `register` 5 req/10 min, `resend_verification` 3 req/10 min, `validate_email` 10 req/5 min, `request_password_reset`/`change_password_reset`/`create_password` 5 req/10 min, `change_password` 10 req/5 min, `refresh` 20 req/1 min, `messaging_replay` 5 req/1 min. Exceeding any policy returns HTTP 429. No rate limiting on `GET /auth/check`, `GET /auth/session`, `POST /auth/logout`, `GET /messaging/outbox/stats`. |

### QA fixes — 2026-04-25 (v0.42.0)

| QA # | Area | Item | Resolution |
|------|------|------|------------|
| QA-24 | Code quality | Encoding artifacts in source strings (mojibake) | Verified all `src/` files are clean 7-bit ASCII — no embedded non-ASCII literals |
| QA-26 | Architecture | `onUnauthorized` registered in render body, no cleanup | Moved into `useEffect([], cleanup)`; handler cleared on unmount via `onUnauthorized(null)` |
| QA-27 | Performance | Auth functions recreated on every render, breaking `useMemo` | Wrapped all six auth functions in `useCallback`; `APPLICATION_CODE` hoisted to module scope; `useMemo` dep array now lists all callbacks explicitly |

### QA fixes — 2026-04-24 (v0.41.0)

| QA # | Area | Item | Resolution |
|------|------|------|------------|
| QA-25 | DX | React Router v6 future flag console warnings on every page load | Added `future={{ v7_startTransition: true, v7_relativeSplatPath: true }}` to `<BrowserRouter>` in `src/App.tsx` |

### Code quality — 2026-04-24

These four refactors were applied to `frontend/dashboard` as part of the dashboard code quality pass:

| Priority | Item | Resolution |
|---|---|---|
| High | No dedicated `types/` directory — types scattered inline across files | Created `src/types/auth.type.ts` (`User`, `AuthContextValue`) and `src/types/api.type.ts` (`ApiResponse<T>`) |
| Medium | `AuthContext` makes HTTP calls directly — mixes concerns | All HTTP auth calls extracted to `src/services/authApi.service.ts`; `AuthContext` now handles state only |
| Low | `NavBar.tsx` had no active-link highlighting via proper API | Replaced `Link` with `NavLink` from react-router-dom; shared `navLinkClass` helper; Settings keeps custom active logic for `/change-password` match |
| Low | `api.ts` error messages were hardcoded strings | Extracted to `src/constants/apiErrors.constant.ts` as `API_ERRORS` typed constants |

All 205 existing tests continued to pass after these changes.

---

## Backend

### SRP — Split `AuthenticationService` — 2026-04-28 (v0.60.0)

`AuthenticationService` was split into four focused classes; `AuthenticationController` split into three controllers.

| Refactor | Resolution |
|---|---|
| Extract `RegistrationService` + `IRegistrationService` | New `RegistrationService` handles `RegisterNewUserAsync` + `ValidateEmailAsync`; `RegistrationController` owns register + validate-email endpoints |
| Extract `JwtTokenService` + `IJwtTokenService` | New `JwtTokenService` handles `GenerateJwtToken` + `ValidateToken`; injected into `AuthenticationController` for token ops |
| Extract `PasswordManagementService` + `IPasswordManagementService` | New `PasswordManagementService` handles `ChangePasswordAsync`, `ResetPasswordAsync`, `RequestPasswordResetAsync`; `PasswordController` owns those endpoints |
| Split `AuthenticationController` | Three controllers: `AuthenticationController` (login/logout/session/refresh/check), `RegistrationController`, `PasswordController` |

### ISP / DIP — Obsolete after SRP refactor — 2026-04-29

These four items were raised before the SRP split (v0.60.0) and are no longer actionable.

| Item | Why obsolete |
|---|---|
| ISP: Split `IAuthenticationService` (7 methods) | The SRP refactor already split it into `IAuthenticationService` (1 method), `IJwtTokenService` (2 methods), `IRegistrationService` (2 methods), `IPasswordManagementService` (3 methods). No fat interface remains |
| ISP: Split `IEmailHelper` into `IEmailSender` / `IEmailValidator` / `IEmailTemplateProvider` | Every consumer (`RegistrationService`, `PasswordManagementService`) uses all three methods — validate → render → send is always one flow. No client is forced to depend on methods it doesn't use, so there is no ISP violation to fix; splitting would add three injected dependencies per class with no practical benefit |
| DIP: Inject `ITokenValidator` instead of inline JWT logic in `AuthenticationService` | JWT validation was extracted to `JwtTokenService` / `IJwtTokenService` in the SRP refactor. `AuthenticationService` now contains only credential verification and has no JWT logic at all |
| DIP: Depend on `IAuthOptions` instead of concrete `JwtAuthOptions` in `AuthenticationController` | `IOptions<JwtAuthOptions>` is already a framework-provided abstraction and is trivially mockable via `Options.Create(...)` — existing tests do exactly that. Introducing a custom `IAuthOptions` interface would duplicate two property declarations with no testability gain and no realistic second implementation |

### LSP — Fix nullable contract mismatches — 2026-04-29 (v0.63.0)

| Priority | Refactor | Resolution |
|---|---|---|
| Medium | `CreateUserAsync` returned `Task<User?>` | Changed to `Task<User>` in `IUserRepository` and `UserRepository`; creation throws on failure and always returns the entity. `RegistrationService` updated: explicit `User` type, null-conditionals removed, `DeleteUserAsync` call cleaned up |
| Low | `GetUserRolesByApplicationCodeAsync` returned `null` on null input / null repository result | `RoleService` now returns `Enumerable.Empty<RoleEo>()` in both cases, honouring the non-nullable `IEnumerable<RoleEo>` contract. `AuthenticationController` guard simplified from `roles == null \|\| !roles.Any()` to `!roles.Any()`. Tests updated accordingly |

### OCP — Abstract email dispatch — 2026-04-29 (v0.62.0)

`IEmailService` abstraction introduced; SMTP dispatch logic extracted from `EmailHelper` into `SmtpEmailService`.

| Refactor | Resolution |
|---|---|
| Introduce `IEmailService` abstraction; move SMTP logic behind it | `IEmailService` contract in `Infrastructure/Contracts/IEmailService.cs`; `SmtpEmailService` implementation in `Infrastructure/SmtpEmailService.cs`; `EmailHelper.SendEmail()` now delegates to `IEmailService`; `Program.cs` registers `SmtpEmailService` as scoped `IEmailService`. Swapping email providers requires only a new implementation + one DI registration change |

### FluentValidation + ConfirmPassword removal — 2026-04-30 (v0.67.0, v0.68.0)

| Item | Resolution |
|---|---|
| VAL-01: `newPassword` minimum length | `ChangePasswordRequestValidator` and `ChangePasswordResetRequestValidator` enforce `MinimumLength(8)` with `CascadeMode.Stop` → `PASSWORD_TOO_SHORT`. Wire format preserved via `InvalidModelStateResponseFactory` returning `401 ErrorResponse { Message = firstError }` |
| FluentValidation auto-validation | All manual `if`-check validation blocks removed from `AuthenticationController`, `RegistrationController`, `PasswordController`. Five `AbstractValidator<T>` classes in `Validators/`; registered via `AddFluentValidationAutoValidation()` + `AddValidatorsFromAssemblyContaining<Program>()` in `Program.cs` |
| ConfirmPassword removed from backend DTOs | `ChangePasswordRequest` and `ChangePasswordResetRequest` no longer have `ConfirmPassword` — it is frontend-only UX. `authApi.service.ts` calls updated; `AuthContext.tsx` signatures updated; 3 test files updated |

### Validation parity — 2026-05-04 (v0.73.0)

| Item | Resolution |
|---|---|
| VAL-03: `email` format validation on login | `LoginRequestValidator`: `.EmailAddress().WithMessage("INVALID_EMAIL_FORMAT")` added; 1 new test. Backend now rejects malformed emails at validator layer instead of silently failing at DB lookup |
| VAL-04: `email` format validation on registration — align to single impl | `RegisterRequestValidator`: `.EmailAddress().WithMessage("INVALID_EMAIL_FORMAT")` added; 1 new test. Redundant `MailAddress` check removed from `RegistrationService.RegisterNewUserAsync` — validator is now authoritative. Obsolete `RegisterNewUserAsync_ReturnsError_WhenEmailFormatIsInvalid` service test removed |

### Validation parity — 2026-05-01 (v0.71.0)

| Item | Resolution |
|---|---|
| VAL-02: `firstName`, `lastName`, `email` max-length on registration | `RegisterRequestValidator`: `.MaximumLength(100).WithMessage("FIELD_TOO_LONG")` on all three fields; 3 new backend tests. `registerSchema` in `auth.schemas.ts`: `.max(100, ...)` on all three fields. `RegisterPage.tsx`: `maxLength={100}` on all three inputs. 3 new frontend tests. |

### Expenses CRUD + Pagination — 2026-05-09 (v0.91.0)

| Item | Resolution |
|---|---|
| Implement expenses CRUD | `POST/PUT/DELETE/GET /expenses` + paged `GET /expenses` live; `ExpenseService`, `ExpenseRepository`, `ExpenseAuditService` with soft-delete, ownership enforcement, full audit trail |
| Pagination from day one | `GetPagedAsync` supports offset pagination (`Page`/`PageSize`) with full filter set: date range, category, subcategory, currency, amount range, description substring; descending by date |

### SonarQube quality — expenses validators — 2026-05-09 (v0.91.1)

| Item | Resolution |
|---|---|
| Duplicated validator code | `ExpenseRequestValidatorBase<T> where T : IExpenseRequest` extracted; both concrete validators (`CreateExpenseRequestValidator`, `UpdateExpenseRequestValidator`) are single-line subclasses |
| Value-type DTO properties nullable | Added `required` modifier to `Amount`, `CurrencyId`, `Date` on both request DTOs |
| 0% validator coverage | `ExpenseRequestValidatorTests`: 13 tests covering all rules for both validators |

---

## Infrastructure

*No applied suggestions yet.*
