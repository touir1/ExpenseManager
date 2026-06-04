# Phase 13 — Notifications (Dedicated Microservice + SignalR)

## Context

Attribution removal is a silent operation today. Phase 13 adds a dedicated `notifications-service` (port 9300) that handles all in-app and email notifications. It consumes events from other services via RabbitMQ and pushes real-time updates to connected clients via SignalR WebSocket.

**Architecture:**
```
Expenses service
  → publishes "family.member.removed" to RabbitMQ (expenses.events Topic exchange)
      → Notifications service consumes it
          → persists Notification to own DB
          → pushes via SignalR to connected user
          → sends email
          → exposes REST API for frontend to fetch/mark-read
```

Frontend connects to the notifications service hub at `/api/notifications/ws/notifications`.

---

## Part 1 — Expenses Service changes

### 1a. New publisher: `FamilyEventPublisher`

Mirrors `UserEventPublisher` in the users service. New files:

- `Messaging/Messages/FamilyEventMessage.cs` — event payload DTO
- `Messaging/Publishers/IFamilyEventPublisher.cs` — interface
- `Messaging/Publishers/FamilyEventPublisher.cs` — implementation

Exchange: `expenses.events` (Topic, durable, non-auto-delete)
Routing key: `family.member.removed`
Message format: JSON-serialized `FamilyEventMessage` with `MessageId` (new GUID) in `IBasicProperties`

```csharp
public class FamilyEventMessage
{
    public string MessageId { get; set; } = null!;
    public string EventType { get; set; } = null!;      // "family.member.removed"
    public int TargetUserId { get; set; }
    public string TargetEmail { get; set; } = null!;
    public string TargetFirstName { get; set; } = null!;
    public int FamilyId { get; set; }
    public string FamilyName { get; set; } = null!;
    public int RemovedByUserId { get; set; }
    public string RemovedByName { get; set; } = null!;
    public int ExpenseCount { get; set; }
    public DateTime OccurredAt { get; set; }
}
```

`FamilyEventPublisher.PublishAsync(FamilyEventMessage)`:
- Opens channel on `IRabbitMQService.GetConnection()`
- Declares `expenses.events` topic exchange (idempotent)
- Publishes JSON with `MessageId` in basic properties, persistent delivery mode

### 1b. `FamilyService` integration

File: `Services/FamilyService.cs`

- Inject `IFamilyEventPublisher` into constructor
- In `RemoveMemberAsync`, replace the two remove calls (lines 260–261) with:

```csharp
var expenseCount = await _familyRepo.CountMemberAttributionsAsync(familyId, targetUserId);

await _familyRepo.RemoveMemberAsync(targetMembership);
await _familyRepo.RemoveMemberAttributionsAsync(familyId, targetUserId);

var target  = await _userRepo.GetUserByIdAsync(targetUserId);
var remover = await _userRepo.GetUserByIdAsync(removedById);

if (target is not null)
{
    try
    {
        await _familyEventPublisher.PublishAsync(new FamilyEventMessage
        {
            MessageId       = Guid.NewGuid().ToString(),
            EventType       = "family.member.removed",
            TargetUserId    = targetUserId,
            TargetEmail     = target.Email,
            TargetFirstName = target.FirstName ?? string.Empty,
            FamilyId        = familyId,
            FamilyName      = family.Name,
            RemovedByUserId = removedById,
            RemovedByName   = remover is not null
                                ? $"{remover.FirstName} {remover.LastName}".Trim()
                                : string.Empty,
            ExpenseCount    = expenseCount,
            OccurredAt      = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"[FamilyEvent] publish failed: {ex.Message}");
    }
}
```

`family` is already loaded at line 237. `_userRepo` is already injected. Publish failure must NOT abort the removal.

### 1c. `IFamilyRepository` addition

Add `CountMemberAttributionsAsync(int familyId, int userId)` — counts `ExpenseFamilyAttributions` joined with `Expense` where `FamilyId == familyId && Expense.UserId == userId`. Must be called **before** `RemoveMemberAttributionsAsync`.

### 1d. `Program.cs` — expenses service

```csharp
builder.Services.AddScoped<IFamilyEventPublisher, FamilyEventPublisher>();
```

---

## Part 2 — Notifications Microservice (new project)

### Project layout

```
backend/notifications/
└── Touir.ExpensesManager.Notifications/
    ├── Controllers/
    │   ├── DTO/NotificationDto.cs
    │   └── NotificationController.cs
    ├── Hubs/
    │   └── NotificationHub.cs
    ├── Infrastructure/
    │   ├── Contracts/
    │   │   ├── IEmailHelper.cs
    │   │   └── IEmailService.cs
    │   ├── Options/
    │   │   ├── PostgresOptions.cs
    │   │   ├── RabbitMQOptions.cs
    │   │   └── EmailOptions.cs
    │   ├── EmailHelper.cs
    │   ├── SmtpEmailService.cs
    │   └── NotificationsDbContext.cs
    ├── Messaging/Consumers/
    │   └── FamilyEventConsumer.cs
    ├── Models/
    │   ├── Notification.cs
    │   └── InboxEvent.cs
    ├── Migrations/
    ├── Repositories/
    │   ├── Contracts/
    │   │   ├── INotificationRepository.cs
    │   │   └── IInboxRepository.cs
    │   ├── NotificationRepository.cs
    │   └── InboxRepository.cs
    ├── Services/
    │   ├── Contracts/INotificationService.cs
    │   └── NotificationService.cs
    ├── Assets/EmailTemplates/
    │   └── FAMILY_MEMBER_REMOVED_TEMPLATE.html
    ├── appsettings.json
    ├── Program.cs
    └── Touir.ExpensesManager.Notifications.csproj
```

Also create `backend/notifications/Touir.ExpensesManager.Notifications.Tests/` following the test project pattern.

### 2a. `Notification` model

```csharp
public class Notification
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; } = null!;    // "FAMILY_MEMBER_REMOVED"
    public string Payload { get; set; } = null!; // JSON
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}
```

### 2b. `InboxEvent` model

Copy from `backend/expenses/…/Models/InboxEvent.cs` verbatim — same schema, same deduplication purpose.

### 2c. `NotificationsDbContext`

Registers only `DbSet<Notification>` and `DbSet<InboxEvent>`.

`OnModelCreating` for `Notification`:
- `varchar(100)` for `Type`
- `HasDefaultValue(false)` for `IsRead`
- FK to `ext.USR_Users` with `DeleteBehavior.Restrict`
- Composite index `(UserId, CreatedAt)`
- Partial index `(UserId) WHERE "IsRead" = false`

`OnModelCreating` for `InboxEvent`: unique index on `MessageId` (same as expenses service).

Initial migration name: `InitialCreate`.

### 2d. `INotificationRepository` / `NotificationRepository`

```csharp
Task<Notification> CreateAsync(Notification notification);
Task<IEnumerable<Notification>> GetByUserAsync(int userId, int page, int pageSize);   // ORDER BY CreatedAt DESC
Task<int> GetUnreadCountAsync(int userId);
Task<Notification?> GetByIdAsync(long id, int userId);
Task MarkAsReadAsync(long id, int userId);
Task MarkAllAsReadAsync(int userId);
```

### 2e. `IInboxRepository` / `InboxRepository`

Copy from expenses service — identical interface and implementation:
```csharp
Task<bool> ExistsAsync(string messageId);
Task AddAsync(InboxEvent inboxEvent);
```

### 2f. `FamilyEventConsumer` (BackgroundService)

Mirrors `UserEventConsumer` in expenses service.

```
Exchange: expenses.events   (Topic, durable — must match what expenses service declares)
Queue:    notifications.expenses.sync   (durable)
Binding:  family.#
```

`OnMessageReceivedAsync`:
1. Deserialize `FamilyEventMessage`
2. Extract `MessageId` from `ea.BasicProperties.MessageId` (fallback: new GUID)
3. `IInboxRepository.ExistsAsync(messageId)` — if duplicate, ack and return
4. `INotificationService.HandleFamilyMemberRemovedAsync(message)`
5. Write `InboxEvent { Status = "Processed" }`, then ack
6. On exception: nack without requeue

Uses `IServiceScopeFactory` to resolve scoped services — same pattern as `UserEventConsumer`.

### 2g. `INotificationService` / `NotificationService`

```csharp
Task HandleFamilyMemberRemovedAsync(FamilyEventMessage message);
Task<IEnumerable<Notification>> GetNotificationsAsync(int userId, int page, int pageSize);
Task<int> GetUnreadCountAsync(int userId);
Task MarkAsReadAsync(long notificationId, int userId);
Task MarkAllAsReadAsync(int userId);
```

`HandleFamilyMemberRemovedAsync` execution order:
1. **Persist** via `INotificationRepository.CreateAsync` — NOT wrapped (real failure)
2. **Hub push** via `IHubContext<NotificationHub>.Clients.Group(userId.ToString()).SendAsync("notification", dto)` — wrapped in try/catch
3. **Email** via `IEmailHelper` — wrapped in try/catch

Payload JSON stored in `Notification.Payload`:
```json
{
  "type": "FAMILY_MEMBER_REMOVED",
  "familyId": 2,
  "familyName": "Smith Family",
  "removedByUserId": 3,
  "removedByName": "John Smith",
  "expenseCount": 5
}
```

### 2h. `NotificationHub`

File: `Hubs/NotificationHub.cs`

Copy `JwtCookieReader.cs` from expenses service. Hub reads `userId` from `auth_token` cookie (nginx already validated it).

```csharp
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var userId = httpContext is null ? null : JwtCookieReader.GetUserId(httpContext.Request);
        if (userId is null) { Context.Abort(); return; }
        await Groups.AddToGroupAsync(Context.ConnectionId, userId.Value.ToString());
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext is not null)
        {
            var userId = JwtCookieReader.GetUserId(httpContext.Request);
            if (userId is not null)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.Value.ToString());
        }
        await base.OnDisconnectedAsync(exception);
    }
}
```

Hub route in `Program.cs`: `app.MapHub<NotificationHub>("/ws/notifications")`

### 2i. `NotificationController`

```
GET  /notifications?page=1&pageSize=20   → 200 NotificationDto[]
GET  /notifications/unread-count         → 200 { count: int }
POST /notifications/{id}/read            → 204
POST /notifications/read-all             → 204
```

Auth: `JwtCookieReader.GetUserId(Request)` → 401 if null. `NotificationDto` maps `Payload` string → `JsonElement`. Applies rate limiting (same sliding-window policy as expenses service).

### 2j. Email template

File: `Assets/EmailTemplates/FAMILY_MEMBER_REMOVED_TEMPLATE.html`

Mirrors `FAMILY_INVITATION_TEMPLATE.html` from expenses service (same CSS/layout). Placeholders: `@@REMOVED_BY_NAME@@`, `@@FAMILY_NAME@@`, `@@EXPENSE_COUNT@@`, `@@YEAR@@`.

Copy `EmailHelper` + `SmtpEmailService` + `IEmailHelper` + `IEmailService` verbatim from expenses service. Add local `EmailHtmlTemplate.cs` static class with key and variable name constants.

Register in `.csproj`:
```xml
<None Remove="Assets\EmailTemplates\FAMILY_MEMBER_REMOVED_TEMPLATE.html" />
<EmbeddedResource Include="Assets\EmailTemplates\FAMILY_MEMBER_REMOVED_TEMPLATE.html">
  <CopyToOutputDirectory>Always</CopyToOutputDirectory>
</EmbeddedResource>
```

### 2k. `appsettings.json`

```json
{
  "Postgres":   { "ConnectionString": "" },
  "RabbitMQ":   { "HostName": "rabbitmq", "Port": 5672, "UserName": "", "Password": "", "VirtualHost": "expense_management" },
  "EmailAuth":  { "Email": "", "Password": "", "Host": "", "Port": 587, "EnableSsl": true }
}
```

All values overridden by env vars (same `GetValue("Key", env) ?? hardcoded` pattern as other services).

### 2l. `Program.cs`

```csharp
builder.Services.AddDbContext<NotificationsDbContext>(...);
builder.Services.AddSignalR();   // built into Microsoft.NET.Sdk.Web — no extra NuGet
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IEmailHelper, EmailHelper>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IInboxRepository, InboxRepository>();
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddHostedService<FamilyEventConsumer>();

// ...

app.MapControllers();
app.MapHub<NotificationHub>("/ws/notifications");
await MigrateAsync(app);   // auto-migrate at startup
```

### 2m. NuGet packages (`.csproj`)

Same as expenses service minus CsvHelper and Quartz:
```xml
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.22" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.22" />
<PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.11" />
<PackageReference Include="RabbitMQ.Client" Version="6.8.1" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="8.0.0" />
```

---

## Part 3 — Infrastructure

### 3a. `docker-compose-apps.yml`

Add `notifications-service` following the same pattern as `expenses-service`:

```yaml
notifications-service:
  image: localhost:5050/expenses-manager/expensemanager/expense-manager-backend-notifications:latest
  ports:
    - "9300:9300"
  environment:
    ASPNETCORE_URLS: "http://+:9300"
    EXPENSES_MANAGEMENT_NOTIFICATIONS_POSTGRES_CONNECTIONSTRING: "..."
    EXPENSES_MANAGEMENT_NOTIFICATIONS_RABBITMQ_HOSTNAME: rabbitmq
    EXPENSES_MANAGEMENT_NOTIFICATIONS_RABBITMQ_PORT: 5672
    EXPENSES_MANAGEMENT_NOTIFICATIONS_RABBITMQ_USERNAME: expense_notifications
    EXPENSES_MANAGEMENT_NOTIFICATIONS_RABBITMQ_PASSWORD: "..."
    EXPENSES_MANAGEMENT_NOTIFICATIONS_RABBITMQ_VIRTUALHOST: expense_management
    EXPENSES_MANAGEMENT_NOTIFICATIONS_EMAILAUTH_EMAIL: "..."
    EXPENSES_MANAGEMENT_NOTIFICATIONS_EMAILAUTH_PASSWORD: "..."
    EXPENSES_MANAGEMENT_NOTIFICATIONS_EMAILAUTH_HOST: "..."
    EXPENSES_MANAGEMENT_NOTIFICATIONS_EMAILAUTH_PORT: 587
    EXPENSES_MANAGEMENT_NOTIFICATIONS_EMAILAUTH_ENABLE_SSL: "true"
  depends_on:
    - rabbitmq
    - postgres
  networks:
    - expenses_manager_apps_net
```

Also add a `notifications` database to the PostgreSQL init script.

### 3b. RabbitMQ provisioning

Add `expense_notifications` user with read permissions on vhost `expense_management` to the Nexus/RabbitMQ provisioning scripts (`infrastructure/configs/nexus/`). The notifications service only consumes — no publish needed.

### 3c. Nginx — `infrastructure/configs/nginx/sites-available/expenses-manager.conf`

The `$connection_upgrade` map already exists (lines 7–10). Add two blocks **before the existing `/api/expenses` block** (currently line 99):

```nginx
# WebSocket block — more specific prefix, must come before /api/notifications
location ^~ /api/notifications/ws/ {
    auth_request /internal/auth/check;

    rewrite ^/api/notifications/?(.*)$ /$1 break;
    proxy_pass http://notifications-service:9300/;

    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection $connection_upgrade;
    proxy_set_header Host $host;
    proxy_cache_bypass $http_upgrade;

    proxy_read_timeout 3600s;
    proxy_send_timeout 3600s;

    add_header 'Access-Control-Allow-Origin' $cors_origin always;
    add_header 'Access-Control-Allow-Credentials' 'true' always;
}

# REST API
location /api/notifications {
    auth_request /internal/auth/check;

    rewrite ^/api/notifications/?(.*)$ /$1 break;
    proxy_pass http://notifications-service:9300/;

    add_header 'Access-Control-Allow-Origin' $cors_origin always;
    add_header 'Access-Control-Allow-Methods' $cors_methods always;
    add_header 'Access-Control-Allow-Headers' $cors_headers always;
    add_header 'Access-Control-Allow-Credentials' 'true' always;

    if ($request_method = 'OPTIONS') {
        add_header 'Access-Control-Max-Age' '1728000';
        add_header 'Content-Type' 'text/plain charset=UTF-8';
        add_header 'Content-Length' '0';
        add_header 'Access-Control-Allow-Origin' $cors_origin always;
        add_header 'Access-Control-Allow-Methods' $cors_methods always;
        add_header 'Access-Control-Allow-Headers' $cors_headers always;
        add_header 'Access-Control-Allow-Credentials' 'true' always;
        return 204;
    }
}
```

SignalR's negotiate POST (`/api/notifications/ws/notifications/negotiate`) falls under the `/api/notifications/ws/` block.

---

## Part 4 — Frontend

### npm dependency

```
npm install @microsoft/signalr
```

### Type definitions

New file: `src/features/notifications/types/notification.type.ts`

```ts
export type NotificationPayloadFamilyMemberRemoved = {
  type: 'FAMILY_MEMBER_REMOVED'
  familyId: number
  familyName: string
  removedByUserId: number
  removedByName: string
  expenseCount: number
}

export type NotificationPayload = NotificationPayloadFamilyMemberRemoved

export type Notification = {
  id: number
  type: string
  payload: NotificationPayload
  isRead: boolean
  createdAt: string
  readAt: string | null
}
```

### API service

New file: `src/features/notifications/services/notificationApi.service.ts`

```ts
const BASE = '/api/notifications'

getNotifications(page, pageSize) → GET ${BASE}?page=&pageSize=
getUnreadCount()                 → GET ${BASE}/unread-count
markAsRead(id)                   → POST ${BASE}/${id}/read
markAllAsRead()                  → POST ${BASE}/read-all
```

Uses existing `get`/`post` from `api.service.ts`.

### `NotificationContext`

New file: `src/features/notifications/NotificationContext.tsx`

- Exposes: `notifications`, `unreadCount`, `isLoading`, `markRead(id)`, `markAllRead()`, `refresh()`
- Stores `HubConnection` in `useRef<HubConnection | null>` — calls `connRef.current?.stop()` in cleanup
- On `isAuthenticated = true`: fetch initial list + unread count; start connection

```ts
const conn = new HubConnectionBuilder()
  .withUrl('/api/notifications/ws/notifications', { withCredentials: true })
  .withAutomaticReconnect()
  .build()

conn.on('notification', (notif: Notification) => {
  setNotifications(prev => [notif, ...prev])
  setUnreadCount(c => c + 1)
})
conn.start()
```

On `isAuthenticated = false`: `connRef.current?.stop()`, clear state.

### `NotificationBell` component

New file: `src/features/notifications/components/NotificationBell.tsx`

- Bell icon + red badge (`unreadCount ≤ 9` shows count, `> 9` shows `9+`)
- Dropdown toggles on click; outside-click closes (same `useRef + mousedown` pattern as user menu)
- Dropdown: "Mark all as read" button + notification list newest-first
- Each row: i18n message from payload, relative time, bold/dot for unread; row click calls `markRead(n.id)`
- Toast on new notification — track via `useEffect` comparing `prevRef` to `unreadCount`:

```ts
const prevRef = useRef(unreadCount)
useEffect(() => {
  if (unreadCount > prevRef.current && notifications[0]) {
    show(t('notifications.familyMemberRemoved', { ...notifications[0].payload }), 'info')
  }
  prevRef.current = unreadCount
}, [unreadCount, notifications])
```

### `AppProviders` integration

File: `src/providers/AppProviders.tsx`

Add `NotificationProvider` after `DisplayCurrencyProvider` in `composeProviders`.

### NavBar integration

Replace placeholder notification `<button>` with `<NotificationBell />`.

### i18n keys

Add to all 4 locale files (`en`, `fr`, `es`, `de`) under `"notifications"`:

```json
"notifications": {
  "title": "Notifications",
  "markAllRead": "Mark all as read",
  "empty": "No notifications yet",
  "familyMemberRemoved": "{{removedByName}} removed you from \"{{familyName}}\". {{expenseCount}} attribution(s) cleared.",
  "badge": "{{count}} unread"
}
```

---

## Implementation order

```
Expenses service
  FamilyEventMessage.cs
  IFamilyEventPublisher + FamilyEventPublisher
  IFamilyRepository.CountMemberAttributionsAsync
  FamilyService (inject publisher, count before delete, publish after)
  Program.cs (register IFamilyEventPublisher)

Notifications microservice
  .csproj + solution entry
  Notification.cs + InboxEvent.cs
  NotificationsDbContext.cs
  dotnet ef migrations add InitialCreate
  INotificationRepository + NotificationRepository
  IInboxRepository + InboxRepository
  Email layer (copy from expenses service)
  EmailHtmlTemplate.cs + template HTML + .csproj entry
  INotificationService + NotificationService
  JwtCookieReader.cs (copy from expenses service)
  NotificationHub
  FamilyEventConsumer
  NotificationController + NotificationDto
  Program.cs
  appsettings.json

Infrastructure
  docker-compose-apps.yml (add notifications-service + notifications DB)
  RabbitMQ provisioning (add expense_notifications user)
  Nginx conf (add two location blocks before /api/expenses)

Frontend
  npm install @microsoft/signalr
  notification.type.ts
  notificationApi.service.ts
  NotificationContext.tsx
  NotificationBell.tsx
  AppProviders.tsx
  NavBar.tsx
  translation files ×4
```

---

## Tests

### Expenses service

- `FamilyServiceTests` — extend `RemoveMemberAsync` tests: assert `IFamilyEventPublisher.PublishAsync` called with correct payload; assert publish failure doesn't abort removal

### Notifications microservice

- `NotificationRepositoryTests` — integration; create, unread count, mark read, mark all
- `NotificationServiceTests` — unit (Moq); persist → hub push → email order; hub failure non-fatal; email failure non-fatal
- `FamilyEventConsumerTests` — unit; duplicate `MessageId` → ack without processing; valid message → `HandleFamilyMemberRemovedAsync` called; exception → nack

### Frontend

- `notificationApi.service.test.ts` — each function hits correct path + method
- `NotificationContext.test.tsx` — mock `@microsoft/signalr`; state updates on `notification` event
- `NotificationBell.test.tsx` — badge with `unreadCount > 0`; dropdown toggles; mark-all calls `markAllRead`

---

## Verification

1. `dotnet build` both projects — no errors
2. `dotnet test` — all tests pass
3. `npm test` — all frontend tests pass
4. Start full docker-compose stack
5. Log in as user A (head) and user B (member, open separate tab)
6. Head removes user B from a family via UI or `DELETE /api/expenses/families/{id}/members/{userId}`
7. Verify: user B's bell badge shows 1; toast appears; dropdown shows removal message; notification persists after reload; marking read clears badge; email arrives at user B's inbox
