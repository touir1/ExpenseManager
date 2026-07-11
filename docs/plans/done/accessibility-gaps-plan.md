# Accessibility Gaps — Implementation Plan

> Source: [ux-ui-improvements.md](ux-ui-improvements.md) section 9 "Accessibility Gaps"
> Scope: `frontend/dashboard` (web) — mobile combobox item listed but no Ionic combobox exists yet, web-only fix applies
> Design work must run through **`/ui-ux-pro-max`** — invoke it both for the design decisions in this plan AND during implementation of each item (component states, colors, focus rings, icon choice must follow the Hearth design system already audited in section 16 of ux-ui-improvements.md).

---

## Items (source: section 9)

1. 🔴 `FormCombobox` dropdown not keyboard-navigable (Web/Mobile)
2. 🔴 Toast notifications not announced to screen readers (Web)
3. 🟡 Modal focus management — focus doesn't return to trigger element (Web)
4. ✅ Color-only differentiator for valid/invalid import rows — already done, skip
5. 🟡 `NotificationBell` badge has no accessible count label (Web)
6. 🟢 Charts have no accessible data table fallback (Web)

---

## Step 0 — Design pass (`/ui-ux-pro-max`)

Run `/ui-ux-pro-max` first to get concrete design decisions before touching code:
- ARIA combobox interaction spec (focus ring, `aria-activedescendant` highlight style using Hearth tokens)
- Screen-reader-only table pattern for charts (`sr-only` class, caption wording)
- Toast `role`/`aria-live` politeness levels per toast type (info/success vs error)

Do not skip this step — prior sections of this doc (16) show hardcoded/inconsistent styling already exists; new a11y markup must not reintroduce non-token colors.

---

## Step 1 — `FormCombobox` keyboard navigation (🔴)

**File:** `frontend/dashboard/src/components/FormCombobox.tsx`

- Add `role="combobox"`, `aria-expanded`, `aria-controls`, `aria-autocomplete="list"` on the input.
- Add `role="listbox"` on the dropdown, `role="option"` + `aria-selected` per item.
- Track a `highlightedIndex` state; wire `aria-activedescendant` to the highlighted option's `id`.
- Keyboard handler on input `onKeyDown`:
  - `ArrowDown`/`ArrowUp`: move highlight, wrap or clamp at ends
  - `Home`/`End`: jump to first/last visible option
  - `Enter`: select highlighted option, close dropdown
  - `Escape`: close dropdown, keep current value
  - type-ahead: buffer printable chars (reset after ~500ms), jump to first match
- Apply `/ui-ux-pro-max`-specified highlight style (replaces the flagged "no selected-item indicator" issue from section 16 in the same pass — same component, same PR).
- Same fix applies to `TagInput`/`StringCombobox` if they share the pattern — check before duplicating logic; extract a shared keyboard-nav hook if 2+ components need identical behavior.

**Unit tests** (`FormCombobox.test.tsx`):
- ArrowDown/ArrowUp move `aria-activedescendant` through options in order
- Enter selects the highlighted option and closes the dropdown
- Escape closes without changing value
- Home/End jump to first/last option
- Type-ahead jumps to matching option
- `role`/`aria-*` attributes present and correct on mount and on open

---

## Step 2 — Toast screen-reader announcements (🔴)

**File:** `frontend/dashboard/src/components/Toast.tsx`

- Wrap toast container in `<div role="status" aria-live="polite" aria-atomic="true">` for info/success toasts.
- Use `aria-live="assertive"` (or a second region) for error-type toasts so they interrupt.
- Verify the existing collapse/count-badge behavior (section 7, already done) still announces updated text when a toast is merged — `aria-atomic="true"` should re-read the whole message on count change.

**Unit tests** (`Toast.test.tsx`):
- Toast container has `role="status"` and `aria-live="polite"` for non-error toast
- Error toast renders with `aria-live="assertive"`
- Re-render with incremented count still exposes updated text within the live region

---

## Step 3 — Modal focus return to trigger (🟡)

**Files:** `frontend/dashboard/src/features/expenses/pages/ExpensesPage.tsx` (`AddExpenseModal`, `ConfirmDeleteModal` usage), any shared `Modal` wrapper if one exists — check for a shared component before patching each modal individually.

- Capture the triggering element (`document.activeElement` at open time, or explicit `triggerRef`) before opening.
- On modal close (any path: Escape, backdrop click, confirm, cancel), call `.focus()` on the stored trigger element.
- If a shared `Modal.tsx` wrapper exists, implement once there; otherwise implement per-modal (`AddExpenseModal`, `ConfirmDeleteModal`, `ConfirmArchiveModal`, `InviteMemberModal`, `CreateFamilyModal`) and note the duplication for future extraction.

**Unit tests**:
- Opening then closing (via Escape and via Cancel button) returns focus to the button that opened it, for at least `AddExpenseModal` and `ConfirmDeleteModal`

---

## Step 4 — NotificationBell accessible count label (🟡)

**File:** `frontend/dashboard/src/features/notifications/components/NotificationBell.tsx`

- Change `aria-label` to include count: `aria-label={t('notifications.bell', { count: unreadCount })}`.
- Add i18n key `notifications.bell` with count interpolation to all 4 locale files (`en`, and the other 3 already used elsewhere in this doc's fixes).
- Zero-count case should read naturally (e.g. "Notifications, no unread" vs "3 unread notifications") — confirm exact strings via `/ui-ux-pro-max` copy guidance if available, else keep consistent with existing i18n tone.

**Unit tests**:
- `aria-label` includes count when `unreadCount > 0`
- `aria-label` falls back to a sensible zero-state string when `unreadCount === 0`

---

## Step 5 — Chart accessible data table fallback (🟢)

**Files:** `frontend/dashboard/src/features/dashboard/components/SpendChart.tsx`, `SameMonthChart.tsx`, `CategoryDonut.tsx`

- Add `<caption>` describing what the chart shows.
- Add a visually-hidden (`sr-only`) `<table>` with the same underlying series data (labels + values) alongside each chart's SVG.
- Reuse one shared helper (e.g. `ChartDataTable.tsx`) taking `{ caption, columns, rows }` rather than writing 3 bespoke tables.

**Unit tests** (`ChartDataTable.test.tsx` + one usage test per chart):
- Renders a `<table>` with `sr-only` class containing all data points
- Caption text matches expected chart description
- Each of the 3 charts renders the fallback table with correct row count matching input data

---

## Rollout order

1. Step 0 (design pass) → 2. Step 2 (Toast, isolated, low risk) → 3. Step 4 (NotificationBell, isolated) → 4. Step 1 (FormCombobox, highest impact, most test surface) → 5. Step 3 (Modal focus, touches multiple files) → 6. Step 5 (Charts, lowest priority/effort).

Run `npm run typecheck` and `npm test` after each step, not just at the end.

---

## Maintenance updates on completion

Per [.claude/maintenance.md](../../.claude/maintenance.md):
- Update section 9 entries in [ux-ui-improvements.md](ux-ui-improvements.md) to ✅ as each lands
- Update section 14 "ARIA combobox keyboard support" / "Screen reader accessible chart data tables" rows to ✅ Done
- Add entries to [CHANGELOG.md](../../CHANGELOG.md)
- Move this plan to `docs/plans/done/` once all steps ship

---

/done
