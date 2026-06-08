import {
  IonPage,
  IonHeader,
  IonToolbar,
  IonTitle,
  IonContent,
  IonList,
  IonItem,
  IonLabel,
  IonSelect,
  IonSelectOption,
  IonButton,
  IonText,
} from '@ionic/react'
import { useTranslation } from 'react-i18next'
import { useAuth } from '@/features/auth/AuthContext'
import { useDisplayCurrency } from '@/features/currencies/DisplayCurrencyContext'
import { useExpensesData } from '@/features/expenses/ExpensesDataContext'

const LANGUAGES = [
  { code: 'en', label: 'English' },
  { code: 'fr', label: 'Français' },
  { code: 'es', label: 'Español' },
  { code: 'de', label: 'Deutsch' },
]

export default function SettingsPage() {
  const { t, i18n } = useTranslation()
  const { user, logout } = useAuth()
  const { displayCurrencyId, setDisplayCurrencyId } = useDisplayCurrency()
  const { currencies } = useExpensesData()

  async function handleLanguageChange(lang: string) {
    await i18n.changeLanguage(lang)
    localStorage.setItem('i18nextLng', lang)
    try {
      const { Preferences } = await import('@capacitor/preferences')
      await Preferences.set({ key: 'lang', value: lang })
    } catch { /* web fallback already done */ }
  }

  return (
    <IonPage>
      <IonHeader>
        <IonToolbar color="light">
          <IonTitle>{t('nav.settings', 'Settings')}</IonTitle>
        </IonToolbar>
      </IonHeader>

      <IonContent>
        {user && (
          <div style={{ padding: '16px 16px 0' }}>
            <IonText color="medium" style={{ fontSize: 13 }}>
              <p>{user.email}</p>
            </IonText>
          </div>
        )}

        <IonList>
          <IonItem>
            <IonLabel>{t('settings.displayCurrency', 'Display currency')}</IonLabel>
            <IonSelect
              value={displayCurrencyId}
              onIonChange={e => setDisplayCurrencyId(e.detail.value)}
              interface="action-sheet"
              slot="end"
            >
              {currencies.map(c => (
                <IonSelectOption key={c.id} value={c.id}>{c.code}</IonSelectOption>
              ))}
            </IonSelect>
          </IonItem>

          <IonItem>
            <IonLabel>{t('settings.language', 'Language')}</IonLabel>
            <IonSelect
              value={i18n.language.split('-')[0]}
              onIonChange={e => handleLanguageChange(e.detail.value)}
              interface="action-sheet"
              slot="end"
            >
              {LANGUAGES.map(l => (
                <IonSelectOption key={l.code} value={l.code}>{l.label}</IonSelectOption>
              ))}
            </IonSelect>
          </IonItem>
        </IonList>

        <div style={{ padding: 16 }}>
          <IonButton expand="block" color="danger" fill="outline" onClick={logout}>
            {t('nav.signOut', 'Sign out')}
          </IonButton>
        </div>
      </IonContent>
    </IonPage>
  )
}
