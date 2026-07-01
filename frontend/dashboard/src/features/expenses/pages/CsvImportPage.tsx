import { createPortal, flushSync } from 'react-dom'
import { useEffect, useRef, useState } from 'react'
import { useNavigate, useBlocker } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { usePageTitle } from '@/hooks/usePageTitle'
import { useExpensesData } from '@/features/expenses/ExpensesDataContext'
import { useFamilies } from '@/features/families/FamilyContext'
import type { Family } from '@/features/families/types/family.type'
import type { Tag } from '@/features/tags/types/tag.type'
import {
  previewCsvImport,
  confirmCsvImport,
  validateCsvRows,
  getImportTemplateUrl,
  detectCsvHeaders,
} from '@/features/expenses/services/expensesApi.service'
import { updateDefaultCsvColumnMapping } from '@/features/settings/services/userConfigApi.service'
import {
  CSV_CANONICAL_FIELDS,
  type CsvImportPreviewDto,
  type CsvImportRowPreview,
  type CsvImportConfirmRowDto,
  type RawCsvRowDto,
  type CsvHeaderDetectionDto,
} from '@/features/expenses/types/expenses.type'

// ── Types ─────────────────────────────────────────────────────────────────────

type EditedFields = {
  date: string
  amount: string
  currencyCode: string
  category: string
  subcategory: string
  description: string
  tags: string[]
  families: string[]  // family ID strings
}

type DropPos = { top: number; left: number; width: number }
type SelectOption = { value: string; label: string }

// ── Portal helpers ────────────────────────────────────────────────────────────

function useDropdownPos(open: boolean) {
  const triggerRef = useRef<HTMLElement>(null)
  const [pos, setPos] = useState<DropPos | null>(null)
  function openAt(el: HTMLElement | null) {
    if (!el) return
    const r = el.getBoundingClientRect()
    setPos({ top: r.bottom + 2, left: r.left, width: Math.max(r.width, 160) })
  }
  useEffect(() => { if (!open) setPos(null) }, [open])
  return { triggerRef, pos, openAt }
}

// ── StringCombobox (with portal) ──────────────────────────────────────────────

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
  const inputRef = useRef<HTMLInputElement>(null)
  const listRef = useRef<HTMLUListElement>(null)
  const [pos, setPos] = useState<DropPos | null>(null)

  const filtered = (query.trim()
    ? options.filter(o =>
        o.label.toLowerCase().includes(query.toLowerCase()) ||
        o.value.toLowerCase().includes(query.toLowerCase()),
      )
    : options
  ).slice(0, 30)

  useEffect(() => { if (!open) setQuery(value) }, [value, open])

  useEffect(() => {
    if (!open) return
    const h = (e: MouseEvent) => {
      if (!inputRef.current?.contains(e.target as Node) && !listRef.current?.contains(e.target as Node)) {
        setOpen(false)
      }
    }
    document.addEventListener('mousedown', h)
    return () => document.removeEventListener('mousedown', h)
  }, [open])

  function handleFocus() {
    if (disabled) return
    if (inputRef.current) {
      const r = inputRef.current.getBoundingClientRect()
      setPos({ top: r.bottom + 2, left: r.left, width: Math.max(r.width, 160) })
    }
    setOpen(true)
    setQuery('')
  }

  return (
    <div className="relative min-w-0">
      <input
        ref={inputRef}
        type="text"
        aria-label={ariaLabel}
        value={open ? query : value}
        onChange={e => { setQuery(e.target.value); onChange(e.target.value); setOpen(true) }}
        onFocus={handleFocus}
        placeholder={placeholder ?? '—'}
        disabled={disabled}
        className={`w-full px-2 py-1 text-xs border rounded-lg bg-surface-card text-ink focus:outline-none focus:ring-1 focus:ring-brand-400 ${
          disabled ? 'opacity-40 cursor-not-allowed border-surface-border' : 'border-surface-border'
        }`}
      />
      {open && !disabled && pos && createPortal(
        <ul
          ref={listRef}
          style={{ position: 'fixed', top: pos.top, left: pos.left, width: pos.width, zIndex: 9999 }}
          className="max-h-40 overflow-y-auto bg-surface-card border border-surface-border rounded-lg shadow-xl text-xs"
        >
          {filtered.length === 0 ? (
            <li className="px-3 py-1.5 text-ink-mute">—</li>
          ) : (
            filtered.map(o => (
              <li
                key={o.value}
                onMouseDown={() => { onChange(o.value); setQuery(o.value); setOpen(false) }}
                className={`px-3 py-1.5 cursor-pointer hover:bg-surface-subtle text-ink whitespace-nowrap ${o.value === value ? 'font-semibold text-brand-600' : ''}`}
              >
                {o.label}
              </li>
            ))
          )}
        </ul>,
        document.body,
      )}
    </div>
  )
}

// ── TagChips (edit) ───────────────────────────────────────────────────────────

function TagChips({
  value,
  onChange,
  availableTags,
  'aria-label': ariaLabel,
}: {
  value: string[]
  onChange: (tags: string[]) => void
  availableTags: Tag[]
  'aria-label'?: string
}) {
  const [query, setQuery] = useState('')
  const [open, setOpen] = useState(false)
  const [pos, setPos] = useState<DropPos | null>(null)
  const inputRef = useRef<HTMLInputElement>(null)
  const listRef = useRef<HTMLUListElement>(null)

  const selectedSet = new Set(value)
  const filtered = availableTags
    .filter(t => !selectedSet.has(t.name) && t.name.toLowerCase().includes(query.toLowerCase()))
    .slice(0, 20)
  const showCreate = query.trim().length > 0 && !availableTags.some(t => t.name === query.trim()) && !selectedSet.has(query.trim())

  function openDropdown() {
    if (!inputRef.current) return
    const r = inputRef.current.getBoundingClientRect()
    setPos({ top: r.bottom + 2, left: r.left, width: Math.max(r.width, 160) })
    setOpen(true)
  }

  function add(name: string) {
    const t = name.trim()
    if (t && !selectedSet.has(t)) onChange([...value, t])
    setQuery('')
    setOpen(false)
  }

  function remove(name: string) { onChange(value.filter(t => t !== name)) }

  useEffect(() => {
    if (!open) return
    const h = (e: MouseEvent) => {
      if (!inputRef.current?.contains(e.target as Node) && !listRef.current?.contains(e.target as Node)) {
        setOpen(false); setQuery('')
      }
    }
    document.addEventListener('mousedown', h)
    return () => document.removeEventListener('mousedown', h)
  }, [open])

  return (
    <div className="flex flex-wrap gap-1 items-center min-w-[7rem] min-h-[1.75rem]">
      {value.map(tag => (
        <span key={tag} className="inline-flex items-center gap-0.5 px-1.5 py-0.5 bg-brand-50 text-brand-700 text-xs rounded border border-brand-200">
          {tag}
          <button type="button" onMouseDown={e => { e.preventDefault(); remove(tag) }} className="leading-none text-brand-400 hover:text-brand-700">×</button>
        </span>
      ))}
      <input
        ref={inputRef}
        type="text"
        aria-label={ariaLabel}
        value={query}
        onChange={e => { setQuery(e.target.value); openDropdown() }}
        onFocus={openDropdown}
        onKeyDown={e => {
          if (e.key === 'Enter' && query.trim()) { e.preventDefault(); add(query) }
          else if (e.key === 'Backspace' && !query && value.length > 0) remove(value[value.length - 1])
          else if (e.key === 'Escape') { setOpen(false); setQuery('') }
        }}
        placeholder={value.length === 0 ? 'tag…' : ''}
        className="flex-1 min-w-[3rem] px-1 py-0.5 text-xs outline-none border-b border-surface-border bg-transparent text-ink"
      />
      {open && pos && createPortal(
        <ul
          ref={listRef}
          style={{ position: 'fixed', top: pos.top, left: pos.left, width: pos.width, zIndex: 9999 }}
          className="bg-surface-card border border-surface-border rounded-lg shadow-xl text-xs max-h-40 overflow-y-auto"
        >
          {filtered.map(t => (
            <li key={t.id} onMouseDown={() => add(t.name)} className="px-3 py-1.5 cursor-pointer hover:bg-surface-subtle text-ink">{t.name}</li>
          ))}
          {showCreate && (
            <li onMouseDown={() => add(query.trim())} className="px-3 py-1.5 cursor-pointer hover:bg-brand-50 text-brand-600 font-medium">
              + "{query.trim()}"
            </li>
          )}
          {filtered.length === 0 && !showCreate && (
            <li className="px-3 py-1.5 text-ink-mute">—</li>
          )}
        </ul>,
        document.body,
      )}
    </div>
  )
}

// ── FamilyMultiSelect ─────────────────────────────────────────────────────────

function FamilyMultiSelect({
  value,
  onChange,
  families,
  'aria-label': ariaLabel,
}: {
  value: string[]    // family ID strings
  onChange: (ids: string[]) => void
  families: Family[]
  'aria-label'?: string
}) {
  const [open, setOpen] = useState(false)
  const [pos, setPos] = useState<DropPos | null>(null)
  const btnRef = useRef<HTMLButtonElement>(null)
  const listRef = useRef<HTMLUListElement>(null)

  const options = families.filter(f => !f.isArchived && !f.isDefault).map(f => ({ id: String(f.id), name: f.name }))
  const selectedSet = new Set(value)

  function toggle(id: string) {
    onChange(selectedSet.has(id) ? value.filter(v => v !== id) : [...value, id])
  }

  function openDropdown() {
    if (!btnRef.current) return
    const r = btnRef.current.getBoundingClientRect()
    setPos({ top: r.bottom + 2, left: r.left, width: Math.max(r.width, 160) })
    setOpen(true)
  }

  useEffect(() => {
    if (!open) return
    const h = (e: MouseEvent) => {
      if (!btnRef.current?.contains(e.target as Node) && !listRef.current?.contains(e.target as Node)) setOpen(false)
    }
    document.addEventListener('mousedown', h)
    return () => document.removeEventListener('mousedown', h)
  }, [open])

  const selectedNames = value.map(id => options.find(o => o.id === id)?.name ?? `#${id}`)

  return (
    <div className="flex flex-wrap gap-1 items-center min-w-[7rem]">
      {selectedNames.map((name, i) => (
        <span key={value[i]} className="inline-flex items-center gap-0.5 px-1.5 py-0.5 bg-sage-50 text-sage-700 text-xs rounded border border-sage-200">
          {name}
          <button type="button" onMouseDown={e => { e.preventDefault(); toggle(value[i]) }} className="leading-none text-sage-400 hover:text-sage-700">×</button>
        </span>
      ))}
      <button
        ref={btnRef}
        type="button"
        aria-label={ariaLabel}
        onMouseDown={e => { e.preventDefault(); openDropdown() }}
        className="px-1.5 py-0.5 text-xs text-ink-mute hover:text-brand-600 border border-dashed border-surface-border rounded hover:border-brand-400 transition-colors"
      >
        +
      </button>
      {open && pos && createPortal(
        <ul
          ref={listRef}
          style={{ position: 'fixed', top: pos.top, left: pos.left, width: pos.width, zIndex: 9999 }}
          className="bg-surface-card border border-surface-border rounded-lg shadow-xl text-xs max-h-40 overflow-y-auto"
        >
          {options.length === 0 ? (
            <li className="px-3 py-1.5 text-ink-mute">—</li>
          ) : (
            options.map(o => (
              <li
                key={o.id}
                onMouseDown={() => toggle(o.id)}
                className={`px-3 py-1.5 cursor-pointer hover:bg-surface-subtle text-ink flex items-center gap-2 ${selectedSet.has(o.id) ? 'font-semibold text-brand-600' : ''}`}
              >
                <span className="w-3 text-center">{selectedSet.has(o.id) ? '✓' : ''}</span>
                {o.name}
              </li>
            ))
          )}
        </ul>,
        document.body,
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
    tags: row.tagNames ?? [],
    families: row.familyIds?.map(String)
      ?? (row.familiesDisplay?.split(';').map(s => s.trim()).filter(Boolean) ?? []),
  }
}

function TagDisplay({ tags }: Readonly<{ tags: string[] }>) {
  if (tags.length === 0) return <span className="text-ink-mute">—</span>
  return (
    <div className="flex flex-wrap gap-0.5">
      {tags.map(t => (
        <span key={t} className="px-1.5 py-0.5 bg-surface-subtle text-ink text-xs rounded">{t}</span>
      ))}
    </div>
  )
}

function FamilyDisplay({ ids, families }: Readonly<{ ids: string[]; families: Family[] }>) {
  if (ids.length === 0) return <span className="text-ink-mute">—</span>
  const names = ids.map(id => families.find(f => String(f.id) === id)?.name ?? `#${id}`)
  return <span className="text-xs">{names.join(', ')}</span>
}

// ── Row component ─────────────────────────────────────────────────────────────

function ImportRow({
  row,
  editing,
  isValidating,
  fields,
  pending,
  currencyOptions,
  categoryOptions,
  subcategoryOptions,
  availableTags,
  userFamilies,
  onEdit,
  onSave,
  onCancel,
  onRemove,
  onPendingChange,
  wasEdited,
}: {
  row: CsvImportRowPreview
  editing: boolean
  isValidating: boolean
  fields: EditedFields
  pending: EditedFields
  currencyOptions: SelectOption[]
  categoryOptions: SelectOption[]
  subcategoryOptions: SelectOption[]
  availableTags: Tag[]
  userFamilies: Family[]
  onEdit: () => void
  onSave: () => void
  onCancel: () => void
  onRemove: () => void
  onPendingChange: (field: keyof EditedFields, value: string | string[]) => void
  wasEdited: boolean
}) {
  const { t } = useTranslation()
  const inputClass = 'w-full px-2 py-1 text-xs border border-surface-border rounded-lg bg-surface-card text-ink focus:outline-none focus:ring-1 focus:ring-brand-400'

  const rowClass = editing ? 'bg-amber-50 dark:bg-amber-900/20' : row.isValid ? '' : 'bg-red-50 dark:bg-red-900/20'

  return (
    <tr className={rowClass}>
      <td className="px-2 py-2 text-ink-mute text-xs">
        <span>{row.rowNumber}</span>
        {wasEdited && !editing && (
          <span className="ml-1 px-1 py-0.5 text-[10px] font-medium bg-amber-100 text-amber-700 rounded">
            {t('expenses.import.columns.edited')}
          </span>
        )}
      </td>

      {editing ? (
        <>
          <td className="px-1.5 py-1.5">
            <input
              type="date"
              value={pending.date}
              onChange={e => onPendingChange('date', e.target.value)}
              aria-label={`Row ${row.rowNumber} date`}
              className={`${inputClass} w-32`}
            />
          </td>
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
          <td className="px-1.5 py-1.5 w-24">
            <StringCombobox
              value={pending.currencyCode}
              onChange={v => onPendingChange('currencyCode', v.toUpperCase())}
              options={currencyOptions}
              placeholder="EUR"
              aria-label={`Row ${row.rowNumber} currency`}
            />
          </td>
          <td className="px-1.5 py-1.5 w-28">
            <StringCombobox
              value={pending.category}
              onChange={v => { onPendingChange('category', v); onPendingChange('subcategory', '') }}
              options={categoryOptions}
              aria-label={`Row ${row.rowNumber} category`}
            />
          </td>
          <td className="px-1.5 py-1.5 w-28">
            <StringCombobox
              value={pending.subcategory}
              onChange={v => onPendingChange('subcategory', v)}
              options={subcategoryOptions}
              disabled={!pending.category}
              aria-label={`Row ${row.rowNumber} subcategory`}
            />
          </td>
          <td className="px-1.5 py-1.5">
            <input
              type="text"
              maxLength={500}
              value={pending.description}
              onChange={e => onPendingChange('description', e.target.value)}
              aria-label={`Row ${row.rowNumber} description`}
              className={`${inputClass} w-36`}
            />
          </td>
          <td className="px-1.5 py-1.5">
            <TagChips
              value={pending.tags}
              onChange={v => onPendingChange('tags', v)}
              availableTags={availableTags}
              aria-label={`Row ${row.rowNumber} tags`}
            />
          </td>
          <td className="px-1.5 py-1.5">
            <FamilyMultiSelect
              value={pending.families}
              onChange={v => onPendingChange('families', v)}
              families={userFamilies}
              aria-label={`Row ${row.rowNumber} families`}
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
          <td className="px-2 py-2"><TagDisplay tags={fields.tags} /></td>
          <td className="px-2 py-2"><FamilyDisplay ids={fields.families} families={userFamilies} /></td>
        </>
      )}

      <td className="px-2 py-2 min-w-[7rem]">
        {isValidating ? (
          <span className="flex items-center gap-1 text-ink-faint text-xs">
            <svg className="animate-spin h-3.5 w-3.5" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
            </svg>
            {t('expenses.loading', 'Loading…')}
          </span>
        ) : row.isValid && !editing ? (
          <span className="flex items-center gap-1 text-emerald-600 text-xs font-medium">
            <svg className="h-3.5 w-3.5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            {t('expenses.import.columns.valid')}
          </span>
        ) : editing ? (
          <span className="flex items-center gap-1 text-amber-600 text-xs font-medium">
            <svg className="h-3.5 w-3.5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M15.232 5.232l3.536 3.536M9 13l6.5-6.5 3.5 3.5L13 16.5H9V13z" />
            </svg>
            {t('expenses.import.columns.editing')}
          </span>
        ) : (
          <span className="flex items-start gap-1 text-red-600 text-xs">
            <svg className="h-3.5 w-3.5 shrink-0 mt-px" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            {row.errors.map(e => t(`expenses.import.errors.${e}`, e)).join(', ')}
          </span>
        )}
      </td>

      <td className="px-2 py-2 whitespace-nowrap">
        {editing ? (
          <div className="flex items-center gap-1.5">
            <button
              onClick={onSave}
              aria-label={`Save row ${row.rowNumber}`}
              title={t('expenses.import.saveRow')}
              className="p-1 rounded-lg text-emerald-600 hover:bg-emerald-50 transition-colors"
            >
              <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
              </svg>
            </button>
            <button
              onClick={onCancel}
              aria-label={`Cancel editing row ${row.rowNumber}`}
              title={t('expenses.import.cancelRow')}
              className="p-1 rounded-lg text-ink-mute hover:bg-surface-subtle transition-colors"
            >
              <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        ) : (
          <div className="flex items-center gap-1">
            <button
              onClick={onEdit}
              aria-label={`Edit row ${row.rowNumber}`}
              title={t('expenses.import.editRow')}
              className="p-1 rounded-lg text-ink-mute hover:text-brand-600 hover:bg-brand-50 transition-colors"
              disabled={isValidating}
            >
              <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
              </svg>
            </button>
            <button
              onClick={onRemove}
              aria-label={`Remove row ${row.rowNumber}`}
              title={t('expenses.import.removeRow', 'Remove row')}
              className="p-1 rounded-lg text-ink-mute hover:text-red-600 hover:bg-red-50 transition-colors"
              disabled={isValidating}
            >
              <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
              </svg>
            </button>
          </div>
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

  const { currencies, categories, tags: availableTags, refresh } = useExpensesData()
  const { families: userFamilies } = useFamilies()

  const fileInputRef = useRef<HTMLInputElement>(null)
  const [dragging, setDragging] = useState(false)
  const [preview, setPreview] = useState<CsvImportPreviewDto | null>(null)

  const [editedRows, setEditedRows] = useState<Record<number, EditedFields>>({})
  const [pendingEdits, setPendingEdits] = useState<Record<number, EditedFields>>({})
  const [editingRows, setEditingRows] = useState<Set<number>>(new Set())
  const [validatingRows, setValidatingRows] = useState<Set<number>>(new Set())

  const [loadingPreview, setLoadingPreview] = useState(false)
  const [confirming, setConfirming] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [userEditedRows, setUserEditedRows] = useState<Set<number>>(new Set())
  const [sortErrors, setSortErrors] = useState(false)

  const [pendingFile, setPendingFile] = useState<File | null>(null)
  const [headerDetection, setHeaderDetection] = useState<CsvHeaderDetectionDto | null>(null)
  const [columnMapping, setColumnMapping] = useState<Record<string, string>>({})
  const [rememberMapping, setRememberMapping] = useState(true)
  const [detecting, setDetecting] = useState(false)
  const [mappingSubmitting, setMappingSubmitting] = useState(false)

  const blocker = useBlocker(preview !== null)

  useEffect(() => {
    if (!preview) return
    const handler = (e: BeforeUnloadEvent) => {
      e.preventDefault()
      e.returnValue = ''
    }
    window.addEventListener('beforeunload', handler)
    return () => window.removeEventListener('beforeunload', handler)
  }, [preview])

  // ── Combobox options ────────────────────────────────────────────────────────

  const currencyOptions: SelectOption[] = currencies.map(c => ({ value: c.code, label: `${c.code} — ${c.name}` }))
  const categoryOptions: SelectOption[] = categories.map(c => ({ value: c.name, label: c.name }))

  function getSubcategoryOptions(categoryName: string): SelectOption[] {
    const cat = categories.find(c => c.name.toLowerCase() === categoryName.toLowerCase())
    return cat?.subcategories.map(s => ({ value: s.name, label: s.name })) ?? []
  }

  // ── Edit state helpers ──────────────────────────────────────────────────────

  function getDisplayFields(rowNumber: number): EditedFields {
    if (editedRows[rowNumber]) return editedRows[rowNumber]
    const row = preview?.rows.find(r => r.rowNumber === rowNumber)
    return row ? rowToEdited(row) : { date: '', amount: '', currencyCode: '', category: '', subcategory: '', description: '', tags: [], families: [] }
  }

  function startEdit(rowNumber: number) {
    setPendingEdits(prev => ({ ...prev, [rowNumber]: { ...getDisplayFields(rowNumber) } }))
    setEditingRows(prev => new Set([...prev, rowNumber]))
  }

  async function saveAndValidateRow(rowNumber: number) {
    const fields = pendingEdits[rowNumber]
    if (!fields) return
    setEditedRows(prev => ({ ...prev, [rowNumber]: fields }))
    setEditingRows(prev => { const s = new Set(prev); s.delete(rowNumber); return s })
    setPendingEdits(prev => { const { [rowNumber]: _, ...rest } = prev; return rest })
    setValidatingRows(prev => new Set([...prev, rowNumber]))

    const rawRow: RawCsvRowDto = {
      rowNumber,
      date: fields.date || null,
      amount: fields.amount || null,
      currencyCode: fields.currencyCode || null,
      category: fields.category || null,
      subcategory: fields.subcategory || null,
      description: fields.description || null,
      tags: fields.tags.length > 0 ? fields.tags.join(';') : null,
      families: fields.families.length > 0
        ? fields.families.map(id => userFamilies.find(f => String(f.id) === id)?.name ?? id).join(';')
        : null,
    }

    const res = await validateCsvRows([rawRow])
    setValidatingRows(prev => { const s = new Set(prev); s.delete(rowNumber); return s })
    if (res.ok && res.data) {
      const validated = res.data.rows[0]
      if (!validated) return
      setPreview(prev => {
        if (!prev) return prev
        const rows = prev.rows.map(r => r.rowNumber === rowNumber ? validated : r)
        return { ...prev, rows, validCount: rows.filter(r => r.isValid).length, errorCount: rows.filter(r => !r.isValid).length }
      })
      setEditedRows(prev => { const { [rowNumber]: _, ...rest } = prev; return rest })
      setUserEditedRows(prev => new Set([...prev, rowNumber]))
    } else {
      setError(res.error ?? t('expenses.errors.loadFailed'))
    }
  }

  function handleRemove(rowNumber: number) {
    setPreview(prev => {
      if (!prev) return prev
      const rows = prev.rows.filter(r => r.rowNumber !== rowNumber)
      return { ...prev, rows, totalRows: rows.length, validCount: rows.filter(r => r.isValid).length, errorCount: rows.filter(r => !r.isValid).length }
    })
    setEditedRows(prev => { const { [rowNumber]: _, ...rest } = prev; return rest })
    setPendingEdits(prev => { const { [rowNumber]: _, ...rest } = prev; return rest })
    setEditingRows(prev => { const s = new Set(prev); s.delete(rowNumber); return s })
    setValidatingRows(prev => { const s = new Set(prev); s.delete(rowNumber); return s })
    setUserEditedRows(prev => { const s = new Set(prev); s.delete(rowNumber); return s })
  }

  function cancelEdit(rowNumber: number) {
    setEditingRows(prev => { const s = new Set(prev); s.delete(rowNumber); return s })
    setPendingEdits(prev => { const { [rowNumber]: _, ...rest } = prev; return rest })
  }

  function handlePendingChange(rowNumber: number, field: keyof EditedFields, value: string | string[]) {
    setPendingEdits(prev => ({
      ...prev,
      [rowNumber]: { ...prev[rowNumber], [field]: value },
    }))
  }

  // ── File handling ───────────────────────────────────────────────────────────

  const MAX_FILE_SIZE = 1 * 1024 * 1024 // 1 MB — must match backend

  function resetImportState() {
    setEditedRows({})
    setPendingEdits({})
    setEditingRows(new Set())
    setSortErrors(false)
    setUserEditedRows(new Set())
  }

  async function handleFile(file: File) {
    setError(null)
    if (file.size > MAX_FILE_SIZE) {
      setError(t('expenses.import.errors.IMPORT_FILE_TOO_LARGE'))
      return
    }
    if (!file.name.toLowerCase().endsWith('.csv')) {
      setError(t('expenses.import.errors.INVALID_FILE_TYPE'))
      return
    }
    setLoadingPreview(true)
    const res = await previewCsvImport(file)
    setLoadingPreview(false)
    if (res.ok) {
      setPreview(res.data!)
      resetImportState()
      return
    }

    if (res.rawCode?.startsWith('MISSING_HEADERS')) {
      setPendingFile(file)
      setDetecting(true)
      const detectRes = await detectCsvHeaders(file)
      setDetecting(false)
      if (detectRes.ok && detectRes.data) {
        setHeaderDetection(detectRes.data)
        const seeded: Record<string, string> = {}
        for (const header of detectRes.data.rawHeaders) {
          seeded[header] = detectRes.data.suggestedMapping[header] ?? 'ignore'
        }
        setColumnMapping(seeded)
      } else {
        setError(detectRes.error ?? t('expenses.errors.loadFailed'))
      }
      return
    }

    setError(res.error ?? t('expenses.errors.loadFailed'))
  }

  function cancelMapping() {
    setPendingFile(null)
    setHeaderDetection(null)
    setColumnMapping({})
    setRememberMapping(true)
  }

  async function handleMappingContinue() {
    if (!pendingFile || !headerDetection) return
    setMappingSubmitting(true)
    const confirmedMapping = Object.fromEntries(
      Object.entries(columnMapping).filter(([, canonical]) => canonical !== 'ignore'),
    )

    const res = await previewCsvImport(pendingFile, confirmedMapping)
    setMappingSubmitting(false)
    if (!res.ok) {
      setError(res.error ?? t('expenses.errors.loadFailed'))
      return
    }

    if (rememberMapping) {
      // Non-blocking: import flow must not fail if saving the default mapping fails.
      updateDefaultCsvColumnMapping(confirmedMapping).catch(() => undefined)
    }

    setPreview(res.data!)
    resetImportState()
    setPendingFile(null)
    setHeaderDetection(null)
    setColumnMapping({})
    setRememberMapping(true)
  }

  const missingRequiredFields = headerDetection
    ? CSV_CANONICAL_FIELDS.filter(
        f => ['date', 'amount', 'currency_code'].includes(f) && !Object.values(columnMapping).includes(f),
      )
    : []

  function handleFileInputChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (file) handleFile(file)
    e.target.value = ''
  }

  function handleDrop(e: React.DragEvent<HTMLDivElement>) {
    e.preventDefault(); setDragging(false)
    const file = e.dataTransfer.files?.[0]
    if (file) handleFile(file)
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
      flushSync(() => setPreview(null))
      refresh()
      navigate('/expenses')
    }
    else setError(res.error ?? t('expenses.errors.saveFailed'))
  }

  // ── Render ──────────────────────────────────────────────────────────────────

  const hasEdits = editingRows.size > 0 || validatingRows.size > 0

  const displayRows = preview
    ? (sortErrors ? [...preview.rows].sort((a, b) => Number(a.isValid) - Number(b.isValid)) : preview.rows)
    : []

  return (
    <div className="max-w-full mx-auto px-4 sm:px-6 py-8">
      <div className="mb-6 flex items-center gap-3">
        <button onClick={() => navigate('/expenses')} className="text-sm text-ink-mute hover:text-ink transition-colors" aria-label="Back">
          ← {t('expenses.actions.cancel')}
        </button>
        <h1 className="text-2xl font-semibold text-ink tracking-tight">{t('expenses.import.pageTitle')}</h1>
      </div>

      {error && (
        <div className="mb-4 rounded-xl bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-700">{error}</div>
      )}

      {!preview && headerDetection ? (
        <div className="max-w-3xl bg-surface-card shadow-card border border-surface-border rounded-2xl p-8">
          <h2 className="text-base font-semibold text-ink mb-1">{t('expenses.import.mapping.title')}</h2>
          <p className="text-sm text-ink-mute mb-5">{t('expenses.import.mapping.description')}</p>

          <table className="w-full text-sm mb-4">
            <thead>
              <tr>
                <th className="text-left text-xs font-semibold text-ink-mute uppercase tracking-wide pb-2">{t('expenses.import.mapping.rawColumn')}</th>
                <th className="text-left text-xs font-semibold text-ink-mute uppercase tracking-wide pb-2">{t('expenses.import.mapping.mapsTo')}</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-surface-border">
              {headerDetection.rawHeaders.map(header => {
                const usedElsewhere = new Set(
                  Object.entries(columnMapping)
                    .filter(([h]) => h !== header)
                    .map(([, v]) => v)
                    .filter(v => v !== 'ignore'),
                )
                const options = [
                  { value: 'ignore', label: t('expenses.import.mapping.ignore') },
                  ...CSV_CANONICAL_FIELDS
                    .filter(f => !usedElsewhere.has(f))
                    .map(f => ({ value: f, label: t(`expenses.table.${f}`, t(`expenses.fields.${f}`, f)) })),
                ]
                const isSuggested = headerDetection.suggestedMapping[header] === columnMapping[header]
                return (
                  <tr key={header}>
                    <td className="py-2 pr-4 font-mono text-xs text-ink">
                      {header}
                      {isSuggested && columnMapping[header] !== 'ignore' && (
                        <span className="ml-2 px-1.5 py-0.5 text-[10px] font-medium bg-amber-100 text-amber-700 rounded">
                          {t('expenses.import.mapping.suggested')}
                        </span>
                      )}
                    </td>
                    <td className="py-2">
                      <select
                        aria-label={t('expenses.import.mapping.mapColumnLabel', { header })}
                        value={columnMapping[header] ?? 'ignore'}
                        onChange={e => setColumnMapping(prev => ({ ...prev, [header]: e.target.value }))}
                        className="w-full max-w-xs text-sm border border-surface-border rounded-lg px-3 py-1.5 bg-surface-card text-ink focus:outline-none focus:ring-2 focus:ring-brand-400"
                      >
                        {options.map(o => (
                          <option key={o.value} value={o.value}>{o.label}</option>
                        ))}
                      </select>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>

          {missingRequiredFields.length > 0 && (
            <div role="alert" className="mb-4 rounded-xl bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-700">
              {t('expenses.import.mapping.missingRequired', {
                fields: missingRequiredFields.map(f => t(`expenses.table.${f}`, t(`expenses.fields.${f}`, f))).join(', '),
              })}
            </div>
          )}

          <label className="flex items-center gap-2 mb-5 cursor-pointer">
            <input
              type="checkbox"
              checked={rememberMapping}
              onChange={e => setRememberMapping(e.target.checked)}
              className="accent-brand-600 h-4 w-4"
            />
            <span className="text-sm text-ink">{t('expenses.import.mapping.remember')}</span>
          </label>
          <p className="text-xs text-ink-mute -mt-3 mb-5">{t('expenses.import.mapping.rememberHint')}</p>

          <div className="flex gap-3 justify-end">
            <button
              onClick={cancelMapping}
              className="px-4 py-2 text-sm font-medium rounded-xl border border-surface-border text-ink hover:bg-surface-subtle transition-colors"
            >
              {t('expenses.import.cancel')}
            </button>
            <button
              onClick={handleMappingContinue}
              disabled={mappingSubmitting || missingRequiredFields.length > 0}
              className="px-4 py-2 text-sm font-medium rounded-xl bg-brand-600 hover:bg-brand-700 text-white transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {t('expenses.import.mapping.continue')}
            </button>
          </div>
        </div>
      ) : !preview ? (
        <div className="max-w-2xl bg-surface-card shadow-card border border-surface-border rounded-2xl p-8">
          <div
            role="button" tabIndex={0} aria-label={t('expenses.import.dropzone')}
            onDragOver={e => { e.preventDefault(); setDragging(true) }}
            onDragLeave={() => setDragging(false)}
            onDrop={handleDrop}
            onClick={() => fileInputRef.current?.click()}
            onKeyDown={e => e.key === 'Enter' && fileInputRef.current?.click()}
            className={`border-2 border-dashed rounded-xl p-12 text-center transition-colors ${loadingPreview || detecting ? 'pointer-events-none opacity-75 cursor-default' : 'cursor-pointer'} ${dragging ? 'border-brand-600 bg-brand-50' : 'border-surface-border hover:border-brand-400'}`}
          >
            {loadingPreview || detecting ? (
              <div className="flex flex-col items-center gap-3">
                <svg className="animate-spin h-8 w-8 text-brand-500" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
                </svg>
                <p className="text-sm text-ink-mute">{t('expenses.import.uploading')}</p>
              </div>
            ) : (
              <>
                <svg className="mx-auto h-10 w-10 text-ink-faint mb-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" />
                </svg>
                <p className="text-sm text-ink-mute">{t('expenses.import.dropzone')}</p>
              </>
            )}
            <input ref={fileInputRef} type="file" accept=".csv" className="hidden" onChange={handleFileInputChange} aria-label={t('expenses.import.dropzone')} />
          </div>
          <div className="mt-6 border-t border-surface-border pt-5 flex flex-col sm:flex-row items-start sm:items-center gap-3">
            <p className="text-xs text-ink-mute flex-1">
              {t('expenses.import.templateDescription')}
            </p>
            <a
              href={getImportTemplateUrl()}
              download="expenses-import-template.csv"
              className="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium rounded-xl border border-surface-border bg-surface-card text-ink hover:bg-surface-subtle transition-colors shrink-0"
            >
              <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" />
              </svg>
              {t('expenses.import.templateLink')}
            </a>
          </div>
        </div>
      ) : (
        <div>
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
            <button
              onClick={() => setSortErrors(s => !s)}
              className="ml-auto flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium rounded-lg border border-surface-border text-ink-mute hover:text-ink hover:bg-surface-subtle transition-colors"
            >
              <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M3 4h13M3 8h9m-9 4h6m4 0l4-4m0 0l4 4m-4-4v12" />
              </svg>
              {sortErrors ? t('expenses.import.sortNatural') : t('expenses.import.sortErrors')}
            </button>
          </div>

          <div className="overflow-x-auto rounded-2xl border border-surface-border shadow-card mb-5">
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
              <tbody className="divide-y divide-surface-border">
                {displayRows.map(row => {
                  const isEditing = editingRows.has(row.rowNumber)
                  const pending = pendingEdits[row.rowNumber] ?? getDisplayFields(row.rowNumber)
                  const displayFields = getDisplayFields(row.rowNumber)
                  const subOptions = getSubcategoryOptions(isEditing ? pending.category : displayFields.category)
                  return (
                    <ImportRow
                      key={row.rowNumber}
                      row={row}
                      editing={isEditing}
                      isValidating={validatingRows.has(row.rowNumber)}
                      fields={displayFields}
                      pending={pending}
                      currencyOptions={currencyOptions}
                      categoryOptions={categoryOptions}
                      subcategoryOptions={subOptions}
                      availableTags={availableTags}
                      userFamilies={userFamilies}
                      wasEdited={userEditedRows.has(row.rowNumber)}
                      onEdit={() => startEdit(row.rowNumber)}
                      onSave={() => saveAndValidateRow(row.rowNumber)}
                      onCancel={() => cancelEdit(row.rowNumber)}
                      onRemove={() => handleRemove(row.rowNumber)}
                      onPendingChange={(field, value) => handlePendingChange(row.rowNumber, field, value)}
                    />
                  )
                })}
              </tbody>
            </table>
          </div>

          <div className="flex gap-3 justify-end flex-wrap">
            <button
              onClick={() => { setPreview(null); setEditedRows({}); setPendingEdits({}); setEditingRows(new Set()); setError(null); setUserEditedRows(new Set()); setSortErrors(false) }}
              className="px-4 py-2 text-sm font-medium rounded-xl border border-surface-border text-ink hover:bg-surface-subtle transition-colors"
            >
              {t('expenses.import.cancel')}
            </button>
            <button
              onClick={handleConfirm}
              disabled={confirming || preview.validCount === 0 || hasEdits}
              className="px-4 py-2 text-sm font-medium rounded-xl bg-brand-600 hover:bg-brand-700 text-white transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {confirming ? t('expenses.actions.saving') : t('expenses.import.confirmButton', { count: preview.validCount })}
            </button>
          </div>
        </div>
      )}

      {blocker.state === 'blocked' && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-surface-card rounded-2xl shadow-card border border-surface-border w-full max-w-sm mx-4 p-6">
            <h2 className="text-base font-semibold text-ink mb-2">
              {t('expenses.import.leaveTitle')}
            </h2>
            <p className="text-sm text-ink-mute mb-5">
              {t('expenses.import.leaveWarning')}
            </p>
            <div className="flex gap-3 justify-end">
              <button
                onClick={() => blocker.reset?.()}
                className="px-4 py-2 text-sm font-medium rounded-xl border border-surface-border text-ink hover:bg-surface-subtle transition-colors"
              >
                {t('expenses.import.stayHere')}
              </button>
              <button
                onClick={() => blocker.proceed?.()}
                className="px-4 py-2 text-sm font-medium rounded-xl bg-red-600 text-white hover:bg-red-700 transition-colors"
              >
                {t('expenses.import.leaveConfirm')}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
