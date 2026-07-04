import { useEffect, useState } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
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
  IonToggle,
  IonAlert,
} from '@ionic/react'
import { useTranslation } from 'react-i18next'
import { useAuth } from '@/features/auth/AuthContext'
import { useDisplayCurrency } from '@/features/currencies/DisplayCurrencyContext'
import { useExpensesData } from '@/features/expenses/ExpensesDataContext'
import { useTheme, type Theme } from '@/features/settings/ThemeContext'
import { getConfig, updateConfig } from '@/features/settings/services/userConfigApi.service'
import { getNotificationPreferences, updateNotificationPreferences } from '@/features/settings/services/notificationPreferencesApi.service'
import { deleteAccountRequest } from '@/features/auth/services/authApi.service'
import type { NotificationPreferenceDto } from '@/features/settings/types/userConfig.type'

const LANGUAGES = [
  { code: 'en', label: 'English' },
  { code: 'fr', label: 'Français' },
  { code: 'es', label: 'Español' },
  { code: 'de', label: 'Deutsch' },
]

const THEMES: { value: Theme; labelKey: string }[] = [
  { value: 'light', labelKey: 'settings.theme.light' },
  { value: 'system', labelKey: 'settings.theme.system' },
  { value: 'dark', labelKey: 'settings.theme.dark' },
]

const NOTIFICATION_EVENT_TYPES = [
  { key: 'familyInvitation', fallback: 'Family invitations' },
  { key: 'familyMemberJoined', fallback: 'Member joined' },
  { key: 'familyMemberRemoved', fallback: 'Member removed' },
  { key: 'familyExpenseAdded', fallback: 'Expense added' },
  { key: 'familyExpenseDeleted', fallback: 'Expense deleted' },
  { key: 'csvImportCompleted', fallback: 'CSV import completed' },
  { key: 'rateConflict', fallback: 'Currency rate conflicts' },
] as const

function NotificationPreferencesSection() {
  const { t } = useTranslation()
  const [prefs, setPrefs] = useState<Record<string, boolean>>({})

  const { data } = useQuery({
    queryKey: ['notificationPreferences'],
    queryFn: async () => {
      const res = await getNotificationPreferences()
      return res.ok ? res.data ?? [] : []
    },
  })

  useEffect(() => {
    if (!data) return
    const map: Record<string, boolean> = {}
    for (const { key } of NOTIFICATION_EVENT_TYPES) {
      const found = data.find((p: NotificationPreferenceDto) => p.eventType === key)
      map[key] = found ? found.emailEnabled : true
    }
    setPrefs(map)
  }, [data])

  async function toggle(eventType: string) {
    const updated = { ...prefs, [eventType]: !prefs[eventType] }
    setPrefs(updated)
    await updateNotificationPreferences(
      Object.entries(updated).map(([et, emailEnabled]) => ({ eventType: et, emailEnabled })),
    )
  }

  return (
    <IonList>
      {NOTIFICATION_EVENT_TYPES.map(({ key, fallback }) => (
        <IonItem key={key}>
          <IonLabel>{t(`settings.notifications.${key}`, fallback)}</IonLabel>
          <IonToggle
            slot="end"
            checked={prefs[key] ?? true}
            onIonChange={() => toggle(key)}
          />
        </IonItem>
      ))}
    </IonList>
  )
}

export default function SettingsPage() {
  const { t, i18n } = useTranslation()
  const { user, logout } = useAuth()
  const { displayCurrencyId, setDisplayCurrencyId } = useDisplayCurrency()
  const { currencies, categories } = useExpensesData()
  const { theme, setTheme } = useTheme()
  const queryClient = useQueryClient()
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false)

  const { data: configData } = useQuery({
    queryKey: ['userConfig'],
    queryFn: async () => {
      const res = await getConfig()
      return res.ok ? res.data ?? null : null
    },
  })

  async function handleLanguageChange(lang: string) {
    await i18n.changeLanguage(lang)
    localStorage.setItem('i18nextLng', lang)
    try {
      const { Preferences } = await import('@capacitor/preferences')
      await Preferences.set({ key: 'lang', value: lang })
    } catch { /* web fallback already done */ }
  }

  async function handleDefaultCategoryChange(categoryId: number | null) {
    await updateConfig({ defaultCategoryId: categoryId })
    queryClient.invalidateQueries({ queryKey: ['userConfig'] })
  }

  async function handleDeleteAccount() {
    const res = await deleteAccountRequest()
    setShowDeleteConfirm(false)
    if (res.ok) {
      logout()
    }
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
            <IonLabel>{t('settings.currency.title', 'Display currency')}</IonLabel>
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
            <IonLabel>{t('settings.defaultCategory.title', 'Default category')}</IonLabel>
            <IonSelect
              value={configData?.defaultCategoryId ?? null}
              onIonChange={e => handleDefaultCategoryChange(e.detail.value)}
              interface="action-sheet"
              slot="end"
              placeholder={t('settings.defaultCategory.none', 'None')}
            >
              <IonSelectOption value={null}>{t('settings.defaultCategory.none', 'None')}</IonSelectOption>
              {categories.map(c => (
                <IonSelectOption key={c.id} value={c.id}>{c.name}</IonSelectOption>
              ))}
            </IonSelect>
          </IonItem>

          <IonItem>
            <IonLabel>{t('settings.language.title', 'Language')}</IonLabel>
            <IonSelect
              value={(i18n.language ?? 'en').split('-')[0]}
              onIonChange={e => handleLanguageChange(e.detail.value)}
              interface="action-sheet"
              slot="end"
            >
              {LANGUAGES.map(l => (
                <IonSelectOption key={l.code} value={l.code}>{l.label}</IonSelectOption>
              ))}
            </IonSelect>
          </IonItem>
          <IonItem>
            <IonLabel>{t('settings.theme.label', 'Theme')}</IonLabel>
            <IonSelect
              value={theme}
              onIonChange={e => setTheme(e.detail.value as Theme)}
              interface="action-sheet"
              slot="end"
            >
              {THEMES.map(opt => (
                <IonSelectOption key={opt.value} value={opt.value}>{t(opt.labelKey)}</IonSelectOption>
              ))}
            </IonSelect>
          </IonItem>
        </IonList>

        <div style={{ padding: '16px 16px 0' }}>
          <IonText color="medium" style={{ fontSize: 12 }}>
            <p style={{ margin: 0 }}>{t('settings.notifications.title', 'Email notifications')}</p>
          </IonText>
        </div>
        <NotificationPreferencesSection />

        <div style={{ padding: 16 }}>
          <IonButton expand="block" color="danger" fill="outline" onClick={logout}>
            {t('nav.signOut', 'Sign out')}
          </IonButton>
          <IonButton
            expand="block"
            color="danger"
            fill="clear"
            onClick={() => setShowDeleteConfirm(true)}
          >
            {t('settings.deleteAccount.button', 'Delete account')}
          </IonButton>
        </div>

        <IonAlert
          isOpen={showDeleteConfirm}
          header={t('settings.deleteAccount.confirmTitle', 'Delete account?')}
          message={t('settings.deleteAccount.confirmBody', 'This cannot be undone.')}
          buttons={[
            { text: t('common.cancel', 'Cancel'), role: 'cancel', handler: () => setShowDeleteConfirm(false) },
            { text: t('settings.deleteAccount.confirmButton', 'Delete'), role: 'destructive', handler: handleDeleteAccount },
          ]}
          onDidDismiss={() => setShowDeleteConfirm(false)}
        />
      </IonContent>
    </IonPage>
  )
}
