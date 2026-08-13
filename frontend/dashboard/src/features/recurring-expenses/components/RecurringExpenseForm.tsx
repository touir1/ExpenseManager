import { useState, useEffect } from 'react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useTranslation } from 'react-i18next'
import { useExpensesData } from '@/features/expenses/ExpensesDataContext'
import { useFamilies } from '@/features/families/FamilyContext'
import { FormCombobox } from '@/components/FormCombobox'
import type { ComboOption } from '@/components/FormCombobox'
import FieldError from '@/components/FieldError'
import SubmitButton from '@/components/SubmitButton'
import { formatAmountDisplay, parseAmountInput, sanitizeAmountInputChars } from '@/features/expenses/utils/amountFormat'
import { makeRecurringExpenseSchema, type RecurringExpenseFormData } from '@/features/recurring-expenses/recurringExpense.schemas'
import type { RecurringExpenseDto } from '@/features/dashboard/types/dashboard.type'

interface RecurringExpenseFormProps {
  readonly initialValues?: RecurringExpenseDto
  readonly onSubmit: (data: RecurringExpenseFormData) => Promise<void>
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

export default function RecurringExpenseForm({ initialValues, onSubmit, isSubmitting, onCancel }: RecurringExpenseFormProps) {
  const { t } = useTranslation()
  const { categories, currencies } = useExpensesData()
  const { families } = useFamilies()

  const schema = makeRecurringExpenseSchema(t)
  const {
    register,
    handleSubmit,
    control,
    watch,
    setValue,
    formState: { errors },
  } = useForm<RecurringExpenseFormData>({
    resolver: zodResolver(schema),
    defaultValues: initialValues
      ? {
          description: initialValues.description,
          amount: initialValues.amount,
          currencyId: initialValues.currencyId,
          categoryId: initialValues.categoryId,
          subcategoryId: initialValues.subcategoryId ?? undefined,
          familyId: initialValues.familyId ?? undefined,
          frequencyId: initialValues.frequencyId,
          nextDueDate: initialValues.nextDueDate,
          autoCreate: initialValues.autoCreate,
          isActive: initialValues.isActive,
        }
      : {
          description: '',
          nextDueDate: today(),
          frequencyId: 2,
          autoCreate: false,
          isActive: true,
        },
  })

  const selectedCategoryId = watch('categoryId')
  const subcategories = categories.find(c => c.id === selectedCategoryId)?.subcategories ?? []

  useEffect(() => {
    if (!initialValues) {
      setValue('subcategoryId', undefined)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedCategoryId])

  const currencyOptions: ComboOption[] = currencies.map(c => ({ value: c.id, label: c.code }))
  const categoryOptions: ComboOption[] = categories.map(c => ({ value: c.id, label: c.name }))
  const subcategoryOptions: ComboOption[] = subcategories.map(s => ({ value: s.id, label: s.name }))
  const activeFamilies = families.filter(f => !f.isArchived)

  const handleFormSubmit = async (data: RecurringExpenseFormData) => {
    await onSubmit(data)
  }

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4" noValidate>
      {/* Description */}
      <div>
        <label htmlFor="description" className="field-label">
          {t('recurringExpenses.fields.description')}
        </label>
        <input
          id="description"
          type="text"
          className="field-input"
          maxLength={500}
          aria-describedby="description-error"
          aria-invalid={!!errors.description}
          {...register('description')}
        />
        <FieldError id="description-error" message={errors.description?.message} />
      </div>

      {/* Amount + Currency */}
      <div className="flex gap-3">
        <div className="flex-1">
          <label htmlFor="amount" className="field-label">
            {t('recurringExpenses.fields.amount')}
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
            {t('recurringExpenses.fields.currency')}
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

      {/* Category */}
      <div>
        <label htmlFor="categoryId" className="field-label">
          {t('recurringExpenses.fields.category')}
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
              aria-describedby="categoryId-error"
              aria-invalid={!!errors.categoryId}
            />
          )}
        />
        <FieldError id="categoryId-error" message={errors.categoryId?.message} />
      </div>

      {/* Subcategory (conditional) */}
      {subcategories.length > 0 && (
        <div>
          <label htmlFor="subcategoryId" className="field-label">
            {t('recurringExpenses.fields.subcategory')}
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

      {/* Family */}
      {activeFamilies.length > 0 && (
        <div>
          <label htmlFor="familyId" className="field-label">
            {t('recurringExpenses.fields.family')}
          </label>
          <select id="familyId" className="field-input" {...register('familyId', { valueAsNumber: true })}>
            <option value="">—</option>
            {activeFamilies.map(f => (
              <option key={f.id} value={f.id}>{f.name}</option>
            ))}
          </select>
        </div>
      )}

      {/* Frequency */}
      <div>
        <label htmlFor="frequencyId" className="field-label">
          {t('recurringExpenses.fields.frequency')}
        </label>
        <select
          id="frequencyId"
          className="field-input"
          aria-describedby="frequencyId-error"
          aria-invalid={!!errors.frequencyId}
          {...register('frequencyId', { valueAsNumber: true })}
        >
          <option value={1}>{t('recurringExpenses.frequency.weekly')}</option>
          <option value={2}>{t('recurringExpenses.frequency.monthly')}</option>
          <option value={3}>{t('recurringExpenses.frequency.yearly')}</option>
        </select>
        <FieldError id="frequencyId-error" message={errors.frequencyId?.message} />
      </div>

      {/* Next due date */}
      <div>
        <label htmlFor="nextDueDate" className="field-label">
          {t('recurringExpenses.fields.nextDueDate')}
        </label>
        <input
          id="nextDueDate"
          type="date"
          className="field-input"
          aria-describedby="nextDueDate-error"
          aria-invalid={!!errors.nextDueDate}
          {...register('nextDueDate')}
        />
        <FieldError id="nextDueDate-error" message={errors.nextDueDate?.message} />
      </div>

      {/* Auto-create */}
      <div>
        <label className="flex items-center gap-2.5 text-sm text-ink-body cursor-pointer">
          <input type="checkbox" className="h-4 w-4 rounded border-surface-border" {...register('autoCreate')} />
          {t('recurringExpenses.fields.autoCreate')}
        </label>
      </div>

      {/* Active (edit only) */}
      {initialValues && (
        <div>
          <label className="flex items-center gap-2.5 text-sm text-ink-body cursor-pointer">
            <input type="checkbox" className="h-4 w-4 rounded border-surface-border" {...register('isActive')} />
            {t('recurringExpenses.fields.isActive')}
          </label>
        </div>
      )}

      <div className="flex gap-3 pt-1">
        <SubmitButton
          isSubmitting={isSubmitting}
          label={t('recurringExpenses.actions.save')}
          loadingLabel={t('recurringExpenses.actions.saving')}
        />
        <button type="button" onClick={onCancel} className="btn-secondary">
          {t('recurringExpenses.actions.cancel')}
        </button>
      </div>
    </form>
  )
}
