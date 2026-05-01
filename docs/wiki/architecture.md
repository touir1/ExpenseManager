# Architecture

← [Wiki Index](./index.md)

---

## Overview

ExpenseManager follows a **microservices architecture** with two backend services, a single-page frontend application, and a shared infrastructure layer. All external traffic flows through a central nginx reverse proxy that handles TLS termination, routing, and authentication enforcement.

```
Browser / Client
       │
       ▼
  ┌─────────┐
  │  nginx  │  :443 (TLS)  ─── serves static SPA files
  └────┬────┘               ─── proxies /api/users/auth/* (public)
       │                    ─── auth_request + proxies /api/expenses/* (protected)
       │                    ─── auth_request + proxies /api/users/* (protected)
       │
  ┌────┴────────────────────────┐
  │                             │
  ▼                             ▼
users-service :9100       expenses-service :9200
(.NET 8 / EF Core)        (.NET 8 / EF Core)
  │                             │
  ▼                             ▼
users PostgreSQL DB       expenses PostgreSQL DB
                               │
                               ▼ (read-only cross-service)
                          ext.USR_Users view
```

---

## Services

### nginx (Reverse Proxy)

The sole ingress point for all traffic. Responsibilities:

- **TLS termination** — HTTPS on port 443; HTTP redirects to HTTPS
- **SPA serving** — static files from `/usr/share/nginx/html`; all unknown paths rewrite to `index.html` for client-side routing
- **Auth enforcement** — `auth_request` subrequest to `GET /auth/check` before forwarding protected routes
- **API routing** — path-based proxying:
  - `/api/users/auth/*` → `users-service:9100/auth/*` (no auth check — public endpoints)
  - `/api/users/*` → `users-service:9100/*` (auth required)
  - `/api/expenses/*` → `expenses-service:9200/*` (auth required)
- **CORS** — `Access-Control-Allow-Origin` set via `map` blocks; credentials allowed; `OPTIONS` preflight handled
- **HSTS** — `Strict-Transport-Security: max-age=1209600`
- **Cookie forwarding** — `proxy_cookie_path` enforces `HTTPOnly; Secure; SameSite=strict` on all proxied Set-Cookie headers

**Configuration:** `infrastructure/configs/nginx/sites-available/expenses-manager.conf`

### Users Service (port 9100)

REST API responsible for the entire identity domain:

- User registration with email verification
- JWT-based authentication (access token as `HttpOnly` cookie)
- Opaque refresh token rotation (DB-backed, `RTK_RefreshTokens` table)
- Password management (change, request reset, reset, initial create)
- Role-based access control (per-application roles)
- Application registry (multi-tenant support via `APP_Applications`)

**Technology:** .NET 8, ASP.NET Core, EF Core 8, Npgsql, FluentValidation 11

### Expenses Service (port 9200)

REST API for the expense tracking domain:

- Expense CRUD (amount, description, category, currency, user)
- Category management
- Currency management
- Cross-service user data via read-only `ext.USR_Users` view
- Async event publishing via RabbitMQ

**Technology:** .NET 8, ASP.NET Core, EF Core 8, Npgsql

### Frontend Dashboard (React SPA)

Single-page application served by nginx as static files:

- Auth flows (login, register, email verification, password management)
- Dashboard and settings views
- Context-based auth state (`AuthContext`)
- Cookie-based session restore on page load with automatic token refresh

**Technology:** React 18, TypeScript, Vite 7, Tailwind CSS v3, React Router v6

---

## Authentication Flow

```
1. User submits login form
      │
      ▼
2. POST /api/users/auth/login  (via nginx, no auth_request)
      │
      ▼
3. users-service validates credentials:
   - Looks up user by email (lowercased)
   - Verifies HMAC password hash
   - Checks application role assignment
      │
      ▼
4. Issues tokens:
   - JWT access token  → auth_token cookie (HttpOnly, Secure, SameSite=Strict)
   - Opaque refresh token → refresh_token cookie (HttpOnly, Secure, SameSite=Strict)
   - RememberMe=true  → persistent cookies with Expires
   - RememberMe=false → session cookies
      │
      ▼
5. Frontend stores nothing — all auth state lives in cookies

6. Subsequent API calls (e.g. GET /api/expenses/):
      │
      ▼
   nginx receives request with auth_token cookie
      │
      ▼
   auth_request → GET /internal/auth/check (internal, not externally accessible)
      │
      ▼
   users-service validates JWT or Bearer token
   Returns 200 (authorized) or 401 (unauthorized)
      │
      ├─ 200 → nginx proxies to expenses-service
      └─ 401 → nginx returns 401 to client

7. On 401:
   Frontend AuthContext intercepts → POST /api/users/auth/refresh
      │
      ├─ Success → sessionCheck() → retry original request
      └─ Failure → redirect to /login
```

---

## Registration & Email Verification Flow

```
1. POST /api/users/auth/register
   - Creates USR_Users record (IsEmailValidated = false)
   - Generates EmailValidationHash
   - Sends verification email with link: {VerifyEmailUrl}?h=…&s=…&app_code=…
   - Assigns default application role

2. User clicks email link
   → GET /api/users/auth/validate-email?h=…&s=…&app_code=…
   - Sets IsEmailValidated = true on user
   - Redirects to {ResetPasswordUrlPath}?email=…&h=…&mode=create
     (the "create initial password" page)
   - On failure: redirects to {VerifyEmailErrorUrlPath} (e.g. /verify-error)

3. POST /api/users/auth/create-password
   - Validates EmailValidationHash
   - Creates authentication record (hashed password + salt)
   - User can now log in
```

---

## Password Reset Flow

```
1. POST /api/users/auth/request-password-reset
   - Generates PasswordResetHash and PasswordResetRequestedAt
   - Sends reset email with link: {ResetPasswordBaseUrl}?email=…&h=…

2. User clicks email link → /reset-password?email=…&h=… (frontend page)

3. POST /api/users/auth/change-password-reset
   - Validates PasswordResetHash (must be < 24 hours old)
   - Updates password hash/salt
   - Clears PasswordResetHash and PasswordResetRequestedAt
```

---

## Cross-Service Communication

### Synchronous (HTTP)
nginx enforces auth by proxying auth checks to the users service. The expenses service itself never calls the users service directly — nginx acts as the auth enforcement layer.

### Cross-Database (read-only)
The expenses service reads user data via `Repositories/External/UserRepository`, which queries the `ext.USR_Users` view in the shared PostgreSQL instance. This is a **read-only, schema-isolated** dependency — the expenses service never writes to the users database.

### Asynchronous (RabbitMQ)
The expenses service uses `IRabbitMQService` to publish/consume events. The specific events and consumers are not yet fully implemented in the current version.

---

## Deployment Model

```
GitLab CI Pipeline
       │
  ┌────┴────────────────────────────────────────┐
  │  build → test → quality → security          │
  │       → docker → docker-security → deploy   │
  └─────────────────────────────────────────────┘
       │
       ▼
  GitLab Container Registry (localhost:5050)
       │ docker push
       ▼
  .docker-deploy-dev job (manual trigger)
       │ curl Bearer token
       ▼
  Docker Updater API (jobs-runner:8989)
       │ docker pull + recreate container
       ▼
  Running container (users-service or expenses-service)
```

The **Docker Updater API** is a custom Python service running inside `jobs-runner` via supervisord. It accepts `GET /v1/update?servicename=<name>` with a Bearer token, pulls the latest image from the registry for the named service, and recreates the container without downtime.

Frontend static assets are zipped, versioned, and uploaded to MinIO (artifact storage) by the `.deploy-web-dev` CI job, then served by nginx from the MinIO bucket or copied to the nginx html volume.

---

## Security Model

| Layer | Mechanism |
|---|---|
| Transport | TLS 1.2/1.3 (nginx); HSTS header |
| Authentication | JWT access token in HttpOnly cookie; opaque refresh token in HttpOnly cookie |
| Cookie protection | `HttpOnly; Secure; SameSite=Strict` enforced by nginx `proxy_cookie_path` |
| Authorization | nginx `auth_request` gate before every protected route |
| Password storage | HMAC hash + random salt (`CryptographyHelper`) |
| Token validation | JWT signature + claims (`IJwtTokenService`) |
| Input validation | FluentValidation on all request DTOs (backend); Zod schemas (frontend) |
| Secret scanning | Gitleaks in CI pipeline |
| SAST | Semgrep in CI pipeline |
| Dependency audit | OWASP Dependency Check |
| Image scanning | Trivy |

**Known open issues:**
- `S-1` — No `limit_req` on the login endpoint in nginx
- `S-2` — CSP and security headers not yet set in nginx config

---

## Layered Architecture (per service)

Each backend service follows a strict layered architecture:

```
Controllers      ← HTTP boundary; request/response DTOs; FluentValidation auto-validation
     │
Services         ← Business logic; orchestrates repositories
     │
Repositories     ← Data access; EF Core; returns domain models
     │
DbContext        ← EF Core context; migrations applied at startup
     │
PostgreSQL
```

Abstractions (interfaces) exist at every layer boundary, enabling Moq-based unit testing throughout.
