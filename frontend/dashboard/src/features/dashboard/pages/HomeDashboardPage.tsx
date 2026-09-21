import { useSearchParams } from 'react-router-dom'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useQuery } from '@tanstack/react-query'
import { useAuth } from '@/features/auth/AuthContext'
import { useFamilies } from '@/features/families/FamilyContext'
import { useDisplayCurrency } from '@/features/currencies/DisplayCurrencyContext'
import { usePageTitle } from '@/hooks/usePageTitle'
import {
  getSummary,
  getMonthly,
  getCategories,
  getSameMonthYearly,
  getByCurrency,
  getRecent,
  getLargest,
  getUpcomingRecurring,
} from '@/features/dashboard/services/dashboardApi.service'
import { SpendChart } from '@/features/dashboard/components/SpendChart'
import { CategoryDonut } from '@/features/dashboard/components/CategoryDonut'
import { SameMonthChart } from '@/features/dashboard/components/SameMonthChart'
import { CurrenciesPanel } from '@/features/dashboard/components/CurrenciesPanel'
import { RecentExpenses } from '@/features/dashboard/components/RecentExpenses'
import { LargestExpenses } from '@/features/dashboard/components/LargestExpenses'
import { UpcomingRecurring } from '@/features/dashboard/components/UpcomingRecurring'
import { DashboardFilters } from '@/features/dashboard/components/DashboardFilters'
import { StatCard } from '@/features/dashboard/components/StatCard'
import { DashboardDetails } from '@/features/dashboard/components/DashboardDetails'
import EmptyState from '@/components/EmptyState'
import { formatAmountDisplay } from '@/features/expenses/utils/amountFormat'
import type { DashboardFilter } from '@/features/dashboard/types/dashboard.type'

function todayStr(): string {
  return new Date().toISOString().slice(0, 10)
}

function startOfMonthStr(): string {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-01`
}

function rangeDays(dateFrom?: string, dateTo?: string): number {
  if (!dateFrom) return 1
  const from = new Date(dateFrom + 'T00:00:00')
  const to = dateTo ? new Date(dateTo + 'T00:00:00') : new Date()
  return Math.max(1, Math.round((to.getTime() - from.getTime()) / (1000 * 60 * 60 * 24)) + 1)
}

function getPreviousPeriodLabel(dateFrom?: string, dateTo?: string): string {
  if (!dateFrom) return ''
  const from = new Date(dateFrom + 'T00:00:00')
  const to = dateTo ? new Date(dateTo + 'T00:00:00') : new Date()
  const rangeDays = Math.max(1, Math.round((to.getTime() - from.getTime()) / (1000 * 60 * 60 * 24)))
  const prevTo = new Date(from)
  prevTo.setDate(prevTo.getDate() - 1)
  const prevFrom = new Date(prevTo)
  prevFrom.setDate(prevFrom.getDate() - rangeDays + 1)
  const yearNeeded = prevFrom.getFullYear() !== new Date().getFullYear()
  const opts: Intl.DateTimeFormatOptions = { month: 'short', day: 'numeric', ...(yearNeeded ? { year: 'numeric' } : {}) }
  return `${prevFrom.toLocaleDateString(undefined, opts)} – ${prevTo.toLocaleDateString(undefined, opts)}`
}

export default function HomeDashboardPage() {
  const { t } = useTranslation()
  usePageTitle(t('dashboard.pageTitle'))

  const navigate = useNavigate()
  const { user } = useAuth()
  const { activeFamilyId } = useFamilies()
  const { displayCurrencyId } = useDisplayCurrency()

  const [searchParams, setSearchParams] = useSearchParams()

  const dateFrom = searchParams.get('from') ?? startOfMonthStr()
  const dateTo = searchParams.get('to') ?? todayStr()
  const dateFilter = { dateFrom, dateTo }

  const filter: DashboardFilter = {
    ...(activeFamilyId != null ? { familyId: activeFamilyId } : {}),
    ...(displayCurrencyId != null ? { displayCurrencyId } : {}),
    ...dateFilter,
  }

  const currentMonth = new Date().getMonth() + 1

  const summaryQ = useQuery({
    queryKey: ['dashboard', 'summary', filter],
    queryFn: () => getSummary(filter),
    staleTime: 60_000,
  })

  const monthlyQ = useQuery({
    queryKey: ['dashboard', 'monthly', filter],
    queryFn: () => getMonthly({ ...filter, dateFrom: `${new Date().getFullYear()}-01-01` }),
  })

  const categoriesQ = useQuery({
    queryKey: ['dashboard', 'categories', filter],
    queryFn: () => getCategories(filter),
  })

  const sameMonthQ = useQuery({
    queryKey: ['dashboard', 'sameMonth', currentMonth, activeFamilyId, displayCurrencyId],
    queryFn: () => getSameMonthYearly(currentMonth, activeFamilyId ?? undefined, displayCurrencyId ?? undefined),
  })

  const currenciesQ = useQuery({
    queryKey: ['dashboard', 'currencies', filter],
    queryFn: () => getByCurrency(filter),
  })

  const recentQ = useQuery({
    queryKey: ['dashboard', 'recent', filter],
    queryFn: () => getRecent(filter),
  })

  const largestQ = useQuery({
    queryKey: ['dashboard', 'largest', filter],
    queryFn: () => getLargest(filter),
  })

  const upcomingRecurringQ = useQuery({
    queryKey: ['dashboard', 'upcomingRecurring'],
    queryFn: () => getUpcomingRecurring(5),
  })

  const summary = summaryQ.data?.ok ? summaryQ.data.data : undefined
  const monthly = monthlyQ.data?.ok ? (monthlyQ.data.data ?? []) : []
  const categories = categoriesQ.data?.ok ? (categoriesQ.data.data ?? []) : []
  const sameMonth = sameMonthQ.data?.ok ? (sameMonthQ.data.data ?? []) : []
  const currencies = currenciesQ.data?.ok ? (currenciesQ.data.data ?? []) : []
  const recentItems = recentQ.data?.ok ? (recentQ.data.data?.items ?? []) : []
  const largestItems = largestQ.data?.ok ? (largestQ.data.data?.items ?? []) : []
  const upcomingRecurring = upcomingRecurringQ.data?.ok ? (upcomingRecurringQ.data.data ?? []) : []

  const displayCurrency = summary?.displayCurrency ?? null

  const name = user?.firstName ?? user?.email ?? t('dashboard.defaultName')

  const allLoaded = !summaryQ.isLoading && !categoriesQ.isLoading && !currenciesQ.isLoading
  const isEmpty = allLoaded && (summary?.expenseCount ?? 0) === 0 && categories.length === 0

  const previousPeriodLabel = getPreviousPeriodLabel(dateFrom, dateTo)
  const comparedToLabel = previousPeriodLabel
    ? t('dashboard.summary.comparedTo', { period: previousPeriodLabel })
    : undefined

  const showConverted = summary?.convertedTotal != null && summary?.displayCurrency != null
  const mainAmount = showConverted ? summary!.convertedTotal! : summary?.totalAmount ?? 0
  const mainCurrency = showConverted ? summary!.displayCurrency! : null
  const mainDecimals = mainCurrency?.decimals ?? 2
  const totalValue = `${mainCurrency?.symbol ?? ''} ${formatAmountDisplay(mainAmount, mainDecimals)}`.trim()

  const expenseCount = summary?.expenseCount ?? 0
  const days = rangeDays(dateFrom, dateTo)
  const avgPerDay = expenseCount > 0 ? mainAmount / days : 0
  const avgValue = `${mainCurrency?.symbol ?? ''} ${formatAmountDisplay(avgPerDay, mainDecimals)}`.trim()

  const changePercent = summary?.changePercent
  const changePositive = (changePercent ?? 0) >= 0

  const handleFilterChange = (f: DashboardFilter) => {
    setSearchParams(prev => {
      const next = new URLSearchParams(prev)
      if (f.dateFrom) next.set('from', f.dateFrom)
      else next.delete('from')
      if (f.dateTo) next.set('to', f.dateTo)
      else next.delete('to')
      return next
    })
  }

  const handleCategoryClick = (categoryId: number | null) => {
    const params = new URLSearchParams()
    if (categoryId != null) params.set('categoryId', String(categoryId))
    if (dateFrom) params.set('dateFrom', dateFrom)
    if (dateTo) params.set('dateTo', dateTo)
    navigate(`/expenses?${params.toString()}`)
  }

  return (
    <div className="max-w-6xl xl:max-w-7xl 2xl:max-w-[1800px] mx-auto w-full px-4 sm:px-6 py-8">
      <div className="flex flex-wrap items-center justify-between gap-4 mb-6">
        <h1 className="text-2xl font-semibold text-ink tracking-tight">
          {t('dashboard.greeting', { name })}
        </h1>
        <DashboardFilters filter={{ ...dateFilter }} onChange={handleFilterChange} />
      </div>

      {isEmpty ? (
        <EmptyDashboard onAddExpense={() => navigate('/expenses/add')} />
      ) : (
        <div className="flex flex-col gap-4 xl:gap-6">
          {/* Stat-card row */}
          <div data-testid="dashboard-stats" className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 xl:gap-6">
            <StatCard
              label={t('dashboard.summary.total')}
              value={totalValue}
              isLoading={summaryQ.isLoading}
            />
            <StatCard
              label={t('dashboard.summary.expenses')}
              value={expenseCount}
              isLoading={summaryQ.isLoading}
            />
            <StatCard
              label={t('dashboard.stats.avgPerDay')}
              value={avgValue}
              isLoading={summaryQ.isLoading}
            />
            <StatCard
              label={t('dashboard.summary.vs')}
              value={changePercent != null ? `${changePositive ? '+' : ''}${changePercent.toFixed(1)}%` : '—'}
              isLoading={summaryQ.isLoading}
              trend={
                changePercent != null
                  ? { text: changePositive ? '↑' : '↓', positive: changePositive, title: comparedToLabel }
                  : undefined
              }
            />
          </div>

          {/* Chart row 1: wide primary + narrow secondary */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 xl:gap-6">
            <div data-testid="widget-spend-chart" className="lg:col-span-2">
              <SpendChart data={monthly} isLoading={monthlyQ.isLoading} displayCurrency={displayCurrency} />
            </div>
            <div data-testid="widget-category-donut" className="lg:col-span-1">
              <CategoryDonut
                data={categories}
                isLoading={categoriesQ.isLoading}
                displayCurrency={displayCurrency}
                onCategoryClick={handleCategoryClick}
              />
            </div>
          </div>

          {/* Chart row 2: wide primary + narrow secondary */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 xl:gap-6">
            <div data-testid="widget-same-month-chart" className="lg:col-span-2">
              <SameMonthChart
                data={sameMonth}
                isLoading={sameMonthQ.isLoading}
                selectedMonth={currentMonth}
                displayCurrency={displayCurrency}
              />
            </div>
            <div data-testid="widget-currencies-panel" className="lg:col-span-1">
              <CurrenciesPanel data={currencies} isLoading={currenciesQ.isLoading} displayCurrency={displayCurrency} />
            </div>
          </div>

          {/* Details: tabbed data section, full width */}
          <div data-testid="widget-dashboard-details">
            <DashboardDetails
              recent={<RecentExpenses data={recentItems} isLoading={recentQ.isLoading} />}
              largest={<LargestExpenses data={largestItems} isLoading={largestQ.isLoading} />}
              recurring={<UpcomingRecurring data={upcomingRecurring} isLoading={upcomingRecurringQ.isLoading} />}
            />
          </div>
        </div>
      )}
    </div>
  )
}

function EmptyDashboard({ onAddExpense }: Readonly<{ onAddExpense: () => void }>) {
  const { t } = useTranslation()
  return (
    <EmptyState
      icon="💸"
      title={t('dashboard.emptyState.title')}
      subtitle={t('dashboard.emptyState.subtitle')}
      action={{ label: t('dashboard.emptyState.cta'), onClick: onAddExpense }}
    />
  )
}
