# Visual Design Deep-Dive Plan (Section 16, ux-ui-improvements.md)

Source: `docs/plans/ux-ui-improvements.md` §16 "Visual Design Deep-Dive (Code-Level)".

**Note:** §13 (Quick Wins) turned out mostly already fixed by a prior session — line numbers/snippets in this doc may be stale. Before touching any item below, re-read the current file and confirm the issue still exists; skip (mark ✅ Done, no code change) anything already resolved.

## Process

- Use `/ui-ux-pro-max` for planning AND implementation of every item below (color tokens, dark-mode contrast, touch targets, motion — verify against Hearth design system rules, don't eyeball it).
- Work priority order: 🔴 (dark-mode breaks / touch targets) → 🟡 (missing affordances/consistency) → 🟢 (polish).
- Each item: verify still-broken → implement → add/update unit tests where behavior is testable (className assertions, conditional rendering, computed color logic) → typecheck → check off.
- Follow `CLAUDE.md` conventions: Hearth tokens (`surface-*`, `ink-*`, `sage`/`berry`/`mustard`), `shadow-warm`/`shadow-card`, `tabular-nums`/`font-mono` for numeric columns, no emoji as structural icons.

## 🔴 Critical — Dark Mode / Touch Targets

### 1. Design token violations in ExpensesPage.tsx
- ConfirmDeleteModal, ExpenseRow, table `<tbody>`, CSV Import button — replace hardcoded `slate-*`/`white` with `surface-*`/`ink-*` tokens.
- Verify first: earlier session already reworked `ConfirmDeleteModal` (now uses `bg-surface-card`/`shadow-warm`/`border-surface-border`/`text-ink`) — check if row/tbody/import-button still use raw slate.
- Unit test: `toHaveClass()` assertions on rendered row/modal/button elements confirming token classes, not raw slate/white.

### 2. Chart colors hardcoded (dark-mode breaks)
- Files: `SpendChart.tsx`, `SameMonthChart.tsx`, `CategoryDonut.tsx`, `CurrenciesPanel.tsx`.
- Fix: read `--chart-*` CSS variables at render time (helper `getCSSVar(name)`) instead of hardcoded hex, react to `dark` class changes.
- Unit test: mock `getComputedStyle`/`document.documentElement.classList`, assert chart color props switch between light/dark values.

### 3. MonthHero delta badge wrong semantic colors
- Replace `green-50/green-700` / `red-50/red-700` with `sage-soft/sage` / `berry-soft/berry`.
- Unit test: assert `toHaveClass('bg-sage-soft')`/`bg-berry-soft` for positive/negative delta.

### 4. Touch targets too small (32px vs 44px)
- `NavBar.tsx` (Add Expense button, Avatar button), `NotificationBell.tsx` — `h-8 w-8` → `h-9 w-9`/`h-10 w-10`, or expand hit area via `before:absolute before:inset-[-6px]`.
- Unit test: assert rendered button computed class includes the enlarged size token (or expanded-hitbox pseudo-element class).

## 🟡 Medium — Consistency / Affordance

### 5. FormCombobox — no selected-item indicator
- Add brand color + checkmark for selected option; swap `shadow-lg` → `shadow-warm`.
- Unit test: selected option renders checkmark/`bg-brand-50`; unselected does not.

### 6. FormCombobox — no dropdown chevron
- Add chevron-down icon inside input wrapper.
- Unit test: chevron icon present in DOM.

### 7. ExpenseForm — textarea has no character counter
- Add `{length}/500` counter below description textarea.
- Unit test: counter text updates as `description` field value changes.

### 8. ExpenseForm — family checkboxes use native browser styling
- Replace with custom `peer`-based checkbox matching Hearth rounded aesthetic.
- Unit test: checking/unchecking still toggles form state correctly (behavior unchanged, only visual); assert `peer-checked` structure present.

### 9. Expense table — amounts not tabular-aligned
- Verify first: `ExpensesPage.tsx` amount cells already have `font-mono tabular-nums` (confirmed present in current code) — likely ✅ Done already. Re-check `CurrenciesPanel.tsx` and `MonthHero.tsx` amount display for the same treatment.
- Unit test only if a change is actually made.

### 10. Expense table — date shown in raw ISO format
- Verify first: current `ExpensesPage.tsx` renders `{expense.date}` raw in the table — confirm still true, then locale-format via `toLocaleDateString`.
- Unit test: date cell renders formatted (non-ISO) string; assert via day/month/year presence, not literal locale-dependent string (see `CLAUDE.md` `toLocaleString` guidance — compare digit/content, not exact separators).

### 11. Table action buttons — too small, no icon
- Verify first: current `ExpensesPage.tsx` already has icon-only `EditButton`/`DeleteButton` (`p-1.5 rounded-lg`, `aria-label`) — likely ✅ Done already.
- Unit test only if a change is actually made.

### 12. Pagination — text links with no button shape
- Verify first: current pagination Prev/Next are plain text-link styled (`text-sm font-medium text-brand-600 …`) — still open per current code. Add button shape (border, padding, chevron icons).
- Unit test: buttons have border/padding classes; disabled state still respects `page<=1`/`page>=totalPages`.

### 13. NavBar — user dropdown missing backdrop/transition
- Replace hard `hidden` toggle with opacity/scale transition classes.
- Unit test: dropdown open/closed state reflected via `opacity-100 scale-100` vs `opacity-0 scale-95` classes (not `hidden`).

### 14. NavBar — logo needs `font-serif` for brand distinction
- Apply `font-serif` to wordmark.
- Unit test: logo element has `font-serif` class.

### 15. Notification dropdown — loading state is just `…`
- Replace with 3 skeleton rows (`animate-pulse`).
- Unit test: loading state renders skeleton placeholder elements, not literal `…` text.

### 16. TopCategory badge uses emoji icon
- Replace emoji rendering with SVG icon mapping (Lucide) or remove icon, keep category name text only.
- Unit test: badge no longer renders raw emoji string as icon (assert SVG/icon element or absence, per chosen fix).

### 17. Shadow inconsistency across modals/dropdowns
- Replace inline `boxShadow` styles (NavBar dropdown, NotificationBell dropdown) and `shadow-xl` (ConfirmDeleteModal, if still present) with `shadow-warm` token class.
- Unit test: `toHaveClass('shadow-warm')`; assert no inline `style` boxShadow remains.

### 18. Missing `font-display: swap` on Manrope
- Check `index.html` (web + mobile) Google Fonts URL / `@font-face` for `&display=swap` / `font-display: swap`.
- No unit test applicable (build/HTML config) — verify manually in browser network tab per `CLAUDE.md` UI-change testing rule.

## 🟢 Low — Polish

### 19. ExpenseForm — amount+currency row proportions
- `w-36` → `w-28` for currency field.
- Unit test: not meaningful for a width class change; skip unless combined with #8's checkbox test file.

### 20. SpendChart — average line uses slate, not Hearth
- `#94a3b8` → `#D6A23F` (mustard).
- Unit test: assert line color prop equals mustard hex.

### 21. FormCombobox — option list item height too dense
- `py-1.5` → `py-2`.
- Unit test: not meaningful alone; skip.

### 22. ExpensesPage — error state uses raw `text-red-500`
- `text-red-500` → `text-berry`.
- Unit test: `toHaveClass('text-berry')` on error state element.

## Unit Tests — Cross-Cutting

- Every item with testable behavior (conditional classes, computed colors, conditional rendering, state toggles) ships a test in the same PR.
- Pure Tailwind width/spacing tweaks (#19, #21) and the font-display HTML check (#18) don't need new tests — note as "verified manually" instead.
- Run full suite after all items: `npm test` (frontend/dashboard). No mobile files touched in this section.
- Run `npm run typecheck` after all items.

## Completion

- For each item, if investigation shows it's already implemented (as happened repeatedly in §13), still update `docs/plans/ux-ui-improvements.md` §16 to note it's resolved (strike through or add a note) rather than leaving it stale.
- Move this file to `docs/plans/done/` when all items closed.
- Update `CHANGELOG.md` per `.claude/maintenance.md` (minor version bump only — never major).

/done
