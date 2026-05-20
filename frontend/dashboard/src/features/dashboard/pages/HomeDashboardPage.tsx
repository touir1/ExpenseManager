import { useState } from 'react'
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
} from '@/features/dashboard/services/dashboardApi.service'
import { MonthHero } from '@/features/dashboard/components/MonthHero'
import { SpendChart } from '@/features/dashboard/components/SpendChart'
import { CategoryDonut } from '@/features/dashboard/components/CategoryDonut'
import { SameMonthChart } from '@/features/dashboard/components/SameMonthChart'
import { CurrenciesPanel } from '@/features/dashboard/components/CurrenciesPanel'
import { RecentExpenses } from '@/features/dashboard/components/RecentExpenses'
import { DashboardFilters } from '@/features/dashboard/components/DashboardFilters'
import type { DashboardFilter } from '@/features/dashboard/types/dashboard.type'

function todayStr(): string {
  return new Date().toISOString().slice(0, 10)
}

function startOfMonthStr(): string {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-01`
}

export default function HomeDashboardPage() {
  const { t } = useTranslation()
  usePageTitle(t('dashboard.pageTitle'))

  const { user } = useAuth()
  const { activeFamilyId } = useFamilies()
  const { displayCurrencyId } = useDisplayCurrency()

  const [dateFilter, setDateFilter] = useState<{ dateFrom?: string; dateTo?: string }>({
    dateFrom: startOfMonthStr(),
    dateTo: todayStr(),
  })

  const filter: DashboardFilter = {
    ...(activeFamilyId != null ? { familyId: activeFamilyId } : {}),
    ...(displayCurrencyId != null ? { displayCurrencyId } : {}),
    ...dateFilter,
  }

  const currentMonth = new Date().getMonth() + 1

  const summaryQ = useQuery({
    queryKey: ['dashboard', 'summary', filter],
    queryFn: () => getSummary(filter),
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

  const summary = summaryQ.data?.ok ? summaryQ.data.data : undefined
  const monthly = monthlyQ.data?.ok ? (monthlyQ.data.data ?? []) : []
  const categories = categoriesQ.data?.ok ? (categoriesQ.data.data ?? []) : []
  const sameMonth = sameMonthQ.data?.ok ? (sameMonthQ.data.data ?? []) : []
  const currencies = currenciesQ.data?.ok ? (currenciesQ.data.data ?? []) : []
  const recentItems = recentQ.data?.ok ? (recentQ.data.data?.items ?? []) : []

  const displayCurrency = summary?.displayCurrency ?? null

  const name = user?.firstName ?? user?.email ?? t('dashboard.defaultName')

  const handleFilterChange = (f: DashboardFilter) => {
    setDateFilter({ dateFrom: f.dateFrom, dateTo: f.dateTo })
  }

  return (
    <div className="max-w-6xl mx-auto w-full px-4 sm:px-6 py-8">
      <div className="mb-6">
        <h1 className="text-2xl font-semibold text-ink tracking-tight">
          {t('dashboard.greeting', { name })}
        </h1>
      </div>

      <DashboardFilters filter={{ ...dateFilter }} onChange={handleFilterChange} />

      {/* Row 1: Hero + Spend Chart */}
      <div className="grid gap-4 lg:grid-cols-3 mb-4">
        <div className="lg:col-span-1">
          <MonthHero data={summary} isLoading={summaryQ.isLoading} />
        </div>
        <div className="lg:col-span-2">
          <SpendChart
            data={monthly}
            isLoading={monthlyQ.isLoading}
            displayCurrency={displayCurrency}
          />
        </div>
      </div>

      {/* Row 2: Category Donut + Recent Expenses */}
      <div className="grid gap-4 lg:grid-cols-2 mb-4">
        <CategoryDonut data={categories} isLoading={categoriesQ.isLoading} />
        <RecentExpenses data={recentItems} isLoading={recentQ.isLoading} />
      </div>

      {/* Row 3: Same Month Chart + Currencies */}
      <div className="grid gap-4 lg:grid-cols-2">
        <SameMonthChart
          data={sameMonth}
          isLoading={sameMonthQ.isLoading}
          selectedMonth={currentMonth}
          displayCurrency={displayCurrency}
        />
        <CurrenciesPanel data={currencies} isLoading={currenciesQ.isLoading} />
      </div>
    </div>
  )
}
