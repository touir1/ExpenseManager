import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import EditExpensePage from '../EditExpensePage'
import type { ExpenseDto } from '@/features/expenses/types/expenses.type'

// ── Mocks ─────────────────────────────────────────────────────────────────────

const mockNavigate = vi.fn()
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom')
  return { ...actual, useNavigate: () => mockNavigate }
})

vi.mock('@/features/expenses/ExpensesDataContext', () => ({
  useExpensesData: () => ({
    categories: [{ id: 1, name: 'Food', subcategories: [{ id: 11, name: 'Groceries' }] }],
    currencies: [{ id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 }],
    isLoading: false,
    refresh: vi.fn(),
  }),
}))

vi.mock('@/features/families/FamilyContext', () => ({
  useFamilies: () => ({ families: [], activeFamilyId: null, setActiveFamilyId: vi.fn(), isLoading: false, refresh: vi.fn() }),
}))

vi.mock('@/features/tags/components/TagInput', () => ({
  default: () => <div data-testid="tag-input" />,
}))

vi.mock('@/features/tags/services/tagsApi.service', () => ({
  getTags: vi.fn().mockResolvedValue({ ok: true, data: { own: [], family: [] } }),
}))

const mockGetExpenseById = vi.fn()
const mockUpdateExpense = vi.fn()
vi.mock('@/features/expenses/services/expensesApi.service', () => ({
  getExpenseById: (...args: unknown[]) => mockGetExpenseById(...args),
  updateExpense: (...args: unknown[]) => mockUpdateExpense(...args),
}))

// ── Fixtures ──────────────────────────────────────────────────────────────────

const expense: ExpenseDto = {
  id: 42,
  amount: 99,
  currency: { id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
  date: '2026-05-01',
  category: null,
  subcategory: null,
  description: 'Lunch',
  createdAt: '2026-05-01T10:00:00Z',
  modifiedAt: null,
  modifiedFrom: null,
  tags: [],
  convertedAmount: null,
  displayCurrency: null,
}

// ── Helpers ───────────────────────────────────────────────────────────────────

function renderPage(id = '42') {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={qc}>
      <MemoryRouter initialEntries={[`/expenses/${id}/edit`]}>
        <Routes>
          <Route path="/expenses/:id/edit" element={<EditExpensePage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>
  )
}

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('EditExpensePage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockGetExpenseById.mockResolvedValue({ ok: true, data: expense })
    mockUpdateExpense.mockResolvedValue({ ok: true })
  })

  it('shows loading state initially', () => {
    mockGetExpenseById.mockReturnValue(new Promise(() => {}))
    renderPage()
    expect(screen.getByText(/loading/i)).toBeInTheDocument()
  })

  it('shows not found when fetch fails', async () => {
    mockGetExpenseById.mockResolvedValue({ ok: false })
    renderPage()
    await waitFor(() => {
      expect(screen.getByText(/not found/i)).toBeInTheDocument()
    })
  })

  it('renders form pre-filled with expense data', async () => {
    renderPage()
    await waitFor(() => {
      expect(screen.getByLabelText(/^amount$/i)).toHaveValue(99)
    })
  })

  it('navigates to /expenses on cancel', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByRole('button', { name: /^cancel$/i }))
    await user.click(screen.getByRole('button', { name: /^cancel$/i }))
    expect(mockNavigate).toHaveBeenCalledWith('/expenses')
  })

  it('calls updateExpense with correct id and navigates on success', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByRole('button', { name: /^save$/i }))
    await user.click(screen.getByRole('button', { name: /^save$/i }))
    await waitFor(() => {
      expect(mockUpdateExpense).toHaveBeenCalledWith(42, expect.any(Object))
    })
    expect(mockNavigate).toHaveBeenCalledWith('/expenses')
  })
})
