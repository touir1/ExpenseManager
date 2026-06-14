import { useTranslation } from 'react-i18next'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts'
import { IonCard, IonCardHeader, IonCardTitle, IonCardContent, IonSkeletonText, IonText } from '@ionic/react'
import type { SameMonthYearlyDto, Currency } from '@/features/dashboard/types/dashboard.type'

type Props = {
  data: SameMonthYearlyDto[]
  isLoading: boolean
  displayCurrency?: Currency | null
}

export function SameMonthChart({ data, isLoading, displayCurrency }: Props) {
  const { t } = useTranslation()

  const chartData = data.map(d => ({
    label: String(d.year),
    amount: displayCurrency ? (d.convertedTotal ?? d.totalAmount) : d.totalAmount,
  }))

  const sym = displayCurrency?.symbol ?? ''

  return (
    <IonCard>
      <IonCardHeader>
        <IonCardTitle style={{ fontSize: 16 }}>
          {t('dashboard.charts.sameMonth', { month: new Date().toLocaleString('default', { month: 'long' }) })}
        </IonCardTitle>
      </IonCardHeader>
      <IonCardContent>
        {isLoading ? (
          <IonSkeletonText animated style={{ height: 140 }} />
        ) : chartData.length === 0 ? (
          <IonText color="medium"><p style={{ textAlign: 'center', padding: '16px 0', fontSize: 13 }}>{t('dashboard.empty')}</p></IonText>
        ) : (
          <ResponsiveContainer width="100%" height={160}>
            <BarChart data={chartData} margin={{ top: 4, right: 4, bottom: 0, left: -16 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--chart-grid)" vertical={false} />
              <XAxis dataKey="label" tick={{ fontSize: 11 }} axisLine={false} tickLine={false} />
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
              <Bar dataKey="amount" fill="#5C8C9E" radius={[4, 4, 0, 0]} maxBarSize={40} />
            </BarChart>
          </ResponsiveContainer>
        )}
      </IonCardContent>
    </IonCard>
  )
}
