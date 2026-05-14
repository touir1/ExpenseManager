import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { usePageTitle } from '@/hooks/usePageTitle'
import LanguageSwitcher from '@/components/LanguageSwitcher'

export default function SettingsPage() {
  const { t } = useTranslation()
  usePageTitle(t('settings.pageTitle'))
  return (
    <div className="max-w-5xl mx-auto w-full px-4 sm:px-6 py-8">
      <div className="mb-8">
        <h1 className="text-2xl font-semibold text-ink tracking-tight">{t('settings.title')}</h1>
        <p className="text-sm text-ink-mute mt-1">{t('settings.subtitle')}</p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {/* Password card */}
        <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6">
          <div className="flex items-center gap-3 mb-4">
            <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-brand-100">
              <svg
                className="h-4.5 w-4.5 text-brand-600"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                strokeWidth={2}
                aria-hidden="true"
              >
                <path strokeLinecap="round" strokeLinejoin="round" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
              </svg>
            </span>
            <h2 className="text-sm font-semibold text-ink">{t('settings.password.title')}</h2>
          </div>
          <p className="text-xs text-ink-mute mb-3">{t('settings.password.description')}</p>
          <Link
            to="/change-password"
            className="inline-flex items-center gap-1.5 text-sm text-brand-600 hover:text-brand-700 font-medium transition-colors duration-150"
          >
            {t('settings.password.changeLink')}
            <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5} aria-hidden="true">
              <path strokeLinecap="round" strokeLinejoin="round" d="M9 5l7 7-7 7" />
            </svg>
          </Link>
        </div>
        {/* Language card */}
        <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6">
          <div className="flex items-center gap-3 mb-4">
            <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-sky-soft">
              <svg
                className="h-4 w-4 text-sky"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                strokeWidth={1.6}
                aria-hidden="true"
              >
                <circle cx="12" cy="12" r="9" />
                <path d="M3 12h18M12 3c2.5 3 2.5 15 0 18M12 3c-2.5 3-2.5 15 0 18" />
              </svg>
            </span>
            <h2 className="text-sm font-semibold text-ink">{t('settings.language.title')}</h2>
          </div>
          <p className="text-xs text-ink-mute mb-3">{t('settings.language.description')}</p>
          <LanguageSwitcher />
        </div>
      </div>
    </div>
  )
}
