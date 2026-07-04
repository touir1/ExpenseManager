# Notifications UX/UI Plan (Web & Mobile)

> Source: `docs/plans/ux-ui-improvements.md`, section 7 "Notifications (Web & Mobile)".
> Scope: `frontend/dashboard` (React/Tailwind, Hearth design tokens) + `frontend/mobile` (Ionic).
> Design tool: **`/ui-ux-pro-max`** — invoke before touching visuals for each item below (icon choices, empty/loading states, full-page inbox layout, toast collapsing pattern). Don't freehand colors/spacing — pull from the Hearth token set already in `variables.css`/`tailwind.config.ts`.

Backend already has: `GET/PUT /config/notifications` (users svc), `NotificationPreferenceDto`, and a wired-but-unused frontend service `notificationPreferencesApi.service.ts`. No new backend work needed for item 4.

---

## Item 1 — Per-type category icon on notification items (Web + Mobile)

**Files:** `frontend/dashboard/src/features/notifications/components/NotificationBell.tsx`, `frontend/mobile/src/features/notifications/components/NotificationBell.tsx`, new shared icon map.

Steps:
1. Run `/ui-ux-pro-max` to pick 6 consistent SVG icons (Lucide-style, not emoji — matches existing "TopCategory Badge Uses Emoji Icon" guidance in section 16) for: `FAMILY_MEMBER_REMOVED`, `FAMILY_INVITATION_ACCEPTED`, `FAMILY_MEMBER_JOINED`, `FAMILY_EXPENSE_ADDED`/`FAMILY_EXPENSE_DELETED`, `CSV_IMPORT_COMPLETED`, `RATE_CONFLICT_CREATED`, plus color per type using Hearth semantic tokens (`text-sage`, `text-berry`, `text-brand-500`, `text-ink-mute`).
2. Add `getNotificationIcon(type: string): JSX.Element` next to the existing `getNotificationText` in each `NotificationBell.tsx` (web: inline SVG components; mobile: `IonIcon` with an icon from `ionicons/icons` matching the same semantic mapping).
3. Render icon in a small fixed-size wrapper (`h-8 w-8 rounded-full` bg tint) to the left of each notification row's text block — web `<button>` row, mobile `<IonItem>`.
4. Keep `default` fallback icon (bell/info) for unknown types.

Unit tests:
- Web `NotificationBell.test.tsx`: assert an icon element (`data-testid="notification-icon-<type>"` or role) renders per notification type fixture; assert default icon renders for an unrecognized type.
- Mobile: no existing `NotificationBell` test file — add `frontend/mobile/src/features/notifications/components/__tests__/NotificationBell.test.tsx` covering render + icon-per-type using the project's Ionic mock patterns (`forwardRef` mocks per `CLAUDE.md` mobile test conventions).

---

## Item 2 — Full-page notification inbox with "view all" link (Web)

**Files:** new `frontend/dashboard/src/features/notifications/pages/NotificationsPage.tsx`, router file (wherever `/dashboard`, `/expenses` routes are registered), `NotificationBell.tsx` dropdown header, i18n files (4 locales), `notificationApi.service.ts` (already supports paged `getNotifications(page, pageSize)`).

Steps:
1. `/ui-ux-pro-max` for full-page inbox layout — list density, pagination footer reuse (matches `ExpensesPage` "Showing X–Y of Z" pattern already Done in section 3), per-item icon (reuse Item 1's icon map), read/unread visual states, empty state (bold heading + CTA, matching section 12 empty-state pattern).
2. Add route `/notifications` (protected, same guard as other authenticated pages).
3. Build `NotificationsPage.tsx`: paged list via existing `getNotifications`, mark-read on click, "Mark all read" button, uses shared `getNotificationText`/icon helpers (extract these two into a shared module e.g. `features/notifications/notificationDisplay.ts` so both the dropdown and the full page use one source of truth instead of duplicating switch statements).
4. Add "View all" link in the dropdown header (`NotificationBell.tsx`) navigating to `/notifications`; dropdown keeps its 320px max-height list as-is.

Unit tests:
- New `NotificationsPage.test.tsx`: renders paged list, pagination controls change page, mark-read/mark-all-read call the API mocks, empty state renders when list is empty.
- Update `NotificationBell.test.tsx`: assert "View all" link present with correct `href`/`to`.
- Extracting `getNotificationText`/icon into a shared module: update existing `NotificationBell.test.tsx` imports if it currently tests the local function directly.

---

## Item 3 — Rate-limit / collapse stacked toasts (Web)

**Files:** `frontend/dashboard/src/components/Toast.tsx`, `frontend/dashboard/src/features/notifications/components/NotificationBell.tsx` (toast-on-new-notification effect at lines 65-70).

Steps:
1. `/ui-ux-pro-max` for the collapsed-toast pattern — single grouped toast copy ("3 new notifications") vs rate-limiting, and its visual treatment (icon, color = info tone already defined).
2. Chosen approach: collapse rather than drop — truer to "no lost information" than a hard rate limit.
3. In `Toast.tsx`'s `show()`: if a toast of the same `type` was shown within a short window (e.g. 3s) and is still active, merge into a single toast with a count badge ("3 new notifications") instead of pushing a new stacked entry — track a `groupKey`/`count` per toast id, bump `count` and reset the dismiss timer instead of adding a new array entry.
4. `NotificationBell.tsx`'s effect (lines 65-70) fires `show()` per new notification already — no change needed there if collapsing happens inside `Toast.tsx`; verify multiple rapid `show()` calls with same type collapse visually.

Unit tests:
- `Toast.test.tsx` (create if none exists — check first): rapid successive `show('x','info')` calls within the window collapse into one visible toast with incrementing count; calls with different `type` still stack separately; toast auto-dismiss timer resets on each collapse.
- `NotificationBell.test.tsx`: simulate 3 rapid unread-count increments, assert only one toast-worthy `show` invocation surface (or that the mocked Toast context received calls that would collapse — depending on where dedup logic lives).

---

## Item 4 — Per-event-type notification preferences UI (Web Settings)

**Files:** `frontend/dashboard/src/features/settings/pages/SettingsPage.tsx` (or wherever settings cards are composed), new `NotificationPreferencesCard.tsx`, existing `notificationPreferencesApi.service.ts` (already implemented, currently unused), `userConfig.type.ts` (already has `NotificationPreferenceDto`), i18n files.

Steps:
1. `/ui-ux-pro-max` for the settings card layout — toggle-per-event-type list, grouped by category (family events, import, rate conflicts) matching the existing Settings card visual language (section 6 "Save ✓" pattern for consistency).
2. Build `NotificationPreferencesCard.tsx`: on mount, `getNotificationPreferences()`; render one toggle per `eventType` with i18n label; `PUT` on toggle change (or batched "Save" — match existing `DefaultCurrencyCard`/`DefaultCategoryCard` save+confirmation pattern already Done in section 6 for consistency, incl. the "Saved ✓" `aria-live` affordance).
3. Add card to `SettingsPage.tsx` grid.
4. Add locale keys for event-type labels (readable names for `FAMILY_MEMBER_REMOVED` etc., reuse mapping ideas from Item 1/2's display helper) across all 4 locale files.

Unit tests:
- New `NotificationPreferencesCard.test.tsx`: loads and renders preferences, toggling calls `updateNotificationPreferences` with expected payload, shows "Saved ✓" confirmation state matching the pattern used in `SettingsPage.test.tsx` for the other two cards.
- Update `SettingsPage.test.tsx` if the grid/snapshot asserts card count.

---

## Execution order

1. Item 1 (icons) — smallest, unblocks shared display helper used by Item 2.
2. Item 2 (full-page inbox) — depends on Item 1's icon map being extracted to a shared module.
3. Item 4 (preferences UI) — independent, backend already done.
4. Item 3 (toast collapsing) — independent, do last since it's the most isolated.

For every item: implement, then update/add the unit tests listed above, run `npm test` in `frontend/dashboard` (and `frontend/mobile` for Item 1's mobile half) to confirm green before moving to the next item.

---

/done
