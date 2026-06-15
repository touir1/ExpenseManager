import { useTranslation } from 'react-i18next'
import type { CurrencyBreakdownDto, Currency } from '@/features/dashboard/types/dashboard.type'

type Props = {
  data: CurrencyBreakdownDto[]
  isLoading: boolean
  displayCurrency?: Currency | null
}

function Skeleton() {
  return (
    <div
      className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6 animate-pulse"
      role="status"
      aria-label="Loading currencies"
    >
      <div className="h-4 bg-surface-muted rounded w-24 mb-4" />
      {[0, 1, 2].map(i => (
        <div key={i} className="flex items-center justify-between py-2.5 border-b border-surface-border last:border-0">
          <div className="flex items-center gap-2">
            <div className="w-8 h-8 rounded-full bg-surface-subtle" />
            <div className="h-3 bg-surface-muted rounded w-10" />
          </div>
          <div className="h-3 bg-surface-subtle rounded w-20" />
        </div>
      ))}
    </div>
  )
}

export function CurrenciesPanel({ data, isLoading, displayCurrency }: Props) {
  const { t } = useTranslation()

  if (isLoading) return <Skeleton />

  const maxAmount = data.length > 0 ? Math.max(...data.map(d => d.totalAmount)) : 0

  return (
    <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6">
      <p className="text-xs font-semibold uppercase tracking-widest text-ink-mute mb-2">
        {t('dashboard.charts.currencies')}
      </p>

      {data.length === 0 ? (
        <p className="text-sm text-ink-faint italic py-4 text-center">{t('dashboard.noCurrencies')}</p>
      ) : (
        <ul>
          {data.map(item => {
            const pct = maxAmount > 0 ? Math.max((item.totalAmount / maxAmount) * 100, 3) : 3
            return (
              <li
                key={item.currency.id}
                className="py-2.5 border-b border-surface-border last:border-0"
              >
                <div className="flex items-center justify-between mb-1.5">
                  <div className="flex items-center gap-2.5">
                    <span className="flex h-8 w-8 items-center justify-center rounded-full bg-surface-subtle text-xs font-bold text-ink-mute">
                      {item.currency.symbol}
                    </span>
                    <div>
                      <p className="text-sm font-semibold text-ink leading-none">{item.currency.code}</p>
                      <p className="text-[11px] text-ink-faint mt-0.5">
                        {item.expenseCount} {t('dashboard.summary.expenses')}
                      </p>
                    </div>
                  </div>

                  <div className="text-right">
                    <p className="text-sm font-semibold text-ink tabular-nums">
                      {item.currency.symbol} {item.totalAmount.toFixed(item.currency.decimals)}
                    </p>
                    {item.convertedAmount != null && (
                      <p className="text-[11px] text-ink-faint tabular-nums">
                        {displayCurrency
                          ? `≈ ${displayCurrency.symbol} ${item.convertedAmount.toFixed(displayCurrency.decimals)}`
                          : `≈ ${item.convertedAmount.toFixed(2)}`}
                      </p>
                    )}
                  </div>
                </div>

                <div className="ml-10 h-1.5 bg-surface-subtle rounded-full overflow-hidden">
                  <div
                    className="h-full bg-brand-400 rounded-full transition-all duration-500"
                    style={{ width: `${pct}%` }}
                    aria-hidden="true"
                  />
                </div>
              </li>
            )
          })}
        </ul>
      )}
    </div>
  )
}
