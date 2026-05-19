import { useEffect, useState } from 'react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useTranslation } from 'react-i18next'
import { useExpensesData } from '@/features/expenses/ExpensesDataContext'
import { useFamilies } from '@/features/families/FamilyContext'
import TagInput from '@/features/tags/components/TagInput'
import FieldError from '@/components/FieldError'
import SubmitButton from '@/components/SubmitButton'
import { makeExpenseSchema, type ExpenseFormData } from '@/features/expenses/expense.schemas'
import type { Tag } from '@/features/tags/types/tag.type'
import type { ExpenseDto } from '@/features/expenses/types/expenses.type'

interface ExpenseFormProps {
  readonly initialValues?: ExpenseDto
  readonly onSubmit: (data: ExpenseFormData) => Promise<void>
  readonly isSubmitting: boolean
  readonly onCancel: () => void
}

function today(): string {
  return new Date().toISOString().slice(0, 10)
}

export default function ExpenseForm({ initialValues, onSubmit, isSubmitting, onCancel }: ExpenseFormProps) {
  const { t } = useTranslation()
  const { categories, currencies } = useExpensesData()
  const { families } = useFamilies()

  const schema = makeExpenseSchema(t)
  const {
    register,
    handleSubmit,
    control,
    watch,
    setValue,
    formState: { errors },
  } = useForm<ExpenseFormData>({
    resolver: zodResolver(schema),
    defaultValues: initialValues
      ? {
          amount: initialValues.amount,
          currencyId: initialValues.currency?.id,
          date: initialValues.date,
          categoryId: initialValues.category?.id,
          subcategoryId: initialValues.subcategory?.id,
          description: initialValues.description ?? '',
          tagIds: initialValues.tags.map(t => t.id),
          familyIds: undefined,
        }
      : {
          date: today(),
          description: '',
          tagIds: [],
          familyIds: [],
        },
  })

  const selectedCategoryId = watch('categoryId')
  const subcategories = categories.find(c => c.id === selectedCategoryId)?.subcategories ?? []

  useEffect(() => {
    if (!initialValues) {
      setValue('subcategoryId', undefined)
    }
  }, [selectedCategoryId, initialValues, setValue])

  const defaultFamily = families.find(f => f.isDefault)
  const nonDefaultFamilies = families.filter(f => !f.isDefault && !f.isArchived)

  const [checkedFamilyIds, setCheckedFamilyIds] = useState<Set<number>>(() => {
    if (initialValues) return new Set<number>()
    return defaultFamily ? new Set([defaultFamily.id]) : new Set()
  })

  useEffect(() => {
    setValue('familyIds', Array.from(checkedFamilyIds))
  }, [checkedFamilyIds, setValue])

  const [selectedTags, setSelectedTags] = useState<Tag[]>(
    initialValues ? initialValues.tags.map(t => ({ id: t.id, name: t.name })) : []
  )

  useEffect(() => {
    setValue('tagIds', selectedTags.map(t => t.id))
  }, [selectedTags, setValue])

  const handleFamilyToggle = (id: number) => {
    setCheckedFamilyIds(prev => {
      const next = new Set(prev)
      if (next.has(id)) next.delete(id)
      else next.add(id)
      return next
    })
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5" noValidate>
      {/* Amount + Currency row */}
      <div className="flex gap-4">
        <div className="flex-1">
          <label htmlFor="amount" className="field-label">
            {t('expenses.fields.amount')}
          </label>
          <input
            id="amount"
            type="number"
            step="0.01"
            min="0.01"
            className="field-input"
            aria-describedby="amount-error"
            aria-invalid={!!errors.amount}
            {...register('amount', { valueAsNumber: true })}
          />
          <FieldError id="amount-error" message={errors.amount?.message} />
        </div>

        <div className="w-36">
          <label htmlFor="currencyId" className="field-label">
            {t('expenses.fields.currency')}
          </label>
          <select
            id="currencyId"
            className="field-input"
            aria-describedby="currencyId-error"
            aria-invalid={!!errors.currencyId}
            {...register('currencyId', { valueAsNumber: true })}
          >
            <option value="">—</option>
            {currencies.map(c => (
              <option key={c.id} value={c.id}>
                {c.code}
              </option>
            ))}
          </select>
          <FieldError id="currencyId-error" message={errors.currencyId?.message} />
        </div>
      </div>

      {/* Date */}
      <div>
        <label htmlFor="date" className="field-label">
          {t('expenses.fields.date')}
        </label>
        <input
          id="date"
          type="date"
          className="field-input"
          aria-describedby="date-error"
          aria-invalid={!!errors.date}
          {...register('date')}
        />
        <FieldError id="date-error" message={errors.date?.message} />
      </div>

      {/* Category + Subcategory row */}
      <div className="flex gap-4">
        <div className="flex-1">
          <label htmlFor="categoryId" className="field-label">
            {t('expenses.fields.category')}
          </label>
          <select
            id="categoryId"
            className="field-input"
            {...register('categoryId', { valueAsNumber: true })}
          >
            <option value="">—</option>
            {categories.map(c => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
        </div>

        <div className="flex-1">
          <label htmlFor="subcategoryId" className="field-label">
            {t('expenses.fields.subcategory')}
          </label>
          <select
            id="subcategoryId"
            className="field-input"
            disabled={!selectedCategoryId || subcategories.length === 0}
            aria-describedby="subcategoryId-error"
            aria-invalid={!!errors.subcategoryId}
            {...register('subcategoryId', { valueAsNumber: true })}
          >
            <option value="">
              {selectedCategoryId ? '—' : t('expenses.fields.noCategorySelected')}
            </option>
            {subcategories.map(s => (
              <option key={s.id} value={s.id}>
                {s.name}
              </option>
            ))}
          </select>
          <FieldError id="subcategoryId-error" message={errors.subcategoryId?.message} />
        </div>
      </div>

      {/* Description */}
      <div>
        <label htmlFor="description" className="field-label">
          {t('expenses.fields.description')}
        </label>
        <textarea
          id="description"
          rows={3}
          className="field-input resize-none"
          maxLength={500}
          {...register('description')}
        />
      </div>

      {/* Tags */}
      <div>
        <label className="field-label">{t('expenses.fields.tags')}</label>
        <Controller
          name="tagIds"
          control={control}
          render={() => (
            <TagInput value={selectedTags} onChange={setSelectedTags} />
          )}
        />
      </div>

      {/* Families */}
      {(defaultFamily || nonDefaultFamilies.length > 0) && (
        <div>
          <label className="field-label">{t('expenses.fields.families')}</label>
          <div className="space-y-1.5">
            {defaultFamily && (
              <label className="flex items-center gap-2 text-sm text-ink-body cursor-not-allowed">
                <input
                  type="checkbox"
                  checked={checkedFamilyIds.has(defaultFamily.id)}
                  disabled
                  readOnly
                  className="h-4 w-4 rounded border-surface-border accent-brand-500"
                />
                {defaultFamily.name}
              </label>
            )}
            {nonDefaultFamilies.map(f => (
              <label key={f.id} className="flex items-center gap-2 text-sm text-ink-body cursor-pointer">
                <input
                  type="checkbox"
                  checked={checkedFamilyIds.has(f.id)}
                  onChange={() => handleFamilyToggle(f.id)}
                  className="h-4 w-4 rounded border-surface-border accent-brand-500 cursor-pointer"
                />
                {f.name}
              </label>
            ))}
          </div>
        </div>
      )}

      {initialValues?.modifiedAt && (
        <p className="text-xs text-ink-mute">
          {t('expenses.modifiedAt', {
            date: new Date(initialValues.modifiedAt).toLocaleDateString(),
            source: initialValues.modifiedFrom ?? '—',
          })}
        </p>
      )}

      <div className="flex gap-3 pt-1">
        <SubmitButton
          isSubmitting={isSubmitting}
          label={t('expenses.actions.save')}
          loadingLabel={t('expenses.actions.saving')}
        />
        <button
          type="button"
          onClick={onCancel}
          className="btn-secondary"
        >
          {t('expenses.actions.cancel')}
        </button>
      </div>
    </form>
  )
}
