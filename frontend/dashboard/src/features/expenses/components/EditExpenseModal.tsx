import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQuery } from '@tanstack/react-query'
import { getExpenseById, updateExpense } from '@/features/expenses/services/expensesApi.service'
import ExpenseForm from '@/features/expenses/components/ExpenseForm'
import type { ExpenseFormData } from '@/features/expenses/expense.schemas'

export default function EditExpenseModal({
  expenseId,
  onSuccess,
  onClose,
}: Readonly<{ expenseId: number; onSuccess: () => void; onClose: () => void }>) {
  const { t } = useTranslation()
  const [isSubmitting, setIsSubmitting] = useState(false)

  const { data: expense, isLoading, isError } = useQuery({
    queryKey: ['expense', expenseId],
    queryFn: async () => {
      const res = await getExpenseById(expenseId)
      if (!res.ok) throw new Error('load_failed')
      return res.data
    },
  })

  const handleSubmit = async (data: ExpenseFormData) => {
    setIsSubmitting(true)
    const res = await updateExpense(expenseId, {
      amount: data.amount,
      currencyId: data.currencyId,
      date: data.date,
      categoryId: data.categoryId,
      subcategoryId: data.subcategoryId,
      description: data.description,
      familyIds: data.familyIds,
      tagIds: data.tagIds,
    })
    setIsSubmitting(false)
    if (res.ok) onSuccess()
  }

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="edit-expense-title"
      className="fixed inset-0 z-50 flex items-start justify-center bg-black/40 overflow-y-auto py-8"
    >
      <div className="bg-surface-card rounded-2xl shadow-xl border border-surface-border w-full max-w-lg mx-4">
        <div className="flex items-center justify-between px-6 pt-5 pb-4 border-b border-surface-border">
          <h2 id="edit-expense-title" className="text-base font-semibold text-ink">
            {t('expenses.editTitle')}
          </h2>
          <button
            type="button"
            onClick={onClose}
            aria-label="Close"
            className="h-7 w-7 rounded-lg text-ink-mute hover:text-ink hover:bg-surface-subtle flex items-center justify-center transition-colors"
          >
            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
              <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
        <div className="px-6 py-5">
          {isLoading && (
            <div className="flex justify-center py-8">
              <span className="text-ink-mute">{t('expenses.loading')}</span>
            </div>
          )}
          {(isError || (!isLoading && !expense)) && (
            <div className="flex justify-center py-8">
              <span className="text-red-500">{t('apiErrors.notFound')}</span>
            </div>
          )}
          {expense && (
            <ExpenseForm
              initialValues={expense}
              isSubmitting={isSubmitting}
              onSubmit={handleSubmit}
              onCancel={onClose}
            />
          )}
        </div>
      </div>
    </div>
  )
}
