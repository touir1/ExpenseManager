# Frontend Dashboard

← [Wiki Index](./index.md)

---

## Overview

The frontend dashboard is a React 18 + TypeScript single-page application that provides the user-facing interface for ExpenseManager. It is served as static files by nginx. All API calls go through nginx (not directly to backend services). The application uses cookie-based authentication — no tokens are stored in `localStorage` or `sessionStorage`.

**Location:** `frontend/dashboard/`  
**Dev server:** `http://localhost:5173`  
**Production entry:** `index.html` served by nginx at `/`

---

## Tech Stack

| Component | Technology |
|---|---|
| Framework | React 18 |
| Language | TypeScript |
| Bundler | Vite 7 |
| Styling | Tailwind CSS v3 |
| Routing | React Router v6 |
| Forms | React Hook Form + Zod |
| Testing | Vitest 4 + React Testing Library |
| Coverage | V8 |

---

## Project Structure

```
frontend/dashboard/src/
├── App.tsx                    ← Provider composition and layout
├── router.tsx                 ← All route definitions
├── main.tsx                   ← React DOM entry point
├── env.d.ts                   ← Vite env type declarations
│
├── components/                ← Shared reusable UI primitives
│   ├── BackLink.tsx           ← Back navigation link
│   ├── FieldError.tsx         ← Inline form field error display
│   ├── SubmitButton.tsx       ← Loading-aware form submit button
│   ├── Toast.tsx              ← Toast notification provider and hook
│   ├── PasswordInput.tsx      ← Password field with show/hide toggle
│   ├── PasswordStrength.tsx   ← Password strength indicator
│   └── __tests__/
│
├── features/
│   ├── auth/
│   │   ├── AuthContext.tsx         ← Auth state, session restore, token refresh
│   │   ├── components/
│   │   │   ├── AuthCard.tsx        ← Centered card layout for auth pages
│   │   │   ├── AuthPageHeader.tsx  ← Page title + subtitle for auth forms
│   │   │   ├── EmailField.tsx      ← Reusable email input
│   │   │   ├── ProtectedRoute.tsx  ← Redirects to /login if not authenticated
│   │   │   └── PublicOnlyRoute.tsx ← Redirects to /dashboard if authenticated
│   │   ├── pages/
│   │   │   ├── LoginPage.tsx
│   │   │   ├── RegisterPage.tsx
│   │   │   ├── ChangePasswordPage.tsx
│   │   │   ├── RequestPasswordResetPage.tsx
│   │   │   └── ResetPasswordPage.tsx
│   │   ├── services/
│   │   │   └── authApi.service.ts  ← Auth API calls (login, register, etc.)
│   │   ├── types/
│   │   │   └── auth.type.ts        ← AuthContextValue, User types
│   │   └── schemas/
│   │       └── auth.schemas.ts     ← Zod validation schemas
│   │
│   ├── dashboard/
│   │   └── pages/
│   │       ├── HomeDashboardPage.tsx   ← Placeholder ("Coming soon…")
│   │       └── SettingsPage.tsx
│   │
│   └── public/
│       └── pages/
│           ├── HomePublicPage.tsx
│           ├── NotFoundPage.tsx
│           └── VerifyErrorPage.tsx     ← Friendly "Verification link expired" page
│
├── layouts/
│   └── NavBar.tsx              ← Navigation bar with auth-aware links
│
└── services/
    └── api.service.ts          ← Base HTTP client (fetch wrapper)
```

---

## App.tsx — Provider Composition

`App.tsx` composes the full provider tree:

```tsx
<BrowserRouter>
  <ToastProvider>          ← Global toast notifications
    <AuthProvider>         ← Authentication state
      <NavBar />
      <router.tsx />       ← All page routes
    </AuthProvider>
  </ToastProvider>
</BrowserRouter>
```

---

## Routing

Defined in `router.tsx`:

| Path | Guard | Component | Description |
|---|---|---|---|
| `/` | Public | `HomePublicPage` | Landing page |
| `/login` | PublicOnly | `LoginPage` | Login form |
| `/register` | PublicOnly | `RegisterPage` | Registration form |
| `/request-password-reset` | PublicOnly | `RequestPasswordResetPage` | Request reset email |
| `/reset-password` | PublicOnly | `ResetPasswordPage` | Create or reset password |
| `/verify-error` | Public | `VerifyErrorPage` | Expired verification link |
| `/dashboard` | Protected | `HomeDashboardPage` | Main dashboard |
| `/settings` | Protected | `SettingsPage` | User settings |
| `/change-password` | Protected | `ChangePasswordPage` | Change password form |
| `*` | Any | `NotFoundPage` | 404 fallback |

**Route guards:**
- `ProtectedRoute` — checks `AuthContext.isAuthenticated`; redirects to `/login` if false
- `PublicOnlyRoute` — checks `AuthContext.isAuthenticated`; redirects to `/dashboard` if true (prevents logged-in users from seeing login/register)
- Both guards respect the loading state — they wait for session restore to complete before deciding

---

## AuthContext

`src/features/auth/AuthContext.tsx` is the central auth state manager.

**State:**
```typescript
isAuthenticated: boolean
isLoading: boolean
user: User | null   // { email, firstName, lastName }
```

**Exposed functions (AuthContextValue):**
```typescript
login(email, password, rememberMe?)
logout()
register(firstName, lastName, email)
changePassword(email, oldPassword, newPassword)
createPassword(email, verificationHash, newPassword)
resetPassword(email, verificationHash, newPassword)
requestPasswordReset(email)
```

**Session restore on mount:**
1. `GET /api/users/auth/session` — validates existing `auth_token` cookie
2. If 401: `POST /api/users/auth/refresh` — uses `refresh_token` cookie
3. If refresh succeeds: retry `sessionCheck()`
4. If refresh fails: remain unauthenticated

**Global 401 handler:**
`onUnauthorized` callback registered in `api.service.ts`. When any API call returns 401 (and the request is not silent), the handler clears user state and redirects to `/login`.

**Application code:**
`APPLICATION_CODE` is read from `VITE_APPLICATION_CODE` env var (default: `EXPENSES_MANAGER`). Sent with login and registration requests to scope role lookups.

---

## API Service Layer

### `api.service.ts` — Base HTTP Client

Wraps native `fetch` with:
- Base URL from `VITE_API_BASE` env var
- JSON request/response handling
- Error response normalization into `ApiResponse<T>` shape
- `silent?: boolean` option — suppresses toast on error when `true`
- `skipUnauthorized?: boolean` — suppresses global 401 redirect
- `onUnauthorized(callback)` — registers/replaces the global 401 handler

```typescript
interface ApiResponse<T> {
  ok: boolean
  status: number
  data: T | null
  error: string | null
}
```

### `authApi.service.ts` — Auth API Calls

Thin wrappers over `api.service.ts` for each auth endpoint:

| Function | HTTP | Endpoint | Notes |
|---|---|---|---|
| `sessionCheck()` | GET | `/api/users/auth/session` | `silent: true` (expected to fail on fresh visit) |
| `loginRequest()` | POST | `/api/users/auth/login` | `silent: true, skipUnauthorized: true` |
| `logoutRequest()` | POST | `/api/users/auth/logout` | |
| `refreshRequest()` | POST | `/api/users/auth/refresh` | `silent: true` |
| `registerRequest()` | POST | `/api/users/auth/register` | |
| `changePasswordRequest()` | POST | `/api/users/auth/change-password` | |
| `createPasswordRequest()` | POST | `/api/users/auth/create-password` | |
| `resetPasswordRequest()` | POST | `/api/users/auth/change-password-reset` | |
| `requestPasswordResetRequest()` | POST | `/api/users/auth/request-password-reset` | |

---

## Zod Schemas (`auth.schemas.ts`)

All auth forms validate with Zod before submission:

- **loginSchema** — email (valid format), password (non-empty)
- **registerSchema** — firstName, lastName (non-empty, max 100), email (valid, max 100)
- **changePasswordSchema** — email, oldPassword, newPassword (min 8), repeatPassword (must match newPassword)
- **requestPasswordResetSchema** — email
- **resetPasswordSchema** — newPassword (min 8), repeatPassword (must match)

---

## Shared Components

### Toast

Global notification system accessible via `useToast()` hook:

```typescript
const { showToast } = useToast()
showToast('message', 'success' | 'error' | 'info')
```

Auto-dismisses after a configurable duration. Positioned fixed at top-right.

### PasswordInput

Password field with a toggle button to show/hide the value. Used in all password entry forms.

### PasswordStrength

Visual indicator showing password strength based on length and character class rules. Displayed below the new-password field on registration, change-password, and reset-password pages.

### SubmitButton

A button that shows a loading spinner when `isLoading` is true. Used in all form submissions to prevent double-submit.

### FieldError

Renders a red error message below a form field. Receives a string error (from Zod validation or API error).

### BackLink

A `←` navigation link that routes back to a specified path. Used at the top of auth sub-pages.

---

## Styling System

Tailwind CSS v3 with a custom design system in `tailwind.config.ts` and `src/styles/index.css`.

**Brand color:** Indigo — `brand-600` = `#4f46e5`  
**Font:** Inter (Google Fonts, loaded in `index.html`)

**Component classes (`@layer components`):**

| Class | Purpose |
|---|---|
| `.field-label` | Form label styling |
| `.field-input` | Form input styling |
| `.btn-primary` | Primary action button |
| `.btn-secondary` | Secondary/outline button |
| `.auth-page` | Full-height centered layout for auth pages |
| `.auth-card` | Shadowed card container for auth forms |
| `.msg-error` | Red error message block |
| `.msg-success` | Green success message block |
| `.msg-info` | Blue informational message block |

---

## Environment Variables

Create `.env` in `frontend/dashboard/`:

```
VITE_API_BASE="https://api.example.com"     # Required — nginx base URL
VITE_APPLICATION_CODE="EXPENSES_MANAGER"    # Optional — defaults to EXPENSES_MANAGER
```

TypeScript types declared in `src/env.d.ts`.

---

## Commands

```bash
npm ci                  # Install dependencies (clean install)
npm run dev             # Development server (http://localhost:5173)
npm run build:prod      # Production build → dist/
npm run typecheck       # TypeScript check (no emit)
npm test                # Run all tests with V8 coverage
npm run test:watch      # Watch mode
```

---

## Testing

Tests are co-located in `__tests__/` folders next to the components/pages they test.

```bash
npm test
```

**Coverage:** V8 via Vitest  
**Test utilities:** React Testing Library (`@testing-library/react`), `@testing-library/user-event`

**Key patterns:**
- Auth pages use `renderWithProviders()` helper that wraps components in `MemoryRouter` + `AuthProvider` with mocked API calls
- `AuthContext.test.tsx` tests the full context lifecycle (session restore, refresh flow, each auth function)
- `api.service.test.ts` tests `silent` flag, `skipUnauthorized`, and error normalization
- Route guard tests verify redirects for authenticated and unauthenticated states

---

## NavBar

`src/layouts/NavBar.tsx` renders navigation links conditionally based on auth state:

- **Unauthenticated:** Login, Register links
- **Authenticated:** Dashboard, Settings, (Change Password accessible from Settings), Logout button

Active link detection: `/settings` and `/change-password` both highlight the Settings nav item as active (`settingsClass` logic groups them as one navigation group).
