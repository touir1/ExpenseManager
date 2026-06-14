# UX/UI Improvements — ExpenseManager

> Audit date: 2026-06-14  
> Scope: `frontend/dashboard` (React/Tailwind) + `frontend/mobile` (Ionic/React/Capacitor)  
> Priority legend: 🔴 High · 🟡 Medium · 🟢 Low

---

## 1. Navigation & Information Architecture

### 🔴 Web: No active page indicator in mobile hamburger menu

**Problem:** Desktop navbar highlights the active route (brand background on active link). When the hamburger menu opens on small screens, the same links lose their active visual state — user has no "you are here" cue.  
**Fix:** Apply the same active class logic to mobile menu items as desktop nav links.

---

### 🔴 Web: FamilySelector and DisplayCurrencySelector are always visible but contextually confusing

**Problem:** Both dropdowns appear in the navbar at all times, even on pages where they have no effect (Settings, Families). Users may change the family filter expecting something to happen on the Settings page.  
**Fix:** Hide/disable navbar selectors on pages that don't consume them, or add a subtle tooltip explaining context ("Filters expenses only").

---

### 🟡 Web: "Add Expense" button in navbar duplicates button on Expenses page

**Problem:** Two entry points for the same action with no visual hierarchy difference. First-time users may not notice the one in the navbar.  
**Fix:** Keep navbar button as the primary global CTA, make the in-page button secondary (outline style). Add tooltip "Quickly add an expense" to navbar button.

---

### 🟡 Mobile: No breadcrumb / back-navigation pattern for nested flows

**Problem:** Entering a sub-flow (e.g. accepting a family invite via deep link) offers no consistent back path.  
**Fix:** Ensure every IonPage that can be pushed onto the nav stack has an IonBackButton in the toolbar.

---

### 🟢 Web: Admin nav item is invisible until page load resolves `isAdmin`

**Problem:** On first render, admin users see no Admin link (it flashes in after the session check). During this 200–500 ms window the layout shifts.  
**Fix:** Reserve navbar space for admin link during auth loading (skeleton placeholder or keep the gap).

---

## 2. Dashboard

### 🔴 Web: Dashboard date filters reset to "month-to-date" on every page load

**Problem:** If a user sets a custom date range to investigate last quarter, refreshing the page resets it. No persistence.  
**Fix:** Persist date range in URL query params (`?from=&to=`) so the URL is shareable and survives refresh.

---

### 🔴 Web: No empty-state on dashboard for new users

**Problem:** A brand-new account with no expenses shows empty/zero charts with no explanation. SpendChart renders an empty line chart; CategoryDonut is a blank circle.  
**Fix:** Add empty-state illustrations or callout cards ("No expenses yet — add your first one!") when all queries return zero data.

---

### 🟡 Web: Dashboard charts have no drill-down

**Problem:** Clicking a category slice on CategoryDonut or a bar on SameMonthChart does nothing. Users cannot discover which expenses compose a data point.  
**Fix:** Make chart elements clickable — navigate to `/expenses` pre-filtered by category and date range. This is a high-value feature with modest implementation cost using existing filter infrastructure.

---

### 🟡 Web: MonthHero percentage change has no context tooltip

**Problem:** The % change vs last month badge is shown without explaining the comparison period (last month? last year?).  
**Fix:** Add a `title` attribute or `<Tooltip>` component that says "Compared to {previousMonthName}".

---

### 🟡 Web: DashboardFilters date range allows `from > to` without validation

**Problem:** A user can set end date before start date, which sends an invalid query to the backend and silently returns empty data.  
**Fix:** Clamp `to` min to `from` value, or show a validation error inline in the filter bar.

---

### 🟡 Mobile: Period selector (month/week/year) and DisplayCurrency selector are separate — interaction is non-obvious

**Problem:** Changing period does not visually indicate it affects the currency display too. The relationship is implicit.  
**Fix:** Group the two selectors visually inside one toolbar segment with a divider, or merge into one combined picker.

---

### 🟢 Web: Charts don't show tooltips for zero-value data points

**Problem:** Hovering over a date where there are no expenses shows nothing. Users can't confirm "was I really at zero that day?".  
**Fix:** Enable `Recharts` tooltips for zero values with explicit "No expenses" label.

---

### 🟢 Web: CurrenciesPanel has no visual chart

**Problem:** CurrenciesPanel shows a plain text/table breakdown while all sibling panels have charts.  
**Fix:** Add a small horizontal bar chart per currency for visual consistency.

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
| 5 | Clamp dashboard date range `to` min = `from` | `DashboardFilters` component | 15 min |
| 6 | Add "Clear filters" button on ExpensesPage | `ExpensesPage.tsx` | 20 min |
| 7 | Replace "Loading…" text with skeleton in ExpensesPage | `ExpensesPage.tsx` | 30 min |
| 8 | Validate date range (from ≤ to) in DashboardFilters | `DashboardFilters` component | 20 min |
| 9 | Add archive confirmation modal | `FamiliesPage.tsx` | 30 min |
| 10 | Show expense summary in delete confirmation modal | `ConfirmDeleteModal` | 20 min |
| 11 | Hide subcategory field when category has no subcategories | `ExpenseForm.tsx` | 15 min |
| 12 | Mobile: "Today" / "Yesterday" group headers in expense list | `ExpensesListPage.tsx` | 20 min |
| 13 | Mobile: Offline banner on expense list | `ExpensesListPage.tsx` | 30 min |
| 14 | Persist dashboard date range in URL query params | `HomeDashboardPage.tsx` | 45 min |

---

## 14. Bigger Initiatives (multi-session)

| Initiative | Impact | Complexity |
|-----------|--------|------------|
| Full-page Notifications inbox (`/notifications`) | High | Medium |
| Chart drill-down → filtered expenses | High | Medium |
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
