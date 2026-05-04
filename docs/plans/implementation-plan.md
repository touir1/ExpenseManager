# ExpenseManager — Implementation Plan

Reference: [application-description.md](application-description.md)

## Current State

| Area | Status |
|------|--------|
| Users service | ✅ Complete — auth, registration, JWT, refresh tokens, password management, FluentValidation |
| Expenses service | ⚠️ Skeleton — `Category`, `Currency`, `Expense` models defined but not migrated; no controllers or services; `RabbitMQService` and external `UserRepository` exist |
| Frontend | ⚠️ Auth flows complete; dashboard is "Coming soon" placeholder; i18n wired |
| Infrastructure | ✅ Docker Compose, nginx, PostgreSQL, RabbitMQ, Grafana, Prometheus |

---

## Phases Overview

| Phase | Name | Scope |
|-------|------|-------|
| 1 | Schema Foundation | Design and migrate all DB tables |
| 2 | Categories & Currencies (read) | Admin data, no expense logic yet |
| 3 | Core Expense CRUD | Add / edit / delete + audit |
| 4 | Family System | Default family, membership, attribution |
| 5 | Tags | User-scoped tags on expenses |
| 6 | Currency Rates | Daily rates, resolution logic, auto-update |
| 7 | Dashboard API | Aggregation and stats endpoints |
| 8 | Frontend — Expense List & Form | Web UI for expenses |
| 9 | Frontend — Dashboard | Charts and summary |
| 10 | Frontend — Family Management | Web UI for families |
| 11 | Admin Screens | Categories, currencies, rates, rate validation, users |
| 12 | CSV Import | Bulk expense upload |
| 13 | Notifications | In-app + email on attribution removal |
| 14 | PWA / Mobile | Progressive web app, offline, quick-add |
| 15 | Suggested Additions | Budgets, recurring, splitting, reports |

---

## Phase 1 — Schema Foundation

**Goal:** Establish the complete database schema in a single migration pass before building services. Avoids costly structural migrations later.

### Existing models to update

| Model | Change |
|-------|--------|
| `Category` | Add `is_archived`, keep self-referencing `parent_id` for subcategory (2-level max enforced in service) |
| `Currency` | Keep as-is; fallback rates live in a new `CurrencyPairDefault` table |
| `Expense` | Major overhaul — see below |

### `Expense` table (revised)

Replace current model with:

| Column | Type | Notes |
|--------|------|-------|
| `id` | PK | |
| `user_id` | FK → ext.USR_Users | Owner |
| `amount` | decimal(18,4) | Original amount |
| `currency_id` | FK → Currency | |
| `date` | date | Expense date (not creation timestamp) |
| `category_id` | FK? → Category | Nullable |
| `subcategory_id` | FK? → Category | Nullable; must have same parent as category_id |
| `description` | varchar(500) | |
| `created_at` | timestamp | |
| `created_by` | FK → ext.USR_Users | |
| `created_from` | enum | `single_web`, `single_mobile`, `bulk_web` |
| `modified_at` | timestamp? | |
| `modified_by` | FK? → ext.USR_Users | |
| `modified_from` | enum? | `web`, `mobile` |

### New tables

**`Family`**

| Column | Type |
|--------|------|
| `id` | PK |
| `name` | varchar(100) |
| `is_default` | bool |
| `is_archived` | bool |
| `created_at` | timestamp |
| `created_by` | FK → ext.USR_Users |

**`FamilyMembership`**

| Column | Type |
|--------|------|
| `id` | PK |
| `family_id` | FK → Family |
| `user_id` | FK → ext.USR_Users |
| `role` | enum: `head`, `member` |
| `joined_at` | timestamp |

**`ExpenseFamilyAttribution`**

| Column | Type |
|--------|------|
| `id` | PK |
| `expense_id` | FK → Expense |
| `family_id` | FK → Family |
| `attributed_at` | timestamp |
| `attributed_by` | FK → ext.USR_Users |

**`Tag`**

| Column | Type |
|--------|------|
| `id` | PK |
| `name` | varchar(50) |
| `user_id` | FK → ext.USR_Users |

**`ExpenseTag`**

| Column | Type |
|--------|------|
| `expense_id` | FK → Expense |
| `tag_id` | FK → Tag |
| PK | composite (expense_id, tag_id) |

**`CurrencyDailyRate`**

| Column | Type | Notes |
|--------|------|-------|
| `id` | PK | |
| `source_currency_id` | FK → Currency | |
| `destination_currency_id` | FK → Currency | |
| `date` | date | |
| `rate` | decimal(18,8) | |
| `source` | enum | `auto`, `manual` |
| UNIQUE | (source_currency_id, destination_currency_id, date) | |

**`CurrencyPairDefault`**

| Column | Type | Notes |
|--------|------|-------|
| `source_currency_id` | FK → Currency | PK composite |
| `destination_currency_id` | FK → Currency | PK composite |
| `rate` | decimal(18,8) | Fallback when no daily rate found |

**`CurrencyRateConflict`**

| Column | Type | Notes |
|--------|------|-------|
| `id` | PK | |
| `source_currency_id` | FK → Currency | |
| `destination_currency_id` | FK → Currency | |
| `date` | date | |
| `automatic_rate` | decimal(18,8) | Rate from auto-update process |
| `manual_rate` | decimal(18,8) | Existing manually-entered rate |
| `status` | enum | `pending`, `resolved` |
| `resolved_at` | timestamp? | |
| `resolved_by` | FK? → ext.USR_Users | |
| `resolution` | enum? | `accept_auto`, `keep_manual`, `custom` |
| `custom_rate` | decimal(18,8)? | Set only if resolution = `custom` |

**`ExpenseAuditLog`**

| Column | Type |
|--------|------|
| `id` | PK |
| `expense_id` | FK → Expense |
| `operation` | enum: `add`, `update`, `delete` |
| `performed_at` | timestamp |
| `performed_by` | FK → ext.USR_Users |
| `performed_from` | enum: `single_web`, `single_mobile`, `bulk_web` |

**`ExpenseAuditSnapshot`**

| Column | Type | Notes |
|--------|------|-------|
| `id` | PK | |
| `audit_log_id` | FK → ExpenseAuditLog | |
| `snapshot_type` | enum | `before`, `after` |
| `amount` | decimal(18,4) | |
| `currency_id` | FK → Currency | |
| `date` | date | |
| `category_id` | FK? → Category | |
| `subcategory_id` | FK? → Category | |
| `description` | varchar(500) | |
| `tags` | varchar(1000) | Comma-separated tag IDs |
| `families` | varchar(1000) | Comma-separated family IDs |

### Tasks

- [ ] Update `Category` model — add `IsArchived`
- [ ] Rewrite `Expense` model with new columns
- [ ] Add `Family`, `FamilyMembership`, `ExpenseFamilyAttribution` models
- [ ] Add `Tag`, `ExpenseTag` models
- [ ] Add `CurrencyDailyRate`, `CurrencyPairDefault`, `CurrencyRateConflict` models
- [ ] Add `ExpenseAuditLog`, `ExpenseAuditSnapshot` models
- [ ] Update `ExpensesDbContext` — register all new models, configure constraints/indexes
- [ ] Generate and apply EF Core migration

### Key indexes to add
- `Expense`: `(user_id, date)`, `(category_id)`, `(currency_id)`
- `ExpenseFamilyAttribution`: `(family_id)`, `(expense_id)`
- `CurrencyDailyRate`: `(source, destination, date DESC)` — critical for rate resolution query
- `ExpenseAuditLog`: `(expense_id)`
- `FamilyMembership`: `(user_id)`, `(family_id)`

---

## Phase 2 — Categories & Currencies (Read)

**Goal:** Expose read endpoints so the frontend can populate dropdowns before expenses exist.

### Backend

- [ ] `CategoryService` — `GetAllAsync()` (tree: categories with subcategory children, excluding archived)
- [ ] `CurrencyService` — `GetAllAsync()`
- [ ] `CategoryController` — `GET /categories`
- [ ] `CurrencyController` — `GET /currencies`
- [ ] FluentValidation not needed (read-only, no request bodies)
- [ ] Unit tests for services

### Frontend

- [ ] `categoriesApi.service.ts` — fetch categories tree
- [ ] `currenciesApi.service.ts` — fetch currencies list
- [ ] Store both in React context or TanStack Query cache (available globally)

**Depends on:** Phase 1

---

## Phase 3 — Core Expense CRUD

**Goal:** Users can add, edit, delete their own expenses. Audit trail written on every operation.

### Backend

- [ ] `ExpenseService`
  - `AddAsync(request, userId, source)` → writes expense + audit log (operation: `add`, 1 `after` snapshot)
  - `UpdateAsync(id, request, userId, source)` → writes audit log (operation: `update`, `before` + `after` snapshots)
  - `DeleteAsync(id, userId)` → hard delete + audit log (operation: `delete`, 1 `before` snapshot)
  - `GetByIdAsync(id, userId)` — enforces ownership
  - `GetPagedAsync(filters, userId)` — paginated, filtered list (own expenses only for now)
- [ ] `ExpenseController`
  - `POST /expenses`
  - `PUT /expenses/{id}`
  - `DELETE /expenses/{id}`
  - `GET /expenses/{id}`
  - `GET /expenses` (paged + filtered)
- [ ] `ExpenseAuditService` — internal, called by `ExpenseService`; never exposed directly
- [ ] FluentValidation for create/update request DTOs
- [ ] Unit tests for service logic and audit writes

### Notes
- No family attribution yet (Phase 4); all expenses implicitly owned by user
- Category/subcategory optional; validate subcategory belongs to selected category if both provided
- No tags yet (Phase 5)

**Depends on:** Phase 1, Phase 2

---

## Phase 4 — Family System

**Goal:** Default family created on user registration. Users can create and manage families. Expenses attributed to one or more families.

### Backend (expenses service)

- [ ] `FamilyService`
  - `CreateDefaultAsync(userId)` — called by RabbitMQ consumer when new user registered
  - `CreateAsync(name, userId)`
  - `GetByUserAsync(userId)` — returns active + archived, with role per family
  - `InviteAsync(familyId, inviteeEmail, invitedBy)` — creates pending membership
  - `AcceptInviteAsync(token, userId)`
  - `RemoveMemberAsync(familyId, targetUserId, removedBy)` — head only
  - `ChangeRoleAsync(familyId, targetUserId, newRole, changedBy)` — head only
  - `RenameAsync(familyId, name, userId)` — head or default family owner
  - `ArchiveAsync(familyId, userId)` — head only, non-default
  - `UnarchiveAsync(familyId, userId)` — head only
- [ ] RabbitMQ consumer — listens for `user.registered` event from users service → calls `CreateDefaultAsync`
- [ ] Expense attribution: update `ExpenseService` to accept `familyIds[]`; validate user is member of each; Default family always included
- [ ] Head "remove from family" → deletes `ExpenseFamilyAttribution` rows for that family (not the expense)
- [ ] `FamilyController` — full CRUD + membership endpoints
- [ ] FluentValidation for all request DTOs
- [ ] Unit + integration tests

### Backend (users service)

- [ ] Publish `user.registered` event to RabbitMQ on successful registration

### Frontend

- [ ] `familyApi.service.ts`
- [ ] Family selector component (sidebar / top bar) — switch active family scope
- [ ] Family management screen (list, create, invite, archive, member management)
- [ ] Update expense add/edit form — family multi-select (Default always checked)

**Depends on:** Phase 3

---

## Phase 5 — Tags

**Goal:** Users can create and assign tags to expenses.

### Backend

- [ ] `TagService`
  - `GetByUserAsync(userId)` — for autocomplete
  - `CreateAsync(name, userId)` — idempotent (return existing if name matches)
  - `DeleteAsync(id, userId)` — removes tag and all `ExpenseTag` links
- [ ] Update `ExpenseService.AddAsync` / `UpdateAsync` to accept `tagIds[]`; create tags on-the-fly if name provided
- [ ] `TagController` — `GET /tags`, `POST /tags`, `DELETE /tags/{id}`
- [ ] Update expense filters to support tag filtering (AND/OR)
- [ ] Unit tests

### Frontend

- [ ] Tag autocomplete input component
- [ ] Wire into add/edit expense form
- [ ] Tag filter in expense list

**Depends on:** Phase 3

---

## Phase 6 — Currency Rates

**Goal:** Daily rates stored and resolved; display currency conversion on read; auto-update process.

### Backend

- [ ] `CurrencyRateService`
  - `ResolveRateAsync(sourceCurrencyId, destinationCurrencyId, date)` — exact → most recent before → default fallback
  - `GetRateHistoryAsync(sourceCurrencyId, destinationCurrencyId)` — admin use
  - `AddManualRateAsync(request, adminUserId)` — single date
  - `BulkAddManualRatesAsync(rows, adminUserId)` — CSV
  - `SetDefaultFallbackAsync(sourceCurrencyId, destinationCurrencyId, rate, adminUserId)`
  - `ResolveConflictAsync(conflictId, resolution, customRate, adminUserId)`
- [ ] Auto-update background service (`IHostedService` / `BackgroundService`)
  - Runs daily (configurable time)
  - Fetches rates from external provider (provider abstracted behind `IRateProvider`)
  - For each fetched rate: if a manual rate already exists for that date → insert into `CurrencyRateConflict` (status: `pending`); otherwise insert into `CurrencyDailyRate`
- [ ] Update `ExpenseService.GetPagedAsync` and `GetByIdAsync` to accept `displayCurrencyId` parameter; compute converted amount using `ResolveRateAsync` per expense date
- [ ] Admin controllers for rates, conflicts
- [ ] Unit tests for rate resolution logic (exact match, fallback chain, default)

### Frontend

- [ ] Display currency selector (session state, not persisted)
- [ ] Expense list: show original amount + currency + converted amount
- [ ] Add/edit form: live converted amount preview

**Depends on:** Phase 2, Phase 3

---

## Phase 7 — Dashboard API

**Goal:** Backend aggregation endpoints for all dashboard charts and summary cards.

### Backend — `DashboardController` / `DashboardService`

- [ ] `GET /dashboard/summary` — total, vs. previous period, top category, count; params: `familyId`, `dateFrom`, `dateTo`, `displayCurrencyId`
- [ ] `GET /dashboard/monthly` — total per month, last N months, broken down by category; params as above
- [ ] `GET /dashboard/categories` — category + subcategory breakdown for period
- [ ] `GET /dashboard/same-month-across-years` — given a month number, returns that month's total per year
- [ ] `GET /dashboard/by-currency` — per-currency totals + converted sum
- [ ] `GET /dashboard/recent` — last 10 expenses (same filters)
- [ ] All endpoints scope to: Default family (all user expenses) or specific family (member vs. head rules apply)
- [ ] Unit tests for aggregation logic

**Depends on:** Phase 4, Phase 5, Phase 6

---

## Phase 8 — Frontend: Expense List & Form

**Goal:** Full expense management UI on web.

### Tasks

- [ ] Install TanStack Query (`@tanstack/react-query`) — replaces manual `[loading, data]` state for expense data
- [ ] `ExpensesPage` — paginated table with filters sidebar
  - Date range, category/subcategory (multi-select + "Uncategorised"), tags, currency, amount range, member (head view)
  - Full-text search on description
  - Edit inline or modal
  - Delete (own only; head sees delete for others = "remove from family")
  - Bulk select → bulk delete / export CSV
- [ ] `AddExpensePage` / `EditExpensePage` (or modal)
  - All fields per spec
  - Category optional; subcategory cascades
  - Tag autocomplete
  - Family multi-select (Default always checked)
  - Live converted amount preview
  - Edit screen: shows modified by / at / from
- [ ] Zod schemas + React Hook Form for add/edit
- [ ] i18n coverage for all new strings
- [ ] Tests: form validation, list rendering, filter state

**Depends on:** Phase 4, Phase 5, Phase 6

---

## Phase 9 — Frontend: Dashboard

**Goal:** Replace "Coming soon" placeholder with real charts and stats.

### Tasks

- [ ] Install Recharts (`recharts`)
- [ ] Dashboard layout: family selector, display currency selector, date range picker
- [ ] Summary cards (total, delta vs. previous, top category, count)
- [ ] Monthly bar chart — stacked by category, 12 months
- [ ] Category/subcategory donut chart (uncategorised as own segment)
- [ ] Monthly average overlay line on bar chart
- [ ] "Same month across years" chart
- [ ] Per-currency breakdown panel
- [ ] Recent expenses panel (last 10, links to expenses screen)
- [ ] All charts respond to family scope and display currency changes
- [ ] i18n coverage
- [ ] Tests: chart rendering with mock data, currency conversion display

**Depends on:** Phase 7, Phase 8 (for shared components)

---

## Phase 10 — Frontend: Family Management

**Goal:** Full family management UI.

### Tasks

- [ ] `FamiliesPage` — tabbed: Active / Archived
- [ ] Create family form
- [ ] Per-family detail: member list, invite by email, change role, remove member
- [ ] Archive / unarchive family (head only; Default family: no archive option rendered)
- [ ] Rename family, family settings
- [ ] Leave family (member; head must transfer first)
- [ ] Accept invite flow (email link → web page)
- [ ] Notification when attribution removed (in-app)
- [ ] i18n coverage
- [ ] Tests

**Depends on:** Phase 4

---

## Phase 11 — Admin Screens

**Goal:** App admin can manage categories, currencies, rates, and users.

### Backend

- [ ] Role guard middleware — `[AppAdmin]` attribute; returns 403 for non-admins
- [ ] `AdminCategoryService` — add/edit/archive category, add/edit/archive subcategory
- [ ] `AdminCurrencyService` — add currency, set default fallback rate
- [ ] Rate endpoints already in Phase 6; wire admin auth guard

### Frontend

- [ ] Admin layout / navigation (separate from user app)
- [ ] **Users screen** — list, disable/enable, assign app role
- [ ] **Categories screen** — tree view, add/edit/archive category + subcategory
- [ ] **Currencies screen** — list, add currency, set fallback rate per pair
- [ ] **Conversion rates screen** — select pair, view history by date, add/edit rate, CSV upload
- [ ] **Rate validation screen** — list pending conflicts (date, pair, auto vs. manual, delta); resolve each (accept auto / keep manual / set custom); bulk resolve
- [ ] **Platform audit log** — rate changes, user management events (not expense data)
- [ ] i18n coverage for admin strings
- [ ] Tests

**Depends on:** Phase 6

---

## Phase 12 — CSV Import

**Goal:** Users can bulk-upload expenses from a CSV file.

### Backend

- [ ] `CsvImportService`
  - Parse uploaded CSV; validate each row (amount, date format, currency code, category name)
  - Return preview payload: parsed rows + per-row errors
  - On confirm: bulk insert; all rows logged as `created_from: bulk_web`
  - Return import summary (imported, skipped, errors)
- [ ] `POST /expenses/import/preview` — returns parsed + validated rows
- [ ] `POST /expenses/import/confirm` — executes import
- [ ] `GET /expenses/import/template` — returns CSV template file
- [ ] FluentValidation for import request DTO
- [ ] Unit tests for parser and validation logic

### Frontend

- [ ] `CsvImportPage`
  - Drag-and-drop file upload
  - Downloadable template link
  - Preview table with per-cell error highlighting
  - Column mapping step (if headers differ from template)
  - Confirm / cancel

**Depends on:** Phase 3, Phase 4, Phase 5

---

## Phase 13 — Notifications

**Goal:** Notify expense owner when a family head removes their expense from a family.

### Backend

- [ ] `NotificationService` — in-app notification record + email dispatch
- [ ] Triggered by `FamilyService` when attribution removed
- [ ] `GET /notifications` — unread in-app notifications for current user
- [ ] `POST /notifications/{id}/read` — mark read
- [ ] Email template: "Your expense was removed from [Family Name] by [Head Name]"
- [ ] Unit tests

### Frontend

- [ ] Notification bell in navbar — unread count badge
- [ ] Notification dropdown / panel — list, mark read
- [ ] Toast on real-time notification (polling or WebSocket — polling first)

**Depends on:** Phase 4

---

## Phase 14 — PWA / Mobile

**Goal:** Progressive web app with offline support and mobile-optimised quick-add.

### Tasks

- [ ] Add Vite PWA plugin (`vite-plugin-pwa`) — service worker, manifest, install prompt
- [ ] Mobile-optimised layout and navigation (bottom tab bar)
- [ ] Quick-add bottom sheet — amount, currency, category tiles, description, tags, date, families
- [ ] Expense list grouped by day, swipe-to-delete
- [ ] Mobile dashboard: month total, progress bar, category bar chart, last 5 expenses
- [ ] Offline queue: IndexedDB via `idb` library; sync on reconnect
- [ ] Receipt photo capture and attach (stored as base64 or object URL pending backend support)
- [ ] Push notification support (Web Push API)
- [ ] Home screen widget (limited — web apps can't do native widgets; document limitation)

**Depends on:** Phase 8, Phase 9

---

## Phase 15 — Suggested Additions (Backlog)

Not scheduled. Implement after core is stable.

| Item | Notes |
|------|-------|
| **Budgets** | Per-family/user monthly budget per category; dashboard bars; alerts |
| **Recurring expenses** | Template + schedule; auto-create or confirm prompt; "Upcoming" section |
| **Expense splitting** | Split across family members; settle-up view |
| **Custom family categories** | Family-level categories alongside global ones |
| **Reports & exports** | Scheduled monthly PDF email; year-end export |
| **Dark mode** | Theme toggle in profile settings |
| **Audit log UI** | Expense owner and family head can view audit history per expense |
| **Soft delete option** | If hard delete proves insufficient for reporting; adds `deleted_at` flag |

---

## Cross-Cutting Concerns (apply throughout)

| Concern | Approach |
|---------|----------|
| **Authorization** | nginx subrequest auth already in place; add family-membership checks in service layer |
| **Pagination** | All list endpoints: cursor or offset-based from day one |
| **Error codes** | All backend errors as uppercase string codes (e.g. `EXPENSE_NOT_FOUND`), consistent with existing pattern |
| **i18n** | Every new frontend string added to all 4 locale files (en, fr, es, de) |
| **Tests** | Backend: unit tests for services, integration tests for controllers using in-memory DB; Frontend: component tests with `@testing-library/react` |
| **Migrations** | Auto-run at startup via `MigrateAsync()` — existing pattern |
| **CHANGELOG** | Update on every phase completion |
| **FILE-TREE.md** | Update when files/folders added |
