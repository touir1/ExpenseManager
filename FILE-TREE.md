# File Tree

Excludes: `node_modules/`, `bin/`, `obj/`, `.git/`, `coverage/`, `dist/`, generated build artifacts.

```
ExpenseManager/
├── .claude/
│   ├── commands/
│   │   ├── cicd.md                    — `/cicd` skill definition
│   │   ├── done.md                    — `/done` skill definition
│   │   ├── qa.md                      — `/qa` skill definition
│   │   └── test.md                    — `/test` skill definition
│   ├── cicd.md                        — CI/CD skill reference
│   ├── commands.md                    — All shell commands (imported by CLAUDE.md)
│   ├── constraints.md                 — Non-obvious architectural constraints (imported by CLAUDE.md)
│   ├── maintenance.md                 — Doc update table (imported by CLAUDE.md)
│   └── settings.local.json            — Claude Code local settings (git-ignored)
├── .vscode/
│   ├── extensions.json                — Recommended VS Code extensions
│   └── settings.json                  — VS Code workspace settings
├── .github/
│   └── copilot-instructions.md        — GitHub Copilot workspace instructions
├── .gitignore                         — Root Git ignore patterns
├── .gitlab-ci.yml                     — Root GitLab CI pipeline
├── .gitleaks.toml                     — Gitleaks secret scanning config
├── .graphifyignore                    — Files excluded from graphify knowledge graph
├── CHANGELOG.md                       — Version history and release notes
├── CLAUDE.md                          — Claude Code instructions
├── FILE-TREE.md                       — Project file tree (this file)
├── LICENSE                            — Project license
├── README.md                          — Main project README
├── docs/
│   ├── issues/
│   │   ├── ongoing/
│   │   │   ├── fixes-and-suggestions.md          — Open improvement ideas and technical debt backlog
│   │   │   └── qa_test_results/
│   │   │       └── 2026-03-22-frontend-dashboard-qa.md  — Frontend dashboard QA (open items only)
│   │   └── fixed/
│   │       ├── fixes-and-suggestions-applied.md  — Applied suggestions (moved here from ongoing once shipped)
│   │       └── qa/
│   │           └── 2026-03-22-frontend-dashboard-fixes.md  — Resolved issues from the 2026-03-22 QA session
│   ├── design/                        — UI design reference files (generated with Claude)
│   │   ├── ExpensesManager Redesign.html             — Full-page design mockup
│   │   ├── ExpensesManager Redesign (standalone).html — Standalone design (minified)
│   │   ├── ExpensesManager Redesign (standalone-src).html — Standalone design (source)
│   │   ├── app-flow.jsx               — App navigation flow diagram
│   │   ├── auth.jsx                   — Auth screens design
│   │   ├── dashboard-a.jsx            — Dashboard variant A
│   │   ├── dashboard-b.jsx            — Dashboard variant B
│   │   ├── dashboard-c.jsx            — Dashboard variant C
│   │   ├── design-canvas.jsx          — Free-form design canvas
│   │   ├── ios-frame.jsx              — iOS device frame wrapper
│   │   ├── marketing.jsx              — Marketing/landing page design
│   │   ├── mobile.jsx                 — Mobile layout design
│   │   ├── system.jsx                 — Design system overview
│   │   └── tokens.jsx                 — Design token reference
│   ├── wiki/                          — Project knowledge base
│   │   ├── index.md                   — Wiki home and navigation
│   │   ├── api-reference.md           — API endpoint reference
│   │   ├── architecture.md            — System architecture overview
│   │   ├── backend-expenses-service.md — Expenses service internals
│   │   ├── backend-users-service.md   — Users service internals
│   │   ├── data-models.md             — Database schema and models
│   │   ├── family-system.md           — Family management feature guide
│   │   ├── frontend.md                — Frontend architecture guide
│   │   ├── infrastructure.md          — Infrastructure and deployment guide
│   │   ├── messaging.md               — RabbitMQ messaging guide
│   │   ├── testing.md                 — Testing strategy and conventions
│   │   └── use-cases.md               — User stories and use cases
│   ├── issues/
│   │   ├── ongoing/
│   │   │   ├── fixes-and-suggestions.md          — Open improvement ideas and technical debt backlog
│   │   │   └── qa_test_results/
│   │   │       ├── 2026-03-22-frontend-dashboard-qa.md  — Frontend dashboard QA (open items only)
│   │   │       └── 2026-04-29-frontend-dashboard-qa.md  — Frontend dashboard QA session 2 (open items)
│   │   └── fixed/
│   │       ├── fixes-and-suggestions-applied.md  — Applied suggestions (moved here from ongoing once shipped)
│   │       └── qa/
│   │           ├── 2026-03-22-frontend-dashboard-fixes.md  — Resolved issues from the 2026-03-22 QA session
│   │           └── 2026-04-29-frontend-dashboard-fixes.md  — Resolved issues from the 2026-04-29 QA session
│   └── plans/
│       ├── application-description.md  — Full product specification (roles, families, audit, rate resolution, all screens)
│       ├── implementation-plan.md      — 15-phase implementation plan
│       └── done/
│           └── nexus-proxy-integration.md  — Completed plan: Nexus Repository Manager integration
│
├── backend/
│   ├── dashboard/
│   │   ├── Dockerfile                 — Docker image for dashboard service
│   │   └── README.md                  — Dashboard service documentation
│   │
│   ├── expenses/
│   │   ├── .dockerignore
│   │   ├── .gitlab-ci.yml             — Expenses service CI/CD pipeline
│   │   ├── .trivyignore               — Trivy scanner ignore list
│   │   ├── Dockerfile                 — Docker image for expenses service
│   │   ├── README.md
│   │   ├── SonarQube.Analysis.xml     — SonarQube project settings
│   │   ├── Touir.ExpensesManager.Expenses.sln
│   │   ├── sql_database_scripts/
│   │   │   ├── create_database.sql
│   │   │   ├── create_tables.sql
│   │   │   └── create_user_and_privileges.sql
│   │   ├── Touir.ExpensesManager.Expenses/
│   │   │   ├── Program.cs             — Entry point, DI registration, migrations
│   │   │   ├── appsettings.json
│   │   │   ├── appsettings.Development.json
│   │   │   ├── Touir.ExpensesManager.Expenses.csproj
│   │   │   ├── Properties/
│   │   │   │   └── launchSettings.json
│   │   │   ├── Jobs/
│   │   │   │   └── RateAutoUpdateJob.cs     — Quartz IJob; [DisallowConcurrentExecution]; cron scheduled from CurrencyRateOptions.UpdateTime; calls ICurrencyRateService.RunDailyUpdateAsync; logs on failure
│   │   │   ├── Messaging/
│   │   │   │   ├── Messages/
│   │   │   │   │   └── UserEventMessage.cs  — Inbound event DTO + UserEventType constants (Created/Updated/Deleted)
│   │   │   │   └── Consumers/
│   │   │   │       └── UserEventConsumer.cs — BackgroundService; binds expenses.users.sync → users.events; inbox deduplication via IInboxRepository.ExistsAsync; calls IUserRepository.SaveOrUpdateUserAsync / DeleteUserAsync
│   │   │   ├── Infrastructure/
│   │   │   │   ├── ExpensesDbContext.cs     — EF Core context; all 13 DbSets with full Fluent API config
│   │   │   │   ├── JwtCookieReader.cs       — Decodes auth_token cookie (base64url payload) to extract sub claim; no signature validation (nginx validates upstream)
│   │   │   │   └── Options/
│   │   │   │       ├── CurrencyRateOptions.cs — UpdateTime (default 02:00 UTC); env prefix EXPENSES_MANAGEMENT_EXPENSES_CURRENCYRATE_*
│   │   │   │       ├── FamilyOptions.cs     — InviteExpiryInDays (env: EXPENSES_MANAGEMENT_EXPENSES_FAMILY_INVITE_EXPIRY_IN_DAYS, default 7)
│   │   │   │       ├── PostgresOptions.cs
│   │   │   │       └── RabbitMQOptions.cs
│   │   │   ├── Controllers/
│   │   │   │   ├── CategoryController.cs    — GET /categories → IEnumerable<CategoryDto>
│   │   │   │   ├── ControllerErrors.cs      — Shared internal static class: SERVER_ERROR, UNAUTHORIZED, EXPENSE_NOT_FOUND, MISSING_PARAMETERS, TAG_NOT_FOUND, RATE_NOT_FOUND, CONFLICT_NOT_FOUND
│   │   │   │   ├── CurrencyController.cs    — GET /currencies → IEnumerable<CurrencyDto>
│   │   │   │   ├── CurrencyRateController.cs — GET /rates/history, POST /rates (201), POST /rates/bulk (204), PUT /rates/default (204), GET /rates/conflicts, POST /rates/conflicts/{id}/resolve (204), POST /rates/refresh (204)
│   │   │   │   ├── ExpenseController.cs     — POST/PUT/DELETE/GET/GET(paged) /expenses; GetByIdAsync accepts ?displayCurrencyId; FamilyForbiddenException → 403 on create/update
│   │   │   │   ├── FamilyController.cs      — 10 endpoints: list, detail, create, rename, archive, unarchive, invite, accept-invite, remove-member, change-role
│   │   │   │   ├── TagController.cs         — GET /tags → TagListDto; POST /tags → TagDto (idempotent); DELETE /tags/{id} → 204 or 404
│   │   │   │   ├── DTO/
│   │   │   │   │   ├── CategoryDto.cs       — Id, Name, Description?, Subcategories: IEnumerable<SubcategoryDto>
│   │   │   │   │   ├── SubcategoryDto.cs    — Id, Name, Description? (reused for category + subcategory slots in ExpenseDto)
│   │   │   │   │   ├── CurrencyDto.cs       — Id, Code, Name, Symbol, Decimals
│   │   │   │   │   ├── ExpenseDto.cs        — Id, Amount, Currency: CurrencyDto?, Date, Category: SubcategoryDto?, Subcategory: SubcategoryDto?, Description?, CreatedAt, ModifiedAt?, ModifiedFrom?, Tags: TagDto[], ConvertedAmount?: decimal, DisplayCurrency?: CurrencyDto
│   │   │   │   │   ├── ExpenseFilterDto.cs  — DateFrom?, DateTo?, CategoryId?, SubcategoryId?, CurrencyId?, AmountMin?, AmountMax?, Description?, TagIds?, DisplayCurrencyId?, Page (default 1), PageSize (default 20)
│   │   │   │   │   ├── RateDto.cs           — SourceCurrencyId, DestinationCurrencyId, Date, Rate, RateSource
│   │   │   │   │   ├── RateConflictDto.cs   — Id, SourceCurrencyId, DestinationCurrencyId, Date, AutomaticRate, ManualRate, Status, ResolvedAt?
│   │   │   │   │   ├── TagDto.cs            — Id, Name
│   │   │   │   │   ├── TagListDto.cs        — Own: IEnumerable<TagDto>, Family: IEnumerable<TagDto>
│   │   │   │   │   └── FamilyDto.cs         — Family response shape: Id, Name, IsDefault, IsDeleted, Members: FamilyMemberDto[]
│   │   │   │   ├── Requests/
│   │   │   │   │   ├── IExpenseRequest.cs      — Shared interface (Amount, CurrencyId, Date, CategoryId?, SubcategoryId?, Description?, TagIds?) implemented by Create + Update DTOs
│   │   │   │   │   ├── CreateExpenseRequest.cs — Amount (required), CurrencyId (required), Date (required), CategoryId?, SubcategoryId?, Description?, TagIds?
│   │   │   │   │   ├── UpdateExpenseRequest.cs — same fields as Create
│   │   │   │   │   ├── CreateTagRequest.cs     — Name (required)
│   │   │   │   │   ├── AddRateRequest.cs       — SourceCurrencyId, DestinationCurrencyId, Date, Rate (all required)
│   │   │   │   │   ├── BulkAddRatesRequest.cs  — Rates: List<AddRateRequest>
│   │   │   │   │   ├── SetDefaultRateRequest.cs — SourceCurrencyId, DestinationCurrencyId, Rate
│   │   │   │   │   ├── ResolveConflictRequest.cs — Resolution (string: AcceptAuto/KeepManual/Custom), CustomRate?
│   │   │   │   │   └── RefreshRatesRequest.cs    — From: DateOnly (required); SourceCurrencyId?: int; DestinationCurrencyId?: int
│   │   │   │   └── Responses/
│   │   │   │       ├── ErrorResponse.cs     — Uniform error envelope (matches users service pattern)
│   │   │   │       └── ExpensePagedResponse.cs — Items: ExpenseDto[], TotalCount, Page, PageSize, TotalPages
│   │   │   ├── Models/
│   │   │   │   ├── Category.cs              — IsDeleted + DeletedAt (soft-delete); ParentCategoryId, Children
│   │   │   │   ├── Currency.cs
│   │   │   │   ├── Expense.cs               — IsDeleted + DeletedAt (soft-delete); owner, amount, date, category, audit fields; FK int columns; ICollection<ExpenseTag> ExpenseTags
│   │   │   │   ├── Family.cs                — IsDeleted + DeletedAt (soft-delete)
│   │   │   │   ├── FamilyInvitation.cs      — GUID token, ExpiresAt, InviteeEmail, AcceptedAt?, AcceptedByUserId?
│   │   │   │   ├── FamilyMembership.cs      — RoleId (int FK) instead of enum
│   │   │   │   ├── ExpenseFamilyAttribution.cs
│   │   │   │   ├── Tag.cs                   — Global tag entity; unique Name; ICollection<UserTag> UserTags + ICollection<ExpenseTag> ExpenseTags
│   │   │   │   ├── UserTag.cs               — Junction: (UserId, TagId); created on explicit POST /tags or auto-adopt when attaching tag to expense
│   │   │   │   ├── ExpenseTag.cs
│   │   │   │   ├── CurrencyDailyRate.cs     — RateSourceId (int FK) instead of enum
│   │   │   │   ├── CurrencyPairDefault.cs
│   │   │   │   ├── CurrencyRateConflict.cs  — StatusId/ResolutionId (int FK) instead of enums
│   │   │   │   ├── ExpenseAuditLog.cs       — OperationId/PerformedFromId (int FK) instead of enums
│   │   │   │   ├── ExpenseAuditSnapshot.cs  — SnapshotTypeId (int FK) instead of enum
│   │   │   │   ├── Lookups/
│   │   │   │   │   ├── ILookupEntity.cs     — Common interface: Id, Name
│   │   │   │   │   ├── OperationSource.cs   — 1=SingleWeb, 2=SingleMobile, 3=BulkWeb
│   │   │   │   │   ├── ModifiedSource.cs    — 1=Web, 2=Mobile
│   │   │   │   │   ├── FamilyRole.cs        — 1=Head, 2=Member
│   │   │   │   │   ├── RateSource.cs        — 1=Auto, 2=Manual
│   │   │   │   │   ├── ConflictStatus.cs    — 1=Pending, 2=Resolved
│   │   │   │   │   ├── ConflictResolution.cs — 1=AcceptAuto, 2=KeepManual, 3=Custom
│   │   │   │   │   ├── AuditOperation.cs    — 1=Add, 2=Update, 3=Delete
│   │   │   │   │   └── SnapshotType.cs      — 1=Before, 2=After
│   │   │   │   ├── InboxEvent.cs            — MessageId (PK), EventType, ReceivedAt, Status, Error?; InboxEventStatus constants
│   │   │   │   └── External/
│   │   │   │       └── User.cs              — Read-only mapping of users DB entity
│   │   │   ├── Validators/
│   │   │   │   ├── ExpenseRequestValidatorBase.cs   — Abstract base AbstractValidator<T> where T : IExpenseRequest; holds all shared rules (amount, currency, date, description, subcategory-requires-category)
│   │   │   │   ├── CreateExpenseRequestValidator.cs — Inherits ExpenseRequestValidatorBase<CreateExpenseRequest>
│   │   │   │   ├── UpdateExpenseRequestValidator.cs — Inherits ExpenseRequestValidatorBase<UpdateExpenseRequest>
│   │   │   │   ├── CreateTagRequestValidator.cs     — Name NotEmpty + MaxLength(50)
│   │   │   │   ├── CreateFamilyRequestValidator.cs  — Name NotEmpty + MaxLength(100)
│   │   │   │   ├── RenameFamilyRequestValidator.cs  — Name NotEmpty + MaxLength(100)
│   │   │   │   ├── InviteMemberRequestValidator.cs  — Email NotEmpty + EmailAddress + MaxLength(255)
│   │   │   │   └── ChangeMemberRoleRequestValidator.cs — Role Must be "Head" or "Member" (case-insensitive)
│   │   │   ├── Repositories/
│   │   │   │   ├── CategoryRepository.cs    — GetAllActiveAsync(): top-level non-archived categories with Include(Children), AsNoTracking
│   │   │   │   ├── CurrencyRepository.cs    — GetAllAsync(): all currencies, AsNoTracking
│   │   │   │   ├── ExpenseRepository.cs     — AddAsync, UpdateAsync, SoftDeleteAsync, GetByIdAsync (ownership + !IsDeleted + ExpenseTags include), GetPagedAsync (filtered + paginated, desc by date; TagIds OR filter); ClearExpenseTagsAsync, AddExpenseTagsAsync
│   │   │   │   ├── FamilyRepository.cs      — family CRUD, membership CRUD, invitation CRUD, attribution helpers (AddAttributionsAsync, ClearAttributionsAsync, RemoveMemberAttributionsAsync)
│   │   │   │   ├── InboxRepository.cs       — ExistsAsync(messageId), AddAsync(InboxEvent) for deduplication
│   │   │   │   ├── TagRepository.cs         — GetOwnAsync, GetFamilyAsync (co-member, excludes deleted families), GetByNameAsync, GetByIdsAsync, AddAsync, EnsureUserTagAsync, RemoveUserTagAsync, IsVisibleAsync
│   │   │   │   ├── CurrencyRateRepository.cs — GetExactAsync, GetMostRecentBeforeAsync, GetDefaultAsync, GetHistoryAsync, AddRateAsync, UpdateRateAsync, ManualRateExistsAsync, AddConflictAsync, GetPendingConflictsAsync, GetConflictByIdAsync, UpdateConflictAsync, SetDefaultAsync (upsert)
│   │   │   │   ├── Contracts/
│   │   │   │   │   ├── ICategoryRepository.cs
│   │   │   │   │   ├── ICurrencyRepository.cs
│   │   │   │   │   ├── IExpenseRepository.cs — AddAsync, UpdateAsync, SoftDeleteAsync, GetByIdAsync, GetPagedAsync, ClearExpenseTagsAsync, AddExpenseTagsAsync
│   │   │   │   │   ├── IFamilyRepository.cs  — family/membership/invitation/attribution methods; IsMemberAsync, HasDefaultFamilyAsync
│   │   │   │   │   ├── ITagRepository.cs     — GetOwnAsync, GetFamilyAsync, GetByNameAsync, GetByIdsAsync, AddAsync, EnsureUserTagAsync, RemoveUserTagAsync, IsVisibleAsync, SaveChangesAsync
│   │   │   │   │   ├── IInboxRepository.cs  — ExistsAsync, AddAsync
│   │   │   │   │   └── ICurrencyRateRepository.cs — GetExactAsync, GetMostRecentBeforeAsync, GetDefaultAsync, GetHistoryAsync, AddRateAsync, UpdateRateAsync, ManualRateExistsAsync, AddConflictAsync, GetPendingConflictsAsync, GetConflictByIdAsync, UpdateConflictAsync, SetDefaultAsync
│   │   │   │   └── External/
│   │   │   │       ├── Contracts/
│   │   │   │       │   └── IUserRepository.cs — GetUserByEmailAsync (invite flow), GetUserByIdAsync (filters !IsDeleted)
│   │   │   │       └── UserRepository.cs    — Read-only cross-service user access
│   │   │   ├── Services/
│   │   │   │   ├── FamilyExceptions.cs      — FamilyNotFoundException (→404), FamilyForbiddenException (→403; also used for tag visibility violations), FamilyConflictException (→409), FamilyInvitationException (→400)
│   │   │   │   ├── FamilyService.cs         — Implements IFamilyService; uses ILookupCacheService for role ID resolution; invite expiry from FamilyOptions
│   │   │   │   ├── RabbitMQService.cs       — RabbitMQ connection and messaging
│   │   │   │   ├── LookupCacheService.cs    — IMemoryCache-backed lookup; NeverRemove priority; loads entire table on first access
│   │   │   │   ├── CategoryService.cs       — Injects ICategoryRepository; projects Category → CategoryDto (filters archived children)
│   │   │   │   ├── CurrencyService.cs       — Injects ICurrencyRepository; projects Currency → CurrencyDto
│   │   │   │   ├── ExpenseService.cs        — Orchestrates IExpenseRepository + IExpenseAuditService + ITagRepository + ICurrencyRateService; validates tag visibility (→403), auto-adopts tags; resolves ConvertedAmount/DisplayCurrency when displayCurrencyId provided; maps Expense → ExpenseDto
│   │   │   │   ├── TagService.cs            — GetVisibleAsync calls repo in parallel; UseTagAsync is idempotent find-or-create + adopt; RemoveTagAsync removes UserTag only
│   │   │   │   ├── ExpenseAuditService.cs   — Writes ExpenseAuditLog + ExpenseAuditSnapshot(s): add→1 after, update→before+after, delete→1 before; snapshots store comma-sep tag IDs
│   │   │   │   ├── IRateProvider.cs         — FetchRatesAsync(code, date) → dict; FetchRatesRangeAsync(code, from, to) → dict<DateOnly, dict>
│   │   │   │   ├── FrankfurterRateProvider.cs — Calls api.frankfurter.app (ECB, no API key); single-date and range endpoints; registered via IHttpClientFactory
│   │   │   │   ├── CurrencyRateService.cs   — ResolveRateAsync; AddManualRateAsync (conflict if auto exists); RunDailyUpdateAsync; RefreshRatesFromAsync (backfill range); ResolveConflictAsync
│   │   │   │   └── Contracts/
│   │   │   │       ├── IRabbitMQService.cs
│   │   │   │       ├── ILookupCacheService.cs — GetIdAsync<T>(name) / GetNameAsync<T>(id)
│   │   │   │       ├── ICategoryService.cs  — GetAllAsync() → active category tree
│   │   │   │       ├── ICurrencyService.cs  — GetAllAsync() → all currencies
│   │   │   │       ├── IExpenseService.cs   — AddAsync, UpdateAsync, DeleteAsync, GetByIdAsync(id, userId, displayCurrencyId?), GetPagedAsync
│   │   │   │       ├── IExpenseAuditService.cs — WriteAddAuditAsync, WriteUpdateAuditAsync, WriteDeleteAuditAsync (all accept string tags for snapshot)
│   │   │   │       ├── IFamilyService.cs    — CreateDefaultAsync, CreateAsync, GetByUserAsync, GetByIdAsync, RenameAsync, InviteAsync, AcceptInviteAsync, RemoveMemberAsync, ChangeRoleAsync, ArchiveAsync, UnarchiveAsync
│   │   │   │       ├── ITagService.cs       — GetVisibleAsync(userId) → TagListDto; UseTagAsync(name, userId) → TagDto; RemoveTagAsync(tagId, userId) → bool
│   │   │   │       └── ICurrencyRateService.cs — ResolveRateAsync, GetRateHistoryAsync, AddManualRateAsync, BulkAddManualRatesAsync, SetDefaultFallbackAsync, ResolveConflictAsync, GetPendingConflictsAsync, RunDailyUpdateAsync, RefreshRatesFromAsync
│   │   │   └── Migrations/
│   │   │       ├── 20260217225816_InitialCreate.cs
│   │   │       ├── 20260217225816_InitialCreate.Designer.cs
│   │   │       ├── 20260505144220_SchemaFoundation.cs   — Phase 1: all domain + 8 lookup tables with seed data
│   │   │       ├── 20260505144220_SchemaFoundation.Designer.cs
│   │   │       ├── 20260505145359_LongIdsForExpenseAndAudit.cs   — bigint PKs/FKs for Expense, AuditLog, AuditSnapshot, FamilyAttribution
│   │   │       ├── 20260505145359_LongIdsForExpenseAndAudit.Designer.cs
│   │   │       ├── 20260506203552_SeedCurrencies.cs     — 154 ISO 4217 active currencies (AED→ZWG)
│   │   │       ├── 20260506203552_SeedCurrencies.Designer.cs
│   │   │       ├── 20260506204543_SeedCategories.cs     — 17 top-level + 108 subcategories (125 total)
│   │   │       ├── 20260506204543_SeedCategories.Designer.cs
│   │   │       ├── 20260506224942_AddInboxEvents.cs     — InboxEvents table (MessageId PK, index on ReceivedAt)
│   │   │       ├── 20260506224942_AddInboxEvents.Designer.cs
│   │   │       ├── 20260509155613_ReplaceCategoryFamilyIsArchivedWithSoftDelete.cs — IsDeleted + DeletedAt on Categories + Families; data-migrates IsArchived; drops IsArchived
│   │   │       ├── 20260509155613_ReplaceCategoryFamilyIsArchivedWithSoftDelete.Designer.cs
│   │   │       ├── 20260509163919_AddExpenseSoftDelete.cs — IsDeleted (default false) + DeletedAt? on Expenses
│   │   │       ├── 20260509163919_AddExpenseSoftDelete.Designer.cs
│   │   │       ├── 20260511130345_Phase4_FamilyInvitation.cs — FamilyInvitation table (token, ExpiresAt, InviteeEmail, AcceptedAt?, AcceptedByUserId?)
│   │   │       ├── 20260511130345_Phase4_FamilyInvitation.Designer.cs
│   │   │       ├── 20260516192901_AddUserTagsRefactorTags.cs — Phase 5: drops Tags.UserId FK+column; adds unique index on Tags.Name; creates UserTags (UserId+TagId PK, Cascade on user, Restrict on tag)
│   │   │       ├── 20260516192901_AddUserTagsRefactorTags.Designer.cs
│   │   │       └── ExpensesDbContextModelSnapshot.cs
│   │   └── Touir.ExpensesManager.Expenses.Tests/
│   │       ├── Touir.ExpensesManager.Expenses.Tests.csproj
│   │       ├── TestHelpers/
│   │       │   └── TestExpensesDbContext.cs  — In-memory DB wrapper for tests
│   │       ├── Controllers/
│   │       │   ├── CategoryControllerTests.cs
│   │       │   ├── CurrencyControllerTests.cs
│   │       │   ├── ExpenseControllerTests.cs        — 16 tests: 401 no-cookie × 5 endpoints, 201/400/403 create, 404/200/403 update, 404/204 delete, 404/200 getById, 200 getPaged
│   │       │   ├── FamilyControllerTests.cs         — 34+ tests: 401 no-cookie paths, all 10 family endpoints (200/201/204/403/404/409 per action) incl. LeaveAsync 401/204/403/404
│   │       │   ├── TagControllerTests.cs            — 13 tests: 401 no-cookie × 3 endpoints, GetTags 200 (list/empty/family), UseTag 200 (new/existing), RemoveTag 204/404
│   │       │   └── CurrencyRateControllerTests.cs   — 24 tests: GetHistory 200/500, AddRate 201/401/500, BulkAdd 204/401/500, SetDefault 204/401/500, GetConflicts 200/500, ResolveConflict 204/401/400/404, RefreshRates 204/401/500/filter
│   │       ├── Jobs/
│   │       │   └── RateAutoUpdateJobTests.cs        — 3 tests: Execute calls RunDailyUpdateAsync, exception does not propagate, exception logs error
│   │       ├── Messaging/
│   │       │   └── UserEventConsumerTests.cs        — 24 tests: constructor, ExecuteAsync, Dispose, OnMessageReceivedAsync (null msg, dedup, Created/Updated/Deleted/unknown/exception), HandleMessageAsync, UserEventMessage/UserEventType
│   │       ├── Repositories/
│   │       │   ├── External/
│   │       │   │   └── UserRepositoryTests.cs
│   │       │   ├── CategoryRepositoryTests.cs       — 5 tests: top-level only, children included, archived excluded, empty, archived subs
│   │       │   ├── CurrencyRepositoryTests.cs       — 4 tests: all currencies, field mapping, empty set, positive IDs
│   │       │   ├── ExpenseRepositoryTests.cs        — 8 tests: AddAsync, GetByIdAsync (owned/wrong-user/soft-deleted), SoftDeleteAsync, GetPagedAsync (excludes deleted/other-users, pagination, UpdateAsync); BuildExpense static
│   │       │   ├── FamilyRepositoryTests.cs         — family CRUD, membership, invitation, attribution, IsMemberAsync, HasDefaultFamilyAsync
│   │       │   ├── InboxRepositoryTests.cs          — 7 tests: ExistsAsync×3, AddAsync×4
│   │       │   ├── TagRepositoryTests.cs            — 16 integration tests: GetOwnAsync×3, GetFamilyAsync×4, EnsureUserTagAsync×3, RemoveUserTagAsync×2, IsVisibleAsync×4
│   │       │   └── CurrencyRateRepositoryTests.cs   — 20 integration tests: GetExact×2, GetMostRecentBefore×2, GetDefault×2, AddRate, ManualRateExists×2, AddConflict, GetPendingConflicts, SetDefault×2, GetHistory×2, UpdateRate, GetConflictById×2, UpdateConflict
│   │       ├── Infrastructure/
│   │       │   └── ExpensesDbContextSchemaTests.cs  — 23 tests: all Phase 1 entities, composite PKs, unique constraints, cascades
│   │       ├── Validators/
│   │       │   ├── CreateTagRequestValidatorTests.cs — 4 tests: valid, empty name, name too long (51 chars), exact max length (50 chars)
│   │       │   ├── ExpenseRequestValidatorTests.cs  — 13 tests: valid pass, amount/currency/date/description/subcategory rules for both Create and Update validators
│   │       │   └── FamilyValidatorTests.cs          — 15 tests: CreateFamily, RenameFamily, InviteMember (incl. email case + length), ChangeMemberRole
│   │       └── Services/
│   │           ├── RabbitMQServiceTests.cs
│   │           ├── LookupCacheServiceTests.cs       — 7 tests: GetId/Name, KeyNotFoundException, cache hit, all 8 types
│   │           ├── CategoryServiceTests.cs          — 8 tests: Mock<ICategoryRepository>; top-level, subcategories, archived exclusion, field mapping, call count
│   │           ├── CurrencyServiceTests.cs          — 5 tests: Mock<ICurrencyRepository>; all currencies, field mapping, empty set, ID mapping, call count
│   │           ├── ExpenseServiceTests.cs           — 16 tests: AddAsync (repo called, audit written, DTO amount/currency), UpdateAsync (null when not found, repo called, audit written, fields updated), DeleteAsync (false/true/soft-delete/audit), GetByIdAsync (null/mapped), GetPagedAsync (result, total pages); updated for ICurrencyRateService dependency
│   │           ├── ExpenseServiceConversionTests.cs — 5 tests: GetByIdAsync with displayCurrencyId set/same currency/no rate/not set; GetPagedAsync with conversion
│   │           ├── ExpenseAuditServiceTests.cs      — 3 tests: WriteAddAuditAsync (log + after snapshot), WriteUpdateAuditAsync (log + before+after snapshots), WriteDeleteAuditAsync (log + before snapshot)
│   │           ├── CurrencyRateServiceTests.cs      — 28 tests: ResolveRateAsync×5, AddManualRateAsync×2, BulkAdd×1, SetDefault×1, ResolveConflict×4, GetRateHistory×1, GetPendingConflicts×1, RunDailyUpdate×5, RefreshRatesFrom×7 (all/manualConflict/providerThrows/skipDest/sourceFilter/destFilter/unknownSource)
│   │           ├── TagServiceTests.cs               — 10 unit tests (Moq): GetVisibleAsync×4, UseTagAsync×4, RemoveTagAsync×2
│   │           └── FamilyServiceTests.cs            — 42+ tests: CreateDefault, Create, GetByUser, GetById, Rename, Invite, AcceptInvite, RemoveMember, ChangeRole, Archive, Unarchive (success + forbidden/not-found paths)
│   │
│   └── users/
│       ├── .config/
│       │   └── dotnet-tools.json
│       ├── .dockerignore
│       ├── .gitignore
│       ├── .gitlab-ci.yml             — Users service CI/CD pipeline
│       ├── .trivyignore
│       ├── Dockerfile
│       ├── README.md
│       ├── SonarQube.Analysis.xml
│       ├── Touir.ExpensesManager.Users.sln
│       ├── sql_database_scripts/
│       │   ├── create_database.sql
│       │   ├── create_tables.sql
│       │   └── create_user_and_privileges.sql
│       ├── Touir.ExpensesManager.Users/
│       │   ├── Program.cs             — Entry point, DI registration, migrations
│       │   ├── appsettings.json
│       │   ├── appsettings.Development.json
│       │   ├── Touir.ExpensesManager.Users.csproj
│       │   ├── Properties/
│       │   │   └── launchSettings.json
│       │   ├── Assets/EmailTemplates/
│       │   │   ├── EMAIL_VERIFICATION_TEMPLATE.html
│       │   │   └── PASSWORD_RESET_TEMPLATE.html
│       │   ├── Messaging/
│       │   │   ├── Messages/
│       │   │   │   └── UserEventMessage.cs  — Outbound event DTO + UserEventType constants (Created/Updated/Deleted)
│       │   │   └── Publishers/
│       │   │       ├── IUserEventPublisher.cs — Publish(UserEventMessage), PublishRaw(eventType, payload, messageId)
│       │   │       ├── UserEventPublisher.cs — Publishes to users.events topic exchange; sets MessageId property on AMQP message
│       │   │       └── OutboxPublisherService.cs — BackgroundService; polls MSG_OutboxEvents every 5 s; max 5 retries; calls PublishRaw then MarkPublishedAsync
│       │   ├── Infrastructure/
│       │   │   ├── UsersAppDbContext.cs      — EF Core context for users schema
│       │   │   ├── CryptographyHelper.cs     — Password hashing and HMAC utilities
│       │   │   ├── EmailHelper.cs            — Email helper: validation, template loading; delegates send to IEmailService
│       │   │   ├── SmtpEmailService.cs       — IEmailService implementation using System.Net.Mail SMTP
│       │   │   ├── EmailHTMLTemplate.cs      — HTML email template keys and variable name constants
│       │   │   ├── Contracts/
│       │   │   │   ├── ICryptographyHelper.cs
│       │   │   │   ├── IEmailHelper.cs
│       │   │   │   └── IEmailService.cs      — Abstraction for email dispatch (OCP boundary)
│       │   │   └── Options/
│       │   │       ├── AuthenticationServiceOptions.cs — VerifyEmailBaseUrl, ResetPasswordBaseUrl, EmailVerificationExpiryInHours, PasswordResetExpiryInHours
│       │   │       ├── CryptographyOptions.cs
│       │   │       ├── EmailOptions.cs
│       │   │       ├── JwtAuthOptions.cs            — SecretKey, ExpiryInMinutes, Audience, Issuer, RefreshExpiryInDays, ShortLivedRefreshExpiryInDays
│       │   │       ├── PostgresOptions.cs
│       │   │       └── RabbitMQOptions.cs
│       │   ├── Models/
│       │   │   ├── AllowedOrigin.cs
│       │   │   ├── Application.cs
│       │   │   ├── Authentication.cs
│       │   │   ├── OutboxEvent.cs               — MSG_OutboxEvents entity: Id, MessageId, EventType, Payload, CreatedAt, PublishedAt?, RetryCount, LastError?
│       │   │   ├── RefreshToken.cs              — Opaque refresh token entity (RTK_RefreshTokens)
│       │   │   ├── RequestAccess.cs
│       │   │   ├── Role.cs
│       │   │   ├── RoleRequestAccess.cs
│       │   │   ├── User.cs                      — IsDeleted + DeletedAt for soft-delete; IsDisabled for suspension; EmailValidationHashExpiresAt? for link expiry
│       │   │   └── UserRole.cs
│       │   ├── Controllers/
│       │   │   ├── AuthenticationController.cs  — Login, logout, session, refresh, auth check (token ops via IJwtTokenService)
│       │   │   ├── ControllerErrors.cs          — Shared internal static class: SERVER_ERROR, MISSING_PARAMETERS, INVALID_USERNAME_OR_PASSWORD, NO_ASSIGNED_ROLE, MISSING_TOKEN, INVALID_TOKEN, USER_NOT_FOUND, EMAIL_VERIFICATION_FAILED, SET_NEW_PASSWORD_FAILED, REQUEST_PASSWORD_RESET_FAILED, CREATE_PASSWORD_FAILED, RESET_PASSWORD_FAILED
│       │   │   ├── RegistrationController.cs    — Register, validate-email, resend-verification; error redirect appends ?email=…&app_code=…; rate limited
│       │   │   ├── PasswordController.cs        — Change-password, request-password-reset, change-password-reset; rate limited
│       │   │   ├── MessagingController.cs       — POST /messaging/replay (requeue outbox events); GET /messaging/outbox/stats; replay rate limited
│       │   │   ├── DTO/
│       │   │   │   ├── ApplicationDto.cs
│       │   │   │   ├── RoleDto.cs
│       │   │   │   └── UserDto.cs              — User DTO with FirstName, LastName, Email
│       │   │   ├── Requests/
│       │   │   │   ├── ChangePasswordRequest.cs        — Requires Email, OldPassword, NewPassword
│       │   │   │   ├── ChangePasswordResetRequest.cs
│       │   │   │   ├── CreatePasswordRequest.cs
│       │   │   │   ├── LoginRequest.cs          — Email, Password, ApplicationCode, RememberMe
│       │   │   │   ├── RegisterRequest.cs
│       │   │   │   ├── RequestPasswordResetRequest.cs
│       │   │   │   └── ResendVerificationRequest.cs    — Email, ApplicationCode (resend verification link)
│       │   │   └── Responses/
│       │   │       ├── ErrorResponse.cs
│       │   │       ├── LoginResponse.cs        — Returns User (UserDto) and Roles (token is cookie-only)
│       │   │       ├── SessionResponse.cs      — Returns Email, FirstName, LastName from JWT claims
│       │   │       └── RegisterResponse.cs
│       │   ├── Validators/
│       │   │   ├── LoginRequestValidator.cs             — ApplicationCode, Email, Password NotEmpty → MISSING_PARAMETERS
│       │   │   ├── RegisterRequestValidator.cs          — FirstName, LastName, Email NotEmpty → MISSING_PARAMETERS
│       │   │   ├── ChangePasswordRequestValidator.cs    — Email, OldPassword NotEmpty; NewPassword NotEmpty + MinLength(8) → PASSWORD_TOO_SHORT
│       │   │   ├── ChangePasswordResetRequestValidator.cs — Email, VerificationHash NotEmpty; NewPassword NotEmpty + MinLength(8)
│       │   │   ├── CreatePasswordRequestValidator.cs    — Email, VerificationHash NotEmpty; NewPassword NotEmpty + MinLength(8)
│       │   │   ├── RequestPasswordResetRequestValidator.cs — Email, AppCode NotEmpty → MISSING_PARAMETERS
│       │   │   └── ResendVerificationRequestValidator.cs — Email, ApplicationCode NotEmpty → MISSING_PARAMETERS
│       │   ├── Repositories/
│       │   │   ├── ApplicationRepository.cs
│       │   │   ├── AuthenticationRepository.cs
│       │   │   ├── OutboxRepository.cs          — EnqueueAsync, GetPendingAsync, MarkPublishedAsync, MarkFailedAsync, RequeueAsync
│       │   │   ├── RefreshTokenRepository.cs    — CRUD for RTK_RefreshTokens
│       │   │   ├── RoleRepository.cs
│       │   │   ├── UserRepository.cs
│       │   │   └── Contracts/
│       │   │       ├── IApplicationRepository.cs
│       │   │       ├── IAuthenticationRepository.cs
│       │   │       ├── IOutboxRepository.cs     — EnqueueAsync, GetPendingAsync, MarkPublishedAsync, MarkFailedAsync, RequeueAsync
│       │   │       ├── IRefreshTokenRepository.cs
│       │   │       ├── IRoleRepository.cs
│       │   │       └── IUserRepository.cs          — includes UpdateEmailValidationHashAsync(userId, newHash, expiresAt)
│       │   ├── Services/
│       │   │   ├── ApplicationService.cs
│       │   │   ├── AuthenticationService.cs        — Credential verification only (AuthenticateAsync)
│       │   │   ├── JwtTokenService.cs              — JWT generation (claims: sub, email, givenName, surname, jti) and validation
│       │   │   ├── RabbitMQService.cs              — Singleton RabbitMQ connection
│       │   │   ├── RegistrationService.cs          — Registration, email validation, resend verification; re-register with unverified email silently resends; hash expiry 24 h
│       │   │   ├── ResendResult.cs                 — Enum: Sent, NotFound
│       │   │   ├── PasswordManagementService.cs    — Change password, reset password, request password reset
│       │   │   ├── RefreshTokenService.cs          — Generates and validates opaque refresh tokens (DB-backed)
│       │   │   ├── RoleService.cs
│       │   │   ├── UserRoleAssignmentService.cs    — Assigns default application role to a newly registered user
│       │   │   └── Contracts/
│       │   │       ├── IApplicationService.cs
│       │   │       ├── IAuthenticationService.cs
│       │   │       ├── IJwtTokenService.cs
│       │   │       ├── IPasswordManagementService.cs
│       │   │       ├── IRabbitMQService.cs
│       │   │       ├── IRefreshTokenService.cs
│       │   │       ├── IRegistrationService.cs     — includes ResendVerificationEmailAsync(email, appCode)
│       │   │       ├── IRoleService.cs
│       │   │       └── IUserRoleAssignmentService.cs
│       │   └── Migrations/
│       │       ├── 20251227165426_InitialCreate.cs
│       │       ├── 20251231180932_SeedInitialData.cs
│       │       ├── 20251231182927_AddAllowedOrigin.cs
│       │       ├── 20260101165439_AddIsDefaultColumnRole.cs
│       │       ├── 20260101171739_SetDefaultRoles.cs
│       │       ├── 20260101174904_SetResetPasswordUrlApplication.cs
│       │       ├── 20260323120000_UpdateApplicationUrls.cs — Updates APP_UrlPath and APP_ResetPasswordUrlPath from localhost:5173 to localhost (nginx)
│       │       ├── 20260412165435_FixResetPasswordUrl.cs — Sets APP_ResetPasswordUrlPath to host-agnostic relative path /reset-password
│       │       ├── 20260427220653_AddRefreshTokens.cs — Creates RTK_RefreshTokens table
│       │       ├── 20260429200824_AddVerifyEmailErrorUrlPath.cs — Adds APP_VerifyEmailErrorUrlPath; seeds /verify-error for EXPENSES_MANAGER app
│       │       ├── 20260506224929_AddOutboxEvents.cs — MSG_OutboxEvents table (bigint PK); unique index on MessageId; composite index on (PublishedAt, RetryCount)
│       │       ├── 20260506224929_AddOutboxEvents.Designer.cs
│       │       ├── 20260509140937_AddUserSoftDelete.cs — USR_IsDeleted + USR_DeletedAt columns; partial unique index ux_usr_email_active (email unique among non-deleted)
│       │       ├── 20260509140937_AddUserSoftDelete.Designer.cs
│       │       ├── 20260510122007_AddEmailValidationHashExpiry.cs — USR_EmailValidationHashExpiresAt nullable DateTime column on USR_Users
│       │       ├── 20260510122007_AddEmailValidationHashExpiry.Designer.cs
│       │       └── UsersAppDbContextModelSnapshot.cs
│       └── Touir.ExpensesManager.Users.Tests/
│           ├── Touir.ExpensesManager.Users.Tests.csproj
│           ├── TestHelpers/
│           │   ├── TestDbContextWrapper.cs          — SQLite in-memory + Migrate() wrapper (int/short PK entities)
│           │   └── TestDbContextEnsureCreated.cs    — SQLite in-memory + EnsureCreated() wrapper; used for long PK entities (OutboxEvent) where Npgsql annotation would force BIGINT
│           ├── Controllers/
│           │   ├── AuthenticationControllerTests.cs
│           │   ├── MessagingControllerTests.cs      — 6 tests: Replay×4, Stats×2
│           │   ├── PasswordControllerTests.cs
│           │   └── RegistrationControllerTests.cs   — includes 4 ResendVerification tests
│           ├── Messaging/
│           │   ├── UserEventPublisherTests.cs       — 15 tests: Publish (serialisation, unique MessageId, channel lifecycle, properties), PublishRaw (exchange declare, UTF-8, MessageId, all event types)
│           │   └── OutboxPublisherServiceTests.cs   — 9 tests: constructor, ExecuteAsync cancellation, ProcessPendingAsync (no events, single, multiple, publish failure → MarkFailed, mixed, max-retries=5, exception propagation)
│           ├── Validators/
│           │   ├── LoginRequestValidatorTests.cs
│           │   ├── RegisterRequestValidatorTests.cs
│           │   ├── ChangePasswordRequestValidatorTests.cs
│           │   ├── ChangePasswordResetRequestValidatorTests.cs
│           │   ├── CreatePasswordRequestValidatorTests.cs
│           │   ├── RequestPasswordResetRequestValidatorTests.cs
│           │   └── ResendVerificationRequestValidatorTests.cs — 3 tests: email empty, appCode empty, valid
│           ├── Infrastructure/
│           │   ├── CryptographyHelperTests.cs
│           │   ├── EmailHelperTests.cs
│           │   └── SmtpEmailServiceTests.cs
│           ├── Repositories/
│           │   ├── ApplicationRepositoryTests.cs
│           │   ├── AuthenticationRepositoryTests.cs
│           │   ├── OutboxRepositoryTests.cs         — 15 tests: EnqueueAsync×2, GetPendingAsync×3, MarkPublishedAsync×2, MarkFailedAsync×4, RequeueAsync×4
│           │   ├── RefreshTokenRepositoryTests.cs
│           │   ├── RoleRepositoryTests.cs
│           │   └── UserRepositoryTests.cs           — includes UpdateEmailValidationHashAsync×4 + ValidateEmailAsync expiry×3
│           └── Services/
│               ├── ApplicationServiceTests.cs
│               ├── AuthenticationServiceTests.cs
│               ├── JwtTokenServiceTests.cs
│               ├── PasswordManagementServiceTests.cs
│               ├── RefreshTokenServiceTests.cs
│               ├── RegistrationServiceTests.cs      — includes ResendVerificationEmailAsync×6
│               ├── RoleServiceTests.cs
│               └── UserRoleAssignmentServiceTests.cs
│
├── frontend/
│   └── dashboard/
│       ├── .env                       — Local env vars (gitignored)
│       ├── .env.example               — Env vars template
│       ├── .gitignore
│       ├── .gitlab-ci.yml             — Frontend CI/CD pipeline
│       ├── README.md
│       ├── index.html                 — HTML entry point
│       ├── package.json
│       ├── package-lock.json
│       ├── postcss.config.cjs         — PostCSS pipeline for Tailwind
│       ├── setupTests.ts              — Vitest global test setup
│       ├── sonar-project.properties   — SonarQube project settings
│       ├── tailwind.config.ts         — Hearth design system tokens (brand/surface/ink/sage/berry/mustard palette, custom fonts, shadows)
│       ├── tsconfig.json
│       ├── tsconfig.app.json
│       ├── tsconfig.node.json
│       ├── vite.config.ts             — Vite bundler config with @ alias
│       └── vitest.config.ts           — Vitest test runner config
│       └── src/
│           ├── main.tsx               — React app mount point
│           ├── App.tsx                — Root component: providers composition
│           ├── router.tsx             — All <Routes> definitions
│           ├── env.d.ts               — Vite env type declarations
│           ├── vitest.d.ts            — Vitest type declarations
│           ├── components/            — Shared UI primitives (generic, cross-feature)
│           │   ├── BackLink.tsx        — Back-arrow link with chevron SVG
│           │   ├── FieldError.tsx      — Per-field error paragraph with role="alert"
│           │   ├── LanguageSwitcher.tsx — Language selector dropdown wired to i18n.changeLanguage
│           │   ├── PasswordInput.tsx   — Password input with show/hide toggle
│           │   ├── PasswordStrength.tsx — Live password strength indicator (5-segment bar + checklist)
│           │   ├── SubmitButton.tsx    — Submit button with spinner SVG and configurable labels
│           │   ├── Toast.tsx           — Toast notification provider and hook
│           │   └── __tests__/
│           │       ├── LanguageSwitcher.test.tsx
│           │       ├── PasswordInput.test.tsx
│           │       ├── PasswordStrength.test.tsx
│           │       └── Toast.test.tsx
│           ├── i18n/                  — Internationalisation (react-i18next)
│           │   ├── index.ts           — i18next singleton config; language detection via localStorage → navigator
│           │   ├── locales/
│           │   │   ├── en/translation.json
│           │   │   ├── fr/translation.json
│           │   │   ├── es/translation.json
│           │   │   └── de/translation.json
│           │   └── __tests__/
│           │       └── i18n.test.ts   — Verifies supported languages, fallback, resources, interpolation config
│           ├── constants/             — App-wide typed constants (used by shared services)
│           │   └── apiErrors.constant.ts — API_ERRORS + BACKEND_ERROR_CODES
│           ├── features/
│           │   ├── auth/              — Authentication feature
│           │   │   ├── components/
│           │   │   │   ├── AuthBrandPanel.tsx   — Terracotta gradient brand panel for split-screen auth layout (hidden on mobile)
│           │   │   │   ├── AuthCard.tsx         — Wraps auth pages in auth-page/auth-card divs
│           │   │   │   ├── AuthPageHeader.tsx   — Page title + subtitle header
│           │   │   │   ├── EmailField.tsx       — Shared email input field for auth forms
│           │   │   │   ├── ProtectedRoute.tsx   — Redirects unauthenticated users to /login
│           │   │   │   └── PublicOnlyRoute.tsx  — Redirects authenticated users to /dashboard
│           │   │   ├── pages/
│           │   │   │   ├── LoginPage.tsx
│           │   │   │   ├── RegisterPage.tsx
│           │   │   │   ├── ChangePasswordPage.tsx
│           │   │   │   ├── ResetPasswordPage.tsx
│           │   │   │   ├── RequestPasswordResetPage.tsx
│           │   │   │   └── __tests__/
│           │   │   │       ├── LoginPage.test.tsx
│           │   │   │       ├── RegisterPage.test.tsx
│           │   │   │       ├── ChangePasswordPage.test.tsx
│           │   │   │       ├── ResetPasswordPage.test.tsx
│           │   │   │       └── RequestPasswordResetPage.test.tsx
│           │   │   ├── services/
│           │   │   │   ├── authApi.service.ts   — Auth HTTP functions (login, logout, register, create/change/reset password)
│           │   │   │   └── __tests__/
│           │   │   │       └── authApi.service.test.ts
│           │   │   ├── types/
│           │   │   │   └── auth.type.ts         — User, AuthResult, AuthContextValue
│           │   │   ├── AuthContext.tsx           — Cookie-based auth state; session restored via GET /auth/session (falls back to POST /auth/refresh); no localStorage/sessionStorage
│           │   │   ├── auth.schemas.ts           — Zod schemas and inferred types for all five auth forms
│           │   │   └── __tests__/
│           │   │       ├── AuthContext.test.tsx
│           │   │       ├── ProtectedRoute.test.tsx
│           │   │       ├── PublicOnlyRoute.test.tsx
│           │   │       └── authApi.service.test.ts
│           │   ├── families/          — Family management feature
│           │   │   ├── components/
│           │   │   │   ├── FamilySelector.tsx   — NavBar dropdown to switch active family scope; hidden when no non-default active families
│           │   │   │   └── __tests__/
│           │   │   │       └── FamilySelector.test.tsx
│           │   │   ├── pages/
│           │   │   │   ├── FamiliesPage.tsx      — Family management screen: active/archived tabs, cards, create/rename/archive/invite modals, inline member panel
│           │   │   │   └── __tests__/
│           │   │   │       └── FamiliesPage.test.tsx
│           │   │   ├── services/
│           │   │   │   └── familyApi.service.ts  — All family CRUD + invitation + member management calls
│           │   │   ├── types/
│           │   │   │   └── family.type.ts        — Family, FamilyDetail, FamilyMember, FamilyRole
│           │   │   ├── FamilyContext.tsx          — FamilyProvider / useFamilies(); loads list on auth, persists activeFamilyId to localStorage
│           │   │   ├── family.schemas.ts          — Zod schemas for create-family and invite-member forms
│           │   │   └── __tests__/
│           │   │       ├── FamilyContext.test.tsx
│           │   │       ├── family.schemas.test.ts
│           │   │       └── familyApi.service.test.ts
│           │   ├── tags/              — Tag management feature (Phase 5; wiring into expense form deferred to Phase 8)
│           │   │   ├── types/
│           │   │   │   └── tag.type.ts          — Tag { id, name }, TagList { own: Tag[], family: Tag[] }
│           │   │   ├── services/
│           │   │   │   └── tagsApi.service.ts   — getTags() → GET /api/expenses/tags; useTag(name) → POST; removeTag(id) → DELETE
│           │   │   └── components/
│           │   │       ├── TagInput.tsx          — Combobox: grouped "My tags"/"Family tags" dropdown, chips, create option, keyboard (Enter/Escape/Backspace)
│           │   │       └── __tests__/
│           │   │           └── TagInput.test.tsx — 17 component tests (role queries updated to menu/menuitem; added getTags/useTag error-path and Enter-key tests)
│           │   ├── currencies/        — Display currency feature (Phase 6)
│           │   │   ├── components/
│           │   │   │   ├── DisplayCurrencySelector.tsx — NavBar dropdown; reads currencies from ExpensesDataContext; writes to DisplayCurrencyContext; "No conversion" option
│           │   │   │   └── __tests__/
│           │   │   │       └── DisplayCurrencySelector.test.tsx — 13 tests: renders/no currencies/label/open/select/clear/close/aria-checked/search-input/filter-by-code/filter-by-name/clear-on-close
│           │   │   ├── services/
│           │   │   │   ├── ratesApi.service.ts — refreshRates(RefreshRatesParams) → POST /api/expenses/rates/refresh (204); params: from, sourceCurrencyId?, destinationCurrencyId?
│           │   │   │   └── __tests__/
│           │   │   │       └── ratesApi.service.test.ts — 8 tests: from-only, sourceCurrencyId, destinationCurrencyId, both, success, error, 401, call count
│           │   │   ├── DisplayCurrencyContext.tsx — Session-only state (number | null, default null); no localStorage
│           │   │   └── __tests__/
│           │   │       └── DisplayCurrencyContext.test.tsx — 5 tests: default null, set/clear, persist on rerender, throws outside provider
│           │   ├── expenses/          — Expense management feature
│           │   │   ├── types/
│           │   │   │   └── expenses.type.ts     — Category, Subcategory, Currency types
│           │   │   ├── services/
│           │   │   │   ├── categoriesApi.service.ts — getCategories() → GET /api/expenses/categories
│           │   │   │   ├── currenciesApi.service.ts — getCurrencies() → GET /api/expenses/currencies
│           │   │   │   └── __tests__/
│           │   │   │       ├── categoriesApi.service.test.ts
│           │   │   │       └── currenciesApi.service.test.ts
│           │   │   ├── ExpensesDataContext.tsx  — ExpensesDataProvider / useExpensesData(); fetches categories + currencies on mount
│           │   │   └── __tests__/
│           │   │       └── ExpensesDataContext.test.tsx
│           │   ├── dashboard/         — Authenticated dashboard feature
│           │   │   └── pages/
│           │   │       ├── HomeDashboardPage.tsx — Dashboard home; shows user greeting and cards
│           │   │       ├── SettingsPage.tsx       — Settings hub; password card (link to /change-password)
│           │   │       └── __tests__/
│           │   │           ├── HomeDashboardPage.test.tsx
│           │   │           └── SettingsPage.test.tsx
│           │   └── public/            — Public (unauthenticated) pages
│           │       └── pages/
│           │           ├── HomePublicPage.tsx    — Public landing page
│           │           ├── NotFoundPage.tsx      — 404 page; shown for any unmatched route
│           │           ├── VerifyErrorPage.tsx   — Friendly error page for expired/used email verification links (/verify-error)
│           │           └── __tests__/
│           │               ├── HomePublicPage.test.tsx
│           │               ├── NotFoundPage.test.tsx
│           │               └── VerifyErrorPage.test.tsx
│           ├── providers/             — Composed provider tree
│           │   ├── AppProviders.tsx   — Nests AuthProvider → ExpensesDataProvider → FamilyProvider → DisplayCurrencyProvider
│           │   └── __tests__/
│           │       └── AppProviders.test.tsx
│           ├── hooks/                 — Shared hooks
│           │   └── usePageTitle.ts    — Sets document.title per page
│           ├── layouts/               — App-wide layout components
│           │   ├── NavBar.tsx          — Auth-aware nav; desktop + mobile responsive; right-side controls: FamilySelector → DisplayCurrencySelector → notification bell → user avatar dropdown (cog Settings, labeled LanguageSwitcher, logout-icon Sign out); unauthenticated desktop nav also shows LanguageSwitcher
│           │   └── __tests__/
│           │       └── NavBar.test.tsx
│           ├── services/              — Shared base services
│           │   ├── api.service.ts     — Base fetch wrapper with cookie auth, transparent refresh-and-retry on 401, and skipUnauthorized option
│           │   └── __tests__/
│           │       ├── api.service.test.ts     — Full coverage: normal flows, 401 refresh-and-retry, deduplication, skipUnauthorized, network errors
│           │       ├── api.service.env.test.ts — Isolated env tests: VITE_API_BASE prefix and trailing-slash strip
│           │       └── api.test.ts
│           ├── styles/
│           │   └── index.css          — Tailwind directives + @layer components
│           └── types/                 — Shared TypeScript type definitions
│               └── api.type.ts         — ApiResponse<T>
│
├── infrastructure/
│   ├── .env                           — Local infrastructure env vars (gitignored)
│   ├── .env.example
│   ├── README.md
│   ├── docker-compose-apps.yml        — App stack: nginx, users-service, expenses-service
│   ├── docker-compose-tools.yml       — Tool stack: PostgreSQL, RabbitMQ, Grafana, Prometheus, SonarQube, Mailpit, Nexus
│   ├── run-docker-compose-apps.bat    — Start app containers (Windows)
│   ├── run-docker-compose-tools.bat   — Start tool containers (Windows)
│   ├── start-expenses-manager-apps.bat
│   ├── configs/
│   │   ├── gitlab-ci-templates/       — Reusable CI/CD job templates
│   │   │   ├── ci-init.yml
│   │   │   ├── ci-stages.yml
│   │   │   ├── ci-build.yml
│   │   │   ├── ci-test.yml
│   │   │   ├── ci-quality.yml
│   │   │   ├── ci-docker.yml
│   │   │   ├── ci-docker-security.yml
│   │   │   ├── ci-security.yml
│   │   │   └── ci-deploy.yml
│   │   ├── nginx/
│   │   │   ├── nginx.conf             — Main nginx config
│   │   │   ├── fastcgi_params / scgi_params / uwsgi_params / mime.types
│   │   │   ├── entrypoint.sh          — Docker entrypoint; injects env vars into config
│   │   │   ├── sites-available/
│   │   │   │   └── expenses-manager.conf  — Route mapping and auth subrequest config
│   │   │   └── ssl/
│   │   │       ├── ssl.conf
│   │   │       ├── expenses-manager.crt
│   │   │       └── expenses-manager.key
│   │   ├── prometheus/
│   │   │   └── prometheus.yml         — Scrape targets config
│   │   ├── rabbitmq/
│   │   │   ├── rabbitmq.conf
│   │   │   ├── enabled_plugins
│   │   │   └── management_definitions.json
│   │   ├── nexus/
│   │   │   ├── Dockerfile             — Custom Nexus image: apk installs bash+curl+jq, bakes in scripts
│   │   │   ├── docker-entrypoint.sh   — Wrapper: launches provision.sh in background, execs nexus
│   │   │   ├── provision.sh           — Provisioning: waits for Nexus, changes admin password, creates repos+CI user from repos.json
│   │   │   └── repos.json             — Repo definitions (docker/npm/nuget proxies and groups)
│   │   └── sql_database_scripts/
│   │       ├── gitlab/
│   │       │   ├── create_database.sql
│   │       │   └── create_user_and_privileges.sql
│   │       └── sonar/
│   │           ├── create_database.sql
│   │           └── create_user_and_privileges.sql
│   ├── jobs/
│   │   ├── Dockerfile
│   │   ├── crontab                    — Scheduled job definitions
│   │   ├── supervisord.conf           — Supervisor daemon config
│   │   ├── configs/                   — Per-job configuration files
│   │   └── scripts/
│   │       ├── cron-runner.sh
│   │       ├── docker-updater-runner.sh
│   │       ├── docker_image_updater_api.py  — Zero-downtime Docker image update API
│   │       ├── github-gitlab-sync.sh
│   │       ├── minio-sync.sh
│   │       └── registry_proxy.py      — Docker registry proxy
│   └── scripts/
│       └── gitlab-register-runner.sh  — GitLab Runner registration helper
│
└── mobile/
    └── README.md                      — Mobile app placeholder
```
