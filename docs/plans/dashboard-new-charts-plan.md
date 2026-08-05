# Dashboard New Charts & Widgets Plan

Companion to [dashboard-responsive-grid-plan.md](dashboard-responsive-grid-plan.md). Each widget below gets its own section: existing-status check, UX design per `ui-ux-pro-max`/`chart` domain guidance, grid insertion point (uses the span/order system from the grid plan), backend needs, and unit tests. Build order = section order (top = highest priority / least backend work).

Existing widgets already covering 3 of the 9 requested items (confirmed via codebase check):
- **Spending over time** → `SpendChart.tsx` (recharts `ComposedChart`, bar+line combo, not pure line)
- **Expenses by category** → `CategoryDonut.tsx` (recharts `PieChart`, donut)
- **Monthly comparison** → `SameMonthChart.tsx` (recharts `BarChart`, same-month-across-years)

These 3 are reviewed for UX gaps below (section 0). The remaining 6 are net-new builds (sections 1–6).

---

## 0. UX review of existing 3 charts

Per `chart` domain guidance: line charts need series distinguished beyond color (dashed/dotted for a11y), donuts need ≤6 slices + mandatory table fallback, bar charts need sort/value labels.

- **`SpendChart.tsx`**: already composed bar+line — confirm it doesn't rely on color alone for the two series (add line dash pattern if missing); confirm `ChartDataTable` a11y fallback exists (per CLAUDE.md it does, shared across `SpendChart`/`CategoryDonut`/`SameMonthChart`) — no action needed if present. Action: audit chart height on `2xl` per grid plan (avoid over-stretch).
- **`CategoryDonut.tsx`**: confirm it caps displayed slices at ≤6 with an "Other" bucket for the rest (chart-domain rule: pie/donut >5 categories → switch representation or bucket). If uncapped today, add "Other" aggregation.
- **`SameMonthChart.tsx`**: full redesign — see section 0b below. This is no longer a "confirm and leave" item; treat as an active rebuild.

No layout changes needed for `SpendChart`/`CategoryDonut` beyond what's in the grid plan — a11y correctness pass only. `SameMonthChart` gets a wider grid span (see grid-plan update below) since it now renders more bars.

**Unit tests:** add regression tests only for whatever gap is found and fixed (e.g. `CategoryDonut.test.tsx` — asserts "Other" bucket appears when >6 categories exist). `SameMonthChart` tests are covered in full in 0b.

---

## 0b. Monthly comparison bar chart — rolling window redesign

Replaces "same month, single year-over-year pair" with a **rolling multi-month window**, each month rendered as a double bar (current year vs prior year), sized responsively (1–6 months) to available width.

### Window rule (exact formula)

Let:
- `W` = window size, 1–6, driven by widget width (see responsive mapping below)
- `M` = current calendar month number (1–12)

```
comparableCount = min(W, M)     // trailing comparable months ending at current month, double bar (current yr vs prior yr)
previewCount    = max(0, W - comparableCount)   // remaining slots, only nonzero early in the year
```

- **Comparable months** (`comparableCount` of them, trailing back from the current month, wrapping across the year boundary when needed e.g. Nov→…→June): each renders **two bars** — current year actual vs same month prior year. If prior-year data doesn't exist (new user), render prior-year bar as 0/absent with a "no data" tick, not a fabricated zero read as "spent nothing."
- **Preview months** (`previewCount` of them, immediately after the current month, only appears when `M < W`): each renders **one bar** — prior year only (current year hasn't happened yet). Visually distinguished from comparable bars (e.g. lower opacity / hatched fill) so it doesn't read as a real current-year data point — ties to `chart` domain rule "don't convey meaning by color alone."

**Worked examples** (matches the request exactly):
- Current = March 2026 (`M=3`), `W=5` → `comparableCount=min(5,3)=3` → Jan/Feb/Mar double bars (2025 vs 2026); `previewCount=5-3=2` → Apr/May single bars (2025 only, muted style).
- Same month, `W=3` (narrower screen) → `comparableCount=min(3,3)=3` → Jan/Feb/Mar double bars only; `previewCount=0` → no Apr/May shown.
- Current = November (`M=11`), `W=6` → `comparableCount=min(6,11)=6` → June–Nov double bars (standard trailing rolling window, no preview since `M>=W`).

### Responsive window-size mapping

Tie `W` to the widget's rendered column span from [dashboard-responsive-grid-plan.md](dashboard-responsive-grid-plan.md), not the raw viewport — the chart should react to its container width (`ResizeObserver` or a width-bucket prop passed down from the grid), matching `chart` domain's `responsive-chart` rule (reflow/simplify on small containers, not just small screens).

| Container width bucket | W (months shown) |
|---|---|
| <360px (mobile, col-span-1) | 1 (current month only, double bar) |
| 360–520px | 2 |
| 520–700px | 3 |
| 700–900px | 4 |
| 900–1200px | 5 |
| ≥1200px (wide desktop/ultrawide) | 6 |

### Visual spec

- Grouped (not stacked) bar pairs per comparable month: `[prior year, current year]`, consistent color pair with `CategoryDonut`/`SpendChart` palette (`chartTheme.ts`).
- Preview months: single bar, muted/hatched fill, no current-year segment.
- X-axis labels: month abbreviation + year only on the first/last tick or on hover tooltip to avoid clutter (`axis-readability` rule — don't cram "Jan 2025 / Jan 2026" per bar).
- Tooltip on hover/tap shows exact values for both years + delta % per `tooltip-on-interact` rule.
- Legend: "This year" / "Last year" / "Upcoming (last year)" for the preview style, always visible (not detached below fold, per `legend-visible`).
- `ChartDataTable` a11y fallback (existing shared pattern) extended to include a "type" column (comparable vs preview) so screen-reader users get the same distinction sighted users get from the muted styling.

### Grid insertion & sizing

Update [dashboard-responsive-grid-plan.md](dashboard-responsive-grid-plan.md)'s span table: `SameMonthChart` moves from `col-span-1` to `col-span-2` at `lg`/`xl`/`2xl` (needs the same width tier as `SpendChart`) since it now renders up to 6 grouped bar-pairs instead of one. Keep it in its existing position in the visual hierarchy (position 6, "is that normal" tier) — no reorder needed, just a wider footprint.

### Data / backend

No new backend entity needed — reuses the same per-month, per-currency-converted total that already powers the existing same-month comparison endpoint; only the query needs to return a **range** of months (trailing `W` months plus up to `previewCount` future-in-year months' prior-year figures) instead of a single month pair. Extend the existing endpoint's response shape from a single `{currentYear, priorYear}` pair to an array of `{month, year, currentYearTotal, priorYearTotal|null}` covering the full window; frontend computes which entries are "comparable" vs "preview" via the same `comparableCount`/`previewCount` formula (or backend flags it directly — flag it backend-side to keep frontend a pure render layer, consistent with other dashboard endpoints doing aggregation server-side).

### Unit tests

- **Backend**: window-range aggregation service test — verifies correct month list for `M<W` (preview months included, prior-year-only) and `M>=W` (pure trailing, no preview) cases; verifies year-boundary wraparound (e.g. Nov with `W=6` spans into prior calendar year correctly); verifies missing-prior-year-data returns `null` not `0`.
- **Frontend** (`SameMonthChart.test.tsx`, full rewrite): 
  - `comparableCount`/`previewCount` formula as a pure exported function — unit test the formula directly against the three worked examples above plus edge cases (`M=1,W=6`→comparableCount=1,previewCount=5; `M=12,W=6`→comparableCount=6,previewCount=0).
  - Renders correct number of bar-groups for a given `W`.
  - Preview months render single muted bar, no current-year segment.
  - Responds to container-width buckets (mock `ResizeObserver`/width prop, assert `W` changes).
  - `ChartDataTable` fallback includes comparable/preview distinction.
  - Missing prior-year data renders "no data" state, not a false zero.

---

## 1. Largest expenses (list)

**Status:** not implemented. Closest relative is `RecentExpenses.tsx` (sorted by recency) — new widget sorts by amount instead.

**Design:** Card-based ranked list (not a chart) — top 5 expenses this period, descending by amount, each row: category icon + description + amount (tabular-nums) + date. Per `chart`/`ux` rules: no color-only meaning, use existing `categoryColors.ts` icon-tint pattern for consistency with `CategoryDonut`.

**Data:** reuse existing expense-list endpoint already powering `RecentExpenses`/`ExpensesPage`, just re-sort client-side by `amount desc`, `take(5)` — **no backend change needed** (filter/sort already available via existing expense query params, confirm `ExpenseFilterDto` supports sort-by-amount; if not, minor backend addition: sort param on existing `GET /expenses` list endpoint used by dashboard).

**Grid insertion point:** next to `RecentExpenses` (col-span-1 at lg/xl, alongside it) — both are "actionable detail" tier per grid plan's hierarchy.

**Unit tests:**
- New `LargestExpenses.test.tsx`: renders top-N sorted desc by amount, empty state, respects family/date filter, tabular amount formatting per `amountFormat.ts` conventions.
- Backend (if sort param added): `ExpenseServiceTests`/`ExpenseControllerTests` — sort-by-amount-desc case.

---

## 2. Recurring payments (upcoming)

**Status:** not implemented anywhere (frontend or backend) — flagged in `docs/plans/implementation-plan.md` as a future roadmap item only ("Template + schedule; auto-create or confirm prompt; 'Upcoming' section").

**Design:** Simple upcoming list, next 3–5 items sorted by due date ascending: description + amount + "in N days" relative label + category tag. Empty state via shared `EmptyState.tsx` compact mode ("No upcoming recurring payments").

**Backend needs (new, biggest lift in this plan):**
- New `RecurringExpense` entity (expenses service): `Id, UserId, Description, Amount, CurrencyId, CategoryId, SubcategoryId?, FamilyId?, Frequency (enum: Weekly/Monthly/Yearly), NextDueDate, IsActive, CreatedAt` — soft-delete pattern (`IsDeleted`+`DeletedAt`) consistent with `Category`/`Family`/`Expense`.
- New lookup table `RecurrenceFrequency` (per CLAUDE.md's enum-as-DB-table convention, resolved via `ILookupCacheService`), not a C# enum.
- New endpoint `GET /recurring-expenses/upcoming?take=5` → sorted by `NextDueDate` asc, `IsActive=true`, `!IsDeleted`.
- CRUD endpoints for managing recurring expenses are **out of scope for the dashboard widget** — this plan only covers the read-only "upcoming" dashboard card. Full CRUD (create/edit/delete recurring templates, auto-create actual `Expense` rows on due date via a Quartz job similar to `RateAutoUpdateJob`) is a separate feature-level plan — flag as a follow-up doc, don't build inline here to avoid scope creep.
- Migration: `AddRecurringExpenses`.

**Grid insertion point:** next to `CurrenciesPanel` (low-urgency reference-tier widgets grouped together per grid plan's hierarchy, col-span-1).

**Unit tests:**
- Backend: `RecurringExpenseRepositoryTests` (SQLite in-memory per existing pattern), `RecurringExpenseServiceTests` (upcoming query filters inactive/deleted/past-due correctly), `RecurringExpenseControllerTests` (200 with list, empty list when none).
- Frontend: `UpcomingRecurring.test.tsx` — renders list, relative date formatting, empty state, respects `take` limit.

---

## 3. Budget progress (per category, progress bars)

**Status:** not implemented — flagged in `implementation-plan.md` as future roadmap ("Per-family/user monthly budget per category; dashboard bars; alerts").

**Design:** Per `chart` domain "Performance vs Target" guidance (gauge/bullet pattern, adapted to progress bars since multiple categories shown together = bullet-chart-grid case): one row per category with a budget set — label + horizontal progress bar (spent/limit) + numeric "€X / €Y (Z%)" text always visible (never color-only — red/amber/green fill AND the percentage text). Cap to top 4–5 categories by usage to avoid a long list; "View all" link to a future full budgets page if list is truncated.

**Backend needs (new):**
- New `Budget` entity (expenses service): `Id, UserId, CategoryId, FamilyId?, MonthlyLimit, CurrencyId, CreatedAt` + soft-delete fields, unique constraint `(UserId, CategoryId, FamilyId)` per active month scope (or simply "current standing budget," no month column, matching "monthly budget" wording — decide: a budget row = a recurring monthly limit, not per-specific-month; only overridden if requirements say otherwise).
- Endpoint `GET /budgets/progress?month=YYYY-MM` → for each budget row, join current month's actual spend per category (reuse existing expense aggregation logic already powering `CategoryDonut`'s totals) → `{categoryId, categoryName, limit, spent, percent}[]`.
- CRUD for creating/editing budgets is a separate settings-page feature (out of scope here, same reasoning as recurring-payments CRUD) — this plan only covers the read-only dashboard progress widget assuming budgets already exist; if none exist yet for a user, widget shows empty state prompting to "Set a budget" (compact `EmptyState` with action link, once the settings UI exists — until then, plain text hint is fine).
- Migration: `AddBudgets`.

**Grid insertion point:** right after `CategoryDonut` (col-span-1 or 2 depending on row count — natural pairing: "where money went" → "how that compares to plan").

**Unit tests:**
- Backend: `BudgetRepositoryTests`, `BudgetServiceTests` (progress calc: percent capped display at 100%+ for over-budget, correct current-month aggregation), `BudgetControllerTests`.
- Frontend: `BudgetProgress.test.tsx` — bar width reflects percent, over-budget visual state (color + text, not color-only per a11y rule), empty state when no budgets set.

---

## 4. Income vs expenses (stacked bar, showcasing savings)

**Status:** not implemented — no income concept exists anywhere in the dashboard/expenses data model today (expenses-only tracking).

**Design:** Per `chart` domain — this is closer to a **waterfall or simple grouped/stacked bar per month**: Income bar (or segment) vs Expense bar (or segment), with Savings = Income − Expenses shown as a call-out number above/beside the chart (always show the number as text, not just implied by bar height per a11y "value labels" rule). Recharts `BarChart` with 2 series (`stackId` not required if side-by-side reads clearer than stacked — evaluate: true "stacked" showing savings as remaining stacked segment on the income bar is more literal to the "showcasing savings" ask, so: single bar per month, stacked segments = [Expenses, Savings], with Income as the bar's total height/label).

**Backend needs (largest new concept in this plan):**
- Requires an **Income** concept, which doesn't exist. Minimal version: new `Income` entity (expenses service, mirrors `Expense` minimally): `Id, UserId, FamilyId?, Amount, CurrencyId, Date, Description?, Source?` + soft-delete. Or, if full income tracking is too large a feature to bundle here, an alternative **reduced scope**: let user set a fixed "Monthly Income" value (single field, like a per-user setting in `UserConfig`, similar to `DefaultCurrencyId`) rather than itemized income entries — much smaller lift, still enables the savings chart.
- **Recommendation:** ship the reduced scope first (single monthly income figure in `UserConfig`, new column `MonthlyIncome decimal?`), defer itemized income tracking to a separate future plan — matches this plan's pattern of not scope-creeping into full CRUD features.
- Endpoint: extend existing `GET /config` response (`UserConfigDto`) with `monthlyIncome`, plus `PUT /config/income` to set it (rate-limited like other config mutations). Dashboard endpoint computes `{month, income: monthlyIncome, expenses: totalSpent, savings: income-expenses}[]` for last N months — likely a new lightweight dashboard aggregation endpoint or computed client-side from data `dashboardApi.service.ts` already fetches.
- Migration: `AddUserConfigMonthlyIncome`.

**Grid insertion point:** near `SpendChart` (col-span-2 at lg/xl) — both are "trend over time" tier, natural pairing per grid plan hierarchy (insert as position 2b, right after or before SameMonthChart).

**Unit tests:**
- Backend: `UserConfigServiceTests` (set/get monthly income), dashboard aggregation test (savings = income − expenses per month, handles missing income as null/hidden state).
- Frontend: `IncomeVsExpenses.test.tsx` — stacked bar renders correct segment proportions, savings call-out number matches computed value, handles "no income set" empty/prompt state (don't render a misleading 0-income bar).

---

## 5. AI-like insights (simple text cards)

**Status:** not implemented.

**Design:** Not actually AI/LLM-backed — simple rule-based comparative statements computed from existing aggregated data (e.g. "You spent 22% more on Restaurants this month" = current-month category total vs previous-month category total, threshold-gated e.g. only show if |change| ≥ 15% to avoid noise). Render as small stat cards (2–3 max), icon + short sentence, no chart — per `ux` guidance this is a content/typography concern more than a chart-domain one: keep each insight one line, `aria-live="polite"` not needed (static per page load, not real-time).

**Backend needs:** none required — can be computed **entirely client-side** from data already fetched for `CategoryDonut`/`SpendChart` (current + previous month category totals). If that comparison data isn't already fetched together, minor addition to existing dashboard aggregation endpoint response to include previous-month category totals alongside current (avoid a second round-trip).

**Insight rules to implement (start small, extensible list):**
1. Category spend delta vs previous month (≥15% threshand, top 1–2 by absolute delta).
2. Total spend delta vs previous month.
3. "No spending in category X this month" (positive framing) if a normally-recurring category has zero this month — nice-to-have, can defer.

**Grid insertion point:** near `RecentExpenses`/`LargestExpenses` (actionable-detail tier), col-span-1 or full-width thin strip if 2–3 cards render side-by-side within their own mini sub-grid.

**Unit tests:**
- `insightsEngine.ts` (pure function, unit-testable independent of React): given current+previous month category maps, returns correct sorted/thresholded insight objects; edge cases: previous month zero (avoid divide-by-zero → show "new category" framing instead of infinite %), no significant changes (returns empty → widget hides or shows neutral "No notable changes" message).
- `Insights.test.tsx`: renders cards from engine output, hides widget/shows empty state when engine returns none.

---

## 6. Insertion order summary (ties into grid plan)

Final widget order after all 6 new + 3 existing (extends grid plan's ordering table — update that file's span table when each widget ships):

1. `MonthHero`
2. `SpendChart` (spending over time)
3. `IncomeVsExpenses` (new)
4. `CategoryDonut` (expenses by category)
5. `BudgetProgress` (new)
6. `SameMonthChart` (monthly comparison)
7. `RecentExpenses`
8. `LargestExpenses` (new)
9. `Insights` (new)
10. `UpcomingRecurring` (new)
11. `CurrenciesPanel`

Build/ship incrementally in section order (1→5 above by increasing backend cost); each ships independently and slots into the existing grid per the extensibility contract in the grid plan — no grid rework needed per addition.

## Cross-cutting unit test notes

- All new backend entities follow existing soft-delete + repository/service/controller layering + FluentValidation validator conventions already in the expenses service — mirror existing test patterns (`TestExpensesDbContextWrapper`/`TestExpensesDbContextEnsureCreated`, Moq for repos, never mock DbContext).
- All new frontend widgets: co-locate tests in `components/__tests__/`, use `ChartDataTable`-style a11y fallback for any new chart type (income-vs-expenses stacked bar, budget progress bars need a text-equivalent per WCAG chart rule).
- Add each new widget's error/loading/empty states to tests explicitly (per `ux` forms/feedback guidance: empty-states, loading-chart skeleton) — don't only test the happy path.

/done
