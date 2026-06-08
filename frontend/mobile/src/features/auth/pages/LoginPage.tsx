import { useState, useEffect } from 'react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate } from 'react-router-dom'
import {
  IonPage,
  IonContent,
  IonInput,
  IonButton,
  IonText,
  IonSpinner,
  IonCheckbox,
  IonItem,
  IonLabel,
  IonToast,
} from '@ionic/react'
import { useTranslation } from 'react-i18next'
import { useAuth } from '@/features/auth/AuthContext'
import { makeLoginSchema, type LoginFormData } from '@/features/auth/auth.schemas'

export default function LoginPage() {
  const { t } = useTranslation()
  const { login, isAuthenticated, isLoading } = useAuth()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!isLoading && isAuthenticated) navigate('/dashboard', { replace: true })
  }, [isAuthenticated, isLoading, navigate])

  const schema = makeLoginSchema(t)
  const { control, handleSubmit, formState: { errors, isSubmitting } } = useForm<LoginFormData>({
    resolver: zodResolver(schema),
    defaultValues: { email: '', password: '', rememberMe: false },
  })

  async function onSubmit(data: LoginFormData) {
    setError(null)
    const result = await login(data.email, data.password, data.rememberMe ?? false)
    if (result.ok) {
      navigate('/dashboard', { replace: true })
    } else {
      setError(result.error ?? t('auth.login.invalidCredentials'))
    }
  }

  return (
    <IonPage>
      <IonContent className="ion-padding" style={{ '--background': 'var(--ion-background-color)' }}>
        <div style={{ maxWidth: 400, margin: '60px auto 0' }}>
          <h1 style={{ fontFamily: 'var(--ion-font-family)', color: 'var(--ion-color-primary)', marginBottom: 8 }}>
            Expenses Manager
          </h1>
          <p style={{ color: 'var(--ion-color-medium)', marginBottom: 32 }}>
            {t('auth.login.subtitle')}
          </p>

          <form onSubmit={handleSubmit(onSubmit)} noValidate>
            <IonItem style={{ marginBottom: 12 }}>
              <IonLabel position="stacked">{t('auth.email.label')}</IonLabel>
              <Controller
                name="email"
                control={control}
                render={({ field }) => (
                  <IonInput
                    type="email"
                    autocomplete="email"
                    value={field.value}
                    onIonInput={e => field.onChange(e.detail.value ?? '')}
                    onBlur={field.onBlur}
                  />
                )}
              />
            </IonItem>
            {errors.email && (
              <IonText color="danger" style={{ fontSize: 13, paddingLeft: 16 }}>
                <p>{errors.email.message}</p>
              </IonText>
            )}

            <IonItem style={{ marginBottom: 12 }}>
              <IonLabel position="stacked">{t('auth.login.password')}</IonLabel>
              <Controller
                name="password"
                control={control}
                render={({ field }) => (
                  <IonInput
                    type="password"
                    autocomplete="current-password"
                    value={field.value}
                    onIonInput={e => field.onChange(e.detail.value ?? '')}
                    onBlur={field.onBlur}
                  />
                )}
              />
            </IonItem>
            {errors.password && (
              <IonText color="danger" style={{ fontSize: 13, paddingLeft: 16 }}>
                <p>{errors.password.message}</p>
              </IonText>
            )}

            <IonItem lines="none" style={{ marginBottom: 24 }}>
              <IonLabel>{t('auth.login.rememberMe')}</IonLabel>
              <Controller
                name="rememberMe"
                control={control}
                render={({ field }) => (
                  <IonCheckbox
                    slot="start"
                    checked={field.value ?? false}
                    onIonChange={e => field.onChange(e.detail.checked)}
                  />
                )}
              />
            </IonItem>

            <IonButton
              expand="block"
              type="submit"
              {...(isSubmitting ? { disabled: true } : {})}
              color="primary"
            >
              {isSubmitting ? <IonSpinner name="crescent" /> : t('auth.login.submit')}
            </IonButton>
          </form>
        </div>

        <IonToast
          isOpen={!!error}
          message={error ?? ''}
          duration={3000}
          color="danger"
          onDidDismiss={() => setError(null)}
        />
      </IonContent>
    </IonPage>
  )
}
