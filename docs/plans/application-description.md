# ExpenseManager — Application Description

## Vision

A multi-user, multi-family expense tracking application that lets individuals and household groups record, categorise, and analyse their spending across currencies. Designed for daily use via web and mobile, with admin-managed currency infrastructure and family-scoped sharing.

---

## Core Concepts

### Users & Roles

| Role | Scope | Capabilities |
|------|-------|--------------|
| **App Admin** | Global | Manage currencies, conversion rates, application-wide settings, all users — no access to expense data |
| **Family Head** | Family | Invite/remove members, manage family settings, view all family expenses, delete any member's expense, archive the family |
| **Family Member** | Family | Add/view own expenses, view family summaries (configurable per family), delete own expenses only |

A user can hold different roles in different families (e.g., Head in their own household, Member in a parents' family).

### Families

- A user can belong to **multiple families** (e.g., "My Home", "Parents", "Shared Flat").
- Each family has **one or more heads** (co-equal: partners, co-guardians, etc.).
- Family membership is invitation-based.
- Expenses are **owned by the individual** and can be **attributed to one or more families** simultaneously.
- A user's **active family scope** can be switched from a profile or sidebar selector — the dashboard updates to reflect the selected scope.

#### Default Family

- On registration, a **Default family** is automatically created containing only the user.
- The Default family name can be renamed and its settings updated, but it **cannot be archived**.
- The Default family always shows **all expenses created by the user**, regardless of which other families those expenses are also attributed to.
- Serves as the user's personal ledger and catch-all view.

#### Archived Families

- A family head can **archive** a family (non-default families only).
- Archived families become **read-only**: no new expenses can be added or attributed, no members can be invited, no edits to existing expenses within that family scope.
- Archived families remain visible in history/reporting but are clearly marked and excluded from active family selectors.

---

## Expenses

### Adding an Expense

Three entry points:

1. **Manual entry** (web + mobile) — form with all fields.
2. **CSV import** (web) — bulk upload; format documented, with a downloadable template. Import preview screen shows parsed rows before confirmation; rows with errors are flagged inline.
3. **Mobile app** — quick-entry optimised (see Mobile section).

### Expense Fields

| Field | Notes |
|-------|-------|
| **Amount** | Original amount in selected currency |
| **Currency** | Any supported currency; defaults to user's default currency |
| **Date** | Defaults to today; editable |
| **Category** | Top-level (e.g., Food, Transport, Health); **optional** — can be left empty and filled in later |
| **Subcategory** | Second level under category; only available when a category is selected |
| **Description** | Free text |
| **Tags** | Multiple; user-defined; searchable; reusable across expenses |
| **Families** | One or more families this expense is attributed to; Default family always included |
| **Attachment** | Optional receipt photo or PDF (future) |

### Categories & Subcategories

- Two-level hierarchy (category → subcategory), admin-managed globally.
- Category is **optional** on an expense — a user may leave it blank and categorise later (e.g., for quick mobile entry or uncertain purchases).
- Subcategory requires a parent category; clearing the category clears the subcategory.
- Suggested addition: family-level custom categories alongside global ones (see Open Design Decisions).

### Tags

- Flat list, user-scoped.
- Free-form creation on entry; auto-complete suggests existing tags.
- Tags enable cross-category grouping (e.g., tag `vacation-2025` spans Food, Transport, Accommodation).

### Delete Permissions

| Actor | Action | Effect |
|-------|--------|--------|
| Expense owner | Delete own expense | Expense removed from **all** families and from Default family — full hard delete |
| Family head | "Delete" a member's expense within their family | Removes only the **family attribution link** for that family; expense survives in the member's Default family and any other attributed families |
| App admin | No delete or edit rights on expenses | App admin is a platform administrator, not a data manager |

**When a family head removes a family attribution:**
- The expense owner is **notified** (in-app + email): which expense, which family, who performed the removal, and when.
- The owner retains full access to the expense via their Default family.

**When an owner deletes their own expense:**
- Hard delete — removed from all families simultaneously.
- Audit record retained (see Audit & Logging).
- No notification needed (owner initiated).

---

## Currency System

### User Currency

- Each user has a **default currency** stored in their profile.
- All amounts are stored as **original currency + original amount**.
- Converted amounts are computed on read using stored rates — never stored themselves.

### Conversion Rates

Rates are stored as **source currency → destination currency pairs**, daily.

**Resolution order for a given date:**
1. Exact rate for that date.
2. Most recent rate before that date.
3. Default fallback rate defined per currency pair (admin-set).

**Rate sources:**

| Source | Description |
|--------|-------------|
| **Automatic daily update** | A scheduled process fetches rates each day and stores them. |
| **Admin manual entry** | Admin can set or override a rate for a specific date (single or via CSV). |
| **Conflict — validation screen** | If the automatic process fetches a rate for a date that already has a manually entered rate, it does **not** silently overwrite — the conflict is queued in a **Rate Validation Screen** for admin review. Admin can accept the automatic rate, keep the manual rate, or set a custom value. |

### Rate Validation Screen (Admin)

- Lists all pending conflicts: date, currency pair, automatic value, manual value, delta.
- Admin can resolve each conflict: **Accept automatic**, **Keep manual**, or **Set custom**.
- Bulk resolve option (accept all automatic / keep all manual).
- Resolved conflicts are logged with timestamp and admin identity.

### Display Currency

- Dashboard has a **display currency selector** (session-scoped, not persisted to profile).
- Changing display currency reconverts all amounts on screen using stored rates for each expense's date.
- Expense list shows: original amount + currency, and converted amount in display currency side by side.

---

## Audit & Logging

Expense changes are recorded across two dedicated tables. No JSON columns.

### `ExpenseAuditLog` — one row per operation

| Column | Type | Notes |
|--------|------|-------|
| `id` | PK | |
| `expense_id` | FK | References the original expense |
| `operation` | enum | `add`, `update`, `delete` |
| `performed_at` | timestamp | |
| `performed_by` | FK (User) | |
| `performed_from` | enum | `single:web`, `single:mobile`, `bulk:web` |

### `ExpenseAuditSnapshot` — one or two rows per log entry

| Column | Type | Notes |
|--------|------|-------|
| `id` | PK | |
| `audit_log_id` | FK → `ExpenseAuditLog` | |
| `snapshot_type` | enum | `before`, `after` |
| `amount` | decimal | |
| `currency_id` | FK | |
| `date` | date | |
| `category_id` | FK? | nullable (uncategorised) |
| `subcategory_id` | FK? | nullable |
| `description` | text | |
| `tags` | text | comma-separated tag IDs (many-to-many, no JSON) |
| `families` | text | comma-separated family IDs |

**Snapshot rows per operation:**

| Operation | Snapshots inserted |
|-----------|--------------------|
| `add` | 1 — `after` only |
| `update` | 2 — `before` + `after` |
| `delete` | 1 — `before` only |

Both tables are **immutable** — no updates or deletes.

Expense owners can view their own expense audit log. Family heads can view audit entries for any expense attributed to their family. App admins have no access to expense audit data.

---

## Screens

### Web Application

#### Auth Screens (existing)
- Login, Register, Email Verification, Change Password, Reset Password.

#### Dashboard

The main screen, scoped to the active family (or Default family if none selected).

**Top bar:**
- Family selector (Default + all active non-archived families).
- Display currency selector.
- Date range picker (default: current month; presets: last 3m, 6m, 12m, year-to-date, custom).

**Summary cards (top):**
- Total expenses in selected period (in display currency).
- vs. previous period (delta + percentage).
- Top category this period.
- Number of expenses logged.

**Charts:**
- Monthly bar chart — total per month, last 12 months, stacked by category.
- Category/subcategory pie or donut — breakdown for selected period (uncategorised shown as its own segment).
- Monthly average line — overlay on bar chart.
- "Same month across years" view — e.g., all Februaries side by side.
- Per-currency breakdown panel — each currency's total and its converted equivalent, summed to display-currency total.

**Recent expenses panel:**
- Last 10 expenses. Each row: date, description, category/subcategory, tags, original amount + currency, converted amount.
- "View all" → Expenses screen.

#### Expenses Screen

Full paginated list with:

**Filters (sidebar or top bar):**
- Date range.
- Family member (heads can filter by member).
- Category / subcategory (multi-select; includes "Uncategorised" option).
- Tags (multi-select, AND/OR toggle).
- Currency.
- Amount range (min/max, in display currency).

**Search:**
- Full-text on description field.
- Tag name search.

**Table columns:**
- Date, Description, Category, Subcategory, Tags, Member (family view), Original Amount, Display Amount, Actions.

**Actions:**
- Edit expense (inline or modal).
- Delete expense (own only; head can delete any member's).
- Bulk select → bulk delete (own expenses only) or export.

**Export:**
- CSV export of current filtered view.

#### Add / Edit Expense Screen

- All expense fields.
- Category optional — "No category" is a valid selection.
- Category → subcategory cascades on selection.
- Tag input with autocomplete.
- Amount + currency side by side; live preview of converted amount (using today's rate).
- Date picker.
- Family multi-select (Default always pre-checked and non-removable).
- Save / Cancel.
- Edit screen shows last-modified info (who, when, from where).

#### CSV Import Screen

- File upload (drag-and-drop).
- Downloadable CSV template.
- Preview table: parsed rows, errors highlighted per cell.
- Column mapping step (if headers don't match expected format).
- Confirm import / fix errors.
- Imported expenses are logged as `created_from: bulk:web`.

#### Family Management Screen

- List families (active + archived, tabbed).
- Create new family.
- Per-family (active): member list (name, role, join date), invite by email, remove member, change role, rename family, configure family settings, archive family.
- Archived family: read-only view, unarchive option (head only).
- Default family: rename and settings only; no archive option shown.
- Leave family option (if member; heads must transfer headship first).

#### Profile / Settings Screen

- Personal info, default currency, language.
- Notification preferences (future).
- Connected families overview.

#### Admin Screens

- **Users**: list all users, disable/enable, reset password, assign app role.
- **Currencies**: add currency, set default fallback rate per pair.
- **Conversion rates**: select currency pair, view rate history by date, add/edit rate for specific date, bulk CSV upload.
- **Rate validation**: review and resolve conflicts between automatic and manual rates (see Rate Validation Screen above).
- **Categories**: add/edit/archive categories and subcategories.
- **Audit log**: global view of platform-level events (rate changes, user management); expense-level audit is family-head scoped, not admin-accessible.

---

### Mobile App

Focus: **fast expense entry** and **lightweight dashboard glance**. Full management stays on web.

#### Screens

**Home (quick-add first)**
- Prominent "+" button opens quick-add sheet.
- Below: today's and yesterday's expenses as a short list.
- Weekly spending meter (current week vs. weekly average).

**Quick-Add Sheet (bottom sheet / modal)**
- Amount field (large, numeric keyboard).
- Currency (defaults to user default; tap to change).
- Category picker (large tiles, recent categories shown first; "Skip for now" option).
- Subcategory picker (appears after category selected; skippable).
- Description (optional; one line).
- Tags (optional; type or pick recent).
- Date (defaults to now; tap to change).
- Family attribution (defaults to Default family; tap to add more families).
- Save → haptic feedback, sheet dismisses. Logged as `created_from: single:mobile`.

**Expense List (mobile)**
- Scrollable list, grouped by day.
- Swipe-left on row → delete own expense (with confirm).
- Tap row → view/edit expense detail.
- Edit logged as `modified_from: mobile`.

**Dashboard (mobile)**
- Current month total (display currency).
- Progress bar vs. last month.
- Category breakdown (horizontal bar chart, simplified; uncategorised shown separately).
- Last 5 expenses.

**Profile**
- Default currency, language, active family selector.

#### Mobile-Specific Features
- **Offline entry**: queue expenses locally when offline; sync on reconnect.
- **Receipt photo**: take or pick photo from library, attached to expense.
- **Spending alert**: optional push notification when monthly total crosses a user-defined threshold.
- **Recurring expenses**: mark an expense as recurring (weekly/monthly); app auto-suggests adding it on the due date via notification.
- **Widget** (future): home-screen widget showing current month total.

---

## Suggested Additions

### Budgets
- Per-family or per-user monthly/yearly budget per category.
- Dashboard shows actual vs. budget bars.
- Alert (in-app + push) when approaching or exceeding a budget.

### Recurring Expenses
- Define a recurring template (amount, currency, category, tags, frequency).
- Auto-creates expense on schedule, or prompts user to confirm (configurable).
- Visible in an "Upcoming" section on dashboard.

### Expense Splitting
- When adding an expense, optionally split it across family members.
- Each member sees their share.
- "Settle up" view shows net balances between members.

### Custom Family Categories
- Allow family heads to define family-specific categories/subcategories alongside global ones.

### Reports & Exports
- Scheduled monthly email report (PDF): summary of previous month, top categories, vs. prior month.
- Year-end summary export (PDF + CSV).

### Notifications
- Weekly spending digest (email or push).
- Budget threshold alerts.
- Recurring expense reminders.
- Family head notified when a member adds an expense above a configurable threshold.

### Multi-Language Support
- Already partially implemented (i18n). Extend to cover all new screens.

### Dark Mode
- Theme toggle in profile settings.

---

## Data Model Summary (conceptual)

```
User
├── default_currency
├── UserFamilyMembership[] (role: Head | Member)
└── Expense[]
    ├── amount (original)
    ├── currency
    ├── date
    ├── category? → subcategory?   -- both optional
    ├── description
    ├── tags[]
    ├── ExpenseFamilyAttribution[] -- one or more families
    ├── created_at, created_by, created_from
    ├── modified_at?, modified_by?, modified_from?
    └── (see audit tables below)

ExpenseAuditLog
├── expense_id (FK)
├── operation: add | update | delete
└── performed_at, performed_by, performed_from

ExpenseAuditSnapshot
├── audit_log_id (FK → ExpenseAuditLog)
├── snapshot_type: before | after
└── all expense fields (amount, currency, date, category?, subcategory?, description, tags, families)

Family
├── is_default (bool)
├── is_archived (bool)
├── FamilyMembership[] (user, role)
└── (optionally) custom categories

Category
└── Subcategory[]

Currency
├── default_fallback_rate (per pair)
└── DailyRate[] (date, source_currency, destination_currency, rate, source: auto|manual)

RateConflict (pending admin resolution)
├── date, currency_pair
├── automatic_value, manual_value
└── resolved_at?, resolved_by?, resolution: accept_auto|keep_manual|custom

Tag (user-scoped)
```

---

## Open Design Decisions

| # | Question | Options |
|---|----------|---------|
| 1 | Are categories global-only or family-customisable? | Global default + family override recommended |
| 2 | Can a member see other members' expenses? | Configurable per family by head |
| 3 | Should tags be user-scoped or family-scoped? | User-scoped keeps it simple; family-scoped enables shared tagging |
| 4 | Automatic rate source: which external provider? | ECB (free, EUR-based), Fixer.io, Open Exchange Rates — decision depends on currency coverage needed |
| 5 | Expense splitting: in v1 or later? | Later — complexity is high; track as backlog |
| 6 | Mobile: native or PWA? | PWA first (reuses existing React stack); native later if needed |
| 7 | Hard delete vs. soft delete for expenses? | Audit log retains full state on hard delete; soft delete adds `deleted_at` flag and keeps row queryable |
