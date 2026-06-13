import { useTranslation } from 'react-i18next'
import { PieChart, Pie, Cell, Tooltip, ResponsiveContainer } from 'recharts'
import { IonCard, IonCardHeader, IonCardTitle, IonCardContent, IonSkeletonText, IonText } from '@ionic/react'
import type { CategoryBreakdownDto, Currency } from '@/features/dashboard/types/dashboard.type'

const PALETTE = [
  '#C8623E', '#6B8E5A', '#8B6720', '#5C8C9E', '#B5443F', '#7A5C9E',
]

function getColor(index: number): string {
  return PALETTE[index % PALETTE.length]
}

type Props = {
  data: CategoryBreakdownDto[]
  isLoading: boolean
  displayCurrency?: Currency | null
}

export function CategoryPieChart({ data, isLoading, displayCurrency }: Props) {
  const { t } = useTranslation()

  const top6 = data.slice(0, 6)
  const rest = data.slice(6)
  const otherTotal = rest.reduce((s, d) => s + (displayCurrency ? (d.convertedTotal ?? d.totalAmount) : d.totalAmount), 0)
  const otherPct = rest.reduce((s, d) => s + d.percentage, 0)

  const chartData = [
    ...top6.map(d => ({
      name: d.category?.name ?? t('expenses.uncategorised'),
      value: displayCurrency ? (d.convertedTotal ?? d.totalAmount) : d.totalAmount,
      percentage: d.percentage,
    })),
    ...(rest.length > 0 ? [{ name: t('dashboard.charts.other', 'Other'), value: otherTotal, percentage: otherPct }] : []),
  ]

  return (
    <IonCard>
      <IonCardHeader>
        <IonCardTitle style={{ fontSize: 16 }}>{t('dashboard.charts.categories')}</IonCardTitle>
      </IonCardHeader>
      <IonCardContent>
        {isLoading ? (
          <IonSkeletonText animated style={{ height: 140 }} />
        ) : chartData.length === 0 ? (
          <IonText color="medium"><p style={{ textAlign: 'center', padding: '16px 0', fontSize: 13 }}>{t('dashboard.empty')}</p></IonText>
        ) : (
          <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
            <div style={{ flexShrink: 0, width: 120, height: 120 }}>
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={chartData}
                    dataKey="value"
                    cx="50%"
                    cy="50%"
                    innerRadius="50%"
                    outerRadius="80%"
                    paddingAngle={2}
                  >
                    {chartData.map((_, i) => (
                      <Cell key={i} fill={getColor(i)} />
                    ))}
                  </Pie>
                  <Tooltip
                    formatter={(v) => [(v as number).toFixed(2), '']}
                    contentStyle={{ borderRadius: 8, fontSize: 11 }}
                  />
                </PieChart>
              </ResponsiveContainer>
            </div>
            <ul style={{ flex: 1, listStyle: 'none', padding: 0, margin: 0 }}>
              {chartData.map((item, i) => (
                <li key={i} style={{ display: 'flex', alignItems: 'center', gap: 6, marginBottom: 4 }}>
                  <span style={{ width: 8, height: 8, borderRadius: '50%', backgroundColor: getColor(i), flexShrink: 0 }} />
                  <span style={{ fontSize: 11, flex: 1, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                    {item.name}
                  </span>
                  <span style={{ fontSize: 11, fontWeight: 600, flexShrink: 0 }}>
                    {item.percentage.toFixed(0)}%
                  </span>
                </li>
              ))}
            </ul>
          </div>
        )}
      </IonCardContent>
    </IonCard>
  )
}
