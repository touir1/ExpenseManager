import { useTranslation } from 'react-i18next'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts'
import type { SameMonthYearlyDto, Currency } from '@/features/dashboard/types/dashboard.type'

const MONTH_NAMES = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December',
]

type Props = {
  data: SameMonthYearlyDto[]
  isLoading: boolean
  selectedMonth: number
  displayCurrency?: Currency | null
}

function Skeleton() {
  return (
    <div
      className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6 animate-pulse"
      role="status"
      aria-label="Loading year comparison"
    >
      <div className="h-4 bg-surface-muted rounded w-40 mb-6" />
      <div className="flex items-end gap-3 h-20">
        {[65, 80, 55, 90].map((h, i) => (
          <div key={i} className="flex-1 bg-surface-subtle rounded-t" style={{ height: `${h}%` }} />
        ))}
      </div>
    </div>
  )
}

export function SameMonthChart({ data, isLoading, selectedMonth, displayCurrency }: Props) {
  const { t } = useTranslation()

  if (isLoading) return <Skeleton />

  const monthName = MONTH_NAMES[(selectedMonth - 1) % 12]
  const currSymbol = displayCurrency?.symbol ?? ''

  const chartData = data.map(d => ({
    year: String(d.year),
    amount: displayCurrency ? (d.convertedTotal ?? d.totalAmount) : d.totalAmount,
  }))

  return (
    <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6">
      <p className="text-xs font-semibold uppercase tracking-widest text-ink-mute mb-4">
        {t('dashboard.charts.sameMonth', { month: monthName })}
      </p>

      {chartData.length === 0 ? (
        <p className="text-sm text-ink-faint italic py-6 text-center">{t('dashboard.noData')}</p>
      ) : (
        <ResponsiveContainer width="100%" height={120}>
          <BarChart data={chartData} margin={{ top: 4, right: 4, bottom: 0, left: 0 }}>
            <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" vertical={false} />
            <XAxis dataKey="year" tick={{ fontSize: 11, fill: '#94a3b8' }} axisLine={false} tickLine={false} />
            <YAxis
              tick={{ fontSize: 11, fill: '#94a3b8' }}
              axisLine={false}
              tickLine={false}
              tickFormatter={v => `${currSymbol}${v >= 1000 ? `${(v / 1000).toFixed(0)}k` : v}`}
              width={44}
            />
            <Tooltip
              formatter={(value: number) => [`${currSymbol}${value.toFixed(2)}`, '']}
              contentStyle={{ borderRadius: '12px', border: '1px solid #e2e8f0', fontSize: 12 }}
            />
            <Bar dataKey="amount" fill="#c8623e" radius={[4, 4, 0, 0]} maxBarSize={40} />
          </BarChart>
        </ResponsiveContainer>
      )}
    </div>
  )
}
