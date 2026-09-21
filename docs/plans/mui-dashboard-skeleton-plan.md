# MUI Dashboard Template — Skeleton Adoption Plan

Reference: https://mui.com/material-ui/getting-started/templates/dashboard/

Goal: match MUI's dashboard **structure/placement** (sidebar, header, stat-card row, content grid, data section) without touching color tokens (`brand-*`, `surface-*`, `ink-*` stay as-is). No MUI library adoption — Tailwind classes reproduce the layout only.

## 1. Current skeleton (baseline)

```
RootLayout
 └─ NavBar (sticky top, full-width, horizontal links: Dashboard/Expenses/Families/Admin)
      right cluster: FamilySelector, CurrencySelector, +Add, NotificationBell, Theme, Avatar menu
 └─ <main>
      HomeDashboardPage
        h1 greeting
        DashboardFilters (date range)
        grid: MonthHero, SpendChart, CategoryDonut, SameMonthChart,
              RecentExpenses, LargestExpenses, CurrenciesPanel, UpcomingRecurring
```

No sidebar. No stat-card row. Widgets are uniform panel cards in one grid, mixing summary + charts + lists at equal visual weight.

## 2. MUI template skeleton (target structure)

```
Shell
 ├─ SideNav (fixed left, collapsible on mobile → temporary drawer)
 │    - Logo/brand block (top)
 │    - Primary nav list (icon + label, active state pill)
 │    - spacer
 │    - Secondary nav list (Settings, About/Help)
 │    - User card (avatar, name/email, menu) — bottom, above a divider
 ├─ AppBar (top, spans content area only — not full width)
 │    - left: mobile menu button (hidden ≥ md) + page title / search field
 │    - right: icon cluster (notifications, theme toggle), avatar
 ├─ Content
 │    - Header row: page title ("Overview") + primary actions (date range control, Export/Add button) right-aligned
 │    - Stat-card row: 4 equal KPI cards (label, big value, trend chip, inline sparkline)
 │    - Main grid (2-col on desktop):
 │        left/wide column: primary chart (large, ~2/3 width)
 │        right/narrow column: secondary chart or donut (~1/3 width)
 │    - Second grid row: more charts / breakdowns, same 2:1 or 1:1:1 split
 │    - Data section: table/list (e.g. "Details" or "Recent orders" equivalent) full width, bottom
```

Key structural traits to replicate:
- **Persistent left sidebar** replaces the horizontal top nav for primary navigation.
- **AppBar is scoped to content width** (sits right of sidebar), carries page context (title/search) instead of nav links.
- **Explicit stat-card row** separates "at a glance" KPIs from charts — currently MonthHero is a single wide card doing this job alone.
- **Two-column asymmetric chart grid** (large + small side by side) instead of a uniform equal-span grid.
- **Actions (date range, export/add) live in the content header**, not the global nav bar.
- **User identity + sign out anchored in the sidebar footer**, not a top-right dropdown.

## 3. Component mapping (existing → new slot)

| MUI slot | Existing component | Change needed |
|---|---|---|
| Sidebar logo | `NavBar` logo block (`Link` + svg mark + wordmark) | Move into new `SideNav`, keep markup/colors |
| Sidebar primary nav | `NavLink`s for Dashboard/Expenses/Families/Admin | Move into `SideNav`, vertical list, icon + label (currently text-only — add simple inline icons) |
| Sidebar secondary nav | Settings link (currently in avatar dropdown) | Promote to sidebar secondary list, above user card |
| Sidebar user card | Avatar circle + dropdown (name absent today, only initials) | New `SideNavUserCard`: avatar, `user.firstName/lastName`, email, click → existing dropdown content (Settings/Language/Sign out) reused as a popover anchored to the card |
| AppBar | Right-hand cluster in current `NavBar` (`FamilySelector`, `DisplayCurrencySelector`, Add button, `NotificationBell`, `NavBarThemeButton`) | Keep components, relocate into new slim `AppHeader` bar that sits above page content, left-aligned page title added |
| Content header actions | `DashboardFilters` (date range) | Move from body into content header row, right-aligned next to page title |
| Stat-card row | `MonthHero` (currently one big card) | Split into up to 4 compact stat cards: total spend, expense count, average/day, vs-previous-period delta — reuse `MonthHero`'s data (`summaryQ`), new smaller card component |
| Primary chart (wide) | `SpendChart` | Keep component, widen to ~2/3 col in new grid |
| Secondary chart (narrow) | `CategoryDonut` | Keep component, narrow to ~1/3 col, same row as `SpendChart` |
| Second chart row | `SameMonthChart` (wide) + `CurrenciesPanel` (narrow) | Same 2:1 split pattern |
| Data/list section | `RecentExpenses`, `LargestExpenses`, `UpcomingRecurring` | Group into one full-width bottom section (tabs or stacked panels — pick tabs to match MUI's single "Details" table slot; avoids 3 competing equal-weight cards) |
| Mobile nav | Current hamburger → horizontal slide-down menu | Sidebar becomes a temporary (overlay) drawer on mobile instead, triggered by AppHeader's menu button |

## 4. New/changed files

- `layouts/SideNav.tsx` — new: logo, primary nav list, secondary nav list, user card + popover (extract dropdown menu content out of current `NavBar` into a shared `UserMenu` piece so it isn't duplicated)
- `layouts/AppHeader.tsx` — new: slim top bar (page title slot via context/prop, or per-page `usePageHeader` hook), right-side icon cluster (moved from `NavBar`)
- `layouts/RootLayout.tsx` — restructure to `SideNav` + (`AppHeader` + `Outlet`) flex row instead of stacked `NavBar` + `main`
- `layouts/NavBar.tsx` — retired; logic split across `SideNav`/`AppHeader`/shared `UserMenu`
- `components/UserMenu.tsx` — new: Settings link, language switcher, sign out (content reused by both `SideNav` user card and, if needed, mobile drawer)
- `features/dashboard/components/StatCard.tsx` — new: compact KPI card (label, value, trend chip, optional sparkline), replaces part of `MonthHero`'s role
- `features/dashboard/components/DashboardDetails.tsx` — new: tabbed/stacked wrapper hosting `RecentExpenses` / `LargestExpenses` / `UpcomingRecurring` as one bottom section
- `features/dashboard/pages/HomeDashboardPage.tsx` — regrid: header row (title + `DashboardFilters`), stat-card row, two chart rows (2:1 split), `DashboardDetails` at bottom
- `features/dashboard/components/MonthHero.tsx` — either removed (superseded by `StatCard` row) or kept as the "total spend" stat card variant — decide during implementation, keep its trend/comparison-label logic either way

Mobile nav test files (`layouts/__tests__/NavBar.test.tsx`, `RootLayout.test.tsx`) move/rename alongside the component split; assertions about nav links and sign-out behavior carry over onto `SideNav`.

## 5. What stays unchanged

- All color tokens (`brand-*`, `surface-*`, `ink-*`), `@layer components` classes in `index.css`
- `FamilySelector`, `DisplayCurrencySelector`, `NotificationBell`, `NavBarThemeButton`, `AddExpenseModal` — same components, only relocated
- Chart components (`SpendChart`, `CategoryDonut`, `SameMonthChart`, `CurrenciesPanel`) — same internals, only grid span changes
- Data fetching (`dashboardApi.service.ts`, all `useQuery` calls) — untouched
- i18n keys — reused; add new ones only for sidebar labels/aria if wording changes (e.g. "toggle sidebar")
- Radix Dialog/Popover patterns already established — `UserMenu` popover follows the existing `Popover.Anchor`/`Popover.Portal` pattern used elsewhere (per `CLAUDE.md` portal-dropdown convention)

## 6. Rollout order

1. Extract `UserMenu` from `NavBar` (no visual change yet) — de-risks the sidebar split
2. Build `SideNav` + `AppHeader`, wire into `RootLayout` behind existing routes (nav links, right-cluster icons move, mobile becomes drawer)
3. Verify all non-dashboard pages still render correctly under new shell (Expenses, Families, Settings, Admin) — shell change is site-wide, not dashboard-only
4. Rebuild `HomeDashboardPage` grid: header row → stat cards → 2:1 chart rows → `DashboardDetails`
5. Add `StatCard`, decide `MonthHero` fate, add `DashboardDetails` tabs
6. Update/move tests (`NavBar.test.tsx` → `SideNav.test.tsx`/`AppHeader.test.tsx`, `HomeDashboardPage.test.tsx` assertions on new `data-testid`s), run `npm run typecheck` + `npm test`
7. Manual browser check (dev server) per CLAUDE.md UI-change rule — golden path + mobile drawer + empty-dashboard state

## 7. Implementation checklist

**Shell**
- [x] Extract `components/UserMenu.tsx` from `NavBar` (language switcher, sign out) — Settings promoted to sidebar's own secondary nav instead of staying in the popover
- [x] Build `layouts/SideNav.tsx` (logo, primary nav list w/ icons, secondary nav (Settings), user card + `UserMenu` Radix popover)
- [x] Build `layouts/AppHeader.tsx` (page title slot, right icon cluster: FamilySelector, DisplayCurrencySelector, +Add, NotificationBell, NavBarThemeButton)
- [x] Restructure `layouts/RootLayout.tsx` to `SideNav` + (`AppHeader` + `Outlet`) flex row (authenticated) / `MarketingHeader` + `Outlet` (unauthenticated)
- [x] Mobile: `SideNav` becomes temporary/overlay drawer, triggered from `AppHeader` menu button
- [x] Retire `layouts/NavBar.tsx` (split into `SideNav`/`AppHeader`/`MarketingHeader`/`UserMenu`)
- [ ] Verify Expenses/Families/Settings/Admin pages render correctly under new shell — unit tests pass under the new shell; no manual browser pass done this session

**Dashboard page**
- [x] Content header row: page title + `DashboardFilters` (moved out of body)
- [x] Build `features/dashboard/components/StatCard.tsx` (label, value, trend chip; no sparkline — not needed for the current data shape)
- [x] Stat-card row (4 cards: total spend, expense count, avg/day, vs-previous-period) wired to `summaryQ` data
- [x] Retire `MonthHero` — its total/trend logic now lives in the stat-card row
- [x] Chart row 1: `SpendChart` (wide, ~2/3) + `CategoryDonut` (narrow, ~1/3)
- [x] Chart row 2: `SameMonthChart` (wide, ~2/3) + `CurrenciesPanel` (narrow, ~1/3)
- [x] Build `features/dashboard/components/DashboardDetails.tsx` (tabs: Recent / Largest / Upcoming Recurring), full width bottom
- [x] Rebuild `HomeDashboardPage.tsx` grid to header → stat cards → chart row 1 → chart row 2 → `DashboardDetails`
- [x] Preserve empty-dashboard (`EmptyState`) path

**Tests & i18n**
- [x] Replace `NavBar.test.tsx` with `SideNav.test.tsx` + `AppHeader.test.tsx` + `MarketingHeader.test.tsx` + `UserMenu.test.tsx`
- [x] Update `RootLayout.test.tsx` for new shell structure (asserts MarketingHeader vs SideNav+AppHeader by auth state)
- [x] Update `HomeDashboardPage.test.tsx` for stat-card row + tabbed details section
- [x] Add `dashboard.stats.avgPerDay` i18n key (`en`/`fr`/`es`/`de`) — no new sidebar/drawer-specific labels needed, existing `nav.toggleMenu`/`nav.mobileNav`/`nav.userMenu` reused
- [x] `npm run typecheck`
- [x] `npm test` (1255 passed)

**Manual verification**
- [ ] Dev server golden path (desktop) — nav, add expense, filters, charts render
- [ ] Mobile drawer open/close, focus trap, Escape-to-close
- [ ] Empty-dashboard state
- [ ] Dark mode (color-scheme unaffected by layout change)
