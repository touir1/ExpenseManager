import { useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { usePageTitle } from '@/hooks/usePageTitle'
import {
  previewCsvImport,
  confirmCsvImport,
  getImportTemplateUrl,
} from '@/features/expenses/services/expensesApi.service'
import type { CsvImportPreviewDto, CsvImportConfirmRowDto } from '@/features/expenses/types/expenses.type'

export default function CsvImportPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  usePageTitle(t('expenses.import.pageTitle'))

  const fileInputRef = useRef<HTMLInputElement>(null)
  const [dragging, setDragging] = useState(false)
  const [preview, setPreview] = useState<CsvImportPreviewDto | null>(null)
  const [loadingPreview, setLoadingPreview] = useState(false)
  const [confirming, setConfirming] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleFile(file: File) {
    setError(null)
    setLoadingPreview(true)
    const res = await previewCsvImport(file)
    setLoadingPreview(false)
    if (res.ok) {
      setPreview(res.data!)
    } else {
      setError(res.error ?? t('expenses.errors.loadFailed'))
    }
  }

  function handleFileInputChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (file) handleFile(file)
    e.target.value = ''
  }

  function handleDrop(e: React.DragEvent<HTMLDivElement>) {
    e.preventDefault()
    setDragging(false)
    const file = e.dataTransfer.files?.[0]
    if (file) handleFile(file)
  }

  async function handleConfirm() {
    if (!preview) return
    setConfirming(true)
    const validRows: CsvImportConfirmRowDto[] = preview.rows
      .filter(r => r.isValid)
      .map(r => ({
        amount: r.amount!,
        currencyId: r.currencyId!,
        date: r.date!,
        categoryId: r.categoryId,
        subcategoryId: r.subcategoryId,
        descriptionDisplay: r.descriptionDisplay,
        tagNames: r.tagNames,
        familyIds: r.familyIds,
      }))

    const res = await confirmCsvImport(validRows)
    setConfirming(false)
    if (res.ok) {
      navigate('/expenses')
    } else {
      setError(res.error ?? t('expenses.errors.saveFailed'))
    }
  }

  return (
    <div className="max-w-5xl mx-auto w-full px-4 sm:px-6 py-8">
      <div className="mb-6 flex items-center gap-3">
        <button
          onClick={() => navigate('/expenses')}
          className="text-sm text-ink-mute hover:text-ink transition-colors"
          aria-label="Back"
        >
          ← {t('expenses.actions.cancel')}
        </button>
        <h1 className="text-2xl font-semibold text-ink tracking-tight">{t('expenses.import.pageTitle')}</h1>
      </div>

      {error && (
        <div className="mb-4 rounded-xl bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      {!preview ? (
        /* ── Step 1: Upload ── */
        <div className="bg-white shadow-card border border-slate-200 rounded-2xl p-8">
          <div
            role="button"
            tabIndex={0}
            aria-label={t('expenses.import.dropzone')}
            onDragOver={e => { e.preventDefault(); setDragging(true) }}
            onDragLeave={() => setDragging(false)}
            onDrop={handleDrop}
            onClick={() => fileInputRef.current?.click()}
            onKeyDown={e => e.key === 'Enter' && fileInputRef.current?.click()}
            className={`border-2 border-dashed rounded-xl p-12 text-center cursor-pointer transition-colors ${
              dragging ? 'border-brand-600 bg-brand-50' : 'border-slate-300 hover:border-brand-400'
            }`}
          >
            <svg className="mx-auto h-10 w-10 text-slate-400 mb-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" />
            </svg>
            <p className="text-sm text-ink-mute">
              {loadingPreview ? t('expenses.loading', 'Loading…') : t('expenses.import.dropzone')}
            </p>
            <input
              ref={fileInputRef}
              type="file"
              accept=".csv"
              className="hidden"
              onChange={handleFileInputChange}
              aria-label={t('expenses.import.dropzone')}
            />
          </div>

          <div className="mt-4 text-center">
            <a
              href={getImportTemplateUrl()}
              download="expenses-import-template.csv"
              className="text-sm text-brand-600 hover:text-brand-700 font-medium transition-colors"
            >
              {t('expenses.import.templateLink')}
            </a>
          </div>
        </div>
      ) : (
        /* ── Step 2: Preview ── */
        <div>
          <div className="mb-4 flex items-center gap-4 flex-wrap">
            <span className="text-sm font-medium text-emerald-700 bg-emerald-50 border border-emerald-200 rounded-lg px-3 py-1.5">
              {t('expenses.import.validRows', { count: preview.validCount })}
            </span>
            {preview.errorCount > 0 && (
              <span className="text-sm font-medium text-red-700 bg-red-50 border border-red-200 rounded-lg px-3 py-1.5">
                {t('expenses.import.errorRows', { count: preview.errorCount })}
              </span>
            )}
          </div>

          <div className="overflow-x-auto rounded-2xl border border-slate-200 shadow-card mb-6">
            <table className="w-full text-sm">
              <thead className="bg-surface-subtle">
                <tr>
                  <th className="px-3 py-2.5 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.import.columns.row')}</th>
                  <th className="px-3 py-2.5 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.table.date')}</th>
                  <th className="px-3 py-2.5 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.table.amount')}</th>
                  <th className="px-3 py-2.5 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.fields.currency')}</th>
                  <th className="px-3 py-2.5 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.fields.category')}</th>
                  <th className="px-3 py-2.5 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.fields.description')}</th>
                  <th className="px-3 py-2.5 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.import.columns.status')}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {preview.rows.map(row => (
                  <tr key={row.rowNumber} className={row.isValid ? '' : 'bg-red-50'}>
                    <td className="px-3 py-2.5 text-ink-mute">{row.rowNumber}</td>
                    <td className="px-3 py-2.5 font-mono text-xs">{row.dateDisplay ?? '—'}</td>
                    <td className="px-3 py-2.5">{row.amountDisplay ?? '—'}</td>
                    <td className="px-3 py-2.5">{row.currencyDisplay ?? '—'}</td>
                    <td className="px-3 py-2.5">{row.categoryDisplay || '—'}</td>
                    <td className="px-3 py-2.5 max-w-xs truncate">{row.descriptionDisplay || '—'}</td>
                    <td className="px-3 py-2.5">
                      {row.isValid ? (
                        <span className="text-emerald-600 font-medium">{t('expenses.import.columns.valid')}</span>
                      ) : (
                        <span className="text-red-600 text-xs">
                          {row.errors.map(e => t(`expenses.import.errors.${e}`, e)).join(', ')}
                        </span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="flex gap-3 justify-end">
            <button
              onClick={() => { setPreview(null); setError(null) }}
              className="px-4 py-2 text-sm font-medium rounded-xl border border-slate-200 hover:bg-slate-50 transition-colors"
            >
              {t('expenses.import.cancel')}
            </button>
            <button
              onClick={handleConfirm}
              disabled={confirming || preview.validCount === 0}
              className="px-4 py-2 text-sm font-medium rounded-xl bg-brand-600 hover:bg-brand-700 text-white transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {confirming
                ? t('expenses.actions.saving')
                : t('expenses.import.confirmButton', { count: preview.validCount })}
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
