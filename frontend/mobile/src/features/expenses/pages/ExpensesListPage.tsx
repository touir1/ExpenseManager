import { useEffect, useRef, useState } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import {
  IonPage,
  IonHeader,
  IonToolbar,
  IonTitle,
  IonContent,
  IonList,
  IonItem,
  IonLabel,
  IonItemDivider,
  IonItemSliding,
  IonItemOption,
  IonItemOptions,
  IonRefresher,
  IonRefresherContent,
  IonInfiniteScroll,
  IonInfiniteScrollContent,
  IonText,
  IonSegment,
  IonSegmentButton,
  IonAlert,
  IonSkeletonText,
} from '@ionic/react'
import { useTranslation } from 'react-i18next'
import { getExpenses, deleteExpense } from '@/features/expenses/services/expensesApi.service'
import { useFamilies } from '@/features/families/FamilyContext'
import { useDisplayCurrency } from '@/features/currencies/DisplayCurrencyContext'
import type { ExpenseDto } from '@/features/expenses/types/expenses.type'
import type { Family } from '@/features/families/types/family.type'
import { NotificationBell } from '@/features/notifications/components/NotificationBell'

function groupByDate(items: ExpenseDto[]): Map<string, ExpenseDto[]> {
  const map = new Map<string, ExpenseDto[]>()
  for (const item of items) {
    const key = item.date.substring(0, 10)
    if (!map.has(key)) map.set(key, [])
    map.get(key)!.push(item)
  }
  return map
}

export default function ExpensesListPage() {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { families, activeFamilyId, setActiveFamilyId } = useFamilies()
  const { displayCurrencyId } = useDisplayCurrency()
  const [page, setPage] = useState(1)
  const [allItems, setAllItems] = useState<ExpenseDto[]>([])
  const [hasMore, setHasMore] = useState(true)
  const [deleteTarget, setDeleteTarget] = useState<number | null>(null)
  const slidingRefs = useRef<Map<number, HTMLIonItemSlidingElement>>(new Map())

  const PAGE_SIZE = 20

  const { data, isFetching, refetch } = useQuery({
    queryKey: ['expenses', activeFamilyId, displayCurrencyId, page],
    queryFn: () =>
      getExpenses({
        familyId: activeFamilyId ?? undefined,
        displayCurrencyId: displayCurrencyId ?? undefined,
        page,
        pageSize: PAGE_SIZE,
      }),
    enabled: true,
  })

  useEffect(() => {
    if (data?.ok && data.data) {
      const items = data.data.items
      if (page === 1) {
        setAllItems(items)
      } else {
        setAllItems(prev => [...prev, ...items])
      }
      setHasMore(data.data.page < data.data.totalPages)
    }
  }, [data, page])

  useEffect(() => {
    setPage(1)
    setAllItems([])
    setHasMore(true)
  }, [activeFamilyId])

  async function handleRefresh(e: CustomEvent) {
    setPage(1)
    setAllItems([])
    await refetch()
    ;(e.target as HTMLIonRefresherElement).complete()
  }

  async function handleInfiniteScroll(e: CustomEvent) {
    if (hasMore && !isFetching) {
      setPage(p => p + 1)
    }
    ;(e.target as HTMLIonInfiniteScrollElement).complete()
  }

  async function handleDelete(id: number) {
    await deleteExpense(id)
    setAllItems(prev => prev.filter(e => e.id !== id))
    queryClient.invalidateQueries({ queryKey: ['expenses'] })
    setDeleteTarget(null)
  }

  const grouped = groupByDate(allItems)
  const activeFamilies = families.filter((f: Family) => !f.isArchived)

  return (
    <IonPage>
      <IonHeader>
        <IonToolbar color="light">
          <IonTitle>{t('nav.expenses')}</IonTitle>
          <NotificationBell slot="end" />
        </IonToolbar>
        {activeFamilies.length > 1 && (
          <IonToolbar color="light">
            <IonSegment
              value={activeFamilyId != null ? String(activeFamilyId) : 'all'}
              onIonChange={e => {
                const v = e.detail.value as string
                setActiveFamilyId(v === 'all' ? null : Number(v))
              }}
            >
              <IonSegmentButton value="all">
                <IonLabel>{t('expenses.allFamilies', 'All')}</IonLabel>
              </IonSegmentButton>
              {activeFamilies.map((f: Family) => (
                <IonSegmentButton key={f.id} value={String(f.id)}>
                  <IonLabel>{f.name}</IonLabel>
                </IonSegmentButton>
              ))}
            </IonSegment>
          </IonToolbar>
        )}
      </IonHeader>

      <IonContent>
        <IonRefresher slot="fixed" onIonRefresh={handleRefresh}>
          <IonRefresherContent />
        </IonRefresher>

        {isFetching && allItems.length === 0 && (
          <IonList>
            {[1, 2, 3, 4, 5].map(i => (
              <IonItem key={i}>
                <IonLabel>
                  <IonSkeletonText animated style={{ width: '60%' }} />
                  <IonSkeletonText animated style={{ width: '40%' }} />
                </IonLabel>
              </IonItem>
            ))}
          </IonList>
        )}

        {!isFetching && allItems.length === 0 && (
          <div style={{ textAlign: 'center', padding: '40px 16px' }}>
            <IonText color="medium">
              <p>{t('expenses.empty', 'No expenses yet.')}</p>
            </IonText>
          </div>
        )}

        <IonList>
          {Array.from(grouped.entries()).map(([date, items]) => (
            <span key={date}>
              <IonItemDivider>
                <IonLabel>
                  {new Date(date).toLocaleDateString(undefined, {
                    weekday: 'short', month: 'short', day: 'numeric',
                  })}
                </IonLabel>
              </IonItemDivider>
              {items.map(expense => (
                <IonItemSliding
                  key={expense.id}
                  ref={el => {
                    if (el) slidingRefs.current.set(expense.id, el)
                    else slidingRefs.current.delete(expense.id)
                  }}
                >
                  <IonItem>
                    <IonLabel>
                      <h3>{expense.category?.name ?? t('expenses.uncategorized', 'Uncategorized')}</h3>
                      <p>{expense.description ?? ''}</p>
                    </IonLabel>
                    <IonText slot="end" color="dark">
                      <span style={{ fontWeight: 600 }}>
                        {expense.amount.toFixed(expense.currency?.decimals ?? 2)}{' '}
                        {expense.currency?.symbol ?? ''}
                      </span>
                    </IonText>
                  </IonItem>
                  <IonItemOptions side="start">
                    <IonItemOption
                      color="danger"
                      onClick={() => {
                        slidingRefs.current.get(expense.id)?.close()
                        setDeleteTarget(expense.id)
                      }}
                    >
                      {t('common.delete', 'Delete')}
                    </IonItemOption>
                  </IonItemOptions>
                </IonItemSliding>
              ))}
            </span>
          ))}
        </IonList>

        <IonInfiniteScroll
          disabled={!hasMore}
          onIonInfinite={handleInfiniteScroll}
        >
          <IonInfiniteScrollContent loadingText={t('common.loading', 'Loading…')} />
        </IonInfiniteScroll>
      </IonContent>

      <IonAlert
        isOpen={deleteTarget !== null}
        header={t('expenses.deleteConfirm', 'Delete expense?')}
        message={t('expenses.deleteConfirmMessage', 'This cannot be undone.')}
        buttons={[
          { text: t('common.cancel', 'Cancel'), role: 'cancel', handler: () => setDeleteTarget(null) },
          { text: t('common.delete', 'Delete'), role: 'destructive', handler: () => deleteTarget && handleDelete(deleteTarget) },
        ]}
        onDidDismiss={() => setDeleteTarget(null)}
      />
    </IonPage>
  )
}
