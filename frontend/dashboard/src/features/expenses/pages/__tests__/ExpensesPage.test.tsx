import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
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
    isLoading: false,
    refresh: vi.fn(),
  }),
}))

const mockGetExpenses = vi.fn()
const mockDeleteExpense = vi.fn()
vi.mock('@/features/expenses/services/expensesApi.service', () => ({
  getExpenses: (...args: unknown[]) => mockGetExpenses(...args),
  deleteExpense: (...args: unknown[]) => mockDeleteExpense(...args),
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

function renderPage() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={qc}>
      <MemoryRouter>
        <ExpensesPage />
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
      expect(screen.getByText('2026-05-01')).toBeInTheDocument()
      expect(screen.getByText(/50.00 EUR/i)).toBeInTheDocument()
    })
  })

  it('renders expense category', async () => {
    renderPage()
    await waitFor(() => {
      expect(screen.getByText('Food')).toBeInTheDocument()
    })
  })

  it('navigates to edit page on Edit click', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByRole('button', { name: /edit/i }))
    await user.click(screen.getByRole('button', { name: /edit/i }))
    expect(mockNavigate).toHaveBeenCalledWith('/expenses/1/edit')
  })

  it('shows confirm modal on Delete click', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByRole('button', { name: /delete/i }))
    await user.click(screen.getByRole('button', { name: /delete/i }))
    expect(screen.getByText(/delete expense/i)).toBeInTheDocument()
  })

  it('calls deleteExpense and closes modal on confirm', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByRole('button', { name: /^delete$/i }))
    // click row Delete to open modal
    await user.click(screen.getByRole('button', { name: /^delete$/i }))
    // modal now open — two Delete buttons exist; the modal's is the last
    const deleteButtons = screen.getAllByRole('button', { name: /^delete$/i })
    await user.click(deleteButtons[deleteButtons.length - 1])
    await waitFor(() => {
      expect(mockDeleteExpense).toHaveBeenCalledWith(1)
    })
  })

  it('closes modal on cancel', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByRole('button', { name: /^delete$/i }))
    await user.click(screen.getByRole('button', { name: /^delete$/i }))
    await user.click(screen.getByRole('button', { name: /^cancel$/i }))
    expect(screen.queryByText(/delete expense/i)).not.toBeInTheDocument()
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
    await waitFor(() => screen.getByText('2026-05-01'))
    expect(screen.queryByText(/page/i)).not.toBeInTheDocument()
  })
})
