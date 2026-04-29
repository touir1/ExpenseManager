# Expenses Manager — Frontend Dashboard: Fixed Issues
**QA date:** 2026-04-29 | **Source:** [Ongoing QA report](../../ongoing/qa_test_results/2026-04-29-frontend-dashboard-qa.md)

Items below were identified in the 2026-04-29 QA session and subsequently resolved.

---

## F-1 — ~~Duplicate "Authentication token is missing" toasts on every page load~~ ✅ FIXED

**Severity:** Medium  
**Affected routes:** All public pages on every cold load while unauthenticated

**Root cause:** `AuthContext` fires `GET /auth/session` → 401, then `POST /auth/refresh` → 401 on startup. Each 401 fell through to `buildErrorResponse` in `api.service.ts` which called `errorHandler` (toast), even with `skipUnauthorized: true`. Result: two error toasts for fresh unauthenticated visitors.

**Fix applied:**
- Added `silent?: boolean` option to `request()`, `get()`, and `post()` in `api.service.ts`. When `true`, `buildErrorResponse` skips the `errorHandler` call while still returning the `ApiResponse` with the error for callers.
- Applied `{ skipUnauthorized: true, silent: true }` to `sessionCheck()` and `refreshRequest()` in `authApi.service.ts` — these are startup probes expected to fail; toasts are suppressed.
- Two new unit tests added in `api.service.test.ts` covering `silent` flag behaviour.
- `AuthContext.test.tsx` assertion updated to match the new opts (`SKIP_SILENT` constant).
