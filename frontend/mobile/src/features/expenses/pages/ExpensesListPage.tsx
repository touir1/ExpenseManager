import { useEffect, useMemo, useRef, useState } from 'react'
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
  IonSearchbar,
  IonToast,
} from '@ionic/react'
import { useTranslation } from 'react-i18next'
import { getExpenses, deleteExpense, addExpense } from '@/features/expenses/services/expensesApi.service'
import { useFamilies } from '@/features/families/FamilyContext'
import { useDisplayCurrency } from '@/features/currencies/DisplayCurrencyContext'
import { useNetworkSync } from '@/hooks/useNetworkSync'
import { dateGroupLabel } from '@/features/expenses/utils/dateGroupLabel'
import type { ExpenseDto } from '@/features/expenses/types/expenses.type'
import type { Family } from '@/features/families/types/family.type'
import { NotificationBell } from '@/features/notifications/components/NotificationBell'

const ITEM_HEIGHT_PX = 72
const MAX_SKELETON_ROWS = 20
const SEARCH_DEBOUNCE_MS = 400
const UNDO_WINDOW_MS = 5000

function groupByDate(items: ExpenseDto[]): Map<string, ExpenseDto[]> {
  const map = new Map<string, ExpenseDto[]>()
  for (const item of items) {
    const key = item.date.substring(0, 10)
    if (!map.has(key)) map.set(key, [])
    map.get(key)!.push(item)
  }
  return map
}

function getSkeletonCount(): number {
  const viewportHeight = typeof window !== 'undefined' ? window.innerHeight : 0
  const computed = Math.ceil(viewportHeight / ITEM_HEIGHT_PX)
  return Math.min(MAX_SKELETON_ROWS, Math.max(5, computed))
}

export default function ExpensesListPage() {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { families, activeFamilyId, setActiveFamilyId } = useFamilies()
  const { displayCurrencyId } = useDisplayCurrency()
  const { isOnline } = useNetworkSync()
  const [page, setPage] = useState(1)
  const [allItems, setAllItems] = useState<ExpenseDto[]>([])
  const [hasMore, setHasMore] = useState(true)
  const [deleteTarget, setDeleteTarget] = useState<number | null>(null)
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState('')
  const [undoState, setUndoState] = useState<{ expense: ExpenseDto } | null>(null)
  const slidingRefs = useRef<Map<number, HTMLIonItemSlidingElement>>(new Map())
  const searchDebounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  const undoTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const PAGE_SIZE = 20

  const { data, isFetching, refetch } = useQuery({
    queryKey: ['expenses', activeFamilyId, displayCurrencyId, page, search],
    queryFn: () =>
      getExpenses({
        familyId: activeFamilyId ?? undefined,
        displayCurrencyId: displayCurrencyId ?? undefined,
        description: search || undefined,
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
  }, [activeFamilyId, search])

  useEffect(() => {
    return () => {
      if (searchDebounceRef.current) clearTimeout(searchDebounceRef.current)
      if (undoTimerRef.current) clearTimeout(undoTimerRef.current)
    }
  }, [])

  function handleSearchInput(value: string) {
    setSearchInput(value)
    if (searchDebounceRef.current) clearTimeout(searchDebounceRef.current)
    searchDebounceRef.current = setTimeout(() => setSearch(value.trim()), SEARCH_DEBOUNCE_MS)
  }

  async function handleRefresh(e: CustomEvent) {
    if (!isOnline) {
      ;(e.target as HTMLIonRefresherElement).complete()
      return
    }
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
    const target = allItems.find(e => e.id === id)
    await deleteExpense(id)
    setAllItems(prev => prev.filter(e => e.id !== id))
    queryClient.invalidateQueries({ queryKey: ['expenses'] })
    setDeleteTarget(null)

    if (target) {
      if (undoTimerRef.current) clearTimeout(undoTimerRef.current)
      setUndoState({ expense: target })
      undoTimerRef.current = setTimeout(() => setUndoState(null), UNDO_WINDOW_MS)
    }
  }

  async function handleUndo() {
    if (!undoState) return
    const { expense } = undoState
    if (undoTimerRef.current) clearTimeout(undoTimerRef.current)
    setUndoState(null)
    const res = await addExpense({
      amount: expense.amount,
      currencyId: expense.currency?.id ?? 0,
      date: expense.date,
      categoryId: expense.category?.id,
      subcategoryId: expense.subcategory?.id,
      description: expense.description ?? undefined,
      familyIds: expense.families?.map(f => f.id),
      tagIds: expense.tags.map(tg => tg.id),
    })
    if (res.ok) {
      queryClient.invalidateQueries({ queryKey: ['expenses'] })
      setPage(1)
      await refetch()
    }
  }

  async function confirmDelete(id: number) {
    try {
      const { Haptics, ImpactStyle } = await import('@capacitor/haptics')
      await Haptics.impact({ style: ImpactStyle.Heavy })
    } catch { /* ignore */ }
    handleDelete(id)
  }

  const grouped = groupByDate(allItems)
  const activeFamilies = families.filter((f: Family) => !f.isArchived)
  const skeletonCount = useMemo(getSkeletonCount, [])

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
        <IonToolbar color="light">
          <IonSearchbar
            value={searchInput}
            onIonInput={e => handleSearchInput(e.detail.value ?? '')}
            placeholder={t('expenses.searchPlaceholder', 'Search description…')}
            debounce={0}
          />
        </IonToolbar>
      </IonHeader>

      <IonContent>
        <IonRefresher slot="fixed" onIonRefresh={handleRefresh} disabled={!isOnline}>
          <IonRefresherContent />
        </IonRefresher>

        {!isOnline && (
          <div style={{ padding: '8px 16px', background: 'var(--ion-color-warning-tint)' }}>
            <IonText color="dark">
              <p style={{ margin: 0, fontSize: 13 }}>
                {t('expenses.offlineBanner', "You're offline — showing cached data.")}
              </p>
            </IonText>
          </div>
        )}

        {isFetching && allItems.length === 0 && (
          <IonList>
            {Array.from({ length: skeletonCount }, (_, i) => i + 1).map(i => (
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
                <IonLabel>{dateGroupLabel(date, t)}</IonLabel>
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
          { text: t('common.delete', 'Delete'), role: 'destructive', handler: () => { if (deleteTarget) confirmDelete(deleteTarget) } },
        ]}
        onDidDismiss={() => setDeleteTarget(null)}
      />

      <IonToast
        isOpen={!!undoState}
        message={t('expenses.deleted', 'Expense deleted.')}
        duration={UNDO_WINDOW_MS}
        onDidDismiss={() => setUndoState(null)}
        buttons={[
          {
            text: t('common.undo', 'Undo'),
            handler: handleUndo,
          },
        ]}
      />
    </IonPage>
  )
}
