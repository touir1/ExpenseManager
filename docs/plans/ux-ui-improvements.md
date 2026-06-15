# UX/UI Improvements — ExpenseManager

> Audit date: 2026-06-14  
> Scope: `frontend/dashboard` (React/Tailwind) + `frontend/mobile` (Ionic/React/Capacitor)  
> Priority legend: 🔴 High · 🟡 Medium · 🟢 Low

---

## 1. Navigation & Information Architecture

### ✅ 🔴 Web: No active page indicator in mobile hamburger menu

**Problem:** Desktop navbar highlights the active route (brand background on active link). When the hamburger menu opens on small screens, the same links lose their active visual state — user has no "you are here" cue.  
**Fix:** Apply the same active class logic to mobile menu items as desktop nav links.  
**Done:** Mobile hamburger links now use `navLinkClass` (same `isActive`-based function as desktop) — `NavBar.tsx`.

---

### ✅ 🔴 Web: FamilySelector and DisplayCurrencySelector are always visible but contextually confusing

**Problem:** Both dropdowns appear in the navbar at all times, even on pages where they have no effect (Settings, Families). Users may change the family filter expecting something to happen on the Settings page.  
**Fix:** Hide/disable navbar selectors on pages that don't consume them, or add a subtle tooltip explaining context ("Filters expenses only").  
**Done:** Added `showContextSelectors` boolean (`/dashboard` or `/expenses` only); both selectors conditionally render — `NavBar.tsx`.

---

### ✅ 🟡 Web: "Add Expense" button in navbar duplicates button on Expenses page

**Problem:** Two entry points for the same action with no visual hierarchy difference. First-time users may not notice the one in the navbar.  
**Fix:** Keep navbar button as the primary global CTA, make the in-page button secondary (outline style). Add tooltip "Quickly add an expense" to navbar button.  
**Done:** In-page button demoted to outline style; navbar button has `title={t('nav.addExpenseTooltip')}` — `ExpensesPage.tsx`, `NavBar.tsx`, all 4 locale files.

---

### ✅ 🟡 Mobile: No breadcrumb / back-navigation pattern for nested flows

**Problem:** Entering a sub-flow (e.g. accepting a family invite via deep link) offers no consistent back path.  
**Fix:** Ensure every IonPage that can be pushed onto the nav stack has an IonBackButton in the toolbar.  
**Done:** Created `AcceptInvitePage.tsx` (mobile) with `IonBackButton defaultHref="/families"`; route `/families/accept-invite` added outside TabsLayout in `router.tsx`.

---

### ✅ 🟢 Web: Admin nav item is invisible until page load resolves `isAdmin`

**Problem:** On first render, admin users see no Admin link (it flashes in after the session check). During this 200–500 ms window the layout shifts.  
**Fix:** Reserve navbar space for admin link during auth loading (skeleton placeholder or keep the gap).  
**Done:** Skeleton `<span>` renders via `authLoading && !user` guard before admin `NavLink` — `NavBar.tsx`.

---

## 2. Dashboard

### ✅ 🔴 Web: Dashboard date filters reset to "month-to-date" on every page load

**Problem:** If a user sets a custom date range to investigate last quarter, refreshing the page resets it. No persistence.  
**Fix:** Persist date range in URL query params (`?from=&to=`) so the URL is shareable and survives refresh.  
**Done:** `HomeDashboardPage` replaced `useState` with `useSearchParams`; date range reads/writes `?from=&to=` query params — `HomeDashboardPage.tsx`.

---

### ✅ 🔴 Web: No empty-state on dashboard for new users

**Problem:** A brand-new account with no expenses shows empty/zero charts with no explanation. SpendChart renders an empty line chart; CategoryDonut is a blank circle.  
**Fix:** Add empty-state illustrations or callout cards ("No expenses yet — add your first one!") when all queries return zero data.  
**Done:** `EmptyDashboard` component renders when `expenseCount === 0` and all queries loaded; replaces the grid with a centered card + CTA — `HomeDashboardPage.tsx`.

---

### ✅ 🟡 Web: Dashboard charts have no drill-down

**Problem:** Clicking a category slice on CategoryDonut or a bar on SameMonthChart does nothing. Users cannot discover which expenses compose a data point.  
**Fix:** Make chart elements clickable — navigate to `/expenses` pre-filtered by category and date range. This is a high-value feature with modest implementation cost using existing filter infrastructure.  
**Done:** `CategoryDonut` accepts `onCategoryClick` prop; pie slices and legend rows navigate to `/expenses?categoryId=X&dateFrom=Y&dateTo=Z`; `ExpensesPage` reads these URL params as initial filter state — `CategoryDonut.tsx`, `HomeDashboardPage.tsx`, `ExpensesPage.tsx`.

---

### ✅ 🟡 Web: MonthHero percentage change has no context tooltip

**Problem:** The % change vs last month badge is shown without explaining the comparison period (last month? last year?).  
**Fix:** Add a `title` attribute or `<Tooltip>` component that says "Compared to {previousMonthName}".  
**Done:** Delta badge has `title={comparedToLabel}` where label is computed from active date range (e.g. "Compared to May 1 – May 31") — `MonthHero.tsx`, `HomeDashboardPage.tsx`.

---

### ✅ 🟡 Web: DashboardFilters date range allows `from > to` without validation

**Problem:** A user can set end date before start date, which sends an invalid query to the backend and silently returns empty data.  
**Fix:** Clamp `to` min to `from` value, or show a validation error inline in the filter bar.  
**Done:** `onChange` handlers clamp automatically (moving `from` past `to` resets `to`; vice versa); inline `role="alert"` error shown with `border-berry` on both inputs when invalid — `DashboardFilters.tsx`.

---

### 🟡 Mobile: Period selector (month/week/year) and DisplayCurrency selector are separate — interaction is non-obvious

**Problem:** Changing period does not visually indicate it affects the currency display too. The relationship is implicit.  
**Fix:** Group the two selectors visually inside one toolbar segment with a divider, or merge into one combined picker.

---

### ✅ 🟢 Web: Charts don't show tooltips for zero-value data points

**Problem:** Hovering over a date where there are no expenses shows nothing. Users can't confirm "was I really at zero that day?".  
**Fix:** Enable `Recharts` tooltips for zero values with explicit "No expenses" label.  
**Done:** `minPointSize={2}` on all Bar elements makes zero-height bars hoverable; Tooltip formatter returns `t('dashboard.charts.noExpenses')` when value is 0 — `SpendChart.tsx`, `SameMonthChart.tsx`.

---

### ✅ 🟢 Web: CurrenciesPanel has no visual chart

**Problem:** CurrenciesPanel shows a plain text/table breakdown while all sibling panels have charts.  
**Fix:** Add a small horizontal bar chart per currency for visual consistency.  
**Done:** Each currency row has a proportional `bg-brand-400` horizontal bar (width = `totalAmount / max * 100%`, min 3%) — `CurrenciesPanel.tsx`.

---

## 3. Expense Management (Web)

### 🔴 Web: ExpensesPage shows table on mobile with no horizontal scroll affordance

**Problem:** The table has 8 columns. On screens < 768px the table overflows or squishes. No scroll hint visible.  
**Fix:** On mobile breakpoints switch to a card list layout (same as mobile app) or add `overflow-x-auto` with a scroll indicator.

---

### 🔴 Web: Pagination controls — no "go to page N" or total item count

**Problem:** Users see "Page 2 of 47" but can't jump to page 40. With large datasets this requires many clicks.  
**Fix:** Add a "jump to page" input and display total expense count ("Showing 21–40 of 940 expenses").

---

### 🔴 Web: Delete confirmation modal lacks expense details

**Problem:** ConfirmDeleteModal says something like "Are you sure?" without showing which expense is being deleted (amount, date, description). Easy to delete the wrong item.  
**Fix:** Show the expense summary (amount + date + description preview) in the confirmation modal body.

---

### 🟡 Web: ExpenseForm subcategory field stays enabled with no options available

**Problem:** If a category has no subcategories, the subcategory `FormCombobox` is enabled but empty — confusing to users who expect something to appear.  
**Fix:** Also hide the subcategory field entirely when the selected category has no subcategories. Show it only when options exist.

---

### 🟡 Web: Expense table "Edit" and "Delete" are small text links — low discoverability

**Problem:** Actions are small blue/red text in the rightmost column. Invisible on first glance; easy to misclick on narrow rows.  
**Fix:** Replace with icon buttons (pencil, trash) with `aria-label`. Larger tap/click target; consistent with most modern expense apps.

---

### 🟡 Web: ExpenseForm — no "save & add another" shortcut

**Problem:** After adding an expense, the modal closes and the user must re-open it to add another. Tedious for bulk entry sessions.  
**Fix:** Add a secondary "Save & Add Another" button that submits and resets the form without closing the modal.

---

### 🟡 Web: Tag input — no visual affordance that Enter/comma creates a tag

**Problem:** The TagInput component accepts text but doesn't indicate how to confirm a tag. First-time users type and see nothing happen until they press Enter.  
**Fix:** Add a placeholder hint ("Type and press Enter to add") and a `+` icon button that creates the tag on click.

---

### 🟡 Web: Expense filters — no "clear all filters" shortcut

**Problem:** Multiple active filters (category + date range + family) must each be cleared individually.  
**Fix:** Show "Clear filters" link/button when any non-default filter is active.

---

### 🟢 Web: No keyboard shortcut to open "Add Expense" modal

**Problem:** Power users must click a button to start. Other financial apps (e.g. Splitwise, YNAB) offer keyboard shortcuts.  
**Fix:** Register `N` or `Ctrl+E` as a global shortcut that opens AddExpenseModal.

---

### 🟢 Web: Amount input — no thousands separator display while typing

**Problem:** Typing `2430.50` shows raw digits. Hard to read large amounts without visual grouping.  
**Fix:** Format display value with locale-aware grouping on blur while keeping raw value in form state.

---

## 4. CSV Import (Web)

### 🔴 Web: Import progress — no feedback while uploading large files

**Problem:** After clicking "Upload", the UI shows nothing until the server responds. For 500-row files this can take 3–5 s. Users may double-click.  
**Fix:** Show an indeterminate progress bar / spinner immediately after file selection, disable the upload zone.

---

### 🔴 Web: No way to re-download the original uploaded file

**Problem:** If the user closes the import page mid-flow and returns, the file is gone. They cannot retrieve it.  
**Fix:** Not fixable at browser level without server storage, but: add a warning when navigating away from an active import session ("Unsaved import — leave?").

---

### 🟡 Web: Import table — edited rows are not visually distinct from valid rows

**Problem:** After editing a row and saving it, the amber "editing" badge disappears and the row looks identical to rows that were never edited. No audit trail of what was changed.  
**Fix:** Show a subtle "Edited" badge or dot indicator on rows that were modified by the user.

---

### 🟡 Web: No column sorting on import preview table

**Problem:** With 100+ rows, finding all error rows requires scrolling. Status column is not sortable.  
**Fix:** Add one-click sort by Status (errors first) — the most requested feature in bulk-import tools.

---

### 🟡 Web: Template download is a link, not clearly labelled

**Problem:** "Download template" link is easy to overlook on the upload zone.  
**Fix:** Style as a secondary button with a download icon. Add a short sentence explaining what columns are expected.

---

### 🟢 Web: No column mapping step

**Problem:** If a user's CSV has columns named `sum`, `cur`, `cat` the import will fail. The preview step shows raw column values but no way to remap.  
**Fix:** Add optional column-mapping step between upload and preview: auto-detect common alternatives (amt/amount, cat/category), let user confirm or override.

---

## 5. Families Management (Web)

### 🟡 Web: FamilyCard expand/collapse animation is instant — no transition

**Problem:** When clicking the chevron, the member list appears/disappears with no animation. Jarring.  
**Fix:** Add a CSS height transition (e.g. `max-height` transition from 0 to auto, or use Headless UI Disclosure).

---

### 🟡 Web: Invite flow — no way to cancel a pending invitation

**Problem:** Once an invitation email is sent, there is no UI to revoke it before it's accepted.  
**Fix:** Show pending invitations in the family member list with a "Revoke" button (requires a new API endpoint: `DELETE /families/{id}/invitations/{token}`).

---

### 🟡 Web: Archive confirmation is missing

**Problem:** Clicking the archive icon immediately archives the family without confirmation. Archiving hides all attributions — hard to undo.  
**Fix:** Show a confirmation modal: "Archive {familyName}? Expenses attributed to this family will remain but the family will be hidden."

---

### 🟢 Web: Family tab badge counts don't update live

**Problem:** Active (3) / Archived (1) tab counts are loaded once and don't update after create/archive actions without a page refresh.  
**Fix:** Derive counts from the `families` array in context which already updates on mutation. Tab badges should be computed values, not server-fetched separate counters.

---

## 6. Settings (Web)

### 🟡 Web: SettingsPage is sparse — only 3 settings

**Problem:** The 2–3 column grid of settings cards looks empty. Users expect more customization options.  
**Suggestions for additional settings:**
  - Default expense date: today vs last used
  - Default category (for quick-add)
  - Default currency (already there, keep)
  - Notifications preferences (email on/off per event type)
  - Data export (download all expenses as CSV)
  - Account deletion

---

### 🟡 Web: Theme toggle is in NavBar user dropdown AND SettingsPage — duplication

**Problem:** Two places to change the theme creates confusion about which is canonical.  
**Fix:** Keep it in Settings only. In the NavBar dropdown, show a quick-access link to Settings instead.

---

### 🟢 Web: Default currency change has no visual confirmation after save

**Problem:** After clicking "Save", the field looks the same. No toast confirmation, no "saved" indicator. (If there is a toast, its timing may be too short to notice.)  
**Fix:** Show a brief inline "Saved ✓" state on the save button that reverts after 2 s (same pattern as most modern web apps).

---

## 7. Notifications (Web & Mobile)

### 🟡 Web/Mobile: Notification bell shows count but items have no category icon

**Problem:** All notifications look the same (same list item style). Users must read the text to identify the type.  
**Fix:** Prepend a small colored icon per notification type (👥 family events, 💸 expense events, 📊 import, ⚠️ rate conflicts). Adds scannability.

---

### 🟡 Web: Notification dropdown has no "view all" link

**Problem:** Max-height 320px means only ~5–6 notifications are visible. Users with many notifications can scroll but there's no full-page view.  
**Fix:** Add `/notifications` route with a full-page notification inbox. Link from dropdown header.

---

### 🟡 Web: Toasts for new notifications can stack and obscure content

**Problem:** If multiple notifications arrive in quick succession (e.g. 3 family events), 3 toasts stack.  
**Fix:** Rate-limit toast notifications to one per 3 s, or collapse multiple into "3 new notifications" toast.

---

### 🟢 Web: No notification preferences — all events generate notifications

**Problem:** Power users in active families may find the notification volume excessive (every expense added triggers a bell).  
**Fix:** Add per-event-type toggle in Settings > Notifications.

---

## 8. Mobile App (Ionic)

### 🔴 Mobile: QuickAddModal — no field-level error messages visible

**Problem:** The bottom sheet form submits via RHF + Zod. If validation fails, errors display but may be below the fold in the half-height (0.75 breakpoint) state. User may not know why submit failed.  
**Fix:** Auto-expand to full height on first validation error. Scroll the first error into view.

---

### 🔴 Mobile: No offline indicator in the main expense list

**Problem:** `useNetworkSync` detects offline status in `QuickAddModal`, but the main expense list has no indicator when the device is offline. A user who pulls to refresh while offline gets no feedback.  
**Fix:** Show an IonBanner ("You're offline — showing cached data") when `navigator.onLine === false`.

---

### 🔴 Mobile: Receipt photo captured but never submitted to backend

**Problem:** `QuickAddModal` captures a photo via `Camera.getPhoto()` and shows a preview, but looking at the add expense API, there's no `receiptUrl` field in the expense DTO. The photo is captured but silently dropped.  
**Fix (option A):** Remove the camera FAB until a receipt storage API exists — avoid the illusion of a feature.  
**Fix (option B):** Upload the image to a storage service (S3/MinIO) as part of the submit flow and store the URL.

---

### 🟡 Mobile: Swipe-to-delete has no undo mechanism

**Problem:** A slip of the finger on the expense list triggers a delete alert. After confirming, the deletion is permanent. No undo toast.  
**Fix:** After successful delete, show a toast with "Undo" action for 5 s that calls a restore/un-delete endpoint. (Requires soft-delete API support — which the backend already has.)

---

### 🟡 Mobile: Date grouping in expense list uses relative dates inconsistently

**Problem:** `IonItemDivider` shows "Mon, Jan 15" but does not say "Today" or "Yesterday" for recent groups. Competitors (Monzo, Revolut) always use relative dates for the most recent 2 days.  
**Fix:** Format group headers as "Today", "Yesterday", then localized date for older groups.

---

### 🟡 Mobile: No search/filter capability on expense list

**Problem:** The web app has a full filter bar (category, date, description search). The mobile list only allows family filtering. Power users manage hundreds of expenses and need to find specific ones.  
**Fix:** Add a searchbar (IonSearchbar) that filters by description client-side (for loaded pages) and triggers a server search on blur.

---

### 🟡 Mobile: Dashboard period selector (month/week/year) lacks custom date range

**Problem:** Business users often need ad-hoc ranges (e.g. "last 10 days"). The period tabs cover most cases but not all.  
**Fix:** Add a "Custom" tab that opens an IonDatetimeButton date range picker.

---

### 🟡 Mobile: SettingsPage is underdeveloped vs web

**Problem:** The mobile settings screen is not documented in the audit — likely a stub or minimal screen. Web settings already has 3 cards; mobile may have less.  
**Fix:** Ensure mobile settings matches web settings feature parity (password change, default currency, theme, notifications).

---

### 🟢 Mobile: No haptic feedback on destructive actions

**Problem:** Haptic feedback (`ImpactStyle.Medium`) fires on successful expense add. No haptic on delete confirmation — the riskiest action.  
**Fix:** Add `Haptics.impact({ style: ImpactStyle.Heavy })` when the user taps "Delete" in the IonAlert.

---

### 🟢 Mobile: Loading skeleton item count (5) is hardcoded

**Problem:** On large screens (tablet) 5 skeleton items don't fill the screen. On small screens this is fine.  
**Fix:** Compute skeleton count as `Math.ceil(window.innerHeight / ITEM_HEIGHT)` to fill viewport.

---

### 🟢 Mobile: Tab bar labels not localized

**Problem:** If tab labels ("Dashboard", "Expenses", "Families", "Settings") are hardcoded strings rather than using `t()`, they won't change with the language setting.  
**Fix:** Wrap all tab labels in `t('nav.dashboard')` etc.

---

## 9. Accessibility Gaps

### 🔴 Web/Mobile: FormCombobox dropdown not keyboard-navigable

**Problem:** The custom `FormCombobox` component (used for currency, category, subcategory) is implemented as a custom dropdown. Arrow key navigation, Home/End, and type-ahead character selection are not confirmed to be implemented.  
**Fix:** Ensure the combobox follows [ARIA Combobox Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/combobox/): `role="combobox"`, `aria-expanded`, `aria-activedescendant`, `aria-autocomplete`, and full keyboard support (Arrow Down/Up, Enter to select, Escape to close).

---

### 🔴 Web: Toast notifications are not announced to screen readers

**Problem:** Toasts appear visually in the top-right corner but are not in an ARIA live region. Screen reader users will never hear them.  
**Fix:** Wrap the toast container in a `<div role="status" aria-live="polite" aria-atomic="true">` (or `"assertive"` for errors).

---

### 🟡 Web: Modal focus management — focus does not return to trigger element on close

**Problem:** When AddExpenseModal or ConfirmDeleteModal closes, focus should return to the element that opened it (the "Add Expense" button or the row's "Delete" link). Without this, keyboard users lose their place.  
**Fix:** Store a `triggerRef` before opening modal; call `triggerRef.current?.focus()` on modal close.

---

### 🟡 Web: Color is the only differentiator for valid/invalid import rows

**Problem:** Valid rows are white, invalid rows are red-background. For color-blind users (protanopia/deuteranopia), red may be indistinguishable from white.  
**Fix:** Add a status icon column (✓ / ✕) in addition to color. Error count badge text is sufficient for badge-only indicators.

---

### 🟡 Web: NotificationBell badge has no accessible label for count

**Problem:** The red badge shows "3" visually but the bell's `aria-label` may say "Notifications" without including the count.  
**Fix:** Update aria-label dynamically: `aria-label={t('notifications.bell', { count: unreadCount })}` → "3 unread notifications".

---

### 🟢 Web: Charts (Recharts) have no accessible data table fallback

**Problem:** SVG charts are invisible to screen readers. Users navigating by keyboard/screen reader get no spending data from the dashboard.  
**Fix:** Add `<caption>` and a visually-hidden `<table>` with the underlying data next to each chart. Recharts doesn't do this automatically.

---

## 10. Performance & Loading States

### 🟡 Web: All 6 dashboard queries fire in parallel — no priority ordering

**Problem:** On slow connections all 6 queries start simultaneously, each competing for bandwidth. The most visible component (MonthHero) may load last.  
**Fix:** Use TanStack Query's `staleTime` and `gcTime` to keep MonthHero data fresh longest. Consider sequential loading (MonthHero → SpendChart → rest) with a waterfall approach for slow connections.

---

### 🟡 Web: ExpensesPage shows "Loading…" text — not skeleton

**Problem:** All other loading states use skeleton placeholders, but ExpensesPage shows plain "Loading…" text. Inconsistent.  
**Fix:** Replace with skeleton rows matching the table layout (consistent with MonthHero and other panels).

---

### 🟡 Web: No image optimization for AuthBrandPanel background

**Problem:** The brand panel likely uses a background image or gradient. No lazy-loading or WebP optimization observed.  
**Recommendation:** Use `loading="lazy"` and WebP format for any images. Use pure CSS gradients where possible.

---

### 🟢 Web: FormCombobox renders all options without virtualization

**Problem:** The currency list has 100+ options. Rendering all as DOM nodes on open is slow.  
**Fix:** Add windowing (`react-window` or manual slice) — show max 50 options, rely on search to narrow.

---

## 11. Visual Design & Polish

### 🟡 Web: Dark mode missing from dashboard charts

**Problem:** ThemeContext provides dark mode, but Recharts chart tooltips and axes use hardcoded colors (likely `#333`, `#fff`). In dark mode these will look wrong.  
**Fix:** Pass theme-aware colors to Recharts props: `stroke={theme === 'dark' ? '#cbd5e1' : '#334155'}` etc.

---

### 🟡 Web: Inconsistent modal sizes

**Problem:** AddExpenseModal, ConfirmDeleteModal, InviteMemberModal, and CreateFamilyModal have different max-widths. No modal size scale.  
**Fix:** Define `sm`/`md`/`lg` modal size classes in `index.css` and apply consistently.

---

### 🟡 Web: Tables lack row hover cursor — no affordance that rows are clickable

**Problem:** In the expense table, rows have a hover darken effect but no pointer cursor change. Users don't know if they can click the row itself.  
**Fix:** Either make rows fully clickable (click row → edit expense) with `cursor-pointer`, or remove the hover darkening to avoid false affordance.

---

### 🟢 Web: No favicon or PWA manifest

**Problem:** Tab icon is the browser default. No `<link rel="manifest">` for installability.  
**Fix:** Add SVG favicon using brand color + "E" logo. Add `manifest.json` for PWA install support.

---

### 🟢 Mobile: App icon and splash screen not customized

**Problem:** `android/` and `ios/` are gitignored but Capacitor generates default Ionic splash screens. The brand icon should be applied.  
**Recommendation:** Use `@capacitor/assets` to generate icons/splash from a single source SVG before production builds.

---

### 🟢 Web: Empty pagination state ("Page 1 of 1") always shows even with no records

**Problem:** When the expense list is empty, pagination still renders "Page 1 of 0" or "Page 1 of 1". Looks broken.  
**Fix:** Hide pagination controls when total pages ≤ 1.

---

## 12. UX Copy & Microcopy

### 🟡 Web: Error messages use internal error codes exposed to users

**Problem:** Messages like `FAMILY_NAME_ALREADY_EXISTS` or `CATEGORY_NAME_DUPLICATE` from the API may reach user-facing toast messages if the i18n key is missing.  
**Fix:** Ensure every backend error code has a corresponding i18n translation key. Add a global fallback: `t(errorCode, errorCode)` → shows the code only if key is missing, then alert developers.

---

### 🟡 Web/Mobile: Empty states use only text — no illustration or CTA

**Problem:** "No expenses yet" is informational but doesn't tell the user what to do next.  
**Fix:** Add consistent empty-state pattern: small illustration + bold "Nothing here yet" + CTA button ("Add your first expense →").

---

### 🟢 Web: Form submit button label is static ("Save", "Add")

**Problem:** After clicking submit, the button shows a spinner but the label doesn't change to indicate what's happening.  
**Fix:** Change button label during submission: "Save" → "Saving…", "Add" → "Adding…". This is more descriptive than a spinner alone.

---

## 13. Quick Wins (≤ 1 hour each)

| # | Fix | File | Effort |
|---|-----|------|--------|
| 1 | Hide pagination when total ≤ 1 page | `ExpensesPage.tsx` | 5 min |
| 2 | Add `cursor-pointer` or remove hover state from non-clickable rows | `ExpensesPage.tsx` | 5 min |
| 3 | Add `aria-live="polite"` to Toast container | `Toast.tsx` | 10 min |
| 4 | Dynamic `aria-label` on notification bell (include count) | `NotificationBell.tsx` | 10 min |
| ~~5~~ | ~~Clamp dashboard date range `to` min = `from`~~ | ~~`DashboardFilters` component~~ | ✅ Done |
| 6 | Add "Clear filters" button on ExpensesPage | `ExpensesPage.tsx` | 20 min |
| 7 | Replace "Loading…" text with skeleton in ExpensesPage | `ExpensesPage.tsx` | 30 min |
| ~~8~~ | ~~Validate date range (from ≤ to) in DashboardFilters~~ | ~~`DashboardFilters` component~~ | ✅ Done |
| 9 | Add archive confirmation modal | `FamiliesPage.tsx` | 30 min |
| 10 | Show expense summary in delete confirmation modal | `ConfirmDeleteModal` | 20 min |
| 11 | Hide subcategory field when category has no subcategories | `ExpenseForm.tsx` | 15 min |
| 12 | Mobile: "Today" / "Yesterday" group headers in expense list | `ExpensesListPage.tsx` | 20 min |
| 13 | Mobile: Offline banner on expense list | `ExpensesListPage.tsx` | 30 min |
| ~~14~~ | ~~Persist dashboard date range in URL query params~~ | ~~`HomeDashboardPage.tsx`~~ | ✅ Done |

---

## 14. Bigger Initiatives (multi-session)

| Initiative | Impact | Complexity |
|-----------|--------|------------|
| Full-page Notifications inbox (`/notifications`) | High | Medium |
| ~~Chart drill-down → filtered expenses~~ | ~~High~~ | ✅ Done (CategoryDonut → /expenses with params) |
| CSV import column mapper | High | High |
| Mobile expense search (IonSearchbar + backend filter) | High | Medium |
| ARIA combobox keyboard support | High | Medium |
| Screen reader accessible chart data tables | Medium | Medium |
| Settings page expansion (export, notification prefs) | Medium | Medium |
| Undo delete (5 s toast with restore) | Medium | Low (backend soft-delete exists) |
| PWA manifest + favicon | Low | Low |
| Receipt storage API + mobile upload | High | High |

---

## 15. Mobile-vs-Web Feature Parity Gaps

| Feature | Web | Mobile | Priority |
|---------|-----|--------|----------|
| Expense search/filter | ✅ Full | ❌ Family only | 🔴 High |
| Custom date range | ✅ | ❌ Period tabs only | 🟡 Medium |
| Settings (full) | ✅ 3 cards | ⚠️ Partial | 🟡 Medium |
| CSV import | ✅ Full | ❌ None | 🟢 Low |
| Families management | ✅ Full | ❌ Read-only context | 🟡 Medium |
| Admin pages | ✅ Full | ❌ None | 🟢 Low |
| Notification bell | ✅ Full | ✅ Partial | 🟡 Medium |
| Dark mode | ✅ | ✅ | ✅ Done |
| Offline queue | ❌ None | ✅ Full | 🟢 Low (web unlikely to need) |
| Receipt capture | ❌ N/A | ⚠️ Captured but not stored | 🔴 Fix or remove |

---

## 16. Visual Design Deep-Dive (Code-Level)

> Based on ui-ux-pro-max design system analysis + direct code audit.  
> The project uses a **"Hearth"** design system — warm earthy palette (cream surfaces, clay #C8623E primary, cocoa ink), Manrope typeface, JetBrains Mono for data.  
> The tokens are well-defined. The problem is **inconsistent usage** — several components bypass the token system entirely.

---

### 🔴 Design Token Violations — Dark Mode Breaks

The biggest visual issue in the codebase. These hardcoded Tailwind colors ignore `--color-surface-*` and `--color-ink-*` tokens and **will not adapt to dark mode**:

#### `ExpensesPage.tsx` — ConfirmDeleteModal (lines 22–41)
```tsx
// ❌ Current — hardcoded, dark-mode blind
<div className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-sm mx-4 p-6">
  <h2 className="text-base font-semibold text-slate-900 mb-2">…</h2>
  <p className="text-sm text-slate-500 mb-5">…</p>
  <button className="… border border-slate-200 hover:bg-slate-50 …">…</button>

// ✅ Fix — use Hearth tokens
<div className="bg-surface-card rounded-2xl shadow-warm border border-surface-border w-full max-w-sm mx-4 p-6">
  <h2 className="text-base font-semibold text-ink mb-2">…</h2>
  <p className="text-sm text-ink-mute mb-5">…</p>
  <button className="… border border-surface-border hover:bg-surface-subtle …">…</button>
```

#### `ExpensesPage.tsx` — ExpenseRow (lines 56–97) and table body (line 205)
```tsx
// ❌ Current
<tr className="border-b border-slate-100 hover:bg-slate-50 …">
<span className="… bg-slate-100 text-slate-600 …">{tag.name}</span>  // tag badge
<tbody className="bg-white">

// ✅ Fix
<tr className="border-b border-surface-border hover:bg-surface-subtle …">
<span className="… bg-surface-subtle text-ink-mute …">{tag.name}</span>
<tbody className="bg-surface-card">
```

#### `ExpensesPage.tsx` — CSV Import button (line 147)
```tsx
// ❌ Current
className="… border border-slate-200 bg-white hover:bg-slate-50 …"

// ✅ Fix
className="… border border-surface-border bg-surface-card hover:bg-surface-subtle …"
```

**Files affected:** [ExpensesPage.tsx](frontend/dashboard/src/features/expenses/pages/ExpensesPage.tsx)  
**Effort:** 20 min

---

### 🔴 Chart Colors — Dark Mode Breaks

`SpendChart.tsx` has hardcoded colors throughout despite `--chart-*` CSS variables already existing in `variables.css`:

```tsx
// ❌ Current — all hardcoded
const BAR_COLOR = '#c8623e'          // OK in light, but static
const LINE_COLOR = '#94a3b8'         // slate-400 — not from Hearth palette
<CartesianGrid stroke="#f1f5f9" />   // near-invisible in dark mode
<XAxis tick={{ fill: '#94a3b8' }} /> // hardcoded
<Tooltip contentStyle={{ border: '1px solid #e2e8f0' }} />  // light-only

// ✅ Fix — read CSS variables at render time
const isDark = document.documentElement.classList.contains('dark')
const gridColor   = isDark ? '#3E2E22' : '#E8DECB'   // --chart-grid
const tickColor   = isDark ? '#A09285' : '#8E8170'   // ink-mute
const tooltipBg   = isDark ? '#251D16' : '#FFFCF6'   // --chart-tooltip-bg
const tooltipBorder = isDark ? '#3E2E22' : '#E8DECB' // --chart-tooltip-border
```

Or better — read directly from CSS variables:
```tsx
function getCSSVar(name: string) {
  return getComputedStyle(document.documentElement).getPropertyValue(name).trim()
}
// then use getCSSVar('--chart-grid') etc.
```

**Same issue in:** `SameMonthChart.tsx`, `CategoryDonut.tsx`, `CurrenciesPanel.tsx`  
**Files affected:** [SpendChart.tsx](frontend/dashboard/src/features/dashboard/components/SpendChart.tsx), [SameMonthChart.tsx](frontend/dashboard/src/features/dashboard/components/SameMonthChart.tsx), [CategoryDonut.tsx](frontend/dashboard/src/features/dashboard/components/CategoryDonut.tsx)

---

### 🔴 MonthHero — Wrong Signal Colors for Delta Badge

```tsx
// ❌ Current — generic green/red, not Hearth semantic tokens
const deltaClass = deltaPositive
  ? 'bg-green-50 text-green-700'
  : 'bg-red-50 text-red-700'

// ✅ Fix — use Hearth sage/berry tokens (already defined, dark-mode aware)
const deltaClass = deltaPositive
  ? 'bg-sage-soft text-sage'
  : 'bg-berry-soft text-berry'
```

**File:** [MonthHero.tsx](frontend/dashboard/src/features/dashboard/components/MonthHero.tsx:42-44)

---

### 🔴 Touch Targets Too Small (32px vs 44px minimum)

Three navbar controls are `h-8 w-8` = 32×32px, violating the 44×44pt minimum:

```tsx
// ❌ NavBar.tsx — Add Expense button, Avatar button  
className="h-8 w-8 rounded-lg bg-brand-500 …"   // 32px
className="h-8 w-8 rounded-full bg-brand-500 …"  // 32px

// ❌ NotificationBell.tsx
className="relative h-8 w-8 rounded-lg …"         // 32px

// ✅ Fix — increase to h-9 w-9 (36px) minimum, ideally h-10 w-10 (40px)
// Or keep visual size but expand hit area:
className="relative h-8 w-8 … before:absolute before:inset-[-6px] before:content-['']"
```

**Files:** [NavBar.tsx](frontend/dashboard/src/layouts/NavBar.tsx:161-179), [NotificationBell.tsx](frontend/dashboard/src/features/notifications/components/NotificationBell.tsx:92)

---

### 🟡 FormCombobox — No Selected-Item Visual Indicator

When an option is selected, only `font-semibold` is applied — no checkmark, no color highlight. Users can't tell at a glance which item is active:

```tsx
// ❌ Current — barely noticeable
className={`px-3 py-1.5 text-sm cursor-pointer hover:bg-surface-subtle ${o.value === value ? 'font-semibold' : ''}`}

// ✅ Fix — add brand color + checkmark for selected
className={`px-3 py-1.5 text-sm cursor-pointer hover:bg-surface-subtle flex items-center justify-between
  ${o.value === value ? 'font-semibold text-brand-600 bg-brand-50' : 'text-ink-body'}`}
// Add checkmark SVG when o.value === value
```

Also: the dropdown uses `shadow-lg` (generic) but the tailwind config defines `shadow-warm` which matches the Hearth aesthetic. Use `shadow-warm` here.

**File:** [FormCombobox.tsx](frontend/dashboard/src/components/FormCombobox.tsx:72-74)

---

### 🟡 FormCombobox — No Dropdown Arrow Indicator

The combobox input looks like a plain text field — no chevron-down icon signals it is a dropdown. Users may type into it thinking it's a regular search box:

```tsx
// ✅ Fix — add a positioned chevron icon inside the input wrapper
<div ref={containerRef} className="relative">
  <input … />
  <span className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-ink-faint">
    <svg className="h-4 w-4" …><path d="M19 9l-7 7-7-7" /></svg>
  </span>
</div>
```

**File:** [FormCombobox.tsx](frontend/dashboard/src/components/FormCombobox.tsx:41-54)

---

### 🟡 ExpenseForm — Textarea Has No Character Counter

The description `<textarea>` has `maxLength={500}` but shows no counter. Users type freely until characters stop appearing:

```tsx
// ✅ Fix — add counter below textarea
<textarea … maxLength={500} value={watch('description')} />
<p className="text-xs text-ink-faint text-right mt-0.5">
  {(watch('description') ?? '').length}/500
</p>
```

**File:** [ExpenseForm.tsx](frontend/dashboard/src/features/expenses/components/ExpenseForm.tsx:210-217)

---

### 🟡 ExpenseForm — Family Checkboxes Use Browser-Default Styling

The browser's native checkbox renders with the OS chrome (gray square on Windows, blue on Mac). Only the fill color is overridden via `accent-brand-500`. The border, size, and shape are inconsistent with the Hearth rounded aesthetic:

```tsx
// ❌ Current — native checkbox with accent override
<input type="checkbox" className="h-4 w-4 rounded border-surface-border accent-brand-500 cursor-pointer" />

// ✅ Fix — custom checkbox using Tailwind peer trick
<label className="flex items-center gap-2.5 cursor-pointer">
  <input type="checkbox" className="peer sr-only" … />
  <span className="h-4 w-4 rounded-md border border-surface-border bg-surface-card
                   peer-checked:bg-brand-500 peer-checked:border-brand-500
                   flex items-center justify-center transition-colors duration-150">
    <svg className="hidden peer-checked:block h-2.5 w-2.5 text-white" …>…</svg>
  </span>
  {f.name}
</label>
```

**File:** [ExpenseForm.tsx](frontend/dashboard/src/features/expenses/components/ExpenseForm.tsx:237-244)

---

### 🟡 Expense Table — Amounts Not Tabular-Aligned

Amount values in the expense list render with proportional-width digits (default). In a column of numbers, digits don't vertically align — `1` is narrower than `8`, making amounts hard to compare at a glance. The project already has JetBrains Mono configured:

```tsx
// ❌ Current
<td className="px-4 py-3 text-sm font-medium text-ink whitespace-nowrap">{amount}</td>

// ✅ Fix — add font-mono or tabular-nums
<td className="px-4 py-3 text-sm font-medium text-ink whitespace-nowrap font-mono tabular-nums">{amount}</td>
```

Same fix applies to: date column (use `tabular-nums`), MonthHero amount (`font-mono` already big, but add `tabular-nums`), CurrenciesPanel amounts.

**File:** [ExpensesPage.tsx](frontend/dashboard/src/features/expenses/pages/ExpensesPage.tsx:58)

---

### 🟡 Expense Table — Date Shown in ISO Format

The date column shows raw `YYYY-MM-DD` ISO strings ("2026-01-15"). This is machine-readable, not human-friendly:

```tsx
// ❌ Current
<td className="…">{expense.date}</td>

// ✅ Fix — locale-format
<td className="… tabular-nums">
  {new Date(expense.date + 'T00:00:00').toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: 'numeric' })}
</td>
// Renders: "15 Jan 2026" (or locale equivalent)
```

**File:** [ExpensesPage.tsx](frontend/dashboard/src/features/expenses/pages/ExpensesPage.tsx:57)

---

### 🟡 Table Action Buttons — Too Small, No Icon

"Edit" and "Delete" are plain text links with no padding or icon. Tiny click targets on a row, easy to misclick between them. Also purely text — no visual affordance of destructiveness for Delete:

```tsx
// ❌ Current — text links crammed together
<button className="text-brand-600 hover:text-brand-700 font-medium mr-3 …">{t('…edit')}</button>
<button className="text-red-500 hover:text-red-700 font-medium …">{t('…delete')}</button>

// ✅ Fix — icon buttons with tooltip
<button aria-label="Edit expense" className="p-1.5 rounded-lg text-ink-mute hover:text-brand-600 hover:bg-brand-50 transition-colors">
  <PencilIcon className="h-4 w-4" />
</button>
<button aria-label="Delete expense" className="p-1.5 rounded-lg text-ink-mute hover:text-berry hover:bg-berry-soft transition-colors">
  <TrashIcon className="h-4 w-4" />
</button>
```

**File:** [ExpensesPage.tsx](frontend/dashboard/src/features/expenses/pages/ExpensesPage.tsx:83-94)

---

### 🟡 Pagination — Text Links With No Button Shape

"Prev" / "Next" are styled as text links. They look inactive even when enabled — no pill/button shape, no icon:

```tsx
// ❌ Current — text only
<button className="text-sm font-medium text-brand-600 hover:text-brand-700 disabled:opacity-40 …">
  {t('expenses.pagination.prev')}
</button>

// ✅ Fix — button shape with chevron icons
<button className="inline-flex items-center gap-1 px-3 py-1.5 rounded-lg border border-surface-border
                   text-sm font-medium text-ink-body hover:bg-surface-subtle
                   disabled:opacity-40 disabled:cursor-not-allowed transition-colors">
  <ChevronLeftIcon className="h-4 w-4" />
  {t('expenses.pagination.prev')}
</button>
```

**File:** [ExpensesPage.tsx](frontend/dashboard/src/features/expenses/pages/ExpensesPage.tsx:217-233)

---

### 🟡 NavBar — User Dropdown Missing Backdrop / Transition

The user dropdown (`userMenuOpen ? '' : 'hidden'`) uses a hard `hidden` class toggle — appears/disappears instantly with no animation. Jarring compared to the 150ms transitions elsewhere:

```tsx
// ❌ Current — instant show/hide
className={`… ${userMenuOpen ? '' : 'hidden'}`}

// ✅ Fix — use opacity + scale transition
className={`… transition-all duration-150 origin-top-right
  ${userMenuOpen
    ? 'opacity-100 scale-100 pointer-events-auto'
    : 'opacity-0 scale-95 pointer-events-none'}`}
```

**File:** [NavBar.tsx](frontend/dashboard/src/layouts/NavBar.tsx:185)

---

### 🟡 NavBar — Logo Text Needs `font-serif` for Brand Distinction

The logo text uses `font-bold text-[15px]` (Manrope, the body font). The Hearth design system includes `Instrument Serif` as a serif font for brand moments. The logo is a great candidate:

```tsx
// ❌ Current — same font as nav links
<span className="font-bold text-ink text-[15px] tracking-tight">
  Expense<span className="text-brand-500">Manager.</span>
</span>

// ✅ Fix — use serif for "ExpenseManager" wordmark
<span className="font-serif font-bold text-ink text-[17px] tracking-tight">
  Expense<span className="text-brand-500">Manager.</span>
</span>
```

**File:** [NavBar.tsx](frontend/dashboard/src/layouts/NavBar.tsx:128-130)

---

### 🟡 Notification Dropdown — Loading State Is Just `…`

When notifications are loading, the dropdown shows a `…` ellipsis centered in the list. Every other loading state in the app uses skeleton placeholders:

```tsx
// ❌ Current
{isLoading && notifications.length === 0 ? (
  <div className="px-3 py-4 text-sm text-ink-mute text-center">…</div>
) : …}

// ✅ Fix — 3 skeleton notification rows
{isLoading && notifications.length === 0 ? (
  <div className="divide-y divide-surface-border">
    {[1,2,3].map(i => (
      <div key={i} className="px-3 py-2.5 animate-pulse">
        <div className="h-3.5 bg-surface-muted rounded w-3/4 mb-1.5" />
        <div className="h-2.5 bg-surface-subtle rounded w-1/3" />
      </div>
    ))}
  </div>
) : …}
```

**File:** [NotificationBell.tsx](frontend/dashboard/src/features/notifications/components/NotificationBell.tsx:127-129)

---

### 🟡 TopCategory Badge Uses Emoji Icon

In `MonthHero`, the top category badge renders `data.topCategory.icon` which is an emoji from the database:

```tsx
{data.topCategory.icon && <span>{data.topCategory.icon}</span>}
```

Emojis render inconsistently across platforms (different sizes, colors, styles on Windows vs macOS vs Android). The design system explicitly avoids emojis as structural icons. Two options:
- Map category icons to SVG icons from a consistent icon library (Lucide)
- Remove the icon from the badge and rely on the category name text alone

**File:** [MonthHero.tsx](frontend/dashboard/src/features/dashboard/components/MonthHero.tsx:83)

---

### 🟡 Shadow Inconsistency Across Modals

Three different shadow styles used across overlays:
- ConfirmDeleteModal: `shadow-xl` (generic Tailwind)
- NavBar user dropdown: `boxShadow: '0 8px 20px -10px rgba(30,20,10,0.5)'` (inline)
- NotificationBell dropdown: `boxShadow: '0 8px 20px -10px rgba(30,20,10,0.5)'` (inline)
- Cards: `shadow-card` (design token)

The inline shadow is already defined as `shadow-warm` in `tailwind.config.ts`. Use it:

```tsx
// ❌ Inline style
style={{ boxShadow: '0 8px 20px -10px rgba(30,20,10,0.5)' }}

// ✅ Design token class
className="… shadow-warm"
```

Consolidate ConfirmDeleteModal to `shadow-warm` as well for cohesion.

**Files:** [NavBar.tsx](frontend/dashboard/src/layouts/NavBar.tsx:186), [NotificationBell.tsx](frontend/dashboard/src/features/notifications/components/NotificationBell.tsx:109), [ExpensesPage.tsx](frontend/dashboard/src/features/expenses/pages/ExpensesPage.tsx:22)

---

### 🟡 Missing `font-display: swap` on Manrope

Both web and mobile use Manrope. If loaded from Google Fonts without `font-display: swap`, the browser shows invisible text (FOIT) until the font loads. Check `index.html`:

```html
<!-- Ensure this is present in the Google Fonts URL -->
&display=swap
<!-- or add to @font-face in CSS -->
font-display: swap;
```

---

### 🟢 ExpenseForm — Amount+Currency Row Proportions

The row splits as `flex-1` (amount) + `w-36` (currency = 144px). On narrow viewports the currency field is cramped. Currency codes are 3 characters ("USD", "EUR") — the field could be `w-28` and still look fine, giving more space to the amount:

```tsx
// ❌ Current — w-36 is wider than needed for 3-char codes
<div className="w-36">

// ✅ Fix — w-28 is sufficient for "EUR" with the combobox dropdown arrow
<div className="w-28">
```

**File:** [ExpenseForm.tsx](frontend/dashboard/src/features/expenses/components/ExpenseForm.tsx:124)

---

### 🟢 SpendChart — Average Line Uses Slate, Not Hearth

```tsx
// ❌ Current
const LINE_COLOR = '#94a3b8'   // Tailwind slate-400 — not from Hearth palette

// ✅ Fix — use Hearth mustard for contrast against clay bars
const LINE_COLOR = '#D6A23F'   // mustard — in palette, good contrast on cream
```

**File:** [SpendChart.tsx](frontend/dashboard/src/features/dashboard/components/SpendChart.tsx:16)

---

### 🟢 FormCombobox — Option List Item Height Too Dense

List items use `py-1.5` (6px top+bottom = 28–30px total height). Below the 44px touch target minimum. While this is a mouse-first dropdown on web, improving density helps reduce misclicks:

```tsx
// ❌ Current
className="px-3 py-1.5 text-sm cursor-pointer …"

// ✅ Fix — py-2 gives ~32px, more comfortable
className="px-3 py-2 text-sm cursor-pointer …"
```

**File:** [FormCombobox.tsx](frontend/dashboard/src/components/FormCombobox.tsx:63-74)

---

### 🟢 ExpensesPage — Error State Uses Raw `text-red-500`

```tsx
// ❌ Current — raw Tailwind red, not Hearth berry
<span className="text-red-500">{t('expenses.errors.loadFailed')}</span>

// ✅ Fix — use Hearth semantic color
<span className="text-berry">{t('expenses.errors.loadFailed')}</span>
```

**File:** [ExpensesPage.tsx](frontend/dashboard/src/features/expenses/pages/ExpensesPage.tsx:175)

---

## 17. Design System Recommendations (from ui-ux-pro-max)

### Typography — Confirmed Good Choice

The project uses **Manrope** — rated "Friendly SaaS" by the design system:
> *Friendly, modern, SaaS, clean, approachable, professional. Best for SaaS products, web apps, dashboards, B2B.*

The existing font stack is optimal. No change needed, just ensure `font-display: swap` is set.

**Potential enhancement:** The design system recommends **Plus Jakarta Sans** as an alternative. Both are excellent for fintech dashboards. Manrope's slightly geometric letterforms match the earthy palette well — keep it.

### Color Palette Assessment

The **Hearth palette** maps well onto the "Expense Splitter / Bill Split" fintech pattern from the design system:
- Primary: sage green `#059669` → **close to Hearth sage `#6B8E5A`** ✅
- Accent: berry red → **matches Hearth berry `#B5443F`** ✅
- The clay brand color `#C8623E` is a distinctive differentiator — not typical for fintech but gives warmth and personality

**One gap:** The fintech pattern uses **pure green** for "income/positive" and **red** for "debt/expense". The Hearth sage is more muted. Consider brighter `#059669` (emerald) for explicit positive-balance scenarios while keeping clay as the brand primary.

### Mobile Style — Recommended Direction

For the Ionic mobile app, the design system recommends **"SaaS Mobile (High-Tech Boutique)"** or **"Flat Design Mobile"** for a fintech tracker:

Key suggestions to incorporate:
- **Cards:** `borderRadius: 16` + subtle border + very light shadow — Hearth already does this ✅
- **Buttons:** Height 56px for primary CTAs — Ionic's default IonButton may be shorter; verify
- **Press feedback:** `scale: 0.97` on press — Ionic provides this via CSS but custom Pressable components may not
- **Staggered entrance:** Fade-in list items on screen mount (Y: 20→0 + opacity: 0→1, staggered 30ms per item) — not currently implemented
- **JetBrains Mono for data labels** — already in tailwind config, not confirmed used on mobile

---

## 18. Visual Polish — Quick Wins (Component Level)

| # | Fix | File | Lines | Effort |
|---|-----|------|-------|--------|
| 1 | Replace `bg-white`/`border-slate-*` in ConfirmDeleteModal with tokens | `ExpensesPage.tsx` | 22–41 | 5 min |
| 2 | Replace `border-slate-100 hover:bg-slate-50` in ExpenseRow with tokens | `ExpensesPage.tsx` | 56 | 5 min |
| 3 | Replace `bg-white` in `<tbody>` with `bg-surface-card` | `ExpensesPage.tsx` | 205 | 2 min |
| 4 | Replace `bg-slate-100 text-slate-600` tag badge with Hearth tokens | `ExpensesPage.tsx` | 67 | 5 min |
| 5 | `bg-sage-soft text-sage` / `bg-berry-soft text-berry` for delta badge | `MonthHero.tsx` | 43-44 | 5 min |
| 6 | Add `tabular-nums font-mono` to amount column in expense table | `ExpensesPage.tsx` | 58 | 2 min |
| 7 | Locale-format date column (remove raw ISO display) | `ExpensesPage.tsx` | 57 | 10 min |
| 8 | Add `shadow-warm` class to dropdown overlays (remove inline style) | `NavBar.tsx`, `NotificationBell.tsx` | — | 5 min |
| 9 | Add chevron-down icon to FormCombobox input | `FormCombobox.tsx` | 41-54 | 15 min |
| 10 | Highlight selected option in FormCombobox with brand color + checkmark | `FormCombobox.tsx` | 72 | 15 min |
| 11 | Add opacity+scale transition to NavBar user dropdown (remove `hidden`) | `NavBar.tsx` | 185 | 10 min |
| 12 | Replace `…` loading in NotificationBell with 3-row skeleton | `NotificationBell.tsx` | 127 | 15 min |
| 13 | Add character counter below description textarea | `ExpenseForm.tsx` | 210-217 | 10 min |
| 14 | Icon buttons (pencil/trash) replace text Edit/Delete in expense row | `ExpensesPage.tsx` | 83-94 | 20 min |
| 15 | Pill-shaped pagination buttons with chevron icons | `ExpensesPage.tsx` | 217-233 | 15 min |
| 16 | Chart colors — use `--chart-*` CSS vars instead of hardcoded hex | `SpendChart.tsx` | 14-16, 70-87 | 30 min |
| 17 | Replace `text-red-500` error text with `text-berry` | `ExpensesPage.tsx` | 175 | 2 min |
| 18 | SpendChart average line: `#94a3b8` → `#D6A23F` (mustard) | `SpendChart.tsx` | 16 | 2 min |
| 19 | Increase navbar icon buttons from `h-8 w-8` to `h-9 w-9` | `NavBar.tsx`, `NotificationBell.tsx` | 161, 179, 92 | 5 min |
| 20 | Custom styled checkbox for family multi-select | `ExpenseForm.tsx` | 240-244 | 20 min |
