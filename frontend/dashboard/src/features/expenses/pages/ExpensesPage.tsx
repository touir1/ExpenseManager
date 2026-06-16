import { useState, useEffect } from 'react'
import { Link, useNavigate, useLocation, useMatch, useSearchParams } from 'react-router-dom'
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

function formatExpenseAmount(expense: ExpenseDto): string {
  return expense.convertedAmount != null && expense.displayCurrency
    ? `${expense.convertedAmount.toFixed(expense.displayCurrency.decimals)} ${expense.displayCurrency.code}`
    : `${expense.amount.toFixed(expense.currency?.decimals ?? 2)} ${expense.currency?.code ?? ''}`
}

function ConfirmDeleteModal({
  expense,
  onConfirm,
  onCancel,
}: Readonly<{ expense: ExpenseDto; onConfirm: () => void; onCancel: () => void }>) {
  const { t } = useTranslation()
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
      <div className="bg-surface-card rounded-2xl shadow-warm border border-surface-border w-full max-w-sm mx-4 p-6">
        <h2 className="text-base font-semibold text-ink mb-2">{t('expenses.delete.confirmTitle')}</h2>
        <p className="text-sm text-ink-body font-medium mb-1">
          {formatExpenseAmount(expense)} — {expense.date}
        </p>
        {expense.description && (
          <p className="text-sm text-ink-mute mb-3 truncate">{expense.description}</p>
        )}
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

function EditButton({ expenseId, className }: Readonly<{ expenseId: number; className?: string }>) {
  const navigate = useNavigate()
  return (
    <button
      onClick={() => navigate(`/expenses/${expenseId}/edit`)}
      aria-label="Edit expense"
      className={`p-1.5 rounded-lg text-ink-mute hover:text-brand-600 hover:bg-brand-50 transition-colors ${className ?? ''}`}
    >
      <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
        <path strokeLinecap="round" strokeLinejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
      </svg>
    </button>
  )
}

function DeleteButton({ onClick, className }: Readonly<{ onClick: () => void; className?: string }>) {
  return (
    <button
      onClick={onClick}
      aria-label="Delete expense"
      className={`p-1.5 rounded-lg text-ink-mute hover:text-berry hover:bg-berry-soft transition-colors ${className ?? ''}`}
    >
      <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
        <path strokeLinecap="round" strokeLinejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6M9 7V4a1 1 0 011-1h4a1 1 0 011 1v3M4 7h16" />
      </svg>
    </button>
  )
}

function ExpenseRow({
  expense,
  onDelete,
}: Readonly<{ expense: ExpenseDto; onDelete: (expense: ExpenseDto) => void }>) {
  const { t } = useTranslation()
  const amount = formatExpenseAmount(expense)

  return (
    <tr className="border-b border-surface-border hover:bg-surface-subtle transition-colors">
      <td className="px-4 py-3 text-sm text-ink-body whitespace-nowrap tabular-nums">{expense.date}</td>
      <td className="px-4 py-3 text-sm font-medium text-ink whitespace-nowrap font-mono tabular-nums">{amount}</td>
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
        <EditButton expenseId={expense.id} className="mr-1" />
        <DeleteButton onClick={() => onDelete(expense)} />
      </td>
    </tr>
  )
}

function ExpenseCard({
  expense,
  onDelete,
}: Readonly<{ expense: ExpenseDto; onDelete: (expense: ExpenseDto) => void }>) {
  const { t } = useTranslation()
  const amount = formatExpenseAmount(expense)

  return (
    <div className="bg-surface-card rounded-2xl border border-surface-border p-3">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-sm font-medium text-ink font-mono tabular-nums">{amount}</p>
          <p className="text-xs text-ink-mute tabular-nums">{expense.date}</p>
        </div>
        <div className="flex items-center -mr-1">
          <EditButton expenseId={expense.id} />
          <DeleteButton onClick={() => onDelete(expense)} />
        </div>
      </div>
      <p className="text-sm text-ink-mute mt-2">
        {expense.category ? expense.category.name : <span className="italic text-ink-faint">{t('expenses.uncategorised')}</span>}
        {expense.subcategory && <span className="text-ink-faint"> / {expense.subcategory.name}</span>}
      </p>
      {expense.description && <p className="text-sm text-ink-mute mt-1 truncate">{expense.description}</p>}
      {(expense.tags.length > 0 || (expense.families && expense.families.length > 0)) && (
        <div className="flex flex-wrap gap-1 mt-2">
          {expense.tags.map(tag => (
            <span key={tag.id} className="inline-flex items-center px-1.5 py-0.5 rounded-full text-xs font-medium bg-surface-subtle text-ink-body">
              {tag.name}
            </span>
          ))}
          {expense.families?.map(f => (
            <span key={f.id} className="inline-flex items-center px-1.5 py-0.5 rounded-full text-xs font-medium bg-brand-100 text-brand-700 dark:bg-brand-900 dark:text-brand-200">
              {f.name}
            </span>
          ))}
        </div>
      )}
    </div>
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
  const [initParams] = useSearchParams()
  const [filter, setFilter] = useState<ExpenseFilter>(() => ({
    page: 1,
    pageSize: DEFAULT_PAGE_SIZE,
    categoryId: initParams.get('categoryId') ? parseInt(initParams.get('categoryId')!) : undefined,
    dateFrom: initParams.get('dateFrom') ?? undefined,
    dateTo: initParams.get('dateTo') ?? undefined,
  }))
  const [deleteTarget, setDeleteTarget] = useState<ExpenseDto | null>(null)
  const [pageInput, setPageInput] = useState('')

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
    if (deleteTarget == null) return
    const res = await deleteExpense(deleteTarget.id)
    setDeleteTarget(null)
    if (res.ok) refetch()
  }

  const totalPages = data?.totalPages ?? 1
  const page = filter.page ?? 1
  const pageSize = filter.pageSize ?? DEFAULT_PAGE_SIZE
  const totalCount = data?.totalCount ?? 0

  const goToPage = (target: number) => {
    const clamped = Math.min(Math.max(1, target), totalPages)
    setFilter(f => ({ ...f, page: clamped }))
    setPageInput('')
  }

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
            <>
              {/* Mobile card list */}
              <div className="md:hidden space-y-2">
                {data.items.map(expense => (
                  <ExpenseCard key={expense.id} expense={expense} onDelete={setDeleteTarget} />
                ))}
              </div>

              {/* Desktop table */}
              <div className="hidden md:block overflow-x-auto rounded-2xl border border-surface-border shadow-card">
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
                      <ExpenseRow key={expense.id} expense={expense} onDelete={setDeleteTarget} />
                    ))}
                  </tbody>
                </table>
              </div>
            </>
          )}

          {/* Pagination */}
          {data.items.length > 0 && (
            <div className="mt-4 flex flex-col items-center gap-2">
              {totalCount > 0 && (
                <span className="text-xs text-ink-faint">
                  {t('expenses.pagination.showing', {
                    from: (page - 1) * pageSize + 1,
                    to: Math.min(page * pageSize, totalCount),
                    totalCount,
                  })}
                </span>
              )}
              {totalPages > 1 && (
                <div className="flex items-center gap-4">
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
                  <label htmlFor="pagination-goto" className="text-sm text-ink-mute flex items-center gap-1.5">
                    {t('expenses.pagination.goToPage')}
                    <input
                      id="pagination-goto"
                      type="number"
                      min={1}
                      max={totalPages}
                      value={pageInput}
                      placeholder={String(page)}
                      onChange={e => setPageInput(e.target.value)}
                      onKeyDown={e => {
                        if (e.key === 'Enter' && pageInput) goToPage(Number(pageInput))
                      }}
                      onBlur={() => {
                        if (pageInput) goToPage(Number(pageInput))
                      }}
                      className="w-16 field-input py-1 px-2 text-sm"
                    />
                  </label>
                </div>
              )}
            </div>
          )}
        </>
      )}

      {deleteTarget != null && (
        <ConfirmDeleteModal
          expense={deleteTarget}
          onConfirm={handleDelete}
          onCancel={() => setDeleteTarget(null)}
        />
      )}

      {isAddOpen && (
        <AddExpenseModal
          onSuccess={() => { refetch(); navigate('/expenses') }}
          onAdded={refetch}
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
