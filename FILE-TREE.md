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
│           ├── nexus-proxy-integration.md  — Completed plan: Nexus Repository Manager integration
│           ├── phase-13-notifications.md   — Completed plan: notifications microservice (SignalR, RabbitMQ, email, v0.112.x)
│           ├── phase-14-mobile-app.md      — Completed plan: Ionic + Capacitor mobile app (v0.113.0–v0.114.0)
│           └── upload_protection.md    — Completed plan: CSV upload security hardening (10 fixes, v0.110.8)
│
├── backend/
│   ├── dashboard/
│   │   ├── Dockerfile                 — Docker image for dashboard service
│   │   └── README.md                  — Dashboard service documentation
│   │
│   ├── notifications/
│   │   ├── .dockerignore
│   │   ├── .gitignore
│   │   ├── .gitlab-ci.yml             — Notifications service CI/CD pipeline
│   │   ├── .trivyignore               — Trivy scanner ignore list
│   │   ├── Dockerfile                 — Docker image for notifications service
│   │   ├── README.md
│   │   ├── SonarQube.Analysis.xml     — SonarQube project settings
│   │   ├── Touir.ExpensesManager.Notifications.sln
│   │   ├── Touir.ExpensesManager.Notifications/
│   │   │   ├── Program.cs             — Entry point, DI registration, migrations, SignalR hub mapping
│   │   │   ├── appsettings.json       — Logging + AllowedHosts only; service config via env vars
│   │   │   ├── appsettings.Development.json — localhost defaults for dotnet run (RabbitMQ + Postgres)
│   │   │   ├── Touir.ExpensesManager.Notifications.csproj
│   │   │   ├── Hubs/
│   │   │   │   └── NotificationHub.cs — SignalR hub at /ws/notifications; cookie auth via JwtCookieReader; groups by userId
│   │   │   ├── Messaging/
│   │   │   │   ├── Messages/
│   │   │   │   │   ├── FamilyEventMessage.cs — Inbound event DTO (family.member.removed) + FamilyEventType constants for all 8 expense event types
│   │   │   │   │   ├── FamilyNotificationMessages.cs — DTOs: FamilyInvitationEventMessage, FamilyInvitationAcceptedEventMessage, FamilyMemberJoinedEventMessage, FamilyExpenseEventMessage, ImportCompletedEventMessage, RateConflictEventMessage
│   │   │   │   │   └── UserNotificationEventMessage.cs — UserNotificationEventMessage DTO + UserEventType constants (email.verification.requested, password.reset.requested, password.changed)
│   │   │   │   └── Consumers/
│   │   │   │       ├── FamilyEventConsumer.cs — BackgroundService; queue notifications.expenses.sync; TWO bindings: family.# + expenses.#; deserializes to BaseEventEnvelope first, then switches on EventType → specific DTO handler; handles 8 event types; inbox deduplication; protected OnMessageReceivedAsync + virtual Ack/Nack
│   │   │   │       └── UserNotificationEventConsumer.cs — BackgroundService; queue notifications.users.email; binding user.#; routes email-only events (verification/reset/password-changed) to INotificationService; inbox deduplication; same protected/virtual pattern
│   │   │   ├── Assets/
│   │   │   │   └── EmailTemplates/
│   │   │   │       ├── FAMILY_MEMBER_REMOVED_TEMPLATE.html — placeholders @@REMOVED_BY_NAME@@ @@FAMILY_NAME@@ @@EXPENSE_COUNT@@ @@YEAR@@
│   │   │   │       ├── FAMILY_INVITATION_TEMPLATE.html — placeholders @@INVITER_NAME@@ @@FAMILY_NAME@@ @@INVITE_LINK@@ @@YEAR@@
│   │   │   │       ├── FAMILY_INVITATION_ACCEPTED_TEMPLATE.html — placeholders @@ACCEPTOR_NAME@@ @@FAMILY_NAME@@ @@YEAR@@
│   │   │   │       ├── EMAIL_VERIFICATION_TEMPLATE.html — placeholder @@VERIFICATION_LINK@@ @@YEAR@@
│   │   │   │       ├── PASSWORD_RESET_TEMPLATE.html — placeholder @@RESET_LINK@@ @@YEAR@@
│   │   │   │       └── PASSWORD_CHANGED_TEMPLATE.html — placeholder @@FIRST_NAME@@ @@YEAR@@
│   │   │   ├── Infrastructure/
│   │   │   │   ├── EmailHelper.cs           — Template loading + email dispatch
│   │   │   │   ├── EmailHtmlTemplate.cs     — Template key + variable constants for all 6 email types (FamilyMemberRemoved, FamilyInvitation, FamilyInvitationAccepted, EmailVerification, PasswordReset, PasswordChanged)
│   │   │   │   ├── JwtCookieReader.cs       — Decodes auth_token cookie to extract sub claim
│   │   │   │   ├── NotificationsDbContext.cs — EF Core context; Notifications + InboxEvents; IsNpgsql() guards for IdentityAlwaysColumn + partial index
│   │   │   │   ├── SmtpEmailService.cs      — SMTP email sender
│   │   │   │   ├── Contracts/
│   │   │   │   │   ├── IEmailHelper.cs
│   │   │   │   │   └── IEmailService.cs
│   │   │   │   └── Options/
│   │   │   │       ├── EmailOptions.cs      — SMTP config; env prefix EXPENSES_MANAGEMENT_NOTIFICATIONS_EMAILAUTH_*
│   │   │   │       ├── PostgresOptions.cs   — Server/Port/UserName/Password/Database + computed ConnectionString
│   │   │   │       └── RabbitMQOptions.cs
│   │   │   ├── Controllers/
│   │   │   │   ├── NotificationController.cs — GET /notifications, GET /notifications/unread-count, POST /notifications/{id}/read, POST /notifications/read-all, POST /notifications/push-token (Phase 14 stub)
│   │   │   │   ├── DTO/
│   │   │   │   │   └── NotificationDto.cs   — Id, Type, Payload: JsonElement, IsRead, CreatedAt, ReadAt?
│   │   │   │   └── Responses/
│   │   │   │       └── ErrorResponse.cs
│   │   │   ├── Models/
│   │   │   │   ├── InboxEvent.cs            — MessageId (PK), EventType, ReceivedAt, Status, Error?
│   │   │   │   └── Notification.cs          — Id (bigserial), UserId, Type, Payload (JSON), IsRead, CreatedAt, ReadAt?; NotificationType constants: FAMILY_MEMBER_REMOVED, FAMILY_INVITATION_ACCEPTED, FAMILY_MEMBER_JOINED, FAMILY_EXPENSE_ADDED, FAMILY_EXPENSE_DELETED, CSV_IMPORT_COMPLETED, RATE_CONFLICT_CREATED
│   │   │   ├── Migrations/
│   │   │   ├── Repositories/
│   │   │   │   ├── Contracts/
│   │   │   │   │   ├── IInboxRepository.cs
│   │   │   │   │   └── INotificationRepository.cs
│   │   │   │   ├── InboxRepository.cs
│   │   │   │   └── NotificationRepository.cs
│   │   │   └── Services/
│   │   │       ├── Contracts/
│   │   │       │   ├── INotificationService.cs — 11 handler methods: HandleFamilyMember*, HandleFamilyInvitation*, HandleFamilyExpense*, HandleImportCompleted*, HandleRateConflict*, HandleEmailVerification*, HandlePasswordReset*, HandlePasswordChanged*
│   │   │       │   └── IRabbitMQService.cs
│   │   │       ├── NotificationService.cs   — Implements all 11 handlers; pattern: persist DB (non-fatal) → SignalR push (non-fatal) → email (non-fatal); email-only handlers skip DB+push; fan-out handlers loop per recipient with independent try/catch
│   │   │       └── RabbitMQService.cs       — Singleton; lazy connection; DispatchConsumersAsync = true
│   │   └── Touir.ExpensesManager.Notifications.Tests/
│   │       ├── Touir.ExpensesManager.Notifications.Tests.csproj
│   │       ├── coverage.runsettings
│   │       ├── Controllers/
│   │       │   └── NotificationControllerTests.cs — 12 tests: 401 (no JWT), 200/204 (success), 400 (exception) per endpoint
│   │       ├── Infrastructure/
│   │       │   └── JwtCookieReaderTests.cs — 8 tests: cookie, Bearer header, null, invalid JWT, missing/non-int sub
│   │       ├── TestHelpers/
│   │       │   └── TestNotificationsDbContext.cs — SQLite in-memory wrapper (EnsureCreated)
│   │       ├── Messaging/
│   │       │   └── FamilyEventConsumerTests.cs — Tests via TestableConsumer for all 8 event types (valid dispatch, dedup, failure, unknown type)
│   │       ├── Repositories/
│   │       │   ├── InboxRepositoryTests.cs — ExistsAsync, AddAsync
│   │       │   └── NotificationRepositoryTests.cs — Integration tests
│   │       └── Services/
│   │           └── NotificationServiceTests.cs — 48 unit tests (Moq) for all handlers including push/email failure paths
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
│   │   │   │   │   ├── UserEventMessage.cs  — Inbound event DTO + UserEventType constants (Created/Updated/Deleted)
│   │   │   │   │   └── FamilyEventMessage.cs — Outbound event DTO + FamilyEventType constants for all 8 event types (MemberRemoved, InvitationRequested, InvitationAccepted, MemberJoined, ExpenseAdded, ExpenseDeleted, ImportCompleted, RateConflict)
│   │   │   │   ├── Publishers/
│   │   │   │   │   ├── IFamilyEventPublisher.cs — Publish(FamilyEventMessage) + PublishRaw(eventType, json, messageId)
│   │   │   │   │   ├── FamilyEventPublisher.cs  — Publishes to expenses.events topic exchange; Publish delegates to PublishRaw
│   │   │   │   │   └── FamilyOutboxPublisherService.cs — BackgroundService; polls OutboxEvents every 5 s; max 5 retries; calls IFamilyEventPublisher.PublishRaw
│   │   │   │   └── Consumers/
│   │   │   │       └── UserEventConsumer.cs — BackgroundService; binds expenses.users.sync → users.events; inbox deduplication via IInboxRepository.ExistsAsync; calls IUserRepository.SaveOrUpdateUserAsync / DeleteUserAsync
│   │   │   ├── Filters/
│   │   │   │   └── AppAdminAttribute.cs     — Action filter; calls JwtCookieReader.GetIsAdmin; returns 403 when isAdmin=false
│   │   │   ├── Infrastructure/
│   │   │   │   ├── ExpensesDbContext.cs     — EF Core context; all DbSets with full Fluent API config; includes OutboxEvents
│   │   │   │   ├── JwtCookieReader.cs       — Decodes auth_token cookie (base64url payload) to extract sub/isAdmin claims; falls back to Authorization: Bearer header when cookie absent (Swagger)
│   │   │   │   ├── FrankfurterRateProvider.cs — [ExcludeFromCodeCoverage] Calls api.frankfurter.app (ECB, no API key); single-date and range endpoints; registered via IHttpClientFactory
│   │   │   │   ├── Contracts/
│   │   │   │   │   └── IRateProvider.cs     — FetchRatesAsync(code, date) → dict; FetchRatesRangeAsync(code, from, to) → dict<DateOnly, dict>
│   │   │   │   └── Options/
│   │   │   │       ├── CurrencyRateOptions.cs — UpdateTime (default 02:00 UTC); env prefix EXPENSES_MANAGEMENT_EXPENSES_CURRENCYRATE_*
│   │   │   │       ├── FamilyOptions.cs     — InviteExpiryInDays + InviteBaseUrl; env prefix EXPENSES_MANAGEMENT_EXPENSES_FAMILY_*
│   │   │   │       ├── PostgresOptions.cs
│   │   │   │       └── RabbitMQOptions.cs
│   │   │   ├── Controllers/
│   │   │   │   ├── AdminCategoryController.cs — POST/PUT /admin/categories(/{id}), POST /admin/categories/{id}/archive|unarchive|subcategories, PUT/POST /admin/categories/{id}/subcategories/{subId}; all [AppAdmin]
│   │   │   │   ├── AdminCurrencyController.cs — POST /admin/currencies (201), PUT /{id} (200/404), DELETE /{id} (204/409), GET /{id}/defaults (200), POST /defaults (204); all [AppAdmin]
│   │   │   │   ├── AdminRateController.cs   — GET /admin/rates/history, POST /admin/rates (201), POST /admin/rates/bulk (204), PUT /admin/rates/default (204), GET /admin/rates/conflicts, POST /admin/rates/conflicts/{id}/resolve (204), POST /admin/rates/refresh (204); all [AppAdmin]
│   │   │   │   ├── CategoryController.cs    — GET /categories → IEnumerable<CategoryDto>
│   │   │   │   ├── ControllerErrors.cs      — Shared internal static class: SERVER_ERROR, UNAUTHORIZED, EXPENSE_NOT_FOUND, MISSING_PARAMETERS, TAG_NOT_FOUND, RATE_NOT_FOUND, CONFLICT_NOT_FOUND, INVALID_MONTH, IMPORT_NO_FILE, IMPORT_FILE_TOO_LARGE, INVALID_FILE_TYPE, INVALID_FILE_CONTENT, IMPORT_TIMEOUT
│   │   │   │   ├── CurrencyController.cs    — GET /currencies → IEnumerable<CurrencyDto>
│   │   │   │   ├── DashboardController.cs   — 6 GET /dashboard/* endpoints: summary, monthly, categories, same-month-across-years, by-currency, recent; default date ranges computed in controller; FamilyForbiddenException → 403
│   │   │   │   ├── ExpenseController.cs     — POST/PUT/DELETE/GET/GET(paged) /expenses; GetByIdAsync accepts ?displayCurrencyId; FamilyForbiddenException → 403 on create/update
│   │   │   │   ├── ExpenseImportController.cs — POST /import/preview (IFormFile → CsvImportPreviewDto; validates ext+MIME+size; copies to MemoryStream; 30s timeout), POST /import/validate-rows (RawCsvRowDto[] → CsvImportPreviewDto; 30s timeout; validated by ValidateRowsRequestValidator), POST /import/confirm (bulk insert), GET /import/template (CSV download)
│   │   │   │   ├── FamilyController.cs      — 10 endpoints: list, detail, create, rename, archive, unarchive, invite, accept-invite, remove-member, change-role
│   │   │   │   ├── TagController.cs         — GET /tags → TagListDto; POST /tags → TagDto (idempotent); DELETE /tags/{id} → 204 or 404
│   │   │   │   ├── UserConfigController.cs  — GET /config → UserConfigDto (null fields if no row); PUT /config → UserConfigDto (upsert; 400 on invalid currencyId)
│   │   │   │   ├── DTO/
│   │   │   │   │   ├── AdminCategoryDto.cs  — Id, Name, Description?, IsArchived, Subcategories: IEnumerable<AdminCategoryDto>
│   │   │   │   │   ├── CategoryDto.cs       — Id, Name, Description?, Subcategories: IEnumerable<SubcategoryDto>
│   │   │   │   │   ├── SubcategoryDto.cs    — Id, Name, Description?, Icon? (reused for category + subcategory slots in ExpenseDto)
│   │   │   │   │   ├── UserConfigDto.cs     — DefaultCurrencyId?, DefaultCurrency?: CurrencyDto
│   │   │   │   │   ├── CurrencyDto.cs       — Id, Code, Name, Symbol, Decimals
│   │   │   │   │   ├── ExpenseDto.cs        — Id, Amount, Currency: CurrencyDto?, Date, Category: SubcategoryDto?, Subcategory: SubcategoryDto?, Description?, CreatedAt, ModifiedAt?, ModifiedFrom?, Tags: TagDto[], ConvertedAmount?: decimal, DisplayCurrency?: CurrencyDto, Families: FamilyNameDto[]
│   │   │   │   │   ├── CategoryAmountDto.cs — Category: SubcategoryDto?, Amount, ConvertedAmount?; used inside MonthlyBreakdownDto and CategoryBreakdownDto
│   │   │   │   │   ├── CategoryBreakdownDto.cs — Category: SubcategoryDto?, TotalAmount, ConvertedTotal?, Percentage, Subcategories: CategoryAmountDto[]
│   │   │   │   │   ├── CurrencyBreakdownDto.cs — Currency: CurrencyDto, TotalAmount, ConvertedAmount?, ExpenseCount
│   │   │   │   │   ├── DashboardSummaryDto.cs — TotalAmount, ConvertedTotal?, DisplayCurrency?, ExpenseCount, PreviousPeriodTotal?, ChangePercent?, TopCategory?: SubcategoryDto, TopCategoryAmount?
│   │   │   │   │   ├── ExpenseFilterDto.cs  — DateFrom?, DateTo?, CategoryId?, SubcategoryId?, CurrencyId?, AmountMin?, AmountMax?, Description?, TagIds?, DisplayCurrencyId?, FamilyId?, Page (default 1), PageSize (default 20)
│   │   │   │   │   ├── MonthlyBreakdownDto.cs — Year, Month, TotalAmount, ConvertedTotal?, ByCategory: CategoryAmountDto[]
│   │   │   │   │   ├── SameMonthYearlyDto.cs — Year, TotalAmount, ConvertedTotal?
│   │   │   │   │   ├── RateDto.cs           — SourceCurrencyId, DestinationCurrencyId, Date, Rate, RateSource
│   │   │   │   │   ├── RateConflictDto.cs   — Id, SourceCurrencyId, DestinationCurrencyId, Date, AutomaticRate, ManualRate, Status, ResolvedAt?
│   │   │   │   │   ├── TagDto.cs            — Id, Name
│   │   │   │   │   ├── TagListDto.cs        — Own: IEnumerable<TagDto>, Family: IEnumerable<TagDto>
│   │   │   │   │   ├── FamilyDto.cs         — Family response shape: Id, Name, IsDefault, IsDeleted, Members: FamilyMemberDto[]
│   │   │   │   │   ├── CsvImportPreviewDto.cs — TotalRows, ValidCount, ErrorCount, Rows: IEnumerable<CsvImportRowPreviewDto> (display values incl. FamiliesDisplay + resolved IDs + errors per row)
│   │   │   │   │   └── CsvImportResultDto.cs  — Imported, Skipped
│   │   │   │   ├── Requests/
│   │   │   │   │   ├── AdminAddCurrencyRequest.cs — Code (3 chars), Name (max 50), Symbol (max 10), Decimals; validated by AdminAddCurrencyRequestValidator
│   │   │   │   │   ├── AdminCategoryRequest.cs — Name (required, max 100), Description?; validated by AdminCategoryRequestValidator
│   │   │   │   │   ├── IExpenseRequest.cs      — Shared interface (Amount, CurrencyId, Date, CategoryId?, SubcategoryId?, Description?, TagIds?) implemented by Create + Update DTOs
│   │   │   │   │   ├── CreateExpenseRequest.cs — Amount (required), CurrencyId (required), Date (required), CategoryId?, SubcategoryId?, Description?, TagIds?
│   │   │   │   │   ├── UpdateExpenseRequest.cs — same fields as Create
│   │   │   │   │   ├── CreateTagRequest.cs     — Name (required)
│   │   │   │   │   ├── AddRateRequest.cs       — SourceCurrencyId, DestinationCurrencyId, Date, Rate (all required)
│   │   │   │   │   ├── BulkAddRatesRequest.cs  — Rates: List<AddRateRequest>
│   │   │   │   │   ├── SetDefaultRateRequest.cs — SourceCurrencyId, DestinationCurrencyId, Rate
│   │   │   │   │   ├── ResolveConflictRequest.cs — Resolution (string: AcceptAuto/KeepManual/Custom), CustomRate?
│   │   │   │   │   ├── RefreshRatesRequest.cs    — From: DateOnly (required); SourceCurrencyId?: int; DestinationCurrencyId?: int
│   │   │   │   │   ├── UpdateUserConfigRequest.cs — DefaultCurrencyId?: int
│   │   │   │   │   ├── CsvImportConfirmRequest.cs — Rows: IEnumerable<CsvImportConfirmRowDto>; CsvImportConfirmRowDto: Amount, CurrencyId, Date, CategoryId?, SubcategoryId?, Description?, TagNames?, FamilyIds?
│   │   │   │   │   └── ValidateRowsRequest.cs    — Rows: IEnumerable<RawCsvRowDto>; RawCsvRowDto: RowNumber + raw string fields (Date, Amount, CurrencyCode, Category, Subcategory, Description, Tags, Families)
│   │   │   │   └── Responses/
│   │   │   │       ├── ErrorResponse.cs     — Uniform error envelope (matches users service pattern)
│   │   │   │       └── ExpensePagedResponse.cs — Items: ExpenseDto[], TotalCount, Page, PageSize, TotalPages
│   │   │   ├── Models/
│   │   │   │   ├── OutboxEvent.cs           — Id (bigserial), MessageId, EventType, Payload, CreatedAt, PublishedAt?, RetryCount, LastError?
│   │   │   │   ├── Category.cs              — IsDeleted + DeletedAt (soft-delete); ParentCategoryId, Children; Icon? (emoji, max 50 chars)
│   │   │   │   ├── UserConfig.cs            — UserId (unique FK), DefaultCurrencyId? (FK Currencies), nav DefaultCurrency?
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
│   │   │   │   ├── AdminAddCurrencyRequestValidator.cs — Code required+3 chars, Name required+max 50, Symbol required+max 10
│   │   │   │   ├── AdminCategoryRequestValidator.cs — Name required+max 100
│   │   │   │   ├── ExpenseRequestValidatorBase.cs   — Abstract base AbstractValidator<T> where T : IExpenseRequest; holds all shared rules (amount, currency, date, description, subcategory-requires-category)
│   │   │   │   ├── UpdateUserConfigRequestValidator.cs — DefaultCurrencyId optional; if set, must be > 0
│   │   │   │   ├── CreateExpenseRequestValidator.cs — Inherits ExpenseRequestValidatorBase<CreateExpenseRequest>
│   │   │   │   ├── UpdateExpenseRequestValidator.cs — Inherits ExpenseRequestValidatorBase<UpdateExpenseRequest>
│   │   │   │   ├── CreateTagRequestValidator.cs     — Name NotEmpty + MaxLength(50)
│   │   │   │   ├── CreateFamilyRequestValidator.cs  — Name NotEmpty + MaxLength(100)
│   │   │   │   ├── RenameFamilyRequestValidator.cs  — Name NotEmpty + MaxLength(100)
│   │   │   │   ├── InviteMemberRequestValidator.cs  — Email NotEmpty + EmailAddress + MaxLength(255)
│   │   │   │   ├── ChangeMemberRoleRequestValidator.cs — Role Must be "Head" or "Member" (case-insensitive)
│   │   │   │   └── ValidateRowsRequestValidator.cs  — Rows NotNull + ≤500; per-row field MaximumLength guards (Date≤10, Amount≤30, CurrencyCode≤10, Category/Subcategory≤200, Description≤500, Tags≤1000, Families≤500)
│   │   │   ├── Repositories/
│   │   │   │   ├── CategoryRepository.cs    — GetAllActiveAsync(): top-level non-archived categories with Include(Children), AsNoTracking
│   │   │   │   ├── UserConfigRepository.cs  — GetByUserIdAsync(userId): includes DefaultCurrency nav; UpsertAsync(userId, currencyId?): insert or update then LoadAsync nav
│   │   │   │   ├── DashboardRepository.cs   — Implements IDashboardRepository; hybrid SQL/C# (WHERE in EF Core, GroupBy/Sum in C#); BaseQuery uses correlated EXISTS on ExpenseFamilyAttributions for family scoping
│   │   │   │   ├── CurrencyRepository.cs    — GetAllAsync(): all currencies, AsNoTracking
│   │   │   │   ├── ExpenseRepository.cs     — AddAsync, UpdateAsync, SoftDeleteAsync, GetByIdAsync (ownership + !IsDeleted + ExpenseTags include), GetPagedAsync (filtered + paginated, desc by date; TagIds OR filter); ClearExpenseTagsAsync, AddExpenseTagsAsync
│   │   │   │   ├── ExpensesOutboxRepository.cs — IExpensesOutboxRepository: EnqueueAsync, GetPendingAsync, MarkPublishedAsync, MarkFailedAsync
│   │   │   │   ├── FamilyRepository.cs      — family CRUD, membership CRUD, invitation CRUD, attribution helpers; CountMemberAttributionsAsync added for Phase 13
│   │   │   │   ├── InboxRepository.cs       — ExistsAsync(messageId), AddAsync(InboxEvent) for deduplication
│   │   │   │   ├── TagRepository.cs         — GetOwnAsync, GetFamilyAsync (co-member, excludes deleted families), GetByNameAsync, GetByIdsAsync, AddAsync, EnsureUserTagAsync, RemoveUserTagAsync, IsVisibleAsync
│   │   │   │   ├── CurrencyRateRepository.cs — GetExactAsync, GetMostRecentBeforeAsync, GetDefaultAsync, GetHistoryAsync, AddRateAsync, UpdateRateAsync, ManualRateExistsAsync, AddConflictAsync, GetPendingConflictsAsync, GetConflictByIdAsync, UpdateConflictAsync, SetDefaultAsync (upsert)
│   │   │   │   ├── Contracts/
│   │   │   │   │   ├── ICategoryRepository.cs
│   │   │   │   │   ├── IUserConfigRepository.cs — GetByUserIdAsync(userId) → UserConfig?; UpsertAsync(userId, currencyId?) → UserConfig
│   │   │   │   │   ├── IDashboardRepository.cs — 5 query methods + 5 record types (CurrencyTotalRow, CategoryTotalRow, MonthlyTotalRow, MonthlyCategoryTotalRow, YearlyTotalRow)
│   │   │   │   │   ├── ICurrencyRepository.cs
│   │   │   │   │   ├── IExpenseRepository.cs — AddAsync, UpdateAsync, SoftDeleteAsync, GetByIdAsync, GetPagedAsync, ClearExpenseTagsAsync, AddExpenseTagsAsync
│   │   │   │   │   ├── IFamilyRepository.cs  — family/membership/invitation/attribution methods; IsMemberAsync, HasDefaultFamilyAsync
│   │   │   │   │   ├── ITagRepository.cs     — GetOwnAsync, GetFamilyAsync, GetByNameAsync, GetByIdsAsync, AddAsync, EnsureUserTagAsync, RemoveUserTagAsync, IsVisibleAsync, SaveChangesAsync
│   │   │   │   │   ├── IInboxRepository.cs  — ExistsAsync, AddAsync
│   │   │   │   │   └── ICurrencyRateRepository.cs — GetExactAsync, GetMostRecentBeforeAsync, GetDefaultAsync, GetHistoryAsync, AddRateAsync, UpdateRateAsync, ManualRateExistsAsync, AddConflictAsync, GetPendingConflictsAsync, GetConflictByIdAsync, UpdateConflictAsync, SetDefaultAsync
│   │   │   │   └── External/
│   │   │   │       ├── Contracts/
│   │   │   │       │   └── IUserRepository.cs — GetUserByEmailAsync (invite flow), GetUserByIdAsync (filters !IsDeleted), GetAdminUserIdsAsync (raw SQL, APP_ADMIN role)
│   │   │   │       └── UserRepository.cs    — Read-only cross-service user access; GetAdminUserIdsAsync joins ext schema (URR_UserRoles + RLE_Roles + USR_Users)
│   │   │   ├── Services/
│   │   │   │   ├── AdminCategoryService.cs  — Implements IAdminCategoryService; add/update/archive/unarchive categories and subcategories; validates parent not archived before adding sub; blocks archiving category with active subcategories
│   │   │   │   ├── AdminCurrencyService.cs  — Implements IAdminCurrencyService; AddCurrencyAsync; SetDefaultFallback delegates to ICurrencyRateService
│   │   │   │   ├── FamilyExceptions.cs      — FamilyNotFoundException (→404), FamilyForbiddenException (→403; also used for tag visibility violations), FamilyConflictException (→409), FamilyInvitationException (→400); default ctor args reference ServiceErrors constants
│   │   │   │   ├── ServiceErrors.cs         — internal static class; 16 domain error-code constants (FAMILY_*, TAG_NOT_VISIBLE, USER_NOT_FOUND, invitation codes) used by service-layer exceptions; mirrors ControllerErrors pattern
│   │   │   │   ├── DashboardService.cs      — Implements IDashboardService; membership check → currency conversion → DTO assembly; previous-period window same duration ending day before dateFrom
│   │   │   │   ├── FamilyService.cs         — Implements IFamilyService; uses ILookupCacheService for role ID resolution; invite expiry from FamilyOptions
│   │   │   │   ├── RabbitMQService.cs       — RabbitMQ connection and messaging
│   │   │   │   ├── LookupCacheService.cs    — IMemoryCache-backed lookup; NeverRemove priority; loads entire table on first access
│   │   │   │   ├── CategoryService.cs       — Injects ICategoryRepository; projects Category → CategoryDto (filters archived children); maps Icon field
│   │   │   │   ├── UserConfigService.cs     — GetAsync(userId) → UserConfigDto; UpdateAsync(userId, currencyId?) → UserConfigDto? (null = invalid currency)
│   │   │   │   ├── CurrencyService.cs       — Injects ICurrencyRepository; projects Currency → CurrencyDto
│   │   │   │   ├── ExpenseService.cs        — Orchestrates IExpenseRepository + IExpenseAuditService + ITagRepository + ICurrencyRateService; validates tag visibility (→403), auto-adopts tags; resolves ConvertedAmount/DisplayCurrency when displayCurrencyId provided; maps Expense → ExpenseDto
│   │   │   │   ├── CsvImportService.cs      — Implements ICsvImportService; ParseAndValidateAsync probes 512 bytes for null bytes (INVALID_FILE_CONTENT), validates required headers (date/amount/currency_code) and ≤20 columns, then parses CSV → RawCsvRowDto[] → ValidateRowsAsync; ValidateRowsAsync accepts pre-parsed rows (re-validate flow) — pre-loads currency/category/family dicts then calls static ValidateRow() per row; ValidateRow validates tags (≤20 per row, ≤100 chars each: TOO_MANY_TAGS/TAG_NAME_TOO_LONG); both async methods accept CancellationToken; ConfirmImportAsync calls ITagService.UseTagAsync per tag name then IExpenseService.AddAsync with sourceId=3 (BulkWeb)
│   │   │   │   ├── TagService.cs            — GetVisibleAsync calls repo in parallel; UseTagAsync is idempotent find-or-create + adopt; RemoveTagAsync removes UserTag only
│   │   │   │   ├── ExpenseAuditService.cs   — Writes ExpenseAuditLog + ExpenseAuditSnapshot(s): add→1 after, update→before+after, delete→1 before; snapshots store comma-sep tag IDs
│   │   │   │   ├── CurrencyRateService.cs   — ResolveRateAsync; AddManualRateAsync (conflict if auto exists); RunDailyUpdateAsync; RefreshRatesFromAsync (backfill range); ResolveConflictAsync
│   │   │   │   └── Contracts/
│   │   │   │       ├── IAdminCategoryService.cs — AddCategoryAsync, UpdateCategoryAsync, ArchiveCategoryAsync, UnarchiveCategoryAsync, AddSubcategoryAsync, UpdateSubcategoryAsync, ArchiveSubcategoryAsync, UnarchiveSubcategoryAsync
│   │   │   │       ├── IAdminCurrencyService.cs — AddCurrencyAsync
│   │   │   │       ├── IRabbitMQService.cs
│   │   │   │       ├── ILookupCacheService.cs — GetIdAsync<T>(name) / GetNameAsync<T>(id)
│   │   │   │       ├── ICategoryService.cs  — GetAllAsync() → active category tree
│   │   │   │       ├── IUserConfigService.cs — GetAsync(userId) → UserConfigDto; UpdateAsync(userId, currencyId?) → UserConfigDto?
│   │   │   │       ├── ICurrencyService.cs  — GetAllAsync() → all currencies
│   │   │   │       ├── IExpenseService.cs   — AddAsync, UpdateAsync, DeleteAsync, GetByIdAsync(id, userId, displayCurrencyId?), GetPagedAsync
│   │   │   │       ├── IExpenseAuditService.cs — WriteAddAuditAsync, WriteUpdateAuditAsync, WriteDeleteAuditAsync (all accept string tags for snapshot)
│   │   │   │       ├── IFamilyService.cs    — CreateDefaultAsync, CreateAsync, GetByUserAsync, GetByIdAsync, RenameAsync, InviteAsync, AcceptInviteAsync, RemoveMemberAsync, ChangeRoleAsync, ArchiveAsync, UnarchiveAsync
│   │   │   │       ├── ITagService.cs       — GetVisibleAsync(userId) → TagListDto; UseTagAsync(name, userId) → TagDto; RemoveTagAsync(tagId, userId) → bool
│   │   │   │       ├── IDashboardService.cs — GetSummaryAsync, GetMonthlyAsync, GetCategoriesAsync, GetSameMonthAcrossYearsAsync, GetByCurrencyAsync, GetRecentAsync
│   │   │   │       ├── ICurrencyRateService.cs — ResolveRateAsync, GetRateHistoryAsync, AddManualRateAsync, BulkAddManualRatesAsync, SetDefaultFallbackAsync, ResolveConflictAsync, GetPendingConflictsAsync, RunDailyUpdateAsync, RefreshRatesFromAsync
│   │   │   │       └── ICsvImportService.cs  — ParseAndValidateAsync(stream, userId, ct?) → CsvImportPreviewDto; ValidateRowsAsync(rows, userId, ct?) → CsvImportPreviewDto; ConfirmImportAsync(rows, userId) → CsvImportResultDto
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
│   │       │   ├── TestExpensesDbContext.cs           — In-memory DB wrapper for tests (Migrate)
│   │       │   └── TestExpensesDbContextEnsureCreated.cs — SQLite EnsureCreated wrapper (for OutboxEvents long PK)
│   │       ├── Controllers/
│   │       │   ├── AdminCategoryControllerTests.cs  — 403 for non-admin; 201/200/404 for all CRUD actions
│   │       │   ├── AdminCurrencyControllerTests.cs  — 403 for non-admin; 201 add; 200/404 update; 204/409 delete; 200 defaults; 204 set default
│   │       │   ├── AdminRateControllerTests.cs      — 403 for non-admin; happy-path for all 7 admin rate endpoints
│   │       │   ├── CategoryControllerTests.cs
│   │       │   ├── CurrencyControllerTests.cs
│   │       │   ├── DashboardControllerTests.cs      — 14 tests: 401 no-cookie, 200 default/explicit date range, 403 FamilyForbidden, 400 generic exception, 400 invalid month (0 and 13), 200 success per endpoint
│   │       │   ├── UserConfigControllerTests.cs     — 6 tests: GET 200 null/populated, GET 401, PUT 200 valid/null, PUT 400 invalid currency, PUT 401
│   │       │   ├── ExpenseControllerTests.cs        — 16 tests: 401 no-cookie × 5 endpoints, 201/400/403 create, 404/200/403 update, 404/204 delete, 404/200 getById, 200 getPaged
│   │       │   └── ExpenseImportControllerTests.cs  — 18 tests: preview 401/400 no-file/400 empty/400 wrong-ext/400 wrong-mime/400 too-large/200/400 exception/400 timeout; confirm 401/200/400 exception; template 401/200 csv/header; validate-rows 401/200/400 exception/400 timeout
│   │       │   ├── FamilyControllerTests.cs         — 34+ tests: 401 no-cookie paths, all 10 family endpoints (200/201/204/403/404/409 per action) incl. LeaveAsync 401/204/403/404
│   │       │   └── TagControllerTests.cs            — 13 tests: 401 no-cookie × 3 endpoints, GetTags 200 (list/empty/family), UseTag 200 (new/existing), RemoveTag 204/404
│   │       ├── Filters/
│   │       │   └── AppAdminAttributeTests.cs        — 403 when isAdmin=false; passes when isAdmin=true; missing cookie → 403
│   │       ├── Jobs/
│   │       │   └── RateAutoUpdateJobTests.cs        — 3 tests: Execute calls RunDailyUpdateAsync, exception does not propagate, exception logs error
│   │       ├── Messaging/
│   │       │   ├── UserEventConsumerTests.cs         — 24 tests: constructor, ExecuteAsync, Dispose, OnMessageReceivedAsync (null msg, dedup, Created/Updated/Deleted/unknown/exception), HandleMessageAsync, UserEventMessage/UserEventType
│   │       │   └── FamilyEventPublisherTests.cs      — 3 tests: PublishRaw calls ExchangeDeclare+BasicPublish, Publish serializes+delegates, MessageId set on properties
│   │       ├── Repositories/
│   │       │   ├── External/
│   │       │   │   └── UserRepositoryTests.cs
│   │       │   ├── CategoryRepositoryTests.cs       — 6 tests: top-level only, children included, archived excluded, empty, archived subs, Category.DeletedAt setter
│   │       │   ├── CurrencyRepositoryTests.cs       — 4 tests: all currencies, field mapping, empty set, positive IDs
│   │       │   ├── DashboardRepositoryTests.cs      — 13 integration tests: GetTotalsAsync×6, GetCategoryTotalsAsync×2, GetMonthlyTotalsAsync×2, GetMonthlyCategoryTotalsAsync×1, GetYearlyTotalsForMonthAsync×2
│   │       │   ├── ExpenseRepositoryTests.cs        — 8 tests: AddAsync, GetByIdAsync (owned/wrong-user/soft-deleted), SoftDeleteAsync, GetPagedAsync (excludes deleted/other-users, pagination, UpdateAsync); BuildExpense static
│   │       │   ├── ExpensesOutboxRepositoryTests.cs  — 8 tests (EnsureCreated): EnqueueAsync persists, GetPendingAsync (unpublished/exceeds retries), MarkPublishedAsync (found/not found), MarkFailedAsync (increments/truncates/not found)
│   │       │   ├── FamilyRepositoryTests.cs         — family CRUD, membership, invitation, attribution, IsMemberAsync, HasDefaultFamilyAsync, ExistsWithNameForUserAsync×4, CountMemberAttributionsAsync
│   │       │   ├── InboxRepositoryTests.cs          — 7 tests: ExistsAsync×3, AddAsync×4
│   │       │   ├── TagRepositoryTests.cs            — 16 integration tests: GetOwnAsync×3, GetFamilyAsync×4, EnsureUserTagAsync×3, RemoveUserTagAsync×2, IsVisibleAsync×4
│   │       │   ├── CurrencyRateRepositoryTests.cs   — 25 integration tests: GetExact×2, GetMostRecentBefore×2, GetDefault×2, AddRate, ManualRateExists×2, AddConflict, GetPendingConflicts, SetDefault×2, GetHistory×2, UpdateRate, GetConflictById×2, UpdateConflict, CurrencyRateConflict.Resolution setter, GetExistingOnDate×3, GetExistingInRange×2, GetExistingForPairs×2, AddRatesBatch×2, AddConflictsBatch, IsUsedInRates×3
│   │       │   └── UserConfigRepositoryTests.cs     — 7 tests: GetByUserIdAsync null/found/loads-nav/wrong-user, UpsertAsync insert/update/clear/no-duplicate/loads-nav
│   │       ├── Infrastructure/
│   │       │   ├── ExpensesDbContextSchemaTests.cs  — 23 tests: all Phase 1 entities, composite PKs, unique constraints, cascades
│   │       │   └── JwtCookieReaderIsAdminTests.cs   — GetIsAdmin: true from cookie, false from cookie, missing cookie → false, Bearer header fallback
│   │       ├── Validators/
│   │       │   ├── CreateTagRequestValidatorTests.cs — 4 tests: valid, empty name, name too long (51 chars), exact max length (50 chars)
│   │       │   ├── ExpenseRequestValidatorTests.cs  — 13 tests: valid pass, amount/currency/date/description/subcategory rules for both Create and Update validators
│   │       │   ├── FamilyValidatorTests.cs          — 15 tests: CreateFamily, RenameFamily, InviteMember (incl. email case + length), ChangeMemberRole
│   │       │   └── ValidateRowsRequestValidatorTests.cs — 9 tests: valid, null rows, >500 rows, Date/Amount/Description/Tags/Families too long, max 500 rows passes
│   │       └── Services/
│   │           ├── AdminCategoryServiceTests.cs     — Add/edit/archive category; add/edit/archive subcategory; cannot-archive-with-active-children validation
│   │           ├── AdminCurrencyServiceTests.cs     — Add currency; duplicate code conflict
│   │           ├── RabbitMQServiceTests.cs
│   │           ├── LookupCacheServiceTests.cs       — 7 tests: GetId/Name, KeyNotFoundException, cache hit, all 8 types
│   │           ├── CategoryServiceTests.cs          — 11 tests: Mock<ICategoryRepository>; top-level, subcategories, archived exclusion, field mapping, call count; icon mapping (category icon, null icon, subcategory icon)
│   │           ├── UserConfigServiceTests.cs        — 6 tests: GetAsync no-row/row, UpdateAsync invalid/valid/null currency, Upsert called, currency check skipped for null
│   │           ├── CurrencyServiceTests.cs          — 5 tests: Mock<ICurrencyRepository>; all currencies, field mapping, empty set, ID mapping, call count
│   │           ├── ExpenseServiceTests.cs           — 20 tests: AddAsync (repo called, audit written, DTO amount/currency, enqueues outbox for non-default families with co-members, skips outbox for default-only), UpdateAsync (null when not found, repo called, audit written, fields updated), DeleteAsync (false/true/soft-delete/audit), GetByIdAsync (null/mapped), GetPagedAsync (result, total pages); updated for ICurrencyRateService dependency
│   │           ├── ExpenseServiceConversionTests.cs — 5 tests: GetByIdAsync with displayCurrencyId set/same currency/no rate/not set; GetPagedAsync with conversion
│   │           ├── ExpenseAuditServiceTests.cs      — 3 tests: WriteAddAuditAsync (log + after snapshot), WriteUpdateAuditAsync (log + before+after snapshots), WriteDeleteAuditAsync (log + before snapshot)
│   │           ├── CurrencyRateServiceTests.cs      — 28 tests: ResolveRateAsync×5, AddManualRateAsync×2, BulkAdd×1, SetDefault×1, ResolveConflict×4, GetRateHistory×1, GetPendingConflicts×1, RunDailyUpdate×5, RefreshRatesFrom×7 (all/manualConflict/providerThrows/skipDest/sourceFilter/destFilter/unknownSource)
│   │           ├── TagServiceTests.cs               — 10 unit tests (Moq): GetVisibleAsync×4, UseTagAsync×4, RemoveTagAsync×2
│   │           ├── DashboardServiceTests.cs         — 20 unit tests: GetSummaryAsync×9 (empty, single-currency, conversion, null rate, +delta, -delta, null delta, top category, FamilyForbidden), GetMonthlyAsync×3 (grouping, category breakdown, rate date), GetCategoriesAsync×3 (subcategory grouping, uncategorised, percentages), GetSameMonthAcrossYearsAsync×2, GetByCurrencyAsync×1, GetRecentAsync×2
│   │           ├── CsvImportServiceTests.cs         — 28 tests: ParseAndValidateAsync×16 (incl. MissingRequiredHeaders, TooManyColumns, BinaryContent), ConfirmImportAsync×5, ValidateRowsAsync×7 (incl. TooManyTags, TagNameTooLong, ValidTagsWithinLimits)
│   │           └── FamilyServiceTests.cs            — 36 tests: CreateDefault, Create, GetByUser, GetById, Rename, Invite (incl. email send + failure non-propagation), AcceptInvite, RemoveMember, ChangeRole, Archive, Unarchive, Leave
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
│       │   ├── Messaging/
│       │   │   ├── Messages/
│       │   │   │   └── UserEventMessage.cs  — Outbound event DTO + UserEventType constants (Created/Updated/Deleted + email.verification.requested/password.reset.requested/password.changed)
│       │   │   └── Publishers/
│       │   │       ├── IUserEventPublisher.cs — Publish(UserEventMessage), PublishRaw(eventType, payload, messageId)
│       │   │       ├── UserEventPublisher.cs — Publishes to users.events topic exchange; sets MessageId property on AMQP message
│       │   │       └── OutboxPublisherService.cs — BackgroundService; polls MSG_OutboxEvents every 5 s; max 5 retries; calls PublishRaw then MarkPublishedAsync
│       │   ├── Infrastructure/
│       │   │   ├── UsersAppDbContext.cs      — EF Core context for users schema
│       │   │   ├── CryptographyHelper.cs     — Password hashing and HMAC utilities
│       │   │   ├── Contracts/
│       │   │   │   └── ICryptographyHelper.cs
│       │   │   └── Options/
│       │   │       ├── AuthenticationServiceOptions.cs — VerifyEmailBaseUrl, ResetPasswordBaseUrl, EmailVerificationExpiryInHours, PasswordResetExpiryInHours
│       │   │       ├── CryptographyOptions.cs
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
│       │   ├── Filters/
│       │   │   └── AdminAuthorizeAttribute.cs   — Action filter; reads isAdmin JWT claim from cookie/header; returns 403 Forbidden if missing or false
│       │   ├── Controllers/
│       │   │   ├── AdminUserController.cs       — GET /admin/users (paged+search), PATCH /admin/users/{id}/disable|enable, PUT /admin/users/{id}/roles; all [AdminAuthorize]
│       │   │   ├── AuthenticationController.cs  — Login, logout, session (now includes isAdmin), refresh, auth check (token ops via IJwtTokenService)
│       │   │   ├── ControllerErrors.cs          — Shared internal static class: SERVER_ERROR, MISSING_PARAMETERS, INVALID_USERNAME_OR_PASSWORD, NO_ASSIGNED_ROLE, MISSING_TOKEN, INVALID_TOKEN, USER_NOT_FOUND, EMAIL_VERIFICATION_FAILED, SET_NEW_PASSWORD_FAILED, REQUEST_PASSWORD_RESET_FAILED, CREATE_PASSWORD_FAILED, RESET_PASSWORD_FAILED
│       │   │   ├── RegistrationController.cs    — Register, validate-email, resend-verification; error redirect appends ?email=…&app_code=…; rate limited
│       │   │   ├── PasswordController.cs        — Change-password, request-password-reset, change-password-reset; rate limited
│       │   │   ├── MessagingController.cs       — POST /messaging/replay (requeue outbox events); GET /messaging/outbox/stats; replay rate limited
│       │   │   ├── DTO/
│       │   │   │   ├── AdminUserDto.cs         — { id, email, firstName, lastName, isDisabled, isDeleted, isEmailValidated, createdAt, roles[] }
│       │   │   │   ├── ApplicationDto.cs
│       │   │   │   ├── RoleDto.cs
│       │   │   │   └── UserDto.cs              — User DTO with FirstName, LastName, Email
│       │   │   ├── Requests/
│       │   │   │   ├── SetUserRolesRequest.cs  — Roles: string[] (replace list)
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
│       │   │       ├── SessionResponse.cs      — Returns Email, FirstName, LastName, IsAdmin from JWT claims
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
│       │   │   ├── AdminUserService.cs             — Implements IAdminUserService; GetUsersPagedAsync (search/page/pageSize), DisableUserAsync, EnableUserAsync, SetUserRolesAsync
│       │   │   ├── ApplicationService.cs
│       │   │   ├── AuthenticationService.cs        — Credential verification only (AuthenticateAsync)
│       │   │   ├── JwtTokenService.cs              — JWT generation (claims: sub, email, givenName, surname, isAdmin, jti) and validation; isAdmin=true when user has APP_ADMIN role
│       │   │   ├── RabbitMQService.cs              — Singleton RabbitMQ connection
│       │   │   ├── RegistrationService.cs          — Registration, email validation, resend verification; re-register with unverified email silently resends; hash expiry 24 h
│       │   │   ├── ResendResult.cs                 — Enum: Sent, NotFound
│       │   │   ├── PasswordManagementService.cs    — Change password, reset password, request password reset
│       │   │   ├── RefreshTokenService.cs          — Generates and validates opaque refresh tokens (DB-backed)
│       │   │   ├── RoleService.cs
│       │   │   ├── UserRoleAssignmentService.cs    — Assigns default application role to a newly registered user
│       │   │   └── Contracts/
│       │   │       ├── IAdminUserService.cs        — GetUsersPagedAsync, DisableUserAsync, EnableUserAsync, SetUserRolesAsync
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
│           │   ├── AdminUserControllerTests.cs      — 403 without admin role; paged list, disable, enable, set roles happy-paths
│           │   ├── AuthenticationControllerTests.cs — extended: session response includes isAdmin
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
│           │   └── CryptographyHelperTests.cs
│           ├── Repositories/
│           │   ├── ApplicationRepositoryTests.cs
│           │   ├── AuthenticationRepositoryTests.cs
│           │   ├── OutboxRepositoryTests.cs         — 15 tests: EnqueueAsync×2, GetPendingAsync×3, MarkPublishedAsync×2, MarkFailedAsync×4, RequeueAsync×4
│           │   ├── RefreshTokenRepositoryTests.cs
│           │   ├── RoleRepositoryTests.cs
│           │   └── UserRepositoryTests.cs           — includes UpdateEmailValidationHashAsync×4 + ValidateEmailAsync expiry×3
│           └── Services/
│               ├── AdminUserServiceTests.cs         — GetUsersPaged with search; disable/enable; role assignment
│               ├── ApplicationServiceTests.cs
│               ├── AuthenticationServiceTests.cs
│               ├── JwtTokenServiceTests.cs          — extended: isAdmin claim present for APP_ADMIN role, absent otherwise
│               ├── PasswordManagementServiceTests.cs
│               ├── RefreshTokenServiceTests.cs
│               ├── RegistrationServiceTests.cs      — includes ResendVerificationEmailAsync×6
│               ├── RoleServiceTests.cs
│               └── UserRoleAssignmentServiceTests.cs
│           ├── Models/
│           │   └── ModelPropertyTests.cs        — 4 tests: navigation-property setters for User, UserDto, UserRole; RefreshToken.Id setter
│
├── frontend/
│   ├── dashboard/
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
│       ├── tailwind.config.ts         — Hearth design system tokens (brand/surface/ink/sage/berry/mustard palette, custom fonts, shadows); darkMode: 'class'; surface+ink use CSS variable references for automatic dark-mode adaptation
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
│           │   ├── FormCombobox.tsx    — Searchable combobox (text input + listbox dropdown); optional className prop; used in ExpenseForm + admin pages
│           │   ├── LanguageSwitcher.tsx — Language selector dropdown wired to i18n.changeLanguage
│           │   ├── PasswordInput.tsx   — Password input with show/hide toggle
│           │   ├── PasswordStrength.tsx — Live password strength indicator (5-segment bar + checklist)
│           │   ├── SubmitButton.tsx    — Submit button with spinner SVG and configurable labels
│           │   ├── ThemeToggle.tsx     — Segmented 3-button control (Day/Default/Dark); uses useTheme(); aria-pressed; active = brand-500
│           │   ├── Toast.tsx           — Toast notification provider and hook
│           │   └── __tests__/
│           │       ├── BackLink.test.tsx
│           │       ├── FieldError.test.tsx
│           │       ├── FormCombobox.test.tsx
│           │       ├── LanguageSwitcher.test.tsx
│           │       ├── PasswordInput.test.tsx
│           │       ├── PasswordStrength.test.tsx
│           │       ├── SubmitButton.test.tsx
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
│           │   │   │   ├── PublicOnlyRoute.tsx  — Redirects authenticated users to /dashboard
│           │   │   │   └── __tests__/
│           │   │   │       ├── AuthBrandPanel.test.tsx
│           │   │   │       ├── AuthCard.test.tsx
│           │   │   │       ├── AuthPageHeader.test.tsx
│           │   │   │       └── EmailField.test.tsx
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
│           │   │   │   └── auth.type.ts         — User (email, firstName?, lastName?, isAdmin?), AuthResult, AuthContextValue
│           │   │   ├── AuthContext.tsx           — Cookie-based auth state; session restored via GET /auth/session (now returns isAdmin); no localStorage/sessionStorage
│           │   │   ├── auth.schemas.ts           — Zod schemas and inferred types for all five auth forms
│           │   │   └── __tests__/
│           │   │       ├── AuthContext.test.tsx
│           │   │       ├── ProtectedRoute.test.tsx
│           │   │       ├── PublicOnlyRoute.test.tsx
│           │   │       ├── auth.schemas.test.ts
│           │   │       └── authApi.service.test.ts
│           │   ├── families/          — Family management feature
│           │   │   ├── components/
│           │   │   │   ├── FamilySelector.tsx   — NavBar dropdown to switch active family scope; hidden when no non-default active families
│           │   │   │   └── __tests__/
│           │   │   │       └── FamilySelector.test.tsx
│           │   │   ├── pages/
│           │   │   │   ├── FamiliesPage.tsx      — Family management screen: active/archived tabs, cards, create/rename/archive/invite modals, inline member panel
│           │   │   │   └── __tests__/
│           │   │   │       ├── FamiliesPage.test.tsx
│           │   │   │       └── AcceptInvitePage.test.tsx — 9 tests: loading state, success, error with/without res.error, missing token, no API call when token absent, silent:true call, go-to-families link on success/error
│           │   │   ├── services/
│           │   │   │   ├── familyApi.service.ts  — All family CRUD + invitation + member management calls
│           │   │   │   └── __tests__/
│           │   │   │       └── familyApi.service.test.ts
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
│           │   │   │   ├── tagsApi.service.ts   — getTags() → GET /api/expenses/tags; useTag(name) → POST; removeTag(id) → DELETE
│           │   │   │   └── __tests__/
│           │   │   │       └── tagsApi.service.test.ts
│           │   │   └── components/
│           │   │       ├── TagInput.tsx          — Combobox: grouped "My tags"/"Family tags" dropdown, chips, create option, keyboard (Enter/Escape/Backspace)
│           │   │       └── __tests__/
│           │   │           └── TagInput.test.tsx — 18 component tests (role queries updated to menu/menuitem; added getTags/useTag error-path, Enter-key tests, outside-click close)
│           │   ├── currencies/        — Display currency feature (Phase 6)
│           │   │   ├── components/
│           │   │   │   ├── DisplayCurrencySelector.tsx — NavBar dropdown; reads currencies from ExpensesDataContext; writes to DisplayCurrencyContext; "No conversion" option
│           │   │   │   └── __tests__/
│           │   │   │       └── DisplayCurrencySelector.test.tsx — 15 tests: renders/no currencies/label/open/select/clear/close/aria-checked/search-input/filter-by-code/filter-by-name/clear-on-close/outside-click
│           │   │   ├── services/
│           │   │   │   ├── ratesApi.service.ts — refreshRates(RefreshRatesParams) → POST /api/expenses/rates/refresh (204); params: from, sourceCurrencyId?, destinationCurrencyId?
│           │   │   │   └── __tests__/
│           │   │   │       └── ratesApi.service.test.ts — 8 tests: from-only, sourceCurrencyId, destinationCurrencyId, both, success, error, 401, call count
│           │   │   ├── DisplayCurrencyContext.tsx — Session-only state (number | null, default null); no localStorage
│           │   │   └── __tests__/
│           │   │       └── DisplayCurrencyContext.test.tsx — 5 tests: default null, set/clear, persist on rerender, throws outside provider
│           │   ├── expenses/          — Expense management feature
│           │   │   ├── types/
│           │   │   │   └── expenses.type.ts     — Category, Subcategory, Currency, ExpenseDto, ExpenseFilter, ExpensePagedResponse, CsvImportRowPreview, CsvImportPreviewDto, CsvImportConfirmRowDto, CsvImportResultDto, RawCsvRowDto types
│           │   │   ├── services/
│           │   │   │   ├── categoriesApi.service.ts — getCategories() → GET /api/expenses/categories
│           │   │   │   ├── currenciesApi.service.ts — getCurrencies() → GET /api/expenses/currencies
│           │   │   │   ├── expensesApi.service.ts  — getExpenses, getExpenseById, addExpense, updateExpense, deleteExpense, previewCsvImport(file), confirmCsvImport(rows), validateCsvRows(rows), getImportTemplateUrl()
│           │   │   │   └── __tests__/
│           │   │   │       ├── categoriesApi.service.test.ts
│           │   │   │       ├── currenciesApi.service.test.ts
│           │   │   │       └── expensesApi.service.test.ts
│           │   │   ├── components/
│           │   │   │   ├── AddExpenseModal.tsx  — Modal overlay with ExpenseForm; calls addExpense; onSuccess/onClose callbacks
│           │   │   │   ├── EditExpenseModal.tsx — Modal overlay with pre-filled ExpenseForm; fetches expense by id via useQuery; onSuccess/onClose callbacks
│           │   │   │   ├── ExpenseForm.tsx     — RHF+Zod form: amount, currency (FormCombobox), date, category (FormCombobox), subcategory (FormCombobox, conditional), description, tags, families (default family hidden)
│           │   │   │   ├── ExpenseFilters.tsx  — Collapsible filter panel; toggle with aria-expanded; resets page to 1 on apply; FilterCombobox for category/subcategory/currency (case-insensitive search)
│           │   │   │   └── __tests__/
│           │   │   │       ├── AddExpenseModal.test.tsx
│           │   │   │       ├── EditExpenseModal.test.tsx
│           │   │   │       ├── ExpenseForm.test.tsx
│           │   │   │       └── ExpenseFilters.test.tsx
│           │   │   ├── pages/
│           │   │   │   ├── ExpensesPage.tsx    — Paginated expense table with Families column; delete confirm modal; filter panel; empty state; AddExpenseModal (/expenses/add) + EditExpenseModal (/expenses/:id/edit) route-based overlays; "Import CSV" button → /expenses/import
│           │   │   │   ├── CsvImportPage.tsx   — Two-step upload→preview flow; all 8 columns; Edit/Save/Cancel per row; currency/category/subcategory = StringCombobox; tags = TagChips (chips + autocomplete from useExpensesData, adds new on confirm); families = FamilyMultiSelect (names shown, IDs stored, from useFamilies); all 3 dropdowns portal-rendered to document.body (position:fixed via getBoundingClientRect) to escape overflow-x-auto clipping; 3-state edit model; Re-validate auto-saves and calls POST /import/validate-rows
│           │   │   │   └── __tests__/
│           │   │   │       ├── ExpensesPage.test.tsx
│           │   │   │       └── CsvImportPage.test.tsx — 26 tests: dropzone/template, preview, all 8 columns header, tags-as-chips display, families-as-"default" display, badge counts, error codes, edit button per row, inputs after edit click (incl. tags/families inputs), save/cancel, cancel discards, re-validate visibility, re-validate auto-saves pending edits, tags serialised as semicolon string, preview updates, confirm/navigate/error, cancel to upload, file too large shows error, wrong extension shows error
│           │   │   ├── expense.schemas.ts  — makeExpenseSchema(t): Zod v4 schema; categoryId/subcategoryId use .catch(undefined) to coerce NaN
│           │   │   ├── ExpensesDataContext.tsx  — ExpensesDataProvider / useExpensesData(); fetches categories + currencies on mount
│           │   │   └── __tests__/
│           │   │       ├── ExpensesDataContext.test.tsx
│           │   │       └── expense.schemas.test.ts
│           │   ├── dashboard/         — Authenticated dashboard feature (Phase 9 — Hearth design)
│           │   │   ├── types/
│           │   │   │   └── dashboard.type.ts — DashboardSummaryDto, MonthlyBreakdownDto, CategoryBreakdownDto, SameMonthYearlyDto, CurrencyBreakdownDto, DashboardFilter
│           │   │   ├── services/
│           │   │   │   ├── dashboardApi.service.ts — getSummary, getMonthly, getCategories, getSameMonthYearly, getByCurrency, getRecent
│           │   │   │   └── __tests__/
│           │   │   │       └── dashboardApi.service.test.ts
│           │   │   ├── utils/
│           │   │   │   ├── categoryColors.ts    — Shared 6-color design palette; CHART_COLORS for recharts; getCategoryColor(id) → {bg, text} for labels
│           │   │   │   └── __tests__/
│           │   │   │       └── categoryColors.test.ts — 11 tests: CHART_COLORS (length/hex/unique), getCategoryColor (fallback/determinism/modulo/shape/palette membership)
│           │   │   ├── components/
│           │   │   │   ├── MonthHero.tsx        — Summary card: total, ±% delta chip, expense count, top category pill
│           │   │   │   ├── SpendChart.tsx        — Monthly stacked bar + average line (Recharts ComposedChart)
│           │   │   │   ├── CategoryDonut.tsx     — Donut chart + legend (Recharts PieChart); design-palette colors; legend shows amount + percentage; optional displayCurrency prop for converted totals
│           │   │   │   ├── SameMonthChart.tsx    — Year-over-year bar chart (Recharts BarChart)
│           │   │   │   ├── CurrenciesPanel.tsx   — Per-currency breakdown rows
│           │   │   │   ├── RecentExpenses.tsx    — Last 10 expenses feed; "View all" → /expenses
│           │   │   │   ├── DashboardFilters.tsx  — Family + display-currency + date-range selectors; "This month"/"This year" presets
│           │   │   │   └── __tests__/
│           │   │   │       ├── MonthHero.test.tsx
│           │   │   │       ├── SpendChart.test.tsx
│           │   │   │       ├── CategoryDonut.test.tsx
│           │   │   │       ├── SameMonthChart.test.tsx
│           │   │   │       ├── CurrenciesPanel.test.tsx
│           │   │   │       ├── RecentExpenses.test.tsx
│           │   │   │       └── DashboardFilters.test.tsx
│           │   │   └── pages/
│           │   │       ├── HomeDashboardPage.tsx — Hearth layout; 6 useQuery calls; DashboardFilters + MonthHero + SpendChart + CategoryDonut + SameMonthChart + CurrenciesPanel + RecentExpenses
│           │   │       ├── SettingsPage.tsx       — Settings hub; password card (link to /change-password); default-currency card; theme card (ThemeToggle)
│           │   │       └── __tests__/
│           │   │           ├── HomeDashboardPage.test.tsx
│           │   │           └── SettingsPage.test.tsx
│           │   ├── admin/             — Admin feature (Phase 11)
│           │   │   ├── components/
│           │   │   │   ├── AdminRoute.tsx       — Route guard; redirects to /dashboard when isAdmin=false
│           │   │   │   ├── AdminLayout.tsx      — Sidebar nav (Users/Categories/Currencies/Rates/Rate Conflicts) + breadcrumb header
│           │   │   │   └── __tests__/
│           │   │   │       ├── AdminLayout.test.tsx
│           │   │   │       └── AdminRoute.test.tsx
│           │   │   ├── pages/
│           │   │   │   ├── AdminUsersPage.tsx   — Searchable paginated user table; Disable/Enable actions; Manage Roles modal; APP_ADMIN checkbox disabled for own account
│           │   │   │   ├── AdminCategoriesPage.tsx — Category tree; Add/Edit/Archive/Unarchive; subcategory management; "Show archived" toggle
│           │   │   │   ├── AdminCurrenciesPage.tsx — Currency list; search bar (code/name); client-side pagination; per-row Edit/Delete/Defaults buttons; edit modal; delete modal (409 in-use handling); defaults modal with editable default rates + add-pair row
│           │   │   │   ├── AdminRatesPage.tsx   — Always-loaded history table; From/To currency code columns; FormCombobox filters; rateSource string column; pagination; Add Manual Rate modal (src/dst comboboxes); Backfill modal (from+to dates)
│           │   │   │   ├── AdminRateConflictsPage.tsx — Pending conflicts; per-row resolve (AcceptAuto/KeepManual/Custom); bulk resolve
│           │   │   │   └── __tests__/
│           │   │   │       ├── AdminRoute.test.tsx
│           │   │   │       ├── AdminUsersPage.test.tsx
│           │   │   │       ├── AdminCategoriesPage.test.tsx
│           │   │   │       ├── AdminCurrenciesPage.test.tsx
│           │   │   │       ├── AdminRatesPage.test.tsx
│           │   │   │       └── AdminRateConflictsPage.test.tsx
│           │   │   └── services/
│           │   │       ├── adminUsersApi.service.ts      — getUsers, getRoles, disableUser, enableUser, setUserRoles
│           │   │       ├── adminCategoriesApi.service.ts — getCategories, addCategory, updateCategory, archiveCategory, unarchiveCategory, addSubcategory, updateSubcategory, archiveSubcategory, unarchiveSubcategory
│           │   │       ├── adminCurrenciesApi.service.ts — addCurrency, updateCurrency, deleteCurrency, getCurrencyDefaults, setDefaultRate; CurrencyDefaultRateDto type
│           │   │       ├── adminRatesApi.service.ts      — getRateHistory (optional filters, paged), addManualRate, bulkAddRates, refreshRates (optional to), getPendingConflicts, resolveConflict; PagedRatesResponse + RateDto types
│           │   │       └── __tests__/
│           │   │           ├── adminCategoriesApi.service.test.ts
│           │   │           ├── adminCurrenciesApi.service.test.ts
│           │   │           ├── adminRatesApi.service.test.ts
│           │   │           └── adminUsersApi.service.test.ts
│           │   ├── notifications/     — In-app notifications feature (Phase 13+14)
│           │   │   ├── types/
│           │   │   │   └── notification.type.ts  — NotificationPayload discriminated union (7 types); Notification shape
│           │   │   ├── components/
│           │   │   │   ├── NotificationBell.tsx  — Bell icon + badge + dropdown; getNotificationText() maps all 7 types to i18n labels; toast on new notification
│           │   │   │   └── __tests__/
│           │   │   │       └── NotificationBell.test.tsx
│           │   │   ├── services/
│           │   │   │   ├── notificationApi.service.ts  — getNotifications, getUnreadCount, markAsRead, markAllAsRead
│           │   │   │   └── __tests__/
│           │   │   │       └── notificationApi.service.test.ts
│           │   │   ├── NotificationContext.tsx   — NotificationProvider / useNotifications(); loads on auth; SignalR connection via dynamic import; markRead/markAllRead/refresh; setupPushNotifications() registers Capacitor PushNotifications + calls POST /api/notifications/push-token
│           │   │   └── __tests__/
│           │   │       └── NotificationContext.test.tsx
│           │   ├── settings/          — Settings feature (theme)
│           │   │   └── ThemeContext.tsx — ThemeProvider / useTheme(); three modes: light/dark/system; applies .dark/.light class to <html>; persists to localStorage
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
│           │   ├── AppProviders.tsx   — Nests ThemeProvider → AuthProvider → ExpensesDataProvider → FamilyProvider → DisplayCurrencyProvider → NotificationProvider
│           │   └── __tests__/
│           │       └── AppProviders.test.tsx
│           ├── hooks/                 — Shared hooks
│           │   ├── usePageTitle.ts    — Sets document.title per page
│           │   └── __tests__/
│           │       └── usePageTitle.test.ts
│           ├── layouts/               — App-wide layout components
│           │   ├── NavBar.tsx          — Auth-aware nav; desktop + mobile responsive; "Admin" link shown only when isAdmin=true; right-side controls: FamilySelector → DisplayCurrencySelector → Add Expense `+` button → notification bell → user avatar dropdown (includes ThemeToggle row)
│           │   └── __tests__/
│           │       └── NavBar.test.tsx
│           ├── services/              — Shared base services
│           │   ├── api.service.ts     — Base fetch wrapper with cookie auth, transparent refresh-and-retry on 401, and skipUnauthorized option
│           │   └── __tests__/
│           │       ├── api.service.test.ts     — Full coverage: normal flows, 401 refresh-and-retry, deduplication, skipUnauthorized, network errors
│           │       ├── api.service.env.test.ts — Isolated env tests: VITE_API_BASE prefix and trailing-slash strip
│           │       └── api.test.ts
│           ├── styles/
│           │   └── index.css          — Tailwind directives + @layer components; CSS variable light/dark palette definitions; @media prefers-color-scheme dark for system mode
│           └── types/                 — Shared TypeScript type definitions
│               └── api.type.ts         — ApiResponse<T>
│
│   └── mobile/                        — Ionic + Capacitor native mobile app (Phase 14)
│       ├── .env.local                 — Local env vars (gitignored; set VITE_API_BASE_URL=https://localhost)
│       ├── .gitignore                 — Ignores node_modules, dist, coverage, .env.local, /android, /ios, .capacitor/
│       ├── .gitlab-ci.yml             — Mobile CI pipeline: build→test→sonarqube→sca→sast→secrets-scan
│       ├── capacitor.config.ts        — appId=com.touir.expensemanager, webDir=dist
│       ├── index.html                 — Vite HTML entry point (root; required by Vite build)
│       ├── ionic.config.json          — Ionic CLI project config (type: react-vite)
│       ├── package.json               — @ionic/react v8, @capacitor/core v7 + plugins, idb v8, @microsoft/signalr, i18next, react-query v5; appId field; build:android/build:android-aab/deploy:android scripts
│       ├── sonar-project.properties   — SonarQube project key touir:expense-manager:frontend:mobile
│       ├── tsconfig.json
│       ├── tsconfig.app.json          — ES2020, strict, path alias @/*→src/*, react-jsx
│       ├── vite.config.ts             — @vitejs/plugin-react, alias @→./src, port 5174, Vitest config
│       ├── README.md                  — Setup, dev, device/emulator, env vars, offline queue, tests docs
│       ├── scripts/
│       │   ├── android-gradle.js      — Runs Gradle task (assembleDebug/bundleRelease) with JDK 21; Windows/bash auto-detect
│       │   └── android-deploy.js      — Installs debug APK via adb; globs output dir for APK; reads appId from package.json
│       └── src/
│           ├── main.tsx               — createRoot with <App />, imports Ionic CSS + theme
│           ├── App.tsx                — <IonApp> + <BrowserRouter> (react-router-dom v6) + <AppProviders> + <AppRouter>
│           ├── router.tsx             — Auth guard + IonTabs (5 tabs: Dashboard/Expenses/+FAB/Families/Settings); QuickAddModal outside IonTabs
│           ├── test-setup.ts          — Vitest global mocks for 5 Capacitor plugins + fake-indexeddb + @testing-library/jest-dom
│           ├── theme/
│           │   └── variables.css      — Ionic CSS vars mapped to Hearth tokens (clay primary, paper bg, sage success, berry danger); .dark block + @media prefers-color-scheme dark for system mode
│           ├── i18n/                  — i18next config + locale JSON (en/fr/es/de copied from dashboard)
│           ├── providers/
│           │   └── AppProviders.tsx   — QueryClientProvider→ThemeProvider→AuthProvider→ExpensesDataProvider→FamilyProvider→DisplayCurrencyProvider→NotificationProvider
│           ├── services/
│           │   └── api.service.ts     — Adapted from dashboard; uses VITE_API_BASE_URL for Capacitor WebView absolute URLs
│           ├── types/
│           │   └── api.type.ts        — ApiResponse<T> (verbatim copy from dashboard)
│           ├── constants/
│           │   └── apiErrors.constant.ts — API_ERRORS + BACKEND_ERROR_CODES (verbatim copy)
│           ├── hooks/
│           │   ├── useOfflineQueue.ts — IndexedDB queue via idb v8; enqueue/drain/getAll/clear; store: offline-expense-queue
│           │   ├── useNetworkSync.ts  — @capacitor/network listener; drains queue on reconnect; browser online/offline event fallback
│           │   └── __tests__/
│           │       ├── useOfflineQueue.test.ts — enqueue/drain/clear with fake-indexeddb
│           │       └── useNetworkSync.test.ts  — offline→online triggers drain
│           └── features/
│               ├── auth/
│               │   ├── AuthContext.tsx          — Cookie auth; adapted from dashboard (IonRouter navigation)
│               │   ├── auth.schemas.ts          — Zod v4 schemas (verbatim copy)
│               │   ├── services/authApi.service.ts
│               │   ├── types/auth.type.ts
│               │   └── pages/
│               │       ├── LoginPage.tsx        — IonPage + IonContent wrapping auth form
│               │       └── __tests__/LoginPage.test.tsx
│               ├── expenses/
│               │   ├── ExpensesDataContext.tsx  — ExpensesDataProvider / useExpensesData() (verbatim copy)
│               │   ├── expense.schemas.ts       — makeExpenseSchema(t) with Zod v4 .catch(undefined) coercions
│               │   ├── services/
│               │   │   ├── expensesApi.service.ts
│               │   │   ├── categoriesApi.service.ts
│               │   │   └── currenciesApi.service.ts
│               │   ├── types/expenses.type.ts
│               │   └── pages/
│               │       ├── ExpensesListPage.tsx   — IonList grouped by date; IonItemSliding swipe-delete; IonRefresher; IonInfiniteScroll; IonSegment family filter
│               │       ├── QuickAddModal.tsx      — IonModal sheet (0.75 breakpoint); offline enqueue; Haptics; Camera receipt
│               │       └── __tests__/
│               │           ├── ExpensesListPage.test.tsx
│               │           └── QuickAddModal.test.tsx
│               ├── dashboard/
│               │   ├── services/dashboardApi.service.ts — getSummary, getMonthly, getDashboardCategories, getSameMonthYearly, getByCurrency, getRecent
│               │   ├── types/dashboard.type.ts
│               │   ├── components/
│               │   │   ├── DashboardDateFilter.tsx — IonSegment (This Month / 6 Months / This Year); exports getPeriodDates(period) pure helper
│               │   │   ├── SpendTrendChart.tsx    — Recharts AreaChart; monthly spend trend; displayCurrency-aware
│               │   │   ├── CategoryPieChart.tsx   — Recharts PieChart (donut); top-6 categories + Other bucket; percentage legend
│               │   │   ├── SameMonthChart.tsx     — Recharts BarChart; year-over-year same-month comparison
│               │   │   ├── CurrenciesPanel.tsx    — IonList; per-currency totals + expense counts
│               │   │   └── __tests__/
│               │   │       ├── DashboardDateFilter.test.tsx
│               │   │       ├── SpendTrendChart.test.tsx
│               │   │       ├── CategoryPieChart.test.tsx
│               │   │       ├── SameMonthChart.test.tsx
│               │   │       └── CurrenciesPanel.test.tsx
│               │   └── pages/
│               │       ├── DashboardPage.tsx     — Period filter state; 6 useQuery calls; DateFilter + hero + SpendTrendChart + CategoryPieChart + CurrenciesPanel + SameMonthChart + recent-5 IonList
│               │       └── __tests__/DashboardPage.test.tsx
│               ├── families/
│               │   ├── FamilyContext.tsx         — FamilyProvider / useFamilies() (adapted from dashboard)
│               │   ├── services/familyApi.service.ts
│               │   ├── types/family.type.ts
│               │   └── pages/
│               │       ├── FamiliesPage.tsx      — IonList family cards; invite by email; member list; leave/archive
│               │       └── AcceptInvitePage.tsx  — IonPage with IonBackButton; reads ?token=; calls acceptInvite; shows loading/success/error
│               ├── notifications/
│               │   ├── NotificationContext.tsx   — SignalR + @capacitor/push-notifications registration; POST /push-token on login
│               │   ├── services/
│               │   │   ├── notificationApi.service.ts — getNotifications, getUnreadCount, markAsRead, markAllAsRead, registerPushToken
│               │   │   └── types/notification.type.ts
│               │   ├── components/
│               │   │   └── NotificationBell.tsx  — IonButton + IonBadge + IonPopover notification list
│               │   └── __tests__/NotificationContext.test.tsx — real class MockHubConnectionBuilder
│               ├── settings/
│               │   ├── ThemeContext.tsx          — ThemeProvider / useTheme(); async load via @capacitor/preferences (localStorage fallback); applies .dark/.light to <html>
│               │   └── pages/
│               │       ├── SettingsPage.tsx      — Display currency + language + theme (IonSelect, action-sheet) selectors; persists language to @capacitor/preferences; logout
│               │       └── __tests__/SettingsPage.test.tsx
│               ├── currencies/
│               │   ├── DisplayCurrencyContext.tsx  — Session state; adapted from dashboard (Preferences persistence)
│               │   └── services/ratesApi.service.ts
│               └── tags/
│                   ├── services/
│                   │   └── tagsApi.service.ts
│                   └── types/
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
```
