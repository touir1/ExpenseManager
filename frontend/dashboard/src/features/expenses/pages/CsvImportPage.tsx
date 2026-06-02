import { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { usePageTitle } from '@/hooks/usePageTitle'
import { useExpensesData } from '@/features/expenses/ExpensesDataContext'
import {
  previewCsvImport,
  confirmCsvImport,
  validateCsvRows,
  getImportTemplateUrl,
} from '@/features/expenses/services/expensesApi.service'
import type {
  CsvImportPreviewDto,
  CsvImportRowPreview,
  CsvImportConfirmRowDto,
  RawCsvRowDto,
} from '@/features/expenses/types/expenses.type'

// ── Types ─────────────────────────────────────────────────────────────────────

type EditedFields = {
  date: string
  amount: string
  currencyCode: string
  category: string
  subcategory: string
  description: string
  tags: string
  families: string
}

type SelectOption = { value: string; label: string }

// ── StringCombobox ────────────────────────────────────────────────────────────

function StringCombobox({
  value,
  onChange,
  options,
  placeholder,
  disabled,
  'aria-label': ariaLabel,
}: {
  value: string
  onChange: (v: string) => void
  options: SelectOption[]
  placeholder?: string
  disabled?: boolean
  'aria-label'?: string
}) {
  const [open, setOpen] = useState(false)
  const [query, setQuery] = useState(value)
  const containerRef = useRef<HTMLDivElement>(null)

  const filtered = (query.trim()
    ? options.filter(o =>
        o.label.toLowerCase().includes(query.toLowerCase()) ||
        o.value.toLowerCase().includes(query.toLowerCase()),
      )
    : options
  ).slice(0, 30)

  // Sync when value changes externally (e.g. subcategory reset)
  useEffect(() => {
    if (!open) setQuery(value)
  }, [value, open])

  useEffect(() => {
    if (!open) return
    const handler = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) setOpen(false)
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [open])

  return (
    <div ref={containerRef} className="relative min-w-0">
      <input
        type="text"
        aria-label={ariaLabel}
        value={open ? query : value}
        onChange={e => { setQuery(e.target.value); onChange(e.target.value); setOpen(true) }}
        onFocus={() => { if (!disabled) { setOpen(true); setQuery('') } }}
        placeholder={placeholder ?? '—'}
        disabled={disabled}
        className={`w-full px-2 py-1 text-xs border rounded-lg bg-white focus:outline-none focus:ring-1 focus:ring-brand-400 ${
          disabled ? 'opacity-40 cursor-not-allowed border-slate-200' : 'border-slate-300'
        }`}
      />
      {open && !disabled && (
        <ul
          role="listbox"
          className="absolute z-30 top-full mt-0.5 left-0 min-w-[10rem] max-h-40 overflow-y-auto bg-white border border-slate-200 rounded-lg shadow-lg text-xs"
        >
          {filtered.length === 0 ? (
            <li className="px-3 py-1.5 text-ink-mute">—</li>
          ) : (
            filtered.map(o => (
              <li
                key={o.value}
                role="option"
                aria-selected={o.value === value}
                onMouseDown={() => { onChange(o.value); setQuery(o.value); setOpen(false) }}
                className={`px-3 py-1.5 cursor-pointer hover:bg-slate-50 whitespace-nowrap ${o.value === value ? 'font-semibold text-brand-600' : ''}`}
              >
                {o.label}
              </li>
            ))
          )}
        </ul>
      )}
    </div>
  )
}

// ── Helpers ───────────────────────────────────────────────────────────────────

function rowToEdited(row: CsvImportRowPreview): EditedFields {
  return {
    date: row.dateDisplay ?? '',
    amount: row.amountDisplay ?? '',
    currencyCode: row.currencyDisplay ?? '',
    category: row.categoryDisplay ?? '',
    subcategory: row.subcategoryDisplay ?? '',
    description: row.descriptionDisplay ?? '',
    tags: row.tagNames?.join(';') ?? '',
    families: row.familiesDisplay ?? (row.familyIds?.join(';') ?? ''),
  }
}

// ── Row component ─────────────────────────────────────────────────────────────

function ImportRow({
  row,
  editing,
  fields,
  pending,
  currencyOptions,
  categoryOptions,
  subcategoryOptions,
  onEdit,
  onSave,
  onCancel,
  onPendingChange,
}: {
  row: CsvImportRowPreview
  editing: boolean
  fields: EditedFields       // saved fields (display when not editing)
  pending: EditedFields      // in-progress (used when editing)
  currencyOptions: SelectOption[]
  categoryOptions: SelectOption[]
  subcategoryOptions: SelectOption[]
  onEdit: () => void
  onSave: () => void
  onCancel: () => void
  onPendingChange: (field: keyof EditedFields, value: string) => void
}) {
  const { t } = useTranslation()
  const inputClass = 'w-full px-2 py-1 text-xs border border-slate-300 rounded-lg bg-white focus:outline-none focus:ring-1 focus:ring-brand-400'

  const rowClass = editing
    ? 'bg-amber-50'
    : row.isValid
      ? ''
      : 'bg-red-50'

  return (
    <tr className={rowClass}>
      <td className="px-2 py-2 text-ink-mute text-xs">{row.rowNumber}</td>

      {editing ? (
        <>
          {/* Date */}
          <td className="px-1.5 py-1.5">
            <input
              type="date"
              value={pending.date}
              onChange={e => onPendingChange('date', e.target.value)}
              aria-label={`Row ${row.rowNumber} date`}
              className={`${inputClass} w-32`}
            />
          </td>
          {/* Amount */}
          <td className="px-1.5 py-1.5">
            <input
              type="text"
              value={pending.amount}
              onChange={e => onPendingChange('amount', e.target.value)}
              placeholder="0.00"
              aria-label={`Row ${row.rowNumber} amount`}
              className={`${inputClass} w-20`}
            />
          </td>
          {/* Currency */}
          <td className="px-1.5 py-1.5 w-24">
            <StringCombobox
              value={pending.currencyCode}
              onChange={v => onPendingChange('currencyCode', v.toUpperCase())}
              options={currencyOptions}
              placeholder="EUR"
              aria-label={`Row ${row.rowNumber} currency`}
            />
          </td>
          {/* Category */}
          <td className="px-1.5 py-1.5 w-28">
            <StringCombobox
              value={pending.category}
              onChange={v => { onPendingChange('category', v); onPendingChange('subcategory', '') }}
              options={categoryOptions}
              aria-label={`Row ${row.rowNumber} category`}
            />
          </td>
          {/* Subcategory */}
          <td className="px-1.5 py-1.5 w-28">
            <StringCombobox
              value={pending.subcategory}
              onChange={v => onPendingChange('subcategory', v)}
              options={subcategoryOptions}
              disabled={!pending.category}
              aria-label={`Row ${row.rowNumber} subcategory`}
            />
          </td>
          {/* Description */}
          <td className="px-1.5 py-1.5">
            <input
              type="text"
              value={pending.description}
              onChange={e => onPendingChange('description', e.target.value)}
              aria-label={`Row ${row.rowNumber} description`}
              className={`${inputClass} w-36`}
            />
          </td>
          {/* Tags */}
          <td className="px-1.5 py-1.5">
            <input
              type="text"
              value={pending.tags}
              onChange={e => onPendingChange('tags', e.target.value)}
              placeholder="tag1;tag2"
              aria-label={`Row ${row.rowNumber} tags`}
              className={`${inputClass} w-28`}
            />
          </td>
          {/* Families */}
          <td className="px-1.5 py-1.5">
            <input
              type="text"
              value={pending.families}
              onChange={e => onPendingChange('families', e.target.value)}
              placeholder="id1;id2"
              aria-label={`Row ${row.rowNumber} families`}
              className={`${inputClass} w-24`}
            />
          </td>
        </>
      ) : (
        <>
          <td className="px-2 py-2 font-mono text-xs whitespace-nowrap">{fields.date || '—'}</td>
          <td className="px-2 py-2 text-xs whitespace-nowrap">{fields.amount || '—'}</td>
          <td className="px-2 py-2 text-xs font-medium whitespace-nowrap">{fields.currencyCode || '—'}</td>
          <td className="px-2 py-2 text-xs whitespace-nowrap">{fields.category || '—'}</td>
          <td className="px-2 py-2 text-xs whitespace-nowrap text-ink-mute">{fields.subcategory || '—'}</td>
          <td className="px-2 py-2 text-xs max-w-[9rem] truncate">{fields.description || '—'}</td>
          <td className="px-2 py-2 text-xs text-ink-mute whitespace-nowrap">{fields.tags || '—'}</td>
          <td className="px-2 py-2 text-xs text-ink-mute whitespace-nowrap">{fields.families || '—'}</td>
        </>
      )}

      {/* Status */}
      <td className="px-2 py-2 min-w-[7rem]">
        {row.isValid && !editing ? (
          <span className="text-emerald-600 text-xs font-medium">{t('expenses.import.columns.valid')}</span>
        ) : editing ? (
          <span className="text-amber-600 text-xs font-medium">{t('expenses.import.columns.editing')}</span>
        ) : (
          <span className="text-red-600 text-xs">
            {row.errors.map(e => t(`expenses.import.errors.${e}`, e)).join(', ')}
          </span>
        )}
      </td>

      {/* Actions */}
      <td className="px-2 py-2 whitespace-nowrap">
        {editing ? (
          <div className="flex items-center gap-1.5">
            <button
              onClick={onSave}
              aria-label={`Save row ${row.rowNumber}`}
              title={t('expenses.import.saveRow')}
              className="p-1 rounded-lg text-emerald-600 hover:bg-emerald-50 transition-colors"
            >
              {/* checkmark */}
              <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
              </svg>
            </button>
            <button
              onClick={onCancel}
              aria-label={`Cancel editing row ${row.rowNumber}`}
              title={t('expenses.import.cancelRow')}
              className="p-1 rounded-lg text-ink-mute hover:bg-slate-100 transition-colors"
            >
              {/* X */}
              <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        ) : (
          <button
            onClick={onEdit}
            aria-label={`Edit row ${row.rowNumber}`}
            title={t('expenses.import.editRow')}
            className="p-1 rounded-lg text-ink-mute hover:text-brand-600 hover:bg-brand-50 transition-colors"
          >
            {/* pencil */}
            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
            </svg>
          </button>
        )}
      </td>
    </tr>
  )
}

// ── Main Page ─────────────────────────────────────────────────────────────────

export default function CsvImportPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  usePageTitle(t('expenses.import.pageTitle'))

  const { currencies, categories } = useExpensesData()

  const fileInputRef = useRef<HTMLInputElement>(null)
  const [dragging, setDragging] = useState(false)
  const [preview, setPreview] = useState<CsvImportPreviewDto | null>(null)

  // Saved edits (used when building re-validate payload)
  const [editedRows, setEditedRows] = useState<Record<number, EditedFields>>({})
  // In-progress edits (only while row is in edit mode)
  const [pendingEdits, setPendingEdits] = useState<Record<number, EditedFields>>({})
  // Which rows are currently in edit mode
  const [editingRows, setEditingRows] = useState<Set<number>>(new Set())

  const [loadingPreview, setLoadingPreview] = useState(false)
  const [revalidating, setRevalidating] = useState(false)
  const [confirming, setConfirming] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // ── Options for comboboxes ──────────────────────────────────────────────────

  const currencyOptions: SelectOption[] = currencies.map(c => ({
    value: c.code,
    label: `${c.code} — ${c.name}`,
  }))

  const categoryOptions: SelectOption[] = categories.map(c => ({
    value: c.name,
    label: c.name,
  }))

  function getSubcategoryOptions(categoryName: string): SelectOption[] {
    const cat = categories.find(c => c.name.toLowerCase() === categoryName.toLowerCase())
    return cat?.subcategories.map(s => ({ value: s.name, label: s.name })) ?? []
  }

  // ── Edit state helpers ──────────────────────────────────────────────────────

  function getDisplayFields(rowNumber: number): EditedFields {
    return editedRows[rowNumber] ?? (preview?.rows.find(r => r.rowNumber === rowNumber)
      ? rowToEdited(preview.rows.find(r => r.rowNumber === rowNumber)!)
      : { date: '', amount: '', currencyCode: '', category: '', subcategory: '', description: '', tags: '', families: '' })
  }

  function startEdit(rowNumber: number) {
    const fields = getDisplayFields(rowNumber)
    setPendingEdits(prev => ({ ...prev, [rowNumber]: { ...fields } }))
    setEditingRows(prev => new Set([...prev, rowNumber]))
  }

  function saveEdit(rowNumber: number) {
    setEditedRows(prev => ({ ...prev, [rowNumber]: pendingEdits[rowNumber] }))
    setEditingRows(prev => { const s = new Set(prev); s.delete(rowNumber); return s })
    setPendingEdits(prev => { const { [rowNumber]: _, ...rest } = prev; return rest })
  }

  function cancelEdit(rowNumber: number) {
    setEditingRows(prev => { const s = new Set(prev); s.delete(rowNumber); return s })
    setPendingEdits(prev => { const { [rowNumber]: _, ...rest } = prev; return rest })
  }

  function handlePendingChange(rowNumber: number, field: keyof EditedFields, value: string) {
    setPendingEdits(prev => ({
      ...prev,
      [rowNumber]: { ...prev[rowNumber], [field]: value },
    }))
  }

  // ── File handling ───────────────────────────────────────────────────────────

  async function handleFile(file: File) {
    setError(null)
    setLoadingPreview(true)
    const res = await previewCsvImport(file)
    setLoadingPreview(false)
    if (res.ok) {
      setPreview(res.data!)
      setEditedRows({})
      setPendingEdits({})
      setEditingRows(new Set())
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

  // ── Re-validate ─────────────────────────────────────────────────────────────

  async function handleRevalidate() {
    if (!preview) return
    setError(null)

    // Auto-save any in-progress edits before validating
    const mergedEdits = { ...editedRows }
    for (const [rn, fields] of Object.entries(pendingEdits)) {
      mergedEdits[Number(rn)] = fields
    }

    setRevalidating(true)
    const rows: RawCsvRowDto[] = preview.rows.map(row => {
      const fields = mergedEdits[row.rowNumber] ?? rowToEdited(row)
      return {
        rowNumber: row.rowNumber,
        date: fields.date || null,
        amount: fields.amount || null,
        currencyCode: fields.currencyCode || null,
        category: fields.category || null,
        subcategory: fields.subcategory || null,
        description: fields.description || null,
        tags: fields.tags || null,
        families: fields.families || null,
      }
    })

    const res = await validateCsvRows(rows)
    setRevalidating(false)
    if (res.ok) {
      setPreview(res.data!)
      setEditedRows({})
      setPendingEdits({})
      setEditingRows(new Set())
    } else {
      setError(res.error ?? t('expenses.errors.loadFailed'))
    }
  }

  // ── Confirm ─────────────────────────────────────────────────────────────────

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
        description: r.descriptionDisplay,
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

  // ── Render ──────────────────────────────────────────────────────────────────

  const hasEdits = Object.keys(editedRows).length > 0 || editingRows.size > 0

  return (
    <div className="max-w-full mx-auto px-4 sm:px-6 py-8">
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
        <div className="max-w-2xl bg-white shadow-card border border-slate-200 rounded-2xl p-8">
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
        /* ── Step 2: Preview + Edit ── */
        <div>
          {/* Summary row */}
          <div className="mb-3 flex items-center gap-3 flex-wrap">
            <span className="text-sm font-medium text-emerald-700 bg-emerald-50 border border-emerald-200 rounded-lg px-3 py-1.5">
              {t('expenses.import.validRows', { count: preview.validCount })}
            </span>
            {preview.errorCount > 0 && (
              <span className="text-sm font-medium text-red-700 bg-red-50 border border-red-200 rounded-lg px-3 py-1.5">
                {t('expenses.import.errorRows', { count: preview.errorCount })}
              </span>
            )}
            <span className="text-xs text-ink-mute ml-1">{t('expenses.import.editHint')}</span>
          </div>

          {/* Table */}
          <div className="overflow-x-auto rounded-2xl border border-slate-200 shadow-card mb-5">
            <table className="w-full text-sm">
              <thead className="bg-surface-subtle">
                <tr>
                  <th className="px-2 py-2.5 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide w-10">#</th>
                  <th className="px-2 py-2.5 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.table.date')}</th>
                  <th className="px-2 py-2.5 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.table.amount')}</th>
                  <th className="px-2 py-2.5 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.fields.currency')}</th>
                  <th className="px-2 py-2.5 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.fields.category')}</th>
                  <th className="px-2 py-2.5 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.fields.subcategory')}</th>
                  <th className="px-2 py-2.5 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.fields.description')}</th>
                  <th className="px-2 py-2.5 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.fields.tags')}</th>
                  <th className="px-2 py-2.5 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.fields.families')}</th>
                  <th className="px-2 py-2.5 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.import.columns.status')}</th>
                  <th className="px-2 py-2.5 w-20"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {preview.rows.map(row => {
                  const isEditing = editingRows.has(row.rowNumber)
                  const pending = pendingEdits[row.rowNumber] ?? getDisplayFields(row.rowNumber)
                  const displayFields = getDisplayFields(row.rowNumber)
                  const subOptions = getSubcategoryOptions(isEditing ? pending.category : displayFields.category)

                  return (
                    <ImportRow
                      key={row.rowNumber}
                      row={row}
                      editing={isEditing}
                      fields={displayFields}
                      pending={pending}
                      currencyOptions={currencyOptions}
                      categoryOptions={categoryOptions}
                      subcategoryOptions={subOptions}
                      onEdit={() => startEdit(row.rowNumber)}
                      onSave={() => saveEdit(row.rowNumber)}
                      onCancel={() => cancelEdit(row.rowNumber)}
                      onPendingChange={(field, value) => handlePendingChange(row.rowNumber, field, value)}
                    />
                  )
                })}
              </tbody>
            </table>
          </div>

          {/* Action bar */}
          <div className="flex gap-3 justify-end flex-wrap">
            <button
              onClick={() => { setPreview(null); setEditedRows({}); setPendingEdits({}); setEditingRows(new Set()); setError(null) }}
              className="px-4 py-2 text-sm font-medium rounded-xl border border-slate-200 hover:bg-slate-50 transition-colors"
            >
              {t('expenses.import.cancel')}
            </button>
            {(preview.errorCount > 0 || hasEdits) && (
              <button
                onClick={handleRevalidate}
                disabled={revalidating}
                className="px-4 py-2 text-sm font-medium rounded-xl border border-brand-600 text-brand-600 hover:bg-brand-50 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {revalidating ? t('expenses.loading', 'Loading…') : t('expenses.import.revalidate')}
              </button>
            )}
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
