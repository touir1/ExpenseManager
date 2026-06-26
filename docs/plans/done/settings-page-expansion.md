# Plan: SettingsPage Expansion

> Source item: `ux-ui-improvements.md` §6 — "Web: SettingsPage is sparse — only 3 settings"  
> Priority: 🟡 Medium  
> Scope: `frontend/dashboard` + `backend/expenses` + `backend/users`

---

## Problem

`SettingsPage` has 3 cards in a 3-col grid (Password, Default Currency, Theme).
The grid looks empty; users expect more customization.
Current `UserConfigDto` only carries `defaultCurrencyId`.

---

## UX/UI Design (via /ui-ux-pro-max)

### Section grouping

Split the page into three labeled sections instead of one flat grid:

| Section | Cards | Visual treatment |
|---------|-------|-----------------|
| **Account** | Password, Notification Preferences | Normal |
| **Preferences** | Default Currency, Theme, Default Expense Date, Default Category | Normal |
| **Danger Zone** | Data Export, Account Deletion | `border border-berry/30 rounded-2xl p-4` wrapper; berry icon badges |

### Card layout

- Keep existing card pattern: `bg-surface-card rounded-2xl border border-surface-border shadow-card p-6`
- Section headings: `text-xs font-semibold text-ink-faint uppercase tracking-widest mb-3`
- Grid: `sm:grid-cols-2 lg:grid-cols-3` inside each section
- Danger Zone: `sm:grid-cols-2` max (never 3-col — destructive actions need breathing room)

### Inline save confirmation

Replace toast-only feedback with an inline "Saved ✓" state on the save button:

```tsx
// Pattern: after successful save, show "Saved ✓" for 2 s then revert
const [saved, setSaved] = useState(false)
async function handleSave() {
  // … save …
  setSaved(true)
  setTimeout(() => setSaved(false), 2000)
}
// Button label: saved ? t('settings.savedConfirm') : t('settings.currency.save')
// Button class when saved: bg-sage text-white (not brand-600)
```

### Destructive action (Account Deletion)

- Card uses `bg-surface-card` normally but icon badge is `bg-berry-soft text-berry`
- "Delete Account" button: `text-berry border border-berry/40 hover:bg-berry-soft` (outline, not filled)
- On click → confirmation modal (two-step: type "DELETE" or click confirm twice)
- Modal title: "Delete your account?" with explicit consequences listed

### Accessibility

- All new selects/toggles: `aria-label` matching card title
- New checkboxes (notification prefs): use `peer` trick (custom styled, same as `ExpenseForm.tsx` family checkboxes pattern)
- Focus management: after modal close, return focus to trigger button

---

## New Settings — Spec

### 1. Default Expense Date (frontend-only)

- Options: "Today" | "Last used date"
- Storage: `localStorage` key `expenseDefaultDate` (`'today' | 'last-used'`)
- No backend change needed
- Consumed by: `ExpenseForm` — pre-fills date field on open
- Section: Preferences

### 2. Default Category (backend + frontend)

- Dropdown of top-level categories from `ExpensesDataProvider.categories`
- Stored in `UserConfig` (backend) — new field `DefaultCategoryId int?`
- Consumed by: `ExpenseForm` — pre-fills category on open
- Section: Preferences

### 3. Notification Preferences (backend + frontend)

- Per-event-type email on/off toggles
- Events: `familyInvitation`, `familyMemberJoined`, `familyMemberRemoved`, `familyExpenseAdded`, `familyExpenseDeleted`, `csvImportCompleted`, `rateConflict`
- Storage: new `NotificationPreferences` table in users DB (or JSON column on `Users`)
- New endpoints: `GET /config/notifications`, `PUT /config/notifications`
- Section: Account

### 4. Data Export (backend + frontend)

- Button triggers `GET /api/expenses/export?format=csv` → streams CSV of all user expenses
- Backend: new `ExpenseExportController` endpoint (no new DB table)
- CSV columns: date, amount, currency, category, subcategory, description, tags, families
- Frontend: button calls the endpoint, triggers browser download via `<a download>`
- Section: Danger Zone (data portability lives here by convention)

### 5. Account Deletion (backend + frontend)

- `DELETE /api/users/me` → soft-deletes user (sets `IsDeleted=true`, `DeletedAt=UtcNow`)
- Publishes `user.deleted` outbox event (already exists for sync to expenses service)
- Frontend: two-step confirm modal
- After deletion: logout + redirect to `/` with a "Your account has been deleted" query param
- Section: Danger Zone

---

## Backend Changes

### `backend/expenses` — UserConfig

**Files:**
- `Models/UserConfig.cs` — add `DefaultCategoryId int?`
- `Migrations/` — `AddDefaultCategoryToUserConfig`
- `Controllers/DTO/UserConfigDto.cs` — add `DefaultCategoryId`
- `Controllers/Requests/UpdateUserConfigRequest.cs` — add `DefaultCategoryId`
- `Validators/UpdateUserConfigRequestValidator.cs` — validate category exists if provided

### `backend/expenses` — Data Export

**Files (new):**
- `Controllers/ExpenseExportController.cs` — `GET /export` (rate limited, no admin guard)
- `Services/ExpenseExportService.cs` / `IExpenseExportService.cs`
- Wire into `Program.cs`
- CSV: use `System.IO.Pipelines` or `StreamWriter` with `text/csv` + `Content-Disposition: attachment`

### `backend/users` — Notification Preferences

**Files (new or extend):**
- `Models/NotificationPreferences.cs` — `UserId int`, `EventType string`, `EmailEnabled bool`; or JSON column on `Users`
- Migration: `AddNotificationPreferences`
- `Controllers/NotificationPreferencesController.cs` — `GET /config/notifications`, `PUT /config/notifications`
- `Services/NotificationPreferencesService.cs`
- `Controllers/DTO/NotificationPreferencesDto.cs`

### `backend/users` — Account Deletion

**Files:**
- `Controllers/UserController.cs` — add `DELETE /me` endpoint (authenticated, rate-limited under `change_password` policy or new policy)
- `Services/UserService.cs` — `DeleteAccountAsync(userId)` calls existing `DeleteUserAsync`
- No new migration (soft-delete already exists: `IsDeleted`, `DeletedAt`)

---

## Frontend Changes (`frontend/dashboard`)

### Step 1 — /ui-ux-pro-max

Run `/ui-ux-pro-max` to validate component designs for each new card before implementation. Focus on:
- Section heading pattern
- Inline save confirmation animation (`bg-sage` flash on button)
- Danger Zone wrapper styling
- Notification preferences toggle row pattern (checkbox + label + description)

### Step 2 — Restructure `SettingsPage.tsx`

- Introduce `<SettingsSection title label>` wrapper component (inline, not a separate file)
- Move existing 3 cards into sections:
  - Password → Account
  - Currency → Preferences
  - Theme → Preferences
- Add inline `savedConfirm` state to `DefaultCurrencyCard` (addresses §6 🟢 "no visual confirmation")

### Step 3 — New components (all in `SettingsPage.tsx` or split if >200 lines)

| Component | Key logic |
|-----------|-----------|
| `DefaultExpenseDateCard` | `localStorage` read/write, radio group (Today / Last used) |
| `DefaultCategoryCard` | `FormCombobox` for categories, `updateConfig({defaultCategoryId})` |
| `NotificationPreferencesCard` | Fetch `GET /config/notifications`, render toggle rows, `PUT` on change (debounced 500ms) |
| `DataExportCard` | Button → `fetch('/api/expenses/export')` → `URL.createObjectURL` + click `<a>` |
| `AccountDeletionCard` | Opens `ConfirmDeleteAccountModal`, calls `DELETE /api/users/me`, then `logout()` |

### Step 4 — i18n keys (all 4 locale files)

New keys under `settings`:

```json
"savedConfirm": "Saved ✓",
"sections": {
  "account": "Account",
  "preferences": "Preferences",
  "dangerZone": "Danger Zone"
},
"expenseDate": {
  "title": "Default Expense Date",
  "description": "Pre-fill the date when adding a new expense.",
  "today": "Today",
  "lastUsed": "Last used date",
  "save": "Save",
  "saving": "Saving…"
},
"defaultCategory": {
  "title": "Default Category",
  "description": "Pre-select a category when opening the add expense form.",
  "none": "No default",
  "save": "Save",
  "saving": "Saving…"
},
"notifications": {
  "title": "Notification Preferences",
  "description": "Choose which events send you an email.",
  "familyInvitation": "Family invitation received",
  "familyMemberJoined": "Member joined your family",
  "familyMemberRemoved": "Removed from a family",
  "familyExpenseAdded": "Expense added in shared family",
  "familyExpenseDeleted": "Expense deleted in shared family",
  "csvImportCompleted": "CSV import completed",
  "rateConflict": "Currency rate conflict",
  "save": "Save",
  "saving": "Saving…"
},
"export": {
  "title": "Export Data",
  "description": "Download all your expenses as a CSV file.",
  "button": "Download CSV",
  "exporting": "Preparing…"
},
"deleteAccount": {
  "title": "Delete Account",
  "description": "Permanently delete your account and all associated data. This cannot be undone.",
  "button": "Delete Account",
  "confirmTitle": "Delete your account?",
  "confirmBody": "All your expenses, families, and settings will be permanently deleted. This action cannot be undone.",
  "confirmButton": "Yes, delete my account",
  "cancelButton": "Cancel",
  "deleting": "Deleting…"
}
```

---

## Unit Tests

### `SettingsPage.test.tsx` — updates

- Add mocks for new API calls: `getNotificationPreferences`, `updateNotificationPreferences`, `deleteAccount`
- Add mock for new `userConfigApi` fields (`defaultCategoryId`)
- Extend `renderSettings` helper if needed for new context

**New test cases:**

```
describe('Section grouping')
  ✓ renders "Account" section heading
  ✓ renders "Preferences" section heading
  ✓ renders "Danger Zone" section heading

describe('DefaultExpenseDateCard')
  ✓ renders "Today" and "Last used date" options
  ✓ reads initial value from localStorage
  ✓ writes to localStorage on selection change

describe('DefaultCategoryCard')
  ✓ renders category combobox
  ✓ pre-selects defaultCategoryId from config
  ✓ calls updateConfig with new categoryId on save
  ✓ shows "Saved ✓" after successful save, reverts after 2s (vi.useFakeTimers)

describe('DefaultCurrencyCard — inline confirmation')
  ✓ shows "Saved ✓" after successful save (existing test extended)
  ✓ button reverts to "Save" after 2 s

describe('NotificationPreferencesCard')
  ✓ renders one toggle per event type (7 rows)
  ✓ calls updateNotificationPreferences on toggle change

describe('DataExportCard')
  ✓ renders "Download CSV" button
  ✓ calls export endpoint on click
  ✓ shows "Preparing…" while fetching

describe('AccountDeletionCard')
  ✓ renders "Delete Account" button with berry styling
  ✓ opens confirmation modal on click
  ✓ calls DELETE /api/users/me and then logout() on confirm
  ✓ does NOT delete on cancel
```

### Backend tests

- `UserConfigServiceTests` — `UpdateAsync_WithDefaultCategoryId_PersistsValue`
- `ExpenseExportServiceTests` — `ExportAsync_ReturnsCsvWithCorrectColumns`, `ExportAsync_EmptyExpenses_ReturnsHeaderOnly`
- `NotificationPreferencesServiceTests` — `GetAsync_ReturnsDefaults_WhenNoRowExists`, `UpdateAsync_PersistsAllFields`
- `UserServiceTests` — `DeleteAccountAsync_SetsIsDeletedAndDeletedAt`, `DeleteAccountAsync_PublishesUserDeletedEvent`

---

## Maintenance Updates

After implementation, update:

| File | What to add |
|------|-------------|
| `CLAUDE.md` | Note `defaultCategoryId` in UserConfig; note notification prefs endpoints; note `DELETE /me` |
| `FILE-TREE.md` | Any new files added |
| `CHANGELOG.md` | Entry for settings expansion |
| `backend/expenses/README.md` | `GET /export`, new UserConfig fields |
| `backend/users/README.md` | `DELETE /me`, notification prefs endpoints |
| `docs/issues/ongoing/fixes-and-suggestions.md` | Mark §6 "SettingsPage sparse" as in-progress / done |

---

## Execution Order

1. **Backend — UserConfig extension** (expenses): migration + DTO + validator
2. **Backend — Notification Preferences** (users): model + endpoints + service
3. **Backend — Data Export** (expenses): controller + service
4. **Backend — Account Deletion** (users): endpoint + service
5. **Frontend — /ui-ux-pro-max** review of card designs before coding
6. **Frontend — Restructure SettingsPage**: sections + inline save confirmation
7. **Frontend — New cards**: DefaultExpenseDate → DefaultCategory → NotificationPreferences → DataExport → AccountDeletion
8. **Frontend — i18n**: all 4 locale files
9. **Tests**: backend unit tests → frontend unit tests (SettingsPage.test.tsx)
10. **Maintenance**: CLAUDE.md, FILE-TREE.md, CHANGELOG.md, READMEs

---

## /done

Run `/done` after implementation to review all changed files and update docs.
