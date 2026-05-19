import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import AddExpensePage from '../AddExpensePage'

// ── Mocks ─────────────────────────────────────────────────────────────────────

const mockNavigate = vi.fn()
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom')
  return { ...actual, useNavigate: () => mockNavigate }
})

vi.mock('@/features/expenses/ExpensesDataContext', () => ({
  useExpensesData: () => ({
    categories: [{ id: 1, name: 'Food', subcategories: [] }],
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

const mockAddExpense = vi.fn()
vi.mock('@/features/expenses/services/expensesApi.service', () => ({
  addExpense: (...args: unknown[]) => mockAddExpense(...args),
}))

// ── Helpers ───────────────────────────────────────────────────────────────────

function renderPage() {
  render(
    <MemoryRouter>
      <AddExpensePage />
    </MemoryRouter>
  )
}

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('AddExpensePage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockAddExpense.mockResolvedValue({ ok: true })
  })

  it('renders page heading', () => {
    renderPage()
    expect(screen.getByRole('heading')).toBeInTheDocument()
  })

  it('renders ExpenseForm', () => {
    renderPage()
    expect(screen.getByLabelText(/^amount$/i)).toBeInTheDocument()
  })

  it('navigates to /expenses on cancel', async () => {
    const user = userEvent.setup()
    renderPage()
    await user.click(screen.getByRole('button', { name: /^cancel$/i }))
    expect(mockNavigate).toHaveBeenCalledWith('/expenses')
  })

  it('calls addExpense and navigates on successful submit', async () => {
    const user = userEvent.setup()
    renderPage()
    await user.type(screen.getByLabelText(/^amount$/i), '25')
    await user.selectOptions(screen.getByLabelText(/^currency$/i), '1')
    await user.click(screen.getByRole('button', { name: /^save$/i }))
    await waitFor(() => {
      expect(mockAddExpense).toHaveBeenCalledOnce()
    })
    expect(mockNavigate).toHaveBeenCalledWith('/expenses')
  })

  it('does not navigate when addExpense fails', async () => {
    mockAddExpense.mockResolvedValue({ ok: false, error: 'Server error' })
    const user = userEvent.setup()
    renderPage()
    await user.type(screen.getByLabelText(/^amount$/i), '10')
    await user.selectOptions(screen.getByLabelText(/^currency$/i), '1')
    await user.click(screen.getByRole('button', { name: /^save$/i }))
    await waitFor(() => {
      expect(mockAddExpense).toHaveBeenCalledOnce()
    })
    expect(mockNavigate).not.toHaveBeenCalledWith('/expenses')
  })
})
