# Dashboard Responsive Grid Plan

Goal: turn `HomeDashboardPage.tsx` from fixed 1-col→lg:3-col rows into a true responsive **CSS grid** that scales 1→2→3→4 columns by breakpoint, stays 4-col (wider cards, more whitespace) on ultrawide, and is future-proof for new widgets (see [dashboard-new-charts-plan.md](dashboard-new-charts-plan.md)).

Design basis: `ui-ux-pro-max` skill, "Data-Dense Dashboard" style (grid layout, KPI cards, minimal padding but max data visibility) + `ux`/`chart` domain checks (no horizontal scroll, content-jumping, table-handling, chart-type fit).

## Current state (as-is)

`frontend/dashboard/src/features/dashboard/pages/HomeDashboardPage.tsx`:
- Container: `max-w-6xl mx-auto w-full px-4 sm:px-6 py-8`
- Row 1: `grid gap-4 lg:grid-cols-3` → `MonthHero` (col-span-1) + `SpendChart` (col-span-2)
- Row 2: `grid gap-4 lg:grid-cols-2` → `CategoryDonut` + `RecentExpenses`
- Row 3: `grid gap-4 lg:grid-cols-2` → `SameMonthChart` + `CurrenciesPanel`
- No `md:` breakpoint at all — jumps straight from 1-col mobile to `lg:` (1024px). Tablet (768–1023px) gets no intermediate layout.
- `max-w-6xl` caps width — ultrawide screens just get side margins, not adapted layout.

## Target breakpoints & column counts

| Breakpoint | Tailwind prefix | Min width | Columns |
|---|---|---|---|
| Phone | (default) | 0px | 1 |
| Tablet | `md:` | 768px | 2 |
| Laptop | `lg:` | 1024px | 3 |
| Desktop | `xl:` | 1280px | 4 |
| Ultrawide | `2xl:` | 1536px | 4 (same col count, wider container + bigger gaps) |

## Grid architecture change

Replace the three separate per-row grids with **one single grid container** for the whole widget area, using explicit `grid-column`/`grid-row` spans per widget (via a `colSpan`/`rowSpan` config, not ad-hoc per-row JSX). This is what makes it extensible for [dashboard-new-charts-plan.md](dashboard-new-charts-plan.md) additions — new widgets just declare a span and drop into the grid instead of requiring a new hand-built row.

```tsx
// container
<div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4 xl:gap-6 2xl:gap-8">
```

Container width: drop the hard `max-w-6xl` cap in favor of `max-w-6xl xl:max-w-7xl 2xl:max-w-[1800px]` so ultrawide gets breathing room without the grid stretching to full 3440px (avoids absurdly wide charts — `ui-ux-pro-max` chart guidance: line/bar charts lose readability past a certain width; prefer added whitespace/padding over infinite stretch).

### Per-widget span table

Span is expressed as Tailwind col-span classes per breakpoint. Row placement is left to grid auto-flow (`grid-auto-flow: row dense` optional if gaps appear from mixed spans — verify visually, use `grid-flow-row-dense` only if empty gaps show up after reorder).

| Widget | Mobile (1col) | Tablet (2col) | Laptop (3col) | Desktop/Ultrawide (4col) | Rationale |
|---|---|---|---|---|---|
| `MonthHero` | col-span-1 | col-span-2 | col-span-1 | col-span-1 | KPI summary — compact, stays small once room exists |
| `SpendChart` (spending over time) | col-span-1 | col-span-2 | col-span-2 | col-span-2 | Primary trend chart — most important, widest span at every breakpoint that allows it |
| `CategoryDonut` | col-span-1 | col-span-1 | col-span-1 | col-span-1 | Donut has fixed ideal aspect ratio — don't stretch wide (chart-domain: pie/donut max 6 slices, wide space wasted) |
| `RecentExpenses` (table) | col-span-1 | col-span-1 | col-span-2 | col-span-2 | Table needs width for columns — avoid horizontal scroll on md/lg |
| `SameMonthChart` (monthly comparison, rolling window) | col-span-1 | col-span-2 | col-span-2 | col-span-2 | Redesigned to a rolling multi-month window (see [dashboard-new-charts-plan.md](dashboard-new-charts-plan.md) §0b) — up to 6 grouped bar-pairs, needs `SpendChart`-tier width, not narrow-safe anymore |
| `CurrenciesPanel` | col-span-1 | col-span-1 | col-span-1 | col-span-1 | Small info panel |

Note: table above reflects **current 6 widgets only**. When new widgets land (budget progress, income vs expenses, largest expenses, upcoming recurring, insights — see other plan), extend this same table; don't create a second layout system.

## Reordered visual hierarchy

Current order (top→bottom in JSX) becomes (per `ui-ux-pro-max` visual-hierarchy guidance: most actionable/important first, KPI before detail, trend before breakdown):

1. `MonthHero` — top-line KPI, orient user immediately
2. `SpendChart` — primary trend (spending over time), the "headline" chart
3. `CategoryDonut` — where money went this month (proportional breakdown)
4. `SameMonthChart` — monthly comparison, contextualizes current month vs history
5. `RecentExpenses` — actionable detail table
6. `CurrenciesPanel` — least time-sensitive, secondary info → moved last

Reasoning: hero+trend+breakdown = "what happened", comparison = "is that normal", recent expenses = "what can I act on", currencies = reference info. This ordering also gives a natural insertion point for new widgets (budget progress after CategoryDonut, income-vs-expenses after SpendChart, largest-expenses/insights near RecentExpenses, upcoming-recurring near CurrenciesPanel) — see other plan's "insertion point" per graph.

## Extensibility contract for future widgets

Document (add to `HomeDashboardPage.tsx` as a short comment, not a doc file) a `DashboardWidget` shape so future graphs (plan 2) don't reinvent layout:

```ts
type DashboardWidgetSpan = {
  base: string;   // col-span-* at mobile
  md?: string;
  lg?: string;
  xl?: string;
};
```

Each widget component wrapper gets its span classes from one place (co-located array or simple per-widget className prop), so column-count changes in this plan don't require touching every widget file again later.

## Implementation steps

1. `HomeDashboardPage.tsx`: replace 3 grids with single grid container, apply span classes per table above, reorder JSX per hierarchy above.
2. Adjust container `max-w-*` classes for xl/2xl per above.
3. Verify `EmptyState` (empty-dashboard branch) still renders full-width/centered regardless of grid change.
4. Check each widget's internal layout (`MonthHero`, `SpendChart`, etc.) doesn't assume a fixed parent width (e.g. recharts `ResponsiveContainer` should already handle it — confirm each chart uses `ResponsiveContainer`, not fixed px width/height).
5. `RecentExpenses` table: confirm `overflow-x-auto` wrapper exists for md/tablet 1-col-span case (table domain rule: wrap tables, don't let them break layout) — add if missing.
6. Visual check for `grid-flow` gaps caused by mixed spans (e.g. col-span-2 next to col-span-1 in a 3-col row leaving 1 empty cell) — reorder JSX or add `lg:grid-flow-row-dense` if gaps appear.
7. Confirm charts don't over-stretch on 2xl (ultrawide) — cap chart height growth even if width grows (e.g. `h-64 xl:h-72 2xl:h-80`, not unbounded), consistent with "bigger graphs / more whitespace" ask without making them illegibly stretched.

## Unit tests

- `HomeDashboardPage.test.tsx`: add assertions that grid container has expected `grid-cols-*` classes at container level (className presence, not actual computed layout — jsdom has no real breakpoints); add a snapshot/order assertion verifying widgets render in the new hierarchy order (query by `data-testid` in expected sequence).
- Per-widget tests (`SpendChart.test.tsx`, `CategoryDonut.test.tsx`, `RecentExpenses.test.tsx`, `SameMonthChart.test.tsx`, `MonthHero.test.tsx`, `CurrenciesPanel.test.tsx`): no behavior change expected from reflow alone — re-run existing suites, only touch them if a widget needs an `overflow-x-auto` wrapper or `ResponsiveContainer` fix (step 4/5 above), then add a regression test for that specific fix (e.g. `RecentExpenses` renders scroll wrapper).
- Add test for `EmptyState` dashboard branch still rendering correctly inside new grid container.
- No backend changes — no backend tests needed for this plan.

## Manual verification (required — plan explicitly calls for visual check)

Use `chrome-devtools` MCP (`resize_page` + `take_screenshot`) or manual browser resize at: 375px (phone), 768px (tablet), 1024px (laptop), 1440px (desktop), 1920px+ and one ultrawide width (e.g. 3440px) to confirm:
- Column counts match table above at each breakpoint.
- No horizontal scroll anywhere.
- No layout-shift/empty-gap artifacts from spans.
- Ultrawide keeps 4 columns with visibly larger gutters/whitespace, not stretched-thin charts.

/done
