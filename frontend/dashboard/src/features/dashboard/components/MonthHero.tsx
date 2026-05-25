import { useTranslation } from 'react-i18next'
import type { DashboardSummaryDto } from '@/features/dashboard/types/dashboard.type'

type Props = {
  data: DashboardSummaryDto | undefined
  isLoading: boolean
}

function Skeleton() {
  return (
    <div
      className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6 animate-pulse"
      role="status"
      aria-label="Loading summary"
    >
      <div className="h-4 bg-surface-muted rounded w-24 mb-4" />
      <div className="h-10 bg-surface-subtle rounded w-40 mb-3" />
      <div className="h-4 bg-surface-muted rounded w-32 mb-4" />
      <div className="flex gap-3">
        <div className="h-6 bg-surface-subtle rounded-full w-20" />
        <div className="h-6 bg-surface-subtle rounded-full w-24" />
      </div>
    </div>
  )
}

function formatAmount(amount: number, decimals = 2): string {
  return amount.toLocaleString(undefined, { minimumFractionDigits: decimals, maximumFractionDigits: decimals })
}

export function MonthHero({ data, isLoading }: Props) {
  const { t } = useTranslation()

  if (isLoading || !data) return <Skeleton />

  const showConverted = data.convertedTotal != null && data.displayCurrency != null
  const mainAmount = showConverted ? data.convertedTotal! : data.totalAmount
  const mainCurrency = showConverted ? data.displayCurrency! : null
  const decimals = mainCurrency?.decimals ?? 2

  const deltaPositive = (data.changePercent ?? 0) >= 0
  const deltaClass = deltaPositive
    ? 'bg-green-50 text-green-700'
    : 'bg-red-50 text-red-700'
  const deltaSign = deltaPositive ? '+' : ''

  return (
    <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6">
      <p className="text-xs font-semibold uppercase tracking-widest text-ink-mute mb-3">
        {t('dashboard.summary.total')}
      </p>

      <div className="flex items-baseline gap-2 mb-1">
        {mainCurrency && (
          <span className="text-2xl font-bold text-ink-mute">{mainCurrency.symbol}</span>
        )}
        <span className="text-4xl font-bold text-ink tracking-tight">
          {formatAmount(mainAmount, decimals)}
        </span>
      </div>

      {showConverted && (
        <p className="text-xs text-ink-faint mb-3">
          {data.displayCurrency!.code} · {t('dashboard.summary.converted')}
        </p>
      )}

      <p className="text-sm text-ink-mute mb-4">
        {data.expenseCount} {t('dashboard.summary.expenses')}
      </p>

      <div className="flex flex-wrap gap-2">
        {data.changePercent != null && (
          <span className={`inline-flex items-center gap-1 text-xs font-semibold px-2.5 py-1 rounded-full ${deltaClass}`}>
            {deltaSign}{data.changePercent.toFixed(1)}%
            <span className="font-normal text-[11px] opacity-75">{t('dashboard.summary.vs')}</span>
          </span>
        )}

        {data.topCategory && (
          <span className="inline-flex items-center gap-1 text-xs font-medium px-2.5 py-1 rounded-full bg-brand-50 text-brand-700">
            <span className="font-normal opacity-75">{t('dashboard.summary.topCategory')}</span>
            {data.topCategory.icon && <span>{data.topCategory.icon}</span>}
            {data.topCategory.name}
          </span>
        )}
      </div>
    </div>
  )
}
