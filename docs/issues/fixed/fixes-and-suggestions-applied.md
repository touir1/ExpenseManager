# Applied Fixes & Suggestions

A record of improvement ideas from [fixes-and-suggestions.md](../ongoing/fixes-and-suggestions.md) that have been implemented. Move items here from the ongoing file once they are shipped, and record the version and date they landed.

---

## Frontend

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

### OCP — Abstract email dispatch — 2026-04-29 (v0.62.0)

`IEmailService` abstraction introduced; SMTP dispatch logic extracted from `EmailHelper` into `SmtpEmailService`.

| Refactor | Resolution |
|---|---|
| Introduce `IEmailService` abstraction; move SMTP logic behind it | `IEmailService` contract in `Infrastructure/Contracts/IEmailService.cs`; `SmtpEmailService` implementation in `Infrastructure/SmtpEmailService.cs`; `EmailHelper.SendEmail()` now delegates to `IEmailService`; `Program.cs` registers `SmtpEmailService` as scoped `IEmailService`. Swapping email providers requires only a new implementation + one DI registration change |

---

## Infrastructure

*No applied suggestions yet.*
