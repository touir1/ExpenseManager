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

## ⚙️ CODE / ARCHITECTURE ISSUES

### 28. No rate limiting or brute-force protection visible in the frontend
**Detail:** The login form has no lockout, CAPTCHA, or delay after repeated failed attempts. There is also no client-side attempt counter.
**Fix:** Implement progressive delay or lockout on the client side (e.g., disable the login button for 5s after 5 failed attempts), and ensure the API has rate limiting.

---

## Open Items Summary

| # | Severity | Category | Issue |
|---|----------|----------|-------|
| 11 | 🟡 Moderate | Feature | Core "Expenses" feature is "Coming soon…" |
| 28 | ⚙️ Security | Feature | No brute-force/rate-limit protection on login |
