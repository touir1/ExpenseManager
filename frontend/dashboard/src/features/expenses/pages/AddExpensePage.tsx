import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { usePageTitle } from '@/hooks/usePageTitle'
import ExpenseForm from '@/features/expenses/components/ExpenseForm'
import { addExpense } from '@/features/expenses/services/expensesApi.service'
import type { ExpenseFormData } from '@/features/expenses/expense.schemas'

export default function AddExpensePage() {
  const { t } = useTranslation()
  usePageTitle(t('expenses.addTitle'))
  const navigate = useNavigate()
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleSubmit = async (data: ExpenseFormData) => {
    setIsSubmitting(true)
    const res = await addExpense({
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

  return (
    <div className="max-w-lg mx-auto py-8 px-4">
      <h1 className="text-2xl font-bold text-ink-title mb-6">{t('expenses.addTitle')}</h1>
      <ExpenseForm
        isSubmitting={isSubmitting}
        onSubmit={handleSubmit}
        onCancel={() => navigate('/expenses')}
      />
    </div>
  )
}
