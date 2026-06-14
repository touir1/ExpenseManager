import { useState, useEffect } from 'react'
import { Link, useNavigate, useLocation, useMatch } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useQuery } from '@tanstack/react-query'
import { usePageTitle } from '@/hooks/usePageTitle'
import { getExpenses, deleteExpense } from '@/features/expenses/services/expensesApi.service'
import ExpenseFilters from '@/features/expenses/components/ExpenseFilters'
import AddExpenseModal from '@/features/expenses/components/AddExpenseModal'
import EditExpenseModal from '@/features/expenses/components/EditExpenseModal'
import { useFamilies } from '@/features/families/FamilyContext'
import type { ExpenseDto, ExpenseFilter } from '@/features/expenses/types/expenses.type'

const DEFAULT_PAGE_SIZE = 20

function ConfirmDeleteModal({
  onConfirm,
  onCancel,
}: Readonly<{ onConfirm: () => void; onCancel: () => void }>) {
  const { t } = useTranslation()
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
      <div className="bg-surface-card rounded-2xl shadow-xl border border-surface-border w-full max-w-sm mx-4 p-6">
        <h2 className="text-base font-semibold text-ink mb-2">{t('expenses.delete.confirmTitle')}</h2>
        <p className="text-sm text-ink-mute mb-5">{t('expenses.delete.confirmBody')}</p>
        <div className="flex gap-3 justify-end">
          <button
            onClick={onCancel}
            className="px-4 py-2 text-sm font-medium rounded-lg border border-surface-border text-ink hover:bg-surface-subtle transition-colors"
          >
            {t('expenses.delete.cancel')}
          </button>
          <button
            onClick={onConfirm}
            className="px-4 py-2 text-sm font-medium rounded-lg bg-red-600 hover:bg-red-700 text-white transition-colors"
          >
            {t('expenses.delete.confirm')}
          </button>
        </div>
      </div>
    </div>
  )
}

function ExpenseRow({
  expense,
  onDelete,
}: Readonly<{ expense: ExpenseDto; onDelete: (id: number) => void }>) {
  const { t } = useTranslation()
  const navigate = useNavigate()

  const amount = expense.convertedAmount != null && expense.displayCurrency
    ? `${expense.convertedAmount.toFixed(expense.displayCurrency.decimals)} ${expense.displayCurrency.code}`
    : `${expense.amount.toFixed(expense.currency?.decimals ?? 2)} ${expense.currency?.code ?? ''}`

  return (
    <tr className="border-b border-surface-border hover:bg-surface-subtle transition-colors">
      <td className="px-4 py-3 text-sm text-ink-body whitespace-nowrap">{expense.date}</td>
      <td className="px-4 py-3 text-sm font-medium text-ink whitespace-nowrap">{amount}</td>
      <td className="px-4 py-3 text-sm text-ink-mute">
        {expense.category ? expense.category.name : <span className="italic text-ink-faint">{t('expenses.uncategorised')}</span>}
        {expense.subcategory && <span className="text-ink-faint"> / {expense.subcategory.name}</span>}
      </td>
      <td className="px-4 py-3 text-sm text-ink-mute max-w-xs truncate">{expense.description ?? '—'}</td>
      <td className="px-4 py-3 text-sm text-ink-mute">
        {expense.tags.length > 0
          ? expense.tags.map(tag => (
              <span key={tag.id} className="inline-flex items-center px-1.5 py-0.5 rounded-full text-xs font-medium bg-surface-subtle text-ink-body mr-1">
                {tag.name}
              </span>
            ))
          : '—'}
      </td>
      <td className="px-4 py-3 text-sm text-ink-mute">
        {expense.families && expense.families.length > 0
          ? expense.families.map(f => (
              <span key={f.id} className="inline-flex items-center px-1.5 py-0.5 rounded-full text-xs font-medium bg-brand-100 text-brand-700 dark:bg-brand-900 dark:text-brand-200 mr-1">
                {f.name}
              </span>
            ))
          : '—'}
      </td>
      <td className="px-4 py-3 text-sm whitespace-nowrap">
        <button
          onClick={() => navigate(`/expenses/${expense.id}/edit`)}
          className="text-brand-600 hover:text-brand-700 font-medium mr-3 transition-colors"
        >
          {t('expenses.actions.edit')}
        </button>
        <button
          onClick={() => onDelete(expense.id)}
          className="text-red-500 hover:text-red-700 font-medium transition-colors"
        >
          {t('expenses.actions.delete')}
        </button>
      </td>
    </tr>
  )
}

export default function ExpensesPage() {
  const { t } = useTranslation()
  usePageTitle(t('expenses.pageTitle'))
  const navigate = useNavigate()
  const { pathname } = useLocation()
  const editMatch = useMatch('/expenses/:id/edit')
  const editId = editMatch ? parseInt(editMatch.params.id ?? '') : null
  const isAddOpen = pathname === '/expenses/add'
  const { activeFamilyId } = useFamilies()
  const [filter, setFilter] = useState<ExpenseFilter>({ page: 1, pageSize: DEFAULT_PAGE_SIZE })
  const [deleteId, setDeleteId] = useState<number | null>(null)

  useEffect(() => {
    setFilter(f => ({ ...f, page: 1 }))
  }, [activeFamilyId])

  const effectiveFilter: ExpenseFilter = { ...filter, familyId: activeFamilyId ?? undefined }

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ['expenses', effectiveFilter],
    queryFn: async () => {
      const res = await getExpenses(effectiveFilter)
      if (!res.ok) throw new Error('load_failed')
      return res.data
    },
  })

  const handleDelete = async () => {
    if (deleteId == null) return
    const res = await deleteExpense(deleteId)
    setDeleteId(null)
    if (res.ok) refetch()
  }

  const totalPages = data?.totalPages ?? 1
  const page = filter.page ?? 1

  return (
    <div className="max-w-6xl mx-auto w-full px-4 sm:px-6 py-8">
      {/* Header */}
      <div className="mb-4 flex items-start justify-between gap-4 flex-wrap">
        <h1 className="text-2xl font-semibold text-ink tracking-tight">{t('expenses.pageTitle')}</h1>
        <div className="flex items-center gap-2">
          <Link
            to="/expenses/import"
            className="inline-flex items-center gap-1.5 px-3.5 py-2 rounded-xl border border-surface-border bg-surface-card hover:bg-surface-subtle text-ink text-sm font-medium transition-colors duration-150"
          >
            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
              <path strokeLinecap="round" strokeLinejoin="round" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" />
            </svg>
            {t('expenses.importCsv')}
          </Link>
          <Link
            to="/expenses/add"
            className="inline-flex items-center gap-1.5 px-3.5 py-2 rounded-xl border border-brand-300 bg-brand-50 hover:bg-brand-100 text-brand-700 text-sm font-medium transition-colors duration-150"
          >
            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5} aria-hidden="true">
              <path strokeLinecap="round" strokeLinejoin="round" d="M12 4v16m8-8H4" />
            </svg>
            {t('expenses.addExpense')}
          </Link>
        </div>
      </div>

      {/* Filters — inline collapsible section */}
      <ExpenseFilters filter={filter} onApply={setFilter} />

      {/* Content */}
      {isLoading && (
        <div className="flex justify-center py-16">
          <span className="text-ink-mute">{t('expenses.loading', 'Loading…')}</span>
        </div>
      )}

      {isError && (
        <div className="flex justify-center py-16">
          <span className="text-red-500">{t('expenses.errors.loadFailed')}</span>
        </div>
      )}

      {!isLoading && !isError && data && (
        <>
          {data.items.length === 0 ? (
            <div className="text-center py-16">
              <p className="text-sm text-ink-mute mb-4">{t('expenses.noExpenses')}</p>
              <Link
                to="/expenses/add"
                className="inline-flex items-center gap-1.5 text-sm font-medium text-brand-600 hover:text-brand-700 transition-colors"
              >
                {t('expenses.firstExpense')}
              </Link>
            </div>
          ) : (
            <div className="overflow-x-auto rounded-2xl border border-surface-border shadow-card">
              <table className="w-full">
                <thead className="bg-surface-subtle">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.table.date')}</th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.table.amount')}</th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.table.category')}</th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.fields.description')}</th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.table.tags')}</th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.table.families')}</th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-ink-mute uppercase tracking-wide">{t('expenses.table.actions')}</th>
                  </tr>
                </thead>
                <tbody className="bg-surface-card">
                  {data.items.map(expense => (
                    <ExpenseRow key={expense.id} expense={expense} onDelete={setDeleteId} />
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="mt-4 flex items-center justify-center gap-4">
              <button
                onClick={() => setFilter(f => ({ ...f, page: Math.max(1, (f.page ?? 1) - 1) }))}
                disabled={page <= 1}
                className="text-sm font-medium text-brand-600 hover:text-brand-700 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
              >
                {t('expenses.pagination.prev')}
              </button>
              <span className="text-sm text-ink-mute">
                {t('expenses.pagination.pageOf', { page, total: totalPages })}
              </span>
              <button
                onClick={() => setFilter(f => ({ ...f, page: Math.min(totalPages, (f.page ?? 1) + 1) }))}
                disabled={page >= totalPages}
                className="text-sm font-medium text-brand-600 hover:text-brand-700 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
              >
                {t('expenses.pagination.next')}
              </button>
            </div>
          )}
        </>
      )}

      {deleteId != null && (
        <ConfirmDeleteModal
          onConfirm={handleDelete}
          onCancel={() => setDeleteId(null)}
        />
      )}

      {isAddOpen && (
        <AddExpenseModal
          onSuccess={() => { refetch(); navigate('/expenses') }}
          onClose={() => navigate('/expenses')}
        />
      )}

      {editId != null && (
        <EditExpenseModal
          expenseId={editId}
          onSuccess={() => { refetch(); navigate('/expenses') }}
          onClose={() => navigate('/expenses')}
        />
      )}
    </div>
  )
}
