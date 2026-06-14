import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import {
  IonPage,
  IonHeader,
  IonToolbar,
  IonTitle,
  IonContent,
  IonBackButton,
  IonButtons,
  IonText,
  IonButton,
  IonSpinner,
} from '@ionic/react'
import { useTranslation } from 'react-i18next'
import { acceptInvite } from '@/features/families/services/familyApi.service'

type Status = 'loading' | 'success' | 'error'

export default function AcceptInvitePage() {
  const { t } = useTranslation()
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')
  const [status, setStatus] = useState<Status>('loading')
  const [errorMessage, setErrorMessage] = useState<string>('')

  useEffect(() => {
    if (!token) {
      setErrorMessage(t('families.acceptInvite.error'))
      setStatus('error')
      return
    }
    acceptInvite(token, { silent: true }).then(res => {
      if (res.ok) {
        setStatus('success')
      } else {
        setErrorMessage(res.error ?? t('families.acceptInvite.error'))
        setStatus('error')
      }
    })
  }, [token, t])

  return (
    <IonPage>
      <IonHeader>
        <IonToolbar>
          <IonButtons slot="start">
            <IonBackButton defaultHref="/families" />
          </IonButtons>
          <IonTitle>{t('families.acceptInvite.pageTitle')}</IonTitle>
        </IonToolbar>
      </IonHeader>

      <IonContent className="ion-padding">
        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', paddingTop: 64, gap: 16 }}>
          {status === 'loading' && (
            <>
              <IonSpinner name="crescent" color="primary" style={{ width: 48, height: 48 }} />
              <IonText color="medium">
                <p style={{ textAlign: 'center' }}>{t('families.acceptInvite.loading')}</p>
              </IonText>
            </>
          )}

          {status === 'success' && (
            <>
              <IonText color="success">
                <h2 style={{ textAlign: 'center', margin: 0 }}>{t('families.acceptInvite.success')}</h2>
              </IonText>
              <IonButton routerLink="/families" routerDirection="back" style={{ marginTop: 8 }}>
                {t('families.acceptInvite.goToFamilies')}
              </IonButton>
            </>
          )}

          {status === 'error' && (
            <>
              <IonText color="danger">
                <p style={{ textAlign: 'center' }}>{errorMessage}</p>
              </IonText>
              <IonButton routerLink="/families" routerDirection="back" fill="outline" style={{ marginTop: 8 }}>
                {t('families.acceptInvite.goToFamilies')}
              </IonButton>
            </>
          )}
        </div>
      </IonContent>
    </IonPage>
  )
}
