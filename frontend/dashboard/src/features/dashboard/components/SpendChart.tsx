import { useTranslation } from 'react-i18next'
import {
  ComposedChart,
  Bar,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  CartesianGrid,
} from 'recharts'
import type { MonthlyBreakdownDto, Currency } from '@/features/dashboard/types/dashboard.type'

const MONTH_NAMES = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']
const BAR_COLOR = '#c8623e'
const LINE_COLOR = '#94a3b8'

type Props = {
  data: MonthlyBreakdownDto[]
  isLoading: boolean
  displayCurrency?: Currency | null
}

function Skeleton() {
  return (
    <div
      className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6 animate-pulse"
      role="status"
      aria-label="Loading chart"
    >
      <div className="h-4 bg-surface-muted rounded w-32 mb-6" />
      <div className="flex items-end gap-1 h-32">
        {[60, 80, 50, 90, 70, 85, 65, 75, 55, 88, 72, 60].map((h, i) => (
          <div key={i} className="flex-1 bg-surface-subtle rounded-t" style={{ height: `${h}%` }} />
        ))}
      </div>
    </div>
  )
}

export function SpendChart({ data, isLoading, displayCurrency }: Props) {
  const { t } = useTranslation()

  if (isLoading) return <Skeleton />

  const chartData = data.map(d => ({
    label: `${MONTH_NAMES[d.month - 1]} ${String(d.year).slice(2)}`,
    amount: displayCurrency ? (d.convertedTotal ?? d.totalAmount) : d.totalAmount,
  }))

  const avg = chartData.length > 0
    ? chartData.reduce((s, d) => s + d.amount, 0) / chartData.length
    : 0

  const chartDataWithAvg = chartData.map(d => ({ ...d, avg }))

  const currSymbol = displayCurrency?.symbol ?? ''

  return (
    <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6">
      <p className="text-xs font-semibold uppercase tracking-widest text-ink-mute mb-4">
        {t('dashboard.charts.monthly')}
      </p>

      {chartData.length === 0 ? (
        <p className="text-sm text-ink-faint italic py-8 text-center">{t('dashboard.empty')}</p>
      ) : (
        <ResponsiveContainer width="100%" height={180}>
          <ComposedChart data={chartDataWithAvg} margin={{ top: 4, right: 4, bottom: 0, left: 0 }}>
            <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" vertical={false} />
            <XAxis
              dataKey="label"
              tick={{ fontSize: 11, fill: '#94a3b8' }}
              axisLine={false}
              tickLine={false}
            />
            <YAxis
              tick={{ fontSize: 11, fill: '#94a3b8' }}
              axisLine={false}
              tickLine={false}
              tickFormatter={v => `${currSymbol}${currSymbol ? ' ' : ''}${v >= 1000 ? `${(v / 1000).toFixed(0)}k` : v}`}
              width={48}
            />
            <Tooltip
              formatter={(value) => {
                const n = value as number
                return [n === 0 ? t('dashboard.charts.noExpenses') : `${currSymbol}${currSymbol ? ' ' : ''}${n.toFixed(2)}`, '']
              }}
              contentStyle={{ borderRadius: '12px', border: '1px solid #e2e8f0', fontSize: 12 }}
            />
            <Bar dataKey="amount" fill={BAR_COLOR} radius={[4, 4, 0, 0]} maxBarSize={32} minPointSize={2} />
            <Line
              type="monotone"
              dataKey="avg"
              stroke={LINE_COLOR}
              strokeWidth={1.5}
              dot={false}
              strokeDasharray="4 3"
            />
          </ComposedChart>
        </ResponsiveContainer>
      )}
    </div>
  )
}
