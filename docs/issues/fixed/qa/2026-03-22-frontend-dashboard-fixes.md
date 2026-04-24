# Expenses Manager — Frontend Dashboard: Fixed Issues
**QA date:** 2026-03-22 | **Source:** [Ongoing QA report](../../ongoing/qa_test_results/2026-03-22-frontend-dashboard-qa.md)

All items below were identified in the 2026-03-22 QA session and subsequently resolved.

---

## 🔴 CRITICAL BUGS

### 1. ~~Login with wrong credentials shows no error and silently redirects to `/login`~~ ✅ FIXED
**Page:** `/login`
**Root cause:** When the login API returns `401`, the global `onUnauthorized` handler in `src/services/api.ts` immediately calls `window.location.assign('/login')` and clears auth state. This fires *before* the `login()` function in `AuthContext` can return `false`, so the Login component's error state is never reached. The user gets silently redirected to a blank login form with zero feedback.
**Fix applied:** Added `skipUnauthorized?: boolean` option to `request()` and `post()` in `src/services/api.ts`. All auth endpoint calls in `AuthContext` (`login`, `register`, `changePassword`, `resetPassword`, `requestPasswordReset`) now pass `{ skipUnauthorized: true }`, preventing the global redirect handler from firing on expected 401 responses.

---

### 2. ~~Register form submits with empty/partial fields and silently redirects to `/login`~~ ✅ FIXED
**Page:** `/register`
**Root cause:** The form uses `noValidate`, disabling all HTML5 validation. When fields are empty or partially filled, the `register()` function calls the API. If the API returns `401`, the `onUnauthorized` handler fires, redirecting immediately before any feedback can be shown.
**Fix applied:** Added client-side validation in `Register.tsx` `onSubmit`: checks for empty fields ("All fields are required.") and invalid email format ("Please enter a valid email address.") before calling the API. Combined with the fix from #1, the silent redirect no longer occurs.

---

### 3. ~~JWT token stored in `localStorage` — XSS vulnerability~~ ✅ FIXED
**File:** `src/features/auth/AuthContext.tsx`
**Root cause:** `localStorage.setItem('auth:token', token)` stores the JWT in localStorage, accessible to any JavaScript on the page.
**Fix applied:** Migrated to HttpOnly cookie-based authentication. The backend (`AuthenticationController`) now sets an `auth_token` cookie with `HttpOnly`, `SameSite=Strict`, and `Secure` flags on login, and clears it on logout. A new `GET /auth/session` endpoint validates the cookie for session restore. The frontend no longer stores or reads any token — `src/services/api.ts` uses `credentials: 'include'` on all requests. `AuthContext` retains only user info (name/email) in localStorage for UI display; session validity is always verified via the cookie.

---

## 🟠 MAJOR BUGS

### 4. ~~`onUnauthorized` handler fires for all 401 responses, including intentional login failures~~ ✅ FIXED
**File:** `src/services/api.ts`
**Root cause:** The handler calls `window.location.assign('/login')` globally on every 401, breaking auth-related flows where a 401 is expected and meaningful.
**Fix applied:** See #1 — the `skipUnauthorized` option in `src/services/api.ts` resolves this. Auth functions now opt out of the global handler.

---

### 5. ~~Register form bypasses `required` validation due to `noValidate`~~ ✅ FIXED
**File:** `Register.tsx`
**Root cause:** The form has `noValidate` and all inputs have `required` in JSX, but there is no corresponding JS validation in `onSubmit`. Submitting with all fields empty called the backend.
**Fix applied:** See #2 — JS validation added to `onSubmit` in `Register.tsx`.

---

### 6. ~~Change Password error message is generic — no differentiation between empty fields vs. mismatched passwords~~ ✅ FIXED
**Page:** `/change-password`
**Root cause:** Both "fields are empty" and "passwords don't match" showed the same message: "Please verify inputs."
**Fix applied:** Added client-side validation in `onSubmit` for both `ChangePassword.tsx` and `ResetPassword.tsx`. Empty fields → "All fields are required." Mismatched passwords → "New passwords do not match." API failure → "Incorrect current password." / "Password reset failed. Please try again."

---

### 7. ~~Route `/dashboard` doesn't exist for authenticated users~~ ✅ FIXED
**Root cause:** The authenticated route was `/home`; navigating to `/dashboard` showed `HomePublic` inside the authenticated navbar — a broken half-state.
**Fix applied:** Renamed the route from `/home` to `/dashboard` throughout the frontend — `App.tsx`, `NavBar.tsx`, `PublicOnlyRoute.tsx`, and `Login.tsx`. Also resolves #30.

---

### 8. ~~`*` (catch-all / 404) route renders the public landing page — no 404 page exists~~ ✅ FIXED
**File:** `App.tsx`
**Root cause:** `<Route path="*" element={<HomePublic />} />` silently rendered the public home for any unknown URL.
**Fix applied:** Created `NotFound.tsx` with a "Page not found" heading and a "Go to home" link. The `*` catch-all route now renders `<NotFound />`.

---

## 🟡 MODERATE ISSUES

### 9. ~~"Request Password Reset" page is accessible while logged in~~ ✅ FIXED
**Pages:** `/request-password-reset`, `/reset-password`
**Root cause:** Neither page was wrapped in `PublicOnlyRoute`, so authenticated users could access them.
**Fix applied:** Both routes wrapped in `<PublicOnlyRoute>` in `App.tsx`. Authenticated users are now redirected to `/dashboard`.

---

### 10. ~~Registration success message is never visible — auto-redirects too fast~~ ✅ FIXED
**File:** `Register.tsx`
**Root cause:** `setTimeout(() => navigate('/login'), 800)` redirected after only 800ms, too fast to read, and the message rendered below the submit button which may not have been in view.
**Fix applied:** Removed the auto-redirect entirely. On success, the form is replaced by a full-card success state showing the message prominently with a manual "Go to login →" link. The message was also corrected: the previous text ("You can now log in.") was factually wrong — the user must first click a verification link from their inbox before they can log in.

---

### 12. ~~Duplicate logout mechanisms — inconsistent post-logout destination~~ ✅ FIXED
**Pages:** `/dashboard`
**Root cause:** Two logout actions existed — "Sign out" in the navbar and "Logout" on the dashboard body — with different redirect destinations.
**Fix applied:** Removed the "Logout" button from `HomeDashboard.tsx`. Sign out is now available exclusively via the navbar.

---

### 13. ~~"Settings" nav link goes directly to `/change-password`, not a settings page~~ ✅ FIXED
**Root cause:** The nav item labeled "Settings" routed directly to `/change-password`, which is just one sub-action.
**Fix applied:** Created `Settings.tsx` at `/settings` — a proper settings page with a "Password" card linking to `/change-password`. The navbar "Settings" link now points to `/settings`, with active-state highlighting covering both `/settings` and `/change-password`. The Change Password page gained a "← Back to settings" link (also resolves #15).

---

### 14. ~~Request Password Reset page has no "Back to login" link~~ ✅ FIXED
**Page:** `/request-password-reset`
**Root cause:** No navigation link to return without using the browser back button or clicking the logo.
**Fix applied:** Added a "← Back to login" link below the form in `RequestPasswordReset.tsx`.

---

### 15. ~~Change Password page has no "Back to settings" link~~ ✅ FIXED
**Page:** `/change-password`
**Root cause:** No navigation link to return after reaching the page.
**Fix applied:** Added a "← Back to settings" link at the top of `ChangePassword.tsx`, resolved as part of #13.

---

## 🔵 UX / UI ISSUES

### 16. ~~Hamburger menu button present in DOM but mobile Settings link went to wrong page~~ ✅ FIXED
**File:** `src/layouts/NavBar.tsx`
**Root cause:** The mobile menu's Settings link pointed to `/change-password` instead of `/settings` — a regression from #13.
**Fix applied:** Corrected the mobile Settings link from `to="/change-password"` to `to="/settings"` in `src/layouts/NavBar.tsx`. Added a test asserting the mobile Settings link href is `/settings`.

---

### 17. ~~Landing page has a lot of empty whitespace — content is not vertically centered~~ ✅ FIXED
**Page:** `/`
**Root cause:** The layout used `flex-1 flex flex-col` without centering.
**Fix applied:** `HomePublic` uses the `.auth-page` shared class (`flex-1 flex items-center justify-center`) which centers the hero content within the full remaining viewport height below the navbar. Test added to verify the class is applied.

---

### 18. ~~Password field placeholders use literal bullet characters (`••••••••`)~~ ✅ FIXED
**Root cause:** All password inputs used `placeholder="••••••••"`, a Unicode workaround that displays inconsistently across browsers/fonts.
**Fix applied:** Replaced with `placeholder=""` on all six password inputs across `Login.tsx`, `ChangePassword.tsx`, and `ResetPassword.tsx`. The `type="password"` attribute provides the masking; no placeholder text is needed.

---

### 19. ~~No "show/hide password" toggle on any password field~~ ✅ FIXED
**Root cause:** None of the password fields had a toggle to reveal the password.
**Fix applied:** Created a reusable `PasswordInput` component (`src/components/PasswordInput.tsx`) with an absolute-positioned eye-icon toggle button. All six password fields across `Login.tsx`, `ChangePassword.tsx`, and `ResetPassword.tsx` now use this component. Full unit test suite added in `src/components/__tests__/PasswordInput.test.tsx`.

---

### 20. ~~No loading/spinner state on the Login button~~ ✅ FIXED
**File:** `Login.tsx`
**Root cause:** The Login page had no submission feedback while the request was in flight, unlike `RequestPasswordReset.tsx`.
**Fix applied:** Added `submitting` state to `Login.tsx`. The Login button is now disabled and shows a spinner with "Signing in…" while the request is in flight. The email input is also disabled during submission.

---

### 21. ~~No loading state on Register button~~ ✅ FIXED
**File:** `Register.tsx`
**Root cause:** Same as #20 — no loading indicator during registration submission.
**Fix applied:** Added `submitting` state to `Register.tsx`. The Register button is disabled and shows a spinner with "Submitting…" while the request is in flight. All three inputs are disabled during submission.

---

### 22. ~~App title doesn't update per page — always "Expenses Manager"~~ ✅ FIXED
**Root cause:** All page components rendered without setting `document.title`, so the browser tab always showed the static value from `index.html`.
**Fix applied:** Created a `usePageTitle` hook (`src/hooks/usePageTitle.ts`) that calls `document.title` in a `useEffect`. Applied to all 9 page components with per-page titles: "Login — Expenses Manager", "Register — Expenses Manager", "Dashboard — Expenses Manager", "Settings — Expenses Manager", "Change Password — Expenses Manager", "Reset Password — Expenses Manager", "Create Password — Expenses Manager" (when `?mode=create`), "Request Password Reset — Expenses Manager", "Page Not Found — Expenses Manager". The home/landing page retains just "Expenses Manager".

---

### 23. ~~No `<meta name="description">` or Open Graph tags~~ ✅ FIXED
**Root cause:** The app had only `charset` and `viewport` meta tags.
**Fix applied:** Added to `index.html`: `<meta name="description">`, `<meta name="robots">`, Open Graph tags (`og:type`, `og:title`, `og:description`, `og:image`), and Twitter Card tags.

---

## ⚙️ CODE / ARCHITECTURE ISSUES

### 25. ~~React Router v6 future flag warnings (console)~~ ✅ FIXED
**Detail:** Two React Router warnings appeared on every page load: `v7_startTransition` flag not set and `v7_relativeSplatPath` flag not set. These cluttered the console and would become breaking changes in v7.
**Fix applied:** Added `future={{ v7_startTransition: true, v7_relativeSplatPath: true }}` to `<BrowserRouter>` in `src/App.tsx`.

---

### 29. ~~No password strength indicator or minimum length requirement shown to users~~ ✅ FIXED
**Root cause:** No indication of what constitutes a valid password and no client-side length check on Change Password and Reset Password pages.
**Fix applied:** Added a reusable `PasswordStrength` component (`src/components/PasswordStrength.tsx`) that displays below the "New password" field on both pages. It shows a 5-segment colour bar and a live checklist of five criteria (≥ 8 characters, uppercase, lowercase, number, special character) with a Weak / Fair / Good / Strong label. Client-side validation now rejects passwords shorter than 8 characters before calling the API.

---

### 30. ~~App URL is `/home` but the nav calls it "Dashboard" — naming inconsistency~~ ✅ FIXED
**Root cause:** The route is `/home`, the page heading says "Dashboard", the nav link says "Dashboard" and links to `/home`, and `/dashboard` goes to a broken state.
**Fix applied:** See #7 — route renamed to `/dashboard`, resolving the inconsistency.

---

## Fixed Items Summary

| # | Severity | Category | Issue |
|---|----------|----------|-------|
| 1 | 🔴 Critical | Bug | ~~Wrong login credentials show no error, silent redirect~~ |
| 2 | 🔴 Critical | Bug | ~~Register submits with empty/partial fields silently~~ |
| 3 | 🔴 Critical | Security | ~~JWT stored in localStorage (XSS risk)~~ |
| 4 | 🟠 Major | Bug | ~~Global `onUnauthorized` fires on ALL 401s including login~~ |
| 5 | 🟠 Major | Bug | ~~Register: `noValidate` + no JS validation = no error~~ |
| 6 | 🟠 Major | UX | ~~Generic error messages on Change/Reset Password~~ |
| 7 | 🟠 Major | Routing | ~~`/dashboard` route does not exist~~ |
| 8 | 🟠 Major | Routing | ~~No 404 page — unknown routes silently show landing page~~ |
| 9 | 🟡 Moderate | Security/UX | ~~Reset password pages accessible while logged in~~ |
| 10 | 🟡 Moderate | UX | ~~Registration success message not readable before redirect~~ |
| 12 | 🟡 Moderate | UX | ~~Duplicate logout buttons with inconsistent destinations~~ |
| 13 | 🟡 Moderate | UX | ~~"Settings" nav leads to change-password, not a settings page~~ |
| 14 | 🟡 Moderate | UX | ~~No "back" link on Request Password Reset page~~ |
| 15 | 🟡 Moderate | UX | ~~No "back" link on Change Password page~~ |
| 16 | 🔵 UI | Responsive | ~~Mobile Settings link pointed to wrong page~~ |
| 17 | 🔵 UI | Layout | ~~Landing page content not vertically centered~~ |
| 18 | 🔵 UI | Polish | ~~Password placeholders use Unicode bullets~~ |
| 19 | 🔵 UI | Accessibility | ~~No show/hide toggle on password fields~~ |
| 20 | 🔵 UI | UX | ~~No loading spinner on Login button~~ |
| 21 | 🔵 UI | UX | ~~No loading spinner on Register button~~ |
| 22 | 🔵 UI | Accessibility/UX | ~~App title always "Expenses Manager", no per-page title~~ |
| 23 | 🔵 UI | SEO | ~~No `<meta description>` or Open Graph tags~~ |
| 25 | ⚙️ Code | Warning | ~~React Router v6 future flag console warnings~~ |
| 29 | ⚙️ Code | Security | ~~No password strength indicator or minimum length~~ |
| 30 | ⚙️ Code | Architecture | ~~Route `/home` vs. UI label "Dashboard" inconsistency~~ |
