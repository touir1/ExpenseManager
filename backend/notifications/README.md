# Notifications Service

REST API and real-time push service for in-app and email notifications. Consumes events from the users and expenses services via RabbitMQ and delivers notifications to connected clients over SignalR WebSocket. Also the sole SMTP email dispatcher for the entire platform.

## Tech Stack

- **.NET 8** — `net8.0` target framework
- **Entity Framework Core 8** + **Npgsql** — PostgreSQL via EF Core
- **SignalR** — real-time WebSocket push (hub at `/ws/notifications`)
- **RabbitMQ.Client 6.8.1** — consumes `expenses.events` (`family.#` + `expenses.#`) and `users.events` (`user.#`) topic exchanges
- **FluentValidation 11** — request DTO validation
- **xUnit** + **Moq** — unit and integration tests

## Usage

```bash
cd Touir.ExpensesManager.Notifications
dotnet restore
dotnet run
```

Service runs on port **9300** by default. Configuration via `appsettings.json` and environment variables.

## Key Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/notifications?page=1&pageSize=20` | Paginated notifications for the authenticated user → `NotificationDto[]` |
| `GET` | `/notifications/unread-count` | Unread notification count → `{ count: int }` |
| `POST` | `/notifications/{id}/read` | Mark a single notification as read → 204 |
| `POST` | `/notifications/read-all` | Mark all notifications as read → 204 |
| `POST` | `/notifications/push-token` | Register a device push token (Phase 14 stub — returns 200 OK, no persistence; FCM/APNs dispatch is Phase 15) |
| `WS` | `/ws/notifications` | SignalR hub; pushes `notification` events to connected clients |
| `GET` | `/health` | Liveness/readiness probe |

All REST endpoints require authentication enforced by nginx's `auth_request` subrequest. The SignalR hub authenticates by reading the `auth_token` cookie (already validated by nginx on the WebSocket upgrade request).

### Response DTOs

**`NotificationDto`** — `{ id, type, payload: object, isRead, createdAt, readAt? }`

**Notification types:**

| Type | Delivery | Payload fields |
|------|----------|----------------|
| `FAMILY_MEMBER_REMOVED` | In-app + email | `familyId, familyName, removedByUserId, removedByName, expenseCount` |
| `FAMILY_INVITATION_ACCEPTED` | In-app + email | `familyId, familyName, acceptorName, acceptorEmail` |
| `FAMILY_MEMBER_JOINED` | In-app | `familyId, familyName, joinerName, joinerUserId` |
| `FAMILY_EXPENSE_ADDED` | In-app | `familyId, familyName, expenseId, amount, currencyCode, actorName, actorUserId` |
| `FAMILY_EXPENSE_DELETED` | In-app | same as added |
| `CSV_IMPORT_COMPLETED` | In-app | `totalRows, importedCount, skippedCount` |
| `RATE_CONFLICT_CREATED` | In-app (admins) | `conflictId, sourceCurrencyCode, destCurrencyCode, date, autoRate, manualRate` |

Email-only events (no DB row stored):
- `user.email.verification.requested` → verification email with link
- `user.password.reset.requested` → password-reset email with link
- `user.password.changed` → security notification email

## Architecture

### Consumers

**`FamilyEventConsumer`** (BackgroundService) subscribes to queue `notifications.expenses.sync` bound to `expenses.events` exchange on two routing keys: `family.#` and `expenses.#`. Handles:
- `family.member.removed` → persist + push + email (removed member)
- `family.invitation.requested` → email only (invitee)
- `family.invitation.accepted` → persist + push + email (family head)
- `family.member.joined` → persist + push fan-out (existing members)
- `family.expense.added` / `family.expense.deleted` → persist + push fan-out (co-members)
- `expenses.import.completed` → persist + push (importer)
- `expenses.rate.conflict` → persist + push fan-out (all admins)

**`UserNotificationEventConsumer`** (BackgroundService) subscribes to queue `notifications.users.email` bound to `users.events` exchange (`user.#` routing). Handles:
- `user.email.verification.requested` → verification email
- `user.password.reset.requested` → password-reset email
- `user.password.changed` → password-changed security email

Both consumers retry on `BrokerUnreachableException` every 5 s and deduplicate via the `InboxEvents` table.

### Notification dispatch

`NotificationService` follows this pattern per handler:
1. Persist `Notification` row (non-fatal try/catch for fan-out handlers)
2. Push via `IHubContext<NotificationHub>` to the user's SignalR group (non-fatal)
3. Send email via `IEmailHelper` when applicable (non-fatal)

Email-only handlers (verification, reset, password changed) skip steps 1 and 2.

`NotificationHub` groups connections by `userId`; extracts user ID from `auth_token` cookie via `JwtCookieReader`; aborts connection if cookie missing or invalid.

### Email templates

All SMTP email is sent from the notifications service. Templates live in `Assets/EmailTemplates/`:

| Template file | Trigger | Placeholders |
|---|---|---|
| `FAMILY_MEMBER_REMOVED_TEMPLATE.html` | Member removed from family | `@@REMOVED_BY_NAME@@`, `@@FAMILY_NAME@@`, `@@EXPENSE_COUNT@@` |
| `FAMILY_INVITATION_TEMPLATE.html` | Family invitation sent | `@@INVITER_NAME@@`, `@@FAMILY_NAME@@`, `@@INVITE_LINK@@` |
| `FAMILY_INVITATION_ACCEPTED_TEMPLATE.html` | Invitation accepted | `@@ACCEPTOR_NAME@@`, `@@FAMILY_NAME@@` |
| `EMAIL_VERIFICATION_TEMPLATE.html` | Registration verification | `@@VERIFICATION_LINK@@` |
| `PASSWORD_RESET_TEMPLATE.html` | Password reset requested | `@@RESET_LINK@@` |
| `PASSWORD_CHANGED_TEMPLATE.html` | Password changed | `@@FIRST_NAME@@` |

All templates also support `@@YEAR@@` (auto-substituted).

## Configuration

SMTP config via `EXPENSES_MANAGEMENT_NOTIFICATIONS_EMAILAUTH_*` env vars (EMAIL, PASSWORD, HOST, PORT, ENABLE_SSL).

RabbitMQ config via `EXPENSES_MANAGEMENT_NOTIFICATIONS_RABBITMQ_*` env vars.

Database config via `EXPENSES_MANAGEMENT_NOTIFICATIONS_DATABASE_*` env vars.

## Testing

```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory "./coverage" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
```

Use `TestNotificationsDbContextWrapper` (SQLite in-memory, `EnsureCreated`) for repository tests.

## Pipeline & Quality

GitLab CI with SonarQube quality gate, Semgrep SAST, OWASP Dependency Check, and Trivy image scanning.

## Docker

Containerized and orchestrated via Docker Compose. Image built and deployed through the CI/CD pipeline.
