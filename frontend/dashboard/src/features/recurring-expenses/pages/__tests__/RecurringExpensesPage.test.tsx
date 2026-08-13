import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import RecurringExpensesPage from '../RecurringExpensesPage'

vi.mock('@/hooks/usePageTitle', () => ({ usePageTitle: () => undefined }))

const showMock = vi.fn()
vi.mock('@/components/Toast', () => ({
  useToast: () => ({ show: showMock }),
}))

const mockGetAll = vi.fn()
const mockGetById = vi.fn()
const mockCreate = vi.fn()
const mockUpdate = vi.fn()
const mockRemove = vi.fn()

vi.mock('@/features/recurring-expenses/services/recurringExpenseApi.service', () => ({
  getAll: (...args: unknown[]) => mockGetAll(...args),
  getById: (...args: unknown[]) => mockGetById(...args),
  create: (...args: unknown[]) => mockCreate(...args),
  update: (...args: unknown[]) => mockUpdate(...args),
  remove: (...args: unknown[]) => mockRemove(...args),
}))

vi.mock('@/features/expenses/ExpensesDataContext', () => ({
  useExpensesData: () => ({
    categories: [{ id: 1, name: 'Food', description: undefined, subcategories: [] }],
    currencies: [{ id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 }],
    isLoading: false,
    refresh: vi.fn(),
  }),
}))

vi.mock('@/features/families/FamilyContext', () => ({
  useFamilies: () => ({ families: [], activeFamilyId: null, setActiveFamilyId: vi.fn(), isLoading: false, refresh: vi.fn() }),
}))

const item = {
  id: 1,
  description: 'Netflix',
  amount: 15.99,
  currencyId: 1,
  currency: { id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
  categoryId: 1,
  category: { id: 1, name: 'Food' },
  subcategoryId: null,
  subcategory: null,
  familyId: null,
  frequencyId: 2,
  nextDueDate: '2026-09-01',
  frequency: 'Monthly',
  isActive: true,
  autoCreate: false,
}

function renderPage() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={qc}>
      <RecurringExpensesPage />
    </QueryClientProvider>,
  )
}

describe('RecurringExpensesPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockGetAll.mockResolvedValue({ ok: true, status: 200, data: [] })
  })

  it('shows empty state when there are no recurring expenses', async () => {
    renderPage()
    await waitFor(() => {
      expect(screen.getByText(/no recurring expenses/i)).toBeInTheDocument()
    })
  })

  it('renders a table row for each recurring expense', async () => {
    mockGetAll.mockResolvedValue({ ok: true, status: 200, data: [item] })
    renderPage()
    await waitFor(() => {
      expect(screen.getByText('Netflix')).toBeInTheDocument()
    })
  })

  it('shows an Auto badge for autoCreate items', async () => {
    mockGetAll.mockResolvedValue({ ok: true, status: 200, data: [{ ...item, autoCreate: true }] })
    renderPage()
    await waitFor(() => {
      expect(screen.getByText('Auto')).toBeInTheDocument()
    })
  })

  it('opens the add modal when the add button is clicked', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => expect(mockGetAll).toHaveBeenCalled())
    await user.click(screen.getByRole('button', { name: /add recurring expense/i }))
    expect(screen.getByRole('dialog')).toBeInTheDocument()
  })

  it('opens a delete confirmation modal and deletes on confirm', async () => {
    mockGetAll.mockResolvedValue({ ok: true, status: 200, data: [item] })
    mockRemove.mockResolvedValue({ ok: true, status: 204 })
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('Netflix'))

    await user.click(screen.getByRole('button', { name: 'Delete' }))
    expect(screen.getByText(/delete recurring expense/i)).toBeInTheDocument()

    const confirmButtons = screen.getAllByRole('button', { name: 'Delete' })
    await user.click(confirmButtons[confirmButtons.length - 1])
    await waitFor(() => {
      expect(mockRemove).toHaveBeenCalledWith(1)
    })
  })

  it('toggles includeInactive checkbox and refetches', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => expect(mockGetAll).toHaveBeenCalledWith(false))
    await user.click(screen.getByRole('checkbox', { name: /show paused/i }))
    await waitFor(() => expect(mockGetAll).toHaveBeenCalledWith(true))
  })
})
