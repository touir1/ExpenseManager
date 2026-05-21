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
│   │   ├── components/
│   │   │   ├── CategoryDonut.tsx      ← PieChart (Recharts) — category breakdown
│   │   │   ├── CurrenciesPanel.tsx    ← Per-currency totals panel
│   │   │   ├── DashboardFilters.tsx   ← Date-range / family / currency filter bar
│   │   │   ├── MonthHero.tsx          ← Current month total + delta vs. previous
│   │   │   ├── RecentExpenses.tsx     ← Last 10 expenses mini-list
│   │   │   ├── SameMonthChart.tsx     ← BarChart — same calendar month across years
│   │   │   ├── SpendChart.tsx         ← ComposedChart — monthly spend over time
│   │   │   └── __tests__/
│   │   ├── pages/
│   │   │   ├── HomeDashboardPage.tsx  ← Dashboard with all chart/stat components
│   │   │   └── SettingsPage.tsx
│   │   ├── services/
│   │   │   └── dashboardApi.service.ts ← Dashboard API calls (summary, monthly, etc.)
│   │   └── types/
│   │       └── dashboard.type.ts      ← DashboardSummaryDto, MonthlyBreakdownDto, etc.
│   │
│   ├── expenses/
│   │   ├── components/
│   │   │   ├── ExpenseFilters.tsx     ← Filter panel (date, category, currency, tags, amount)
│   │   │   ├── ExpenseForm.tsx        ← Shared add/edit form (RHF + Zod)
│   │   │   └── __tests__/
│   │   ├── pages/
│   │   │   ├── ExpensesPage.tsx       ← Paginated expense list with filters + delete
│   │   │   ├── AddExpensePage.tsx     ← Thin wrapper — renders ExpenseForm for create
│   │   │   ├── EditExpensePage.tsx    ← Loads expense by id, renders ExpenseForm for update
│   │   │   └── __tests__/
│   │   ├── services/
│   │   │   └── expensesApi.service.ts ← Expense CRUD + getById API calls
│   │   ├── types/
│   │   │   └── expenses.type.ts       ← ExpenseDto, ExpenseFilter, ExpenseRequest, etc.
│   │   └── expense.schemas.ts         ← Zod schema for ExpenseForm
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
| `/` | PublicOnly | `HomePublicPage` | Landing page |
| `/login` | PublicOnly | `LoginPage` | Login form |
| `/register` | PublicOnly | `RegisterPage` | Registration form |
| `/request-password-reset` | PublicOnly | `RequestPasswordResetPage` | Request reset email |
| `/reset-password` | PublicOnly | `ResetPasswordPage` | Create or reset password |
| `/verify-error` | Public | `VerifyErrorPage` | Expired verification link |
| `/dashboard` | Protected | `HomeDashboardPage` | Dashboard with charts and stats |
| `/settings` | Protected | `SettingsPage` | User settings |
| `/change-password` | Protected | `ChangePasswordPage` | Change password form |
| `/families` | Protected | `FamiliesPage` | Family management |
| `/families/accept-invite` | Protected | `AcceptInvitePage` | Accept family invitation |
| `/expenses` | Protected | `ExpensesPage` | Paginated expense list with filters |
| `/expenses/add` | Protected | `AddExpensePage` | Add new expense |
| `/expenses/:id/edit` | Protected | `EditExpensePage` | Edit existing expense |
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

## Expenses Feature

### Pages

| Component | Description |
|---|---|
| `ExpensesPage` | Paginated list with filters. Uses TanStack Query (`useQuery`) for fetching. Delete triggers `refetch`. |
| `AddExpensePage` | Thin wrapper — renders `ExpenseForm` in create mode. On success navigates to `/expenses`. |
| `EditExpensePage` | Loads expense by route param `id` via `useQuery(getExpenseById)`. Renders `ExpenseForm` in edit mode. |

### Components

**`ExpenseForm`** — shared add/edit form driven by `expense.schemas.ts` (Zod) + React Hook Form. Fields: amount, currency, date, category, subcategory, description, tags, family attribution. Disabled selects use `.catch(undefined)` in the Zod schema to coerce NaN from `valueAsNumber` on unset `<select>`.

**`ExpenseFilters`** — collapsible filter panel. Supported filters: `dateFrom`, `dateTo`, `categoryId`, `subcategoryId`, `currencyId`, `amountMin`, `amountMax`, `description`, `tagIds`, `displayCurrencyId`.

### Service — `expensesApi.service.ts`

| Function | HTTP | Endpoint |
|---|---|---|
| `getExpenses(filter)` | GET | `/api/expenses` |
| `getExpenseById(id)` | GET | `/api/expenses/{id}` |
| `createExpense(req)` | POST | `/api/expenses` |
| `updateExpense(id, req)` | PUT | `/api/expenses/{id}` |
| `deleteExpense(id)` | DELETE | `/api/expenses/{id}` |

### Types — `expenses.type.ts`

```typescript
ExpenseDto         // id, amount, currency, date, category, subcategory, description, tags, convertedAmount, displayCurrency
ExpensePagedResponse  // items, totalCount, page, pageSize, totalPages
ExpenseFilter      // all filter query params
ExpenseRequest     // POST/PUT body (amount, currencyId, date, categoryId?, subcategoryId?, description?, familyIds?, tagIds?)
Currency, Subcategory, Category, TagDto  // shared DTO shapes
```

---

## Dashboard Feature

### HomeDashboardPage

Composes all dashboard sub-components. Fetches data in parallel on mount (and on filter change) from the dashboard API. Each section shows a loading skeleton or error state independently.

### Components

| Component | Chart type | Data source |
|---|---|---|
| `MonthHero` | Stat card | `GET /dashboard/summary` — total, delta %, top category |
| `SpendChart` | ComposedChart (Recharts) | `GET /dashboard/monthly` — monthly totals over date range |
| `CategoryDonut` | PieChart (Recharts) | `GET /dashboard/categories` — breakdown by top-level category |
| `SameMonthChart` | BarChart (Recharts) | `GET /dashboard/same-month-across-years` — current month across all years |
| `CurrenciesPanel` | Stat list | `GET /dashboard/by-currency` — per-currency totals |
| `RecentExpenses` | Mini-list | `GET /dashboard/recent` — 10 most recent expenses |
| `DashboardFilters` | Filter bar | Controls `familyId`, `dateFrom`, `dateTo`, `displayCurrencyId` |

### Service — `dashboardApi.service.ts`

| Function | Endpoint |
|---|---|
| `getSummary(filter)` | `/api/expenses/dashboard/summary` |
| `getMonthly(filter)` | `/api/expenses/dashboard/monthly` |
| `getCategories(filter)` | `/api/expenses/dashboard/categories` |
| `getSameMonthYearly(month, familyId?, displayCurrencyId?)` | `/api/expenses/dashboard/same-month-across-years` |
| `getByCurrency(filter)` | `/api/expenses/dashboard/by-currency` |
| `getRecent(filter)` | `/api/expenses/dashboard/recent` |

All accept a `DashboardFilter` (`familyId?`, `dateFrom?`, `dateTo?`, `displayCurrencyId?`).

### Types — `dashboard.type.ts`

```typescript
DashboardSummaryDto      // totalAmount, convertedTotal, displayCurrency, expenseCount, previousPeriodTotal, changePercent, topCategory, topCategoryAmount
MonthlyBreakdownDto      // year, month, totalAmount, convertedTotal, byCategory: CategoryAmountDto[]
CategoryBreakdownDto     // category, totalAmount, convertedTotal, percentage, subcategories
SameMonthYearlyDto       // year, totalAmount, convertedTotal
CurrencyBreakdownDto     // currency, totalAmount, convertedAmount, expenseCount
DashboardFilter          // familyId?, dateFrom?, dateTo?, displayCurrencyId?
```

---

## NavBar

`src/layouts/NavBar.tsx` renders navigation links conditionally based on auth state.

**Unauthenticated:** marketing anchor links (How it Works, For Families, Pricing, Help), Language Switcher, Sign In, Get Started button.

**Authenticated (desktop):**
- Nav links: Dashboard, Expenses, Families
- Right-side controls: `FamilySelector` dropdown, `DisplayCurrencySelector` dropdown, notifications placeholder, user avatar with dropdown (Settings, Language Switcher, Sign Out)

**Authenticated (mobile):** hamburger menu with focus trap — Dashboard, Expenses, Families, Settings, Sign Out, Language Switcher.

**Active link detection:**
- `/expenses/*` → Expenses link active (`pathname.startsWith('/expenses')`)
- `/families` → Families link active
- `/settings` or `/change-password` → Settings dropdown item active
