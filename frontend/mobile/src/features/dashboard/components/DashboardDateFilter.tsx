import { useState } from 'react'
import { IonSegment, IonSegmentButton, IonLabel, IonDatetimeButton, IonModal, IonDatetime, IonText } from '@ionic/react'
import { useTranslation } from 'react-i18next'

export type Period = 'month' | '6m' | 'year' | 'custom'

export type PeriodDates = {
  dateFrom: string
  dateTo: string
  period: Period
}

export function getPeriodDates(period: Period, customFrom?: string, customTo?: string): PeriodDates {
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
  if (period === 'year') {
    return { dateFrom: `${now.getFullYear()}-01-01`, dateTo, period }
  }
  return { dateFrom: customFrom ?? dateTo, dateTo: customTo ?? dateTo, period: 'custom' }
}

type Props = {
  value: Period
  onChange: (dates: PeriodDates) => void
}

export function DashboardDateFilter({ value, onChange }: Props) {
  const { t } = useTranslation()
  const defaults = getPeriodDates('month')
  const [customFrom, setCustomFrom] = useState<string>(defaults.dateFrom)
  const [customTo, setCustomTo] = useState<string>(defaults.dateTo)
  const [rangeError, setRangeError] = useState(false)

  function handleChange(period: Period) {
    if (period === 'custom') {
      onChange(getPeriodDates('custom', customFrom, customTo))
      return
    }
    setRangeError(false)
    onChange(getPeriodDates(period))
  }

  function handleCustomChange(from: string, to: string) {
    if (from > to) {
      setRangeError(true)
      return
    }
    setRangeError(false)
    setCustomFrom(from)
    setCustomTo(to)
    onChange({ dateFrom: from, dateTo: to, period: 'custom' })
  }

  return (
    <>
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
        <IonSegmentButton value="custom">
          <IonLabel style={{ fontSize: 12 }}>{t('dashboard.filters.custom', 'Custom')}</IonLabel>
        </IonSegmentButton>
      </IonSegment>

      {value === 'custom' && (
        <div style={{ display: 'flex', alignItems: 'center', gap: 8, padding: '8px 12px', flexWrap: 'wrap' }}>
          <IonDatetimeButton datetime="dashboard-date-from" />
          <IonModal keepContentsMounted>
            <IonDatetime
              id="dashboard-date-from"
              presentation="date"
              value={customFrom}
              onIonChange={e => {
                const v = e.detail.value
                const from = typeof v === 'string' ? v.substring(0, 10) : v?.[0]?.substring(0, 10) ?? customFrom
                handleCustomChange(from, customTo)
              }}
            />
          </IonModal>
          <span>–</span>
          <IonDatetimeButton datetime="dashboard-date-to" />
          <IonModal keepContentsMounted>
            <IonDatetime
              id="dashboard-date-to"
              presentation="date"
              value={customTo}
              onIonChange={e => {
                const v = e.detail.value
                const to = typeof v === 'string' ? v.substring(0, 10) : v?.[0]?.substring(0, 10) ?? customTo
                handleCustomChange(customFrom, to)
              }}
            />
          </IonModal>
          {rangeError && (
            <IonText color="danger" style={{ fontSize: 12, width: '100%' }}>
              <p style={{ margin: 0 }}>{t('dashboard.filters.invalidRange', 'From date must be before To date.')}</p>
            </IonText>
          )}
        </div>
      )}
    </>
  )
}
