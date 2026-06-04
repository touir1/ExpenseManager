# Notifications Service

REST API and real-time push service for in-app and email notifications. Consumes family events from the expenses service via RabbitMQ and delivers notifications to connected clients over SignalR WebSocket.

## Tech Stack

- **.NET 8** — `net8.0` target framework
- **Entity Framework Core 8** + **Npgsql** — PostgreSQL via EF Core
- **SignalR** — real-time WebSocket push (hub at `/ws/notifications`)
- **RabbitMQ.Client 6.8.1** — consumes `expenses.events` topic exchange (`family.#` routing)
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
| `WS` | `/ws/notifications` | SignalR hub; pushes `notification` events to connected clients |
| `GET` | `/health` | Liveness/readiness probe |

All REST endpoints require authentication enforced by nginx's `auth_request` subrequest. The SignalR hub authenticates by reading the `auth_token` cookie (already validated by nginx on the WebSocket upgrade request).

### Response DTOs

**`NotificationDto`** — `{ id, type, payload: object, isRead, createdAt, readAt? }`

**Notification types:**
- `FAMILY_MEMBER_REMOVED` — payload: `{ type, familyId, familyName, removedByUserId, removedByName, expenseCount }`

## Architecture

- `FamilyEventConsumer` (BackgroundService) subscribes to queue `notifications.expenses.sync` bound to `expenses.events` exchange (`family.#`); retries on `BrokerUnreachableException` every 5 s; deduplicates via `InboxEvents` table
- On each consumed event: persists `Notification` row → pushes via `IHubContext<NotificationHub>` → sends email; hub push and email failures are non-fatal (wrapped in try/catch)
- `NotificationHub` groups connections by `userId`; extracts user ID from `auth_token` cookie via `JwtCookieReader`; aborts connection if cookie missing or invalid

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
