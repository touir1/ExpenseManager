# Frontend UI Library Evaluation (MUI vs alternatives vs status quo)

## Current state (confirmed via codebase check)

- Styling: Tailwind CSS (`@layer components` in `index.css`), custom design tokens (`brand-600`, card pattern `bg-white shadow-card border border-slate-200 rounded-2xl`).
- All interactive primitives are hand-rolled: `FormCombobox.tsx`, `StringCombobox`, `TagChips`, `FamilyMultiSelect`, `TagInput` (all portal-based via `createPortal(...,document.body)` + `getBoundingClientRect()` for `overflow:auto` contexts), `Toast.tsx` (custom collapsing logic), `EmptyState.tsx`, `PasswordInput`/`PasswordStrength`, modals (`AddExpenseModal`/`EditExpenseModal`) with custom focus-return hook (`useReturnFocusOnUnmount.ts`).
- Forms: React Hook Form + Zod, `aria-describedby` wired by hand.
- Charts: Recharts, with a hand-built `ChartDataTable` a11y fallback pattern shared across `SpendChart`/`CategoryDonut`/`SameMonthChart`.
- Dark mode: `color-scheme` CSS property + `.dark` class, tuned specifically so native controls (incl. would-be `IonSelect` equivalents) don't mismatch OS theme.
- No component library dependency exists today (`package.json` confirmed: react, RHF, zod, recharts, react-query, i18next only — no MUI/AntD/Chakra/Radix).

## Option comparison

### A. Full MUI adoption
- **Pros:** large ready-made component set (DataGrid, DatePicker, Autocomplete), strong a11y defaults, mature theming API.
- **Cons:**
  - Styling collision: MUI's emotion-based CSS-in-JS runs alongside Tailwind's utility classes — two styling systems, two theme sources of truth (`tailwind.config.js` tokens vs MUI `theme.ts`), real risk of visual drift and specificity fights.
  - Bundle cost: `@mui/material` + `@emotion/react` + `@emotion/styled` adds ~90-100KB gzipped baseline, on top of existing recharts/RHF/zod payload — no current perf budget doc, but this is a step-change, not incremental.
  - Rework cost: every hand-built primitive (portal dropdowns, collapsing toast, focus-return modals) already solves the exact problem MUI would solve — replacing them is a rewrite, not an addition, and risks regressing the many documented edge cases (Moq/test gotchas, `overflow:hidden` portal requirement, toast collapsing window, mobile Ionic dark-mode `color-scheme` tuning already fought for).
  - Dark mode: would need a second dark-mode implementation (MUI `palette.mode`) parallel to the existing `.dark`/`color-scheme` one, or a bridge layer — extra surface, not simplification.
- **Verdict:** not recommended as a wholesale swap. High risk, low marginal value given the hand-rolled system is already mature and documented.

### B. Headless/unstyled libraries (Radix UI primitives, Headless UI, Floating UI)
- **Pros:** no CSS-in-JS, work natively with Tailwind (this is literally their design intent), replace only the *behavior* (focus trap, positioning, keyboard nav, portal mounting) while keeping existing Tailwind visual classes — smallest blast radius.
- **Cons:** still a migration per component; not zero-cost.
- **Verdict:** viable, but shadcn/ui (below) wraps the same primitives with less glue code to hand-write — prefer it over raw Floating UI/Radix unless a single narrow behavior is needed.

### C. shadcn/ui (Radix primitives + Tailwind, copy-in components — not a runtime dependency)
- **What it is:** a CLI that copies component source (Dialog, Popover, Combobox via `cmdk`, Toast/Sonner, etc.) directly into the repo. Each component is Radix (behavior/a11y/focus/positioning) + Tailwind classes (styling). No `@mui/material`-style npm package sitting between the app and its UI — the code lives in `src/components/ui/` and is edited like any other project file.
- **Pros:**
  - No CSS-in-JS, no second theme system — components read the existing `tailwind.config.js` tokens (`brand-600`, `rounded-2xl`, `shadow-card`) directly, so visual consistency with hand-rolled components is a styling edit, not a theme bridge.
  - Bundle cost scales with what's actually copied in (Radix packages per component used), not an all-or-nothing package — much smaller than MUI's baseline.
  - Solves the same problems Floating UI alone would (positioning, focus trap, keyboard nav, portal mounting) but for **both** the dropdown components and the modals in one consistent system, instead of Floating UI for dropdowns + something else for dialogs.
  - Since it's copied source, it doesn't fight the project's existing patterns (portal-to-`document.body`, `aria-describedby` wiring, dark mode via `color-scheme`/`.dark`) — those stay as-is or get absorbed into the copied component, reviewable in a normal PR diff.
- **Cons:** still a migration per component (not free); pulls in `cmdk` for combobox and a few Radix packages; requires deciding a `components.json` config (Tailwind prefix, aliases) once.
- **Best fit candidates in this codebase:**
  - **shadcn `Combobox`** (Radix `Popover` + `cmdk`) → replaces `FormCombobox`/`StringCombobox` — removes the manual `getBoundingClientRect()` + scroll-capture-listener + mousedown-outside-click logic (documented CLAUDE.md gotchas) in favor of Radix's positioning/dismiss primitives.
  - **shadcn `Dialog`** → replaces `AddExpenseModal`/`EditExpenseModal` — built-in focus trap + return-focus + escape/backdrop close, retires the custom `useReturnFocusOnUnmount` hook.
  - **shadcn `Tag input`/multi-select pattern** → `TagChips`/`FamilyMultiSelect`/`TagInput`, same positioning benefit as Combobox.
  - **shadcn `Sonner`/`Toast`** — optional, lowest priority; current `Toast.tsx` collapsing logic is small and already well-tested.
- **Verdict:** preferred over both MUI and raw Floating UI. Best cost/benefit: Tailwind-native styling, one consistent primitive system for dropdowns *and* modals, and the migrated code stays fully inspectable/editable rather than living behind a package boundary.

### D. Status quo (no library, continue hand-rolling)
- **Pros:** zero migration cost, zero new dependency risk, existing patterns already documented and battle-tested (see `CLAUDE.md` accumulated gotchas — this is institutional knowledge that a swap would strand).
- **Cons:** every new widget (per `docs/plans/dashboard-new-charts-plan.md`'s 6 new widgets) re-derives focus/positioning/a11y behavior from scratch or copy-paste from existing components.
- **Verdict:** fine as-is for simple widgets (progress bars, stat cards, list rows — sections 1, 3, 5 of the charts plan don't need any library). Not fine to keep indefinitely for dropdown/modal-heavy work.

## Recommendation

Do **not** adopt MUI. Adopt **shadcn/ui**, scoped first to the portal-based dropdown components, then the two expense modals. Tailwind-native styling means no theme-bridge work; Radix underneath means positioning/focus/a11y stop being hand-maintained. Leave `Toast.tsx` and all other Tailwind visual styling untouched for now.

## Scoped plan (if approved)

1. **Initialize shadcn/ui**: run its CLI (`npx shadcn@latest init`) in `frontend/dashboard/` — configures `components.json` against the existing `tailwind.config.js` (reuse existing color tokens, don't let it generate a competing palette), adds Radix + `cmdk`/`tailwind-merge`/`class-variance-authority` as needed.
2. **Migrate one dropdown first** (`FormCombobox.tsx` — has existing test coverage `FormCombobox.test.tsx`) as the pilot:
   - Copy in shadcn's `Combobox` component, restyle to match current visual output (same Tailwind classes as today: card/border/rounded conventions).
   - Keep the portal-to-`document.body` behavior (Radix `Popover` portals by default, compatible with the `overflow:auto/hidden` requirement already documented).
   - Update `FormCombobox.test.tsx` — Radix `Popover` interactions differ slightly from the current manual `fireEvent.focus`/`fireEvent.mouseDown` pattern; adjust and verify against jsdom (no `ResizeObserver` gaps expected, confirm).
3. **If pilot succeeds** (tests green, no visual regression, code simpler): migrate `StringCombobox`, `TagChips`, `FamilyMultiSelect`, `TagInput` the same way.
4. **Second phase — modals:** migrate `AddExpenseModal`/`EditExpenseModal` to shadcn `Dialog`, retire `useReturnFocusOnUnmount.ts` once confirmed Radix's built-in focus-return covers the same cases (Escape, backdrop, confirm, cancel — all documented close paths).
5. **Do not touch (yet):** `Toast.tsx`, mobile app (`frontend/mobile/` uses Ionic components already, different constraints entirely — not in scope).
6. **Verify manually in browser** per project convention (`CsvImportPage` navigation-blocker precedent) — dropdown positioning inside `overflow-y-auto` modals is exactly the case that broke before and needs eyes-on confirmation, not just unit tests.

## Unit test notes

- Existing `FormCombobox.test.tsx`/`AddExpenseModal.test.tsx`/`EditExpenseModal.test.tsx` interaction patterns must keep passing (or be deliberately updated) after migration — Radix's event model differs slightly from the hand-rolled mousedown/focus listeners.
- Add regression test for scroll-close behavior (currently `window.addEventListener('scroll',close,true)`) — Radix `Popover` handles this internally, confirm equivalent behavior post-migration.
- Add regression test that modal focus-return still works for all 4 close paths (Escape, backdrop, confirm, cancel) after dropping `useReturnFocusOnUnmount`.
- No new test infra needed — jsdom + testing-library already used project-wide.

## Explicitly out of scope

- MUI, Ant Design, Chakra full adoption — rejected per styling-collision/bundle-cost reasoning above.
- Any change to the 6 new dashboard widgets planned in `dashboard-new-charts-plan.md` — those are simple list/bar/text widgets, don't need dropdown/modal libraries, build with existing Tailwind + recharts patterns as already planned.
