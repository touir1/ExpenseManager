
# Changelog

All notable changes to this project will be documented in this file.

## [0.105.0] - 2026-05-19
### Added
- **Phase 8 — Frontend Expense List & Form**: Full CRUD UI for expenses.
  - **`ExpensesPage`** (`/expenses`): paginated expense table with date, amount (converted if display currency differs), category, description, tags columns; inline Edit/Delete actions; `ConfirmDeleteModal` before deletion; `ExpenseFilters` panel integration; empty-state with "Add your first expense" link; pagination controls when `totalPages > 1`; uses TanStack Query `useQuery` + `deleteExpense`.
  - **`AddExpensePage`** (`/expenses/add`): `ExpenseForm` wired to `addExpense`; navigates to `/expenses` on success.
  - **`EditExpensePage`** (`/expenses/:id/edit`): fetches expense via `useQuery(getExpenseById)`; pre-fills `ExpenseForm`; wired to `updateExpense`; shows loading/not-found states.
  - **`ExpenseForm`**: RHF + Zod form with amount, currency, date, category, subcategory (conditional), description, tags, families fields; `isSubmitting` prop for disabled state.
  - **`ExpenseFilters`**: collapsible filter panel (date range, category/subcategory, currency, amount range, description); `aria-expanded` toggle; resets page to 1 on apply.
  - **Router**: Added `/expenses`, `/expenses/add`, `/expenses/:id/edit` routes (all `ProtectedRoute`).
  - **NavBar**: Added "Expenses" NavLink in desktop nav and mobile menu (active for all `/expenses/*` paths).
  - **Zod schema fix**: `expense.schemas.ts` updated to Zod v4 API (`error:` replaces deprecated `required_error:`/`invalid_type_error:`); `categoryId`/`subcategoryId` use `.catch(undefined)` to silently coerce NaN (from disabled RHF selects) to `undefined`.
  - **Tests**: 56 new tests across `ExpensesPage`, `AddExpensePage`, `EditExpensePage`, `ExpenseForm`, `ExpenseFilters`.

## [0.104.0] - 2026-05-18
### Added
- **Dashboard API (Phase 7 — expenses service)**: 6 new aggregation endpoints under `GET /dashboard/*` powering all dashboard charts and summary cards.
  - `GET /dashboard/summary` — total amount + expense count + previous-period delta (% change) + top category; accepts `?familyId`, `?dateFrom`, `?dateTo`, `?displayCurrencyId`
  - `GET /dashboard/monthly` — per-month totals broken down by category; default range = Jan 1 of current year → today
  - `GET /dashboard/categories` — category/subcategory breakdown with percentages for a period
  - `GET /dashboard/same-month-across-years` — given `?month=1–12`, returns per-year totals across all recorded years
  - `GET /dashboard/by-currency` — per-currency totals + converted amount + expense count
  - `GET /dashboard/recent` — last 10 expenses (delegates to existing paged expense query)
  - All endpoints accept `?familyId` (scopes to family-attributed expenses, verifies membership → 403 if not member), `?displayCurrencyId` (currency conversion via `ICurrencyRateService`), missing userId → 401, other errors → 400
  - Currency conversion: `dateTo` for summary/categories/by-currency; last day of each month for monthly; Dec 31 of each year (or today for current year) for same-month-across-years
  - **`DashboardRepository`**: hybrid SQL/C# approach — EF Core projects to anonymous types (WHERE filter in SQL), GroupBy/Sum in C# (SQLite test compatibility); family scoping via correlated EXISTS subquery on `ExpenseFamilyAttributions`
  - **`DashboardService`**: membership check → currency conversion → DTO assembly; previous-period window = same duration ending the day before `dateFrom`
  - **`ControllerErrors.InvalidMonth`** (`"INVALID_MONTH"`) added for month 0/13 validation
  - **DI**: `IDashboardService`/`DashboardService` and `IDashboardRepository`/`DashboardRepository` registered Scoped in `Program.cs`
  - **Tests**: 47 new tests — 13 `DashboardRepositoryTests` (integration, SQLite), 20 `DashboardServiceTests` (Moq), 14 `DashboardControllerTests` (Moq)

## [0.103.5] - 2026-05-18
### Changed
- **FamiliesPage — hide default family from active list**: `activeFamilies` filter now excludes `isDefault` families (`!f.isArchived && !f.isDefault`). The default family is managed system-side and should not appear in the user-facing configuration UI. Updated all affected `FamiliesPage.test.tsx` tests to use a non-default fixture and replaced the obsolete "shows default badge" test with "does not show default family in active list".

## [0.103.4] - 2026-05-18
### Changed
- **`ServiceErrors` constants class (expenses)**: New `Services/ServiceErrors.cs` (`internal static class`) consolidates all 16 domain error-code strings thrown by service-layer exceptions (`FAMILY_NOT_FOUND`, `USER_NOT_FOUND`, `FAMILY_NOT_MEMBER`, `FAMILY_ALREADY_MEMBER`, `FAMILY_FORBIDDEN`, `FAMILY_CANNOT_INVITE/REMOVE/REMOVE_SELF_HEAD/CHANGE_OWN_ROLE/ARCHIVE/LEAVE/LEAVE_LAST_HEAD _DEFAULT`, `TAG_NOT_VISIBLE`, `FAMILY_INVITATION_{INVALID/ALREADY_ACCEPTED/EXPIRED}`). All hardcoded string literals in `FamilyService`, `ExpenseService`, and `FamilyExceptions` (default ctor args) replaced with `ServiceErrors.*` references. Mirrors the existing `ControllerErrors` pattern.
- **Frontend — `FAMILY_FORBIDDEN` mapping**: Added `FAMILY_FORBIDDEN: 'apiErrors.familyForbidden'` to `BACKEND_ERROR_CODES` in `apiErrors.constant.ts`. Added `"familyForbidden"` translation to all 4 locale files (EN: "You do not have access to this family." and equivalents in FR/ES/DE).

## [0.103.3] - 2026-05-18
### Changed
- **Refactor — `IRateProvider` / `FrankfurterRateProvider` moved to `Infrastructure/`**: Both files relocated from `Services/` to the infrastructure layer (`Infrastructure/Contracts/IRateProvider.cs` and `Infrastructure/FrankfurterRateProvider.cs`). Namespaces updated accordingly. `CurrencyRateService` and `CurrencyRateServiceTests` gain `using Touir.ExpensesManager.Expenses.Infrastructure.Contracts`. No behavioral change.

## [0.103.2] - 2026-05-18
### Changed
- **Test coverage — 100% line coverage across all three projects**: Closed all remaining coverage gaps.
  - **Backend (expenses)**: Model setter tests for `Category.DeletedAt` and `CurrencyRateConflict.Resolution`. Total: **523 tests** (was 454).
  - **Backend (users)**: New `Models/ModelPropertyTests.cs` (4 tests: navigation-property setters for `User`, `UserDto`, `UserRole`, `RefreshToken`); 2 service tests for invalid-GUID paths in `PasswordManagementService`; 2 repository tests for `AuthenticationRepository` catch blocks (duplicate insert and conflicting ChangeTracker update). `UsersAppDbContext.OnModelCreating` refactored — Npgsql-only branch extracted to `ConfigureOutboxIdProperty` helper marked `[ExcludeFromCodeCoverage]`. Total: **326 tests** (was 318).
  - **Frontend**: Outside-click handler tests for `DisplayCurrencySelector` and `TagInput`; new `AcceptInvitePage.test.tsx` (9 tests: loading state, success, error with/without `res.error`, missing token, no API call when token absent, `silent:true` param, go-to-families link on success/error). Total: **560 tests** (was 548).

## [0.103.1] - 2026-05-18
### Changed
- **Family invitation email — inviter name**: Email now includes `@@INVITER_NAME@@` placeholder showing `{FirstName} {LastName}` of the inviting user. `FamilyService.InviteAsync` fetches the inviter via `GetUserByIdAsync` inside the email try-catch; falls back to empty string if user not found.
- **Dynamic copyright year in all email templates**: All 3 templates (`FAMILY_INVITATION_TEMPLATE.html`, `EMAIL_VERIFICATION_TEMPLATE.html`, `PASSWORD_RESET_TEMPLATE.html`) now use `@@YEAR@@` instead of hardcoded `2024`. Both `EmailHelper.GetEmailTemplate` implementations auto-substitute `@@YEAR@@` with `DateTime.UtcNow.Year` before caller-provided parameters.
- **Tests**: 2 new `@@YEAR@@` auto-substitution tests in expenses `EmailHelperTests`; 2 in users `EmailHelperTests`. `InviteAsync_SendsEmail_ToInvitee_WhenValid` updated to set up inviter lookup and verify `INVITER_NAME` is passed to the template. Total: expenses **454** (was 452), users **318** (was 316).

## [0.103.0] - 2026-05-18
### Added
- **Family invitation email (expenses service)**: `InviteAsync` now sends an HTML invitation email to the invitee after persisting the token. Email failure is caught and logged — invitation still succeeds and token is returned. Template: `Assets/EmailTemplates/FAMILY_INVITATION_TEMPLATE.html` with `@@INVITE_LINK@@` and `@@FAMILY_NAME@@` placeholders. Invite URL: `FamilyOptions.InviteBaseUrl` (env: `EXPENSES_MANAGEMENT_EXPENSES_FAMILY_INVITE_BASE_URL`, default `https://localhost/families/accept-invite`) + `?token={token}`.
- **Email infrastructure (expenses service)**: Added `IEmailService`, `SmtpEmailService`, `IEmailHelper`, `EmailHelper`, `EmailOptions`, and `EmailHtmlTemplate` mirroring the users service pattern. Env prefix `EXPENSES_MANAGEMENT_EXPENSES_EMAILAUTH_*`.
- **`FamilyOptions.InviteBaseUrl`**: New property (env: `EXPENSES_MANAGEMENT_EXPENSES_FAMILY_INVITE_BASE_URL`, default `https://localhost/families/accept-invite`).
- **Tests**: 19 new tests. `SmtpEmailServiceTests` (10): all `SendEmail` paths (SSL on/off, CC, BCC, HTML, null body, minimal, empty attachments, single attachment, all params). `EmailHelperTests` (7): template replacement, no-params, empty-params, multi-occurrence, family-invitation placeholders, `SendEmail` delegation×2. `FamilyServiceTests` (2): `InviteAsync_SendsEmail_ToInvitee_WhenValid`, `InviteAsync_EmailFailure_DoesNotPropagate`. Total backend: **452 tests** (was 433).

## [0.102.2] - 2026-05-17
### Changed
- **Test coverage — backend (expenses)**: Added 13 new tests across 4 files. `FamilyControllerTests`: 4 `LeaveAsync` paths (401 no-cookie, 204 success, 403 last-head, 404 not-member). `ExpenseControllerTests`: 4 missing 401 no-cookie tests (UpdateAsync, DeleteAsync, GetByIdAsync, GetPagedAsync). `CurrencyRateControllerTests`: 1 missing `ResolveConflict_ServiceThrows_ReturnsBadRequest`. `Validators/CreateTagRequestValidatorTests` (new file): 4 tests (valid, empty name, too-long, exact max length). Total backend: **433 tests** (was 420).
- **Test coverage — frontend**: Added tests for `FamiliesPage` leave-button flow (show/hide by isDefault/isArchived/headCount, success toast, failure no-toast), `DisplayCurrencySelector` search/filter/clear, and `familyApi.service` `leaveFamily`. Total frontend: **548 tests** (was 530).
- **Sonar fixes — backend**: `FrankfurterRateProvider` — extracted `BaseUrl` and `DateFormat` constants; added `CultureInfo.InvariantCulture` to `ToString`/`ParseExact` calls. `ExpenseService.ResolveConversionAsync` — removed unused `expenseCurrency` parameter and all call sites. `Program.cs` — added `CultureInfo.InvariantCulture` to both `TimeOnly.Parse` calls. `RateAutoUpdateJobTests.Execute_ServiceThrows_DoesNotPropagate` — replaced implicit no-throw assertion with `Record.ExceptionAsync` + `Assert.Null`.
- **Sonar fixes — frontend**: `DisplayCurrencyContext` — removed redundant `useCallback` wrapper; replaced non-standard `setDisplayCurrencyIdState` alias with standard `[displayCurrencyId, setDisplayCurrencyId]` `useState` destructure.

## [0.102.1] - 2026-05-17
### Changed
- **`POST /rates/refresh` — optional source/destination currency filter**: Body now accepts optional `sourceCurrencyId` and `destinationCurrencyId`; when omitted, all pairs are refreshed (previous behavior preserved). Service filters the source-currencies loop and skips non-matching destination entries without an extra API call.
- **Frontend `refreshRates`**: Signature changed from `refreshRates(from: string)` to `refreshRates(params: RefreshRatesParams)` where `RefreshRatesParams = { from, sourceCurrencyId?, destinationCurrencyId? }`.
- **Tests**: 3 new `RefreshRatesFromAsync` filter tests (source filter, dest filter, unknown source → no-op); 1 new controller filter passthrough test. Total backend: **414 tests** (was 410). Frontend tests expanded to **538** (was 535).

## [0.102.0] - 2026-05-17
### Added
- **Rate refresh endpoint + range provider (backend + frontend)**
  - **Backend — `IRateProvider.FetchRatesRangeAsync`**: New interface method `FetchRatesRangeAsync(code, from, to, ct)` → `Dictionary<DateOnly, Dictionary<string, decimal>>` for fetching a date range in one API call.
  - **Backend — `FrankfurterRateProvider`**: Implements `FetchRatesRangeAsync` via Frankfurter's `{from}..{to}?from={code}` endpoint; parses nested date-keyed JSON.
  - **Backend — `ICurrencyRateService.RefreshRatesFromAsync`**: New method iterates all currencies as source, calls `FetchRatesRangeAsync` from the given date to today, inserts auto rates or creates conflicts (same logic as `RunDailyUpdateAsync`).
  - **Backend — `POST /rates/refresh`**: New authenticated endpoint; body `{ from: "YYYY-MM-DD" }`; calls `RefreshRatesFromAsync`; returns 204 or 401/400.
  - **Backend — `RefreshRatesRequest.cs`**: Request DTO with `required DateOnly From`.
  - **Backend — Quartz migration**: `RateAutoUpdateService` (BackgroundService + PeriodicTimer) replaced by `Jobs/RateAutoUpdateJob` implementing `IJob` with `[DisallowConcurrentExecution]`; scheduled via Quartz cron `0 {min} {hour} * * ?` built from `CurrencyRateOptions.UpdateTime`; registered via `AddQuartz` / `AddQuartzHostedService`.
  - **Frontend — `ratesApi.service.ts`**: New service at `features/currencies/services/`; exports `refreshRates(from: string)` calling `POST /api/expenses/rates/refresh`.
  - **Backend tests**: 4 new `RefreshRatesFromAsync` tests in `CurrencyRateServiceTests`; 3 new `RefreshRates_*` + 3 error-path tests in `CurrencyRateControllerTests`; 7 new repository tests (GetHistory, UpdateRate, GetConflictById, UpdateConflict); 3 new `RateAutoUpdateJobTests`. Total backend: **410 tests** (was 386), all passing.
  - **Frontend tests**: 5 new `ratesApi.service` tests. Total frontend: **535 tests** (was 530), all passing.

## [0.101.0] - 2026-05-17
### Added
- **Phase 6 — Currency Rates (backend + frontend)**
  - **Backend — `CurrencyDailyRate` storage & resolution**: New `ICurrencyRateRepository` / `CurrencyRateRepository` with exact-date lookup, most-recent-before fallback, `CurrencyPairDefault` global fallback, and upsert for defaults.
  - **Backend — `ICurrencyRateService` / `CurrencyRateService`**: Resolution chain (same currency → 1; exact date → most-recent → default → null); manual rate insertion with conflict detection; bulk insert; default fallback upsert; conflict resolution (AcceptAuto / KeepManual / Custom); `RunDailyUpdateAsync` for background fetch.
  - **Backend — `IRateProvider` / `FrankfurterRateProvider`**: Fetches ECB rates from `https://api.frankfurter.app/{date}?from={code}` (no API key); registered via `IHttpClientFactory`.
  - **Backend — `RateAutoUpdateService`**: `BackgroundService` with 24-hour `PeriodicTimer`; resolves scoped `ICurrencyRateService` via `IServiceScopeFactory`; calls `RunDailyUpdateAsync` daily.
  - **Backend — `CurrencyRateController`** (`/rates`): `GET /rates/history`, `POST /rates` (201), `POST /rates/bulk` (204), `PUT /rates/default` (204), `GET /rates/conflicts`, `POST /rates/conflicts/{id}/resolve` (204); all require auth; rate-limited via `expenses_global`.
  - **Backend — `ExpenseDto`**: Added `decimal? ConvertedAmount` and `CurrencyDto? DisplayCurrency` for converted display.
  - **Backend — `ExpenseFilterDto`**: Added `int? DisplayCurrencyId` query param.
  - **Backend — `ExpenseService`**: Injected `ICurrencyRateService`; `GetByIdAsync` and `GetPagedAsync` now accept `displayCurrencyId` and call `ResolveConversionAsync` to populate `ConvertedAmount`.
  - **Backend — `ExpenseController`**: `GetByIdAsync` now accepts `[FromQuery] int? displayCurrencyId`.
  - **Backend tests**: `CurrencyRateServiceTests` (15 tests), `CurrencyRateControllerTests` (12 tests), `CurrencyRateRepositoryTests` (13 tests), `ExpenseServiceConversionTests` (5 tests). Total backend: **386 tests** (was 341), all passing.
  - **Frontend — `DisplayCurrencyContext`**: Session-only React context (`number | null`, default null); no localStorage persistence.
  - **Frontend — `DisplayCurrencySelector`**: Dropdown in NavBar showing all currencies from `ExpensesDataContext`; "No conversion" option clears selection; mirrors `FamilySelector` ARIA pattern.
  - **Frontend — `AppProviders`**: Added `DisplayCurrencyProvider` to provider compose chain.
  - **Frontend tests**: `DisplayCurrencyContext.test.tsx` (5 tests), `DisplayCurrencySelector.test.tsx` (9 tests). Total frontend: **530 tests** (was 516), all passing.

## [0.100.0] - 2026-05-17
### Added
- **Family member self-action guards (backend + frontend)**
  - **Backend — `RemoveMemberAsync`**: Head can now remove themselves from a family only when another Head exists; blocked with `FamilyForbiddenException("FAMILY_CANNOT_REMOVE_SELF_HEAD")` when they are the sole Head. Previously self-removal was always blocked.
  - **Backend — `ChangeRoleAsync`**: Any user attempting to change their own role now throws `FamilyForbiddenException("FAMILY_CANNOT_CHANGE_OWN_ROLE")` immediately, regardless of their current role.
  - **Backend — `IFamilyRepository` / `FamilyRepository`**: Added `CountHeadsAsync(familyId, headRoleId)` — counts Head-role memberships for a family; used by the self-removal guard.
  - **Backend tests**: 3 new unit tests in `FamilyServiceTests.cs` — `RemoveMemberAsync_ThrowsForbidden_WhenRemovingSelf_AndNoOtherHead`, `RemoveMemberAsync_AllowsSelfRemoval_WhenOtherHeadExists`, `ChangeRoleAsync_ThrowsForbidden_WhenChangingOwnRole`. Total backend: **341 tests** (was 338), all passing.
  - **Frontend — `FamiliesPage.tsx`**: Member list now hides the role-toggle button for the current user (`isSelf` via email match) and hides the remove button for the current user when they are the sole Head. Uses `useAuth()` for current-user identification.
  - **Frontend tests**: Added `vi.mock('@/features/auth/AuthContext')` to `FamiliesPage.test.tsx`. Total frontend: **62 tests** in that suite, all passing.

## [0.99.0] - 2026-05-16
### Fixed
- **Sonar alerts — `TagInput.tsx`**: renamed `useTag` import alias to `adoptTag` (React hooks naming rule); removed `onClick` from wrapper div (a11y `no-static-element-interactions`); changed `role="listbox"` → `role="menu"` and `role="option"` → `role="menuitem"` on dropdown (avoid `prefer-tag-over-role`); added `aria-haspopup="menu"` + `aria-controls="tag-input-menu"`; `li` wrappers get `role="none"`/`role="presentation"`; interface fields now `readonly`.
- **Sonar alert — `HomePublicPage.tsx`**: added explicit `{' '}` between dot `<span />` and adjacent text node (`react/jsx-child-element-spacing`).
- **Sonar alerts — `FamiliesPage.tsx`**: extracted `detailPanel` constant to eliminate nested ternary in `expandedContent`; extracted `familiesList` constant to eliminate nested ternary in `listContent`.

### Added
- **`TagControllerTests.cs`** — 13 new backend unit tests covering all three `TagController` endpoints (`GetTagsAsync`, `UseTagAsync`, `RemoveTagAsync`): 401 no-cookie, 200 success, 404 not-found paths. Total backend: **338 tests** (was 325).
- **`TagInput.test.tsx`** — 4 additional tests: `getTags ok=false` branch (tag list stays empty), `useTag ok=false` branch (onChange not called), `Enter` key selects first result, `Enter` key triggers create when no match. Total frontend: **515 tests** (was 511). Updated all role queries from `listbox`/`option` → `menu`/`menuitem`.

## [0.98.0] - 2026-05-16
### Added
- **Phase 5 — Tags (backend + frontend)**
  - **Schema redesign**: Tags are now global (unique by name, case-sensitive). Removed `UserId` column from `Tags` table; added `UserTags` junction table `(UserId, TagId)` to track which users have "adopted" each tag. Tag adoption is created automatically when a user attaches a tag to an expense, or explicitly via `POST /tags`.
  - **Migration `AddUserTagsRefactorTags`**: drops `Tags.UserId` FK + column, adds unique index on `Tags.Name`, creates `UserTags` with Cascade on user-delete and Restrict on tag-delete.
  - **New models**: `UserTag.cs` — junction entity `(UserId, TagId)` with `User` and `Tag` nav properties.
  - **`Tag.cs`**: Removed `UserId`/`User`; added `ICollection<UserTag> UserTags` and `ICollection<ExpenseTag> ExpenseTags`.
  - **`ITagRepository` / `TagRepository`**: `GetOwnAsync`, `GetFamilyAsync` (co-member tags not adopted by user, excluding deleted families), `GetByNameAsync`, `GetByIdsAsync`, `AddAsync`, `EnsureUserTagAsync` (idempotent adopt), `RemoveUserTagAsync`, `IsVisibleAsync` (own or co-member).
  - **`ITagService` / `TagService`**: `GetVisibleAsync` (parallel own + family fetch → `TagListDto`), `UseTagAsync` (find-or-create tag + adopt), `RemoveTagAsync` (removes UserTag only; tag entity persists for history).
  - **`TagController`** (`GET /tags`, `POST /tags`, `DELETE /tags/{id}`): all endpoints read userId from JWT cookie; `GET` returns `TagListDto { own, family }`; `POST` is idempotent (case-sensitive); `DELETE` returns 204 or 404.
  - **`TagDto`** (`{ id, name }`), **`TagListDto`** (`{ own: TagDto[], family: TagDto[] }`), **`CreateTagRequest`** + **`CreateTagRequestValidator`** (name not-empty, max 50 chars).
  - **Expense requests updated**: `IExpenseRequest`, `CreateExpenseRequest`, `UpdateExpenseRequest` now include `int[]? TagIds`; `ExpenseFilterDto` includes `int[]? TagIds` (OR-semantics filter).
  - **`ExpenseDto`**: added `IEnumerable<TagDto> Tags`.
  - **`ExpenseRepository`**: `GetByIdAsync` and `GetPagedAsync` now `.Include(e => e.ExpenseTags).ThenInclude(et => et.Tag)`; added `ClearExpenseTagsAsync` and `AddExpenseTagsAsync`; tag filter in `GetPagedAsync`.
  - **`ExpenseService`**: tag visibility validation (`IsVisibleAsync` → 403), auto-adopt (`EnsureUserTagAsync`), tag writes in `AddAsync`/`UpdateAsync` with before/after audit snapshots; `MapToDto` uses explicit tags for add/update, nav property for read paths.
  - **`IExpenseAuditService`** / **`ExpenseAuditService`**: audit methods now accept `string tags`/`string beforeTags`/`string afterTags` to record comma-separated tag IDs in snapshots.
  - **Backend tests**: `TagServiceTests.cs` (10 unit tests, Moq); `TagRepositoryTests.cs` (16 integration tests via `TestExpensesDbContextWrapper`); `ExpenseServiceTests.cs` updated for new `ITagRepository` dependency and explicit tag audit args. Total: 325 tests, all passing.
  - **Frontend `features/tags/`**:
    - `types/tag.type.ts` — `Tag { id, name }`, `TagList { own, family }`.
    - `services/tagsApi.service.ts` — `getTags()`, `useTag(name)`, `removeTag(id)`.
    - `components/TagInput.tsx` — grouped combobox with "My tags" / "Family tags" sections, chip display, create option, keyboard (Enter/Escape/Backspace) support.
    - `components/__tests__/TagInput.test.tsx` — 13 component tests. Total frontend: 511 tests, all passing.

## [0.97.3] - 2026-05-15
### Fixed
- **Sonar static analysis — backend and frontend**: Resolved all flagged issues across FamilyService, FamilyContext, LanguageSwitcher, FamilySelector, FamiliesPage, and HomePublicPage.
  - **`FamilyService.cs`**: Extracted `"Member"` and `"Head"` string literals to `private const string RoleMember` / `RoleHead` (used 4+ times each).
  - **`FamilyContext.tsx`**: `parseInt` → `Number.parseInt` (2 occurrences); renamed useState destructure to `activeFamilyIdRaw`/`setActiveFamilyIdRaw` to satisfy naming-convention rule for setter/state pair.
  - **`LanguageSwitcher.tsx`**: `placement` prop wrapped in `Readonly<{}>`, `aria-haspopup="listbox"` → `"menu"`, dropdown changed from `role="listbox"` + `role="option"` + `aria-selected` to `role="menu"` + `role="menuitemradio"` + `aria-checked`.
  - **`FamilySelector.tsx`**: Same ARIA pattern fix as LanguageSwitcher (`role="menu"`, `role="menuitemradio"`, `aria-checked`, `aria-haspopup="menu"`).
  - **`FamiliesPage.tsx`**: All 7 inline component prop types wrapped in `Readonly<{}>` (`RoleBadge`, `Modal`, `CreateFamilyModal`, `RenameFamilyModal`, `InviteMemberModal`, `FamilyDetailPanel`, `FamilyCard`); Modal keyboard/outside-click handling refactored from `onClick`/`onKeyDown`/`role="presentation"` on the backdrop div to a `useRef` + `document.addEventListener('mousedown'/'keydown')` pattern (identical to `FamilySelector`) — eliminates the `jsx-a11y/prefer-tag-over-role` alert on `role="presentation"`; two nested ternaries extracted to `expandedContent` and `listContent` variables.
  - **`HomePublicPage.tsx`**: Self-closing `<span ... />` before text node changed to explicit `<span ...></span>`.
  - **Tests updated**: `LanguageSwitcher.test.tsx`, `FamilySelector.test.tsx`, `FamiliesPage.test.tsx` updated to match new ARIA roles/attributes; empty assertion test `'shows archived badge for archived family'` filled in; Escape-key test added to `Modal` describe block — `FamiliesPage.tsx` now at 100% statement/branch/function/line coverage (62 tests).

## [0.97.2] - 2026-05-15
### Changed
- **LanguageSwitcher — trigger button styled to match FamilySelector**: Removed border (`border border-surface-border`), orange focus ring (`focus:ring-2 focus:ring-brand-500`), and `bg-transparent`. Now uses `font-medium px-2.5 py-1.5 rounded-lg text-ink-mute hover:text-ink hover:bg-surface-subtle` — identical pattern to the `FamilySelector` navbar button. Also removed custom `boxShadow` inline style from the dropdown list; uses plain `shadow-lg`.

## [0.97.1] - 2026-05-15
### Fixed
- **FamilySelector — name no longer truncated**: Removed `max-w-[140px]` and `truncate` from the selector button; family names now display in full.
- **Family name max length — backend/frontend parity**: Reduced max from 100 → 30 characters across all layers:
  - `CreateFamilyRequestValidator` + `RenameFamilyRequestValidator` (FluentValidation): `MaximumLength(30)`
  - `family.schemas.ts` (Zod): `.max(30, ...)`
  - All 4 translation files (`en/fr/es/de`): `familyNameMax` message updated to "30 characters"
  - Validator tests + schema tests updated to use 30/31 boundary values
- **LanguageSwitcher — flag emoji not rendering on Windows**: Native `<select>` cannot render emoji flag characters on Windows (renders as ISO country code letters "GB", "FR" etc.). Replaced with a custom ARIA-compliant dropdown (button + `role="listbox"` + `role="option"` items) using inline SVG flag components (`FlagEN`, `FlagFR`, `FlagES`, `FlagDE`). Supports `placement` prop (`'down'` default / `'up'` for user menu and mobile). Tests rewritten to target the custom dropdown.

## [0.97.0] - 2026-05-15
### Changed
- **Frontend — NavBar and user menu redesign**:
  - **`NavBar.tsx`** (authenticated): `FamilySelector` moved from left nav to right-side controls (before notification button and user avatar). Notification bell button added between family selector and user menu (placeholder, does nothing). User menu dropdown: Settings link now shows a cog icon; language section restructured as a labeled row (`Language` label + `LanguageSwitcher` inline); Sign out button now shows a logout icon. Dropdown width increased to `w-56` to accommodate the labeled language row.
  - **`NavBar.tsx`** (unauthenticated / desktop): `LanguageSwitcher` now shown in the desktop nav right-side controls (before Sign in / Get started) so language can be changed without logging in.
  - **`LanguageSwitcher.tsx`**: Flag emoji prepended to each language option in the native `<select>` (intermediate state; superseded by the SVG-flag custom dropdown in v0.97.1).
  - **`SettingsPage.tsx`**: Language card removed — language is now exclusively configurable from the NavBar user menu (both authenticated dropdown and unauthenticated desktop nav) and the mobile menu.
  - **i18n** (`en/fr/es/de` `translation.json`): Added `nav.notifications` key.
  - **Tests**: `NavBar.test.tsx` — 3 active-link-styling Settings finders updated from `a.textContent === 'Settings'` to `a.textContent?.trim() === 'Settings'` (Settings link now has SVG child, adding whitespace to `textContent`).

## [0.96.2] - 2026-05-15
### Fixed
- **Sign out still redirected to `/login` with deferred `useState` pattern**: Root cause was that `ProtectedRoute`'s `<Navigate to="/login" replace />` effect fires in the same React commit as NavBar's `useEffect`, but later (tree order: NavBar is earlier). NavBar's `navigate('/')` (pushState) ran first, then ProtectedRoute's effect ran `replaceState('/login')`, overwriting it. Fix: wrap `navigate('/')` in `Promise.resolve().then(...)` so it fires as a microtask, after all same-commit effects including ProtectedRoute's redirect, letting the push to `/` be the final history entry. Also switched from `useState` loggingOut flag to `useRef` to avoid urgent re-render interrupting React Router's startTransition.

## [0.96.1] - 2026-05-14
### Fixed
- **Sign out redirected to `/login` instead of `/`**: Three-layer fix.
  1. `api.service.ts` — `redirectToLogin()` changed from `if (handler) handler(); else location.assign('/login')` to `handler?.()`. The `else` fallback was firing for any racing 401 after logout cleared the handler, hard-navigating to `/login` regardless of app state.
  2. `AuthContext.logout()` — calls `onUnauthorized(null)` before `logoutRequest()` so the handler is already cleared when any in-flight request resolves with 401.
  3. `NavBar.tsx` — replaced synchronous `navigate('/')` inside `handleLogout` with a deferred pattern: `setLoggingOut(true)` in the click handler + `useEffect` that navigates only when `loggingOut && !isAuthenticated`. React 18 automatic batching means `navigate('/')` fired before `isAuthenticated` flipped → `PublicOnlyRoute` at `/` still saw the user as authenticated → redirected to `/dashboard` → `ProtectedRoute` saw `false` → `/login`.

### Tests
- **Frontend coverage: 478 → 492 tests, 98.51% → 100% statements/lines/functions, 93.77% → 99.28% branches**
  - `NavBar.test.tsx` (+4): user avatar button toggles `aria-expanded`, outside-click closes user menu, Settings link closes user menu, `?` initials fallback when user has no name
  - `NavBar.test.tsx` (updated): 2 logout tests (desktop + mobile Sign out) now use `rerender` with `isAuthenticated: false` after clicking Sign out, so the deferred `useEffect` navigation fires and `mockNavigate` is asserted correctly
  - `FamiliesPage.test.tsx` (+10): archive/unarchive/rename/invite/removeMember/changeMemberRole failure paths (no toast/no close on API error); rename and invite validation error branches; `getFamilyById` failure (no detail panel); active-tab switch-back after viewing archived
  - `VerifyErrorPage.tsx`: `/* c8 ignore next */` on env-var `??` line (non-testable branch)

## [0.96.0] - 2026-05-14
### Changed
- **Frontend — marketing landing page & navbar redesign**: No functionality changed; pure UI overhaul to match the Hearth design reference (`docs/design/marketing.jsx` + `docs/design/dashboard-a.jsx`).
  - **`HomePublicPage.tsx`**: Replaced minimal placeholder with full marketing hero — 2-col grid layout (`lg:grid-cols-2`), large Instrument Serif headline with italic brand accent, two CTA pills (Sign in dark / Create account light), social-proof avatar row, and an animated `HeroReceipt` card (rotated receipt with tape decoration, line items, total, split avatars) visible on `lg:` screens only.
  - **`NavBar.tsx`** (public/unauthenticated state): Desktop nav now shows marketing anchor links (How it works, For families, Pricing, Help) + Sign in NavLink + Get started pill button; mobile menu keeps Home + Sign in + Get started.
  - **`NavBar.tsx`** (authenticated state): Added user-avatar button (initials pill, `bg-brand-500`) with dropdown containing Settings NavLink, LanguageSwitcher, divider, and Sign out button. Dropdown is always rendered (CSS `hidden` toggled) for test stability. Outside-click closes it via `mousedown` listener.
  - **`SettingsPage.tsx`**: Added language card (globe icon, `bg-sky-soft`) exposing `LanguageSwitcher` inline — language is now accessible from both the navbar user menu and the Settings page.
  - **i18n** (`en/fr/es/de` `translation.json`): Added keys `nav.userMenu`, `nav.howItWorks`, `nav.forFamilies`, `nav.pricing`, `nav.help`; `settings.language.title`, `settings.language.description`.
  - **Tests**: Updated `NavBar.test.tsx` (replaced `Home`-count assertions with `Sign in`/`Home`-presence checks to match new public desktop nav) and `HomePublicPage.test.tsx` (`bg-surface-page` class check).

## [0.95.0] - 2026-05-13
### Changed
- **Frontend UI redesign — Hearth design system**: Applied warm terracotta/cream palette throughout the dashboard; no functionality changed.
  - **Design tokens** (`tailwind.config.ts`): Full Hearth palette — `brand` (clay #C8623E), `surface` (page/card/subtle/border/muted), `ink` (DEFAULT/body/mute/faint), `sage`, `berry`, `mustard` with semantic variants; font families Manrope (sans), Instrument Serif (serif), JetBrains Mono (mono); custom `shadow-card` / `shadow-card-md` tokens
  - **Google Fonts** (`index.html`): Manrope 400–700, Instrument Serif regular/italic, JetBrains Mono regular
  - **Component CSS** (`src/styles/index.css`): `@layer components` primitives — `.field-label`, `.field-input`, `.btn-primary`, `.msg-error`, `.msg-success`, `.auth-card`
  - **Split-screen auth layout** (`AuthBrandPanel.tsx` NEW): Terracotta gradient brand panel (46% width, `hidden lg:flex`); Login uses brand-left/form-right, Register uses form-left/brand-right
  - **NavBar**: `bg-surface-card`, active nav = `bg-brand-100 text-brand-600`, inactive = `text-ink-mute hover:bg-surface-subtle`
  - **HomeDashboardPage / SettingsPage**: Cards `bg-surface-card border-surface-border`; headings `text-ink`; skeleton loaders `bg-surface-muted`; icon bg `bg-brand-100`; coming-soon = `text-mustard` / `bg-mustard-soft`
  - **PasswordStrength**: weak/met bars `bg-berry`/`bg-sage`; fair `bg-mustard`; met criteria `text-sage`; unmet `text-ink-faint`
  - **Toast**: success `bg-sage-soft border-sage/30`; error `bg-berry-soft border-berry/30`; info unchanged (`bg-sky-50`)
  - **BackLink**: `text-ink-mute hover:text-ink-body`
  - **LanguageSwitcher**: `text-ink-mute border-surface-border hover:border-surface-muted focus:ring-brand-500`
  - Test assertions updated across `NavBar.test.tsx`, `PasswordStrength.test.tsx`, `Toast.test.tsx`, `LanguageSwitcher.test.tsx` to match new class names; 478 tests pass

## [0.94.2] - 2026-05-12
### Fixed
- **`FamilyController.CreateAsync` / `ExpenseController.CreateAsync` — 500 on 201 response**: `CreatedAtAction(nameof(GetByIdAsync), ...)` passed `"GetByIdAsync"` to the URL helper, but `MvcOptions.SuppressAsyncSuffixInActionNames` (default `true`) registers action descriptors without the "Async" suffix → `"GetById"`. URL helper returned `null` → `InvalidOperationException: No route matches the supplied values` during response formatting. Fixed by switching to `CreatedAtRoute` with named routes: `[HttpGet("{id:int}", Name = "GetFamilyById")]` / `[HttpGet("{id:long}", Name = "GetExpenseById")]` and returning `CreatedAtRoute("GetFamilyById", ...)` / `CreatedAtRoute("GetExpenseById", ...)` — route name lookup is unaffected by `SuppressAsyncSuffixInActionNames`.
- Updated `ExpenseControllerTests` and `FamilyControllerTests`: `Assert.IsType<CreatedAtActionResult>` → `Assert.IsType<CreatedAtRouteResult>`.

## [0.94.1] - 2026-05-12
### Added
- **Frontend test coverage — family system + auth API:**
  - `FamilySelector.test.tsx` — 19 tests: null-render (no active / all-archived families), toggle button, label, dropdown open/close, option list, `setActiveFamilyId` calls, aria attributes, click-outside via `fireEvent.mouseDown`
  - `FamilyContext.test.tsx` — full coverage of `FamilyProvider`: load on auth, loading state, unauthenticated clear, `activeFamilyId` localStorage persistence, stale/archived ID eviction, `setActiveFamilyId`, `refresh`
  - `FamiliesPage.test.tsx` — 51 tests: layout, loading/empty states, active/archived tabs, `FamilyCard` actions (expand, rename, archive, unarchive, invite), `FamilyDetailPanel` (member list, remove, role change), `CreateFamilyModal` submit/error, Head-only button gating
  - `authApi.service.test.ts` — 10 tests covering all auth service functions including previously uncovered `resendVerificationRequest`
  - `NavBar.test.tsx` — added test for mobile Families link closing the mobile menu (line 185 coverage)
  - Total frontend test suite: **478 tests** (up from 427)

## [0.94.0] - 2026-05-12
### Added
- **Phase 4 frontend — family system:**
  - `familyApi.service.ts` — full CRUD for families, invitations, and member management
  - `FamilyContext` (`FamilyProvider` / `useFamilies`) — loads family list on auth, persists active family to `localStorage`
  - `FamiliesPage` — management screen with active/archived tabs, family cards, inline member list, create/rename/archive/invite modals
  - `FamilySelector` — NavBar dropdown to switch active family scope; hidden when no non-default active families
  - `family.schemas.ts` — Zod schemas for create-family and invite-member forms
  - `family.type.ts` — `Family`, `FamilyDetail`, `FamilyMember`, `FamilyRole` TypeScript types
  - Full i18n — `families.*` and `validation.familyName*` keys added to all 4 locales (en, fr, es, de)
  - `/families` route wired into router and protected

## [0.93.1] - 2026-05-11
### Changed
- **Hardcoded expiry values → configurable options (both services):**
  - `FamilyService` invite expiry (`InviteExpiry = TimeSpan.FromDays(7)`) → `FamilyOptions.InviteExpiryInDays` via `IOptions<FamilyOptions>`; env var `EXPENSES_MANAGEMENT_EXPENSES_FAMILY_INVITE_EXPIRY_IN_DAYS` (default 7)
  - `RegistrationService` email verification expiry (hardcoded 24 h) → `AuthenticationServiceOptions.EmailVerificationExpiryInHours`; env var `EXPENSES_MANAGEMENT_USERS_AUTHSERVICE_EMAIL_VERIFICATION_EXPIRY_IN_HOURS` (default 24)
  - `PasswordManagementService` password reset expiry (hardcoded 24 h) → `AuthenticationServiceOptions.PasswordResetExpiryInHours`; env var `EXPENSES_MANAGEMENT_USERS_AUTHSERVICE_PASSWORD_RESET_EXPIRY_IN_HOURS` (default 24)
  - `RefreshTokenService` short-lived refresh expiry (hardcoded 1 day) → `JwtAuthOptions.ShortLivedRefreshExpiryInDays`; env var `EXPENSES_MANAGEMENT_USERS_JWT_SHORT_REFRESH_EXPIRY_IN_DAYS` (default 1)
  - `PasswordManagementService` reset-password base URL (hardcoded) → `AuthenticationServiceOptions.ResetPasswordBaseUrl`; env var `EXPENSES_MANAGEMENT_USERS_AUTHSERVICE_RESET_PASSWORD_URL`
  - All 5 env vars added to `infrastructure/.env`, `.env.example`, and `docker-compose-apps.yml`
- **FamilyController — removed `RoleNameToId` hardcoded dictionary**: role name→ID resolution moved into `FamilyService.ChangeRoleAsync(string roleName)` via `ILookupCacheService.GetIdAsync<FamilyRole>`; `ChangeRoleAsync` signature changed from `int newRoleId` to `string roleName`
- **FamilyService — removed hardcoded `RoleHead = 1` / `RoleMember = 2` constants**: all usages replaced with `await _lookupCache.GetIdAsync<FamilyRole>("Head")` / `("Member")` per method; normalised via `char.ToUpperInvariant + ToLowerInvariant` before lookup
- **Shared error constants — `ControllerErrors` static class** added to both services (`Controllers/ControllerErrors.cs`): all inline `private const string` error codes consolidated; controllers reference `ControllerErrors.ServerError`, `ControllerErrors.MissingUser`, etc. — no using directive needed (same namespace)
- **`ExpenseController` — `FamilyForbiddenException` now returns 403**: previously caught by generic `catch (Exception)` → `400 SERVER_ERROR`; now caught specifically → `403 Forbidden` with `ex.Message`; applies to `POST /expenses` and `PUT /expenses/{id}`; `[ProducesResponseType(403)]` added to both actions
### Fixed
- `RefreshTokenServiceTests`, `RegistrationServiceTests`, `PasswordManagementServiceTests`, `FamilyServiceTests` — test option factories updated with newly required fields (`ShortLivedRefreshExpiryInDays`, `EmailVerificationExpiryInHours`, `PasswordResetExpiryInHours`, `InviteExpiryInDays`); previously defaulted to 0 causing expiry-check tests to fail
- 2 new `ExpenseControllerTests` covering 403 path for Create and Update when user is not a family member; 299 expenses tests / 316 users tests — all pass

## [0.93.0] - 2026-05-11
### Added
- **Phase 4 — Family System (expenses service):**
  - `FamilyInvitation` model + `Phase4_FamilyInvitation` EF Core migration
  - `IFamilyRepository` / `FamilyRepository` — family CRUD, membership CRUD, invitation CRUD, expense attribution helpers (`AddAttributionsAsync`, `ClearAttributionsAsync`, `RemoveMemberAttributionsAsync`)
  - `IFamilyService` / `FamilyService` — `CreateDefaultAsync` (idempotent), `CreateAsync`, `GetByUserAsync`, `GetByIdAsync`, `RenameAsync`, `InviteAsync` (GUID token, 7-day expiry), `AcceptInviteAsync`, `RemoveMemberAsync`, `ChangeRoleAsync`, `ArchiveAsync`, `UnarchiveAsync`
  - `FamilyController` — 10 endpoints: list, detail, create, rename, archive, unarchive, invite, accept-invite, remove-member, change-role; head-only guards via custom exceptions (403/404/409)
  - FluentValidation for `CreateFamilyRequest`, `RenameFamilyRequest`, `InviteMemberRequest`, `ChangeMemberRoleRequest`
  - `FamilyIds int[]?` added to `CreateExpenseRequest` / `UpdateExpenseRequest`; null → auto-attribute to user's default family; empty → no attribution; provided → validate membership + write `ExpenseFamilyAttribution` rows
  - `UserEventConsumer` extended: `user.created` now calls `CreateDefaultAsync` after `SaveOrUpdateUserAsync` (idempotent — safe to replay)
  - `IUserRepository.GetUserByEmailAsync` added for invite lookup; `GetUserByIdAsync` now filters soft-deleted users
  - 42 unit tests for `FamilyService`, 30+ controller tests (`FamilyControllerTests`), repository tests (`FamilyRepositoryTests`), validator tests (`FamilyValidatorTests`); all 298 tests pass

## [0.92.1] - 2026-05-10
### Added
- **Swagger documentation — users service:** XML `<summary>` and `<param>` doc comments added to all 14 controller actions; added missing `[ProducesResponseType]` attributes across `AuthenticationController`, `PasswordController`, and `RegistrationController` so every route shows complete request/response shapes in Swagger UI
- **Swagger documentation — expenses service:** XML `<summary>` and `<param>` doc comments added to all 5 `ExpenseController` actions, `CategoryController.GetAllAsync`, and `CurrencyController.GetAllAsync`; no missing `[ProducesResponseType]` attributes found

## [0.92.0] - 2026-05-10
### Added
- **Verification link expiry (users service):**
  - `User.EmailValidationHashExpiresAt` (`DateTime?`) — stored in `USR_EmailValidationHashExpiresAt`
  - Migration `AddEmailValidationHashExpiry` — adds nullable column to `USR_Users`
  - `ValidateEmailAsync` returns `null` (→ redirect to `/verify-error`) when hash exists but is past its expiry time
  - Expiry window: 24 hours (consistent with password reset tokens)
- **Resend verification email:**
  - `IUserRepository.UpdateEmailValidationHashAsync(userId, newHash, expiresAt)` — atomically replaces the old hash + expiry in one DB write; old link immediately invalid
  - `ResendResult` enum (`Sent`, `NotFound`) in `Services/ResendResult.cs`
  - `IRegistrationService.ResendVerificationEmailAsync(email, applicationCode)` — finds user, generates new hash, calls `UpdateEmailValidationHashAsync`, sends email; returns `NotFound` for unknown or already-validated emails
  - `POST /auth/resend-verification` endpoint — always returns `200 OK` regardless of result to prevent account-existence leaks; validated by `ResendVerificationRequestValidator`
  - `ResendVerificationRequest` DTO (`Email`, `ApplicationCode`)
  - Re-registration with an unverified-email account silently triggers resend + hash rotation (no error returned to caller)
  - `/verify-error` redirect now includes `?email=…&app_code=…` query params so the frontend can surface the resend button
- **Rate limiting — users service (built-in .NET 8 `Microsoft.AspNetCore.RateLimiting`):**
  - `login`: 10 req / 1 min per IP (fixed window)
  - `register`: 5 req / 10 min per IP
  - `resend_verification`: 3 req / 10 min per IP
  - `validate_email`: 10 req / 5 min per IP
  - `request_password_reset`, `change_password_reset`, `create_password`: 5 req / 10 min per IP
  - `change_password`: 10 req / 5 min per IP
  - `refresh`: 20 req / 1 min per IP
  - `messaging_replay`: 5 req / 1 min per IP
- **Rate limiting — expenses service:** sliding window 100 req / 60 s per IP applied globally to all expenses routes via `[EnableRateLimiting("expenses_global")]`
- **Frontend — `VerifyErrorPage`:** reads `email` + `app_code` from `useSearchParams()`; shows "Resend verification email" button when both params present; on click calls `resendVerificationRequest` and replaces button with success message
- **Frontend — `authApi.service.ts`:** `resendVerificationRequest(email, appCode)` — `POST /auth/resend-verification` with `skipUnauthorized + silent` flags
- **i18n:** added `public.verifyError.resend` and `public.verifyError.resendSent` keys in all 4 locales (en, fr, es, de); updated `description` to mention requesting a new link
### Changed
- **Coverage — users backend:** 296 → 316 tests (+20: `RegistrationServiceTests` +6, `RegistrationControllerTests` +4, `UserRepositoryTests` +7, `ResendVerificationRequestValidatorTests` +3)

## [0.91.1] - 2026-05-09
### Changed
- **SonarQube quality fixes — expenses backend:**
  - Extracted `IExpenseRequest` interface (shared by `CreateExpenseRequest` + `UpdateExpenseRequest`); `ExpenseRequestValidatorBase<T>` holds all shared validation rules — both concrete validators are now one-liners eliminating code duplication
  - Added `required` modifier to value-type properties (`Amount`, `CurrencyId`, `Date`) on both request DTOs to prevent under-posting
  - `BuildExpense` helper in `ExpenseRepositoryTests` marked `static` (no instance data access)
  - **13 new tests** in `ExpenseRequestValidatorTests` covering all validation rules for both `CreateExpenseRequestValidator` and `UpdateExpenseRequestValidator`
- **Coverage — expenses backend:** 177 → 190 tests

## [0.91.0] - 2026-05-09
### Added
- **Phase 3 — Core Expense CRUD (expenses service):**
  - `Expense` model: added `IsDeleted` (bool, default false) + `DeletedAt` (DateTime?) soft-delete fields; migration `AddExpenseSoftDelete` applied
  - `ExpenseRepository` (`IExpenseRepository`): `AddAsync`, `UpdateAsync`, `SoftDeleteAsync`, `GetByIdAsync` (ownership + !IsDeleted filter), `GetPagedAsync` (filter by date range, category, currency, amount range, description substring; paginated, descending by date)
  - `ExpenseAuditService` (`IExpenseAuditService`): writes `ExpenseAuditLog` + `ExpenseAuditSnapshot(s)` for add (1 `after`), update (`before` + `after`), delete (1 `before`)
  - `ExpenseService` (`IExpenseService`): `AddAsync`, `UpdateAsync`, `DeleteAsync`, `GetByIdAsync`, `GetPagedAsync`; maps to `ExpenseDto`; calls audit service on every mutation
  - `ExpenseController`: `POST /expenses`, `PUT /expenses/{id}`, `DELETE /expenses/{id}`, `GET /expenses/{id}`, `GET /expenses` (paged + filtered); reads `userId` from `auth_token` JWT cookie (nginx already validated; controller only decodes)
  - `JwtCookieReader`: static helper that base64url-decodes JWT payload and extracts `sub` claim — no extra NuGet dependency
  - `CreateExpenseRequest` / `UpdateExpenseRequest` DTOs with `CreateExpenseRequestValidator` / `UpdateExpenseRequestValidator` (FluentValidation; validate amount > 0, date not in future, description ≤ 500 chars, subcategory requires category)
  - `ExpenseDto` — nested `CurrencyDto? Currency`, `SubcategoryDto? Category`, `SubcategoryDto? Subcategory` (not flat IDs/names); `SubcategoryDto` reused for both category and subcategory slots since it has no children collection
  - `ExpenseFilterDto`, `ExpensePagedResponse` DTOs
  - **37 new tests**: `ExpenseServiceTests` (16), `ExpenseAuditServiceTests` (3), `ExpenseRepositoryTests` (8), `ExpenseControllerTests` (10)
### Changed
- **Coverage — expenses backend:** 140 → 177 tests

## [0.90.0] - 2026-05-09
### Added
- **Tests — expenses `UserEventConsumer`:** 24 new tests covering constructor, `ExecuteAsync` (happy path, cancellation, `BrokerUnreachableException` retry), `Dispose`, `OnMessageReceivedAsync` (null message, duplicate inbox deduplication, `Created`/`Updated`/`Deleted`/unknown event types, exception → nack, null/missing `MessageId` → generated GUID), and `HandleMessageAsync` (all 4 branches); `UserEventMessage` + `UserEventType` constants also covered
- **Tests — users `UserEventPublisher`:** 15 new tests covering `Publish` (serialisation, unique `MessageId` per call, channel created + disposed per call, properties set) and `PublishRaw` (exchange declaration, UTF-8 encoding, `MessageId` from argument, all 3 event types)
- **Tests — users `OutboxPublisherService`:** 9 new tests covering constructor, `ExecuteAsync` (cancellation), and `ProcessPendingAsync` via reflection (no events, single event published + marked, multiple events, publish failure → `MarkFailedAsync`, mixed success/failure, max-retries=5 verified, exception propagation)
### Changed
- **Coverage — users backend:** sequence 88.02% → 88.61%, branch 73.64% → 75.33%; 272 → 296 tests
- **Coverage — expenses backend:** sequence 83.94% → 84.88%, branch 26.04% → 50.00%; 116 → 140 tests

## [0.89.0] - 2026-05-09
### Changed
- **Expenses service — soft-delete for `Category` and `Family`:** replaced `IsArchived` (bool) with `IsDeleted` (bool, default false) + `DeletedAt` (DateTime?) on both models; `CategoryRepository.GetAllActiveAsync` filters `!c.IsDeleted`; `CategoryService.GetAllAsync` filters subcategories by `!s.IsDeleted`
- **Migration `ReplaceCategoryFamilyIsArchivedWithSoftDelete`:** adds `IsDeleted`/`DeletedAt` to `Categories` and `Families`, data-migrates `IsArchived → IsDeleted` (with `DeletedAt = CURRENT_TIMESTAMP` for previously archived rows), then drops `IsArchived`; uses `CURRENT_TIMESTAMP` (works on both PostgreSQL and SQLite test runner)
- **`implementation-plan.md` Phase 3:** `DeleteAsync` updated from "hard delete" to soft-delete (`IsDeleted = true`, `DeletedAt = UtcNow`) with `!IsDeleted` filter on all queries
- **Tests:** all `IsArchived` references replaced with `IsDeleted` across `CategoryRepositoryTests`, `CategoryServiceTests`, `ExpensesDbContextSchemaTests`

## [0.88.0] - 2026-05-09
### Changed
- **Users service — soft-delete for `User`:** `DeleteUserAsync` now sets `IsDeleted = true` + `DeletedAt = UtcNow` instead of `EF Core .Remove()`; all `UserRepository` queries (`GetUserByEmailAsync`, `GetUserByIdAsync`, `GetUsedEmailValidationHashesAsync`, `ValidateEmail`, `ValidateEmailAsync`) filter `!IsDeleted` — deleted users are invisible to the app layer
- **`User` model:** added `IsDeleted` (bool, default false) and `DeletedAt` (DateTime?) properties; mapped to `USR_IsDeleted` / `USR_DeletedAt` in `UsersAppDbContext`
- **Migration `AddUserSoftDelete`:** adds `USR_IsDeleted` + `USR_DeletedAt` columns; creates partial unique index `ux_usr_email_active` on `USR_Email WHERE USR_IsDeleted = FALSE` — enforces email uniqueness among active users only, allowing re-registration after deletion
- **Re-registration after deletion:** `RegisterNewUserAsync` uses `GetUserByEmailAsync` which filters soft-deleted rows — same email can be reused after a user is deleted without DB constraint violations
- **Tests — `UserRepositoryTests`:** replaced 3 hard-delete assertions with soft-delete equivalents; added 2 new tests (`HidesUserFromGetByEmail_AfterSoftDelete`, `HidesUserFromGetById_AfterSoftDelete`)
- **Tests — `RegistrationServiceTests`:** added `RegisterNewUserAsync_AllowsRegistration_WhenEmailBelongsToSoftDeletedUser`

## [0.87.0] - 2026-05-08
### Added
- **Nexus Repository Manager** (`infrastructure/configs/nexus/`): custom Docker image built on `sonatype/nexus3:latest` with `curl`+`jq`+`bash` installed; `provision.sh` runs at container startup via `docker-entrypoint.sh` wrapper — polls REST API until ready, changes admin password, creates all proxy/group repos from `repos.json`, creates CI user; idempotent via `/nexus-data/.provisioned` flag file
- **`infrastructure/configs/nexus/repos.json`**: externalised repo definitions (docker-hub-proxy, mcr-proxy, ghcr-proxy, docker-group:8082, npm-proxy/group, nuget-proxy/group) — add/remove proxies without touching scripts
- **`infrastructure/docker-compose-tools.yml`**: `nexus` service at `172.50.0.50`, ports 8081+8082, `build: ./configs/nexus`, `nexus-data` volume, `NEXUS_ADMIN_PASSWORD`/`NEXUS_CI_USER`/`NEXUS_CI_PASSWORD` env vars
### Changed
- **`infrastructure/configs/gitlab-runner/config.toml`**: added `nexus:172.50.0.50` to `extra_hosts`
- **`infrastructure/configs/gitlab-ci-templates/ci-init.yml`**: added `NEXUS_HOST`, `NEXUS_DOCKER_REGISTRY`, `NEXUS_NPM_REGISTRY`, `NEXUS_NUGET_SOURCE` variables
- **`infrastructure/configs/gitlab-ci-templates/ci-build.yml`**: `.dotnet-build` adds Nexus NuGet source before `dotnet restore`; `.node-build` sets npm registry to `$NEXUS_NPM_REGISTRY` before `npm ci`
- **`infrastructure/configs/gitlab-ci-templates/ci-docker.yml`**: DinD gets `--insecure-registry=nexus:8082` and `--registry-mirror=http://nexus:8082`; Nexus login added before `docker build`; `--build-arg REGISTRY=$NEXUS_DOCKER_REGISTRY` passed to `docker build`
- **`backend/users/Dockerfile`** and **`backend/expenses/Dockerfile`**: added `ARG REGISTRY=mcr.microsoft.com`; both `FROM` lines use `${REGISTRY}/...` — local builds use MCR default, CI passes `nexus:8082`
- **`infrastructure/.env.example`**: added `NEXUS_ADMIN_PASSWORD`, `NEXUS_CI_USER`, `NEXUS_CI_PASSWORD` keys

## [0.86.0] - 2026-05-07
### Changed
- **expenses `appsettings.json`:** Removed `RabbitMQ` section entirely — Docker uses env vars, local dev uses `appsettings.Development.json`; no env-specific values belong in the base config
- **expenses `appsettings.Development.json`:** Added missing `VirtualHost: expense_management` to `RabbitMQ` section

## [0.85.0] - 2026-05-07
### Changed
- **Config priority (both services, all options):** All `Configure<T>` blocks now use `appsettings.json` first, env var as fallback, hardcoded default last — consistent across `PostgresOptions`, `RabbitMQOptions`, `JwtAuthOptions`, `EmailOptions`, `AuthenticationServiceOptions`, `CryptographyOptions`; pattern: `builder.Configuration.GetValue("Key", Environment.GetEnvironmentVariable("ENV_VAR")) ?? "default"`
- **Reverted expenses RabbitMQ hostname priority** from env-var-first (introduced in v0.84.0) back to appsettings-first to match all other options

## [0.84.0] - 2026-05-07
### Fixed
- **RabbitMQ VirtualHost (both services):** `RabbitMQOptions` and `ConnectionFactory` now set `VirtualHost`; defaults to `expense_management` (where `expense_expenses` and `expense_users` have permissions); previously defaulted to `/` → auth failure after TCP connect
- **RabbitMQ hostname priority (expenses service):** `Program.cs` options binding now checks env var first, then `appsettings.json`; previously `GetValue(key, envVarDefault)` would return the `appsettings.json` value (`localhost`) before ever checking the env var — caused "Connection refused" inside Docker where `localhost` = the container itself
- **RabbitMQ startup resilience (expenses service):** `UserEventConsumer.ExecuteAsync` now retries on `BrokerUnreachableException` with 5 s delay instead of crashing the host

## [0.83.0] - 2026-05-07
### Changed
- **Infrastructure:** Both `backend/users/Dockerfile` and `backend/expenses/Dockerfile` final runtime stage switched from `mcr.microsoft.com/dotnet/aspnet:8.0-noble-chiseled` to `mcr.microsoft.com/dotnet/aspnet:8.0-alpine`

## [0.82.0] - 2026-05-07
### Fixed
- **Outbox `MSG_Id` type (users service):** `OutboxEvent.Id` stays `long`; `UsersAppDbContext.OnModelCreating` now applies `UseIdentityAlwaysColumn()` only when Npgsql is the provider and `ValueGeneratedOnAdd()` otherwise — SQLite `EnsureCreated()` correctly emits `INTEGER PRIMARY KEY` (auto-increment) without the Npgsql annotation forcing `BIGINT`
- **Deleted `FixOutboxEventsIdType` migration** (was generated as a workaround when `Id` was temporarily changed to `int`); reverted `AddOutboxEvents.cs` and `UsersAppDbContextModelSnapshot.cs` to `bigint` + `IdentityAlwaysColumn`
- **`IOutboxRepository` / `OutboxRepository`:** `MarkPublishedAsync(long)` and `MarkFailedAsync(long, string)` reverted to `long` id params
- **Sonar alerts fixed (`UserEventConsumer`):** `JsonSerializerOptions` now static readonly (no per-call allocation); `GC.SuppressFinalize(this)` added to `Dispose()`
### Added
- **Tests — users service:** `OutboxRepositoryTests` (15 tests: `EnqueueAsync`×2, `GetPendingAsync`×3, `MarkPublishedAsync`×2, `MarkFailedAsync`×4, `RequeueAsync`×4) using new `TestDbContextEnsureCreated` helper; `MessagingControllerTests` (6 tests: `Replay`×4, `Stats`×2)
- **Tests — expenses service:** `InboxRepositoryTests` (7 tests: `ExistsAsync`×3, `AddAsync`×4) using `TestExpensesDbContextWrapper`
- **`TestDbContextEnsureCreated`** (`TestHelpers/`) — SQLite in-memory wrapper using `EnsureCreated()` instead of `Migrate()` so `long` PKs map to `INTEGER PRIMARY KEY` correctly; used by `OutboxRepositoryTests`

## [0.81.0] - 2026-05-07
### Added
- **RabbitMQ reliability — outbox pattern (users service):**
  - `OutboxEvent` model (`MSG_OutboxEvents` table: `MSG_Id` bigint identity PK, `MSG_MessageId` varchar(36) unique, `MSG_EventType`, `MSG_Payload`, `MSG_CreatedAt`, `MSG_PublishedAt` nullable, `MSG_RetryCount` default 0, `MSG_LastError` varchar(2000))
  - `IOutboxRepository` / `OutboxRepository`: `EnqueueAsync(OutboxEvent)` (adds row + saves), `GetPendingAsync(maxRetries)`, `MarkPublishedAsync`, `MarkFailedAsync` (increments retry + truncates error to 2000 chars), `RequeueAsync` (resets `PublishedAt`/`RetryCount`/`LastError` for replay)
  - `OutboxPublisherService` (`BackgroundService`): polls every 5 s; fetches unpublished events with `RetryCount < 5`; calls `IUserEventPublisher.PublishRaw()` then `MarkPublishedAsync`; on failure calls `MarkFailedAsync`
  - `IUserEventPublisher` extended with `PublishRaw(eventType, jsonPayload, messageId)`; `UserEventPublisher.Publish()` delegates to `PublishRaw` with new GUID
  - `MessagingController`: `POST /messaging/replay?eventType=&from=&forceAll=` → calls `RequeueAsync`, returns count; `GET /messaging/outbox/stats` → pending/published/failed counts
  - `RegistrationService` refactored: injects `IOutboxRepository`; `ValidateEmailAsync` calls `IUserRepository.ValidateEmailAsync` (validates + saves user), then independently calls `IOutboxRepository.EnqueueAsync` with the built `OutboxEvent` — no shared-transaction coupling
  - `IUserRepository.ValidateEmailAsync(hash, email) → User?` added (validates, saves, returns user); replaces old `ValidateEmailAndEnqueueAsync` factory-lambda approach
  - Migration `20260506224929_AddOutboxEvents`: creates `MSG_OutboxEvents` with unique index on `MSG_MessageId` and composite index on `(MSG_PublishedAt, MSG_RetryCount)`
  - `Program.cs`: registered `IOutboxRepository` (scoped), `OutboxPublisherService` (hosted)
- **RabbitMQ reliability — inbox pattern (expenses service):**
  - `InboxEvent` model (`InboxEvents` table: `MessageId` varchar(36) PK, `EventType` varchar(100), `ReceivedAt`, `Status` varchar(20), `Error` varchar(2000) nullable); `InboxEventStatus` constants (`Processed`, `Failed`)
  - `IInboxRepository` / `InboxRepository`: `ExistsAsync(messageId)`, `AddAsync(InboxEvent)`
  - `UserEventConsumer` updated with inbox deduplication: extracts `MessageId` from `ea.BasicProperties.MessageId` (fallback: new GUID); checks `ExistsAsync` before processing — duplicate → `BasicAck`, skip; on success writes `InboxEvent { Status=Processed }` then `BasicAck`; on failure `BasicNack` without inbox write (allows RabbitMQ redelivery)
  - Migration `20260506224942_AddInboxEvents`: creates `InboxEvents` table with index on `ReceivedAt`
  - `Program.cs`: registered `IInboxRepository` (scoped)
- **Tests:** `RegistrationServiceTests` updated — `CreateService` accepts `Mock<IOutboxRepository>`; `ValidateEmailAsync_ReturnsTrue_WhenValid` verifies `ValidateEmailAsync` + `EnqueueAsync` both called; `ValidateEmailAsync_ReturnsFalse_WhenUserNotFound` verifies `EnqueueAsync` not called when user missing; 243/243 passing

## [0.80.0] - 2026-05-07
### Added
- **RabbitMQ user sync messaging (users → expenses):**
  - **Users service:** added `RabbitMQ.Client` package; `IRabbitMQService` / `RabbitMQService` (singleton, double-checked-lock connection); `RabbitMQOptions` (`EXPENSES_MANAGEMENT_USERS_RABBITMQ_*` env vars); `IUserEventPublisher` / `UserEventPublisher` — publishes JSON messages to `users.events` topic exchange with routing key matching `UserEventType` (`user.created`, `user.updated`, `user.deleted`)
  - **Users service — trigger:** `RegistrationService.ValidateEmailAsync` publishes `user.created` after successful email validation with full user payload (Id, FirstName, LastName, Email, FamilyId)
  - **Expenses service:** `UserEventConsumer` (`BackgroundService`) declares `expenses.users.sync` queue bound to `users.events` with `user.#` routing key; routes `user.created`/`user.updated` → `IUserRepository.SaveOrUpdateUserAsync`, `user.deleted` → `IUserRepository.DeleteUserAsync`; nacks without requeue on deserialization/processing errors; `RabbitMQService` updated to set `DispatchConsumersAsync = true` for async consumer support
  - **Tests:** `RegistrationServiceTests` updated — `CreateService` factory accepts `Mock<IUserEventPublisher>`; `ValidateEmailAsync_ReturnsTrue_WhenValid` now verifies publisher called with `user.created` event

## [0.79.0] - 2026-05-06
### Added
- **Backend — Expenses service: reference data seeded via migrations:**
  - `20260506203552_SeedCurrencies`: inserts 154 ISO 4217 active currencies (AED→ZWG) into `Currencies` table with `Code`, `Name`, `Decimals`, `Symbol`; `Down` deletes by explicit ID range
  - `20260506204543_SeedCategories`: inserts 17 top-level categories (IDs 1–17) + 108 subcategories (IDs 18–125) into `Categories` table; parents inserted first, subcategories in a second pass per group to satisfy self-referential FK
  - Test suite kept green: `SeedCurrencyAsync` and `SeedCategoryAsync` helpers moved to IDs 1000+ / 2000+ to avoid PK conflicts with seed rows; `CurrencyRepositoryTests` and `CategoryRepositoryTests` rewritten to assert on named items rather than `Single`/`Empty` counts

## [0.78.1] - 2026-05-06
### Fixed
- **Frontend — `ExpensesDataContext` infinite reload loop on app start:**
  - Root cause: `ExpensesDataProvider` fired `getCategories()` + `getCurrencies()` immediately on mount regardless of auth state; unauthenticated requests returned 401 → `api.service.ts` attempted token refresh → refresh failed → `unauthorizedHandler` called `location.assign('/login')` → page reloaded → infinite loop
  - Fix: `ExpensesDataProvider` now consumes `useAuth()` and gates all fetching on `isAuthenticated`; effect re-runs when auth state changes (fetch on login, clear data on logout)
  - `isLoading` default changed from `true` to `false` — no loading state until auth confirmed
  - `ExpensesDataContext.test.tsx`: mocks `@/features/auth/AuthContext` (`useAuth` returns `{ isAuthenticated: true }`); added 2 new tests: "does not fetch when not authenticated" and "clears data when unauthenticated"; total: 12/12 passing

## [0.78.0] - 2026-05-06
### Changed
- **Backend — Expenses service: repository layer added (structural parity with users service):**
  - `ICategoryRepository` / `CategoryRepository`: `GetAllActiveAsync()` — filters top-level non-archived categories with `Include(Children)` + `AsNoTracking()`; DbContext access now owned by repo
  - `ICurrencyRepository` / `CurrencyRepository`: `GetAllAsync()` — all currencies with `AsNoTracking()`
  - `CategoryService` and `CurrencyService`: inject `ICategoryRepository` / `ICurrencyRepository` respectively; no longer depend on `ExpensesDbContext` directly; project domain models → DTOs in memory
  - `Program.cs`: repos registered in `#region Repositories`
  - `CategoryServiceTests` and `CurrencyServiceTests`: rewritten to use `Mock<IXRepository>` with static `CreateService()` factory (no more in-memory DB in service tests)
  - `CategoryRepositoryTests` (5 tests) and `CurrencyRepositoryTests` (4 tests) added using `TestExpensesDbContextWrapper`; total: 109/109 passing

## [0.77.0] - 2026-05-06
### Added
- **Backend — Phase 2: Categories & Currencies read endpoints (expenses service):**
  - `ICategoryService` / `CategoryService`: `GetAllAsync()` returns active top-level categories with their active children as a tree (archived entries excluded at both levels); loads via `Include(Children)` then projects in memory
  - `ICurrencyService` / `CurrencyService`: `GetAllAsync()` returns all currencies
  - `CategoryController`: `GET /categories` → `IEnumerable<CategoryDto>` (tree)
  - `CurrencyController`: `GET /currencies` → `IEnumerable<CurrencyDto>`
  - `Controllers/DTO/` folder: `CategoryDto` (with `IEnumerable<SubcategoryDto>`), `SubcategoryDto`, `CurrencyDto` — matches users service DTO pattern
  - Both services registered as scoped in `Program.cs`
  - `CategoryServiceTests` (7 tests) and `CurrencyServiceTests` (4 tests) using in-memory DB; total: 88/88 passing
- **Frontend — Phase 2: expenses data layer:**
  - `src/features/expenses/types/expenses.type.ts` — `Category`, `Subcategory`, `Currency` types
  - `src/features/expenses/services/categoriesApi.service.ts` — `getCategories()` → `GET /api/expenses/categories`
  - `src/features/expenses/services/currenciesApi.service.ts` — `getCurrencies()` → `GET /api/expenses/currencies`
  - `src/features/expenses/ExpensesDataContext.tsx` — `ExpensesDataProvider` / `useExpensesData()` — fetches both on mount, exposes `{ categories, currencies, isLoading, refresh }`
  - `App.tsx` — `ExpensesDataProvider` nested inside `AuthProvider`

## [0.76.3] - 2026-05-05
### Changed
- **Backend — Expenses service: `int` → `long` PKs and FKs for high-volume tables:**
  - `Expense.Id`, `ExpenseAuditLog.Id`, `ExpenseAuditSnapshot.Id`, `ExpenseFamilyAttribution.Id` changed to `long`
  - FK columns updated to match: `ExpenseAuditLog.ExpenseId`, `ExpenseAuditSnapshot.AuditLogId`, `ExpenseTag.ExpenseId`, `ExpenseFamilyAttribution.ExpenseId`
  - EF Core migration `LongIdsForExpenseAndAudit` generated (2026-05-05)
  - Tests updated: `SeedAuditLogAsync` takes `long expenseId`; `FindAsync` calls use `1L` literals for affected tables

## [0.76.2] - 2026-05-05
### Changed
- **Backend — Expenses service: enums → lookup tables with memory cache:**
  - Deleted 8 C# enums from `Models/Enums/`; replaced with 8 DB-backed lookup entity classes in `Models/Lookups/` (each implements `ILookupEntity`): `OperationSource`, `ModifiedSource`, `FamilyRole`, `RateSource`, `ConflictStatus`, `ConflictResolution`, `AuditOperation`, `SnapshotType`
  - All FK columns in domain models now use `int` FK + nav property instead of string-converted enum (e.g. `Expense.CreatedFromId`, `FamilyMembership.RoleId`, `CurrencyDailyRate.RateSourceId`, `CurrencyRateConflict.StatusId`/`ResolutionId`, `ExpenseAuditLog.OperationId`/`PerformedFromId`, `ExpenseAuditSnapshot.SnapshotTypeId`)
  - `ExpensesDbContext`: 8 new lookup `DbSet<T>` properties; each lookup table has `HasData` seed (e.g. OperationSource: 1=SingleWeb, 2=SingleMobile, 3=BulkWeb)
  - Added `ILookupCacheService` / `LookupCacheService`: generic `GetIdAsync<T>(name)` / `GetNameAsync<T>(id)` backed by `IMemoryCache` with `CacheItemPriority.NeverRemove`; registered as scoped in `Program.cs`; `AddMemoryCache()` added
  - `SchemaFoundation` migration regenerated from InitialCreate baseline to create all tables with correct FK int columns
  - `ExpensesDbContextSchemaTests` rewritten (enum references → int FK IDs); `LookupCacheServiceTests` added (7 tests: GetId, GetName, KeyNotFoundException, cache hit, all 8 lookup types)
  - Total tests: 77/77 passing

## [0.76.1] - 2026-05-05
### Added
- **Backend — Expenses service schema tests:** `ExpensesDbContextSchemaTests` (23 tests) covering all Phase 1 entities — Category hierarchy, Expense with all FK variants, Family/Membership/Attribution, Tag, ExpenseTag composite PK, CurrencyDailyRate unique constraint, CurrencyPairDefault composite PK, CurrencyRateConflict nullable fields, ExpenseAuditLog all operations, ExpenseAuditSnapshot before/after, cascade deletes. Total test count: 70/70 passing.

## [0.76.0] - 2026-05-05
### Added
- **Backend — Phase 1 Schema Foundation (expenses service):**
  - 8 new enums in `Models/Enums/`: `CreatedFromSource`, `ModifiedFromSource`, `FamilyMemberRole`, `CurrencyRateSource`, `CurrencyRateConflictStatus`, `CurrencyRateConflictResolution`, `ExpenseAuditOperation`, `ExpenseAuditSnapshotType`
  - `Category` updated: added `IsArchived`, `ParentCategoryId` (explicit FK), `Children` collection
  - `Expense` rewritten: `UserId`, `CurrencyId`, `CategoryId`, `SubcategoryId`, `Date` (DateOnly), `CreatedAt/By/From`, `ModifiedAt/By/From`; removed `IsHidden`/`CreatedDate`
  - New models: `Family`, `FamilyMembership`, `ExpenseFamilyAttribution`, `Tag`, `ExpenseTag`, `CurrencyDailyRate`, `CurrencyPairDefault`, `CurrencyRateConflict`, `ExpenseAuditLog`, `ExpenseAuditSnapshot`
  - `ExpensesDbContext` updated: all 13 application DbSets registered with full Fluent API (precision, max-lengths, enum-to-string, FKs, indexes)
  - EF Core migration `SchemaFoundation` generated and applied; all 47 tests pass

## [0.75.1] - 2026-05-05
### Changed
- **Backend — Expenses service structural parity with users service:**
  - `Expense.Amount`: `double` → `decimal` (financial precision; `double` loses precision for money)
  - `RabbitMQService`: connection now cached with double-checked lock; no longer creates new `IConnection` per `GetConnection()` call
  - `Program.cs`: added FluentValidation auto-validation + `InvalidModelStateResponseFactory` (returns `401 UnauthorizedObjectResult(ErrorResponse)` on model errors, matching users service wire format); added health checks (`/health`); replaced bare `AddSwaggerGen()` with named info (title, version, contact, XML docs); added `app.UsePathBase("/api/expenses")` in Development; added `ENABLE_SWAGGER` env-var gate; removed redundant `AddJsonFile` call; added `WhenWritingNull` JSON option
  - `.csproj`: added `FluentValidation.AspNetCore 11.3.0`, `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore 8.0.0`; bumped `Swashbuckle.AspNetCore` from `6.4.0` → `8.0.0`; added `GenerateDocumentationFile` + `NoWarn` (CS8618/CS8603/CS8604/CS1591) to match users service
  - `Controllers/Responses/ErrorResponse.cs`: created (identical pattern to users service)

## [0.75.0] - 2026-05-04
### Added
- **Docs:** `docs/plans/application-description.md` — full product specification (roles, Default family semantics, multi-family attribution, categories, delete permissions, two-table audit design, currency rate resolution, all screens).
- **Docs:** `docs/plans/implementation-plan.md` — 15-phase implementation plan covering DB schema, categories, CRUD, families, tags, currency rates, dashboard API, frontend screens, admin, CSV, notifications, PWA.

## [0.74.0] - 2026-05-04
### Changed
- **Backend — EO → DTO rename (users service):** Renamed `Controllers/EO/` → `Controllers/DTO/`, all `*Eo` classes → `*Dto` throughout the users service.
  - `ApplicationEo` → `ApplicationDto`, `RoleEo` → `RoleDto`, `UserEo` → `UserDto`.
  - Namespace `Controllers.EO` → `Controllers.DTO` across all source and test files.
  - Old `EO/` folder deleted; `DTO/` folder added with `ApplicationDto.cs`, `RoleDto.cs`, `UserDto.cs`.
- **Backend — Docker security (users + expenses):** Hardened `.dockerignore` for both services.
  - Added `**/appsettings.Development.json` and `**/coverage` exclusions.
  - Removed erroneous `!**/.gitignore`, `!.git/HEAD`, `!.git/config`, `!.git/packed-refs`, `!.git/refs/heads/**` re-inclusions.

## [0.73.0] - 2026-05-04
### Changed
- **Backend — Validation parity (VAL-03, VAL-04):** Email format validation moved to the FluentValidation layer for both login and registration.
  - `LoginRequestValidator`: added `.EmailAddress().WithMessage("INVALID_EMAIL_FORMAT")` — login now rejects malformed emails at the validator instead of silently failing at DB lookup.
  - `RegisterRequestValidator`: added `.EmailAddress().WithMessage("INVALID_EMAIL_FORMAT")` — registration validator is now the single authoritative source for email format.
  - `RegistrationService.RegisterNewUserAsync`: removed redundant `MailAddress` constructor check (was strict RFC; diverged from Zod's lenient regex on the frontend).
  - Tests: 2 new validator tests; obsolete `RegisterNewUserAsync_ReturnsError_WhenEmailFormatIsInvalid` service test removed.

## [0.72.3] - 2026-05-03
### Changed
- **Frontend tests:** Expanded test coverage to close all remaining gaps.
  - `src/components/__tests__/LanguageSwitcher.test.tsx` (new): covers `resolvedLanguage` fallback to `i18n.language` when undefined.
  - `src/i18n/__tests__/i18n.test.ts` (new): verifies supported languages, fallback locale, resource bundles, and interpolation config.
  - `src/services/__tests__/api.service.env.test.ts` (new): isolated `vi.resetModules()` tests for `VITE_API_BASE` prefix and trailing-slash stripping.
  - `src/features/auth/__tests__/AuthContext.test.tsx`: added `createPassword` describe block (success, missing-fields guard, API error).
  - `src/services/api.service.ts`: added `/* c8 ignore next */` on module-level `API_BASE` constant (V8 can't merge coverage from dynamically imported instances).
  - `vitest.config.ts`: excluded `src/i18n/index.ts` and `src/i18n/locales/**` from coverage (side-effect init; locale JSON not executable).
  - Result: 330 tests across 24 files; 100% statements/lines, 99.22% branches, 99.23% functions.

## [0.72.2] - 2026-05-02
### Changed
- **Frontend:** Resolved Sonar alert on `Toast.tsx` (no behaviour change).
  - `src/components/Toast.tsx`: toast ID generation `Date.now() + Math.random()` → `crypto.randomUUID()`; `Toast.id` type updated `number` → `string`.

## [0.72.1] - 2026-05-02
### Changed
- **Frontend:** Resolved three Sonar static-analysis alerts (no behaviour change).
  - `src/i18n/index.ts`: `export default i18n` → `export { i18n as default }` (re-export form).
  - `src/features/auth/AuthContext.tsx`: `window.location.assign` → `globalThis.location.assign`.
  - `src/constants/apiErrors.constant.ts`: added `// NOSONAR` on four i18n key constants falsely flagged as hardcoded credentials (S2068).

## [0.72.0] - 2026-05-02
### Added
- **Frontend:** Internationalisation (react-i18next) with English, French, Spanish, and German support.
  - `react-i18next`, `i18next`, `i18next-browser-languagedetector` installed.
  - `src/i18n/index.ts`: singleton config; language detection via localStorage → browser navigator; resources inlined (no HTTP backend).
  - Translation JSON files: `src/i18n/locales/{en,fr,es,de}/translation.json` — full coverage of all UI strings.
  - `src/components/LanguageSwitcher.tsx`: select dropdown wired to `i18n.changeLanguage`.
  - All auth schemas (`auth.schemas.ts`) converted to factory functions `makeXxxSchema(t: TFunction)` for translated validation messages.
  - All pages and components updated to use `useTranslation` + `t()`: `LoginPage`, `RegisterPage`, `ChangePasswordPage`, `RequestPasswordResetPage`, `ResetPasswordPage`, `HomeDashboardPage`, `SettingsPage`, `HomePublicPage`, `NotFoundPage`, `VerifyErrorPage`, `NavBar`, `EmailField`, `PasswordStrength`.
  - `src/constants/apiErrors.constant.ts`: getters + Proxy pattern for dynamic i18next lookups.

## [0.71.0] - 2026-05-01
### Fixed
- **Backend + Frontend:** Added max-length (100) validation on `firstName`, `lastName`, and `email` for the registration flow (F-4 / VAL-02).
  - `RegisterRequestValidator.cs`: `.MaximumLength(100).WithMessage("FIELD_TOO_LONG")` on all three fields; 3 new validator tests.
  - `auth.schemas.ts`: `.max(100, ...)` added to `firstName`, `lastName`, and `email` in `registerSchema`.
  - `RegisterPage.tsx`: `maxLength={100}` on all three inputs.
  - `RegisterPage.test.tsx`: 3 new tests for max-length error messages.

## [0.70.0] - 2026-05-01
### Changed
- **Frontend:** `ResetPasswordPage` now routes `mode=create` submissions to `POST /auth/create-password` (via new `createPassword` context function) instead of `POST /auth/change-password-reset`.
  - `authApi.service.ts`: added `createPasswordRequest` targeting `POST /auth/create-password`.
  - `auth.type.ts`: added `createPassword` to `AuthContextValue`.
  - `AuthContext.tsx`: added `createPassword` callback wired to `createPasswordRequest`.
  - `ResetPasswordPage.tsx`: destructures `createPassword` from `useAuth()`; `onSubmit` calls `createPassword` when `isCreateMode`, `resetPassword` otherwise.
  - `ResetPasswordPage.test.tsx`: create-mode tests updated to mock and assert `createPassword` instead of `resetPassword`.

## [0.69.0] - 2026-05-01
### Refactored
- **Backend (users):** Separated password creation (initial setup) and password reset (via link) into two distinct service methods and endpoints.
  - `POST /auth/create-password` — initial password setup after email verification; validates via `EmailValidationHash`; can create or update auth record; returns `CREATE_PASSWORD_FAILED` on failure.
  - `POST /auth/change-password-reset` — resets password using a `PasswordResetHash` issued by `RequestPasswordResetAsync`; hash must match and be within 24 hours; clears reset fields on success.
  - `IPasswordManagementService`: added `CreatePasswordAsync(email, verificationHash, newPassword)`; `ResetPasswordAsync` signature unchanged but now only handles the reset-hash flow.
  - `AuthenticationRepository.UpdateAuthenticationAsync`: added `resetHash` bool parameter; null-user guard applies only when `resetHash: true`.
  - `RequestPasswordResetAsync`: reset link now uses dedicated `ResetPasswordBaseUrl` with `?email=…&h=…` format (previously reused `VerifyEmailBaseUrl` with `app_code`).
  - New `ResetPasswordBaseUrl` property in `AuthenticationServiceOptions`; configurable via `AuthenticationService:ResetPasswordBaseUrl` in `appsettings.json` or `EXPENSES_MANAGEMENT_USERS_AUTHSERVICE_RESET_PASSWORD_URL` env var.
  - `appsettings.Development.json`: seeded `ResetPasswordBaseUrl = "https://localhost/reset-password"`.
  - New `CreatePasswordRequest.cs` and `CreatePasswordRequestValidator.cs`.
  - Tests: 238 total (+9 net). New `CreatePasswordAsync` region (5 tests); `ResetPasswordAsync` region overhauled (6 tests, explicit hash-mismatch and auth-not-found cases added); 3 new controller tests for `CreatePassword`; 6 new validator tests in `CreatePasswordRequestValidatorTests`; old cross-flow tests removed.

## [0.68.0] - 2026-04-30
### Refactored
- **Backend (users) + Frontend:** Removed `ConfirmPassword` from `ChangePasswordRequest` and `ChangePasswordResetRequest` DTOs — password confirmation is a UI-only concern.
  - `ChangePasswordRequest.cs`, `ChangePasswordResetRequest.cs`: `ConfirmPassword` field dropped.
  - `ChangePasswordRequestValidator`, `ChangePasswordResetRequestValidator`: `.Equal(x => x.NewPassword)` rule removed (field no longer exists on DTO).
  - `PasswordControllerTests`: 2 remaining confirmation-match assertions removed.
  - `authApi.service.ts`: `changePasswordRequest` and `resetPasswordRequest` no longer send `confirmPassword` in the payload.
  - `AuthContext.tsx`: `changePassword` and `resetPassword` signatures updated to remove `repeatPassword` parameter; API calls updated accordingly.
  - `AuthContext.test.tsx`, `ChangePasswordPage.test.tsx`, `ResetPasswordPage.test.tsx`: all assertions updated to match new 2- and 3-arg signatures.
  - `repeatPassword` field remains in Zod schemas and form UI for client-side UX validation only.

## [0.67.0] - 2026-04-30
### Refactored
- **Backend (users):** Replaced all manual request-field `if`-checks in controllers with FluentValidation auto-validation. Added minimum 8-char password length enforcement (VAL-01).
  - New `Validators/` folder with five validators: `LoginRequestValidator`, `RegisterRequestValidator`, `ChangePasswordRequestValidator`, `ChangePasswordResetRequestValidator`, `RequestPasswordResetRequestValidator`.
  - `Program.cs`: added `AddFluentValidationAutoValidation()`, `AddValidatorsFromAssemblyContaining<Program>()`, and `InvalidModelStateResponseFactory` (returns 401 `ErrorResponse { Message = firstError }` to preserve existing wire format).
  - `AuthenticationController`, `RegistrationController`, `PasswordController`: all manual null/empty field checks removed; `MissingParameters`, `PasswordTooShort`, `MinPasswordLength` constants dropped.
  - `ChangePasswordRequestValidator` and `ChangePasswordResetRequestValidator`: `NewPassword` rule uses `CascadeMode.Stop` + `NotEmpty` + `MinimumLength(8)` with message `PASSWORD_TOO_SHORT`.
  - New `Tests/Validators/` folder with 26 tests across five validator test classes using `FluentValidation.TestHelper` (`TestValidate`, `ShouldHaveValidationErrorFor`, `ShouldNotHaveAnyValidationErrors`).
  - `PasswordControllerTests`: 17 validation-path tests deleted (now covered by validator tests); 9 business-logic tests retained using `"newPassword1"` (8-char) passwords.
  - `AuthenticationControllerTests`, `RegistrationControllerTests`: 4 validation tests each deleted (now covered by validator tests).

## [0.66.0] - 2026-04-29
### Fixed
- **Frontend/Backend — F-3:** Invalid/expired email verification links now redirect to a friendly error page instead of rendering raw JSON.
  - Added `VerifyEmailErrorUrlPath` property to `Application` model, `ApplicationEo`, and `UsersAppDbContext` column mapping (`APP_VerifyEmailErrorUrlPath`).
  - Migration `20260429200824_AddVerifyEmailErrorUrlPath` seeds `/verify-error` for the EXPENSES_MANAGER app.
  - `ApplicationService` maps the new field through to `ApplicationEo`.
  - `RegistrationController.ValidateEmail`: when token validation fails and `VerifyEmailErrorUrlPath` is configured, redirects to `{app.UrlPath}{app.VerifyEmailErrorUrlPath}`; falls back to JSON `EMAIL_VERIFICATION_FAILED` if path is not configured.
  - New `VerifyErrorPage.tsx` renders a friendly "Verification link expired" message with a "Back to register" CTA at `/verify-error` (public route, no auth guard).
  - New test `ValidateEmail_ReturnsRedirectToErrorPage_WhenValidationFails_AndErrorUrlConfigured`; existing failure test renamed to clarify the no-URL fallback path. All 15 controller tests pass.

## [0.65.0] - 2026-04-29
### Fixed
- **Frontend — F-2:** Eliminated duplicate error toasts on failed login.
  - `loginRequest()` in `authApi.service.ts` now passes `silent: true` alongside `skipUnauthorized: true`. The generic `errorHandler` toast is suppressed; `LoginPage` continues to show its own "Invalid credentials. Please try again." message.
  - `AuthContext.test.tsx`: two `toHaveBeenCalledWith` assertions updated from `SKIP` to `SKIP_SILENT` to match the new opts. All 267 tests pass.

## [0.64.0] - 2026-04-29
### Fixed
- **Frontend — F-1:** Suppressed duplicate "Authentication token is missing" toasts on every page load for unauthenticated visitors.
  - Added `silent?: boolean` option to `api.service.ts` `request()`/`get()`/`post()`. When `true`, `errorHandler` (toast) is not called even on error responses — the `ApiResponse` still carries the error for callers to handle.
  - Applied `silent: true` to `sessionCheck()` and `refreshRequest()` in `authApi.service.ts`. These are startup auth probes expected to fail for fresh visitors; suppressing their toasts is correct.
  - Two new tests added to `api.service.test.ts` covering `silent` flag behaviour. `AuthContext.test.tsx` assertion updated to match new opts.

## [0.63.0] - 2026-04-29
### Refactored
- **Backend (users) — LSP:** Fixed nullable contract mismatches in the users service.
  - `IUserRepository.CreateUserAsync`: return type changed from `Task<User?>` to `Task<User>` — creation either succeeds (returning the entity) or throws; returning null was never reachable and violated the contract.
  - `UserRepository.CreateUserAsync`: implementation signature updated to match.
  - `RegistrationService`: `user` variable now typed as non-nullable `User`; null-conditional operators (`?.`) removed; `DeleteUserAsync(user)` call is now clean.
  - `RoleService.GetUserRolesByApplicationCodeAsync`: now returns `Enumerable.Empty<RoleEo>()` instead of `null` when `applicationCode` is null or the repository returns null — honouring the non-nullable `IEnumerable<RoleEo>` contract declared in `IRoleService`.
  - `AuthenticationController`: simplified guard from `if (roles == null || !roles.Any())` to `if (!roles.Any())` — the null check was only needed because the service violated its own contract.
  - Tests updated: `RoleServiceTests` renamed two cases from `ReturnsNull` to `ReturnsEmpty` with updated assertions; `AuthenticationControllerTests` mock changed from a null return to an empty enumerable. Total test count unchanged: 217.

## [0.62.0] - 2026-04-29
### Refactored
- **Backend (users) — OCP:** Introduced `IEmailService` abstraction and `SmtpEmailService` implementation.
  - SMTP dispatch logic extracted from `EmailHelper` into `SmtpEmailService : IEmailService`.
  - `EmailHelper.SendEmail()` now delegates to `IEmailService` — swapping the email provider requires only a new `IEmailService` implementation and a single DI registration change, with no modification to `EmailHelper` or any service that calls it.
  - `Program.cs`: registers `SmtpEmailService` as `IEmailService` (scoped).
  - Tests: `EmailHelperTests` updated to mock `IEmailService`; SMTP failure coverage moved to new `SmtpEmailServiceTests`. Test count: 217 (was 208).

## [0.61.0] - 2026-04-29
### Fixed
- **CI (users):** SonarQube job was crashing with an OOM error in the JS/HTML bridge server while parsing email template files.
  - Added `**/Assets/**` to `sonar.exclusions` and `sonar.coverage.exclusions` in `backend/users/SonarQube.Analysis.xml`.
  - Email templates contain no analyzable source code; excluding them is correct and prevents the bridge from exhausting memory on large inline HTML/CSS.

### Changed
- **Backend (users) — Email templates:** Aligned `EMAIL_VERIFICATION_TEMPLATE.html` to match the style of `PASSWORD_RESET_TEMPLATE.html`.
  - Unified body background (`#eef2f7`), wrapper/card layout, banner padding (`24px 20px`), heading size (`22px`), body padding (`32px 28px`), paragraph colors/line-height, CTA button style, `hr.divider`, and footer (`#f8fafc` background).
  - Removed box-shadow and boilerplate Privacy Policy/Terms links from the verification template; corrected copyright to "Expenses Manager".

## [0.60.0] - 2026-04-28
### Refactored
- **Backend (users):** Applied SRP to the authentication layer — split the monolithic `AuthenticationService` and `AuthenticationController` into focused, single-responsibility units.
  - `AuthenticationService` / `IAuthenticationService`: now contains only `AuthenticateAsync` (credential verification).
  - `JwtTokenService` / `IJwtTokenService` (new): `GenerateJwtToken` + `ValidateToken`.
  - `RegistrationService` / `IRegistrationService` (new): `RegisterNewUserAsync` + `ValidateEmailAsync` (+ private hash/email helpers).
  - `PasswordManagementService` / `IPasswordManagementService` (new): `ChangePasswordAsync` + `ResetPasswordAsync` + `RequestPasswordResetAsync`.
  - `AuthenticationController`: retains login, logout, check, session, refresh; now injects `IJwtTokenService` for token operations.
  - `RegistrationController` (new): register + validate-email endpoints; injects `IRegistrationService`.
  - `PasswordController` (new): change-password, request-password-reset, change-password-reset endpoints; injects `IPasswordManagementService`.
  - `Program.cs`: registers the three new service types.
  - Test files split accordingly: `AuthenticationServiceTests`, `JwtTokenServiceTests`, `RegistrationServiceTests`, `PasswordManagementServiceTests`, `AuthenticationControllerTests`, `RegistrationControllerTests`, `PasswordControllerTests`. Total test count unchanged: 208.

## [0.59.0] - 2026-04-28
### Fixed
- **Backend (users) + Frontend:** Password reset email link was missing the `app_code` query parameter, causing a 400 validation error when clicking the link.
  - `RequestPasswordResetRequest`: added `AppCode` property.
  - `IAuthenticationService` / `AuthenticationService.RequestPasswordResetAsync`: added `appCode` parameter; reset link now appends `&app_code=...` (mirrors the existing email-verification link at line 196).
  - `AuthenticationController.RequestPasswordReset`: validates `AppCode` is non-empty; passes it to the service.
  - `authApi.service.ts` (`requestPasswordResetRequest`): added `applicationCode` parameter, sent as `appCode` in the request body.
  - `AuthContext.tsx`: passes `APPLICATION_CODE` to `requestPasswordResetRequest`.
  - All affected controller and service tests updated; total test count: 208 → 209.

## [0.58.0] - 2026-04-28
### Added
- **Backend (users) tests:** Added repository test coverage for `RefreshTokenRepository` (previously at 0%).
  - `RefreshTokenRepositoryTests` (11 tests): `GetActiveByTokenAsync` — returns token when active, null when not found, null when expired, null when revoked; `AddAsync` — persists token to DB; `RevokeAsync` — sets `RevokedAt`, makes `IsActive` false; `RevokeAllByUserIdAsync` — revokes all active tokens for user, leaves already-revoked tokens unchanged, skips expired tokens, does not affect other users.
  - Total test count: 196 → 207.

## [0.57.0] - 2026-04-28
### Added
- **Backend (users) tests:** Added unit test coverage for two previously untested services.
  - `RefreshTokenServiceTests` (11 tests): `GenerateAsync` — non-empty token, persists to repo, 1-day expiry when `rememberMe=false`, configured-days expiry when `rememberMe=true`, unique token per call; `ValidateAsync` — invalid when not found, invalid when inactive (expired/revoked), valid + userId when active; `RevokeAsync` — no-op when not found, revokes when found; `RevokeAllForUserAsync` — delegates to repo.
  - `UserRoleAssignmentServiceTests` (5 tests): `TryAssignDefaultRoleAsync` — no-op when applicationCode is null, when user is null, when application not found, when no default role exists; assigns role when both application and default role are present.
  - Total test count: 180 → 196.

## [0.56.0] - 2026-04-28
### Fixed
- **Backend (users):** Resolved remaining SonarQube quality-gate findings.
  - `AuthenticationService`: constructor parameter count reduced from 8 → 7 by extracting `IUserRoleAssignmentService` / `UserRoleAssignmentService` (wraps `IApplicationRepository` + `IRoleRepository` for role assignment during registration); `errors.Any()` → `errors.Count > 0` (S2971 — prefer Count comparison).
  - `PASSWORD_RESET_TEMPLATE.html`: `.foot` text color changed from `#9ca3af` to `#4b5563` to meet WCAG AA contrast ratio (~6.3:1 on `#f8fafc` background).
  - `AuthenticationServiceTests.CreateService()`: marked `static` (rule S2325); factory updated to accept `Mock<IUserRoleAssignmentService>` replacing the former `appRepo`/`roleRepo` params; `AssignsDefaultRole` test updated accordingly.

## [0.55.0] - 2026-04-28
### Fixed
- **Backend (users):** Resolved SonarQube quality-gate findings across multiple files.
  - `EmailHTMLTemplate` class renamed to `EmailHtmlTemplate` (pascal-case rule S101).
  - `AuthenticationController`: extracted `MissingParameters` and `ServerError` constants (rule S1192 — repeated literals); `ApplicationEo app` → `ApplicationEo? app` (nullable correctness).
  - `AuthenticationService`: `IList<string>` → `List<string>`, `ISet<string>` → `HashSet<string>` (performance rules); `errors.Count() > 0` → `errors.Any()` (rule S2971); `Application app` → `Application? app` (nullable correctness); constructor suppressed S107 via `// NOSONAR` (all 8 dependencies are genuinely required); `RegisterNewUserAsync` cognitive complexity reduced from 22 → ~8 by extracting `CreateAndRegisterUserAsync`, `GenerateUniqueEmailValidationHashAsync`, and `TryAssignDefaultRoleAsync` private helpers.
  - `UserRepository`: `user.Email?.ToLowerInvariant()` → `user.Email!.ToLowerInvariant()` (null-safe assignment); `u.IsEmailValidated == false` → `!u.IsEmailValidated` (rule S1125 — unnecessary boolean literal).
  - `TestDbContextWrapper.Dispose()`: added `GC.SuppressFinalize(this)` (rule S3881).
  - `AuthenticationServiceTests.CreateEmailHelperMock()`: marked `static` (rule S2325).
  - `EMAIL_VERIFICATION_TEMPLATE.html`: `.header` and `.button` colors changed from `#007bff` to `#0056b3` to meet WCAG AA contrast ratio (~7:1).

## [0.54.0] - 2026-04-28
### Added
- **Backend (users):** Implemented `RequestPasswordResetAsync` email sending — was previously a stub with commented-out code.
  - Added `EmailHTMLTemplate.PasswordReset` class (key: `PASSWORD_RESET_TEMPLATE`, variable: `RESET_LINK`).
  - Created `Assets/EmailTemplates/PASSWORD_RESET_TEMPLATE.html` email template, matching the style of the existing email verification template.
  - Registered the template in `.csproj` (`EmbeddedResource` + `CopyToOutputDirectory=Always`).
  - `POST /auth/request-password-reset` now sends a `[Expenses Manager] Password Reset` email to the user with a reset link.

## [0.53.2] - 2026-04-28
### Fixed
- **Backend (users):** Resolved two SonarQube quality-gate findings in `AuthenticationController` / `LoginRequest`.
  - `Session()` annotated with `[ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]`.
  - `LoginRequest.RememberMe` changed from `bool` to `bool?` (nullable value type on controller input); controller resolves it with `?? false` so runtime behaviour is unchanged.

## [0.53.1] - 2026-04-28
### Fixed
- **Frontend (dashboard):** Resolved two SonarQube quality-gate findings in `api.service.ts`.
  - `attemptTokenRefresh`: replaced `if (!refreshInFlight)` guard with nullish coalescing assignment (`??=`).
  - `request`: reduced cognitive complexity from 27 → ~11 by extracting `redirectToLogin()`, `buildErrorResponse()`, and `retryRequest()` helpers; logic and behaviour are unchanged.

## [0.53.0] - 2026-04-28
### Added
- **Frontend (dashboard):** `rememberMe` option wired end-to-end.
  - `LoginPage` passes `rememberMe` flag to `loginRequest`; `authApi.service.ts` forwards it to the backend so the backend can set persistent vs. session cookies.
  - `AuthContext` re-exported `firstName` / `lastName` from the session response for use in `NavBar`.
  - `PasswordStrength`: minor accessibility tweak.
- **Frontend (dashboard):** Full test suite for `api.service.ts` (`src/services/__tests__/api.service.test.ts`, 303 lines) covering: normal GET/POST/PUT/DELETE flows, transparent 401 → refresh → retry, concurrent refresh deduplication, `skipUnauthorized` bypass, network errors, and `onUnauthorized` / `onError` handler registration.

## [0.52.0] - 2026-04-28
### Added
- **Backend (users):** Refresh token flow — `POST /auth/refresh` issues a new `auth_token` cookie and rotates the `refresh_token` cookie using a DB-backed opaque token (`RTK_RefreshTokens` table).
  - `RefreshToken` model, `IRefreshTokenRepository` / `RefreshTokenRepository`, `IRefreshTokenService` / `RefreshTokenService`.
  - `JwtAuthOptions` extended with `RefreshExpiryInDays` (default 7).
  - Migration `AddRefreshTokens` adds `RTK_RefreshTokens` table.
  - `LoginRequest` gains `RememberMe` bool: `true` → persistent cookies with `Expires`; `false` → session cookies (cleared on browser close).
  - Both `auth_token` and `refresh_token` cookies set on login / rotated on refresh / deleted on logout.
  - `GET /auth/session` now returns `{ email, firstName, lastName }` extracted from JWT claims (no DB hit).
  - `GenerateJwtToken` extended with `GivenName` and `Surname` claims.
  - `LoginResponse.Token` removed — token is cookie-only; `IUserRepository.GetUserByIdAsync` added.
- **Frontend (dashboard):** Pure cookie-based auth state — no more `localStorage` / `sessionStorage` for user data.
  - `AuthContext.tsx`: on mount calls `GET /auth/session`; falls back to `POST /auth/refresh` then retries session check if access token is expired; no storage reads/writes anywhere.
  - `api.service.ts`: transparent refresh-and-retry on 401 for all non-`skipUnauthorized` requests (deduplicates concurrent refreshes).
  - `authApi.service.ts`: `sessionCheck` returns `User` data; new `refreshRequest`; `loginRequest` accepts `rememberMe`.
  - `auth.type.ts`: `User` type now includes `lastName`.

## [0.51.0] - 2026-04-27
### Refactor
- Frontend dashboard: resolved SonarQube quality-gate findings.
  - Extracted shared `EmailField` component (`features/auth/components/EmailField.tsx`) to eliminate duplicated email-field JSX block between `LoginPage` and `RequestPasswordResetPage`.
  - Marked props as read-only (`Readonly<>` / `readonly`) in `BackLink`, `FieldError`, `SubmitButton`, `AuthCard`, and `AuthPageHeader`.

## [0.50.0] - 2026-04-27
### Refactor
- Frontend dashboard: reorganized `src/` into a feature-first structure.
  - **Common** (unchanged paths): `components/`, `hooks/`, `layouts/`, `services/api.service.ts`, `types/api.type.ts`, `constants/`, `styles/`.
  - **`features/auth/`** expanded: `components/` (AuthCard, AuthPageHeader, ProtectedRoute, PublicOnlyRoute), `pages/` (LoginPage, RegisterPage, ChangePasswordPage, ResetPasswordPage, RequestPasswordResetPage + their tests), `services/authApi.service.ts`, `types/auth.type.ts`.
  - **`features/dashboard/pages/`** (new): HomeDashboardPage, SettingsPage + their tests.
  - **`features/public/pages/`** (new): HomePublicPage, NotFoundPage + their tests.
  - All imports and test paths updated; 239 tests remain green.

## [0.49.0] - 2026-04-27
### Refactor
- Frontend dashboard: extracted five shared components to eliminate SonarQube duplicate-code findings across the five auth pages.
  - `AuthCard` — wraps the `auth-page` / `auth-card` div structure.
  - `AuthPageHeader` — renders the `<h1>` + subtitle paragraph.
  - `SubmitButton` — submit button with built-in spinner SVG and configurable labels.
  - `FieldError` — per-field error `<p>` with `role="alert"` and scoped `id`.
  - `BackLink` — back-arrow `<Link>` with inline SVG chevron.
  - All five pages (`LoginPage`, `RegisterPage`, `RequestPasswordResetPage`, `ResetPasswordPage`, `ChangePasswordPage`) updated to use these components.

## [0.48.0] - 2026-04-26
### Changed
- Frontend dashboard: Phase 1 library adoption — React Hook Form + Zod across all five auth pages.
  - Installed `react-hook-form`, `zod`, and `@hookform/resolvers`.
  - Added `src/features/auth/auth.schemas.ts` with typed Zod schemas (`loginSchema`, `registerSchema`, `changePasswordSchema`, `resetPasswordSchema`, `requestPasswordResetSchema`) and exported inferred types.
  - Refactored `LoginPage`, `RegisterPage`, `ChangePasswordPage`, `ResetPasswordPage`, `RequestPasswordResetPage`: replaced per-field `useState` + manual `setSubmitting` + inline validation with `useForm<T>({ resolver: zodResolver(...) })`. `isSubmitting` from `formState` drives loading/disabled state automatically.
  - Per-field inline validation errors shown under each input (with `id` + `aria-describedby` linkage). Server-level errors continue via `setError('root', ...)` or local `serverMsg` state.
  - `PasswordInput` updated to use `forwardRef` so React Hook Form's `ref` callback reaches the underlying `<input>`.
  - Added `.field-error` CSS primitive in `index.css` for per-field error text.
  - All affected tests updated: per-field error message text, `aria-describedby` IDs updated to field-scoped values, sync assertions after async submit wrapped in `waitFor`.

## [0.47.0] - 2026-04-26
### Changed
- **CLAUDE.md** refactored from a single monolithic file into a slim root file that `@`-imports three sub-files housed in `.claude/`: `commands.md` (all shell commands), `constraints.md` (non-obvious architectural constraints), and `maintenance.md` (doc update table). Claude Code skill definitions placed in `.claude/commands/` (`cicd.md`, `done.md`, `test.md`). `.gitignore` updated to track the `.claude/` directory while excluding `settings.local.json`.

## [0.46.0] - 2026-04-26
### Refactor
- Frontend dashboard: applied consistent file naming conventions across `src/`.
  - Pages renamed with `Page` suffix: `Login.tsx` → `LoginPage.tsx`, `Register.tsx` → `RegisterPage.tsx`, `HomeDashboard.tsx` → `HomeDashboardPage.tsx`, `HomePublic.tsx` → `HomePublicPage.tsx`, `ChangePassword.tsx` → `ChangePasswordPage.tsx`, `RequestPasswordReset.tsx` → `RequestPasswordResetPage.tsx`, `ResetPassword.tsx` → `ResetPasswordPage.tsx`, `Settings.tsx` → `SettingsPage.tsx`, `NotFound.tsx` → `NotFoundPage.tsx`.
  - Services renamed with `.service` suffix: `api.ts` → `api.service.ts`, `authApi.ts` → `authApi.service.ts`.
  - Types renamed with `.type` suffix: `auth.ts` → `auth.type.ts`, `api.ts` → `api.type.ts`.
  - Constants renamed with `.constant` suffix: `apiErrors.ts` → `apiErrors.constant.ts`.
  - All imports, test files, and references updated accordingly.

## [0.45.0] - 2026-04-25
### Changed
- Frontend dashboard: auth context functions (`login`, `register`, `changePassword`, `resetPassword`, `requestPasswordReset`) now return `AuthResult` (`{ ok: boolean; error?: string }`) instead of plain `boolean`, giving callers access to the specific error message from the API. `AuthResult` type exported from `src/types/auth.ts`. All pages and tests updated accordingly.

### Added
- Frontend dashboard: edge-case test coverage in `AuthContext.test.tsx` — network-error path during session restore (status 0), network-error path during login (error message propagated through `AuthResult`), sessionStorage token-expiry scenario (unauthorized handler clears sessionStorage and redirects). Session restore from `sessionStorage` (no localStorage entry) also now covered. `AuthContext.test.tsx` grows from 27 to 32 tests.

## [0.44.0] - 2026-04-25
### Changed
- Frontend dashboard: accessibility pass across the navigation and form components.
  - `NavBar.tsx`: hamburger button now carries `aria-expanded` (reflects open/close state) and `aria-controls="mobile-menu"`. Mobile menu panel has `id="mobile-menu"`, `role="navigation"`, and `aria-label="Mobile navigation"`. A `useEffect` implements a full focus trap — first focusable item receives focus on open, Tab/Shift-Tab cycle within the panel, Escape closes it, and focus returns to the hamburger button on close.
  - `Register.tsx`, `ChangePassword.tsx`, `ResetPassword.tsx`: inline error message elements given stable IDs; all form inputs link to the active error via `aria-describedby` so screen readers announce the error when an input is focused.
  - Landmark structure confirmed correct: `App.tsx` wraps all routes in `<main>` and `NavBar` renders as `<header>`.

## [0.43.0] - 2026-04-25
### Added
- Frontend dashboard: "Remember me" checkbox on the Login page. Unchecked (default) stores the session in `sessionStorage` so it clears when the tab closes; checked stores in `localStorage` for cross-session persistence. `AuthProvider` login, logout, session-restore, and the `onUnauthorized` handler all updated to clear both storages consistently. `login` type in `AuthContextValue` updated to accept an optional `rememberMe` boolean.
- Frontend dashboard: Loading skeleton in `HomeDashboard.tsx` — animated placeholder cards are shown while `isLoading` is `true` (session restore in progress), preventing layout shift.

### Changed
- Frontend dashboard: Login error now shown as a toast notification (via `useToast`) instead of an inline `msg-error` paragraph, making it consistent with `RequestPasswordReset.tsx` and `ChangePassword.tsx`.
- Frontend dashboard: First focusable input on `Login.tsx` (email), `Register.tsx` (first name), and `RequestPasswordReset.tsx` (email) now receives `autoFocus` on page load so users can start typing without clicking.

## [0.42.0] - 2026-04-25
### Fixed
- Frontend dashboard: `onUnauthorized` handler in `AuthProvider` moved into a `useEffect` with an empty dependency array and a cleanup that calls `onUnauthorized(null)` on unmount. Previously the handler was re-registered on every render with no cleanup. (QA #26)
- Frontend dashboard: all six auth functions in `AuthProvider` (`login`, `logout`, `register`, `changePassword`, `resetPassword`, `requestPasswordReset`) wrapped in `useCallback` with appropriate dependency arrays. `APPLICATION_CODE` moved to module scope. The `useMemo` dep array for the context value now explicitly lists all callbacks, preventing unnecessary consumer re-renders. (QA #27)
- Frontend dashboard: verified all source files under `src/` are clean 7-bit ASCII — no embedded non-ASCII literals that could produce mojibake when read by Latin-1 tools. (QA #24)

## [0.41.0] - 2026-04-24
### Fixed
- Frontend dashboard: suppressed React Router v6 console warnings by adding `future={{ v7_startTransition: true, v7_relativeSplatPath: true }}` to `<BrowserRouter>` in `src/App.tsx`. Eliminates the `v7_startTransition` and `v7_relativeSplatPath` deprecation warnings that appeared on every page load and prepares the app for a future v7 upgrade. (QA #25)

## [0.40.0] - 2026-04-24
### Fixed
- Frontend dashboard: backend error codes (e.g. `INVALID_USERNAME_OR_PASSWORD`) are now translated to human-readable messages before being displayed in toasts. Added `BACKEND_ERROR_CODES` lookup map to `src/constants/apiErrors.ts` covering all codes returned by the users service. `getErrorMessage` in `api.ts` now checks both `message` and `Message` JSON fields and resolves any known code before falling back to generic status-based messages; 400 responses no longer swallow the backend's specific error detail.

## [0.39.0] - 2026-04-24
### Refactor
- Frontend dashboard: extracted auth HTTP calls out of `AuthContext` into a dedicated `src/services/authApi.ts` module — `AuthContext` is now pure state management with no direct `fetch` calls.
- Frontend dashboard: created `src/types/auth.ts` (`User`, `AuthContextValue`) and `src/types/api.ts` (`ApiResponse<T>`) to centralize shared TypeScript types; types are imported from these files rather than defined inline.
- Frontend dashboard: extracted all hardcoded HTTP error strings from `api.ts` into `src/constants/apiErrors.ts` as the `API_ERRORS` typed constant object.
- Frontend dashboard: replaced `Link` + manual `useLocation` active-state logic in `NavBar` with `NavLink` from react-router-dom; `navLinkClass` helper drives active styling declaratively; Settings retains custom path matching for `/change-password`.

## [0.38.0] - 2026-04-24
### Refactor
- Frontend dashboard: reorganized `src/` folder structure to align with React 2025 conventions. Moved `auth/AuthContext.tsx` + route guards (`ProtectedRoute`, `PublicOnlyRoute`) into `features/auth/`; `NavBar` into `layouts/`; `api.ts` into `services/`; `index.css` into `styles/`. Extracted all `<Routes>` from `App.tsx` into a new `router.tsx`. `components/` now holds only shared reusable UI (PasswordInput, PasswordStrength, Toast). All imports updated; 205 tests continue to pass.

## [0.37.0] - 2026-04-23
### Fixed
- Per-page browser tab titles: added `usePageTitle` hook (`src/hooks/usePageTitle.ts`) and applied it to all 9 page components. Tab titles now read "Login — Expenses Manager", "Dashboard — Expenses Manager", etc. The landing page keeps "Expenses Manager". The Reset Password page switches between "Reset Password" and "Create Password" based on the `?mode=create` query param. (QA #22)

## [0.36.0] - 2026-04-23
### Fixed
- `Login.tsx`: added `submitting` state — the Login button is now disabled and shows a spinner with "Signing in…" while the request is in flight, matching the UX pattern already present in `RequestPasswordReset.tsx`. Email and password inputs are also disabled during submission.
- `Register.tsx`: same fix — the Register button is now disabled and shows a spinner with "Submitting…" during registration. All three inputs are disabled while the request is pending.

## [0.35.0] - 2026-04-23
### Security
- Expenses service `Dockerfile`: switched runtime base image from `mcr.microsoft.com/dotnet/aspnet:8.0` (Debian 12) to `mcr.microsoft.com/dotnet/aspnet:8.0-noble-chiseled` (Ubuntu 24.04, chiseled), matching the users service. Removes the package surface carrying OS-level HIGH Trivy alerts and eliminates the explicit `USER app` directive (chiseled runs non-root by default).

## [0.34.0] - 2026-04-23
### Security
- Users service `Dockerfile`: switched runtime base image from `mcr.microsoft.com/dotnet/aspnet:8.0` (Debian 12) to `mcr.microsoft.com/dotnet/aspnet:8.0-noble-chiseled` (Ubuntu 24.04, chiseled). The chiseled variant contains only the files required to run the .NET runtime — no shell, no package manager, no systemd, no ncurses — eliminating the package surface that carried the HIGH Trivy alerts (`CVE-2026-0861`, `CVE-2026-29111`, `CVE-2025-69720`). The image also runs non-root by default, making the explicit `USER app` directive redundant (removed).

## [0.33.0] - 2026-04-23
### Security
- Users service: pinned `Microsoft.Bcl.Memory` to `9.0.14` in both the main and test projects to remediate CVE-2026-26127 (HIGH — .NET denial of service via out-of-bounds read). The package was a transitive dependency pulled in at a vulnerable version; the explicit reference overrides it with the patched release.

## [0.32.0] - 2026-04-22
### Fixed
- CI Docker template (`ci-docker.yml`): added `docker builder prune -af` after each image push to reclaim build cache disk space on the CI runner.
- Docker-in-Docker (`docker-compose-tools.yml`): switched the DinD storage driver from `vfs` to `overlay2` for improved build performance and reduced disk usage.

## [0.31.0] - 2026-04-21
### Added
- `index.html`: added SEO and Open Graph meta tags — `<meta name="description">`, `<meta name="robots">`, `og:type`, `og:title`, `og:description`, `og:image`, and Twitter Card equivalents (QA #23).

## [0.30.0] - 2026-04-19
### Fixed
- `AuthProvider`, `ToastProvider`, and `PasswordInput` component props marked as `Readonly<>` (SonarQube: mark component props read-only).
- `Toast.tsx` `show` function: extracted inner `setToasts` filter callback to a named `removeById` variable, reducing function nesting depth from 5 to 4 (SonarQube: do not nest functions more than 4 levels deep).

## [0.29.0] - 2026-04-19
### Added
- `PasswordInput` component (`src/components/PasswordInput.tsx`): reusable password field with an eye-icon show/hide toggle. Clicking the button toggles `type` between `"password"` and `"text"` and updates the `aria-label` ("Show password" / "Hide password"). All six password inputs across `Login.tsx`, `ChangePassword.tsx`, and `ResetPassword.tsx` now use this component (QA #19).

## [0.28.0] - 2026-04-19
### Fixed
- Password inputs: replaced `placeholder="••••••••"` with `placeholder=""` on all six password fields across `Login.tsx`, `ChangePassword.tsx`, and `ResetPassword.tsx`. The Unicode bullet workaround displayed inconsistently across browsers; `type="password"` already handles masking (QA #18).

## [0.27.0] - 2026-04-19
### Fixed
- Landing page (`/`): hero content is now vertically centred in the viewport. `HomePublic` uses the `.auth-page` shared class (`flex-1 flex items-center justify-center`) which centres the content within the full remaining viewport height below the navbar (QA #17).

## [0.26.0] - 2026-04-19
### Fixed
- NavBar: corrected the mobile menu Settings link from `/change-password` to `/settings`, fixing a regression introduced during QA #13 where mobile users were routed to the wrong page (QA #16).

## [0.25.0] - 2026-04-19
### Added
- `PasswordStrength` component (`src/components/PasswordStrength.tsx`): live password strength indicator with a 5-segment colour bar (Weak → Fair → Good → Strong) and a checklist of five criteria — at least 8 characters, uppercase letter, lowercase letter, number, and special character. Displayed below the "New password" field on Change Password and Reset Password pages (QA #29).
### Fixed
- Change Password and Reset Password pages: added client-side minimum-length validation; passwords shorter than 8 characters are now rejected with "Password must be at least 8 characters." before the API is called (QA #29).

## [0.24.0] - 2026-04-19
### Fixed
- Request Password Reset page: added "← Back to login" link below the form, giving users a clear navigation path back without relying on the browser back button or the logo (QA #14).

## [0.23.0] - 2026-04-15
### Added
- `Settings` page (`src/pages/Settings.tsx`) at route `/settings`: a proper settings hub with a "Password" card linking to `/change-password`. The navbar "Settings" link now points to `/settings`; the active state covers both `/settings` and `/change-password` (QA #13).
### Fixed
- Change Password page: added "← Back to settings" link at the top, returning users to `/settings` (QA #15).

## [0.22.0] - 2026-04-15
### Fixed
- Dashboard: removed the redundant "Logout" button from the page body. Sign out is now available exclusively via the navbar "Sign out" link, eliminating the duplicate action and the inconsistent redirect destinations (QA #12).

## [0.21.0] - 2026-04-14
### Fixed
- Register page: replaced auto-redirect after successful registration with a dedicated success state. The form is now replaced by a full-card success message and a manual "Go to login →" link — users control when to proceed and the message is always fully visible (QA #10).
- Register page: corrected misleading success message. Registration creates an unvalidated account with no password; the user must click the verification link in their inbox (which redirects to `/reset-password`) before they can log in. The new message tells the user exactly what to do next.

## [0.20.0] - 2026-04-12
### Added
- `NotFound` page (`src/pages/NotFound.tsx`): dedicated 404 component with a "Go to home" link, replacing the silent fallback that rendered the public landing page for unknown routes (QA #8). The `*` catch-all route in `App.tsx` now uses `<NotFound />`.

## [0.19.0] - 2026-04-12
### Fixed
- `/reset-password` and `/request-password-reset` routes are now wrapped in `PublicOnlyRoute` — authenticated users are redirected to `/dashboard` instead of seeing the unauthenticated form (QA #9).
- Reset Password page: on successful reset, the form fields are cleared, the submit button is disabled, and the user is redirected to `/` after 3 seconds so the success message is readable.

## [0.18.0] - 2026-04-12
### Fixed
- Email validation redirect now correctly lands on the frontend `/reset-password` page. Root cause: `ApplicationService.GetApplicationByCodeAsync` mapped the `Application` entity to `ApplicationEo` but omitted `ResetPasswordUrlPath`, so it was always `null` and the redirect became a bare `?email=...` relative URL. Fixed by adding `ResetPasswordUrlPath = app.ResetPasswordUrlPath` to the mapping in `ApplicationService.cs`.
- `APP_ResetPasswordUrlPath` changed from absolute URL (`https://localhost/reset-password`) to a host-agnostic relative path (`/reset-password`) via `FixResetPasswordUrl` migration — the browser resolves it against the request origin automatically, removing any environment-specific host from the database.
- Email address in the validate-email redirect URL is now percent-encoded via `Uri.EscapeDataString` so `@` is safely transmitted as `%40` in the `Location` header (`AuthenticationController.cs`).

## [0.17.0] - 2026-04-12
### Fixed
- Route `/dashboard` now works for authenticated users: renamed the authenticated route from `/home` to `/dashboard` throughout the frontend (`App.tsx`, `NavBar.tsx`, `PublicOnlyRoute.tsx`, `Login.tsx`). All redirect targets, logo links, and nav links updated. Resolves QA Bug #7 and naming inconsistency #30.

## [0.16.0] - 2026-03-23
### Fixed
- Change Password and Reset Password pages now show distinct validation error messages: "All fields are required." when any field is empty, and "New passwords do not match." when the new password fields differ (Bug #6). API failures show "Incorrect current password." / "Password reset failed. Please try again." respectively. The API is no longer called when client-side validation fails.
- Trivy Docker image reference updated from `aquasec/trivy:latest` (removed from Docker Hub) to `ghcr.io/aquasecurity/trivy:latest` in `ci-docker-security.yml`; fixes `docker-security` pipeline stage failing with manifest unknown error.
- Mailpit `MP_SMTP_AUTH_ACCEPT_ANY` and `MP_SMTP_AUTH_ALLOW_INSECURE` env vars added to `docker-compose-tools.yml`; fixes `5.7.0 Authentication Required` SMTP error that silently swallowed registration emails.
- `EXPENSES_MANAGEMENT_USERS_AUTHSERVICE_VERIFY_EMAIL_URL` corrected in `.env` from `localhost:9100/api/auth/validate-email` to `localhost/api/users/auth/validate-email` (nginx route, not direct service port).
- `APP_UrlPath` and `APP_ResetPasswordUrlPath` for the `EXPENSES_MANAGER` application updated from `localhost:5173` (Vite dev server) to `localhost` (nginx) via migration `20260323120000_UpdateApplicationUrls`.

### Changed
- `Migrations/` added to `sonar.exclusions` (full analysis exclusion) in both backend `SonarQube.Analysis.xml` files; it was already in `sonar.coverage.exclusions` but SonarQube still raised issues on migration files — auto-generated code that cannot be meaningfully fixed.


## [0.15.0] - 2026-03-23
### Added
- Mailpit added to `docker-compose-tools.yml` for local email testing (SMTP on port 1025, web UI on `http://localhost:8025`).
- `EXPENSES_MANAGEMENT_USERS_EMAILAUTH_ENABLESSL` env var to control SSL on the SMTP connection; defaults to `true` to preserve existing behaviour.
- `EnableSsl` property on `EmailOptions`, wired through `Program.cs`, `EmailHelper`, and passed via `docker-compose-apps.yml`.
- `.env` updated to point at Mailpit (`host.docker.internal:1025`, `EnableSsl=false`); `.env.example` includes the new variable.
- NuGet package cache added to `ci-build.yml` and `ci-test.yml` (stored in MinIO via the S3 runner cache) to avoid redundant downloads across pipeline runs.
- `network_mode = "host"` added to the GitLab runner Docker config (`config.toml`) so job containers inherit the dind container's network and can reach external hosts (fixes `NU1301` restore failures).

### Fixed
- GitLab CI `dotnet restore` failing with `NU1301` (unable to reach `api.nuget.org`) caused by broken NAT inside nested Docker; resolved by setting `network_mode = "host"` on the runner.
- SonarQube S5332 hotspot on `client.EnableSsl` in `EmailHelper` suppressed with `// NOSONAR` and a justification comment (value is `false` only in local dev against Mailpit; all other environments use `true`).

## [0.14.0] - 2026-03-22
### Added
- HttpOnly cookie-based authentication: the backend now sets `auth_token` as an `HttpOnly; Secure; SameSite=Strict` cookie on login and clears it on logout, eliminating the JWT XSS vulnerability.
- `GET /auth/session` endpoint in users service: validates the HttpOnly cookie and returns 200/401, used by the frontend to restore session state on page load.
- `isLoading` state in `AuthContext`: prevents `ProtectedRoute` and `PublicOnlyRoute` from incorrectly redirecting while session restore is in progress.
- `PublicOnlyRoute` component: redirects authenticated users to `/home` when accessing `/`, `/login`, or `/register`.

### Fixed
- JWT token no longer stored in `localStorage` — resolves critical XSS risk (Bug #3).
- `api.ts` now passes `credentials: 'include'` on all requests to support cross-origin cookie forwarding.
- `api.ts` `request()` cognitive complexity reduced from 21 to 13 (SonarQube gate): extracted `getErrorMessage()` helper and simplified the unauthorized handler to `if/else`.
- `AuthenticationController` `auth_token` cookie now sets `Secure = true` (SonarQube gate).
- `api.ts` login redirect changed from `window.location.assign` to `globalThis.location.assign` (SonarQube gate: prefer `globalThis` over `window`).
- nginx config updated to forward `Cookie` header in `/internal/auth/check` subrequest and include `Access-Control-Allow-Credentials: true` on all API location blocks.
- `changePassword` now includes the user's email in the request body, resolving a 400 validation error from the API.

### Changed
- `AuthContext` removed all token state (`token`, `setAuthToken`, JWT decode logic); login now stores only user info in localStorage for UI display.
- All auth `post()` calls use `{ skipUnauthorized: true }` to prevent the global unauthorized handler from firing on expected 401 responses.
- `AuthenticationController`: extracted cookie name into `private const string AuthTokenCookie = "auth_token"` to eliminate magic string duplication (SonarQube gate).
- `AuthenticationControllerTests`: updated cookie assertions to use `Headers.SetCookie` and `Headers.Cookie` typed properties instead of string indexers (SonarQube recommendations).
- Dashboard welcome message now displays the user's first name instead of their email address, falling back to email if first name is unavailable.
- `AuthContext` user type extended with optional `firstName` field, populated from the login API response.

## [0.13.0] - 2026-03-21
### Added
- Tailwind CSS v3 integrated into the frontend dashboard (PostCSS pipeline, `tailwind.config.ts` with custom design system: indigo brand palette, Inter font, surface tokens, custom shadows).
- Responsive `NavBar` component with auth-aware links and mobile hamburger menu.
- Shared UI primitive classes in `@layer components` (`.field-input`, `.btn-primary`, `.auth-card`, `.msg-error`, etc.) for consistent form and layout styling.
- `GET /auth/check` endpoint in users service for nginx auth subrequest validation — enables nginx to authenticate all inbound requests internally before forwarding to backend services.
- CI/CD deploy job added for the expenses service (completing full pipeline coverage for both backend services).

### Changed
- Full frontend UI redesign: modern minimalist SaaS aesthetic replacing all inline styles with Tailwind utilities.
- Toast notifications redesigned with light-themed semantic colors and SVG icons; added `aria-live` region for accessibility.
- All auth pages (Login, Register, ResetPassword, RequestPasswordReset, ChangePassword) restyled with centered card layout.
- Expenses service Dockerfile refactored for leaner image builds.
- Updated documentation: `README.md` files for backend services corrected from .NET 6 to .NET 8; `frontend/dashboard/README.md` rewritten to reflect current tech stack and structure.

## [0.12.0] - 2026-03-17
### Added
- Custom Docker Image Updater API in Python to replace Watchtower for automatic zero-downtime deployments.
- Health check endpoints to backend services (`users` and `expenses`).
- Unit tests and associated testing commands to the `frontend/dashboard`.

### Changed
- Fixed CI/CD pipelines across the repository and integrated strict SonarQube quality gates.
- Resolved various SCA security scanner vulnerabilities.
- Updated frontend URL routing logic and dashboard paths.

## [0.11.0] - 2026-03-07
### Changed
- Updated infrastructure/README.md and added README files to configs, jobs, scripts, and volumes directories for improved documentation.
- Refactored infrastructure folder structure for clarity and maintainability.
- Fixed SonarQube quality gate and coverage issues in backend services.

## [0.10.0] - 2026-03-05
### Added
- Initial workspace structure documented in README.md.
- Backend services: expenses, users (.NET 8, EF Core).
- Frontend dashboard: React + Vite + TypeScript.
- Docker Compose infrastructure for apps and tools.
- Environment variable-based configuration for secrets.
- SQL scripts for database setup.
- Quick start instructions for backend and frontend.
- Linked RabbitMQ, Grafana, Prometheus management URLs.
- Contributing and license sections.
- Nginx full configuration (virtual host, SSL, mime types, fastcgi/scgi params) and automated deployment via CI/CD.
- Expenses service GitLab CI/CD pipeline (build, test, quality, docker, deploy stages).
- Expenses backend unit tests: external user repository and RabbitMQ service coverage; in-memory test DB helper (`TestExpensesDbContextWrapper`).
- EF Core migrations for expenses service applied automatically at startup via `db.Database.MigrateAsync()`.

## [0.9.0] - 2026-02-15
### Added
- Integrated GitLab CI/CD, SonarQube, and security scanning (SAST, SCA, secrets-scan, docker-security, gitleaks, trivy).
- Added job runner, cron, and supervisor configuration for scheduled/background tasks.
- Added monitoring dashboards: Grafana, Prometheus, RabbitMQ.
- Added frontend dashboard build/test pipeline and unit tests.

### Changed
- Multiple fixes and refactors to CI/CD, SonarQube, and test pipelines.
- Improved Dockerfile and infrastructure scripts for cross-platform compatibility.

## [0.8.0] - 2026-01-31
### Added
- Frontend and backend integration improvements.
- Users backend comprehensive unit test suite: repository tests (User, Application, Authentication, Role), service tests (Application, Authentication, Role), controller tests (AuthenticationController), and infrastructure tests (CryptographyHelper, EmailHelper).
- EO (Entity Object) DTO pattern adopted in users service and controller layers.
- GitLab mirror to GitHub, CRLF/LF normalization, and improved pipeline triggers.

### Changed
- Frontend home page route renamed to `/home-auth` (dashboard).
- Refactored backend and frontend code for maintainability.
- Improved Docker build and security.

## [0.7.0] - 2025-12-31
### Added
- Switched frontend to React (Next.js, then native React, then TailwindCSS).
- Upgraded Angular and improved login/register flows.
- Replaced Dapper with EF in Users backend.

## [0.6.0] - 2024-05-29
### Added
- Users backend: register, verify email, change password, email helper.
- Expenses backend: models, RabbitMQ integration, Postgres config, migrations.

## [0.5.0] - 2024-02-19
### Added
- Initial repository structure, .gitignore, and documentation.
- Added backend and infrastructure README updates.
- Added monitoring dashboards and run scripts.
