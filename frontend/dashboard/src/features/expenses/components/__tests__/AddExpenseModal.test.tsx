import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import AddExpenseModal from '../AddExpenseModal'

const mockAddExpense = vi.fn()
vi.mock('@/features/expenses/services/expensesApi.service', () => ({
  addExpense: (...args: unknown[]) => mockAddExpense(...args),
}))

vi.mock('@/features/expenses/components/ExpenseForm', () => ({
  default: ({ onSubmit, onCancel }: { onSubmit: (d: unknown) => void; onCancel: () => void }) => (
    <div data-testid="expense-form">
      <button onClick={() => onSubmit({ amount: 10, currencyId: 1, date: '2026-06-01' })}>Submit</button>
      <button onClick={onCancel}>Cancel</button>
    </div>
  ),
}))

describe('AddExpenseModal', () => {
  const onSuccess = vi.fn()
  const onClose = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
    mockAddExpense.mockResolvedValue({ ok: true, status: 201, data: { id: 1 } })
  })

  it('renders dialog with title', () => {
    render(<AddExpenseModal onSuccess={onSuccess} onClose={onClose} />)
    expect(screen.getByRole('dialog')).toBeInTheDocument()
    expect(screen.getByRole('heading', { level: 2 })).toBeInTheDocument()
  })

  it('renders ExpenseForm inside modal', () => {
    render(<AddExpenseModal onSuccess={onSuccess} onClose={onClose} />)
    expect(screen.getByTestId('expense-form')).toBeInTheDocument()
  })

  it('calls onClose when close button clicked', async () => {
    const user = userEvent.setup()
    render(<AddExpenseModal onSuccess={onSuccess} onClose={onClose} />)
    await user.click(screen.getByRole('button', { name: /close/i }))
    expect(onClose).toHaveBeenCalled()
  })

  it('calls onClose when form cancel clicked', async () => {
    const user = userEvent.setup()
    render(<AddExpenseModal onSuccess={onSuccess} onClose={onClose} />)
    await user.click(screen.getByText('Cancel'))
    expect(onClose).toHaveBeenCalled()
  })

  it('calls addExpense and onSuccess on form submit', async () => {
    const user = userEvent.setup()
    render(<AddExpenseModal onSuccess={onSuccess} onClose={onClose} />)
    await user.click(screen.getByText('Submit'))
    expect(mockAddExpense).toHaveBeenCalledWith(
      expect.objectContaining({ amount: 10, currencyId: 1, date: '2026-06-01' })
    )
    expect(onSuccess).toHaveBeenCalled()
  })

  it('does not call onSuccess when addExpense fails', async () => {
    mockAddExpense.mockResolvedValue({ ok: false, status: 400 })
    const user = userEvent.setup()
    render(<AddExpenseModal onSuccess={onSuccess} onClose={onClose} />)
    await user.click(screen.getByText('Submit'))
    expect(onSuccess).not.toHaveBeenCalled()
  })
})
