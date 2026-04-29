# QA Audit Report — Frontend Dashboard
**Date:** 2026-04-29  
**Module:** frontend-dashboard  
**Mode:** FULL (Functional + UX/UI + Security + Destructive)  
**Destructive Testing:** Enabled  
**Target:** https://localhost  
**Email Inbox:** http://localhost:8025 (Mailpit)  
**Tester:** Claude (automated browser QA via claude-in-chrome)

---

## Executive Summary

The Expenses Manager frontend is well-structured and functional. Authentication, session management, email verification, and form validation all work correctly. The UI is clean and consistent. However, several issues were found ranging from a **High** security finding (no rate limiting on login) to **Medium** bugs (duplicate error toasts, raw JSON on invalid verification links) and **Low** accessibility gaps. The Expenses module is a stub ("Coming soon…") so CRUD testing was not possible. No XSS vulnerabilities were found.

| Severity | Count |
|---|---|
| High | 2 |
| Medium | 4 |
| Low | 4 |
| Info | 3 |

---

## Scope

- **Mode:** FULL
- **Destructive testing:** Enabled
- **Pages tested:** `/` (home), `/login`, `/register`, `/reset-password`, `/dashboard`, `/settings`, `/change-password`
- **API endpoints exercised:** `/auth/session`, `/auth/refresh`, `/auth/login`, `/auth/logout`, `/auth/register`, `/auth/validate-email`, `/auth/change-password-reset`
- **Email flows tested:** Registration verification email (Mailpit)
- **Expenses CRUD:** Not testable — feature stub ("Coming soon…")

---

## Functional Issues

### F-3 — Invalid/expired verification link shows raw JSON (Medium)

**Reproduction:**
1. Click an already-used or expired verification link: `GET /api/users/auth/validate-email?h=<used-token>&…`
2. Browser displays: `{"message":"EMAIL_VERIFICATION_FAILED"}`

**Expected:** Redirect to a friendly frontend error page (e.g., `/verify-error`) explaining the link is expired with a call-to-action to request a new one.

**Affected flow:** Email verification → expired/reused token path.

---

### F-4 — No max-length validation on registration name fields (Medium)

**Reproduction:**
1. Navigate to `/register`
2. Enter 256+ character strings in First name / Last name fields and submit

**Observed:** Request accepted (HTTP 200), registration succeeds.

**Expected:** Frontend Zod schema should enforce a maximum length (e.g., 100 chars) to prevent oversized payloads and potential DB truncation.

---

### F-5 — Login/register page buttons unresponsive to coordinate clicks (Low)

**Reproduction:**
1. Navigate to `/` (home page)
2. Click "Sign in" or "Create account" buttons by coordinate

**Observed:** No navigation. Buttons only work when clicked via element reference (they are `<a>` tags, not `<button>` elements).

**Note:** This may be an interaction issue specific to the browser automation extension rather than a real user-facing bug. Requires manual verification.

---

## UX/UI Issues

### U-1 — "Authentication token is missing" is misleading for fresh visitors (Medium)

Same root as F-1. The error message implies the user was previously logged in and their session expired, which is false for first-time visitors. This is confusing and erodes trust.

**Recommendation:** Suppress these toasts entirely on public/auth pages when no prior session existed.

---

### U-2 — Expenses card on dashboard shows "Coming soon…" with no context (Info)

The Expenses card on `/dashboard` is a placeholder. No link, no ETA, no description of what it will do.

**Recommendation:** Add a short descriptive sentence and either disable the card visually or hide it until the feature ships.

---

### U-3 — Email footer shows stale copyright year "© 2024" (Low)

The verification email template footer reads "© 2024 Expenses Manager. All rights reserved." The current year is 2026.

**Recommendation:** Make the year dynamic or update to the current year.

---

### U-4 — No "skip to main content" link (Low)

No skip-navigation link is present on any page.

**Impact:** Keyboard-only and screen reader users must tab through the entire header/nav on every page before reaching main content (WCAG 2.4.1 — Level A).

**Recommendation:** Add `<a href="#main-content" class="sr-only focus:not-sr-only">Skip to content</a>` as the first focusable element in the layout.

---

### U-5 — Show/hide password button missing `aria-pressed` (Low)

The "Show password" toggle button correctly updates its `aria-label` ("Show password" ↔ "Hide password") but does not set `aria-pressed="true/false"`.

**Impact:** Screen readers may not announce the current toggle state.

**Recommendation:** Add `aria-pressed={isVisible}` to the `PasswordInput` component's toggle button.

---

### U-6 — Password reset/create page accessible without a valid token (Info)

Navigating directly to `/reset-password` with a fake hash renders the "Create Password" form. Submitting it calls the API which then rejects the invalid token — but the page itself doesn't pre-validate the URL parameters before rendering the form.

**Recommendation:** Pre-validate the hash server-side on page load and redirect to an error page if invalid, rather than letting the user fill out the form and fail on submit.

---

## Security Findings

### S-1 — No rate limiting on login endpoint (High)

**Test:** 6 concurrent `POST /api/users/auth/login` requests with invalid credentials all returned HTTP 401. No HTTP 429 observed at any attempt count.

**Risk:** An attacker can perform unlimited brute-force password attacks against any known email address. Combined with the public registration form (email enumeration is mitigated), this is a significant risk.

**Recommendation:** Implement rate limiting at the nginx or application level (e.g., 5 failed attempts per email per 15 minutes, with exponential backoff or account lockout).

---

### S-2 — Missing `Content-Security-Policy` header (High)

**Observed headers:**

| Header | Value |
|---|---|
| `Content-Security-Policy` | **MISSING** |
| `X-Content-Type-Options` | **MISSING** |
| `Referrer-Policy` | **MISSING** |
| `Permissions-Policy` | **MISSING** |
| `X-Frame-Options` | `sameorigin` ✓ |
| `Strict-Transport-Security` | `max-age=1209600` ✓ |
| `X-XSS-Protection` | MISSING (deprecated, low priority) |

**Risk:** Without CSP, any injected script (e.g., via a future XSS vulnerability or a compromised CDN) can execute freely. Without `X-Content-Type-Options`, MIME sniffing attacks are possible.

**Recommendation:**
- Add `Content-Security-Policy` with a strict policy (`default-src 'self'`, allow `fonts.googleapis.com` for the Inter font).
- Add `X-Content-Type-Options: nosniff`.
- Add `Referrer-Policy: strict-origin-when-cross-origin`.

---

### S-3 — Auth tokens stored in HttpOnly cookies (Pass ✓)

`document.cookie` returns empty string after login — all auth tokens are HttpOnly and not accessible to JavaScript. Session cookies are correctly used with `credentials: 'include'`.

---

### S-4 — XSS: URL parameters properly escaped by React (Pass ✓)

Injecting `<script>alert('xss')</script>` via the `?email=` URL parameter on `/reset-password` did not execute. React's DOM rendering correctly escapes the value.

---

### S-5 — Verification token single-use enforced (Pass ✓)

Re-using an already-consumed verification token returns `{"message":"EMAIL_VERIFICATION_FAILED"}`. Tokens are correctly invalidated after first use.

---

### S-6 — Duplicate registration uses intentional anti-enumeration pattern (Info)

Registering with an already-used email returns "Registration successful!" (HTTP 200) rather than an error. This prevents email enumeration. Intentional design per security best practices.

---

## Destructive Test Results

| Test | Input | Result |
|---|---|---|
| Empty login form | — | Inline validation errors shown correctly ✓ |
| Empty register form | — | All 3 fields show errors correctly ✓ |
| Invalid email format | `notanemail` | "Please enter a valid email address." ✓ |
| Mismatched passwords | `TestPass123!` vs `Different123!` | "New passwords do not match." ✓ |
| 256-char name fields | 256× 'A', 256× 'B' | Accepted without validation error ✗ (F-4) |
| Unicode / emoji names | `محمد علي 中文 🔥💀`, `Ünïcödé` | Accepted and processed correctly ✓ |
| Rapid login (6 concurrent) | Wrong credentials | All 6 returned 401, no 429 ✗ (S-1) |
| Token reuse | Consumed hash | `EMAIL_VERIFICATION_FAILED` ✓ |
| Submit during loading | Rapid double-click | Button disabled during submission ✓ |

---

## Email Testing (Mailpit)

| Check | Result |
|---|---|
| Email received after registration | ✓ Delivered within ~3s |
| Sender address | `noreply@expenses-manager.local` ✓ |
| Subject | `[Expenses Manager] Email Verification` ✓ |
| Verify Email button present | ✓ |
| Link domain | `https://localhost` (correct for local env) ✓ |
| Link parameters | `h` (hash), `s` (email), `app_code=EXPENSES_MANAGER` ✓ |
| HTML render | Clean, readable ✓ |
| Copyright year in footer | `© 2024` — stale (U-3) ✗ |
| Password reset email | Present in inbox from prior test ✓ |
| Duplicate registration email | Re-sends verification (consistent with anti-enumeration) ✓ |

---

## Performance Notes

- **Initial load:** Single JS bundle (`index-BnTg6gw-.js`) — no code splitting observed. For a small app this is acceptable, but as the Expenses feature grows, consider route-based splitting.
- **Font:** Google Fonts (Inter) loaded from external CDN — adds a network dependency and a potential CSP constraint.
- **Session check on load:** `GET /session` + `POST /refresh` (2 requests) fire on every cold load. This adds ~2 API round-trips before the user sees their state. Acceptable for now.

---

## Recommendations (Priority Order)

1. **[High]** Add rate limiting to `POST /auth/login` — 5 attempts / email / 15 min minimum.
2. **[High]** Add `Content-Security-Policy`, `X-Content-Type-Options`, `Referrer-Policy` headers in nginx config.
3. **[Medium]** Fix double error toasts: suppress "Authentication token is missing" toasts on public pages for fresh unauthenticated sessions (F-1); deduplicate login error toasts (F-2).
4. **[Medium]** Redirect invalid/expired verification token API calls to a frontend error page instead of returning raw JSON (F-3).
5. **[Medium]** Add `maxLength` Zod validation (e.g., 100 chars) to first name / last name fields in the register schema (F-4).
6. **[Low]** Add `<a href="#main-content">Skip to content</a>` skip-nav link (U-4).
7. **[Low]** Add `aria-pressed` to the show/hide password toggle button (U-5).
8. **[Low]** Update email template copyright year to dynamic current year (U-3).
9. **[Info]** Pre-validate reset token on page load and redirect to error page if invalid (U-6).
10. **[Info]** Add description or hide Expenses "Coming soon…" card until feature ships (U-2).
