import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import {
  IonPage,
  IonHeader,
  IonToolbar,
  IonTitle,
  IonContent,
  IonCard,
  IonCardContent,
  IonCardHeader,
  IonCardSubtitle,
  IonCardTitle,
  IonList,
  IonItem,
  IonLabel,
  IonText,
  IonSkeletonText,
  IonSelect,
  IonSelectOption,
} from '@ionic/react'
import { useTranslation } from 'react-i18next'
import {
  getSummary,
  getDashboardCategories,
  getMonthly,
  getSameMonthYearly,
  getByCurrency,
  getRecent,
} from '@/features/dashboard/services/dashboardApi.service'
import { useFamilies } from '@/features/families/FamilyContext'
import { useDisplayCurrency } from '@/features/currencies/DisplayCurrencyContext'
import { useExpensesData } from '@/features/expenses/ExpensesDataContext'
import { NotificationBell } from '@/features/notifications/components/NotificationBell'
import { DashboardDateFilter, getPeriodDates } from '@/features/dashboard/components/DashboardDateFilter'
import { SpendTrendChart } from '@/features/dashboard/components/SpendTrendChart'
import { CategoryPieChart } from '@/features/dashboard/components/CategoryPieChart'
import { SameMonthChart } from '@/features/dashboard/components/SameMonthChart'
import { CurrenciesPanel } from '@/features/dashboard/components/CurrenciesPanel'
import type { Period } from '@/features/dashboard/components/DashboardDateFilter'

function formatAmount(amount: number, decimals = 2): string {
  return amount.toFixed(decimals)
}

export default function DashboardPage() {
  const { t } = useTranslation()
  const { activeFamilyId } = useFamilies()
  const { displayCurrencyId, setDisplayCurrencyId } = useDisplayCurrency()
  const { currencies } = useExpensesData()

  const [period, setPeriod] = useState<Period>('month')
  const { dateFrom, dateTo } = getPeriodDates(period)

  const filter = {
    familyId: activeFamilyId ?? undefined,
    displayCurrencyId: displayCurrencyId ?? undefined,
    dateFrom,
    dateTo,
  }

  const { data: summaryRes, isLoading: summaryLoading } = useQuery({
    queryKey: ['dashboard-summary', filter],
    queryFn: () => getSummary(filter),
  })

  const { data: categoriesRes, isLoading: catsLoading } = useQuery({
    queryKey: ['dashboard-categories', filter],
    queryFn: () => getDashboardCategories(filter),
  })

  const { data: monthlyRes, isLoading: monthlyLoading } = useQuery({
    queryKey: ['dashboard-monthly', filter],
    queryFn: () => getMonthly(filter),
  })

  const { data: byCurrencyRes, isLoading: currencyLoading } = useQuery({
    queryKey: ['dashboard-by-currency', filter],
    queryFn: () => getByCurrency(filter),
  })

  const now = new Date()
  const { data: sameMonthRes, isLoading: sameMonthLoading } = useQuery({
    queryKey: ['dashboard-same-month', now.getMonth() + 1, activeFamilyId, displayCurrencyId],
    queryFn: () => getSameMonthYearly(now.getMonth() + 1, activeFamilyId ?? undefined, displayCurrencyId ?? undefined),
  })

  const { data: recentRes } = useQuery({
    queryKey: ['dashboard-recent', filter],
    queryFn: () => getRecent(filter),
  })

  const summary = summaryRes?.ok ? summaryRes.data : null
  const categories = (categoriesRes?.ok ? categoriesRes.data : null) ?? []
  const monthly = (monthlyRes?.ok ? monthlyRes.data : null) ?? []
  const byCurrency = (byCurrencyRes?.ok ? byCurrencyRes.data : null) ?? []
  const sameMonth = (sameMonthRes?.ok ? sameMonthRes.data : null) ?? []
  const recentExpenses = (recentRes?.ok ? recentRes.data?.items : null) ?? []

  const changePercent = summary?.changePercent ?? null
  const isPositive = changePercent !== null && changePercent >= 0
  const displayCurrency = summary?.displayCurrency ?? null

  return (
    <IonPage>
      <IonHeader>
        <IonToolbar color="light">
          <IonTitle>{t('nav.dashboard', 'Dashboard')}</IonTitle>
          <IonSelect
            slot="end"
            value={displayCurrencyId}
            onIonChange={e => setDisplayCurrencyId(e.detail.value)}
            interface="action-sheet"
            style={{ maxWidth: 80, fontSize: 14 }}
          >
            {currencies.map(c => (
              <IonSelectOption key={c.id} value={c.id}>{c.code}</IonSelectOption>
            ))}
          </IonSelect>
          <NotificationBell slot="end" />
        </IonToolbar>
      </IonHeader>

      <IonContent>
        <DashboardDateFilter
          value={period}
          onChange={({ period: p }) => setPeriod(p)}
        />

        {/* Month hero card */}
        <IonCard>
          <IonCardHeader>
            <IonCardSubtitle>{t('dashboard.filters.thisMonth')}</IonCardSubtitle>
            {summaryLoading ? (
              <IonSkeletonText animated style={{ width: '60%', height: 28 }} />
            ) : (
              <IonCardTitle style={{ fontSize: 28, fontWeight: 700 }}>
                {formatAmount(summary?.totalAmount ?? 0)}{' '}
                {summary?.displayCurrency?.symbol ?? ''}
              </IonCardTitle>
            )}
          </IonCardHeader>
          <IonCardContent>
            {changePercent !== null && (
              <IonText color={isPositive ? 'danger' : 'success'}>
                <span style={{ fontWeight: 600 }}>
                  {isPositive ? '+' : ''}{changePercent.toFixed(1)}% {t('dashboard.summary.vs')}
                </span>
              </IonText>
            )}
            {summary?.topCategory && (
              <p style={{ color: 'var(--ion-color-medium)', marginTop: 4, fontSize: 13 }}>
                {t('dashboard.summary.topCategory')} {summary.topCategory.name}
              </p>
            )}
            {summaryLoading && (
              <IonSkeletonText animated style={{ width: '80%', height: 14 }} />
            )}
          </IonCardContent>
        </IonCard>

        {/* Monthly spend trend */}
        <SpendTrendChart
          data={monthly}
          isLoading={monthlyLoading}
          displayCurrency={displayCurrency}
        />

        {/* Category breakdown */}
        <CategoryPieChart
          data={categories}
          isLoading={catsLoading}
          displayCurrency={displayCurrency}
        />

        {/* Currency breakdown */}
        <CurrenciesPanel
          data={byCurrency}
          isLoading={currencyLoading}
          displayCurrency={displayCurrency}
        />

        {/* Year-over-year same month */}
        <SameMonthChart
          data={sameMonth}
          isLoading={sameMonthLoading}
          displayCurrency={displayCurrency}
        />

        {/* Recent expenses */}
        {(recentExpenses.length > 0 || summaryLoading) && (
          <IonCard>
            <IonCardHeader>
              <IonCardTitle style={{ fontSize: 16 }}>{t('dashboard.recent.title')}</IonCardTitle>
            </IonCardHeader>
            <IonList>
              {recentExpenses.slice(0, 5).map(expense => (
                <IonItem key={expense.id}>
                  <IonLabel>
                    <h3>{expense.category?.name ?? t('expenses.uncategorised')}</h3>
                    <p>{expense.date.substring(0, 10)}</p>
                  </IonLabel>
                  <IonText slot="end" color="dark">
                    {formatAmount(expense.amount)} {expense.currency?.symbol ?? ''}
                  </IonText>
                </IonItem>
              ))}
            </IonList>
          </IonCard>
        )}
      </IonContent>
    </IonPage>
  )
}
