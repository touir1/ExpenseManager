# Quick Wins Plan (Section 13, ux-ui-improvements.md)

Source: `docs/plans/ux-ui-improvements.md` §13 "Quick Wins (≤ 1 hour each)". Open items only (5, 6, 8, 14 already ✅ Done).

## Process

- Use `/ui-ux-pro-max` for planning AND implementation of every item below (design tokens, a11y, interaction states — don't hand-roll styling decisions).
- Work items in order. Each item: implement → add/update unit tests → verify (typecheck + run tests) → check off.
- Follow project conventions in `CLAUDE.md` (Hearth design tokens, `EmptyState`, `useReturnFocusOnUnmount` for new modals, `aria-describedby`, etc).

## Items

### 1. Hide pagination when total ≤ 1 page
- File: `frontend/dashboard/src/features/expenses/pages/ExpensesPage.tsx`
- Plan/implement via `/ui-ux-pro-max`.
- Unit test: pagination control not rendered when `totalPages <= 1`; still renders when > 1.

### 2. Fix hover state on non-clickable rows
- File: `ExpensesPage.tsx`
- Add `cursor-pointer` only on actually-clickable rows, or strip hover style from non-interactive ones.
- Unit test: assert class presence/absence via `toHaveClass()` per row type.

### 3. `aria-live="polite"` on Toast container
- File: `frontend/dashboard/src/components/Toast.tsx` (or shared `Toast.tsx` per platform)
- Unit test: container has `aria-live="polite"` attribute; toast text still reachable via existing tests.

### 4. Dynamic `aria-label` on notification bell (include count)
- File: `NotificationBell.tsx`
- Unit test: `aria-label` includes unread count text; updates when count changes (rerender).

### 7. Replace "Loading…" text with skeleton in ExpensesPage
- File: `ExpensesPage.tsx`
- Reuse existing skeleton pattern from dashboard components (per `docs/plans/done/performance-loading-states-plan.md`).
- Unit test: skeleton element renders during loading state, replaced by list once loaded.

### 9. Add archive confirmation modal
- File: `FamiliesPage.tsx`
- Follow `ConfirmDeleteModal` pattern; apply `useReturnFocusOnUnmount`.
- Unit test: modal opens on archive click, confirm calls archive action, cancel closes without action, focus returns to trigger on close.

### 10. Show expense summary in delete confirmation modal
- File: `ConfirmDeleteModal`
- Unit test: modal renders expense amount/description/date passed as props.

### 11. Hide subcategory field when category has no subcategories
- File: `ExpenseForm.tsx`
- Unit test: subcategory field absent when selected category has empty `Children`; present otherwise.

### 12. Mobile: "Today" / "Yesterday" group headers in expense list
- File: `frontend/mobile/.../ExpensesListPage.tsx`
- Unit test: grouping helper produces correct headers for today/yesterday/older dates (locale-independent, mock date).

### 13. Mobile: Offline banner on expense list
- File: `frontend/mobile/.../ExpensesListPage.tsx`
- Unit test: banner renders when offline state true, hidden when online (mock network status hook).

## Unit Tests — Cross-Cutting

- Every item above ships with new/updated unit tests in the same PR — no item is "done" without test coverage for the new behavior.
- Run full suite after all items: `npm test` (frontend/dashboard), plus mobile suite if items 12/13 touched.
- Update snapshots/mocks only where the change legitimately alters output — don't paper over regressions.

## Completion

- Mark each row `~~n~~` / ✅ Done in `docs/plans/ux-ui-improvements.md` §13 as completed, matching existing convention (see items 5, 6, 8, 14).
- Move this file to `docs/plans/done/` when all items closed, per repo convention.
- Update `CHANGELOG.md` per `.claude/maintenance.md`.

/done
