import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useQuery } from '@tanstack/react-query'
import { usePageTitle } from '@/hooks/usePageTitle'
import ExpenseForm from '@/features/expenses/components/ExpenseForm'
import { getExpenseById, updateExpense } from '@/features/expenses/services/expensesApi.service'
import type { ExpenseFormData } from '@/features/expenses/expense.schemas'

export default function EditExpensePage() {
  const { t } = useTranslation()
  usePageTitle(t('expenses.editTitle'))
  const navigate = useNavigate()
  const { id } = useParams<{ id: string }>()
  const expenseId = Number(id)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const { data: expense, isLoading, isError } = useQuery({
    queryKey: ['expense', expenseId],
    queryFn: () => getExpenseById(expenseId),
    enabled: !!expenseId,
    select: res => (res.ok ? res.data : undefined),
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
    if (res.ok) navigate('/expenses')
  }

  if (isLoading) {
    return (
      <div className="flex justify-center py-16">
        <span className="text-ink-mute">{t('expenses.loading')}</span>
      </div>
    )
  }

  if (isError || !expense) {
    return (
      <div className="flex justify-center py-16">
        <span className="text-ink-mute">{t('apiErrors.notFound')}</span>
      </div>
    )
  }

  return (
    <div className="max-w-lg mx-auto py-8 px-4">
      <h1 className="text-2xl font-bold text-ink-title mb-6">{t('expenses.editTitle')}</h1>
      <ExpenseForm
        initialValues={expense}
        isSubmitting={isSubmitting}
        onSubmit={handleSubmit}
        onCancel={() => navigate('/expenses')}
      />
    </div>
  )
}
