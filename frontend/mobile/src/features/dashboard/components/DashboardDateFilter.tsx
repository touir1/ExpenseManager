import { IonSegment, IonSegmentButton, IonLabel } from '@ionic/react'
import { useTranslation } from 'react-i18next'

export type Period = 'month' | '6m' | 'year'

export type PeriodDates = {
  dateFrom: string
  dateTo: string
  period: Period
}

export function getPeriodDates(period: Period): PeriodDates {
  const now = new Date()
  const dateTo = now.toISOString().substring(0, 10)
  if (period === 'month') {
    const dateFrom = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-01`
    return { dateFrom, dateTo, period }
  }
  if (period === '6m') {
    const from = new Date(now)
    from.setMonth(from.getMonth() - 6)
    return { dateFrom: from.toISOString().substring(0, 10), dateTo, period }
  }
  return { dateFrom: `${now.getFullYear()}-01-01`, dateTo, period }
}

type Props = {
  value: Period
  onChange: (dates: PeriodDates) => void
}

export function DashboardDateFilter({ value, onChange }: Props) {
  const { t } = useTranslation()

  function handleChange(period: Period) {
    onChange(getPeriodDates(period))
  }

  return (
    <IonSegment
      value={value}
      onIonChange={e => handleChange(e.detail.value as Period)}
      style={{ margin: '8px 12px 0' }}
    >
      <IonSegmentButton value="month">
        <IonLabel style={{ fontSize: 12 }}>{t('dashboard.filters.thisMonth')}</IonLabel>
      </IonSegmentButton>
      <IonSegmentButton value="6m">
        <IonLabel style={{ fontSize: 12 }}>{t('dashboard.filters.sixMonths')}</IonLabel>
      </IonSegmentButton>
      <IonSegmentButton value="year">
        <IonLabel style={{ fontSize: 12 }}>{t('dashboard.filters.thisYear')}</IonLabel>
      </IonSegmentButton>
    </IonSegment>
  )
}
