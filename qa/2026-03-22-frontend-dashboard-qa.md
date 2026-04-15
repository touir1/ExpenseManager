Here is the full QA report based on thorough testing of the application, including source code review, functional testing, security analysis, and UX evaluation.

---

# Expenses Manager — Full QA Report

---

## 🔴 CRITICAL BUGS

### 1. ~~Login with wrong credentials shows no error and silently redirects to `/login`~~ ✅ FIXED
**Page:** `/login`
**Root cause (confirmed via source):** When the login API returns `401`, the global `onUnauthorized` handler in `api.ts` immediately calls `window.location.assign('/login')` and clears auth state. This fires *before* the `login()` function in `AuthContext` can return `false`, so the Login component's error state (`setError('Invalid credentials...')`) is never reached. The user gets silently redirected to a blank login form with zero feedback.
**Fix applied:** Added `skipUnauthorized?: boolean` option to `request()` and `post()` in `api.ts`. All auth endpoint calls in `AuthContext` (`login`, `register`, `changePassword`, `resetPassword`, `requestPasswordReset`) now pass `{ skipUnauthorized: true }`, preventing the global redirect handler from firing on expected 401 responses.

---

### 2. ~~Register form submits with empty/partial fields and silently redirects to `/login`~~ ✅ FIXED
**Page:** `/register`
**Root cause:** The form uses `noValidate`, disabling all HTML5 validation. When fields are empty or partially filled, the `register()` function calls the API. If the API returns `401` (which it does for invalid registrations), the same `onUnauthorized` handler fires, redirecting immediately before any feedback can be shown. Submitting with only "First name" filled (no last name, no email) was tested and it silently redirected.
**Fix applied:** Added client-side validation in `Register.tsx` `onSubmit`: checks for empty fields ("All fields are required.") and invalid email format ("Please enter a valid email address.") before calling the API. Combined with the fix from Bug #1, the silent redirect no longer occurs.

---

### 3. ~~JWT token stored in `localStorage` — XSS vulnerability~~ ✅ FIXED
**File:** `AuthContext.tsx`
**Detail:** `localStorage.setItem('auth:token', token)` stores the JWT in localStorage, which is accessible to any JavaScript running on the page. If an XSS vulnerability is ever introduced, an attacker can steal the token and impersonate the user.
**Fix applied:** Migrated to HttpOnly cookie-based authentication. The backend (`AuthenticationController`) now sets an `auth_token` cookie with `HttpOnly`, `SameSite=Strict`, and `Secure` flags on login, and clears it on logout. A new `GET /auth/session` endpoint validates the cookie for session restore. The frontend no longer stores or reads any token — `api.ts` uses `credentials: 'include'` on all requests. `AuthContext` retains only user info (name/email) in localStorage for UI display; session validity is always verified via the cookie.

---

## 🟠 MAJOR BUGS

### 4. ~~`onUnauthorized` handler fires for all 401 responses, including intentional login failures~~ ✅ FIXED
**File:** `api.ts`
**Detail:** The handler calls `window.location.assign('/login')` globally whenever any request returns 401. This breaks all auth-related flows (login, register, change-password when the old password is wrong) because a 401 from those endpoints is expected and meaningful, not a "session expired" event.
**Fix applied:** See Bug #1 — the `skipUnauthorized` option in `api.ts` resolves this. Auth functions now opt out of the global handler.

---

### 5. ~~Register form bypasses `required` validation due to `noValidate`~~ ✅ FIXED
**File:** `Register.tsx`
**Detail:** The form has `noValidate` and all three inputs have `required` in JSX, but there is no corresponding JavaScript validation in `onSubmit`. Submitting the form with all fields empty calls the backend. This causes a 401 (and the silent redirect), instead of showing a proper client-side error.
**Fix applied:** See Bug #2 — JS validation added to `onSubmit` in `Register.tsx`.

---

### 6. ~~Change Password error message is generic — no differentiation between empty fields vs. mismatched passwords~~ ✅ FIXED
**Page:** `/change-password`
**Detail:** Both "fields are empty" and "passwords don't match" show the exact same message: "Please verify inputs." This gives users no actionable guidance.
**Fix applied:** Added client-side validation in `onSubmit` for both `ChangePassword.tsx` and `ResetPassword.tsx`. Empty fields → "All fields are required." Mismatched passwords → "New passwords do not match." API failure → "Incorrect current password." / "Password reset failed. Please try again."

---

### 7. ~~Route `/dashboard` doesn't exist for authenticated users~~ ✅ FIXED
**Detail:** When logged in and navigating to `/dashboard` (a very common, expected URL for a dashboard app), the app shows the public landing page (`HomePublic`) inside the authenticated navbar — a broken, confusing half-state. The authenticated route is `/home`, which is non-obvious.
**Fix applied:** Renamed the route from `/home` to `/dashboard` throughout the frontend — `App.tsx`, `NavBar.tsx`, `PublicOnlyRoute.tsx`, and `Login.tsx`. All redirect targets, logo links, and nav links updated. Also resolves naming inconsistency #30.

---

### 8. ~~`*` (catch-all / 404) route renders the public landing page — no 404 page exists~~ ✅ FIXED
**File:** `App.tsx`
**Detail:** `<Route path="*" element={<HomePublic />} />` silently renders the public home page for any unknown URL. Users who mistype a URL get no indication that the page doesn't exist.
**Fix applied:** Created `NotFound.tsx` with a "Page not found" heading and a "Go to home" link. The `*` catch-all route in `App.tsx` now renders `<NotFound />` instead of `<HomePublic />`.

---

## 🟡 MODERATE ISSUES

### 9. ~~"Request Password Reset" page is accessible while logged in~~ ✅ FIXED
**Pages:** `/request-password-reset`, `/reset-password`
**Detail:** Neither page is wrapped in `PublicOnlyRoute`, so authenticated users can access them. This creates a confusing UX (the navbar shows "Dashboard / Settings / Sign out" but the page is a password-reset form for unauthenticated flows).
**Fix applied:** Both routes wrapped in `<PublicOnlyRoute>` in `App.tsx`. Authenticated users are now redirected to `/dashboard`.

---

### 10. ~~Registration success message is never visible — auto-redirects too fast~~ ✅ FIXED
**File:** `Register.tsx`
**Detail:** On successful registration, `setMessage('Registered successfully. You can now log in.')` was set, but `setTimeout(() => navigate('/login'), 800)` redirected after only 800ms — too fast to read, and the message rendered below the submit button which may not have been in view.
**Fix applied:** Removed the auto-redirect entirely. On success, the form is replaced by a full-card success state showing the message prominently and a manual "Go to login →" link, giving the user full control over when to proceed. The success message was also corrected: the previous text ("You can now log in.") was factually wrong — registration creates an unvalidated account with no password. The user must click the verification link sent to their inbox, which redirects to `/reset-password` to set a password before they can log in. The new message reflects this flow.

---

### 11. Dashboard page says "Expenses — Coming soon…" for the core feature
**Page:** `/home`
**Detail:** The entire expenses management feature — which is the core purpose of the app — is marked as "Coming soon…" with no further indication of timeline or workaround. The app's tagline is "Track your expenses, simply." but the feature doesn't exist.
**Fix:** Either implement the feature or be explicit about the app's current state (e.g., "Beta — Expenses tracking coming in v2").

---

### 12. ~~Duplicate logout mechanisms — inconsistent post-logout destination~~ ✅ FIXED
**Pages:** `/dashboard`
**Detail:** There were two logout actions: "Sign out" in the navbar and "Logout" button on the dashboard page body, with different redirect destinations.
**Fix applied:** Removed the "Logout" button from `HomeDashboard.tsx`. Sign out is now available exclusively via the navbar.

---

### 13. "Settings" nav link goes directly to `/change-password`, not a settings page
**Detail:** The nav item labeled "Settings" routes to `/change-password`, which is just one sub-action. This is conceptually wrong and will become a problem when more settings are added.
**Fix:** Either rename the nav link to "Change Password", or create a proper `/settings` page with "Change Password" as a section within it.

---

### 14. Request Password Reset page has no "Back to login" link
**Page:** `/request-password-reset`
**Detail:** Unlike the Login page (which has "← Back to home"), the Request Password Reset page offers no way to navigate back without using the browser's back button or clicking the logo.
**Fix:** Add a `← Back to login` link below the form.

---

### 15. Change Password page has no "Back to dashboard" link
**Page:** `/change-password`
**Detail:** After reaching the page (via the "Change Password" link on the dashboard), there is no navigation link to return. Users must use the navbar or browser back.
**Fix:** Add a `← Back to dashboard` link.

---

## 🔵 UX / UI ISSUES

### 16. Hamburger menu button is present in DOM but hidden at all tested viewport sizes
**File:** `NavBar.tsx`
**Detail:** The toggle button is `sm:hidden` (only shown below 640px CSS pixels). However, in testing at 375–390px viewport width, the browser's actual rendered size stayed above the breakpoint due to `window.innerWidth` vs. CSS viewport differences. The desktop nav (`hidden sm:flex`) was always showing. The mobile menu worked in the source code but was never visually triggered.
**Fix:** Test the mobile breakpoint in actual device emulation. Verify the `sm:hidden` / `hidden sm:flex` breakpoint is applied consistently with the actual viewport meta tag.

---

### 17. Landing page has a lot of empty whitespace — content is not vertically centered
**Page:** `/`
**Detail:** The hero content (icon, heading, buttons) sits in the lower third of the viewport with large empty space above. The layout uses `flex-1 flex flex-col` but the centering is missing.
**Fix:** Add `items-center justify-center` to the main container to vertically center the content.

---

### 18. Password field placeholders use literal bullet characters (`••••••••`) rather than proper placeholder styling
**Detail:** All password inputs use `placeholder="••••••••"` which is a Unicode workaround. This can display inconsistently across browsers/fonts and looks slightly off.
**Fix:** Use an empty placeholder or a text-based one like `placeholder="Enter your password"`. The password masking comes from `type="password"` anyway.

---

### 19. No "show/hide password" toggle on any password field
**Detail:** None of the password fields (login, change password, reset password) have an eye icon or toggle to reveal the password. This is now a standard accessibility/UX feature.
**Fix:** Add a show/hide toggle button to the right of password fields.

---

### 20. No loading/spinner state on the Login button
**File:** `Login.tsx`
**Detail:** The "Request Password Reset" page has a loading spinner and disables the button while submitting, but the Login page does not. Users clicking Login on a slow connection have no feedback that the request is in progress.
**Fix:** Add `submitting` state to the Login page, disable the button, and show a spinner identical to the one in `RequestPasswordReset.tsx`.

---

### 21. No loading state on Register button
**File:** `Register.tsx`
**Detail:** Same as above — no loading indicator during registration submission.
**Fix:** Same solution as #20.

---

### 22. App title doesn't update per page — always "Expenses Manager"
**Detail:** Every page has the same browser tab title "Expenses Manager". This makes it hard to distinguish open tabs and is bad for accessibility (screen readers announce the title on page navigation).
**Fix:** Set per-page titles using React Helmet or `document.title` in each page component, e.g., "Login — Expenses Manager", "Dashboard — Expenses Manager".

---

### 23. No `<meta name="description">` or Open Graph tags
**Detail:** The app has only `charset` and `viewport` meta tags. There are no SEO/OG description tags.
**Fix:** Add at minimum `<meta name="description" content="...">`.

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

### 26. `onUnauthorized` handler is registered with `onUnauthorized(...)` inside the component render cycle without cleanup
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

### 29. No password strength indicator or minimum length requirement shown to users
**Detail:** On the Register page (which creates a new account via passwordless flow), and on the Change Password page, there is no indication of what constitutes a valid password. The `changePassword` function in `AuthContext` performs no length check.
**Fix:** Add a minimum password length check (e.g., 8 characters) client-side and display a hint below the field.

---

### 30. ~~App URL is `/home` but the nav calls it "Dashboard" — naming inconsistency~~ ✅ FIXED
**Detail:** The route is `/home`, the page heading says "Dashboard", the nav link says "Dashboard" and links to `/home`, and the logo also links to `/home` when authenticated. The URL `/dashboard` goes to a broken state.
**Fix applied:** See Bug #7 — route renamed to `/dashboard`, resolving the inconsistency.

---

## Summary Table

| # | Severity | Category | Issue |
|---|----------|----------|-------|
| 1 | ✅ Fixed | Bug | ~~Wrong login credentials show no error, silent redirect~~ |
| 2 | ✅ Fixed | Bug | ~~Register submits with empty/partial fields silently~~ |
| 3 | ✅ Fixed | Security | ~~JWT stored in localStorage (XSS risk)~~ |
| 4 | ✅ Fixed | Bug | ~~Global `onUnauthorized` fires on ALL 401s including login~~ |
| 5 | ✅ Fixed | Bug | ~~Register: `noValidate` + no JS validation = no error~~ |
| 6 | ✅ Fixed | UX | ~~Generic error messages on Change/Reset Password~~ |
| 7 | ✅ Fixed | Routing | ~~`/dashboard` route does not exist~~ |
| 8 | ✅ Fixed | Routing | ~~No 404 page — unknown routes silently show landing page~~ |
| 9 | ✅ Fixed | Security/UX | ~~Reset password pages accessible while logged in~~ |
| 10 | ✅ Fixed | UX | ~~Registration success message not readable before redirect~~ |
| 11 | 🟡 Moderate | Feature | Core "Expenses" feature is "Coming soon…" |
| 12 | ✅ Fixed | UX | ~~Duplicate logout buttons with inconsistent destinations~~ |
| 13 | 🟡 Moderate | UX | "Settings" nav leads to change-password, not a settings page |
| 14 | 🟡 Moderate | UX | No "back" link on Request Password Reset page |
| 15 | 🟡 Moderate | UX | No "back" link on Change Password page |
| 16 | 🔵 UI | Responsive | Hamburger menu not triggering at mobile breakpoint |
| 17 | 🔵 UI | Layout | Landing page content not vertically centered |
| 18 | 🔵 UI | Polish | Password placeholders use Unicode bullets |
| 19 | 🔵 UI | Accessibility | No show/hide toggle on password fields |
| 20 | 🔵 UI | UX | No loading spinner on Login button |
| 21 | 🔵 UI | UX | No loading spinner on Register button |
| 22 | 🔵 UI | SEO/A11y | Tab title doesn't update per page |
| 23 | 🔵 UI | SEO | No `<meta description>` or Open Graph tags |
| 24 | 🔵 Code | Quality | Encoding artifacts in source strings |
| 25 | ⚙️ Code | Warning | React Router v6 future flag console warnings |
| 26 | ⚙️ Code | Architecture | `onUnauthorized` set outside `useEffect`, no cleanup |
| 27 | ⚙️ Code | Performance | Auth functions not memoized with `useCallback` |
| 28 | ⚙️ Security | Feature | No brute-force/rate-limit protection on login |
| 29 | ⚙️ Security | Feature | No password strength indicator or minimum length |
| 30 | ✅ Fixed | Architecture | ~~Route `/home` vs. UI label "Dashboard" inconsistency~~ |