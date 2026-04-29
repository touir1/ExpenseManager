Run tests with coverage for the project(s) that have modified files. Detect scope from git status (added/modified/deleted files) or $ARGUMENTS.

## 1. Detect scope

Run `git status --short` and map changed paths to projects:
- Any file under `frontend/` → **frontend**
- Any file under `backend/expenses/` → **expenses**
- Any file under `backend/users/` → **users**
- If $ARGUMENTS names a project explicitly, use that instead.
- If nothing changed and no argument given, run all three.

## 2. Run tests with coverage

**Frontend** (`frontend/dashboard`):
```bash
cd frontend/dashboard && npm test -- --coverage --coverageReporters=text-summary
```

**Expenses backend** (`backend/expenses`):
```bash
cd backend/expenses && dotnet test --collect:"XPlat Code Coverage" --results-directory "./coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
```

**Users backend** (`backend/users`):
```bash
cd backend/users && dotnet test --collect:"XPlat Code Coverage" --results-directory "./coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
```

## 3. Report

- Pass/fail counts and any failures with file:line.
- Coverage summary per project (lines/branches covered %).
- If any test fails, investigate and fix before reporting done.

## 4. Maximize coverage

After all tests pass, check for untested public methods or branches in the changed files. Add missing tests to bring coverage up. Re-run to confirm improvement.
