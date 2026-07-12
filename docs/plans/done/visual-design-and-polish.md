# 11. Visual Design & Polish

> Use `/ui-ux-pro-max` for plan refinement and implementation of every item below.

### 🟡 Web: Dark mode missing from dashboard charts

**Problem:** ThemeContext provides dark mode, but Recharts chart tooltips and axes use hardcoded colors (likely `#333`, `#fff`). In dark mode these will look wrong.
**Fix:** Pass theme-aware colors to Recharts props: `stroke={theme === 'dark' ? '#cbd5e1' : '#334155'}` etc.

**To do:**
- Add `getCSSVar(name)` helper (`getComputedStyle(document.documentElement).getPropertyValue(name).trim()`) — put in shared `features/dashboard/utils/chartTheme.ts`.
- Read `--chart-grid`, `--chart-tooltip-bg`, `--chart-tooltip-border`, ink-mute token for tick color; replace all hardcoded hex in:
  - `SpendChart.tsx` (`BAR_COLOR`, `LINE_COLOR`, `CartesianGrid stroke`, `XAxis tick`, `Tooltip contentStyle`)
  - `SameMonthChart.tsx`
  - `CategoryDonut.tsx`
  - `CurrenciesPanel.tsx`
- Re-derive colors on theme toggle (subscribe to `ThemeContext` or re-read on re-render — don't cache at module scope).
- Tests: update/add tests in each chart's `.test.tsx` asserting correct CSS var is read for `light`/`dark` theme (mock `getComputedStyle` or `ThemeContext` value).

---

### 🟡 Web: Inconsistent modal sizes

**Problem:** AddExpenseModal, ConfirmDeleteModal, InviteMemberModal, and CreateFamilyModal have different max-widths. No modal size scale.
**Fix:** Define `sm`/`md`/`lg` modal size classes in `index.css` and apply consistently.

**To do:**
- Add `.modal-sm{max-width:24rem}` / `.modal-md{max-width:32rem}` / `.modal-lg{max-width:48rem}` under `@layer components` in `index.css` (follow existing Hearth token pattern, no hardcoded colors).
- Map each modal to a size: `ConfirmDeleteModal`→sm, `AddExpenseModal`/`InviteMemberModal`→md, `CreateFamilyModal`→md (confirm against current visual width before switching).
- Swap ad-hoc `max-w-*` Tailwind classes in `ExpensesPage.tsx` (ConfirmDeleteModal), `AddExpenseModal.tsx`, `FamiliesPage.tsx` (InviteMemberModal, CreateFamilyModal) for the new `.modal-*` classes.
- Tests: snapshot/className assertions per modal test file confirming the correct `.modal-*` class is applied.

---

### 🟡 Web: Tables lack row hover cursor — no affordance that rows are clickable

**Problem:** In the expense table, rows have a hover darken effect but no pointer cursor change. Users don't know if they can click the row itself.
**Fix:** Either make rows fully clickable (click row → edit expense) with `cursor-pointer`, or remove the hover darkening to avoid false affordance.

**To do:**
- Decide direction: rows become clickable (open edit modal on row click, ignore clicks originating from action buttons via `e.stopPropagation()` on Edit/Delete buttons) — preferred, matches existing edit-icon affordance.
- `ExpensesPage.tsx`: add `onClick` to `<tr>` opening `EditExpenseModal`/edit flow, add `cursor-pointer`; add `stopPropagation` to the Edit/Delete icon button handlers so row click doesn't double-fire.
- Apply same to mobile `ExpenseCard` list rows for consistency.
- Tests: `ExpensesPage.test.tsx` — click on row (outside action buttons) opens edit modal; click on Delete icon does NOT open edit modal (propagation stopped).

---

### 🟢 Web: No favicon or PWA manifest

**Problem:** Tab icon is the browser default. No `<link rel="manifest">` for installability.
**Fix:** Add SVG favicon using brand color + "E" logo. Add `manifest.json` for PWA install support.

**To do:**
- Create `frontend/dashboard/public/favicon.svg` — brand-clay (#C8623E) background, "E" glyph, matches Hearth palette.
- Add `frontend/dashboard/public/manifest.json` (`name`, `short_name`, `icons`, `theme_color:#C8623E`, `background_color`, `display:standalone`, `start_url:/`).
- Wire `<link rel="icon" href="/favicon.svg">` + `<link rel="manifest" href="/manifest.json">` in `index.html`.
- No unit test surface (static assets) — verify manually: build (`npm run build:prod`), check tab icon + installability prompt in browser devtools Application panel.

---

### 🟢 Mobile: App icon and splash screen not customized

**Problem:** `android/` and `ios/` are gitignored but Capacitor generates default Ionic splash screens. The brand icon should be applied.
**Recommendation:** Use `@capacitor/assets` to generate icons/splash from a single source SVG before production builds.

**To do:**
- Add `frontend/mobile/resources/icon.png` (1024×1024) + `resources/splash.png` (2732×2732) using brand-clay + "E" mark, consistent with the web favicon above.
- Install `@capacitor/assets` as a devDependency; run `npx capacitor-assets generate` to populate `android/`/`ios/` (gitignored, generated at build time — add a note to build docs, not a CI step unless native builds are already automated).
- No unit test surface — verify manually on a device/simulator build before a production release.

---

### 🟢 Web: Empty pagination state ("Page 1 of 1") always shows even with no records

**Problem:** When the expense list is empty, pagination still renders "Page 1 of 0" or "Page 1 of 1". Looks broken.
**Fix:** Hide pagination controls when total pages ≤ 1.

**To do:**
- `ExpensesPage.tsx`: wrap the pagination footer (page nav + jump-to-page input, from section 3's "Showing X–Y of Z" feature) in `{totalPages > 1 && (...)}`.
- Keep the "Showing X–Y of Z expenses" count line visible even when `totalPages <= 1` (still useful info) — only hide prev/next/jump controls.
- Tests: `ExpensesPage.test.tsx` — assert pagination nav absent when `totalPages === 1` or `0`, present when `> 1`.

---

## Implementation notes

- Use `/ui-ux-pro-max` skill for both planning (design tokens, style choices) and implementation (component edits) of each item above.
- Each fix must include unit test updates: add/update tests covering the new behavior (dark-mode chart color branch, modal size class application, row cursor/click behavior, pagination hidden state) — do not mark an item done without matching test coverage.
- Follow existing Hearth design token conventions (`--color-surface-*`, `--color-ink-*`, `--chart-*`) — no new hardcoded colors.

/done
