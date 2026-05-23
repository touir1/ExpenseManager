import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useExpensesData } from '@/features/expenses/ExpensesDataContext'
import type { ExpenseFilter } from '@/features/expenses/types/expenses.type'
import type { Tag } from '@/features/tags/types/tag.type'

interface ExpenseFiltersProps {
  readonly filter: ExpenseFilter
  readonly onApply: (filter: ExpenseFilter) => void
}

interface ComboOption {
  value: number
  label: string
}

interface FilterComboboxProps {
  id: string
  value: number | undefined
  options: ComboOption[]
  onChange: (value: number | undefined) => void
}

function FilterCombobox({ id, value, options, onChange }: FilterComboboxProps) {
  const [open, setOpen] = useState(false)
  const [query, setQuery] = useState('')
  const containerRef = useRef<HTMLDivElement>(null)

  const selectedLabel = options.find(o => o.value === value)?.label ?? ''
  const filtered = query
    ? options.filter(o => o.label.toLowerCase().includes(query.toLowerCase()))
    : options

  useEffect(() => {
    if (!open) return
    const handler = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setOpen(false)
        setQuery('')
      }
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [open])

  return (
    <div ref={containerRef} className="relative">
      <input
        id={id}
        type="text"
        autoComplete="off"
        className="field-input"
        value={open ? query : selectedLabel}
        placeholder={open ? '' : '—'}
        onFocus={() => {
          setOpen(true)
          setQuery('')
        }}
        onChange={e => setQuery(e.target.value)}
      />
      {open && (
        <ul
          role="listbox"
          className="absolute z-30 w-full mt-1 bg-surface-card border border-surface-border rounded-lg shadow-lg max-h-48 overflow-y-auto"
        >
          <li
            role="option"
            aria-selected={value === undefined}
            className="px-3 py-1.5 text-sm cursor-pointer hover:bg-surface-subtle text-text-muted"
            onMouseDown={() => {
              onChange(undefined)
              setOpen(false)
              setQuery('')
            }}
          >
            —
          </li>
          {filtered.map(o => (
            <li
              key={o.value}
              role="option"
              aria-selected={o.value === value}
              className={`px-3 py-1.5 text-sm cursor-pointer hover:bg-surface-subtle ${o.value === value ? 'font-semibold' : ''}`}
              onMouseDown={() => {
                onChange(o.value)
                setOpen(false)
                setQuery('')
              }}
            >
              {o.label}
            </li>
          ))}
          {filtered.length === 0 && (
            <li className="px-3 py-1.5 text-sm text-text-muted">—</li>
          )}
        </ul>
      )}
    </div>
  )
}

interface FilterTagComboboxProps {
  id: string
  value: number[]
  options: Tag[]
  onChange: (value: number[]) => void
}

function FilterTagCombobox({ id, value, options, onChange }: FilterTagComboboxProps) {
  const [open, setOpen] = useState(false)
  const [query, setQuery] = useState('')
  const containerRef = useRef<HTMLDivElement>(null)

  const filtered = query
    ? options.filter(o => o.name.toLowerCase().includes(query.toLowerCase()))
    : options

  const displayValue = value.length === 0
    ? ''
    : options.filter(o => value.includes(o.id)).map(o => o.name).join(', ')

  useEffect(() => {
    if (!open) return
    const handler = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setOpen(false)
        setQuery('')
      }
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [open])

  const toggle = (id: number) => {
    onChange(value.includes(id) ? value.filter(v => v !== id) : [...value, id])
  }

  return (
    <div ref={containerRef} className="relative">
      <input
        id={id}
        type="text"
        autoComplete="off"
        className="field-input"
        value={open ? query : displayValue}
        placeholder={open ? '' : '—'}
        onFocus={() => {
          setOpen(true)
          setQuery('')
        }}
        onChange={e => setQuery(e.target.value)}
      />
      {open && (
        <ul
          role="listbox"
          aria-multiselectable="true"
          className="absolute z-30 w-full mt-1 bg-surface-card border border-surface-border rounded-lg shadow-lg max-h-48 overflow-y-auto"
        >
          {filtered.map(o => {
            const selected = value.includes(o.id)
            return (
              <li
                key={o.id}
                role="option"
                aria-selected={selected}
                className={`px-3 py-1.5 text-sm cursor-pointer hover:bg-surface-subtle flex items-center gap-2 ${selected ? 'font-semibold' : ''}`}
                onMouseDown={() => toggle(o.id)}
              >
                <span className={`h-3.5 w-3.5 rounded border flex-shrink-0 flex items-center justify-center ${selected ? 'bg-brand-600 border-brand-600' : 'border-surface-border'}`}>
                  {selected && (
                    <svg className="h-2.5 w-2.5 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={3}>
                      <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                    </svg>
                  )}
                </span>
                {o.name}
              </li>
            )
          })}
          {filtered.length === 0 && (
            <li className="px-3 py-1.5 text-sm text-text-muted">—</li>
          )}
        </ul>
      )}
    </div>
  )
}

export default function ExpenseFilters({ filter, onApply }: ExpenseFiltersProps) {
  const { t } = useTranslation()
  const { categories, currencies, tags = [] } = useExpensesData()
  const [open, setOpen] = useState(false)
  const panelRef = useRef<HTMLDivElement>(null)

  const [local, setLocal] = useState<ExpenseFilter>(filter)

  const selectedCategory = categories.find(c => c.id === local.categoryId)
  const subcategories = selectedCategory?.subcategories ?? []

  const categoryOptions: ComboOption[] = categories.map(c => ({ value: c.id, label: c.name }))
  const subcategoryOptions: ComboOption[] = subcategories.map(s => ({ value: s.id, label: s.name }))
  const currencyOptions: ComboOption[] = currencies.map(c => ({ value: c.id, label: c.code }))

  useEffect(() => {
    if (!open) return
    const handler = (e: MouseEvent) => {
      if (panelRef.current && !panelRef.current.contains(e.target as Node)) {
        setOpen(false)
      }
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [open])

  const set = <K extends keyof ExpenseFilter>(key: K, value: ExpenseFilter[K]) => {
    setLocal(prev => ({ ...prev, [key]: value || undefined }))
  }

  const handleApply = () => {
    onApply({ ...local, page: 1 })
    setOpen(false)
  }

  const handleReset = () => {
    const empty: ExpenseFilter = {}
    setLocal(empty)
    onApply(empty)
    setOpen(false)
  }

  const handleCategoryChange = (id: number | undefined) => {
    setLocal(prev => ({ ...prev, categoryId: id, subcategoryId: undefined }))
  }

  return (
    <div className="relative" ref={panelRef}>
      <button
        type="button"
        onClick={() => setOpen(o => !o)}
        aria-expanded={open}
        aria-controls="filter-panel"
        className="flex items-center gap-1.5 text-sm font-semibold px-3 py-1.5 rounded-lg border border-surface-border bg-surface-card hover:bg-surface-subtle transition-colors duration-150"
      >
        <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
          <path strokeLinecap="round" strokeLinejoin="round" d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2a1 1 0 01-.293.707L13 13.414V19a1 1 0 01-.553.894l-4 2A1 1 0 017 21v-7.586L3.293 6.707A1 1 0 013 6V4z" />
        </svg>
        {t('expenses.filters.toggle')}
      </button>

      {open && (
        <div
          id="filter-panel"
          role="region"
          aria-label={t('expenses.filters.title')}
          className="absolute right-0 top-full mt-2 z-20 w-80 bg-surface-card border border-surface-border rounded-2xl p-5 space-y-4"
          style={{ boxShadow: '0 8px 20px -10px rgba(30,20,10,0.3)' }}
        >
          {/* Date range */}
          <div className="flex gap-3">
            <div className="flex-1 min-w-0">
              <label htmlFor="filter-dateFrom" className="field-label">
                {t('expenses.filters.dateFrom')}
              </label>
              <input
                id="filter-dateFrom"
                type="date"
                className="field-input"
                value={local.dateFrom ?? ''}
                onChange={e => set('dateFrom', e.target.value || undefined)}
              />
            </div>
            <div className="flex-1 min-w-0">
              <label htmlFor="filter-dateTo" className="field-label">
                {t('expenses.filters.dateTo')}
              </label>
              <input
                id="filter-dateTo"
                type="date"
                className="field-input"
                value={local.dateTo ?? ''}
                onChange={e => set('dateTo', e.target.value || undefined)}
              />
            </div>
          </div>

          {/* Category + Subcategory */}
          <div>
            <label htmlFor="filter-category" className="field-label">
              {t('expenses.fields.category')}
            </label>
            <FilterCombobox
              id="filter-category"
              value={local.categoryId}
              options={categoryOptions}
              onChange={handleCategoryChange}
            />
          </div>

          {subcategories.length > 0 && (
            <div>
              <label htmlFor="filter-subcategory" className="field-label">
                {t('expenses.fields.subcategory')}
              </label>
              <FilterCombobox
                id="filter-subcategory"
                value={local.subcategoryId}
                options={subcategoryOptions}
                onChange={id => set('subcategoryId', id)}
              />
            </div>
          )}

          {/* Currency */}
          <div>
            <label htmlFor="filter-currency" className="field-label">
              {t('expenses.fields.currency')}
            </label>
            <FilterCombobox
              id="filter-currency"
              value={local.currencyId}
              options={currencyOptions}
              onChange={id => set('currencyId', id)}
            />
          </div>

          {/* Tags */}
          {tags.length > 0 && (
            <div>
              <label htmlFor="filter-tags" className="field-label">
                {t('expenses.filters.tags')}
              </label>
              <FilterTagCombobox
                id="filter-tags"
                value={local.tagIds ?? []}
                options={tags}
                onChange={ids => setLocal(prev => ({ ...prev, tagIds: ids.length > 0 ? ids : undefined }))}
              />
            </div>
          )}

          {/* Amount range */}
          <div className="flex gap-3">
            <div className="flex-1 min-w-0">
              <label htmlFor="filter-amountMin" className="field-label">
                {t('expenses.filters.amountMin')}
              </label>
              <input
                id="filter-amountMin"
                type="number"
                min="0"
                step="0.01"
                className="field-input"
                value={local.amountMin ?? ''}
                onChange={e => set('amountMin', e.target.value ? Number(e.target.value) : undefined)}
              />
            </div>
            <div className="flex-1 min-w-0">
              <label htmlFor="filter-amountMax" className="field-label">
                {t('expenses.filters.amountMax')}
              </label>
              <input
                id="filter-amountMax"
                type="number"
                min="0"
                step="0.01"
                className="field-input"
                value={local.amountMax ?? ''}
                onChange={e => set('amountMax', e.target.value ? Number(e.target.value) : undefined)}
              />
            </div>
          </div>

          {/* Description */}
          <div>
            <label htmlFor="filter-description" className="field-label">
              {t('expenses.fields.description')}
            </label>
            <input
              id="filter-description"
              type="text"
              className="field-input"
              value={local.description ?? ''}
              onChange={e => set('description', e.target.value || undefined)}
            />
          </div>

          {/* Actions */}
          <div className="flex gap-3 pt-1">
            <button
              type="button"
              onClick={handleApply}
              className="btn-primary flex-1"
            >
              {t('expenses.filters.apply')}
            </button>
            <button
              type="button"
              onClick={handleReset}
              className="flex-1 text-sm font-semibold px-3 py-1.5 rounded-lg border border-surface-border hover:bg-surface-subtle transition-colors duration-150"
            >
              {t('expenses.filters.reset')}
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
