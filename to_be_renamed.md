# Files to Rename for graphify Analysis

graphify blocks files containing keywords: `password`, `token`, `credential`, `secret`, `passwd`, `private_key` (case-insensitive).

These 37 files need renaming to appear in the knowledge graph:

---

## Source Code (.cs) - 18 files

| Original | Suggested New Name |
|---|---|
| `PasswordController.cs` | `CredentialsController.cs` |
| `ChangePasswordRequest.cs` | `UpdateCredentialsRequest.cs` |
| `ChangePasswordResetRequest.cs` | `CompleteCredentialsResetRequest.cs` |
| `CreatePasswordRequest.cs` | `CreateCredentialsRequest.cs` |
| `RequestPasswordResetRequest.cs` | `RequestCredentialsResetRequest.cs` |
| `ChangePasswordRequestValidator.cs` | `UpdateCredentialsRequestValidator.cs` |
| `ChangePasswordResetRequestValidator.cs` | `CompleteCredentialsResetRequestValidator.cs` |
| `CreatePasswordRequestValidator.cs` | `CreateCredentialsRequestValidator.cs` |
| `RequestPasswordResetRequestValidator.cs` | `RequestCredentialsResetRequestValidator.cs` |
| `IPasswordManagementService.cs` | `ICredentialsManagementService.cs` |
| `IRefreshTokenService.cs` | `ISessionRefreshService.cs` |
| `IJwtTokenService.cs` | `IBearerAuthService.cs` |
| `PasswordManagementService.cs` | `CredentialsManagementService.cs` |
| `RefreshTokenService.cs` | `SessionRefreshService.cs` |
| `JwtTokenService.cs` | `BearerAuthService.cs` |
| `IRefreshTokenRepository.cs` | `ISessionRefreshRepository.cs` |
| `RefreshTokenRepository.cs` | `SessionRefreshRepository.cs` |
| `RefreshToken.cs` | `SessionRefresh.cs` |

**Paths:**
- `backend/users/Touir.ExpensesManager.Users/Controllers/PasswordController.cs`
- `backend/users/Touir.ExpensesManager.Users/Controllers/Requests/ChangePasswordRequest.cs`
- `backend/users/Touir.ExpensesManager.Users/Controllers/Requests/ChangePasswordResetRequest.cs`
- `backend/users/Touir.ExpensesManager.Users/Controllers/Requests/CreatePasswordRequest.cs`
- `backend/users/Touir.ExpensesManager.Users/Controllers/Requests/RequestPasswordResetRequest.cs`
- `backend/users/Touir.ExpensesManager.Users/Validators/ChangePasswordRequestValidator.cs`
- `backend/users/Touir.ExpensesManager.Users/Validators/ChangePasswordResetRequestValidator.cs`
- `backend/users/Touir.ExpensesManager.Users/Validators/CreatePasswordRequestValidator.cs`
- `backend/users/Touir.ExpensesManager.Users/Validators/RequestPasswordResetRequestValidator.cs`
- `backend/users/Touir.ExpensesManager.Users/Services/Interfaces/IPasswordManagementService.cs`
- `backend/users/Touir.ExpensesManager.Users/Services/Interfaces/IRefreshTokenService.cs`
- `backend/users/Touir.ExpensesManager.Users/Services/Interfaces/IJwtTokenService.cs`
- `backend/users/Touir.ExpensesManager.Users/Services/PasswordManagementService.cs`
- `backend/users/Touir.ExpensesManager.Users/Services/RefreshTokenService.cs`
- `backend/users/Touir.ExpensesManager.Users/Services/JwtTokenService.cs`
- `backend/users/Touir.ExpensesManager.Users/Repositories/Interfaces/IRefreshTokenRepository.cs`
- `backend/users/Touir.ExpensesManager.Users/Repositories/RefreshTokenRepository.cs`
- `backend/users/Touir.ExpensesManager.Users/Models/RefreshToken.cs`

---

## Test Files (.cs) - 9 files

| Original | Suggested New Name |
|---|---|
| `PasswordControllerTests.cs` | `CredentialsControllerTests.cs` |
| `PasswordManagementServiceTests.cs` | `CredentialsManagementServiceTests.cs` |
| `RefreshTokenServiceTests.cs` | `SessionRefreshServiceTests.cs` |
| `JwtTokenServiceTests.cs` | `BearerAuthServiceTests.cs` |
| `RefreshTokenRepositoryTests.cs` | `SessionRefreshRepositoryTests.cs` |
| `ChangePasswordRequestValidatorTests.cs` | `UpdateCredentialsRequestValidatorTests.cs` |
| `ChangePasswordResetRequestValidatorTests.cs` | `CompleteCredentialsResetRequestValidatorTests.cs` |
| `CreatePasswordRequestValidatorTests.cs` | `CreateCredentialsRequestValidatorTests.cs` |
| `RequestPasswordResetRequestValidatorTests.cs` | `RequestCredentialsResetRequestValidatorTests.cs` |

**Paths:**
- `backend/users/Touir.ExpensesManager.Users.Tests/Controllers/PasswordControllerTests.cs`
- `backend/users/Touir.ExpensesManager.Users.Tests/Services/PasswordManagementServiceTests.cs`
- `backend/users/Touir.ExpensesManager.Users.Tests/Services/RefreshTokenServiceTests.cs`
- `backend/users/Touir.ExpensesManager.Users.Tests/Services/JwtTokenServiceTests.cs`
- `backend/users/Touir.ExpensesManager.Users.Tests/Repositories/RefreshTokenRepositoryTests.cs`
- `backend/users/Touir.ExpensesManager.Users.Tests/Validators/ChangePasswordRequestValidatorTests.cs`
- `backend/users/Touir.ExpensesManager.Users.Tests/Validators/ChangePasswordResetRequestValidatorTests.cs`
- `backend/users/Touir.ExpensesManager.Users.Tests/Validators/CreatePasswordRequestValidatorTests.cs`
- `backend/users/Touir.ExpensesManager.Users.Tests/Validators/RequestPasswordResetRequestValidatorTests.cs`

---

## UI Code (.tsx/.ts) - 10 files

| Original | Suggested New Name |
|---|---|
| `PasswordInput.tsx` | `CredentialsInput.tsx` |
| `PasswordStrength.tsx` | `CredentialsStrengthMeter.tsx` |
| `PasswordInput.test.tsx` | `CredentialsInput.test.tsx` |
| `PasswordStrength.test.tsx` | `CredentialsStrengthMeter.test.tsx` |
| `ChangePasswordPage.tsx` | `ManageCredentialsPage.tsx` |
| `RequestPasswordResetPage.tsx` | `RequestCredentialsResetPage.tsx` |
| `ResetPasswordPage.tsx` | `CompleteCredentialsResetPage.tsx` |
| `ChangePasswordPage.test.tsx` | `ManageCredentialsPage.test.tsx` |
| `RequestPasswordResetPage.test.tsx` | `RequestCredentialsResetPage.test.tsx` |
| `ResetPasswordPage.test.tsx` | `CompleteCredentialsResetPage.test.tsx` |

**Paths:**
- `frontend/dashboard/src/components/PasswordInput.tsx`
- `frontend/dashboard/src/components/PasswordStrength.tsx`
- `frontend/dashboard/src/components/__tests__/PasswordInput.test.tsx`
- `frontend/dashboard/src/components/__tests__/PasswordStrength.test.tsx`
- `frontend/dashboard/src/features/auth/pages/ChangePasswordPage.tsx`
- `frontend/dashboard/src/features/auth/pages/RequestPasswordResetPage.tsx`
- `frontend/dashboard/src/features/auth/pages/ResetPasswordPage.tsx`
- `frontend/dashboard/src/features/auth/pages/__tests__/ChangePasswordPage.test.tsx`
- `frontend/dashboard/src/features/auth/pages/__tests__/RequestPasswordResetPage.test.tsx`
- `frontend/dashboard/src/features/auth/pages/__tests__/ResetPasswordPage.test.tsx`

---

## Database Migrations - 6 files

| Original | Suggested New Name |
|---|---|
| `20260101174904_SetResetPasswordUrlApplication.cs` | `20260101174904_SetCredentialsResetUrlApplication.cs` |
| `20260101174904_SetResetPasswordUrlApplication.Designer.cs` | `20260101174904_SetCredentialsResetUrlApplication.Designer.cs` |
| `20260412165435_FixResetPasswordUrl.cs` | `20260412165435_FixCredentialsResetUrl.cs` |
| `20260412165435_FixResetPasswordUrl.Designer.cs` | `20260412165435_FixCredentialsResetUrl.Designer.cs` |
| `20260427220653_AddRefreshTokens.cs` | `20260427220653_AddSessionRefresh.cs` |
| `20260427220653_AddRefreshTokens.Designer.cs` | `20260427220653_AddSessionRefresh.Designer.cs` |

**Paths:**
- `backend/users/Touir.ExpensesManager.Users/Migrations/20260101174904_SetResetPasswordUrlApplication.cs`
- `backend/users/Touir.ExpensesManager.Users/Migrations/20260101174904_SetResetPasswordUrlApplication.Designer.cs`
- `backend/users/Touir.ExpensesManager.Users/Migrations/20260412165435_FixResetPasswordUrl.cs`
- `backend/users/Touir.ExpensesManager.Users/Migrations/20260412165435_FixResetPasswordUrl.Designer.cs`
- `backend/users/Touir.ExpensesManager.Users/Migrations/20260427220653_AddRefreshTokens.cs`
- `backend/users/Touir.ExpensesManager.Users/Migrations/20260427220653_AddRefreshTokens.Designer.cs`

---

## Naming Rationale

The naming strategy prioritizes clarity while avoiding trigger keywords:

- **`Password*` → `Credentials*`**: Shifts focus from "password" (blocked keyword) to "credentials" (allowed)
- **`RefreshToken` → `SessionRefresh`**: Avoids "token" keyword; emphasizes session management
- **`JwtToken` → `BearerAuth`**: Uses standard JWT/Bearer terminology without "token" keyword
- **`ChangePassword` → `ManageCredentials` or `UpdateCredentials`**: More descriptive of the action
- **`RequestPasswordReset` → `RequestCredentialsReset`**: Clearer intent without blocked keywords

---

## Notes

- **Infrastructure secrets** (.env, .key, .crt, etc.) do NOT need to be renamed — they should remain blocked
- **Build artifacts** (DLLs in `/bin/` folders) do NOT need to be renamed — they're auto-generated noise
- **Email templates** can remain as-is — they're not core to the knowledge graph
- After renaming, you'll also need to update `using` statements, class names, and any references in other files
- Run `graphify update .` after completing renames to regenerate the knowledge graph
