import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useQuery } from '@tanstack/react-query'
import { usePageTitle } from '@/hooks/usePageTitle'
import { useExpensesData } from '@/features/expenses/ExpensesDataContext'
import { getConfig, updateConfig } from '@/features/settings/services/userConfigApi.service'
import { useToast } from '@/components/Toast'
import ThemeToggle from '@/components/ThemeToggle'

function DefaultCurrencyCard() {
  const { t } = useTranslation()
  const { currencies } = useExpensesData()
  const { show } = useToast()
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const [saving, setSaving] = useState(false)

  const { data: configData } = useQuery({
    queryKey: ['userConfig'],
    queryFn: async () => {
      const res = await getConfig()
      return res.ok ? res.data ?? null : null
    },
  })

  useEffect(() => {
    if (configData?.defaultCurrencyId) {
      setSelectedId(configData.defaultCurrencyId)
    } else if (currencies.length > 0 && selectedId === null) {
      setSelectedId(currencies[0].id)
    }
  }, [configData, currencies])

  async function handleSave() {
    setSaving(true)
    try {
      const res = await updateConfig({ defaultCurrencyId: selectedId })
      if (res.ok) {
        show(t('settings.currency.success'), 'success')
      } else {
        show(t('settings.currency.error'), 'error')
      }
    } finally {
      setSaving(false)
    }
  }

  return (
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
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </span>
        <h2 className="text-sm font-semibold text-ink">{t('settings.currency.title')}</h2>
      </div>
      <p className="text-xs text-ink-mute mb-3">{t('settings.currency.description')}</p>

      <select
        aria-label={t('settings.currency.title')}
        value={selectedId ?? ''}
        onChange={e => setSelectedId(Number(e.target.value))}
        className="w-full text-sm border border-surface-border rounded-lg px-3 py-2 mb-4 bg-surface-card text-ink focus:outline-none focus:ring-2 focus:ring-brand-400"
      >
        {currencies.map(c => (
          <option key={c.id} value={c.id}>
            {c.code} {c.symbol}
          </option>
        ))}
      </select>

      <button
        onClick={handleSave}
        disabled={saving}
        className="inline-flex items-center gap-1.5 text-sm font-medium text-white bg-brand-600 hover:bg-brand-700 disabled:opacity-60 px-4 py-2 rounded-lg transition-colors duration-150"
      >
        {saving ? t('settings.currency.saving') : t('settings.currency.save')}
      </button>
    </div>
  )
}

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

        {/* Default currency card */}
        <DefaultCurrencyCard />

        {/* Theme card */}
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
                <path strokeLinecap="round" strokeLinejoin="round" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
              </svg>
            </span>
            <h2 className="text-sm font-semibold text-ink">{t('settings.theme.title')}</h2>
          </div>
          <p className="text-xs text-ink-mute mb-4">{t('settings.theme.description')}</p>
          <ThemeToggle />
        </div>
      </div>
    </div>
  )
}
