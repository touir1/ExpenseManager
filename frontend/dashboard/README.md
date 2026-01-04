# Expenses Manager Frontend (React + Vite + TypeScript)

A minimal React template to get you moving fast. Includes Vite for dev/build and strict TypeScript settings.

## Quick start

```bash
# from this folder
npm install
npm run dev
```

## Environment config

Create an `.env` in this folder with your backend base URL:

```
VITE_API_BASE="https://api.example.com"
```

`AuthContext` reads `import.meta.env.VITE_API_BASE` to call APIs (e.g., `/auth/login`).
For TypeScript support, `src/env.d.ts` includes Vite env types.

## Routes

- Public: `/` (Home), `/login`, `/register`, `/reset-password`
- Private: `/home-auth`, `/change-password` (requires login)

Login expects email + password. Register collects first name, last name, email. Change password requires old + new + repeat. Reset password requires email + verificationHash (prefilled via link query `?email` and `?h`) + new + repeat.

- Dev server: http://localhost:5173
- Build for production:

```bash
npm run build
npm run preview
```

## Structure

- index.html — Vite entry
- src/main.tsx — App bootstrap
- src/App.tsx — Sample component
- src/index.css — Minimal styles (you can replace with Tailwind later)
