import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import type { ExpenseDto } from '@/features/expenses/types/expenses.type'

type Props = {
  data: ExpenseDto[]
  isLoading: boolean
}

function Skeleton() {
  return (
    <div
      className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6 animate-pulse"
      role="status"
      aria-label="Loading recent expenses"
    >
      <div className="flex items-center justify-between mb-4">
        <div className="h-4 bg-surface-muted rounded w-32" />
        <div className="h-4 bg-surface-subtle rounded w-16" />
      </div>
      {[0, 1, 2, 3, 4].map(i => (
        <div key={i} className="flex items-center justify-between py-2.5 border-b border-surface-border last:border-0">
          <div className="flex items-center gap-3">
            <div className="h-3 bg-surface-muted rounded w-16" />
            <div className="h-3 bg-surface-subtle rounded w-32" />
          </div>
          <div className="h-3 bg-surface-subtle rounded w-20" />
        </div>
      ))}
    </div>
  )
}

function formatDate(dateStr: string): string {
  const [year, month, day] = dateStr.split('-')
  return `${day}/${month}/${String(year).slice(2)}`
}

function buildCategoryLabel(expense: ExpenseDto): string | null {
  const { category, subcategory } = expense
  if (!category && !subcategory) return null
  const icon = category?.icon ?? subcategory?.icon
  const parts: string[] = []
  if (icon) parts.push(icon)
  if (category) parts.push(category.name)
  if (subcategory) parts.push(subcategory.name)
  if (category && subcategory) {
    return `${icon ? icon + ' ' : ''}${category.name} / ${subcategory.name}`
  }
  return parts.join(' ')
}

export function RecentExpenses({ data, isLoading }: Props) {
  const { t } = useTranslation()

  if (isLoading) return <Skeleton />

  return (
    <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6">
      <div className="flex items-center justify-between mb-3">
        <p className="text-xs font-semibold uppercase tracking-widest text-ink-mute">
          {t('dashboard.recent.title')}
        </p>
        <Link
          to="/expenses"
          className="text-xs font-medium text-brand-600 hover:text-brand-700 transition-colors duration-150"
        >
          {t('dashboard.recent.viewAll')} →
        </Link>
      </div>

      {data.length === 0 ? (
        <p className="text-sm text-ink-faint italic py-4 text-center">{t('dashboard.recent.empty')}</p>
      ) : (
        <ul>
          {data.map(expense => {
            const showConverted = expense.convertedAmount != null && expense.currency != null
            const displayAmount = expense.convertedAmount ?? expense.amount
            const displayCurrency = expense.displayCurrency ?? expense.currency
            const symbol = displayCurrency?.symbol ?? ''
            const decimals = displayCurrency?.decimals ?? 2
            const categoryLabel = buildCategoryLabel(expense)

            const originalSymbol = expense.currency?.symbol ?? ''
            const originalDecimals = expense.currency?.decimals ?? 2

            return (
              <li
                key={expense.id}
                className="flex items-center justify-between py-2.5 border-b border-surface-border last:border-0 gap-3"
              >
                <div className="flex items-center gap-2.5 min-w-0">
                  <span className="text-[11px] text-ink-faint shrink-0 tabular-nums w-14">
                    {formatDate(expense.date)}
                  </span>
                  <div className="min-w-0">
                    <p className="text-sm text-ink-body truncate leading-tight">
                      {expense.description ?? t('expenses.uncategorised')}
                    </p>
                    {categoryLabel && (
                      <span className="inline-block text-[11px] px-1.5 py-0.5 rounded bg-surface-subtle text-ink-mute mt-0.5">
                        {categoryLabel}
                      </span>
                    )}
                  </div>
                </div>
                <div className="text-right shrink-0">
                  <span className="text-sm font-semibold text-ink tabular-nums">
                    {symbol}{displayAmount.toFixed(decimals)}
                  </span>
                  {showConverted && (
                    <p className="text-[11px] text-ink-faint tabular-nums">
                      ≈ {originalSymbol}{expense.amount.toFixed(originalDecimals)}
                    </p>
                  )}
                </div>
              </li>
            )
          })}
        </ul>
      )}
    </div>
  )
}
