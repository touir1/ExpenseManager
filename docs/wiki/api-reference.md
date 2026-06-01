# API Reference

← [Wiki Index](./index.md)

---

## Overview

All requests and responses use `application/json`. Authentication is enforced by nginx `auth_request` — requests to protected endpoints without a valid `auth_token` cookie are rejected at the proxy level with `401` before reaching the service.

Both services are reachable through nginx:

| Service | nginx path prefix | Direct port |
|---|---|---|
| Users | `/api/users/` | `9100` |
| Expenses | `/api/expenses/` | `9200` |

---

## Authentication Model

JWT access tokens and opaque refresh tokens are set as `HttpOnly; Secure; SameSite=Strict` cookies:

| Cookie | Content | Scope |
|---|---|---|
| `auth_token` | Signed JWT | Sent with every request |
| `refresh_token` | Opaque random value | Used only by `POST /auth/refresh` |

When `RememberMe: false`, cookies are session-scoped (no `Expires`). When `RememberMe: true`, cookies expire per the JWT config.

---

## Common Error Response

All error responses use the same shape:

```json
{ "message": "ERROR_CODE" }
```

**Error codes:**

| Code | Meaning |
|---|---|
| `SERVER_ERROR` | Unhandled exception in handler |
| `MISSING_PARAMETERS` | Required query parameters missing |
| `MISSING_TOKEN` | No auth cookie or Bearer token present |
| `INVALID_TOKEN` | JWT invalid or expired |
| `INVALID_USERNAME_OR_PASSWORD` | Login credential mismatch |
| `NO_ASSIGNED_ROLE` | User has no role in the requested application |
| `EMAIL_VERIFICATION_FAILED` | Email hash mismatch or app not found |
| `SET_NEW_PASSWORD_FAILED` | Old password incorrect or user not found |
| `CREATE_PASSWORD_FAILED` | Hash mismatch or already set |
| `RESET_PASSWORD_FAILED` | Reset hash invalid/expired |
| `REQUEST_PASSWORD_RESET_FAILED` | User not found or app not found |
| `USER_NOT_FOUND` | User ID from refresh token not found |

---

## Users Service — Auth Endpoints

Base path: `/auth` (direct) or `/api/users/auth` (via nginx)

> **nginx note:** All `/api/users/auth/*` routes are public — no `auth_request` enforcement. The external path `/api/users/auth/check` is explicitly blocked (returns 404) to prevent direct access.

---

### POST /auth/register

Register a new user. Sends a verification email to the provided address.

**Access:** Public

**Request body:**

```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "jane@example.com",
  "applicationCode": "EXPENSES_MANAGER"
}
```

| Field | Type | Required | Notes |
|---|---|---|---|
| `firstName` | string | Yes | |
| `lastName` | string | Yes | |
| `email` | string | Yes | Stored lowercase |
| `applicationCode` | string? | No | Determines which app's email/URLs are used |

**Response: 200 OK**

```json
{
  "errors": null,
  "hasError": false
}
```

If validation fails, `hasError: true` and `errors` contains field-level messages.

**Response: 400 Bad Request** — `SERVER_ERROR`

---

### GET /auth/validate-email

Called from the link in the verification email. Validates the hash and redirects.

**Access:** Public

**Query parameters:**

| Parameter | Description |
|---|---|
| `h` | Email verification hash |
| `s` | Email address (URL-encoded) |
| `app_code` | Application code |

**Responses:**

| Status | Condition | Body/Action |
|---|---|---|
| 302 | Valid hash | Redirects to `{app.ResetPasswordUrlPath}?email=...&h=...&mode=create` |
| 302 | Valid hash but `VerifyEmailErrorUrlPath` configured | Redirects to error page if validation fails |
| 401 | Missing parameters | `{ "message": "MISSING_PARAMETERS" }` |
| 401 | App not found or hash mismatch | `{ "message": "EMAIL_VERIFICATION_FAILED" }` |
| 400 | Server error | `{ "message": "SERVER_ERROR" }` |

---

### POST /auth/create-password

Set the initial password after email verification (used on first login or after email re-verification).

**Access:** Public

**Request body:**

```json
{
  "email": "jane@example.com",
  "verificationHash": "<hash from email>",
  "newPassword": "MySecureP@ss1"
}
```

| Field | Type | Required |
|---|---|---|
| `email` | string | Yes |
| `verificationHash` | string? | No — but required functionally |
| `newPassword` | string | Yes |

**Response: 200 OK** — Empty body (success)  
**Response: 401** — `{ "message": "CREATE_PASSWORD_FAILED" }`  
**Response: 400** — `{ "message": "SERVER_ERROR" }`

---

### POST /auth/login

Authenticate a user. On success, sets `auth_token` and `refresh_token` cookies.

**Access:** Public

**Request body:**

```json
{
  "email": "jane@example.com",
  "password": "MySecureP@ss1",
  "applicationCode": "EXPENSES_MANAGER",
  "rememberMe": true
}
```

| Field | Type | Required |
|---|---|---|
| `email` | string? | Yes |
| `password` | string? | Yes |
| `applicationCode` | string? | No |
| `rememberMe` | bool? | No — defaults to false |

**Response: 200 OK**

```json
{
  "user": {
    "id": 1,
    "firstName": "Jane",
    "lastName": "Doe",
    "email": "jane@example.com",
    "familyId": null,
    "createdAt": "2026-01-01T00:00:00Z",
    "createdBy": null,
    "lastUpdatedAt": "2026-01-01T00:00:00Z",
    "lastUpdatedBy": null,
    "isDisabled": false
  },
  "roles": [
    {
      "id": 1,
      "code": "USER",
      "name": "Standard User",
      "description": null,
      "application": null
    }
  ],
  "isAdmin": false
}
```

Sets cookies:
- `auth_token`: JWT access token
- `refresh_token`: Opaque refresh token

**Response: 401** — `INVALID_USERNAME_OR_PASSWORD` or `NO_ASSIGNED_ROLE`  
**Response: 400** — `SERVER_ERROR`

---

### GET /auth/check

Validates the `auth_token` cookie (or `Authorization: Bearer <token>` header). Used internally by nginx `auth_request`. External access is blocked by nginx (returns 404).

**Access:** Internal (nginx `auth_request` only)

**Response: 200 OK** — Token valid  
**Response: 401** — `MISSING_TOKEN` or `INVALID_TOKEN`  
**Response: 400** — `SERVER_ERROR`

---

### GET /auth/session

Returns user identity from the `auth_token` cookie. Used by the SPA on initial load to restore session state.

**Access:** Public (but requires valid `auth_token` cookie)

**Response: 200 OK**

```json
{
  "email": "jane@example.com",
  "firstName": "Jane",
  "lastName": "Doe",
  "isAdmin": false
}
```

**Response: 401** — `MISSING_TOKEN` or `INVALID_TOKEN`  
**Response: 400** — `SERVER_ERROR`

---

### POST /auth/refresh

Rotates the refresh token and issues a new access token. Called automatically by the SPA when a 401 is received.

**Access:** Public (requires valid `refresh_token` cookie)

**Request body:** Empty

**Response: 200 OK** — Sets new `auth_token` and `refresh_token` cookies  
**Response: 401** — `MISSING_TOKEN`, `INVALID_TOKEN`, or `USER_NOT_FOUND`  
**Response: 400** — `SERVER_ERROR`

---

### POST /auth/logout

Revokes the refresh token and clears both cookies.

**Access:** Public

**Request body:** Empty

**Response: 200 OK** — Always succeeds (best-effort revocation)

---

### POST /auth/resend-verification

Resend the email verification link. Atomically rotates the hash and resets the 24 h expiry window — the old link is immediately invalidated.

**Access:** Public

**Request body:**

```json
{
  "email": "jane@example.com",
  "applicationCode": "EXPENSES_MANAGER"
}
```

**Response: 200 OK** — Always (no email enumeration)

---

## Users Service — Messaging Endpoints

Base path: `/messaging` (direct) or `/api/users/messaging` (via nginx)

### POST /messaging/replay

Replay outbox events. Re-publishes events from `MSG_OutboxEvents` that match the given filters.

**Access:** Protected (nginx `auth_request`)  
**Rate limit:** `messaging_replay` — 5 req / 1 min

**Query parameters:**

| Parameter | Type | Description |
|---|---|---|
| `eventType` | string? | Filter by event type (e.g. `user.created`) |
| `from` | datetime? | Include events created at or after this timestamp |
| `forceAll` | bool? | When `true`, re-publish events already in `Sent` state |

**Response: 200 OK** — Replay triggered

---

### GET /messaging/outbox/stats

Returns counts of outbox events by status.

**Access:** Public (no rate limit)

**Response: 200 OK**

```json
{
  "pending": 3,
  "sent": 142,
  "failed": 1
}
```

---

### POST /auth/change-password

Change password for an authenticated user by supplying the current password.

**Access:** Protected (`auth_request` enforced by nginx on `/api/users/*`)

**Request body:**

```json
{
  "email": "jane@example.com",
  "oldPassword": "OldP@ss1",
  "newPassword": "NewP@ss2"
}
```

| Field | Type | Required |
|---|---|---|
| `email` | string | Yes |
| `oldPassword` | string? | No — required functionally |
| `newPassword` | string | Yes |

**Response: 200 OK** — Success  
**Response: 401** — `SET_NEW_PASSWORD_FAILED`  
**Response: 400** — `SERVER_ERROR`

---

### POST /auth/request-password-reset

Send a password reset email to the user. No authentication required.

**Access:** Public

**Request body:**

```json
{
  "email": "jane@example.com",
  "appCode": "EXPENSES_MANAGER"
}
```

**Response: 200 OK** — Email sent  
**Response: 401** — `REQUEST_PASSWORD_RESET_FAILED`  
**Response: 400** — `SERVER_ERROR`

---

### POST /auth/change-password-reset

Reset password using the hash from the reset email.

**Access:** Public

**Request body:**

```json
{
  "email": "jane@example.com",
  "verificationHash": "<hash from email>",
  "newPassword": "ResetP@ss3"
}
```

| Field | Type | Required |
|---|---|---|
| `email` | string | Yes |
| `verificationHash` | string? | No — required functionally |
| `newPassword` | string | Yes |

**Response: 200 OK** — Password updated  
**Response: 401** — `RESET_PASSWORD_FAILED`  
**Response: 400** — `SERVER_ERROR`

---

## Users Service — Admin Endpoints

Base path: `/admin/users` (direct) or `/api/users/admin/users` (via nginx)

**Access:** `APP_ADMIN` role required (`AdminAuthorize` filter). All endpoints return `403` if caller lacks admin role.

---

### GET /admin/users

Paginated list of all users with optional search.

**Query parameters:**

| Parameter | Type | Default |
|---|---|---|
| `search` | string? | null |
| `page` | int | 1 |
| `pageSize` | int | 20 |

**Response: 200 OK**

```json
{ "users": [...], "total": 42, "page": 1, "pageSize": 20 }
```

---

### GET /admin/users/roles

List all application roles.

**Response: 200 OK** — array of `RoleDto`

---

### PATCH /admin/users/{id}/disable

Disable a user account (`IsDisabled=true`). Admin cannot disable themselves — returns `403 CANNOT_SELF_DISABLE`.

**Response: 204 No Content** | **403** (`CANNOT_SELF_DISABLE`) | **404** | **400**

---

### PATCH /admin/users/{id}/enable

Re-enable a disabled user account.

**Response: 204 No Content** | **404** | **400**

---

### PUT /admin/users/{id}/roles

Replace all role assignments for a user. Admin cannot remove their own `APP_ADMIN` role — returns `403 CANNOT_REMOVE_OWN_ADMIN_ROLE`.

**Request body:**

```json
{ "roleIds": [1, 2] }
```

**Response: 204 No Content** | **403** (`CANNOT_REMOVE_OWN_ADMIN_ROLE`) | **401** | **400**

---

## Expenses Service — Endpoints

All expenses endpoints require authentication (nginx `auth_request`). User identity extracted from `auth_token` cookie by `JwtCookieReader`. Rate limit: `expenses_global` — 100 req / 60 s sliding window per IP, applied to all three resource controllers.

Base path: `/expenses` (direct port 9200) or `/api/expenses` (via nginx)

---

### GET /expenses

Paginated list of expenses for the authenticated user.

**Access:** Protected

**Query parameters:** pagination (`page`, `pageSize`), date range, etc.

**Response: 200 OK**

```json
{
  "items": [
    {
      "id": 1,
      "description": "Coffee",
      "amount": 3.50,
      "createdDate": "2026-05-01T08:00:00Z",
      "isHidden": false,
      "currency": { "id": 1, "code": "USD", "name": "US Dollar" },
      "category": { "id": 2, "name": "Food", "description": null },
      "subcategory": { "id": 5, "name": "Beverages", "description": null }
    }
  ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 20
}
```

**Response: 401** — Missing/invalid `auth_token` cookie

---

### POST /expenses

Create a new expense.

**Access:** Protected

**Request body:**

```json
{
  "description": "Lunch",
  "amount": 12.00,
  "createdDate": "2026-05-11T12:00:00Z",
  "currencyId": 1,
  "categoryId": 2,
  "subcategoryId": 5,
  "familyIds": null
}
```

| Field | Notes |
|---|---|
| `familyIds` | `null` = auto-attribute to user's default family; `[]` = no attribution; `[id1]` = validate membership |

**Response: 201 Created** — Created expense  
**Response: 400** — Validation failure  
**Response: 401** — Auth failure  
**Response: 403** — `FamilyForbiddenException` (user not a member of a provided familyId)

---

### PUT /expenses/{id}

Update an existing expense.

**Access:** Protected

**Request body:** Same shape as POST.

**Response: 200 OK** — Updated expense  
**Response: 400** — Validation failure  
**Response: 401** — Auth failure  
**Response: 403** — FamilyForbiddenException  
**Response: 404** — Expense not found or belongs to another user

---

### DELETE /expenses/{id}

Soft-delete an expense (`IsDeleted=true`, `DeletedAt` set).

**Access:** Protected

**Response: 204 No Content**  
**Response: 404** — Not found

---

### GET /categories

List all active top-level categories with active subcategories.

**Access:** Protected

**Response: 200 OK**

```json
[
  {
    "id": 1,
    "name": "Food",
    "description": null,
    "subcategories": [
      { "id": 5, "name": "Beverages", "description": null }
    ]
  }
]
```

---

### POST /categories

Create a category (or subcategory if `parentId` provided).

**Access:** Protected

**Response: 201 Created**  
**Response: 400** — Validation failure

---

### PUT /categories/{id}

Update a category.

**Response: 200 OK** | **404** | **400**

---

### DELETE /categories/{id}

Soft-delete a category.

**Response: 204** | **404**

---

### GET /currencies

List all currencies.

**Access:** Protected

**Response: 200 OK**

```json
[
  { "id": 1, "code": "USD", "name": "US Dollar" },
  { "id": 2, "code": "EUR", "name": "Euro" }
]
```

---

---

## Expenses Service — Dashboard Endpoints

All dashboard endpoints require authentication. Rate limit: `expenses_global` (100 req / 60 s sliding window per IP).

Base path: `/dashboard` (direct port 9200) or `/api/expenses/dashboard` (via nginx)

**Common query parameters (all except `same-month-across-years`):**

| Parameter | Type | Default |
|---|---|---|
| `familyId` | int? | null (own expenses) |
| `dateFrom` | DateOnly? | varies per endpoint |
| `dateTo` | DateOnly? | today |
| `displayCurrencyId` | int? | null (original currencies) |

---

### GET /dashboard/summary

Returns aggregated totals for the period: total spend, delta vs. previous period, top category, expense count.

**Default period:** current calendar month to today.

**Response: 200 OK**

```json
{
  "totalAmount": 430.00,
  "convertedTotal": 395.20,
  "displayCurrency": { "id": 2, "code": "EUR", "name": "Euro" },
  "expenseCount": 12,
  "previousPeriodTotal": 510.00,
  "changePercent": -15.7,
  "topCategory": { "id": 1, "name": "Food" },
  "topCategoryAmount": 180.00
}
```

**Response: 401** | **403** (invalid `familyId`) | **400** (`SERVER_ERROR`)

---

### GET /dashboard/monthly

Returns monthly expense totals with per-category breakdown.

**Default period:** Jan 1 of current year to today.

**Response: 200 OK** — array of:

```json
[
  {
    "year": 2026,
    "month": 5,
    "totalAmount": 430.00,
    "convertedTotal": null,
    "byCategory": [
      { "category": { "id": 1, "name": "Food" }, "amount": 180.00, "convertedAmount": null }
    ]
  }
]
```

---

### GET /dashboard/categories

Returns per-category breakdown with subcategory detail and percentage.

**Default period:** current calendar month to today.

**Response: 200 OK** — array of:

```json
[
  {
    "category": { "id": 1, "name": "Food" },
    "totalAmount": 180.00,
    "convertedTotal": null,
    "percentage": 41.9,
    "subcategories": [
      { "category": { "id": 3, "name": "Groceries" }, "amount": 120.00, "convertedAmount": null }
    ]
  }
]
```

---

### GET /dashboard/same-month-across-years

Returns totals for the same calendar month number across all years that have data.

**Query parameters:**

| Parameter | Type | Default |
|---|---|---|
| `month` | int? | current month (1–12) |
| `familyId` | int? | null |
| `displayCurrencyId` | int? | null |

**Response: 200 OK**

```json
[
  { "year": 2024, "totalAmount": 390.00, "convertedTotal": null },
  { "year": 2025, "totalAmount": 460.00, "convertedTotal": null },
  { "year": 2026, "totalAmount": 430.00, "convertedTotal": null }
]
```

**Response: 400** — `INVALID_MONTH` if `month` outside 1–12.

---

### GET /dashboard/by-currency

Returns per-currency totals for the period.

**Default period:** current calendar month to today.

**Response: 200 OK**

```json
[
  {
    "currency": { "id": 1, "code": "USD", "name": "US Dollar" },
    "totalAmount": 200.00,
    "convertedAmount": 183.50,
    "expenseCount": 7
  }
]
```

---

### GET /dashboard/recent

Returns the 10 most recent expenses, optionally filtered by family and date range. No `displayCurrencyId` support (returns expenses in their original currencies).

**Response: 200 OK** — `ExpensePagedResponse` (same shape as `GET /expenses`, pageSize fixed at 10).

---

## Expenses Service — Family Endpoints

### GET /families

List all families for the authenticated user (as member or owner).

**Response: 200 OK** — Array of family objects

---

### GET /families/{id}

Get family detail including members and their roles.

**Response: 200 OK** | **404** | **403**

---

### POST /families

Create a named family. The creator becomes the Head member.

**Request body:**

```json
{ "name": "Smith Family" }
```

**Response: 201 Created** | **400** | **409** (conflict)

---

### PUT /families/{id}/rename

**Request body:** `{ "name": "New Name" }`

**Response: 200 OK** | **403** (not Head) | **404**

---

### POST /families/{id}/archive

Soft-delete (archive) a non-default family.

**Response: 200 OK** | **403** | **404** | **409** (default family cannot be archived)

---

### POST /families/{id}/unarchive

Restore an archived family.

**Response: 200 OK** | **403** | **404**

---

### POST /families/{id}/invite

Send an invitation to a user by email.

**Request body:**

```json
{ "email": "friend@example.com" }
```

**Response: 200 OK** — Invitation token created  
**Response: 400** — Validation / `FamilyInvitationException`  
**Response: 403** | **404**

---

### POST /families/{id}/accept-invite

Accept an invitation by token.

**Request body:**

```json
{ "token": "<guid>" }
```

**Response: 200 OK** — User added as Member  
**Response: 400** — Token expired, already accepted, or email mismatch (`FamilyInvitationException`)  
**Response: 404**

---

### DELETE /families/{id}/members/{memberId}

Remove a member. Purges their expense attributions for this family via `RemoveMemberAttributionsAsync`.

**Response: 204** | **403** (not Head) | **404**

---

### PUT /families/{id}/members/{memberId}/role

Change a member's role (Head/Member).

**Request body:**

```json
{ "roleName": "Head" }
```

Role name normalised to title case before lookup via `ILookupCacheService.GetIdAsync<FamilyRole>`.

**Response: 200 OK** | **403** | **404** | **400**

---

## Expenses Service — User Config Endpoints

Base path: `/config` (direct) or `/api/expenses/config` (via nginx)

Rate limit: `expenses_global`. Authentication required.

---

### GET /config

Returns authenticated user's configuration. Null fields if no config row exists yet.

**Response: 200 OK**

```json
{ "defaultCurrencyId": 1, "defaultCurrency": { "id": 1, "code": "USD", "name": "US Dollar" } }
```

**Response: 401** — missing/invalid `auth_token`

---

### PUT /config

Upsert user configuration. Pass `null` for `defaultCurrencyId` to clear the default.

**Request body:**

```json
{ "defaultCurrencyId": 1 }
```

**Response: 200 OK** — `UserConfigDto`  
**Response: 400** — `USER_CONFIG_INVALID_CURRENCY`  
**Response: 401**

---

## Expenses Service — Admin Currency Endpoints

Base path: `/admin/currencies` (direct) or `/api/expenses/admin/currencies` (via nginx)

**Access:** `APP_ADMIN` role required (`AppAdmin` filter). Rate limit: `expenses_global`.

---

### POST /admin/currencies

Add a new currency.

**Request body:** `{ "code": "CHF", "name": "Swiss Franc", "symbol": "Fr.", "decimals": 2 }`

**Response: 201 Created** — `CurrencyDto` | **409** (code already exists) | **400**

---

### PUT /admin/currencies/{id}

Update currency name, symbol, or decimal places. Code is immutable.

**Request body:** `{ "name": "Swiss Franc", "symbol": "CHF", "decimals": 2 }`

**Response: 200 OK** — `CurrencyDto` | **404** (`CURRENCY_NOT_FOUND`) | **400**

---

### DELETE /admin/currencies/{id}

Delete a currency (only if no expenses reference it).

**Response: 204 No Content** | **404** | **409** (in use) | **400**

---

### GET /admin/currencies/{id}/defaults

List all default fallback rates defined for this currency.

**Response: 200 OK** — array of `CurrencyDefaultRateDto`

---

### POST /admin/currencies/defaults

Set a default fallback rate for a currency pair.

**Request body:** `{ "sourceCurrencyId": 1, "destinationCurrencyId": 2, "rate": 0.92 }`

**Response: 204 No Content** | **401** | **400**

---

## Validation Rules

FluentValidation auto-validation middleware rejects requests with `401 UnauthorizedObjectResult` (preserves existing wire format via `InvalidModelStateResponseFactory`) before reaching controller logic. The response body is `{ "message": "FIRST_ERROR_CODE" }`.

**Password rules (from `PasswordValidator`):**

| Rule | Constraint |
|---|---|
| Minimum length | 8 characters |
| Maximum length | 50 characters |

> `PASSWORD_TOO_SHORT` and `FIELD_TOO_LONG` appear in validator error messages returned in the `errors` array of validation failure responses.

**Email rules:**

- Must be valid email format
- Stored as lowercase

**Name rules:**

- `FirstName`, `LastName`: required, max 100 characters
