import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import EditExpenseModal from '../EditExpenseModal'
import type { ExpenseDto } from '@/features/expenses/types/expenses.type'

const mockGetExpenseById = vi.fn()
const mockUpdateExpense = vi.fn()

vi.mock('@/features/expenses/services/expensesApi.service', () => ({
  getExpenseById: (...args: unknown[]) => mockGetExpenseById(...args),
  updateExpense: (...args: unknown[]) => mockUpdateExpense(...args),
}))

vi.mock('@/features/expenses/components/ExpenseForm', () => ({
  default: ({ onSubmit, onCancel }: { onSubmit: (d: unknown) => void; onCancel: () => void }) => (
    <div data-testid="expense-form">
      <button onClick={() => onSubmit({ amount: 20, currencyId: 1, date: '2026-06-01' })}>Save</button>
      <button onClick={onCancel}>Cancel</button>
    </div>
  ),
}))

const mockExpense: ExpenseDto = {
  id: 5,
  amount: 50,
  currency: { id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
  date: '2026-06-01',
  category: null,
  subcategory: null,
  description: null,
  createdAt: '2026-06-01T00:00:00Z',
  modifiedAt: null,
  modifiedFrom: null,
  tags: [],
  convertedAmount: null,
  displayCurrency: null,
}

function makeQC() {
  return new QueryClient({ defaultOptions: { queries: { retry: false } } })
}

function renderModal(expenseId = 5, onSuccess = vi.fn(), onClose = vi.fn()) {
  return render(
    <QueryClientProvider client={makeQC()}>
      <EditExpenseModal expenseId={expenseId} onSuccess={onSuccess} onClose={onClose} />
    </QueryClientProvider>
  )
}

describe('EditExpenseModal', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockGetExpenseById.mockResolvedValue({ ok: true, status: 200, data: mockExpense })
    mockUpdateExpense.mockResolvedValue({ ok: true, status: 200, data: mockExpense })
  })

  it('renders dialog', () => {
    renderModal()
    expect(screen.getByRole('dialog')).toBeInTheDocument()
  })

  it('shows loading state initially', () => {
    let resolve!: (v: unknown) => void
    mockGetExpenseById.mockReturnValue(new Promise(r => { resolve = r }))
    renderModal()
    expect(screen.getByText(/loading/i)).toBeInTheDocument()
    resolve({ ok: true, status: 200, data: mockExpense })
  })

  it('renders ExpenseForm after data loads', async () => {
    renderModal()
    await waitFor(() => expect(screen.getByTestId('expense-form')).toBeInTheDocument())
  })

  it('shows error state when API call fails', async () => {
    mockGetExpenseById.mockRejectedValue(new Error('load_failed'))
    renderModal()
    await waitFor(() => expect(screen.queryByText(/loading/i)).not.toBeInTheDocument())
    expect(screen.queryByTestId('expense-form')).not.toBeInTheDocument()
  })

  it('calls onClose when close button clicked', async () => {
    const user = userEvent.setup()
    const onClose = vi.fn()
    renderModal(5, vi.fn(), onClose)
    await user.click(screen.getByRole('button', { name: /close/i }))
    expect(onClose).toHaveBeenCalled()
  })

  it('calls onClose when form cancel clicked', async () => {
    const user = userEvent.setup()
    const onClose = vi.fn()
    renderModal(5, vi.fn(), onClose)
    await waitFor(() => expect(screen.getByTestId('expense-form')).toBeInTheDocument())
    await user.click(screen.getByText('Cancel'))
    expect(onClose).toHaveBeenCalled()
  })

  it('calls updateExpense and onSuccess on form save', async () => {
    const user = userEvent.setup()
    const onSuccess = vi.fn()
    renderModal(5, onSuccess)
    await waitFor(() => expect(screen.getByTestId('expense-form')).toBeInTheDocument())
    await user.click(screen.getByText('Save'))
    expect(mockUpdateExpense).toHaveBeenCalledWith(5, expect.objectContaining({ amount: 20 }))
    expect(onSuccess).toHaveBeenCalled()
  })

  it('does not call onSuccess when updateExpense fails', async () => {
    const user = userEvent.setup()
    mockUpdateExpense.mockResolvedValue({ ok: false, status: 400 })
    const onSuccess = vi.fn()
    renderModal(5, onSuccess)
    await waitFor(() => expect(screen.getByTestId('expense-form')).toBeInTheDocument())
    await user.click(screen.getByText('Save'))
    expect(onSuccess).not.toHaveBeenCalled()
  })
})
