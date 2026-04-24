# Expenses Manager Frontend

React 18 + TypeScript + Vite SPA with Tailwind CSS v3, serving the main user-facing dashboard.

## Tech Stack

- **React 18** + **TypeScript**
- **Vite 7** — dev server and bundler
- **Tailwind CSS v3** — utility-first styling with custom design system
- **React Router v6** — client-side routing
- **Vitest 4** + **React Testing Library** — unit and component tests

## Quick Start

```bash
# from this folder
npm ci
npm run dev        # http://localhost:5173
```

## Environment Config

Create a `.env` in this folder with your backend base URL:

```
VITE_API_BASE="https://api.example.com"
```

`AuthContext` reads `import.meta.env.VITE_API_BASE` to call APIs. TypeScript support via `src/env.d.ts`.

## Routes

| Path | Visibility | Component |
|---|---|---|
| `/` | Public | HomePublic |
| `/login` | Public | Login — email + password |
| `/register` | Public | Register — first name, last name, email |
| `/request-password-reset` | Public | RequestPasswordReset |
| `/reset-password` | Public | ResetPassword — prefilled from `?email` and `?h` query params |
| `/dashboard` | Private | HomeDashboard |
| `/settings` | Private | Settings |
| `/change-password` | Private | ChangePassword — old + new + repeat |
| `*` | Any | NotFound — 404 page |

Private routes are guarded by `ProtectedRoute` (`src/features/auth/ProtectedRoute.tsx`), which checks cookie-based auth state from `AuthContext`. Public routes are wrapped with `PublicOnlyRoute` (`src/features/auth/PublicOnlyRoute.tsx`), which redirects already-authenticated users to `/dashboard`.

## Commands

```bash
npm ci                # Install dependencies
npm run dev           # Development server
npm run build:prod    # Production build
npm run typecheck     # TypeScript type checking (no emit)
npm test              # Run tests with coverage (V8)
npm run test:watch    # Watch mode
```

## Styling

Tailwind CSS v3 with a custom design system defined in `tailwind.config.ts`:

- **Brand color:** indigo (`brand-600` = `#4f46e5`)
- **Font:** Inter (loaded via Google Fonts in `index.html`)
- **Reusable UI primitives** are defined in `@layer components` in `src/styles/index.css`:
  - `.field-label`, `.field-input` — form controls
  - `.btn-primary`, `.btn-secondary` — buttons
  - `.auth-page`, `.auth-card` — centered card layout for auth pages
  - `.msg-error`, `.msg-success`, `.msg-info` — feedback messages

## Structure

```
src/
  App.tsx                 # Provider composition (BrowserRouter, Toast, Auth, NavBar)
  router.tsx              # All <Routes> definitions
  env.d.ts                # Vite env type declarations
  components/             # Shared reusable UI components
    PasswordInput.tsx
    PasswordStrength.tsx
    Toast.tsx             # Toast notification provider and hook
    __tests__/
  features/
    auth/                 # Authentication feature
      AuthContext.tsx     # Cookie-based auth state and context
      ProtectedRoute.tsx  # Redirects unauthenticated users to /login
      PublicOnlyRoute.tsx # Redirects authenticated users to /dashboard
      __tests__/
  hooks/
    usePageTitle.ts       # Sets document.title per page
  layouts/                # Layout-level components
    NavBar.tsx            # Auth-aware responsive navigation
    __tests__/
  pages/
    HomePublic.tsx
    Login.tsx
    Register.tsx
    HomeDashboard.tsx
    ChangePassword.tsx
    ResetPassword.tsx
    RequestPasswordReset.tsx
    Settings.tsx
    NotFound.tsx
    __tests__/
  services/               # API layer
    api.ts                # Centralized API client (credentials: include, skipUnauthorized)
    __tests__/
  styles/
    index.css             # Tailwind directives + component layer
tailwind.config.ts        # Design system tokens and font config
postcss.config.cjs        # PostCSS pipeline for Tailwind
```
