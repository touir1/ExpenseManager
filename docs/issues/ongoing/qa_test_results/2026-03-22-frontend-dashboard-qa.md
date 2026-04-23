# Expenses Manager — Frontend Dashboard QA Report
**Date:** 2026-03-22 | **Status:** In progress — open issues only

This file contains only unresolved issues. All fixed items have been moved to
[docs/issues/fixed/qa/2026-03-22-frontend-dashboard-fixes.md](../../fixed/qa/2026-03-22-frontend-dashboard-fixes.md).

---

## 🟡 MODERATE ISSUES

### 11. Dashboard page says "Expenses — Coming soon…" for the core feature
**Page:** `/dashboard`
**Detail:** The entire expenses management feature — which is the core purpose of the app — is marked as "Coming soon…" with no further indication of timeline or workaround. The app's tagline is "Track your expenses, simply." but the feature doesn't exist.
**Fix:** Either implement the feature or be explicit about the app's current state (e.g., "Beta — Expenses tracking coming in v2").

---

## 🔵 UX / UI ISSUES

### 22. App title doesn't update per page — always "Expenses Manager"
**Detail:** Every page has the same browser tab title "Expenses Manager". This makes it hard to distinguish open tabs and is bad for accessibility (screen readers announce the title on page navigation).
**Fix:** Set per-page titles using React Helmet or `document.title` in each page component, e.g., "Login — Expenses Manager", "Dashboard — Expenses Manager".

---

### 24. Encoding artifacts in source-rendered text (mojibake)
**Detail:** When reading source files, several strings appeared garbled (e.g., `â€"` instead of `—`, `â€¢` instead of `•`). This is a UTF-8 encoding issue in how the source files are served. While the browser renders them correctly (the JS runtime handles them fine), it indicates the source files may have encoding inconsistencies.
**Fix:** Ensure all source files are saved as UTF-8 and use proper Unicode escape sequences or copy-paste characters cleanly.

---

## ⚙️ CODE / ARCHITECTURE ISSUES

### 25. React Router v6 future flag warnings (console)
**Detail:** Two React Router warnings appear on every page load:
- `v7_startTransition` flag not set
- `v7_relativeSplatPath` flag not set

These warnings clutter the console and will become breaking changes in v7.
**Fix:** Add the flags to the `BrowserRouter`: `<BrowserRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>`.

---

### 26. `onUnauthorized` handler is registered inside the component render cycle without cleanup
**File:** `AuthContext.tsx`
**Detail:** `onUnauthorized(() => {...})` is called directly in the component body (not in a `useEffect`), meaning it's called on every render. The handler is set as a module-level variable and not cleaned up.
**Fix:** Call `onUnauthorized(...)` inside a `useEffect` with appropriate cleanup: `useEffect(() => { onUnauthorized(handler); return () => onUnauthorized(null); }, [...])`.

---

### 27. `value` memoization in AuthContext includes function references that recreate on every render
**File:** `AuthContext.tsx`
**Detail:** The `useMemo` for `value` includes `login`, `logout`, `register` etc. in its dependency array, but those functions are recreated on every render (they're not wrapped in `useCallback`). This causes the memoized value to update unnecessarily.
**Fix:** Wrap the auth functions (`login`, `logout`, `register`, `changePassword`, etc.) in `useCallback`.

---

### 28. No rate limiting or brute-force protection visible in the frontend
**Detail:** The login form has no lockout, CAPTCHA, or delay after repeated failed attempts. There is also no client-side attempt counter.
**Fix:** Implement progressive delay or lockout on the client side (e.g., disable the login button for 5s after 5 failed attempts), and ensure the API has rate limiting.

---

## Open Items Summary

| # | Severity | Category | Issue |
|---|----------|----------|-------|
| 11 | 🟡 Moderate | Feature | Core "Expenses" feature is "Coming soon…" |
| 22 | 🔵 UI | SEO/A11y | Tab title doesn't update per page |
| 24 | 🔵 Code | Quality | Encoding artifacts in source strings |
| 25 | ⚙️ Code | Warning | React Router v6 future flag console warnings |
| 26 | ⚙️ Code | Architecture | `onUnauthorized` set outside `useEffect`, no cleanup |
| 27 | ⚙️ Code | Performance | Auth functions not memoized with `useCallback` |
| 28 | ⚙️ Security | Feature | No brute-force/rate-limit protection on login |
