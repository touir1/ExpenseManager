import { useTranslation } from 'react-i18next'
import { PieChart, Pie, Cell, Tooltip, ResponsiveContainer } from 'recharts'
import type { CategoryBreakdownDto } from '@/features/dashboard/types/dashboard.type'

const COLORS = ['#C8623E', '#6B8E5A', '#D6A23F', '#5C8C9E', '#B5443F', '#E8B89A']

type DisplayCurrency = { symbol: string; decimals: number }

type Props = {
  data: CategoryBreakdownDto[]
  isLoading: boolean
  displayCurrency?: DisplayCurrency | null
}

function Skeleton() {
  return (
    <div
      className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6 animate-pulse"
      role="status"
      aria-label="Loading categories"
    >
      <div className="h-4 bg-surface-muted rounded w-28 mb-6" />
      <div className="flex gap-4">
        <div className="w-28 h-28 rounded-full bg-surface-subtle shrink-0" />
        <div className="flex-1 space-y-2 pt-2">
          {[0, 1, 2, 3].map(i => (
            <div key={i} className="flex items-center gap-2">
              <div className="w-2.5 h-2.5 rounded-full bg-surface-muted shrink-0" />
              <div className="h-3 bg-surface-subtle rounded flex-1" />
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}

export function CategoryDonut({ data, isLoading, displayCurrency }: Props) {
  const { t } = useTranslation()

  if (isLoading) return <Skeleton />

  return (
    <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6">
      <p className="text-xs font-semibold uppercase tracking-widest text-ink-mute mb-4">
        {t('dashboard.charts.categories')}
      </p>

      {data.length === 0 ? (
        <p className="text-sm text-ink-faint italic py-8 text-center">{t('dashboard.empty')}</p>
      ) : (
        <div className="flex gap-4 items-center">
          <div className="shrink-0 w-28 h-28">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={data}
                  dataKey={displayCurrency ? 'convertedTotal' : 'totalAmount'}
                  cx="50%"
                  cy="50%"
                  innerRadius="55%"
                  outerRadius="80%"
                  paddingAngle={2}
                >
                  {data.map((_, i) => (
                    <Cell key={i} fill={COLORS[i % COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip
                  formatter={(value: number) => [value.toFixed(2), '']}
                  contentStyle={{ borderRadius: '12px', border: '1px solid #e2e8f0', fontSize: 12 }}
                />
              </PieChart>
            </ResponsiveContainer>
          </div>

          <ul className="flex-1 space-y-1.5 min-w-0">
            {data.slice(0, 6).map((item, i) => (
              <li key={i} className="flex items-center gap-2 min-w-0">
                <span
                  className="w-2 h-2 rounded-full shrink-0"
                  style={{ backgroundColor: COLORS[i % COLORS.length] }}
                />
                <span className="text-xs text-ink-body truncate flex-1">
                  {item.category?.name ?? t('expenses.uncategorised')}
                </span>
                <span className="text-xs font-semibold text-ink shrink-0 tabular-nums">
                  {displayCurrency
                    ? `${displayCurrency.symbol} ${(item.convertedTotal ?? item.totalAmount).toFixed(displayCurrency.decimals)} (${item.percentage.toFixed(0)}%)`
                    : `${item.totalAmount.toFixed(2)} (${item.percentage.toFixed(0)}%)`}
                </span>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  )
}
