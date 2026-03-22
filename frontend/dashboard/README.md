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
| `/home` | Private | HomeDashboard |
| `/change-password` | Private | ChangePassword — old + new + repeat |
| `*` | Public | Redirects to HomePublic |

Private routes are guarded by `ProtectedRoute`, which checks JWT auth state from `AuthContext`. Public routes (`/`, `/login`, `/register`) are wrapped with `PublicOnlyRoute`, which redirects already-authenticated users to `/home`.

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
- **Reusable UI primitives** are defined in `@layer components` in `src/index.css`:
  - `.field-label`, `.field-input` — form controls
  - `.btn-primary`, `.btn-secondary` — buttons
  - `.auth-page`, `.auth-card` — centered card layout for auth pages
  - `.msg-error`, `.msg-success`, `.msg-info` — feedback messages

## Structure

```
src/
  api.ts                  # Centralized API client
  App.tsx                 # Root component, router, providers
  index.css               # Tailwind directives + component layer
  env.d.ts                # Vite env type declarations
  auth/
    AuthContext.tsx        # JWT auth state and context
  components/
    NavBar.tsx             # Auth-aware responsive navigation
    ProtectedRoute.tsx     # Route guard (redirects to /login when unauthenticated)
    PublicOnlyRoute.tsx    # Route guard (redirects to /home when authenticated)
    Toast.tsx              # Notification toasts
    __tests__/
  pages/
    HomePublic.tsx
    Login.tsx
    Register.tsx
    HomeDashboard.tsx
    ChangePassword.tsx
    ResetPassword.tsx
    RequestPasswordReset.tsx
    __tests__/
tailwind.config.ts         # Design system tokens and font config
postcss.config.cjs         # PostCSS pipeline for Tailwind
```
