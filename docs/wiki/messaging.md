# Messaging — RabbitMQ, Outbox & Inbox

← [Wiki Index](./index.md)

---

## Overview

ExpenseManager uses RabbitMQ for async event propagation between the users service and the expenses service. A transactional **outbox pattern** on the publisher side and an **inbox deduplication** pattern on the consumer side together guarantee at-least-once, exactly-once-processed delivery.

---

## RabbitMQ Topology

```
users-service
  └─ publishes to: users.events (topic exchange)
       routing keys: user.created | user.updated | user.deleted

expenses-service
  └─ consumes from: expenses.users.sync (queue)
       binding: user.# on users.events exchange
```

**Vhost:** `expense_management`  
Both services use credentials with permissions only on this vhost — not on the default `/` vhost.

**users service credentials:** `expense_users`  
**expenses service credentials:** `expense_expenses`

**`RabbitMQService`** requires `DispatchConsumersAsync = true` for async message handling in the expenses consumer.

---

## Outbox Pattern (users service)

The problem: writing a DB record and publishing a message to RabbitMQ cannot be made atomic without distributed transactions. If the service crashes after writing the user but before publishing, the event is lost.

**Solution:** Write an outbox event row in the same database as the user, then have a background service publish it.

```
RegistrationService
  ├─ IUserRepository.ValidateEmailAsync()     → writes USR_Users (save user)
  └─ IOutboxRepository.EnqueueAsync()         → writes MSG_OutboxEvents (no shared transaction)

OutboxPublisherService (BackgroundService, polls every 5 s)
  ├─ Fetch pending events from MSG_OutboxEvents
  ├─ For each: IUserEventPublisher.PublishRaw(eventType, payload, messageId)
  ├─ On success: mark event Sent, set PublishedAt
  └─ On failure: increment RetryCount; mark Failed after 5 retries
```

**`MSG_OutboxEvents` fields:**

| Field | Description |
|---|---|
| `EventType` | `user.created`, `user.updated`, `user.deleted` |
| `Payload` | JSON body of the event |
| `MessageId` | Unique ID used as RabbitMQ `BasicProperties.MessageId` for inbox dedup |
| `Status` | `Pending` → `Sent` / `Failed` |
| `RetryCount` | Max 5 before marking Failed |

**Important:** `RegistrationService.ValidateEmailAsync` calls `IUserRepository.ValidateEmailAsync` (saves user) then independently calls `IOutboxRepository.EnqueueAsync` — **no shared transaction**. This is intentional: the outbox write can be retried independently.

### Replay

Stuck or failed events can be re-published without restarting the service:

```
POST /api/users/messaging/replay
  ?eventType=user.created
  &from=2026-05-01T00:00:00Z
  &forceAll=true      ← re-publish already-Sent events too
```

Rate limited: `messaging_replay` — 5 req / 1 min.

### Stats

```
GET /api/users/messaging/outbox/stats
```

Returns `{ pending, sent, failed }` counts. No authentication required, no rate limit.

---

## Inbox Pattern (expenses service)

The problem: RabbitMQ delivers at-least-once. If the consumer processes a message but crashes before acking, the message is redelivered. Processing it twice could corrupt data (duplicate user sync, duplicate family creation).

**Solution:** Store processed `MessageId` values in an `InboxEvents` table; skip already-seen messages.

```
UserEventConsumer.ExecuteAsync
  │
  ├─ Extract MessageId from ea.BasicProperties.MessageId
  │   └─ Fallback: generate new GUID (legacy messages without MessageId)
  │
  ├─ IInboxRepository.ExistsAsync(messageId)
  │   ├─ true  → ack, skip (duplicate)
  │   └─ false → proceed
  │
  ├─ Route by eventType:
  │   ├─ user.created  → SaveOrUpdateUserAsync + FamilyService.CreateDefaultAsync
  │   ├─ user.updated  → SaveOrUpdateUserAsync
  │   └─ user.deleted  → DeleteUserAsync
  │
  ├─ On success:
  │   ├─ Write InboxEvent { MessageId, Status="Processed", ProcessedAt }
  │   └─ channel.BasicAck
  │
  └─ On failure:
      └─ channel.BasicNack (no inbox write → message can be redelivered)
```

**Startup resilience:**  
`UserEventConsumer.ExecuteAsync` catches `BrokerUnreachableException` and retries the connection every 5 s. Both services can start before RabbitMQ is ready.

---

## Event Payloads

### user.created / user.updated

```json
{
  "UserId": 42,
  "Email": "jane@example.com",
  "FirstName": "Jane",
  "LastName": "Doe"
}
```

### user.deleted

```json
{
  "UserId": 42
}
```

---

## Configuration

### Users Service

| Environment Variable | Description |
|---|---|
| `EXPENSES_MANAGEMENT_USERS_RABBITMQ_HOSTNAME` | RabbitMQ host |
| `EXPENSES_MANAGEMENT_USERS_RABBITMQ_PORT` | Port (default 5672) |
| `EXPENSES_MANAGEMENT_USERS_RABBITMQ_USERNAME` | Credentials |
| `EXPENSES_MANAGEMENT_USERS_RABBITMQ_PASSWORD` | Credentials |

Config key pattern: `GetValue("Key", envVar) ?? hardcoded` — `appsettings.json` takes priority over env vars.

### Expenses Service

| Environment Variable | Description |
|---|---|
| `EXPENSES_MANAGEMENT_EXPENSES_RABBITMQ_HOSTNAME` | RabbitMQ host |
| `EXPENSES_MANAGEMENT_EXPENSES_RABBITMQ_PORT` | Port (default 5672) |
| `EXPENSES_MANAGEMENT_EXPENSES_RABBITMQ_USERNAME` | Credentials |
| `EXPENSES_MANAGEMENT_EXPENSES_RABBITMQ_PASSWORD` | Credentials |

---

## End-to-End Flow: User Registration → Default Family

```
1. POST /api/users/auth/register
      │
      ▼
2. RegistrationService
   ├─ Creates USR_Users (IsEmailValidated=false)
   └─ Enqueues MSG_OutboxEvents { EventType="user.created", ... }

3. OutboxPublisherService (≤5 s later)
   └─ Publishes to users.events exchange (routing key: user.created)
      MessageId = MSG_OutboxEvents.MessageId

4. UserEventConsumer (expenses-service)
   ├─ ExistsAsync check → not duplicate
   ├─ SaveOrUpdateUserAsync (creates/updates External.User)
   ├─ FamilyService.CreateDefaultAsync(userId)   ← idempotent
   ├─ Writes InboxEvent { MessageId, Status="Processed" }
   └─ BasicAck
```

The default family is provisioned asynchronously after registration — it is ready before the user can log in and create expenses.

---

## Monitoring & Debugging

- **RabbitMQ Management UI:** `http://localhost:15672`  
  Inspect queues, bindings, message rates, and unacked counts.
- **Outbox stats:** `GET /api/users/messaging/outbox/stats`  
  Check for stuck pending or failed events.
- **Replay:** `POST /api/users/messaging/replay?forceAll=true`  
  Re-publish failed events after fixing an issue.
- **Dead-letter:** Events that exceed 5 retries are marked `Failed` in `MSG_OutboxEvents` — they are not sent to a DLQ. Use the replay endpoint to retry.
