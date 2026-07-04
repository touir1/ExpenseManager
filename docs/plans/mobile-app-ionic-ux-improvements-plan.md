# Mobile App (Ionic) UX Improvements — Implementation Plan

> Source: [ux-ui-improvements.md](ux-ui-improvements.md), section **"8. Mobile App (Ionic)"**
> Scope: `frontend/mobile` (Ionic/React/Capacitor) for all items **except 2.3 (receipt storage)**, which now spans `backend/expenses`, `infrastructure/`, `frontend/mobile`, and `frontend/dashboard` (web) — see below.
> Design guidance: `/ui-ux-pro-max` skill (see below)

---

## 0. Design guidance (`/ui-ux-pro-max`)

Run before/while implementing each item — do not skip:

```bash
python3 <plugin-cache>/ui-ux-pro-max/2.5.0/cli/assets/scripts/search.py "<keyword>" --domain ux
python3 <plugin-cache>/ui-ux-pro-max/2.5.0/cli/assets/scripts/search.py "<keyword>" --domain web
python3 <plugin-cache>/ui-ux-pro-max/2.5.0/cli/assets/scripts/search.py "<keyword>" --stack react-native
```

Findings already pulled for this plan (reuse, don't re-derive):

- **Touch & Interaction (CRITICAL):** min 44×44pt targets, 8px+ spacing between targets, haptic feedback for confirmations only (not overused), visual press feedback within 100ms.
- **Feedback:** empty states need message + action (not blank); loading >300ms needs skeleton/spinner, not frozen UI; destructive actions get confirm + optional undo; success/error toasts use `aria-live`/non-blocking.
- **Navigation:** bottom tab bar max 5 items (current app has 5 incl. center FAB — at the limit, don't add more), active tab state must be visually distinct, deep-link every top-level screen.
- **Lists (react-native stack guidance, applies conceptually to Ionic's virtual-scroll too):** memoize row renderers, stable keys, virtualize 50+ item lists.
- **Anti-patterns to avoid:** icon-only controls without `aria-label`, decorative-only animation, gesture-only interactions with no visible fallback control, color-only status signaling.
- **File upload/attachment (for 2.3):** show progress feedback for the upload step (not silent), constrain preview image size (`max-width:100%`/responsive), compress before upload rather than sending raw camera output.

Apply these checks to every item below before marking it done. Re-run a targeted `--domain ux` search per item if the fix touches a pattern not covered here (e.g. searchbar UX, custom date-range pickers).

---

## 1. Current file map (confirmed via codebase read)

| Concern | File |
|---|---|
| QuickAddModal | `frontend/mobile/src/features/expenses/pages/QuickAddModal.tsx` |
| Expense list (date grouping, swipe-to-delete) | `frontend/mobile/src/features/expenses/pages/ExpensesListPage.tsx` |
| Offline detection hook | `frontend/mobile/src/hooks/useNetworkSync.ts` |
| Offline queue hook | `frontend/mobile/src/hooks/useOfflineQueue.ts` |
| Dashboard page + period selector | `frontend/mobile/src/features/dashboard/pages/DashboardPage.tsx`, `frontend/mobile/src/features/dashboard/components/DashboardDateFilter.tsx` |
| Settings page | `frontend/mobile/src/features/settings/pages/SettingsPage.tsx` |
| Tab bar / router | `frontend/mobile/src/router.tsx` |
| i18n locales | `frontend/mobile/src/i18n/locales/{en,fr,es,de}/translation.json` |

Existing tests to update: `QuickAddModal.test.tsx`, `ExpensesListPage.test.tsx`, `useNetworkSync.test.ts`, `DashboardPage.test.tsx`, `DashboardDateFilter.test.tsx`, `SettingsPage.test.tsx` — all under matching `__tests__/`.

---

## 2. Items (🔴 High → 🟢 Low, matching source doc order)

### 🔴 2.1 QuickAddModal — field-level errors not visible

**Problem:** bottom-sheet form (`0.75` breakpoint) hides RHF/Zod errors below the fold.
**Fix:**
- On submit failure (`formState.errors` non-empty), programmatically expand sheet to `1` breakpoint (`IonModal`'s `setCurrentBreakpoint` via ref, or controlled `initialBreakpoint`/`breakpoints` prop + imperative call).
- Scroll first invalid field into view (`fieldRef.current?.scrollIntoView({behavior:'smooth', block:'center'})`), focus it (`focus-management` guideline).
- Keep using `Controller`+`onIonInput` pattern already established for IonInput/RHF in this codebase (per CLAUDE.md constraint — no `register()` spread on Ionic inputs).
**Files:** `QuickAddModal.tsx`
**Tests:** `QuickAddModal.test.tsx` — assert breakpoint change / focus call when Zod validation fails on submit.

---

### 🔴 2.2 No offline indicator in main expense list

**Problem:** `useNetworkSync` only used in `QuickAddModal`; `ExpensesListPage` gives no feedback when `navigator.onLine === false`.
**Fix:**
- Reuse `useNetworkSync` (or its underlying online-state signal) in `ExpensesListPage`.
- Render `IonBanner`/inline banner "You're offline — showing cached data" (`t('expenses.offlineBanner')`) when offline, above the list.
- Per ux guidance: don't block interaction — cached list stays scrollable/read-only, only pull-to-refresh should be disabled or show a toast explaining why it no-ops.
**Files:** `ExpensesListPage.tsx`, reuse `useNetworkSync.ts` (no changes expected there unless it needs to expose a plain boolean).
**Tests:** `ExpensesListPage.test.tsx` — mock offline state, assert banner renders; assert it's absent when online.

---

### 🔴 2.3 Receipt photo captured but never submitted → full receipt storage (Option B)

**Problem:** `QuickAddModal` captures via `Camera.getPhoto({resultType: CameraResultType.DataUrl, ...})` (`frontend/mobile/src/features/expenses/pages/QuickAddModal.tsx:84-96`), previews it locally (`receiptDataUrl` state, `<img>` at lines 301-305), but never uploads it — no `receiptUrl` field exists on `Expense`/`ExpenseDto`. **Confirmed via repo audit:** no object storage runs in the app stack today — `minio` exists only in `infrastructure/docker-compose-tools.yml` (CI/dev tooling, backs GitLab's registry), **not** in `infrastructure/docker-compose-apps.yml`. No `AWSSDK.S3`/`Minio` NuGet package anywhere in `backend/`.

**Decision: implement Option B — full upload/store/view/download.** This expands scope beyond mobile-only; treat as a sub-project inside this plan.

#### 2.3.1 Storage backend
- Add a `minio` service to `infrastructure/docker-compose-apps.yml` (new bucket, e.g. `expense-receipts`), separate from the CI-only `minio` in `docker-compose-tools.yml` — do not reuse that one, it's GitLab's registry backing store and shouldn't take app traffic.
- Add `Minio` (or `AWSSDK.S3`, since MinIO is S3-compatible) NuGet package to `backend/expenses`.
- New `IReceiptStorageService` (expenses service): `UploadAsync(Stream, contentType, expenseId) → storageKey`, `GetDownloadUrlAsync(storageKey)` (presigned URL, short expiry) or `GetStreamAsync(storageKey)` for a proxy-download endpoint — prefer proxy-through-API (simpler auth reuse, no presigned-URL/cookie-auth mismatch) over presigned URLs, consistent with this app's cookie-based auth model.
- Env config: new `EXPENSES_MANAGEMENT_EXPENSES_RECEIPTSTORAGE_*` prefix (`Endpoint`, `AccessKey`, `SecretKey`, `Bucket`), following the existing `GetValue("Key", envVar) ?? hardcoded` pattern used elsewhere in this codebase.

#### 2.3.2 Data model
- New nullable column on `Expense`: `ReceiptStorageKey string?` (or a separate `ExpenseReceipt` table if multiple receipts per expense should ever be supported — start with 1:1 nullable column, simpler, matches "a receipt" singular in the audit item).
- Migration: `AddExpenseReceiptStorageKey`.
- `ExpenseDto`: add `HasReceipt: bool` (computed from `ReceiptStorageKey != null`) — do **not** expose the raw storage key to clients, only a boolean + a link-generating endpoint.

#### 2.3.3 Backend endpoints (expenses service, `ExpenseController` or new `ExpenseReceiptController`)
- `POST /{id}/receipt` — `IFormFile`, reuse the CSV-import validation pattern already established in `ExpenseImportController.cs` (extension + content-type whitelist restricted to `image/jpeg`/`image/png`/`image/webp`, size cap e.g. 5MB, `CopyToAsync(MemoryStream)` first for a seekable stream). Validates the expense belongs to the requesting user (same ownership check pattern as `ExpenseService`). Overwrites any existing receipt (delete old object on replace).
- `GET /{id}/receipt` — streams the image back (`FileStreamResult`, same pattern as `ExpenseExportController.ExportCsvAsync`'s `File(stream, contentType, fileName)`), used for both "view" (`<img src>`, browser renders inline) and "download" (add `Content-Disposition: attachment` variant via a `?download=true` query flag, or a second route).
- `DELETE /{id}/receipt` — removes stored object + clears `ReceiptStorageKey`.
- All three behind the same auth/ownership checks as existing expense endpoints; rate-limited under `expenses_global` like other `ExpenseController` actions.

#### 2.3.4 nginx
- `infrastructure/configs/nginx/sites-available/expenses-manager.conf` — existing `location /api/expenses` block already proxies everything under that prefix to `expenses-service:9200`; new receipt routes need no new location block. **Do** raise `client_max_body_size` for this block (currently `10M`, fine for a 5MB image cap, but confirm the cap chosen in 2.3.3 fits under it).

#### 2.3.5 Mobile (`frontend/mobile`)
- `QuickAddModal.tsx`: after successful `POST /expenses` (add), if `receiptDataUrl` is set, convert data URL → `Blob` → `FormData` → `POST /{id}/receipt`. Show upload progress (per `/ui-ux-pro-max` file-upload guidance above — don't block the "expense added" success feedback on the receipt upload; treat it as best-effort follow-up with its own error toast if it fails, since the expense itself already saved).
- `ExpensesListPage.tsx`: expense row shows a small receipt icon when `expense.hasReceipt` is true; tapping opens the image in a full-screen `IonModal` viewer (`GET /{id}/receipt` as `<img src>`), with a download/share action (`Capacitor Filesystem`/`Share` plugin, or just browser-native long-press-save since it's an `<img>`).
- Add a delete-receipt affordance (trash icon inside the viewer modal) calling `DELETE /{id}/receipt`.

#### 2.3.6 Web (`frontend/dashboard`) — new requirement from this update
- `ExpenseForm.tsx` (add/edit): add a file input (`accept="image/jpeg,image/png,image/webp"`) styled consistently with the rest of the Hearth-token form (per constraints.md's design-token rules — no raw `bg-white`/`border-slate-*`). Show a thumbnail preview once selected/already-uploaded.
- On save, if a file is selected, `POST /{id}/receipt` after the expense create/update call succeeds (mirror the mobile best-effort-follow-up approach).
- `ExpensesPage.tsx`: table row + mobile card (existing `ExpenseCard`) get a receipt icon/thumbnail when `hasReceipt`; click opens a modal with the image (`GET /{id}/receipt`) plus a "Download" link/button (`<a href download>` or a blob-fetch-then-save, since the endpoint requires cookie auth — a plain `<a href>` works fine since `credentials:'include'` isn't needed for same-origin nginx-proxied cookie requests).
- Reuse existing modal size/shadow-token conventions flagged elsewhere in [ux-ui-improvements.md](ux-ui-improvements.md) section 16 (`shadow-warm`, Hearth surface tokens) rather than introducing a one-off modal style.

**Files:**
Backend: `backend/expenses/.../Models/Expense.cs`, new migration, `Controllers/DTO/ExpenseDto.cs`, new `IReceiptStorageService`/implementation, `ExpenseController.cs` or new `ExpenseReceiptController.cs`, `appsettings.json`.
Infra: `infrastructure/docker-compose-apps.yml`, `infrastructure/configs/nginx/sites-available/expenses-manager.conf` (+ `sites-enabled/` mirror).
Mobile: `QuickAddModal.tsx`, `ExpensesListPage.tsx`, `expensesApi.service.ts`, `expenses.type.ts`.
Web: `ExpenseForm.tsx`, `ExpensesPage.tsx`, `expensesApi.service.ts`/`expense.type.ts` (dashboard).

**Tests:**
- Backend: unit tests for `ExpenseReceiptController`/`IReceiptStorageService` (mock storage client) — upload validates content-type/size, ownership check rejects other users' expenses, delete clears the column, `Moq CS0854` pattern (`It.IsAny<CancellationToken>()`) for optional CT params per existing convention.
- Mobile: `QuickAddModal.test.tsx` (upload call fires after add succeeds, failure shows non-blocking error), `ExpensesListPage.test.tsx` (receipt icon renders when `hasReceipt`, viewer modal opens/deletes).
- Web: new/updated tests for `ExpenseForm.test.tsx` (file input + preview) and `ExpensesPage.test.tsx` (receipt icon, view/download modal).
- Do not skip backend tests just because this is framed as a "UX improvements" plan — the storage layer is new product surface, not cosmetic.

---

### 🟡 2.4 Swipe-to-delete has no undo

**Problem:** `IonItemSliding` + `IonItemOption color="danger"` in `ExpensesListPage.tsx` deletes immediately after the confirm alert — no recovery.
**Fix:**
- After successful delete, show a toast (reuse app's toast pattern if one exists on mobile, else `IonToast`) with an "Undo" button, 5s auto-dismiss.
- Backend already supports soft-delete (per CLAUDE.md: expenses `IsDeleted`/`SoftDeleteAsync`) — confirm a restore/un-delete endpoint exists or needs adding; if the API has no un-delete route, undo must re-`POST` the same expense payload client-side (only reliable option without a new backend endpoint) — check `expensesApi.service.ts` before deciding.
- Haptic (`Haptics.impact({style: ImpactStyle.Heavy})`) on the delete confirm tap — ties into 2.9 below, do together.
**Files:** `ExpensesListPage.tsx`, possibly `expensesApi.service.ts` if a restore call is added.
**Tests:** `ExpensesListPage.test.tsx` — assert undo toast appears post-delete, assert clicking "Undo" restores the row / re-adds the expense.

---

### 🟡 2.5 Date grouping — no "Today"/"Yesterday"

**Problem:** `IonItemDivider` (`ExpensesListPage.tsx:169-175`) always renders `toLocaleDateString(weekday, month, day)`, even for the last 2 days.
**Fix:** small date-label helper — `isToday(date) → t('common.today')`, `isYesterday(date) → t('common.yesterday')`, else existing locale format. Extract as a pure function so it's unit-testable in isolation.
**Files:** `ExpensesListPage.tsx` (inline helper or new `frontend/mobile/src/features/expenses/utils/dateGroupLabel.ts`)
**Tests:** new/updated test — table-driven: today's date → "Today", yesterday → "Yesterday", 3 days ago → localized weekday string.

---

### 🟡 2.6 No search/filter on mobile expense list

**Problem:** only family filter exists; web has full filter bar (category/date/description).
**Fix:**
- Add `IonSearchbar` above the list, client-side filter over already-loaded pages by `description` (debounced, per `debounce-throttle` guideline), plus trigger a server-side search (extend `expensesApi.service.ts` list call with a `search` query param — confirm backend `ExpenseFilterDto` supports a description filter; if not, this needs a backend addition — flag as a dependency, don't silently add scope).
- Follow `search-accessible` guideline: keep the searchbar reachable without extra taps (top of list, not behind a menu).
**Files:** `ExpensesListPage.tsx`, `expensesApi.service.ts`, `expenses.type.ts` (filter type), possibly backend `ExpenseFilterDto`/`ExpenseController` if server-side search doesn't exist yet — confirm before implementing.
**Tests:** `ExpensesListPage.test.tsx` — typing in searchbar filters visible rows; debounce behavior (fake timers).

---

### 🟡 2.7 Dashboard period selector — no custom range

**Problem:** `DashboardDateFilter.tsx` only offers `month`/`6m`/`year` segments — no ad-hoc range.
**Fix:**
- Add a 4th `IonSegmentButton value="custom"` that reveals two `IonDatetimeButton`+`IonModal`+`IonDatetime` pickers (from/to).
- Keep the `getPeriodDates`-style return contract (`{dateFrom, dateTo, period}`) so `DashboardPage.tsx` doesn't need restructuring — `period: 'custom'` just carries user-picked dates instead of computed ones.
- Validate `from <= to` (same rule as web's `DashboardFilters.tsx` per existing CLAUDE.md pattern) — clamp or show inline error.
**Files:** `DashboardDateFilter.tsx`, `DashboardPage.tsx` (if it branches on `Period` type)
**Tests:** `DashboardDateFilter.test.tsx` — selecting "custom" reveals date pickers, changing dates calls `onChange` with correct shape, `from > to` is rejected/clamped.

---

### 🟡 2.8 Mobile SettingsPage underdeveloped vs web

**Problem:** current `SettingsPage.tsx` has display currency, language, theme, sign-out — no default category, no notification preferences, no data export, no account deletion (web already has notification prefs + account deletion per CLAUDE.md).
**Fix:** bring to parity with web where a backend endpoint already exists (avoid inventing new backend work in a mobile-UX plan):
- Default category selector (`GET/PUT /config` already used by `useExpensesData`/`userConfigApi.service.ts` on web — mobile has `userConfigApi.service.ts` already, confirm it exposes the same call).
- Notification preferences toggle list (`GET/PUT /config/notifications` — already exists per CLAUDE.md, web has this wired; mobile does not yet).
- Account deletion (`DELETE /me` — already exists per CLAUDE.md self-service deletion).
- Data export (CSV `GET /export`) — lower priority, can defer to a follow-up if time-boxed.
**Files:** `SettingsPage.tsx`, `userConfigApi.service.ts` (mobile), possibly new `notificationPreferencesApi.service.ts` mirroring the web one.
**Tests:** `SettingsPage.test.tsx` — new sections render, save calls hit the right endpoints, account-deletion confirm flow.

---

### 🟢 2.9 No haptic feedback on destructive actions

**Problem:** success add has `Haptics.impact({style: ImpactStyle.Medium})` in `QuickAddModal.tsx`; delete confirm (`ExpensesListPage.tsx`) has none — the riskiest action has the least feedback.
**Fix:** `Haptics.impact({style: ImpactStyle.Heavy})` in the `IonAlert`'s delete-confirm handler. Do this alongside 2.4 (same interaction).
**Files:** `ExpensesListPage.tsx`
**Tests:** `ExpensesListPage.test.tsx` — mock `@capacitor/haptics`, assert `impact` called with `Heavy` on confirm.

---

### 🟢 2.10 Loading skeleton count hardcoded (5)

**Problem:** `ExpensesListPage.tsx:147` — `[1,2,3,4,5].map(...)` doesn't scale to tablet viewport height.
**Fix:** compute `Math.max(5, Math.ceil(window.innerHeight / ITEM_HEIGHT_PX))`, capped at a sane max (e.g. 20) to avoid runaway render on huge screens. Define `ITEM_HEIGHT_PX` as a named constant from the actual row height.
**Files:** `ExpensesListPage.tsx`
**Tests:** `ExpensesListPage.test.tsx` — mock `window.innerHeight` at a large value, assert skeleton count scales; small screen still gets ≥5.

---

### 🟢 2.11 Tab bar labels not localized

**Problem:** `router.tsx:44,49,58,63` — `<IonLabel>Dashboard</IonLabel>`, `Expenses`, `Family`, `Settings` are hardcoded strings, won't change with language setting (mobile already supports `en/fr/es/de` — see SettingsPage language switcher).
**Fix:** wrap each in `t('nav.dashboard')`, `t('nav.expenses')`, `t('nav.families')`, `t('nav.settings')` — reuse existing `nav.*` keys already present in `SettingsPage.tsx`'s locale files if they match, otherwise add missing keys across all 4 locale JSON files.
**Files:** `router.tsx`, `i18n/locales/{en,fr,es,de}/translation.json`
**Tests:** no existing `router.tsx` test — add a minimal one (render `TabsLayout` under each locale, assert label text matches translation) or skip if router isn't unit-tested elsewhere and cost outweighs value — use judgment at implementation time.

---

## 3. Testing

For every item above:
- Update the existing `__tests__` file for the touched component, don't create a parallel test file.
- New pure helpers (e.g. date-group label, skeleton-count calc) get their own small test file if extracted.
- Run `npm test` (frontend/mobile) after each item, not just at the end — catch regressions early.
- Do not lower coverage thresholds to make tests pass.

---

## 4. Close-out

1. After all items in section 2 are implemented and tested, update [ux-ui-improvements.md](ux-ui-improvements.md) section **"8. Mobile App (Ionic)"**: mark each finished item `✅` with a "Done:" line (same format as the rest of that document), same as done for section 7 earlier.
2. Also update section 15 ("Mobile-vs-Web Feature Parity Gaps") rows affected by 2.6/2.8 (search/filter, settings parity) **and the "Receipt capture" row** (currently `❌ N/A` web / `⚠️ Captured but not stored` mobile / `🔴 Fix or remove`) — flip to `✅ Done` both columns once 2.3 ships, since it's now implemented on both platforms.
3. Update section 14 ("Bigger Initiatives") — the `Receipt storage API + mobile upload` row (currently listed as High/High) should flip to `✅ Done` once 2.3 ships.
4. Run `/done`.
