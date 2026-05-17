# Testing Guide

← [Wiki Index](./index.md)

---

## Overview

Testing spans three codebases — users service, expenses service, and the React frontend. Each has its own runner, helpers, and patterns. The rule across all three: **never mock DbContext**.

---

## Users Service

**Runner:** xUnit + Moq + FluentValidation.TestHelper  
**Location:** `backend/users/Touir.ExpensesManager.Users.Tests/`  
**Test count:** ~318 (as of v0.103.1)

### Running

```bash
cd backend/users
dotnet test --collect:"XPlat Code Coverage" --results-directory "./coverage" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

# Single test class
dotnet test --filter FullyQualifiedName~UserRepositoryTests
```

### Test Helpers

Two SQLite-backed contexts exist because `OutboxEvent.Id` is a `long` PK and Npgsql's `IdentityAlwaysColumn` annotation breaks SQLite's auto-increment. **Never call `Migrate()` for `OutboxRepositoryTests`.**

| Helper | DB setup | Use for |
|---|---|---|
| `TestDbContextWrapper` | SQLite + `Migrate()` | Entities with `int`/`short` PKs (users, roles, etc.) |
| `TestDbContextEnsureCreated` | SQLite + `EnsureCreated()` | `OutboxEvent` (long PK) |

Both implement `IDisposable` — each test gets a clean database state.

### Patterns

**Repository tests** (real SQLite DB):
```csharp
using var ctx = new TestDbContextWrapper();
var repo = new UserRepository(ctx.DbContext);
// seed → act → assert
```

**Service tests** (Moq):
```csharp
var mockRepo = new Mock<IUserRepository>();
mockRepo.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(user);
var svc = new RegistrationService(mockRepo.Object, ...);
```

**Validator tests** (FluentValidation.TestHelper):
```csharp
var validator = new RegisterRequestValidator();
var result = validator.TestValidate(new RegisterRequest { Email = "" });
result.ShouldHaveValidationErrorFor(x => x.Email)
      .WithErrorMessage("MISSING_PARAMETERS");
```

**Controller tests** (real service + Moq infrastructure):
```csharp
var controller = new AuthenticationController(mockAuthSvc.Object, ...);
var result = await controller.Login(request);
Assert.IsType<OkObjectResult>(result);
```

### Option Factories

Service tests that involve configurable TTLs must set all required option fields, otherwise expiry-check tests fail silently:

```csharp
Options.Create(new JwtAuthOptions {
    SecretKey = "...",
    ShortLivedRefreshExpiryInDays = 1,   // required
    ExpiryInMinutes = 60
})
```

---

## Expenses Service

**Runner:** xUnit + Moq  
**Location:** `backend/expenses/Touir.ExpensesManager.Expenses.Tests/`  
**Test count:** ~454 (as of v0.103.1)

### Running

```bash
cd backend/expenses
dotnet test --collect:"XPlat Code Coverage" --results-directory "./coverage" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
```

### Test Helpers

| Helper | DB setup | Use for |
|---|---|---|
| `TestExpensesDbContextWrapper` | In-memory EF Core | All expenses repository tests |

```csharp
using var ctx = new TestExpensesDbContextWrapper();
var repo = new FamilyRepository(ctx.DbContext);
```

### Patterns

**Repository tests** (in-memory EF Core):
- Seed data directly via `ctx.DbContext.Add(entity)` + `SaveChangesAsync()`
- In-memory provider has no FK constraint enforcement — test at the service layer for that

**Service tests** (Moq):
```csharp
var mockFamilyRepo = new Mock<IFamilyRepository>();
var mockLookup = new Mock<ILookupCacheService>();
mockLookup.Setup(l => l.GetIdAsync<FamilyRole>("Head")).ReturnsAsync(1);
var svc = new FamilyService(mockFamilyRepo.Object, mockLookup.Object, ...);
```

**Controller tests**:
- Moq the service layer
- `JwtCookieReader` reads `auth_token` cookie — inject a mock `IHttpContextAccessor` or set cookies on `controller.ControllerContext`

**Option factories for expenses services:**
```csharp
Options.Create(new FamilyOptions {
    InviteExpiryInDays = 7    // required for invite expiry tests
})
```

### Validator tests

Uses `ClassLevelCascadeMode = CascadeMode.Stop` — only first error per class fires. Test in dependency order:

```csharp
var result = validator.TestValidate(new CreateExpenseRequest {
    CategoryId = null,
    SubcategoryId = 5    // invalid: requires CategoryId
});
result.ShouldHaveValidationErrorFor("SubcategoryId");
```

---

## Frontend Dashboard

**Runner:** Vitest 4 + React Testing Library + jsdom  
**Location:** `frontend/dashboard/src/`  
**Co-location:** Tests in `__tests__/` next to the component/page they test

### Running

```bash
cd frontend/dashboard
npm test                # run all + coverage
npm run test:watch      # watch mode
npm run typecheck       # TypeScript check (no coverage)
```

### Helpers

**`renderWithProviders(ui, options?)`**  
Wraps a component in `MemoryRouter` + `AuthProvider` with pre-configured API mocks. Use for pages and components that access auth context or routing.

```typescript
const { getByRole } = renderWithProviders(<LoginPage />, {
  initialEntries: ['/login'],
  authState: { isAuthenticated: false }
})
```

### Key Patterns

**Mock API calls with `vi.mock`:**
```typescript
vi.mock('../services/authApi.service', () => ({
  loginRequest: vi.fn().mockResolvedValue({ ok: true, data: mockUser })
}))
```

**Prefer `toHaveClass()` over `toHaveStyle()`** — Tailwind doesn't produce inline styles in jsdom:
```typescript
// correct
expect(button).toHaveClass('btn-primary')

// wrong — fails in jsdom
expect(button).toHaveStyle({ backgroundColor: '#4f46e5' })
```

**Form interaction:**
```typescript
await userEvent.type(screen.getByLabelText(/email/i), 'jane@example.com')
await userEvent.click(screen.getByRole('button', { name: /log in/i }))
```

**Assert error messages via `aria-describedby`:**
```typescript
const input = screen.getByRole('textbox', { name: /email/i })
expect(input).toHaveAttribute('aria-describedby', 'email-error')
expect(screen.getByText('MISSING_PARAMETERS')).toBeInTheDocument()
```

**Route guard tests:**
```typescript
renderWithProviders(<ProtectedRoute><Dashboard /></ProtectedRoute>, {
  authState: { isAuthenticated: false }
})
expect(mockNavigate).toHaveBeenCalledWith('/login', expect.any(Object))
```

### Coverage

V8 coverage via Vitest. View report at `coverage/index.html` after `npm test`.

---

## Shared Rules

| Rule | Applies to |
|---|---|
| Never mock `DbContext` | Both backend services |
| Never call `Migrate()` for `OutboxRepositoryTests` | Users service |
| Use `ClassLevelCascadeMode = CascadeMode.Stop` in validators | Expenses service |
| Prefer `toHaveClass` not `toHaveStyle` | Frontend |
| One option field missing = silent test failure on TTL checks | Both services |
| `SubcategoryId` requires `CategoryId` — test the `.Must()` rule | Expenses validators |

---

## CI Coverage

Coverage is collected in CI via:

- **Backend:** `XPlat Code Coverage` → OpenCover format → uploaded to SonarQube
- **Frontend:** Vitest V8 → `lcov` → uploaded to SonarQube

Coverage gate is enforced by the SonarQube quality gate in the `quality` CI stage.
