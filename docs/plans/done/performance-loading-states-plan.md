# Plan: Section 10 — Performance & Loading States

> Source: [ux-ui-improvements.md § 10](ux-ui-improvements.md#10-performance--loading-states)
> Scope: `frontend/dashboard`

Use `/ui-ux-pro-max` for design decisions in this plan (skeleton layout, loading hierarchy) and again during implementation of each item (component styling, accessibility of loading states).

---

## Item 1 — 🟡 Dashboard queries fire in parallel, no priority

**File:** [HomeDashboardPage.tsx](../../frontend/dashboard/src/features/dashboard/pages/HomeDashboardPage.tsx)

Current: 6 `useQuery` calls (`summary`, `monthly`, `categories`, `sameMonth`, `currencies`, `recent`) fire simultaneously, all default `staleTime`/`gcTime`.

Steps:
1. Run `/ui-ux-pro-max` to confirm priority order (MonthHero > SpendChart > rest) and whether a visible stagger/skeleton sequence is wanted or just cache tuning.
2. Add `staleTime: 60_000` (or similar) to `summaryQ` so MonthHero data doesn't refetch/flash on quick nav.
3. Evaluate `enabled` gating: keep `summaryQ` unconditional, gate `categoriesQ`/`currenciesQ`/`sameMonthQ`/`recentQ`/`monthlyQ` on `summaryQ.isSuccess` for a lightweight waterfall — OR skip gating if `/ui-ux-pro-max` says parallel is fine for this data size and only `staleTime` tuning is warranted. Decide based on actual query cost, not blind waterfall (over-serializing 6 cheap queries can make total load slower on fast connections).
4. Keep existing `isLoading` skeleton props passed per-panel — no change to panel components needed.

## Item 2 — 🟡 ExpensesPage "Loading…" text, not skeleton

**File:** [ExpensesPage.tsx](../../frontend/dashboard/src/features/expenses/pages/ExpensesPage.tsx) (loading block: line 343-347; table structure: line 377-396)

Current table has 7 columns: date, amount, category, description, tags, families, actions. Mobile uses card list (`ExpenseCard`).

Steps:
1. Run `/ui-ux-pro-max` to design the skeleton row (shimmer bar widths per column, row count ~5-8, matches `animate-pulse` pattern already used in [MonthHero.tsx](../../frontend/dashboard/src/features/dashboard/components/MonthHero.tsx)).
2. Add `ExpenseTableSkeleton` (desktop, `<table>`-shaped, N rows × 7 `<td>` with `bg-surface-subtle animate-pulse` bars) and `ExpenseCardSkeleton` (mobile card shape) — colocate in `ExpensesPage.tsx` or extract to `components/ExpenseSkeleton.tsx` if reused elsewhere (e.g. CsvImportPage has same "Loading…" text at line 518 — check whether to reuse).
3. Replace the `isLoading` block (line 343-347) to render skeletons matching the responsive split (`hidden md:block` table skeleton + mobile card skeleton), same breakpoint logic as the loaded state.
4. Keep `t('expenses.loading', 'Loading…')` as `aria-live="polite"` sr-only text alongside the skeleton (screen readers still need the loading announcement — skeleton alone is silent).

## Item 3 — 🟡 AuthBrandPanel image optimization

**File:** [AuthBrandPanel.tsx](../../frontend/dashboard/src/features/auth/components/AuthBrandPanel.tsx)

Current state check: component already uses pure CSS `linear-gradient`/`radial-gradient` — **no `<img>` tag exists**. The audit item appears stale/already resolved.

Steps:
1. Confirm with `/ui-ux-pro-max` there's no other image asset (e.g. background referenced via CSS `background-image: url(...)`) — none found in current read.
2. If truly no image: mark this item **done, no-op** in the changelog with a note explaining why (CSS gradient, not raster image).
3. If a future design adds a raster image here, apply `loading="lazy"` + WebP at that time — not now.

## Item 4 — 🟢 FormCombobox no virtualization

**File:** [FormCombobox.tsx](../../frontend/dashboard/src/components/FormCombobox.tsx)

Current: `items = [undefined, ...filtered]` renders every option as a `<li>` in the portal dropdown (line 167-190), unbounded.

Steps:
1. Run `/ui-ux-pro-max` to confirm UX for a capped list (e.g. "showing 50 of 120 — type to narrow" hint vs silent cap).
2. Add a `MAX_VISIBLE = 50` slice: render `items.slice(0, MAX_VISIBLE)` instead of full `items`, keep `filtered`/typeahead search against the full unsliced array so keyboard jump-to-match still works across all options.
3. When `filtered.length > MAX_VISIBLE`, render a trailing non-interactive `<li>` hint: "{count} more — keep typing to narrow".
4. Do not pull in `react-window` for a ≤200-item list — manual slice is sufficient and avoids a new dependency (per project's no-premature-abstraction guidance).

---

## Unit Tests

**Item 1** — [HomeDashboardPage.test.tsx](../../frontend/dashboard/src/features/dashboard/pages/__tests__/HomeDashboardPage.test.tsx)
- Assert `summaryQ`'s `useQuery` call includes the tuned `staleTime`.
- If `enabled` gating added: assert dependent queries carry `enabled: false` until `summaryQ.isSuccess`, then queries fire (mock QueryClient, assert call order/count).

**Item 2** — [ExpensesPage.test.tsx](../../frontend/dashboard/src/features/expenses/pages/__tests__/ExpensesPage.test.tsx) (check existing file; add if absent)
- While `isLoading`, skeleton rows render (`data-testid` or role query) instead of literal "Loading…" text node.
- sr-only `aria-live="polite"` loading text still present for a11y.
- Skeleton row count and column count match desktop table's 7 columns (structural assertion, not pixel widths).
- Mobile skeleton renders under the `md:hidden` branch (jsdom won't apply CSS breakpoints — assert both skeleton blocks exist in DOM per existing project convention for responsive dual-render).

**Item 3** — [AuthBrandPanel.test.tsx](../../frontend/dashboard/src/features/auth/components/__tests__/AuthBrandPanel.test.tsx)
- No new test needed if no-op; if a note-only change, no test changes required. Skip unless an image is later added.

**Item 4** — [FormCombobox.test.tsx](../../frontend/dashboard/src/components/__tests__/FormCombobox.test.tsx)
- Given >50 options, opening dropdown renders exactly `MAX_VISIBLE` `<li role="option">` plus the trailing hint `<li>`.
- Typeahead (`jumpToTypeahead`) still matches and highlights an option beyond index 50 (proves search runs against full list, not the sliced one) — may require scrolling the highlighted index into view rather than assuming it's rendered; adjust slice logic if this fails (e.g. slice around highlighted index, not just first 50).
- Given ≤50 options, no hint `<li>` renders and behavior is unchanged from before (regression guard).

Run: `npm test` from `frontend/dashboard` (full suite with coverage, per [commands.md](../../.claude/commands.md)). Confirm no coverage regression on touched files.

---

/done
