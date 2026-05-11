# Family System

← [Wiki Index](./index.md)

---

## Overview

The family system lets users group together and share expense attribution. Every user automatically gets a **default family** when their account is created. Additional named families can be created, members invited, and expenses attributed to one or more families.

All family logic lives in the expenses service (`FamilyService`, `FamilyController`, `FamilyRepository`).

---

## Core Concepts

| Concept | Description |
|---|---|
| **Family** | A named group with one or more members |
| **Default family** | Auto-created per user (idempotent); cannot be archived |
| **Member** | A user inside a family with a role (Head or Member) |
| **Head** | Full control: rename, archive, invite, remove members, change roles |
| **Member** | Can view the family and have expenses attributed to it |
| **Invitation** | A GUID token sent by a Head; scoped to an email address |
| **Attribution** | A row linking an expense to a family (`FAM_ExpenseFamilyAttributions`) |

---

## Family Lifecycle

### Default Family (automatic)

```
user.created event received by UserEventConsumer
  └─ FamilyService.CreateDefaultAsync(userId)
       ├─ HasDefaultFamilyAsync check (idempotent — safe to replay)
       └─ Creates Family { Name="{FirstName}'s Family", IsDefault=true, OwnerId=userId }
           └─ Owner added as Head member
```

Default families:
- Cannot be archived
- Cannot be renamed away from "default" status
- Always available as the auto-attribution target

### Named Family

```
POST /families  { "name": "Smith Family" }
  └─ FamilyService.CreateAsync(userId, name)
       ├─ Creates Family { IsDefault=false }
       └─ Creator added as Head member
```

### Archive (soft-delete)

Only non-default families can be archived. `IsDeleted=true`, `DeletedAt` set. The family still exists and attributions are preserved — it is just hidden from normal listing.

`POST /families/{id}/archive` / `POST /families/{id}/unarchive`

---

## Invitation Flow

```
1. HEAD: POST /families/{id}/invite  { "email": "friend@example.com" }
         └─ FamilyService.InviteAsync
              ├─ IUserRepository.GetUserByEmailAsync → resolve invited user (or future user)
              ├─ Create FamilyInvitation {
              │    Token = Guid.NewGuid().ToString(),
              │    InviteeEmail = email,
              │    ExpiresAt = UtcNow + FamilyOptions.InviteExpiryInDays (default 7 days),
              │    IsAccepted = false
              │  }
              └─ Returns invitation token

2. INVITEE: POST /families/{id}/accept-invite  { "token": "<guid>" }
            └─ FamilyService.AcceptInviteAsync(userId, token)
                 ├─ Load invitation by token
                 ├─ Validate: not expired, not already accepted
                 ├─ Validate: acceptor's email == InviteeEmail
                 ├─ Mark IsAccepted = true
                 └─ Add user as FamilyMember { RoleId = Member }
```

**`InviteExpiryInDays`** is configurable:  
`EXPENSES_MANAGEMENT_EXPENSES_FAMILY_INVITE_EXPIRY_IN_DAYS` (default `7`)

---

## Role Management

Roles are resolved from the `FamilyRole` lookup table via `ILookupCacheService.GetIdAsync<FamilyRole>`. Seed IDs: `1=Head`, `2=Member`. Role name is normalised to title case before lookup — never hardcode IDs.

**Change role:**

```
PUT /families/{id}/members/{memberId}/role  { "roleName": "Head" }
  └─ FamilyService.ChangeRoleAsync(familyId, memberId, roleName)
       └─ resolves roleId via ILookupCacheService.GetIdAsync<FamilyRole>("Head")
```

Only a Head can change roles.

---

## Expense Attribution

When creating or updating an expense, the `FamilyIds int[]?` field controls attribution:

| Value | Behaviour |
|---|---|
| `null` | Auto-attribute to user's **default family** |
| `[]` (empty array) | No attribution — expense is personal only |
| `[1, 3]` (IDs) | Validate user is a member of each family; write one `ExpenseFamilyAttribution` row per ID |

`FamilyForbiddenException` is thrown (→ 403) if the user is not a member of a provided family ID.

Attribution rows are written by `IFamilyRepository` inside `ExpenseService` — keeping attribution logic co-located with expense mutation.

### Remove Member + Attribution Cleanup

```
DELETE /families/{id}/members/{memberId}
  └─ FamilyService.RemoveMemberAsync(familyId, memberId)
       ├─ Remove FamilyMember row
       └─ RemoveMemberAttributionsAsync(familyId, memberId)
            └─ Deletes all ExpenseFamilyAttribution rows
               where FamilyId = familyId AND Expense.UserId = memberId
```

This prevents orphaned attributions for removed members.

---

## Exception → HTTP Mapping

| Exception | Controller Response | Meaning |
|---|---|---|
| `FamilyNotFoundException` | 404 | Family or member not found |
| `FamilyForbiddenException` | 403 | Operation requires Head role, or user not a member |
| `FamilyConflictException` | 409 | Conflict (e.g. archiving default family) |
| `FamilyInvitationException` | 400 | Invalid/expired/accepted token, or email mismatch |

---

## Data Model Summary

```
FAM_Families
  │ 1:*
FAM_FamilyMembers ─── FamilyRole (lookup)
  │
FAM_FamilyInvitations

EXP_Expenses
  │ *:*  (via FAM_ExpenseFamilyAttributions)
FAM_Families
```

See [Data Models](./data-models.md) for full column definitions.

---

## API Endpoints Summary

| Method | Path | Who | Description |
|---|---|---|---|
| GET | `/families` | Any member | List user's families |
| GET | `/families/{id}` | Any member | Family detail + members |
| POST | `/families` | Authenticated | Create named family |
| PUT | `/families/{id}/rename` | Head only | Rename family |
| POST | `/families/{id}/archive` | Head only | Soft-delete (non-default only) |
| POST | `/families/{id}/unarchive` | Head only | Restore |
| POST | `/families/{id}/invite` | Head only | Send invitation |
| POST | `/families/{id}/accept-invite` | Invitee | Join family |
| DELETE | `/families/{id}/members/{memberId}` | Head only | Remove member + purge attributions |
| PUT | `/families/{id}/members/{memberId}/role` | Head only | Change member role |

---

## Configuration

| Environment Variable | Default | Description |
|---|---|---|
| `EXPENSES_MANAGEMENT_EXPENSES_FAMILY_INVITE_EXPIRY_IN_DAYS` | `7` | Days before an invitation token expires |

---

## Testing

Family tests live in the expenses test project:

| File | Coverage |
|---|---|
| `FamilyServiceTests.cs` | 42 unit tests covering all service methods |
| `FamilyControllerTests.cs` | 30+ tests covering all 10 endpoints and exception → HTTP mapping |
| `FamilyRepositoryTests.cs` | Repository CRUD + attribution helpers |
| `FamilyValidatorTests.cs` | FluentValidation for all family request DTOs |
