import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import ExpensesPage from '../ExpensesPage'
import type { ExpenseDto, ExpensePagedResponse } from '@/features/expenses/types/expenses.type'

// ── Mocks ─────────────────────────────────────────────────────────────────────

const mockNavigate = vi.fn()
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom')
  return { ...actual, useNavigate: () => mockNavigate }
})

vi.mock('@/features/expenses/ExpensesDataContext', () => ({
  useExpensesData: () => ({
    categories: [],
    currencies: [],
    tags: [],
    isLoading: false,
    refresh: vi.fn(),
  }),
}))

vi.mock('@/features/families/FamilyContext', () => ({
  useFamilies: () => ({ families: [], activeFamilyId: null, setActiveFamilyId: vi.fn(), isLoading: false, refresh: vi.fn() }),
}))

vi.mock('@/features/expenses/components/ExpenseForm', () => ({
  default: ({ onSubmit, onCancel }: { onSubmit: (d: unknown) => void; onCancel: () => void }) => (
    <div data-testid="expense-form">
      <button onClick={() => onSubmit({ amount: 25, currencyId: 1, date: '2026-05-01' })}>Save</button>
      <button onClick={onCancel}>Cancel</button>
    </div>
  ),
}))

const mockGetExpenses = vi.fn()
const mockDeleteExpense = vi.fn()
const mockAddExpense = vi.fn()
vi.mock('@/features/expenses/services/expensesApi.service', () => ({
  getExpenses: (...args: unknown[]) => mockGetExpenses(...args),
  deleteExpense: (...args: unknown[]) => mockDeleteExpense(...args),
  addExpense: (...args: unknown[]) => mockAddExpense(...args),
}))

// ── Fixtures ──────────────────────────────────────────────────────────────────

const expense: ExpenseDto = {
  id: 1,
  amount: 50,
  currency: { id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
  date: '2026-05-01',
  category: { id: 1, name: 'Food' },
  subcategory: null,
  description: 'Lunch',
  createdAt: '2026-05-01T10:00:00Z',
  modifiedAt: null,
  modifiedFrom: null,
  tags: [],
  convertedAmount: null,
  displayCurrency: null,
}

const pagedResponse: ExpensePagedResponse = {
  items: [expense],
  totalCount: 1,
  page: 1,
  pageSize: 20,
  totalPages: 1,
}

const emptyPagedResponse: ExpensePagedResponse = {
  items: [],
  totalCount: 0,
  page: 1,
  pageSize: 20,
  totalPages: 0,
}

// ── Helpers ───────────────────────────────────────────────────────────────────

function renderPage(path = '/expenses') {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={qc}>
      <MemoryRouter initialEntries={[path]}>
        <Routes>
          <Route path="/expenses" element={<ExpensesPage />} />
          <Route path="/expenses/add" element={<ExpensesPage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>
  )
}

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('ExpensesPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockGetExpenses.mockResolvedValue({ ok: true, data: pagedResponse })
    mockDeleteExpense.mockResolvedValue({ ok: true })
    mockAddExpense.mockResolvedValue({ ok: true })
  })

  it('renders page heading', () => {
    renderPage()
    expect(screen.getByRole('heading', { level: 1 })).toBeInTheDocument()
  })

  it('renders Add expense link', () => {
    renderPage()
    expect(screen.getByRole('link', { name: /add expense/i })).toHaveAttribute('href', '/expenses/add')
  })

  it('shows loading state initially', () => {
    mockGetExpenses.mockReturnValue(new Promise(() => {}))
    renderPage()
    expect(screen.getByText(/loading/i)).toBeInTheDocument()
  })

  it('shows error when fetch fails', async () => {
    mockGetExpenses.mockResolvedValue({ ok: false })
    renderPage()
    await waitFor(() => {
      expect(screen.getByText(/failed to load/i)).toBeInTheDocument()
    })
  })

  it('shows empty state when no expenses', async () => {
    mockGetExpenses.mockResolvedValue({ ok: true, data: emptyPagedResponse })
    renderPage()
    await waitFor(() => {
      expect(screen.getByText(/no expenses/i)).toBeInTheDocument()
    })
  })

  it('renders expense row with date and amount', async () => {
    renderPage()
    await waitFor(() => {
      const table = within(screen.getByRole('table'))
      expect(table.getByText('2026-05-01')).toBeInTheDocument()
      expect(table.getByText(/50.00 EUR/i)).toBeInTheDocument()
    })
  })

  it('renders expense category', async () => {
    renderPage()
    await waitFor(() => {
      expect(within(screen.getByRole('table')).getByText('Food')).toBeInTheDocument()
    })
  })

  it('navigates to edit page on Edit click', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByRole('table'))
    const editButton = within(screen.getByRole('table')).getByRole('button', { name: /edit/i })
    await user.click(editButton)
    expect(mockNavigate).toHaveBeenCalledWith('/expenses/1/edit')
  })

  it('shows confirm modal on Delete click', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByRole('table'))
    const deleteButton = within(screen.getByRole('table')).getByRole('button', { name: /delete/i })
    await user.click(deleteButton)
    expect(screen.getByText(/delete expense\?/i)).toBeInTheDocument()
  })

  it('calls deleteExpense and closes modal on confirm', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByRole('table'))
    const deleteButton = within(screen.getByRole('table')).getByRole('button', { name: /delete/i })
    await user.click(deleteButton)
    await user.click(screen.getByRole('button', { name: /^delete$/i }))
    await waitFor(() => {
      expect(mockDeleteExpense).toHaveBeenCalledWith(1)
    })
  })

  it('closes modal on cancel', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByRole('table'))
    const deleteButton = within(screen.getByRole('table')).getByRole('button', { name: /delete/i })
    await user.click(deleteButton)
    await user.click(screen.getByRole('button', { name: /^cancel$/i }))
    expect(screen.queryByText(/delete expense\?/i)).not.toBeInTheDocument()
  })

  it('shows pagination when totalPages > 1', async () => {
    mockGetExpenses.mockResolvedValue({
      ok: true,
      data: { ...pagedResponse, totalPages: 3, totalCount: 60 },
    })
    renderPage()
    await waitFor(() => {
      expect(screen.getByText(/page 1 of 3/i)).toBeInTheDocument()
    })
  })

  it('hides pagination when only 1 page', async () => {
    renderPage()
    await waitFor(() => screen.getByRole('table'))
    expect(screen.queryByText(/page \d+ of \d+/i)).not.toBeInTheDocument()
  })

  describe('add expense modal', () => {
    it('does not show modal when on /expenses', () => {
      renderPage('/expenses')
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
    })

    it('shows modal when on /expenses/add', async () => {
      renderPage('/expenses/add')
      await waitFor(() => {
        expect(screen.getByRole('dialog')).toBeInTheDocument()
      })
    })

    it('modal contains the expense form', async () => {
      renderPage('/expenses/add')
      await waitFor(() => {
        expect(screen.getByTestId('expense-form')).toBeInTheDocument()
      })
    })

    it('closes modal and navigates to /expenses on cancel', async () => {
      const user = userEvent.setup()
      renderPage('/expenses/add')
      await waitFor(() => screen.getByRole('dialog'))
      await user.click(screen.getByRole('button', { name: /^cancel$/i }))
      expect(mockNavigate).toHaveBeenCalledWith('/expenses')
    })

    it('closes modal via X button', async () => {
      const user = userEvent.setup()
      renderPage('/expenses/add')
      await waitFor(() => screen.getByRole('dialog'))
      await user.click(screen.getByRole('button', { name: /close/i }))
      expect(mockNavigate).toHaveBeenCalledWith('/expenses')
    })

    it('calls addExpense and navigates on successful submit', async () => {
      const user = userEvent.setup()
      renderPage('/expenses/add')
      await waitFor(() => screen.getByRole('dialog'))
      await user.click(screen.getByRole('button', { name: /^save$/i }))
      await waitFor(() => {
        expect(mockAddExpense).toHaveBeenCalledOnce()
      })
      expect(mockNavigate).toHaveBeenCalledWith('/expenses')
    })

    it('does not navigate when addExpense fails', async () => {
      mockAddExpense.mockResolvedValue({ ok: false })
      const user = userEvent.setup()
      renderPage('/expenses/add')
      await waitFor(() => screen.getByRole('dialog'))
      await user.click(screen.getByRole('button', { name: /^save$/i }))
      await waitFor(() => {
        expect(mockAddExpense).toHaveBeenCalledOnce()
      })
      expect(mockNavigate).not.toHaveBeenCalledWith('/expenses')
    })
  })
})
