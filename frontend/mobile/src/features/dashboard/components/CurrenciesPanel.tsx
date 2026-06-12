import { useTranslation } from 'react-i18next'
import {
  IonCard, IonCardHeader, IonCardTitle,
  IonList, IonItem, IonLabel, IonText,
  IonSkeletonText,
} from '@ionic/react'
import type { CurrencyBreakdownDto, Currency } from '@/features/dashboard/types/dashboard.type'

type Props = {
  data: CurrencyBreakdownDto[]
  isLoading: boolean
  displayCurrency?: Currency | null
}

export function CurrenciesPanel({ data, isLoading, displayCurrency }: Props) {
  const { t } = useTranslation()

  return (
    <IonCard>
      <IonCardHeader>
        <IonCardTitle style={{ fontSize: 16 }}>{t('dashboard.charts.currencies')}</IonCardTitle>
      </IonCardHeader>
      {isLoading ? (
        <div style={{ padding: '0 16px 16px' }}>
          {[1, 2, 3].map(i => <IonSkeletonText key={i} animated style={{ height: 16, marginBottom: 10 }} />)}
        </div>
      ) : data.length === 0 ? (
        <div style={{ padding: '0 16px 16px' }}>
          <IonText color="medium"><p style={{ fontSize: 13 }}>{t('dashboard.noCurrencies')}</p></IonText>
        </div>
      ) : (
        <IonList>
          {data.map(row => {
            const amount = displayCurrency ? (row.convertedAmount ?? row.totalAmount) : row.totalAmount
            const sym = displayCurrency ? (displayCurrency.symbol ?? row.currency.symbol) : row.currency.symbol
            const decimals = displayCurrency ? (displayCurrency.decimals ?? 2) : 2
            return (
              <IonItem key={row.currency.id}>
                <IonLabel>
                  <h3 style={{ fontWeight: 600 }}>{row.currency.code}</h3>
                  <p style={{ fontSize: 12 }}>{row.expenseCount} {t('dashboard.summary.expenses')}</p>
                </IonLabel>
                <IonText slot="end" color="dark">
                  <span style={{ fontWeight: 600, fontSize: 14 }}>
                    {sym} {amount.toFixed(decimals)}
                  </span>
                </IonText>
              </IonItem>
            )
          })}
        </IonList>
      )}
    </IonCard>
  )
}
