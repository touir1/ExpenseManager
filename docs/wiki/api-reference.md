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
  ]
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
  "lastName": "Doe"
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

## Expenses Service — Endpoints

> **Status:** The expenses service backend is scaffolded with domain models, EF Core, and RabbitMQ infrastructure but does not yet expose REST controllers. Endpoints will be documented here as they are implemented.

The frontend currently shows a placeholder expenses page (`QA-11` tracking item). The intended API design:

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/expenses/` | List expenses for authenticated user |
| `POST` | `/api/expenses/` | Create a new expense |
| `PUT` | `/api/expenses/{id}` | Update an expense |
| `DELETE` | `/api/expenses/{id}` | Delete (or hide) an expense |
| `GET` | `/api/expenses/categories` | List available categories |
| `GET` | `/api/expenses/currencies` | List available currencies |

All expenses endpoints will require authentication (nginx `auth_request` gate already configured for `/api/expenses/*`).

---

## Validation Rules

FluentValidation auto-validation middleware rejects requests with `400 Bad Request` before reaching controller logic.

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
