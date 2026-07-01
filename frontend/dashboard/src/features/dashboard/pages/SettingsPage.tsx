import { useState, useEffect, useRef, useCallback, type ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useQuery } from '@tanstack/react-query'
import { usePageTitle } from '@/hooks/usePageTitle'
import { useExpensesData } from '@/features/expenses/ExpensesDataContext'
import { useAuth } from '@/features/auth/AuthContext'
import {
  getConfig,
  updateConfig,
  updateDefaultCsvColumnMapping,
  clearDefaultCsvColumnMapping,
} from '@/features/settings/services/userConfigApi.service'
import { getNotificationPreferences, updateNotificationPreferences } from '@/features/settings/services/notificationPreferencesApi.service'
import { deleteAccountRequest } from '@/features/auth/services/authApi.service'
import { useToast } from '@/components/Toast'
import ThemeToggle from '@/components/ThemeToggle'
import type { NotificationPreferenceDto } from '@/features/settings/types/userConfig.type'
import { CSV_CANONICAL_FIELDS } from '@/features/expenses/types/expenses.type'

const NOTIFICATION_EVENT_TYPES = [
  'familyInvitation',
  'familyMemberJoined',
  'familyMemberRemoved',
  'familyExpenseAdded',
  'familyExpenseDeleted',
  'csvImportCompleted',
  'rateConflict',
] as const

function SettingsSection({ title, children, danger }: { title: string; children: ReactNode; danger?: boolean }) {
  return (
    <div className={danger ? 'border border-berry/30 rounded-2xl p-4' : ''}>
      <h2 className="text-xs font-semibold text-ink-faint uppercase tracking-widest mb-3">{title}</h2>
      <div className={`grid gap-4 sm:grid-cols-2 ${danger ? '' : 'lg:grid-cols-3'}`}>{children}</div>
    </div>
  )
}

function DefaultCurrencyCard() {
  const { t } = useTranslation()
  const { currencies } = useExpensesData()
  const { show } = useToast()
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const [saving, setSaving] = useState(false)
  const [saved, setSaved] = useState(false)

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
        setSaved(true)
        setTimeout(() => setSaved(false), 2000)
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
          <svg className="h-4.5 w-4.5 text-brand-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </span>
        <h3 className="text-sm font-semibold text-ink">{t('settings.currency.title')}</h3>
      </div>
      <p className="text-xs text-ink-mute mb-3">{t('settings.currency.description')}</p>
      <select
        aria-label={t('settings.currency.title')}
        value={selectedId ?? ''}
        onChange={e => setSelectedId(Number(e.target.value))}
        className="w-full text-sm border border-surface-border rounded-lg px-3 py-2 mb-4 bg-surface-card text-ink focus:outline-none focus:ring-2 focus:ring-brand-400"
      >
        {currencies.map(c => (
          <option key={c.id} value={c.id}>{c.code} {c.symbol}</option>
        ))}
      </select>
      <button
        onClick={handleSave}
        disabled={saving}
        className={`inline-flex items-center gap-1.5 text-sm font-medium text-white px-4 py-2 rounded-lg transition-colors duration-150 disabled:opacity-60 ${saved ? 'bg-sage hover:bg-sage/90' : 'bg-brand-600 hover:bg-brand-700'}`}
      >
        {saved ? (
          <>
            <svg className="h-4 w-4 shrink-0 transition-transform duration-200 scale-100" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5} aria-hidden="true">
              <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
            </svg>
            {t('settings.savedConfirm')}
          </>
        ) : saving ? t('settings.currency.saving') : t('settings.currency.save')}
      </button>
      <span aria-live="polite" className="sr-only">{saved ? t('settings.savedConfirm') : ''}</span>
    </div>
  )
}

function DefaultCategoryCard() {
  const { t } = useTranslation()
  const { categories } = useExpensesData()
  const { show } = useToast()
  const [selectedId, setSelectedId] = useState<number | ''>('')
  const [saving, setSaving] = useState(false)
  const [saved, setSaved] = useState(false)

  const { data: configData } = useQuery({
    queryKey: ['userConfig'],
    queryFn: async () => {
      const res = await getConfig()
      return res.ok ? res.data ?? null : null
    },
  })

  useEffect(() => {
    if (configData?.defaultCategoryId) {
      setSelectedId(configData.defaultCategoryId)
    }
  }, [configData])

  async function handleSave() {
    setSaving(true)
    try {
      const res = await updateConfig({ defaultCategoryId: selectedId === '' ? null : selectedId })
      if (res.ok) {
        setSaved(true)
        setTimeout(() => setSaved(false), 2000)
      } else {
        show(t('settings.defaultCategory.error'), 'error')
      }
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6">
      <div className="flex items-center gap-3 mb-4">
        <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-brand-100">
          <svg className="h-4.5 w-4.5 text-brand-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
            <path strokeLinecap="round" strokeLinejoin="round" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-5 5a2 2 0 01-2.828 0l-7-7A2 2 0 013 9V4a1 1 0 011-1h3z" />
          </svg>
        </span>
        <h3 className="text-sm font-semibold text-ink">{t('settings.defaultCategory.title')}</h3>
      </div>
      <p className="text-xs text-ink-mute mb-3">{t('settings.defaultCategory.description')}</p>
      <select
        aria-label={t('settings.defaultCategory.title')}
        value={selectedId}
        onChange={e => setSelectedId(e.target.value === '' ? '' : Number(e.target.value))}
        className="w-full text-sm border border-surface-border rounded-lg px-3 py-2 mb-4 bg-surface-card text-ink focus:outline-none focus:ring-2 focus:ring-brand-400"
      >
        <option value="">{t('settings.defaultCategory.none')}</option>
        {categories.map(c => (
          <option key={c.id} value={c.id}>{c.name}</option>
        ))}
      </select>
      <button
        onClick={handleSave}
        disabled={saving}
        className={`inline-flex items-center gap-1.5 text-sm font-medium text-white px-4 py-2 rounded-lg transition-colors duration-150 disabled:opacity-60 ${saved ? 'bg-sage hover:bg-sage/90' : 'bg-brand-600 hover:bg-brand-700'}`}
      >
        {saved ? (
          <>
            <svg className="h-4 w-4 shrink-0 transition-transform duration-200 scale-100" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5} aria-hidden="true">
              <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
            </svg>
            {t('settings.savedConfirm')}
          </>
        ) : saving ? t('settings.defaultCategory.saving') : t('settings.defaultCategory.save')}
      </button>
      <span aria-live="polite" className="sr-only">{saved ? t('settings.savedConfirm') : ''}</span>
    </div>
  )
}

function DefaultCsvColumnMappingCard() {
  const { t } = useTranslation()
  const { show } = useToast()
  const [rows, setRows] = useState<{ raw: string; canonical: string }[]>([])
  const [saving, setSaving] = useState(false)
  const [saved, setSaved] = useState(false)
  const seededRef = useRef(false)

  const { data: configData } = useQuery({
    queryKey: ['userConfig'],
    queryFn: async () => {
      const res = await getConfig()
      return res.ok ? res.data ?? null : null
    },
  })

  useEffect(() => {
    if (!configData || seededRef.current) return
    seededRef.current = true
    const mapping = configData.defaultCsvColumnMapping
    setRows(mapping ? Object.entries(mapping).map(([raw, canonical]) => ({ raw, canonical })) : [])
  }, [configData])

  function updateRow(index: number, field: 'raw' | 'canonical', value: string) {
    setRows(prev => prev.map((r, i) => (i === index ? { ...r, [field]: value } : r)))
  }

  function removeRow(index: number) {
    setRows(prev => prev.filter((_, i) => i !== index))
  }

  function addRow() {
    seededRef.current = true // once the user starts editing, the query-seeded snapshot must not overwrite it
    setRows(prev => [...prev, { raw: '', canonical: CSV_CANONICAL_FIELDS[0] }])
  }

  async function handleSave() {
    setSaving(true)
    try {
      const mapping = Object.fromEntries(
        rows.filter(r => r.raw.trim().length > 0).map(r => [r.raw.trim(), r.canonical]),
      )
      const res = await updateDefaultCsvColumnMapping(mapping)
      if (res.ok) {
        setSaved(true)
        setTimeout(() => setSaved(false), 2000)
      } else {
        show(t('settings.csvColumnMapping.error'), 'error')
      }
    } finally {
      setSaving(false)
    }
  }

  async function handleClear() {
    const res = await clearDefaultCsvColumnMapping()
    if (res.ok) {
      setRows([])
    } else {
      show(t('settings.csvColumnMapping.error'), 'error')
    }
  }

  return (
    <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6 sm:col-span-2 lg:col-span-1">
      <div className="flex items-center gap-3 mb-4">
        <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-brand-100">
          <svg className="h-4.5 w-4.5 text-brand-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
            <path strokeLinecap="round" strokeLinejoin="round" d="M9 17V7m6 10V7M5 5h14a2 2 0 012 2v10a2 2 0 01-2 2H5a2 2 0 01-2-2V7a2 2 0 012-2z" />
          </svg>
        </span>
        <h3 className="text-sm font-semibold text-ink">{t('settings.csvColumnMapping.title')}</h3>
      </div>
      <p className="text-xs text-ink-mute mb-4">{t('settings.csvColumnMapping.description')}</p>

      {rows.length === 0 ? (
        <p className="text-xs text-ink-mute mb-4">{t('settings.csvColumnMapping.empty')}</p>
      ) : (
        <div className="flex flex-col gap-2 mb-4">
          {rows.map((row, i) => (
            <div key={i} className="flex items-center gap-2">
              <input
                type="text"
                value={row.raw}
                onChange={e => updateRow(i, 'raw', e.target.value)}
                aria-label={t('settings.csvColumnMapping.rawColumn')}
                placeholder={t('settings.csvColumnMapping.rawColumn')}
                className="flex-1 min-w-0 text-xs border border-surface-border rounded-lg px-2 py-1.5 bg-surface-card text-ink focus:outline-none focus:ring-1 focus:ring-brand-400"
              />
              <select
                value={row.canonical}
                onChange={e => updateRow(i, 'canonical', e.target.value)}
                aria-label={t('settings.csvColumnMapping.mapsTo')}
                className="flex-1 min-w-0 text-xs border border-surface-border rounded-lg px-2 py-1.5 bg-surface-card text-ink focus:outline-none focus:ring-1 focus:ring-brand-400"
              >
                {CSV_CANONICAL_FIELDS.map(f => (
                  <option key={f} value={f}>{t(`expenses.table.${f}`, t(`expenses.fields.${f}`, f))}</option>
                ))}
              </select>
              <button
                type="button"
                onClick={() => removeRow(i)}
                aria-label={`Remove ${row.raw || 'row'}`}
                className="p-1 rounded-lg text-ink-mute hover:text-berry hover:bg-berry-soft transition-colors shrink-0"
              >
                <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
          ))}
        </div>
      )}

      <button
        type="button"
        onClick={addRow}
        className="mb-4 text-xs font-medium text-brand-600 hover:text-brand-700 transition-colors"
      >
        + {t('settings.csvColumnMapping.addColumn')}
      </button>

      <div className="flex items-center gap-3 flex-wrap">
        <button
          onClick={handleSave}
          disabled={saving}
          className={`inline-flex items-center gap-1.5 text-sm font-medium text-white px-4 py-2 rounded-lg transition-colors duration-150 disabled:opacity-60 ${saved ? 'bg-sage hover:bg-sage/90' : 'bg-brand-600 hover:bg-brand-700'}`}
        >
          {saved ? (
            <>
              <svg className="h-4 w-4 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5} aria-hidden="true">
                <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
              </svg>
              {t('settings.savedConfirm')}
            </>
          ) : saving ? t('settings.csvColumnMapping.saving') : t('settings.csvColumnMapping.save')}
        </button>
        {rows.length > 0 && (
          <button
            type="button"
            onClick={handleClear}
            className="text-xs font-medium text-berry hover:text-berry/80 transition-colors"
          >
            {t('settings.csvColumnMapping.clear')}
          </button>
        )}
      </div>
      <span aria-live="polite" className="sr-only">{saved ? t('settings.savedConfirm') : ''}</span>
    </div>
  )
}

function DefaultExpenseDateCard() {
  const { t } = useTranslation()
  const [value, setValue] = useState<'today' | 'last-used'>(() => {
    return (localStorage.getItem('expenseDefaultDate') as 'today' | 'last-used') ?? 'today'
  })
  const [saved, setSaved] = useState(false)

  function handleChange(v: 'today' | 'last-used') {
    setValue(v)
    localStorage.setItem('expenseDefaultDate', v)
    setSaved(true)
    setTimeout(() => setSaved(false), 2000)
  }

  return (
    <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6">
      <div className="flex items-center gap-3 mb-4">
        <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-brand-100">
          <svg className="h-4.5 w-4.5 text-brand-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
            <path strokeLinecap="round" strokeLinejoin="round" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
        </span>
        <h3 className="text-sm font-semibold text-ink">{t('settings.expenseDate.title')}</h3>
      </div>
      <p className="text-xs text-ink-mute mb-3">{t('settings.expenseDate.description')}</p>
      <div className="flex flex-col gap-2 mb-4">
        {(['today', 'last-used'] as const).map(opt => (
          <label key={opt} className="flex items-center gap-2 cursor-pointer">
            <input
              type="radio"
              name="expenseDate"
              value={opt}
              checked={value === opt}
              onChange={() => handleChange(opt)}
              className="accent-brand-600"
            />
            <span className="text-sm text-ink">
              {opt === 'today' ? t('settings.expenseDate.today') : t('settings.expenseDate.lastUsed')}
            </span>
          </label>
        ))}
      </div>
      {saved && <p className="text-xs text-sage-600 font-medium">{t('settings.savedConfirm')}</p>}
    </div>
  )
}

function NotificationPreferencesCard() {
  const { t } = useTranslation()
  const { show } = useToast()
  const [prefs, setPrefs] = useState<Record<string, boolean>>({})
  const [saving, setSaving] = useState(false)
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const { data, isLoading } = useQuery({
    queryKey: ['notificationPreferences'],
    queryFn: async () => {
      const res = await getNotificationPreferences()
      return res.ok ? res.data ?? [] : []
    },
  })

  useEffect(() => {
    if (!data) return
    const map: Record<string, boolean> = {}
    for (const et of NOTIFICATION_EVENT_TYPES) {
      const found = data.find((p: NotificationPreferenceDto) => p.eventType === et)
      map[et] = found ? found.emailEnabled : true
    }
    setPrefs(map)
  }, [data])

  const debouncedSave = useCallback((updated: Record<string, boolean>) => {
    if (debounceRef.current) clearTimeout(debounceRef.current)
    debounceRef.current = setTimeout(async () => {
      setSaving(true)
      try {
        const payload = Object.entries(updated).map(([eventType, emailEnabled]) => ({ eventType, emailEnabled }))
        const res = await updateNotificationPreferences(payload)
        if (!res.ok) show(t('settings.notifications.error'), 'error')
      } finally {
        setSaving(false)
      }
    }, 500)
  }, [show, t])

  function toggle(et: string) {
    const updated = { ...prefs, [et]: !prefs[et] }
    setPrefs(updated)
    debouncedSave(updated)
  }

  return (
    <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6 sm:col-span-2 lg:col-span-1">
      <div className="flex items-center gap-3 mb-4">
        <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-brand-100">
          <svg className="h-4.5 w-4.5 text-brand-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
            <path strokeLinecap="round" strokeLinejoin="round" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
          </svg>
        </span>
        <h3 className="text-sm font-semibold text-ink">{t('settings.notifications.title')}</h3>
        {saving && <span className="ml-auto text-xs text-ink-faint">{t('settings.notifications.saving')}</span>}
      </div>
      <p className="text-xs text-ink-mute mb-4">{t('settings.notifications.description')}</p>
      {isLoading ? (
        <div className="space-y-2">
          {[1, 2, 3].map(i => <div key={i} className="h-5 bg-surface-border rounded animate-pulse" />)}
        </div>
      ) : (
        <div className="flex flex-col gap-2">
          {NOTIFICATION_EVENT_TYPES.map(et => (
            <label key={et} className="flex items-center justify-between gap-2 cursor-pointer">
              <span className="text-xs text-ink">{t(`settings.notifications.${et}`)}</span>
              <input
                type="checkbox"
                aria-label={t(`settings.notifications.${et}`)}
                checked={prefs[et] ?? true}
                onChange={() => toggle(et)}
                className="accent-brand-600 h-4 w-4 shrink-0"
              />
            </label>
          ))}
        </div>
      )}
    </div>
  )
}

function DataExportCard() {
  const { t } = useTranslation()
  const { show } = useToast()
  const [exporting, setExporting] = useState(false)

  async function handleExport() {
    setExporting(true)
    try {
      const res = await fetch('/api/expenses/export', { credentials: 'include' })
      if (!res.ok) {
        show(t('settings.export.error'), 'error')
        return
      }
      const blob = await res.blob()
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `expenses_${new Date().toISOString().slice(0, 10)}.csv`
      document.body.appendChild(a)
      a.click()
      a.remove()
      URL.revokeObjectURL(url)
    } catch {
      show(t('settings.export.error'), 'error')
    } finally {
      setExporting(false)
    }
  }

  return (
    <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6">
      <div className="flex items-center gap-3 mb-4">
        <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-berry-soft">
          <svg className="h-4.5 w-4.5 text-berry" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
            <path strokeLinecap="round" strokeLinejoin="round" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
          </svg>
        </span>
        <h3 className="text-sm font-semibold text-ink">{t('settings.export.title')}</h3>
      </div>
      <p className="text-xs text-ink-mute mb-4">{t('settings.export.description')}</p>
      <button
        onClick={handleExport}
        disabled={exporting}
        className="inline-flex items-center gap-1.5 text-sm font-medium text-berry border border-berry/40 hover:bg-berry-soft disabled:opacity-60 px-4 py-2 rounded-lg transition-colors duration-150"
      >
        {exporting ? t('settings.export.exporting') : t('settings.export.button')}
      </button>
    </div>
  )
}

function ConfirmDeleteModal({ onConfirm, onCancel }: { onConfirm: () => void; onCancel: () => void }) {
  const { t } = useTranslation()
  const [deleting, setDeleting] = useState(false)
  const cancelRef = useRef<HTMLButtonElement>(null)

  useEffect(() => {
    cancelRef.current?.focus()
  }, [])

  async function handleConfirm() {
    setDeleting(true)
    try {
      await onConfirm()
    } finally {
      setDeleting(false)
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm" role="dialog" aria-modal="true" aria-labelledby="delete-modal-title">
      <div className="bg-surface-card rounded-2xl border border-surface-border shadow-xl p-6 max-w-md w-full mx-4">
        <h2 id="delete-modal-title" className="text-base font-semibold text-ink mb-2">{t('settings.deleteAccount.confirmTitle')}</h2>
        <p className="text-sm text-ink-mute mb-6">{t('settings.deleteAccount.confirmBody')}</p>
        <div className="flex gap-3 justify-end">
          <button
            ref={cancelRef}
            onClick={onCancel}
            disabled={deleting}
            className="text-sm font-medium text-ink-mute border border-surface-border hover:bg-surface-hover px-4 py-2 rounded-lg transition-colors duration-150 disabled:opacity-60"
          >
            {t('settings.deleteAccount.cancelButton')}
          </button>
          <button
            onClick={handleConfirm}
            disabled={deleting}
            className="text-sm font-medium text-white bg-berry hover:bg-berry/90 disabled:opacity-60 px-4 py-2 rounded-lg transition-colors duration-150"
          >
            {deleting ? t('settings.deleteAccount.deleting') : t('settings.deleteAccount.confirmButton')}
          </button>
        </div>
      </div>
    </div>
  )
}

function AccountDeletionCard() {
  const { t } = useTranslation()
  const { show } = useToast()
  const { logout } = useAuth()
  const [showModal, setShowModal] = useState(false)
  const triggerRef = useRef<HTMLButtonElement>(null)

  async function handleDelete() {
    const res = await deleteAccountRequest()
    if (res.ok) {
      setShowModal(false)
      logout()
    } else {
      show(t('settings.deleteAccount.error'), 'error')
      setShowModal(false)
      triggerRef.current?.focus()
    }
  }

  return (
    <>
      <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6">
        <div className="flex items-center gap-3 mb-4">
          <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-berry-soft">
            <svg className="h-4.5 w-4.5 text-berry" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
              <path strokeLinecap="round" strokeLinejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
            </svg>
          </span>
          <h3 className="text-sm font-semibold text-ink">{t('settings.deleteAccount.title')}</h3>
        </div>
        <p className="text-xs text-ink-mute mb-4">{t('settings.deleteAccount.description')}</p>
        <button
          ref={triggerRef}
          onClick={() => setShowModal(true)}
          className="inline-flex items-center gap-1.5 text-sm font-medium text-berry border border-berry/40 hover:bg-berry-soft px-4 py-2 rounded-lg transition-colors duration-150"
        >
          {t('settings.deleteAccount.button')}
        </button>
      </div>
      {showModal && (
        <ConfirmDeleteModal
          onConfirm={handleDelete}
          onCancel={() => {
            setShowModal(false)
            triggerRef.current?.focus()
          }}
        />
      )}
    </>
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

      <div className="flex flex-col gap-8">
        <SettingsSection title={t('settings.sections.account')}>
          {/* Password card */}
          <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6">
            <div className="flex items-center gap-3 mb-4">
              <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-brand-100">
                <svg className="h-4.5 w-4.5 text-brand-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
                  <path strokeLinecap="round" strokeLinejoin="round" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                </svg>
              </span>
              <h3 className="text-sm font-semibold text-ink">{t('settings.password.title')}</h3>
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

          <NotificationPreferencesCard />
        </SettingsSection>

        <SettingsSection title={t('settings.sections.preferences')}>
          <DefaultCurrencyCard />
          {/* Theme card */}
          <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-6">
            <div className="flex items-center gap-3 mb-4">
              <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-brand-100">
                <svg className="h-4.5 w-4.5 text-brand-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
                  <path strokeLinecap="round" strokeLinejoin="round" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
                </svg>
              </span>
              <h3 className="text-sm font-semibold text-ink">{t('settings.theme.title')}</h3>
            </div>
            <p className="text-xs text-ink-mute mb-4">{t('settings.theme.description')}</p>
            <ThemeToggle />
          </div>
          <DefaultExpenseDateCard />
          <DefaultCategoryCard />
          <DefaultCsvColumnMappingCard />
        </SettingsSection>

        <SettingsSection title={t('settings.sections.dangerZone')} danger>
          <DataExportCard />
          <AccountDeletionCard />
        </SettingsSection>
      </div>
    </div>
  )
}
