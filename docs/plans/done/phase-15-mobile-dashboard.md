# Phase 15 — Mobile Dashboard: Graphs & Stats

**Status:** Complete  
**Version:** 0.115.0  
**Date:** 2026-06-12

---

## Context

The mobile app's `DashboardPage` had a complete `dashboardApi.service.ts` (all 6 backend endpoints wired) but displayed only text, `IonProgressBar` bars, and a basic category list. The web app (Recharts, 6 chart components) was the reference implementation. Goal: bring the mobile home screen to feature parity for analytics.

---

## What Was Built

### Charting Library

`recharts` added to `frontend/mobile/package.json` — same library as the web app, SVG-based, works in Ionic's WebView without a native bridge.

### New Components (`src/features/dashboard/components/`)

| File | Type | Data |
|---|---|---|
| `DashboardDateFilter.tsx` | `IonSegment` (3 buttons) | Local state → emits `{ dateFrom, dateTo, period }` |
| `SpendTrendChart.tsx` | Recharts `AreaChart` | `getMonthly()` |
| `CategoryPieChart.tsx` | Recharts `PieChart` (donut) | `getDashboardCategories()` — top 6 + Other bucket |
| `SameMonthChart.tsx` | Recharts `BarChart` | `getSameMonthYearly()` — year-over-year |
| `CurrenciesPanel.tsx` | `IonList` | `getByCurrency()` |

`DashboardDateFilter` exports `getPeriodDates(period)` — pure function, computes `{ dateFrom, dateTo }` for "month" / "6m" / "year" presets.

All chart components:
- Accept `isLoading: boolean` → render `IonSkeletonText` while fetching
- Accept `displayCurrency?: Currency | null` → use `convertedTotal` when set
- Show translated empty message when data is empty

### Updated DashboardPage (`src/features/dashboard/pages/DashboardPage.tsx`)

- Added `period` state (`useState<Period>('month')`) replacing hardcoded current-month dates
- Added 3 new `useQuery` calls: `getMonthly`, `getByCurrency`, `getSameMonthYearly`
- Section order: DateFilter → Hero → SpendTrendChart → CategoryPieChart → CurrenciesPanel → SameMonthChart → Recent expenses
- Hero subtitle now uses `dashboard.filters.thisMonth` locale key (was a hardcoded fallback)
- Change% indicator uses `dashboard.summary.vs` locale key

### i18n

Added `dashboard.filters.sixMonths` to all 4 locale files:
- EN: `"6 Months"`
- FR: `"6 mois"`
- ES: `"6 meses"`
- DE: `"6 Monate"`

---

## Tests Added / Updated

### New test files (`src/features/dashboard/components/__tests__/`)

| File | Key assertions |
|---|---|
| `DashboardDateFilter.test.tsx` | `getPeriodDates` pure function; button labels rendered |
| `SpendTrendChart.test.tsx` | skeleton / empty / chart renders; converted total path |
| `CategoryPieChart.test.tsx` | category names; percentages; >6 → "Other" bucket |
| `SameMonthChart.test.tsx` | skeleton / empty / chart; title contains month name |
| `CurrenciesPanel.test.tsx` | currency codes; expense counts; convertedAmount used when displayCurrency set |

### Updated test file

`src/features/dashboard/pages/__tests__/DashboardPage.test.tsx` — expanded from 4 to 11 tests:
- All 6 queries fired on mount
- Date filter change updates `period` state
- Summary total and top category render after load
- `SpendTrendChart` and `CategoryPieChart` receive correct `data-count`

### Recharts mock pattern

All chart test files mock `recharts` with:

```typescript
vi.mock('recharts', async () => {
  const actual = await vi.importActual<typeof import('recharts')>('recharts')
  return {
    ...actual,
    ResponsiveContainer: ({ children }: { children: React.ReactNode }) =>
      <div data-testid="responsive-container">{children}</div>,
  }
})
```

This lets jsdom render the chart without SVG viewport issues while keeping all other recharts components (Tooltip, Cell, etc.) from the real implementation.

---

## Verification

```bash
cd frontend/mobile
npm run typecheck   # zero errors
npm test            # all tests pass
```

Manual: `npm run dev` → Dashboard tab — date filter segment switches period → all charts reload.
