import { useState, useEffect } from 'react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useTranslation } from 'react-i18next'
import { useExpensesData } from '@/features/expenses/ExpensesDataContext'
import { useFamilies } from '@/features/families/FamilyContext'
import { FormCombobox } from '@/components/FormCombobox'
import type { ComboOption } from '@/components/FormCombobox'
import TagInput from '@/features/tags/components/TagInput'
import FieldError from '@/components/FieldError'
import SubmitButton from '@/components/SubmitButton'
import { makeExpenseSchema, type ExpenseFormData } from '@/features/expenses/expense.schemas'
import { formatAmountDisplay, parseAmountInput, sanitizeAmountInputChars } from '@/features/expenses/utils/amountFormat'
import type { Tag } from '@/features/tags/types/tag.type'
import type { ExpenseDto } from '@/features/expenses/types/expenses.type'

interface ExpenseFormProps {
  readonly initialValues?: ExpenseDto
  readonly onSubmit: (data: ExpenseFormData) => Promise<void>
  readonly onSaveAndAddAnother?: (data: ExpenseFormData) => Promise<void>
  readonly isSubmitting: boolean
  readonly onCancel: () => void
}

function today(): string {
  return new Date().toISOString().slice(0, 10)
}

interface AmountInputProps {
  readonly id: string
  readonly value: number | undefined
  readonly onChange: (value: number | undefined) => void
  readonly onFieldBlur: () => void
  readonly ariaDescribedBy: string
  readonly ariaInvalid: boolean
}

function AmountInput({ id, value, onChange, onFieldBlur, ariaDescribedBy, ariaInvalid }: AmountInputProps) {
  const [isFocused, setIsFocused] = useState(false)
  const [displayValue, setDisplayValue] = useState(() => (value != null ? formatAmountDisplay(value) : ''))

  useEffect(() => {
    if (!isFocused) {
      setDisplayValue(value != null ? formatAmountDisplay(value) : '')
    }
  }, [value, isFocused])

  return (
    <input
      id={id}
      type="text"
      inputMode="decimal"
      className="field-input"
      aria-describedby={ariaDescribedBy}
      aria-invalid={ariaInvalid}
      value={displayValue}
      onFocus={() => {
        setIsFocused(true)
        setDisplayValue(value != null ? String(value) : '')
      }}
      onChange={e => {
        const sanitized = sanitizeAmountInputChars(e.target.value)
        setDisplayValue(sanitized)
        onChange(parseAmountInput(sanitized))
      }}
      onBlur={() => {
        setIsFocused(false)
        onFieldBlur()
      }}
    />
  )
}

export default function ExpenseForm({ initialValues, onSubmit, onSaveAndAddAnother, isSubmitting, onCancel }: ExpenseFormProps) {
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
    reset,
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

  const nonDefaultFamilies = families.filter(f => !f.isDefault && !f.isArchived)
  const defaultFamily = families.find(f => f.isDefault)

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

  const handleSaveAndAddAnother = async (data: ExpenseFormData) => {
    if (!onSaveAndAddAnother) return
    await onSaveAndAddAnother(data)
    reset({ date: today(), description: '', tagIds: [], familyIds: defaultFamily ? [defaultFamily.id] : [] })
    setSelectedTags([])
    setCheckedFamilyIds(defaultFamily ? new Set([defaultFamily.id]) : new Set())
  }

  const currencyOptions: ComboOption[] = currencies.map(c => ({ value: c.id, label: c.code }))
  const categoryOptions: ComboOption[] = categories.map(c => ({ value: c.id, label: c.name }))
  const subcategoryOptions: ComboOption[] = subcategories.map(s => ({ value: s.id, label: s.name }))

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
      <div className="grid grid-cols-2 gap-x-5 gap-y-4">
        {/* Left column: financial fields */}
        <div className="space-y-4">
          {/* Amount + Currency */}
          <div className="flex gap-3">
            <div className="flex-1">
              <label htmlFor="amount" className="field-label">
                {t('expenses.fields.amount')}
              </label>
              <Controller
                name="amount"
                control={control}
                render={({ field }) => (
                  <AmountInput
                    id="amount"
                    value={field.value as number | undefined}
                    onChange={field.onChange}
                    onFieldBlur={field.onBlur}
                    ariaDescribedBy="amount-error"
                    ariaInvalid={!!errors.amount}
                  />
                )}
              />
              <FieldError id="amount-error" message={errors.amount?.message} />
            </div>

            <div className="w-28">
              <label htmlFor="currencyId" className="field-label">
                {t('expenses.fields.currency')}
              </label>
              <Controller
                name="currencyId"
                control={control}
                render={({ field }) => (
                  <FormCombobox
                    id="currencyId"
                    value={field.value as number | undefined}
                    onChange={v => field.onChange(v)}
                    options={currencyOptions}
                    aria-describedby="currencyId-error"
                    aria-invalid={!!errors.currencyId}
                  />
                )}
              />
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

          {/* Category */}
          <div>
            <label htmlFor="categoryId" className="field-label">
              {t('expenses.fields.category')}
            </label>
            <Controller
              name="categoryId"
              control={control}
              render={({ field }) => (
                <FormCombobox
                  id="categoryId"
                  value={field.value as number | undefined}
                  onChange={v => field.onChange(v)}
                  options={categoryOptions}
                />
              )}
            />
          </div>

          {/* Subcategory (conditional) */}
          {subcategories.length > 0 && (
            <div>
              <label htmlFor="subcategoryId" className="field-label">
                {t('expenses.fields.subcategory')}
              </label>
              <Controller
                name="subcategoryId"
                control={control}
                render={({ field }) => (
                  <FormCombobox
                    id="subcategoryId"
                    value={field.value as number | undefined}
                    onChange={v => field.onChange(v)}
                    options={subcategoryOptions}
                    aria-describedby="subcategoryId-error"
                    aria-invalid={!!errors.subcategoryId}
                  />
                )}
              />
              <FieldError id="subcategoryId-error" message={errors.subcategoryId?.message} />
            </div>
          )}
        </div>

        {/* Right column: details */}
        <div className="space-y-4">
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

          {/* Non-default families */}
          {nonDefaultFamilies.length > 0 && (
            <div>
              <label className="field-label">{t('expenses.fields.families')}</label>
              <div className="space-y-1.5">
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
        </div>
      </div>

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
        {onSaveAndAddAnother && (
          <button
            type="button"
            disabled={isSubmitting}
            onClick={handleSubmit(handleSaveAndAddAnother)}
            className="btn-secondary disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {t('expenses.actions.saveAndAddAnother')}
          </button>
        )}
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
