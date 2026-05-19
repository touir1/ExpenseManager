import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useExpensesData } from '@/features/expenses/ExpensesDataContext'
import type { ExpenseFilter } from '@/features/expenses/types/expenses.type'

interface ExpenseFiltersProps {
  readonly filter: ExpenseFilter
  readonly onApply: (filter: ExpenseFilter) => void
}

export default function ExpenseFilters({ filter, onApply }: ExpenseFiltersProps) {
  const { t } = useTranslation()
  const { categories, currencies } = useExpensesData()
  const [open, setOpen] = useState(false)

  const [local, setLocal] = useState<ExpenseFilter>(filter)

  const selectedCategory = categories.find(c => c.id === local.categoryId)
  const subcategories = selectedCategory?.subcategories ?? []

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

  const handleCategoryChange = (val: string) => {
    const id = val ? Number(val) : undefined
    setLocal(prev => ({ ...prev, categoryId: id, subcategoryId: undefined }))
  }

  return (
    <div className="relative">
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
            <div className="flex-1">
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
            <div className="flex-1">
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
            <select
              id="filter-category"
              className="field-input"
              value={local.categoryId ?? ''}
              onChange={e => handleCategoryChange(e.target.value)}
            >
              <option value="">—</option>
              {categories.map(c => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
          </div>

          {subcategories.length > 0 && (
            <div>
              <label htmlFor="filter-subcategory" className="field-label">
                {t('expenses.fields.subcategory')}
              </label>
              <select
                id="filter-subcategory"
                className="field-input"
                value={local.subcategoryId ?? ''}
                onChange={e => set('subcategoryId', e.target.value ? Number(e.target.value) : undefined)}
              >
                <option value="">—</option>
                {subcategories.map(s => (
                  <option key={s.id} value={s.id}>
                    {s.name}
                  </option>
                ))}
              </select>
            </div>
          )}

          {/* Currency */}
          <div>
            <label htmlFor="filter-currency" className="field-label">
              {t('expenses.fields.currency')}
            </label>
            <select
              id="filter-currency"
              className="field-input"
              value={local.currencyId ?? ''}
              onChange={e => set('currencyId', e.target.value ? Number(e.target.value) : undefined)}
            >
              <option value="">—</option>
              {currencies.map(c => (
                <option key={c.id} value={c.id}>
                  {c.code}
                </option>
              ))}
            </select>
          </div>

          {/* Amount range */}
          <div className="flex gap-3">
            <div className="flex-1">
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
            <div className="flex-1">
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
