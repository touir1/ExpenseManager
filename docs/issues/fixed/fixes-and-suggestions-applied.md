# Applied Fixes & Suggestions

A record of improvement ideas from [fixes-and-suggestions.md](../ongoing/fixes-and-suggestions.md) that have been implemented. Move items here from the ongoing file once they are shipped, and record the version and date they landed.

---

## Frontend

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
