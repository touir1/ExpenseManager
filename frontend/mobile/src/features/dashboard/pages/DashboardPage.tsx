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
  IonProgressBar,
  IonText,
  IonSkeletonText,
  IonSelect,
  IonSelectOption,
} from '@ionic/react'
import { useTranslation } from 'react-i18next'
import { getSummary, getDashboardCategories, getRecent } from '@/features/dashboard/services/dashboardApi.service'
import { useFamilies } from '@/features/families/FamilyContext'
import { useDisplayCurrency } from '@/features/currencies/DisplayCurrencyContext'
import { useExpensesData } from '@/features/expenses/ExpensesDataContext'
import { NotificationBell } from '@/features/notifications/components/NotificationBell'
import type { CategoryBreakdownDto } from '@/features/dashboard/types/dashboard.type'

function formatAmount(amount: number, decimals = 2): string {
  return amount.toFixed(decimals)
}

export default function DashboardPage() {
  const { t } = useTranslation()
  const { activeFamilyId } = useFamilies()
  const { displayCurrencyId, setDisplayCurrencyId } = useDisplayCurrency()
  const { currencies } = useExpensesData()

  const now = new Date()
  const dateFrom = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-01`
  const dateTo = now.toISOString().substring(0, 10)

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

  const { data: recentRes } = useQuery({
    queryKey: ['dashboard-recent', filter],
    queryFn: () => getRecent(filter),
  })

  const summary = summaryRes?.ok ? summaryRes.data : null
  const categories = (categoriesRes?.ok ? categoriesRes.data : null) ?? []
  const recentExpenses = (recentRes?.ok ? recentRes.data?.items : null) ?? []

  const topCategories = categories.slice(0, 5)
  const maxCatAmount = topCategories[0]?.totalAmount ?? 1

  const changePercent = summary?.changePercent ?? null
  const isPositive = changePercent !== null && changePercent >= 0

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
        {/* Month hero card */}
        <IonCard>
          <IonCardHeader>
            <IonCardSubtitle>{t('dashboard.thisMonth', 'This month')}</IonCardSubtitle>
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
                  {isPositive ? '+' : ''}{changePercent.toFixed(1)}% vs last month
                </span>
              </IonText>
            )}
            {summary?.topCategory && (
              <p style={{ color: 'var(--ion-color-medium)', marginTop: 4, fontSize: 13 }}>
                Top: {summary.topCategory.name}
              </p>
            )}
          </IonCardContent>
        </IonCard>

        {/* Category breakdown */}
        {topCategories.length > 0 && (
          <IonCard>
            <IonCardHeader>
              <IonCardTitle style={{ fontSize: 16 }}>{t('dashboard.byCategory', 'By category')}</IonCardTitle>
            </IonCardHeader>
            <IonCardContent>
              {catsLoading ? (
                [1, 2, 3].map(i => <IonSkeletonText key={i} animated style={{ height: 16, marginBottom: 12 }} />)
              ) : (
                topCategories.map((cat: CategoryBreakdownDto) => (
                  <div key={cat.category?.id ?? 'uncategorized'} style={{ marginBottom: 12 }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                      <IonText style={{ fontSize: 13 }}>{cat.category?.name ?? t('expenses.uncategorized', 'Uncategorized')}</IonText>
                      <IonText style={{ fontSize: 13, fontWeight: 600 }}>{cat.percentage.toFixed(0)}%</IonText>
                    </div>
                    <IonProgressBar
                      value={cat.totalAmount / maxCatAmount}
                      color="primary"
                    />
                  </div>
                ))
              )}
            </IonCardContent>
          </IonCard>
        )}

        {/* Recent expenses */}
        {recentExpenses.length > 0 && (
          <IonCard>
            <IonCardHeader>
              <IonCardTitle style={{ fontSize: 16 }}>{t('dashboard.recentExpenses', 'Recent expenses')}</IonCardTitle>
            </IonCardHeader>
            <IonList>
              {recentExpenses.slice(0, 5).map(expense => (
                <IonItem key={expense.id}>
                  <IonLabel>
                    <h3>{expense.category?.name ?? t('expenses.uncategorized', 'Uncategorized')}</h3>
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
