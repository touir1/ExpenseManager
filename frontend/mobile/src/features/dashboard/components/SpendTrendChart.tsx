import { useTranslation } from 'react-i18next'
import {
  AreaChart,
  Area,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  CartesianGrid,
} from 'recharts'
import { IonCard, IonCardHeader, IonCardTitle, IonCardContent, IonSkeletonText, IonText } from '@ionic/react'
import type { MonthlyBreakdownDto, Currency } from '@/features/dashboard/types/dashboard.type'

const MONTH_ABBR = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

type Props = {
  data: MonthlyBreakdownDto[]
  isLoading: boolean
  displayCurrency?: Currency | null
}

export function SpendTrendChart({ data, isLoading, displayCurrency }: Props) {
  const { t } = useTranslation()

  const chartData = data.map(d => ({
    label: MONTH_ABBR[d.month - 1],
    amount: displayCurrency ? (d.convertedTotal ?? d.totalAmount) : d.totalAmount,
  }))

  const sym = displayCurrency?.symbol ?? ''

  return (
    <IonCard>
      <IonCardHeader>
        <IonCardTitle style={{ fontSize: 16 }}>{t('dashboard.charts.monthly')}</IonCardTitle>
      </IonCardHeader>
      <IonCardContent>
        {isLoading ? (
          <IonSkeletonText animated style={{ height: 140 }} />
        ) : chartData.length === 0 ? (
          <IonText color="medium"><p style={{ textAlign: 'center', padding: '16px 0', fontSize: 13 }}>{t('dashboard.empty')}</p></IonText>
        ) : (
          <ResponsiveContainer width="100%" height={160}>
            <AreaChart data={chartData} margin={{ top: 4, right: 4, bottom: 0, left: -16 }}>
              <defs>
                <linearGradient id="spendGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#C8623E" stopOpacity={0.3} />
                  <stop offset="95%" stopColor="#C8623E" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--chart-grid)" vertical={false} />
              <XAxis dataKey="label" tick={{ fontSize: 10 }} axisLine={false} tickLine={false} />
              <YAxis
                tick={{ fontSize: 10 }}
                axisLine={false}
                tickLine={false}
                tickFormatter={v => v >= 1000 ? `${(v / 1000).toFixed(0)}k` : String(v)}
              />
              <Tooltip
                formatter={(v) => [`${sym}${sym ? ' ' : ''}${(v as number).toFixed(2)}`, '']}
                contentStyle={{
                  borderRadius: 8,
                  fontSize: 12,
                  backgroundColor: 'var(--chart-tooltip-bg)',
                  borderColor: 'var(--chart-tooltip-border)',
                  color: 'var(--chart-tooltip-text)',
                }}
              />
              <Area
                type="monotone"
                dataKey="amount"
                stroke="#C8623E"
                strokeWidth={2}
                fill="url(#spendGrad)"
                dot={false}
              />
            </AreaChart>
          </ResponsiveContainer>
        )}
      </IonCardContent>
    </IonCard>
  )
}
