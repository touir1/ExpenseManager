import { useTranslation } from 'react-i18next'
import { PieChart, Pie, Cell, Tooltip, ResponsiveContainer } from 'recharts'
import type { CategoryBreakdownDto } from '@/features/dashboard/types/dashboard.type'
import { getCategoryColor } from '@/features/dashboard/utils/categoryColors'
import { ChartDataTable } from '@/features/dashboard/components/ChartDataTable'
import { useChartColors } from '@/features/dashboard/utils/chartTheme'
import EmptyState from '@/components/EmptyState'

type DisplayCurrency = { symbol: string; decimals: number }

type Props = {
  data: CategoryBreakdownDto[]
  isLoading: boolean
  displayCurrency?: DisplayCurrency | null
  onCategoryClick?: (categoryId: number | null) => void
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

export function CategoryDonut({ data, isLoading, displayCurrency, onCategoryClick }: Props) {
  const { t } = useTranslation()
  const clickable = !!onCategoryClick
  const chartColors = useChartColors()

  if (isLoading) return <Skeleton />

  return (
    <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6">
      <p className="text-xs font-semibold uppercase tracking-widest text-ink-mute mb-4">
        {t('dashboard.charts.categories')}
        {clickable && (
          <span className="ml-2 font-normal normal-case tracking-normal text-ink-faint">
            — {t('dashboard.charts.drillDown')}
          </span>
        )}
      </p>

      {data.length === 0 ? (
        <EmptyState compact icon="🥧" title={t('dashboard.empty')} />
      ) : (
        <>
        <ChartDataTable
          caption={t('dashboard.charts.categories')}
          columns={[
            { key: 'category', header: t('dashboard.charts.category') },
            { key: 'amount', header: t('dashboard.charts.amount') },
            { key: 'percentage', header: t('dashboard.charts.percentage') },
          ]}
          rows={data.map(item => ({
            category: item.category?.name ?? t('expenses.uncategorised'),
            amount: displayCurrency
              ? `${displayCurrency.symbol} ${(item.convertedTotal ?? item.totalAmount).toFixed(displayCurrency.decimals)}`
              : item.totalAmount.toFixed(2),
            percentage: `${item.percentage.toFixed(0)}%`,
          }))}
        />
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
                  onClick={clickable ? (_, index) => onCategoryClick!(data[index]?.category?.id ?? null) : undefined}
                  className={clickable ? 'cursor-pointer' : undefined}
                >
                  {data.map((item, i) => (
                    <Cell key={i} fill={getCategoryColor(item.category?.id).text} />
                  ))}
                </Pie>
                <Tooltip
                  formatter={(value) => [(value as number).toFixed(2), '']}
                  contentStyle={{
                    borderRadius: '12px',
                    border: `1px solid ${chartColors.tooltipBorder}`,
                    backgroundColor: chartColors.tooltipBg,
                    fontSize: 12,
                  }}
                />
              </PieChart>
            </ResponsiveContainer>
          </div>

          <ul className="flex-1 space-y-1.5 min-w-0">
            {data.slice(0, 6).map((item, i) => (
              <li
                key={i}
                className={`flex items-center gap-2 min-w-0 rounded-lg transition-colors ${
                  clickable ? 'cursor-pointer hover:bg-surface-subtle px-1.5 -mx-1.5 py-0.5' : ''
                }`}
                onClick={clickable ? () => onCategoryClick!(item.category?.id ?? null) : undefined}
                title={clickable ? t('dashboard.charts.drillDown') : undefined}
              >
                <span
                  className="w-2 h-2 rounded-full shrink-0"
                  style={{ backgroundColor: getCategoryColor(item.category?.id).text }}
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
        </>
      )}
    </div>
  )
}
