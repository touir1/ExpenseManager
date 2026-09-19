import { useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as Dialog from '@radix-ui/react-dialog'
import { addExpense } from '@/features/expenses/services/expensesApi.service'
import ExpenseForm from '@/features/expenses/components/ExpenseForm'
import type { ExpenseFormData } from '@/features/expenses/expense.schemas'

export default function AddExpenseModal({
  onSuccess,
  onAdded,
  onClose,
}: Readonly<{ onSuccess: () => void; onAdded?: () => void; onClose: () => void }>) {
  const { t } = useTranslation()
  const [isSubmitting, setIsSubmitting] = useState(false)
  // Radix restores focus to Dialog.Trigger on close, but the trigger button lives in the
  // parent page rather than as a Dialog.Trigger here — restore it manually instead.
  const triggerRef = useRef(document.activeElement as HTMLElement | null)

  const submitExpense = (data: ExpenseFormData) =>
    addExpense({
      amount: data.amount,
      currencyId: data.currencyId,
      date: data.date,
      categoryId: data.categoryId,
      subcategoryId: data.subcategoryId,
      description: data.description,
      familyIds: data.familyIds,
      tagIds: data.tagIds,
    })

  const handleSubmit = async (data: ExpenseFormData) => {
    setIsSubmitting(true)
    const res = await submitExpense(data)
    setIsSubmitting(false)
    if (res.ok) onSuccess()
    return res.ok && res.data ? { id: res.data.id } : undefined
  }

  const handleSaveAndAddAnother = async (data: ExpenseFormData) => {
    setIsSubmitting(true)
    const res = await submitExpense(data)
    setIsSubmitting(false)
    if (res.ok) onAdded?.()
    return res.ok && res.data ? { id: res.data.id } : undefined
  }

  return (
    <Dialog.Root open onOpenChange={o => { if (!o) onClose() }}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-50 bg-black/40" />
        <Dialog.Content
          className="fixed inset-0 z-50 flex items-center justify-center p-4 outline-none"
          aria-describedby={undefined}
          onCloseAutoFocus={e => { e.preventDefault(); triggerRef.current?.focus() }}
        >
          <div className="bg-surface-card rounded-2xl shadow-xl border border-surface-border w-full modal-lg flex flex-col max-h-[90dvh]">
            <div className="flex items-center justify-between px-6 pt-5 pb-4 border-b border-surface-border flex-shrink-0">
              <Dialog.Title asChild>
                <h2 className="text-base font-semibold text-ink">{t('expenses.addTitle')}</h2>
              </Dialog.Title>
              <Dialog.Close asChild>
                <button
                  type="button"
                  aria-label="Close"
                  className="h-7 w-7 rounded-lg text-ink-mute hover:text-ink hover:bg-surface-subtle flex items-center justify-center transition-colors"
                >
                  <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </Dialog.Close>
            </div>
            <div className="px-6 py-5 overflow-y-auto">
              <ExpenseForm
                isSubmitting={isSubmitting}
                onSubmit={handleSubmit}
                onSaveAndAddAnother={handleSaveAndAddAnother}
                onCancel={onClose}
              />
            </div>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  )
}
