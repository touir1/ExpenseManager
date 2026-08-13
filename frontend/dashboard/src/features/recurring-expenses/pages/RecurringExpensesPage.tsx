import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { usePageTitle } from '@/hooks/usePageTitle'
import { useReturnFocusOnUnmount } from '@/hooks/useReturnFocusOnUnmount'
import EmptyState from '@/components/EmptyState'
import { useToast } from '@/components/Toast'
import {
  getAll,
  getById,
  create,
  update,
  remove,
} from '@/features/recurring-expenses/services/recurringExpenseApi.service'
import RecurringExpenseForm from '@/features/recurring-expenses/components/RecurringExpenseForm'
import type { RecurringExpenseFormData } from '@/features/recurring-expenses/recurringExpense.schemas'
import type { RecurringExpenseDto } from '@/features/dashboard/types/dashboard.type'
import { formatExpenseDate } from '@/features/expenses/utils/dateFormat'

function toRequest(data: RecurringExpenseFormData) {
  return {
    description: data.description,
    amount: data.amount,
    currencyId: data.currencyId,
    categoryId: data.categoryId,
    subcategoryId: data.subcategoryId,
    familyId: data.familyId,
    frequencyId: data.frequencyId,
    nextDueDate: data.nextDueDate,
    autoCreate: data.autoCreate,
    isActive: data.isActive,
  }
}

function FormModal({
  editId,
  onClose,
  onSaved,
}: Readonly<{ editId: number | 'new' | null; onClose: () => void; onSaved: () => void }>) {
  const { t } = useTranslation()
  const [isSubmitting, setIsSubmitting] = useState(false)
  useReturnFocusOnUnmount()
  const { show } = useToast()

  const isEdit = typeof editId === 'number'

  const { data: initialValues, isLoading } = useQuery({
    queryKey: ['recurringExpenses', 'detail', editId],
    queryFn: async () => {
      if (!isEdit) return undefined
      const res = await getById(editId)
      return res.ok ? res.data : undefined
    },
    enabled: isEdit,
  })

  const handleSubmit = async (data: RecurringExpenseFormData) => {
    setIsSubmitting(true)
    const res = isEdit ? await update(editId, toRequest(data)) : await create(toRequest(data))
    setIsSubmitting(false)
    if (res.ok) {
      onSaved()
    } else {
      show(t(isEdit ? 'recurringExpenses.errors.saveFailed' : 'recurringExpenses.errors.saveFailed'), 'error')
    }
  }

  if (isEdit && isLoading) return null

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="recurring-expense-modal-title"
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
    >
      <div className="bg-surface-card rounded-2xl shadow-xl border border-surface-border w-full modal-lg flex flex-col max-h-[90dvh]">
        <div className="flex items-center justify-between px-6 pt-5 pb-4 border-b border-surface-border flex-shrink-0">
          <h2 id="recurring-expense-modal-title" className="text-base font-semibold text-ink">
            {isEdit ? t('recurringExpenses.editTitle') : t('recurringExpenses.addTitle')}
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
        <div className="px-6 py-5 overflow-y-auto">
          <RecurringExpenseForm
            initialValues={initialValues}
            isSubmitting={isSubmitting}
            onSubmit={handleSubmit}
            onCancel={onClose}
          />
        </div>
      </div>
    </div>
  )
}

function ConfirmDeleteModal({
  item,
  onConfirm,
  onCancel,
}: Readonly<{ item: RecurringExpenseDto; onConfirm: () => void; onCancel: () => void }>) {
  const { t } = useTranslation()
  useReturnFocusOnUnmount()
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
      <div className="bg-surface-card rounded-2xl shadow-warm border border-surface-border w-full modal-sm mx-4 p-6">
        <h2 className="text-base font-semibold text-ink mb-2">{t('recurringExpenses.delete.confirmTitle')}</h2>
        <p className="text-sm text-ink-body font-medium mb-3 truncate">{item.description}</p>
        <p className="text-sm text-ink-mute mb-5">{t('recurringExpenses.delete.confirmBody')}</p>
        <div className="flex gap-3 justify-end">
          <button
            onClick={onCancel}
            className="px-4 py-2 text-sm font-medium rounded-lg border border-surface-border text-ink hover:bg-surface-subtle transition-colors"
          >
            {t('recurringExpenses.delete.cancel')}
          </button>
          <button
            onClick={onConfirm}
            className="px-4 py-2 text-sm font-medium rounded-lg bg-red-600 hover:bg-red-700 text-white transition-colors"
          >
            {t('recurringExpenses.delete.confirm')}
          </button>
        </div>
      </div>
    </div>
  )
}

export default function RecurringExpensesPage() {
  const { t } = useTranslation()
  usePageTitle(t('recurringExpenses.pageTitle'))
  const { show } = useToast()
  const queryClient = useQueryClient()

  const [includeInactive, setIncludeInactive] = useState(false)
  const [editId, setEditId] = useState<number | 'new' | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<RecurringExpenseDto | null>(null)

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['recurringExpenses', 'list', includeInactive],
    queryFn: async () => {
      const res = await getAll(includeInactive)
      return res.ok ? res.data ?? [] : []
    },
  })

  const handleSaved = () => {
    setEditId(null)
    refetch()
    queryClient.invalidateQueries({ queryKey: ['dashboard'] })
  }

  const handleDelete = async () => {
    if (!deleteTarget) return
    const res = await remove(deleteTarget.id)
    setDeleteTarget(null)
    if (res.ok) {
      refetch()
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    } else {
      show(t('recurringExpenses.errors.deleteFailed'), 'error')
    }
  }

  const items = data ?? []

  return (
    <div className="max-w-4xl mx-auto w-full px-4 sm:px-6 py-8">
      <div className="mb-4 flex items-start justify-between gap-4 flex-wrap">
        <div>
          <h1 className="text-2xl font-semibold text-ink tracking-tight">{t('recurringExpenses.title')}</h1>
          <p className="text-sm text-ink-mute mt-1">{t('recurringExpenses.subtitle')}</p>
        </div>
        <button
          onClick={() => setEditId('new')}
          className="inline-flex items-center gap-1.5 px-3.5 py-2 rounded-xl border border-brand-300 bg-brand-50 hover:bg-brand-100 text-brand-700 text-sm font-medium transition-colors duration-150"
        >
          <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5} aria-hidden="true">
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 4v16m8-8H4" />
          </svg>
          {t('recurringExpenses.add')}
        </button>
      </div>

      <label className="flex items-center gap-2 text-sm text-ink-mute mb-4 cursor-pointer">
        <input
          type="checkbox"
          checked={includeInactive}
          onChange={e => setIncludeInactive(e.target.checked)}
          className="h-4 w-4 rounded border-surface-border"
        />
        {t('recurringExpenses.includeInactive')}
      </label>

      {!isLoading && items.length === 0 && (
        <EmptyState
          icon="🔁"
          title={t('recurringExpenses.empty')}
          subtitle={t('recurringExpenses.emptySubtitle')}
          action={{ label: t('recurringExpenses.add'), onClick: () => setEditId('new') }}
        />
      )}

      {items.length > 0 && (
        <div className="overflow-x-auto rounded-2xl border border-surface-border shadow-card">
          <table className="w-full">
            <thead className="bg-surface-subtle">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('recurringExpenses.table.description')}</th>
                <th className="px-4 py-3 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('recurringExpenses.table.amount')}</th>
                <th className="px-4 py-3 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('recurringExpenses.table.frequency')}</th>
                <th className="px-4 py-3 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('recurringExpenses.table.nextDueDate')}</th>
                <th className="px-4 py-3 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('recurringExpenses.table.status')}</th>
                <th className="px-4 py-3 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('recurringExpenses.table.actions')}</th>
              </tr>
            </thead>
            <tbody className="bg-surface-card">
              {items.map(item => (
                <tr key={item.id} className="border-t border-surface-border">
                  <td className="px-4 py-3 text-sm text-ink-body">
                    {item.description}
                    {item.autoCreate && (
                      <span className="ml-2 inline-block text-[11px] px-2 py-0.5 rounded-full font-medium bg-brand-100 text-brand-700">
                        {t('dashboard.recurring.auto')}
                      </span>
                    )}
                  </td>
                  <td className="px-4 py-3 text-sm font-medium text-ink tabular-nums">
                    {item.currency?.symbol ?? ''} {item.amount.toFixed(item.currency?.decimals ?? 2)}
                  </td>
                  <td className="px-4 py-3 text-sm text-ink-mute">{item.frequency}</td>
                  <td className="px-4 py-3 text-sm text-ink-mute tabular-nums">{formatExpenseDate(item.nextDueDate)}</td>
                  <td className="px-4 py-3 text-sm">
                    <span className={`inline-block text-xs px-2 py-0.5 rounded-full font-medium ${item.isActive ? 'bg-sage/20 text-sage' : 'bg-surface-subtle text-ink-mute'}`}>
                      {item.isActive ? t('recurringExpenses.status.active') : t('recurringExpenses.status.paused')}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-sm whitespace-nowrap">
                    <button
                      onClick={() => setEditId(item.id)}
                      aria-label={t('recurringExpenses.actions.edit')}
                      className="p-1.5 rounded-lg text-ink-mute hover:text-brand-600 hover:bg-brand-50 transition-colors mr-1"
                    >
                      <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                      </svg>
                    </button>
                    <button
                      onClick={() => setDeleteTarget(item)}
                      aria-label={t('recurringExpenses.actions.delete')}
                      className="p-1.5 rounded-lg text-ink-mute hover:text-berry hover:bg-berry-soft transition-colors"
                    >
                      <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6M9 7V4a1 1 0 011-1h4a1 1 0 011 1v3M4 7h16" />
                      </svg>
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {editId != null && (
        <FormModal editId={editId} onClose={() => setEditId(null)} onSaved={handleSaved} />
      )}

      {deleteTarget != null && (
        <ConfirmDeleteModal item={deleteTarget} onConfirm={handleDelete} onCancel={() => setDeleteTarget(null)} />
      )}
    </div>
  )
}
