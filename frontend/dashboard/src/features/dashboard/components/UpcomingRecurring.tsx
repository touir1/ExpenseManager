import { useTranslation } from 'react-i18next'
import type { RecurringExpenseDto } from '@/features/dashboard/types/dashboard.type'
import EmptyState from '@/components/EmptyState'

type Props = {
  data: RecurringExpenseDto[]
  isLoading: boolean
}

function Skeleton() {
  return (
    <div
      className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6 animate-pulse"
      role="status"
      aria-label="Loading upcoming recurring payments"
    >
      <div className="h-4 bg-surface-muted rounded w-32 mb-4" />
      {[0, 1, 2].map(i => (
        <div key={i} className="flex items-center justify-between py-2.5 border-b border-surface-border last:border-0">
          <div className="flex items-center gap-3">
            <div className="h-3 bg-surface-muted rounded w-24" />
            <div className="h-3 bg-surface-subtle rounded w-16" />
          </div>
          <div className="h-3 bg-surface-subtle rounded w-14" />
        </div>
      ))}
    </div>
  )
}

function daysUntil(dateStr: string): number {
  const [year, month, day] = dateStr.split('-').map(Number)
  const due = new Date(year, month - 1, day)
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  due.setHours(0, 0, 0, 0)
  return Math.round((due.getTime() - today.getTime()) / (1000 * 60 * 60 * 24))
}

export function UpcomingRecurring({ data, isLoading }: Props) {
  const { t } = useTranslation()

  if (isLoading) return <Skeleton />

  return (
    <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6">
      <p className="text-xs font-semibold uppercase tracking-widest text-ink-mute mb-3">
        {t('dashboard.recurring.title')}
      </p>

      {data.length === 0 ? (
        <EmptyState compact icon="🔁" title={t('dashboard.recurring.empty')} />
      ) : (
        <ul>
          {data.map(item => {
            const days = daysUntil(item.nextDueDate)
            const dueLabel =
              days <= 0
                ? t('dashboard.recurring.dueToday')
                : days === 1
                  ? t('dashboard.recurring.dueTomorrow')
                  : t('dashboard.recurring.dueInDays', { count: days })
            const symbol = item.currency?.symbol ?? ''
            const decimals = item.currency?.decimals ?? 2
            const categoryLabel = item.category?.name ?? null

            return (
              <li
                key={item.id}
                className="flex items-center justify-between py-2.5 border-b border-surface-border last:border-0 gap-3"
              >
                <div className="min-w-0">
                  <p className="text-sm text-ink-body truncate leading-tight">{item.description}</p>
                  <div className="flex items-center gap-1.5 mt-0.5">
                    {categoryLabel && (
                      <span className="inline-block text-[11px] px-2 py-0.5 rounded-full font-medium bg-surface-subtle text-ink-mute">
                        {categoryLabel}
                      </span>
                    )}
                    <span className="text-[11px] text-ink-faint">{dueLabel}</span>
                  </div>
                </div>
                <span className="text-sm font-semibold text-ink tabular-nums shrink-0">
                  {symbol} {item.amount.toFixed(decimals)}
                </span>
              </li>
            )
          })}
        </ul>
      )}
    </div>
  )
}
