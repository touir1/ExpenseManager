# Applied Fixes & Suggestions

A record of improvement ideas from [fixes-and-suggestions.md](../ongoing/fixes-and-suggestions.md) that have been implemented. Move items here from the ongoing file once they are shipped, and record the version and date they landed.

---

## Frontend

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
| High | No dedicated `types/` directory — types scattered inline across files | Created `src/types/auth.ts` (`User`, `AuthContextValue`) and `src/types/api.ts` (`ApiResponse<T>`) |
| Medium | `AuthContext` makes HTTP calls directly — mixes concerns | All HTTP auth calls extracted to `src/services/authApi.ts`; `AuthContext` now handles state only |
| Low | `NavBar.tsx` had no active-link highlighting via proper API | Replaced `Link` with `NavLink` from react-router-dom; shared `navLinkClass` helper; Settings keeps custom active logic for `/change-password` match |
| Low | `api.ts` error messages were hardcoded strings | Extracted to `src/constants/apiErrors.ts` as `API_ERRORS` typed constants |

All 205 existing tests continued to pass after these changes.

---

## Backend

*No applied suggestions yet.*

---

## Infrastructure

*No applied suggestions yet.*
