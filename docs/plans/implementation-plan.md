# ExpenseManager — Implementation Plan

Reference: [application-description.md](application-description.md)

## Current State

| Area | Status |
|------|--------|
| Users service | ✅ Complete — auth, registration, JWT (incl. `isAdmin` claim), refresh tokens, password management, FluentValidation, admin user management (`AdminUserController`) |
| Expenses service | ✅ Phase 1–13 complete — schema, categories/currencies, expense CRUD, family system, tags, currency rates (daily storage, resolution, auto-update via Quartz, backfill, conflict management, display currency conversion), dashboard API, admin controllers (categories, currencies, rates), CSV bulk import, outbox for family events |
| Notifications service | ✅ Phase 13 complete — dedicated microservice (port 9300); SignalR hub; RabbitMQ consumer (`family.member.removed`); inbox deduplication; in-app notifications + email; `NotificationBell` in frontend NavBar; 20 tests |
| Frontend | ✅ Auth + family management + tag input + display currency selector + expense list/form (Phase 8) + dashboard (Phase 9 — Hearth design) + admin screens (Phase 11: users, categories, currencies, rates, rate conflicts, AdminRoute guard, AdminLayout, i18n) + CSV import (Phase 12) + notification bell with SignalR real-time push (Phase 13) + Ionic + Capacitor native mobile app at `frontend/mobile/` (Phase 14) complete |
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
| 9 | Frontend — Dashboard ✅ | Charts and summary (Hearth design, v0.106.0) |
| 10 | Frontend — Family Management | Web UI for families |
| 11 | Admin Screens ✅ | Categories, currencies, rates, rate validation, users (v0.108.0) |
| 12 | CSV Import ✅ | Bulk expense upload |
| 13 | Notifications ✅ | In-app + email on attribution removal (dedicated microservice, SignalR, outbox/inbox) |
| 14 | Mobile App ✅ | Ionic + Capacitor + React native app at `frontend/mobile/`; offline queue, push token stub |
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

**`Tag`** *(global — no per-user ownership; unique by name, case-sensitive)*

| Column | Type | Notes |
|--------|------|-------|
| `id` | PK | |
| `name` | varchar(50) | UNIQUE index |

**`UserTag`** *(junction — tracks which users have adopted each tag)*

| Column | Type | Notes |
|--------|------|-------|
| `user_id` | FK → ext.USR_Users | PK composite; Cascade on delete |
| `tag_id` | FK → Tag | PK composite; Restrict on delete |

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

- [x] Update `Category` model — add `IsArchived`
- [x] Rewrite `Expense` model with new columns
- [x] Add `Family`, `FamilyMembership`, `ExpenseFamilyAttribution` models
- [x] Add `Tag`, `UserTag`, `ExpenseTag` models — `Tag` is global (no `UserId`); `UserTag` junction tracks adoption (Phase 5 redesign; migration `AddUserTagsRefactorTags`)
- [x] Add `CurrencyDailyRate`, `CurrencyPairDefault`, `CurrencyRateConflict` models
- [x] Add `ExpenseAuditLog`, `ExpenseAuditSnapshot` models
- [x] Update `ExpensesDbContext` — register all new models, configure constraints/indexes
- [x] Generate and apply EF Core migration (`SchemaFoundation` — 2026-05-05)
- [x] Replace all 8 C# enums with DB lookup tables (`Models/Lookups/`); FK int columns in all domain models; `ILookupCacheService` / `LookupCacheService` with `IMemoryCache` (NeverRemove); migration regenerated with full seed data (2026-05-05)

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

- [x] `CategoryService` — `GetAllAsync()` (tree: categories with subcategory children, excluding archived)
- [x] `CurrencyService` — `GetAllAsync()`
- [x] `CategoryController` — `GET /categories`
- [x] `CurrencyController` — `GET /currencies`
- [x] FluentValidation not needed (read-only, no request bodies)
- [x] Unit tests for services

### Frontend

- [x] `categoriesApi.service.ts` — fetch categories tree
- [x] `currenciesApi.service.ts` — fetch currencies list
- [x] Store both in React context or TanStack Query cache (available globally)

**Depends on:** Phase 1

---

## Phase 3 — Core Expense CRUD

**Goal:** Users can add, edit, delete their own expenses. Audit trail written on every operation.

### Backend

- [x] `ExpenseService`
  - `AddAsync(request, userId, source)` → writes expense + audit log (operation: `add`, 1 `after` snapshot)
  - `UpdateAsync(id, request, userId, source)` → writes audit log (operation: `update`, `before` + `after` snapshots)
  - `DeleteAsync(id, userId)` → soft delete (`IsDeleted = true`, `DeletedAt = UtcNow`) + audit log (operation: `delete`, 1 `before` snapshot); all queries filter `!IsDeleted`
  - `GetByIdAsync(id, userId)` — enforces ownership
  - `GetPagedAsync(filters, userId)` — paginated, filtered list (own expenses only for now)
- [x] `ExpenseController`
  - `POST /expenses`
  - `PUT /expenses/{id}`
  - `DELETE /expenses/{id}`
  - `GET /expenses/{id}`
  - `GET /expenses` (paged + filtered)
- [x] `ExpenseAuditService` — internal, called by `ExpenseService`; never exposed directly
- [x] FluentValidation for create/update request DTOs
- [x] Unit tests for service logic and audit writes

### Notes
- No family attribution yet (Phase 4); all expenses implicitly owned by user
- Category/subcategory optional; validate subcategory belongs to selected category if both provided
- No tags yet (Phase 5)

**Depends on:** Phase 1, Phase 2

---

## Phase 4 — Family System

**Goal:** Default family created when email validation succeeds (`user.created` event). Users can create and manage families. Expenses attributed to one or more families.

### Backend (expenses service)

- [x] `FamilyService`
  - `CreateDefaultAsync(userId)` — called by RabbitMQ consumer on `user.created`
  - `CreateAsync(name, userId)`
  - `GetByUserAsync(userId)` — returns active + archived, with role per family
  - `InviteAsync(familyId, inviteeEmail, invitedBy)` — creates `FamilyInvitation` with GUID token (7-day expiry)
  - `AcceptInviteAsync(token, userId)` — validates token, creates `FamilyMembership` as Member
  - `RemoveMemberAsync(familyId, targetUserId, removedBy)` — head only
  - `ChangeRoleAsync(familyId, targetUserId, newRole, changedBy)` — head only
  - `RenameAsync(familyId, name, userId)` — head only
  - `ArchiveAsync(familyId, userId)` — head only, non-default (soft delete)
  - `UnarchiveAsync(familyId, userId)` — head only
  - `LeaveAsync(familyId, userId)` — any member; head blocked if last head (`FAMILY_CANNOT_LEAVE_LAST_HEAD`)
- [x] `FamilyInvitation` model + `Phase4_FamilyInvitation` EF migration
- [x] `UserEventConsumer` — on `user.created` → `SaveOrUpdateUserAsync` then `CreateDefaultAsync` (idempotent)
- [x] Expense attribution: `FamilyIds int[]?` on Create/Update requests; null = auto-attribute to default family; empty = no attribution; provided = validate membership + write `ExpenseFamilyAttribution` rows
- [x] Head "remove from family" → deletes `ExpenseFamilyAttribution` rows via `RemoveMemberAttributionsAsync`
- [x] `FamilyController` — `GET /families`, `GET /families/{id}`, `POST /families`, `PUT /families/{id}/name`, `POST /families/{id}/archive`, `POST /families/{id}/unarchive`, `POST /families/{id}/invite`, `POST /families/accept-invite/{token}`, `DELETE /families/{id}/members/{userId}`, `PUT /families/{id}/members/{userId}/role`, `DELETE /families/{id}/leave`
- [x] FluentValidation for all request DTOs
- [x] Unit tests — FamilyService (34 cases), UserEventConsumer updated

### Backend (users service)

- No changes needed — `user.created` already published on `GET /auth/validate-email` success

### Frontend

- [x] `familyApi.service.ts`
- [x] Family selector component (sidebar / top bar) — switch active family scope
- [x] Family management screen (list, create, invite, archive, member management)
- [x] Update expense add/edit form — family multi-select (Default always checked) — implemented in Phase 8 via `ExpenseForm`

**Depends on:** Phase 3

---

## Phase 5 — Tags ✅ Complete (v0.98.0)

**Goal:** Users can create and assign tags to expenses. Tags differentiated between own (adopted by user) and family (adopted by co-members).

**Design note:** Tags are global (unique by name, case-sensitive). `UserTag` junction tracks adoption — a row is created when user calls `POST /tags` or auto-adopts by attaching a tag to an expense. Once adopted, tag remains visible even if the adopting co-member leaves the family.

### Backend

- [x] `TagService`
  - `GetVisibleAsync(userId)` → `TagListDto { own, family }` — own + co-member tags, fetched in parallel
  - `UseTagAsync(name, userId)` — idempotent: find-or-create `Tag` by name + `EnsureUserTagAsync`
  - `RemoveTagAsync(tagId, userId)` — removes `UserTag` only; `Tag` entity and `ExpenseTag` rows persist for history
- [x] `TagRepository` — `GetOwnAsync`, `GetFamilyAsync` (co-member tags excluding own and deleted-family members), `GetByNameAsync`, `GetByIdsAsync`, `AddAsync`, `EnsureUserTagAsync`, `RemoveUserTagAsync`, `IsVisibleAsync`
- [x] Migration `AddUserTagsRefactorTags` — drops `Tags.UserId` FK+column; adds unique index on `Tags.Name`; creates `UserTags` table
- [x] Update `ExpenseService.AddAsync` / `UpdateAsync` — validate `tagIds` visibility (→ 403 if invisible), auto-adopt via `EnsureUserTagAsync`, insert `ExpenseTag` rows, capture tag IDs in audit snapshot
- [x] `TagController` — `GET /tags` → `TagListDto`; `POST /tags` → `TagDto` (idempotent, case-sensitive); `DELETE /tags/{id}` → 204 or 404
- [x] Update `ExpenseFilterDto` with `int[]? TagIds` — OR-semantics filter in `GetPagedAsync`
- [x] `ExpenseDto` includes `IEnumerable<TagDto> Tags`
- [x] FluentValidation for `CreateTagRequest` (name not-empty, max 50 chars)
- [x] Unit tests: `TagServiceTests` (10 tests, Moq), `TagRepositoryTests` (16 integration tests)

### Frontend

- [x] `tag.type.ts` — `Tag { id, name }`, `TagList { own, family }`
- [x] `tagsApi.service.ts` — `getTags()`, `useTag(name)`, `removeTag(id)`
- [x] `TagInput.tsx` — grouped combobox: "My tags" / "Family tags" sections, chip display, "Create" option when no exact match, keyboard (Enter / Escape / Backspace); 13 component tests
- [x] Wire `TagInput` into add/edit expense form — implemented in Phase 8 via `ExpenseForm`
- [ ] Tag filter in expense list — deferred to Phase 9+

**Depends on:** Phase 3

---

## Phase 6 — Currency Rates

**Goal:** Daily rates stored and resolved; display currency conversion on read; auto-update process.

### Backend

- [x] `CurrencyRateService`
  - `ResolveRateAsync(sourceCurrencyId, destinationCurrencyId, date)` — exact → most recent before → default fallback
  - `GetRateHistoryAsync(sourceCurrencyId, destinationCurrencyId)` — admin use
  - `AddManualRateAsync(request, adminUserId)` — single date
  - `BulkAddManualRatesAsync(rows, adminUserId)` — CSV
  - `SetDefaultFallbackAsync(sourceCurrencyId, destinationCurrencyId, rate, adminUserId)`
  - `ResolveConflictAsync(conflictId, resolution, customRate, adminUserId)`
  - `GetPendingConflictsAsync()` — list conflicts awaiting resolution
  - `RunDailyUpdateAsync(ct)` — fetches today's rates, creates conflicts for existing manual rates
  - `RefreshRatesFromAsync(from, sourceCurrencyId?, destinationCurrencyId?, ct)` — date-range backfill
- [x] Auto-update background service — `RateAutoUpdateJob` (`IJob` with `[DisallowConcurrentExecution]`, Quartz cron `0 {m} {h} * * ?` from `CurrencyRateOptions.UpdateTime`)
- [x] Update `ExpenseService.GetPagedAsync` and `GetByIdAsync` to accept `displayCurrencyId` parameter; compute `ConvertedAmount` + `DisplayCurrency` via `ResolveRateAsync` per expense date
- [x] `CurrencyRateController` (`/rates`) — initial implementation; **moved to `AdminRateController` (`/admin/rates`) in Phase 11** with `[AppAdmin]` guard; `CurrencyRateController` removed
- [x] Unit tests — `CurrencyRateServiceTests` (27 cases), `RateAutoUpdateJobTests`

### Frontend

- [x] `DisplayCurrencySelector` — session state (not persisted), search by code/name, NavBar integration
- [x] Expense list: show original + converted amount — implemented in Phase 8 (`ExpensesPage` shows `convertedAmount`/`displayCurrency` when present)
- [ ] Add/edit form: live converted amount preview — deferred to Phase 9+

**Depends on:** Phase 2, Phase 3

---

## Phase 7 — Dashboard API

**Goal:** Backend aggregation endpoints for all dashboard charts and summary cards.

### Backend — `DashboardController` / `DashboardService`

- [x] `GET /dashboard/summary` — total, vs. previous period, top category, count; params: `familyId`, `dateFrom`, `dateTo`, `displayCurrencyId`
- [x] `GET /dashboard/monthly` — total per month, last N months, broken down by category; params as above
- [x] `GET /dashboard/categories` — category + subcategory breakdown for period
- [x] `GET /dashboard/same-month-across-years` — given a month number, returns that month's total per year
- [x] `GET /dashboard/by-currency` — per-currency totals + converted sum
- [x] `GET /dashboard/recent` — last 10 expenses (same filters)
- [x] All endpoints scope to: Default family (all user expenses) or specific family (member vs. head rules apply)
- [x] Unit tests for aggregation logic (47 tests: 13 repository, 20 service, 14 controller)

**Depends on:** Phase 4, Phase 5, Phase 6

---

## Phase 8 — Frontend: Expense List & Form

**Goal:** Full expense management UI on web.

### Tasks

- [x] Install TanStack Query (`@tanstack/react-query`) v5 — `useQuery` for expense list + edit prefetch
- [x] `ExpensesPage` — paginated table with filters sidebar
  - [x] Date range, category/subcategory, currency, amount range filters (`ExpenseFilters`)
  - [x] Description search
  - [x] Delete own expense (confirm modal)
  - [x] Shows original + converted amount when display currency differs
  - [ ] Tag filter in `ExpenseFilters` — deferred to Phase 9+
  - [ ] Bulk select → bulk delete / export CSV — deferred to Phase 11+
  - [ ] Head "remove from family" action — deferred to Phase 9+
- [x] `AddExpensePage` / `EditExpensePage` (separate pages, not modal)
  - [x] All fields: amount, currency, date, category, subcategory, description
  - [x] Category optional; subcategory cascades (disabled when no category selected)
  - [x] Tag autocomplete via `TagInput`
  - [x] Family multi-select (Default always included)
  - [ ] Live converted amount preview — deferred to Phase 9+
  - [ ] Edit screen: shows modified by / at / from — deferred to Phase 9+
- [x] `ExpenseForm` — shared form component with Zod v4 schemas + React Hook Form
- [x] i18n coverage for all new strings (en/fr/es/de)
- [x] Tests: 56 new tests (13 `ExpensesPage`, 15 `AddExpensePage`, 14 `EditExpensePage`, 14 `ExpenseForm`); 644 total passing

**Depends on:** Phase 4, Phase 5, Phase 6

---

## Phase 9 — Frontend: Dashboard ✅ Complete (v0.106.0)

**Goal:** Replace "Coming soon" placeholder with real charts and stats.

### Tasks

- [x] Install Recharts (`recharts`)
- [x] Dashboard layout: `DashboardFilters` — family selector, display currency selector, date range picker with "This month" / "This year" preset buttons (`aria-pressed`)
- [x] `MonthHero` — total amount, ±% delta chip (green/red), expense count, top category pill; skeleton on loading
- [x] `SpendChart` — Recharts `ComposedChart`: stacked bar by category + monthly average line overlay, 12-month window
- [x] `CategoryDonut` — Recharts `PieChart` (donut) with right-side legend (name, amount, %)
- [x] `SameMonthChart` — Recharts `BarChart`: year-over-year comparison for selected month
- [x] `CurrenciesPanel` — per-currency rows: symbol, code, total, converted amount when present, count
- [x] `RecentExpenses` — last 10 expenses feed; "View all" → `/expenses`; skeleton + empty states
- [x] All components respond to `DashboardFilter` state (family, currency, date range) via `useQuery` refetch
- [x] i18n coverage — `dashboard.*` keys in all 4 locale files (en/fr/es/de)
- [x] `dashboard.type.ts` — `DashboardSummaryDto`, `MonthlyBreakdownDto`, `CategoryBreakdownDto`, `SameMonthYearlyDto`, `CurrencyBreakdownDto`, `DashboardFilter`
- [x] `dashboardApi.service.ts` — 6 API functions wiring all Phase 7 endpoints
- [x] Tests: 9 test files — `dashboardApi.service.test.ts`, 7 component tests, updated `HomeDashboardPage.test.tsx` (full mock suite + `QueryClientProvider`)
- [ ] Tag filter in `ExpenseFilters` — deferred to Phase 10+
- [ ] Live converted amount preview in add/edit form — deferred to Phase 10+
- [ ] Edit screen: shows modified by / at / from — deferred to Phase 10+
- [ ] Head "remove from family" action in expense list — deferred to Phase 10+

**Depends on:** Phase 7, Phase 8 (for shared components)

---

## Phase 10 — Frontend: Family Management

**Goal:** Full family management UI.

### Tasks

- [x] `FamiliesPage` — tabbed: Active / Archived
- [x] Create family form
- [x] Per-family detail: member list, invite by email, change role, remove member
- [x] Archive / unarchive family (head only; Default family: no archive option rendered)
- [x] Rename family, family settings
- [x] Leave family — `DELETE /families/{id}/leave`; heads blocked if last head (`FAMILY_CANNOT_LEAVE_LAST_HEAD`); button in expanded detail panel; 6 service tests
- [x] Accept invite flow (email link → web page) — `AcceptInvitePage` at `/families/accept-invite?token=`
- [ ] Notification when attribution removed (in-app)
- [x] i18n coverage — all 4 locales (en/fr/es/de) complete for families + currencies sections
- [ ] Tests — `FamiliesPage.test.tsx` exists (layout, cards, detail panel, create/rename modals) but missing coverage for leave button and `DisplayCurrencySelector` search

**Depends on:** Phase 4

---

## Phase 11 — Admin Screens ✅ Complete (v0.108.0)

**Goal:** App admin can manage categories, currencies, rates, and users.

### Backend — Admin Role System

- [x] `isAdmin` JWT claim in `JwtTokenService` — true when user has `APP_ADMIN` role (`Code == "APP_ADMIN"`)
- [x] `GET /auth/session` response extended with `IsAdmin: bool`
- [x] `AdminAuthorizeAttribute` (users service) — reads `isAdmin` claim from cookie; 403 if absent/false
- [x] `AppAdminAttribute` (expenses service) — reads `isAdmin` via `JwtCookieReader.GetIsAdmin`; 403 if false
- [x] `JwtCookieReader` extended with `GetIsAdmin()` + `Authorization: Bearer` header fallback for Swagger
- [x] Swagger Bearer auth in both `Program.cs` files

### Backend — Admin Category Management (Expenses)

- [x] `IAdminCategoryService` / `AdminCategoryService` — add/update/archive/unarchive categories and subcategories; validates parent not archived before adding sub; blocks archiving category with active children
- [x] `AdminCategoryController` at `[Route("admin/categories")]`; all actions `[AppAdmin]`; 8 endpoints
- [x] `AdminCategoryRequestValidator` — name required + max 100 chars

### Backend — Admin Currency Management (Expenses)

- [x] `IAdminCurrencyService` / `AdminCurrencyService` — `AddCurrencyAsync`; set default delegates to `CurrencyRateService`
- [x] `AdminCurrencyController` at `[Route("admin/currencies")]`; 2 endpoints
- [x] `AdminAddCurrencyRequestValidator`

### Backend — Admin Rate Management (Expenses)

- [x] `AdminRateController` at `[Route("admin/rates")]`; replaces `CurrencyRateController`; all 7 endpoints; all `[AppAdmin]`
- [x] `CurrencyRateController` removed

### Backend — Admin User Management (Users)

- [x] `IAdminUserService` / `AdminUserService` — paged list with search, disable/enable, set roles
- [x] `AdminUserController` at `[Route("admin/users")]`; 4 endpoints; all `[AdminAuthorize]`

### Frontend

- [x] `AdminRoute.tsx` — guard component; redirects to `/dashboard` when `isAdmin=false`
- [x] `AdminLayout.tsx` — sidebar nav (Users/Categories/Currencies/Rates/Rate Conflicts) + breadcrumb
- [x] `AdminUsersPage.tsx` — searchable paginated table; Disable/Enable; Manage Roles modal
- [x] `AdminCategoriesPage.tsx` — tree view; Add/Edit/Archive modals; subcategory management; "Show archived" toggle
- [x] `AdminCurrenciesPage.tsx` — currency list; Add Currency modal; Set Default Rate modal
- [x] `AdminRatesPage.tsx` — pair selector; rate history table; Add Manual Rate + Backfill modals
- [x] `AdminRateConflictsPage.tsx` — pending conflicts; per-row resolve (Accept Auto / Keep Manual / Custom); bulk resolve
- [x] Admin service layer — `adminUsersApi.service.ts`, `adminCategoriesApi.service.ts`, `adminCurrenciesApi.service.ts`, `adminRatesApi.service.ts`
- [x] Router — admin routes under `<AdminRoute>` at `/admin/*`
- [x] NavBar — "Admin" link visible only when `isAdmin === true`
- [x] `User` type extended with `isAdmin?: boolean`; `AuthContext` propagates from session response
- [x] i18n — `"admin"` key in all 4 locale files (en/fr/es/de)
- [x] Tests — 6 test files: `AdminRoute`, `AdminUsersPage`, `AdminCategoriesPage`, `AdminCurrenciesPage`, `AdminRatesPage`, `AdminRateConflictsPage`; 798 frontend tests total

### Deferred

- [ ] **Platform audit log** — rate changes and user management events; deferred to Phase 15

**Depends on:** Phase 6

---

## Phase 12 — CSV Import

**Goal:** Users can bulk-upload expenses from a CSV file.

### Backend

- [x] `CsvImportService` (`ICsvImportService`)
  - `ParseAndValidateAsync(stream, userId)` — CsvHelper CSV parsing; validates date, amount, currency code (by code lookup), category/subcategory (by name lookup), family membership; tags parsed but not validated (auto-created on confirm); returns per-row preview with error codes
  - `ConfirmImportAsync(rows, userId)` — `ITagService.UseTagAsync` per tag name, then `IExpenseService.AddAsync` with sourceId=3 (BulkWeb); per-row exception → skipped (no batch abort)
- [x] `POST /import/preview` — multipart IFormFile → `CsvImportPreviewDto`; max 1 MB
- [x] `POST /import/confirm` — `CsvImportConfirmRequest` (rows with resolved IDs + tag names) → `CsvImportResultDto`
- [x] `GET /import/template` — returns `expenses-import-template.csv` with 2 example rows
- [x] `CsvImportConfirmRequestValidator` — rows not empty, max 500 rows, per-row amount/currency/date
- [x] 17 unit tests: `CsvImportServiceTests` (13) + `ExpenseImportControllerTests` (6 + 3 template tests)

### Frontend

- [x] `CsvImportPage` at `/expenses/import`
  - Drag-and-drop file upload dropzone (click or drop)
  - Downloadable template link (`GET /import/template`)
  - Preview table with per-row error highlighting (red background on invalid rows)
  - Summary badges: "N valid rows" / "N rows with errors"
  - Confirm button (disabled when validCount=0); navigates to /expenses on success
  - Cancel returns to upload step
- [x] `api.service.ts`: `postFormData<T>()` helper; fixed Content-Type guard for FormData
- [x] `expensesApi.service.ts`: `previewCsvImport`, `confirmCsvImport`, `getImportTemplateUrl`
- [x] `ExpensesPage.tsx`: "Import CSV" secondary button → /expenses/import
- [x] `router.tsx`: `/expenses/import` route added
- [x] i18n: `expenses.importCsv` + full `expenses.import.*` namespace in en/fr/es/de
- [x] 10 frontend tests (`CsvImportPage.test.tsx`)

**Depends on:** Phase 3, Phase 4, Phase 5

---

## Phase 13 — Notifications ✅ Complete

**Goal:** Notify expense owner when a family head removes their expense from a family.

### Backend — Expenses service changes

- [x] `FamilyEventMessage` + `IFamilyEventPublisher` / `FamilyEventPublisher` — publishes to `expenses.events` topic exchange
- [x] `OutboxEvent` model + `IExpensesOutboxRepository` + `ExpensesOutboxRepository` — durable outbox
- [x] `FamilyOutboxPublisherService` BackgroundService — polls every 5 s, max 5 retries
- [x] `FamilyService.RemoveMemberAsync` — writes to outbox after member + attribution removal (non-fatal)
- [x] Migration `AddExpensesOutbox`

### Backend — Notifications microservice (`backend/notifications/`, port 9300)

- [x] `Notification` model + `InboxEvent` model + `NotificationsDbContext`
- [x] `INotificationRepository` / `NotificationRepository` (create, list, unread count, mark read/all)
- [x] `IInboxRepository` / `InboxRepository` (deduplication)
- [x] `FamilyEventConsumer` BackgroundService — consumes `expenses.events`, routing `family.#`, inbox dedup, nacks on failure
- [x] `NotificationService` — persist → SignalR push → email (hub/email non-fatal)
- [x] `NotificationHub` (SignalR at `/ws/notifications`) — cookie auth, groups by userId
- [x] `NotificationController` — `GET /notifications`, `GET /notifications/unread-count`, `POST /notifications/{id}/read`, `POST /notifications/read-all`
- [x] Email template `FAMILY_MEMBER_REMOVED_TEMPLATE.html`
- [x] `Touir.ExpensesManager.Notifications.Tests` — 20 tests: 6 repository, 8 service, 4 consumer
- [x] GitLab CI, SonarQube, Trivy, Dockerfile, .dockerignore, .gitignore, README

### Infrastructure

- [x] `docker-compose-apps.yml` — `notifications-service` at port 9300
- [x] Nginx — `/api/notifications/ws/` (WebSocket, 3600s timeout) + `/api/notifications` REST blocks
- [x] `.env` + `.env.example` — `EXPENSES_MANAGEMENT_NOTIFICATIONS_DATABASE_*` + RabbitMQ + Email vars
- [x] Root `.gitlab-ci.yml` — `backend-notifications-pipeline` trigger

### Frontend

- [x] `@microsoft/signalr` installed
- [x] `NotificationContext` — SignalR connection, notification list + unread count state
- [x] `NotificationBell` component — bell icon, red unread badge, dropdown with mark-all, toast on new notification
- [x] NavBar — placeholder button replaced with `<NotificationBell />`
- [x] `AppProviders` — `NotificationProvider` added
- [x] i18n `notifications.*` keys in all 4 locales (en/fr/es/de)

**Depends on:** Phase 4

---

## Phase 14 — Mobile App ✅ Complete (v0.113.0)

**Goal:** Standalone Ionic + Capacitor + React native mobile app at `frontend/mobile/`. Shares backend and locale files with the web dashboard.

### Delivered

- [x] **`frontend/mobile/`** — separate Vite build; `@ionic/react` v8, `@capacitor/core` v7, `idb` v8
- [x] **5-tab navigation** — Dashboard, Expenses, + FAB (QuickAddModal), Families, Settings via `IonTabs`
- [x] **Hearth design tokens** — mapped to Ionic CSS custom properties in `theme/variables.css`
- [x] **QuickAddModal** — `IonModal` bottom sheet (0.75 breakpoint); offline enqueue; `@capacitor/haptics`; `@capacitor/camera` receipt preview
- [x] **ExpensesListPage** — `IonItemSliding` swipe-delete; `IonRefresher`; `IonInfiniteScroll`; family `IonSegment` filter
- [x] **DashboardPage** — month hero `IonCard`; `IonProgressBar` category breakdown; last-5 feed; display currency `IonSelect`
- [x] **FamiliesPage** — expandable cards; invite; leave; archive
- [x] **SettingsPage** — currency + language selectors; language persisted to `@capacitor/preferences`
- [x] **NotificationBell** — `IonButton` + `IonBadge`; `IonPopover` list; mark-all-read
- [x] **`useOfflineQueue`** — IndexedDB (`expense-manager` DB, `offline-expense-queue` store)
- [x] **`useNetworkSync`** — Capacitor Network listener; drain on reconnect; browser fallback
- [x] **NotificationContext** — SignalR + `@capacitor/push-notifications`; push token via `POST /api/notifications/push-token` stub
- [x] **Backend stub** — `POST /notifications/push-token` in `NotificationController` (Phase 15 will add FCM/APNs)
- [x] **8 unit test files** — `useOfflineQueue`, `useNetworkSync`, `LoginPage`, `ExpensesListPage`, `QuickAddModal`, `DashboardPage`, `NotificationContext`, `SettingsPage`
- [x] **`frontend/mobile/README.md`** — setup, env vars, device/emulator instructions, offline queue docs

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
