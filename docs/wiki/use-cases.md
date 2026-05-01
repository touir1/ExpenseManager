# Use Cases

← [Wiki Index](./index.md)

---

## Overview

ExpenseManager is a personal expense tracking application. It provides a secure user identity system, an expense management domain, and a web dashboard. The use cases below describe what a user can do, organized by domain.

---

## UC-01 — Register an Account

**Actor:** Unregistered visitor  
**Precondition:** User has no account

**Flow:**
1. User navigates to `/register`
2. User fills in first name, last name, and email (all required; max 100 chars each)
3. Frontend validates with Zod schema; shows inline errors on failure
4. On submit: `POST /api/users/auth/register`
5. Backend creates a user record (unverified), generates an `EmailValidationHash`, assigns the default application role, and sends a verification email
6. Frontend shows success message instructing the user to check their email
7. User receives an email with a verification link

**Validation rules:**
- First name, last name, email: required, max 100 characters
- Email must be unique

**Error messages (wire format):**
- `MISSING_PARAMETERS` — required field is empty
- `FIELD_TOO_LONG` — field exceeds 100 characters

---

## UC-02 — Verify Email Address

**Actor:** Registered user (email unverified)  
**Precondition:** Registration email has been sent

**Flow:**
1. User clicks the verification link in their email
2. `GET /api/users/auth/validate-email?h=…&s=…&app_code=…`
3. Backend sets `IsEmailValidated = true` on the user record
4. Backend redirects to the "create password" page: `{ResetPasswordUrlPath}?email=…&h=…&mode=create`
5. If the link is expired or invalid, and the app has `VerifyEmailErrorUrlPath` configured, user is redirected to `/verify-error` (friendly error page)

---

## UC-03 — Create Initial Password

**Actor:** Newly verified user  
**Precondition:** Email verified (UC-02 completed); user has been redirected to the create-password page

**Flow:**
1. User is on `/reset-password?email=…&h=…&mode=create`
2. User enters new password (min 8 characters)
3. `POST /api/users/auth/create-password` with `{ email, verificationHash, newPassword }`
4. Backend validates hash, creates the `ATH_Authentications` record with hashed password
5. User can now log in

**Validation:**
- `NewPassword`: required, minimum 8 characters (error: `PASSWORD_TOO_SHORT`)

---

## UC-04 — Log In

**Actor:** Registered user with verified email and password  
**Precondition:** Account fully set up (UC-01 → UC-03)

**Flow:**
1. User navigates to `/login`
2. User enters email, password; optionally checks "Remember me"
3. `POST /api/users/auth/login` with `{ email, password, applicationCode, rememberMe }`
4. Backend verifies credentials, checks role assignment
5. On success: issues `auth_token` (JWT) + `refresh_token` (opaque) as `HttpOnly; Secure; SameSite=Strict` cookies
   - `RememberMe = true` → persistent cookies with `Expires` date
   - `RememberMe = false` → session cookies
6. Backend returns `{ user: { id, email, firstName, lastName }, roles: [...] }`
7. Frontend stores auth state in `AuthContext`; redirects to `/dashboard`

**Error cases:**
- Invalid credentials → `INVALID_USERNAME_OR_PASSWORD` (401)
- No role assigned → `NO_ASSIGNED_ROLE` (401)

---

## UC-05 — Session Restore on Page Load

**Actor:** Returning user with valid cookies  
**Precondition:** User previously logged in

**Flow (AuthContext on mount):**
1. `GET /api/users/auth/session` — validates `auth_token` cookie
2. If valid → restore auth state from JWT claims (email, firstName, lastName)
3. If 401 (expired) → `POST /api/users/auth/refresh` using `refresh_token` cookie
4. If refresh succeeds → new `auth_token` issued; retry `GET /api/users/auth/session`
5. If refresh fails → stay unauthenticated; redirect to `/login` on next protected route access

---

## UC-06 — Log Out

**Actor:** Authenticated user

**Flow:**
1. User clicks "Logout" in the navigation
2. `POST /api/users/auth/logout`
3. Backend revokes the refresh token in the DB and deletes both cookies
4. Frontend clears auth state; redirects to `/login`

---

## UC-07 — Change Password

**Actor:** Authenticated user  
**Precondition:** User is logged in

**Flow:**
1. User navigates to `/change-password`
2. User enters current email, old password, new password, and repeat new password (repeat is client-side only)
3. `POST /api/users/auth/change-password` with `{ email, oldPassword, newPassword }`
4. Backend verifies old password, updates hash/salt
5. Success message shown; user remains logged in

**Validation:**
- Email: required
- Old password: required
- New password: required, minimum 8 characters

---

## UC-08 — Request Password Reset

**Actor:** User who has forgotten their password  
**Precondition:** Account exists

**Flow:**
1. User navigates to `/request-password-reset`
2. User enters email address
3. `POST /api/users/auth/request-password-reset` with `{ email, appCode }`
4. Backend generates a `PasswordResetHash`, stores it with `PasswordResetRequestedAt`
5. Sends reset email with link: `{ResetPasswordBaseUrl}?email=…&h=…`
6. Frontend shows confirmation message (regardless of whether email exists, to prevent enumeration)

---

## UC-09 — Reset Password via Email Link

**Actor:** User who requested password reset (UC-08)  
**Precondition:** Reset email sent; link not older than 24 hours

**Flow:**
1. User clicks reset link → `/reset-password?email=…&h=…`
2. Frontend pre-fills email from query params
3. User enters new password
4. `POST /api/users/auth/change-password-reset` with `{ email, verificationHash, newPassword }`
5. Backend validates hash (must match DB value and be within 24 hours)
6. Password updated; reset hash cleared
7. User can log in with new password

**Error cases:**
- Hash mismatch or expired → error response

---

## UC-10 — View Dashboard

**Actor:** Authenticated user  
**Precondition:** Logged in

**Flow:**
1. User navigates to `/dashboard`
2. `ProtectedRoute` checks `AuthContext.isAuthenticated` — redirects to `/login` if false
3. Dashboard page renders

**Current state:** The dashboard is a placeholder ("Coming soon…"). The expenses feature is not yet implemented in the frontend (`QA-11`).

---

## UC-11 — View and Edit Settings

**Actor:** Authenticated user  
**Precondition:** Logged in

**Flow:**
1. User navigates to `/settings`
2. Settings page renders with available options (e.g., navigation to change password)

---

## UC-12 — Track an Expense *(backend ready, frontend pending)*

**Actor:** Authenticated user  
**Precondition:** Logged in

**Backend capability (implemented):**
- `POST /api/expenses/expenses` — create expense with amount, description, category, currency
- `GET /api/expenses/expenses` — list expenses for the authenticated user
- `PUT /api/expenses/expenses/{id}` — update an expense
- `DELETE /api/expenses/expenses/{id}` — delete an expense

**Frontend:** Not yet implemented. Placeholder UI shown on dashboard (`QA-11`).

**Authentication:** Enforced by nginx `auth_request` before the request reaches the expenses service.

---

## UC-13 — Token Refresh (Transparent)

**Actor:** Frontend (automatic)  
**Trigger:** Any API call returns 401

**Flow:**
1. `api.service.ts` `onUnauthorized` callback fires
2. `POST /api/users/auth/refresh` — attempts to renew `auth_token` using `refresh_token` cookie
3. On success: `auth_token` rotated; original page stays
4. On failure: user state cleared; redirect to `/login`

This flow is silent and transparent to the user unless the refresh token itself is expired.

---

## UC-14 — Email Verification Error Page

**Actor:** User with expired/invalid verification link  
**Precondition:** Verification link clicked after it expired

**Flow:**
1. `GET /api/users/auth/validate-email?h=…` fails (hash invalid or already used)
2. Backend redirects to `/verify-error`
3. Frontend renders a friendly error page: "Verification link expired" with a "Back to register" CTA
4. User can re-register or request a new verification email

---

## Functional Scope Summary

| Feature | Backend | Frontend | Status |
|---|---|---|---|
| User registration | ✅ | ✅ | Production-ready |
| Email verification | ✅ | ✅ | Production-ready |
| Initial password creation | ✅ | ✅ | Production-ready |
| Login / logout | ✅ | ✅ | Production-ready |
| Session restore + token refresh | ✅ | ✅ | Production-ready |
| Change password | ✅ | ✅ | Production-ready |
| Password reset via email | ✅ | ✅ | Production-ready |
| Expense CRUD | ✅ | ⏳ | Backend done; frontend pending |
| Category management | ✅ | ⏳ | Backend done; frontend pending |
| Currency management | ✅ | ⏳ | Backend done; frontend pending |
| Expense charts | — | ⏳ | Planned (Recharts) |
| Mobile app | — | — | Reserved (folder exists) |
