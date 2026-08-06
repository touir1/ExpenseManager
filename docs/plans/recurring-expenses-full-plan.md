# Recurring Expenses — Full Implementation Plan

Builds on the read-only "Upcoming" widget already shipped (section 2 of [dashboard-new-charts-plan.md](dashboard-new-charts-plan.md), CHANGELOG 0.140.0). That shipped `RecurringExpense`/`RecurrenceFrequency` entities, `GetUpcomingAsync`, and `GET /recurring-expenses/upcoming`. This plan adds the piece explicitly deferred there: full CRUD for managing recurring templates, and the auto-create-or-confirm generation flow from `implementation-plan.md`'s roadmap line ("Template + schedule; auto-create or confirm prompt; 'Upcoming' section").

## 1. Schema additions

Existing `RecurringExpense` columns are sufficient for CRUD as-is. Generation needs two new columns — migration `AddRecurringExpenseGeneration`:

- `AutoCreate bool` (default `false`) — `true` = job creates the `Expense` row automatically on due date; `false` = due item surfaces as "needs confirmation" and the user creates it manually from the widget.
- `LastGeneratedDate DateOnly?` — date an `Expense` was last generated from this template; guards the job against double-firing if it runs more than once on the due date (idempotency key alongside `NextDueDate`).

No `EndDate` column — an indefinite recurrence is the only mode for v1 (matches "Template + schedule" wording, not "Template + schedule + end condition"); stopping recurrence = user sets `IsActive=false` or deletes the template. Flag as a follow-up if requirements change.

## 2. Backend — CRUD

Mirror `ExpenseController`/`ExpenseService`/`ExpenseRepository`'s existing conventions (ownership check via `userId`, soft-delete, `FluentValidation` request validators, `CreatedAtRoute` on POST).

**Requests** (`Controllers/Requests/`):
- `CreateRecurringExpenseRequest` — `Description, Amount, CurrencyId, CategoryId, SubcategoryId?, FamilyId?, FrequencyId, NextDueDate, AutoCreate`
- `UpdateRecurringExpenseRequest` — same fields, plus `IsActive` (pause/resume without deleting)
- Validators: `CreateRecurringExpenseRequestValidator`/`UpdateRecurringExpenseRequestValidator`, `CascadeMode.Stop`, `SubcategoryId` requires `CategoryId` (reuse `ExpenseRequestValidatorBase<T>` pattern), `Description` max 500, `Amount > 0`, `NextDueDate >= today` on create.

**Repository** (`IRecurringExpenseRepository` — extend, already has `GetUpcomingAsync`):
- `GetPagedAsync(userId, includeInactive)` → all templates (active + paused) for the management page, ordered by `NextDueDate` asc
- `GetByIdAsync(id, userId)` → ownership-scoped single row
- `AddAsync(RecurringExpense)`, `UpdateAsync(RecurringExpense)`, `SoftDeleteAsync(id, userId)`
- `GetDueForGenerationAsync(asOfDate)` → cross-user query for the Quartz job: `IsActive && !IsDeleted && NextDueDate <= asOfDate && (LastGeneratedDate == null || LastGeneratedDate < NextDueDate)`

**Service** (`IRecurringExpenseService` — extend):
- `CreateAsync(request, userId) → RecurringExpenseDto`
- `UpdateAsync(id, request, userId) → RecurringExpenseDto?` (404 via null, same pattern as `ExpenseService.UpdateAsync`)
- `DeleteAsync(id, userId) → bool`
- `GetAllAsync(userId, includeInactive) → RecurringExpenseDto[]` (management page)
- `ConfirmAsync(id, userId) → ExpenseDto?` — for `AutoCreate=false` items: creates the real `Expense` via `IExpenseService.AddAsync` (source = `SingleWeb`), advances `NextDueDate`/`LastGeneratedDate` on the template. 404 if not found/not due/not owned.
- Shared helper `AdvanceNextDueDate(DateOnly current, int frequencyId)`: Weekly → `+7d`, Monthly → `AddMonths(1)`, Yearly → `AddYears(1)`.

**Controller** (`RecurringExpenseController` — extend):
- `POST /recurring-expenses` → 201, `CreatedAtRoute("GetRecurringExpenseById", ...)`
- `PUT /recurring-expenses/{id}` → 200 or 404
- `DELETE /recurring-expenses/{id}` → 204 or 404
- `GET /recurring-expenses/{id}` (named route for the 201 Location header) → 200 or 404
- `GET /recurring-expenses?includeInactive=false` → management list
- `POST /recurring-expenses/{id}/confirm` → 200 with the created `ExpenseDto`, 404 if not found/not owned, 400 `RECURRING_NOT_DUE` if `NextDueDate` is in the future (guards against confirming early)
- New `ControllerErrors.RecurringExpenseNotFound` / `RecurringExpenseNotDue` constants

**Auto-create job** (`Jobs/RecurringExpenseGenerationJob.cs`):
- `[DisallowConcurrentExecution] IJob`, same shape as `RateAutoUpdateJob` — try/catch + `ILogger`, non-fatal on failure
- Daily cron (new `RecurringExpenseOptions.GenerationTime`, env `EXPENSES_MANAGEMENT_EXPENSES_RECURRINGEXPENSE_GENERATION_TIME`, default `03:00` — after the currency-rate job at `02:00` so a same-day generated expense can pick up a fresh rate)
- For each row from `GetDueForGenerationAsync(today)` where `AutoCreate=true`: create the `Expense` (source = `BulkWeb`, matching CSV-import's precedent for system-initiated inserts), advance `NextDueDate`/`LastGeneratedDate`, best-effort per-row try/catch so one bad template doesn't block the batch (same pattern as `CsvImportService.ConfirmImportAsync`'s per-row exception handling)
- Rows with `AutoCreate=false` are left untouched by the job — they only advance via the user hitting `POST /recurring-expenses/{id}/confirm` from the widget

**Program.cs wiring:** register `RecurringExpenseOptions`, add the job + trigger under the existing `#region Quartz` block alongside `RateAutoUpdateJob`.

## 3. Frontend — management UI

**New page** `RecurringExpensesPage.tsx` (route `/recurring-expenses`, linked from `SettingsPage.tsx` — same tier as other "manage your data" cards): table/list of templates (description, amount, frequency, next due date, active toggle, edit/delete actions), "Add recurring expense" button opening a modal.

**`RecurringExpenseForm.tsx`** (RHF + zod, modal, mirrors `ExpenseForm.tsx`'s `AmountInput`/`StringCombobox` category+currency pickers): fields `description, amount, currencyId, categoryId, subcategoryId?, familyId?, frequencyId, nextDueDate, autoCreate`. Frequency as a simple 3-option select (Weekly/Monthly/Yearly), not a combobox.

**`recurringExpenseApi.service.ts`** additions to the existing `dashboardApi.service.ts`'s sibling (new file, since these aren't dashboard-scoped): `getAll`, `getById`, `create`, `update`, `remove`, `confirm`.

**`UpcomingRecurring.tsx` update**: items with `autoCreate=false` and `nextDueDate <= today` get a "Confirm" button (calls `confirm(id)`, invalidates the `upcomingRecurring` + `recent`/`largest` dashboard queries on success via react-query). Items with `autoCreate=true` show a small "Auto" badge instead, per the a11y "no color-only meaning" rule already used elsewhere in this codebase.

**i18n**: extend `dashboard.recurring.*` with `confirm`, `auto`, `manage` (link to the new page); new `recurringExpenses.*` namespace for the management page itself (title, form labels, delete-confirm dialog, empty state) across en/fr/es/de.

## 4. Tests

**Backend**: `RecurringExpenseRepositoryTests` (add: `GetPagedAsync` scoping, `GetDueForGenerationAsync` — due/not-due/already-generated-today/inactive/deleted cases), `RecurringExpenseServiceTests` (Create/Update/Delete ownership + `AdvanceNextDueDate` for all 3 frequencies including month-end edge case e.g. Jan 31 → Feb 28/29, `ConfirmAsync` not-due guard), `RecurringExpenseControllerTests` (new CRUD + confirm endpoints, 401/404/400 paths), new `RecurringExpenseGenerationJobTests` (mirrors whatever `RateAutoUpdateJob` test coverage looks like, if any — otherwise a service-level test on the generation logic is sufficient since the job itself is a thin Quartz wrapper).

**Frontend**: `RecurringExpensesPage.test.tsx`, `RecurringExpenseForm.test.tsx`, `recurringExpenseApi.service.test.ts`, `UpcomingRecurring.test.tsx` additions (Confirm button appears/calls API/invalidates queries; Auto badge for `autoCreate=true`).

## 5. Build order

1. Migration `AddRecurringExpenseGeneration` (`AutoCreate`, `LastGeneratedDate`) + repository/service/controller CRUD (no job yet) — ships a usable management UI on its own.
2. `RecurringExpenseForm.tsx` + `RecurringExpensesPage.tsx` + `recurringExpenseApi.service.ts`, linked from `SettingsPage.tsx`.
3. `ConfirmAsync` + `POST /recurring-expenses/{id}/confirm` + `UpcomingRecurring.tsx` Confirm button — closes the loop for `AutoCreate=false` templates without needing the job.
4. `RecurringExpenseGenerationJob` + Quartz wiring — `AutoCreate=true` templates now self-generate.
5. Update `CLAUDE.md` constraints, `CHANGELOG.md`, `FILE-TREE.md`, `backend/expenses/README.md` per the maintenance table, same as section 2's rollout.

Each step is independently shippable and testable; step 1 alone already unblocks users maintaining their own recurring list even before auto-generation exists.
