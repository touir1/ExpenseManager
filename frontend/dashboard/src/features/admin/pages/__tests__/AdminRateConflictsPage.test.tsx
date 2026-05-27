import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import AdminRateConflictsPage from '../AdminRateConflictsPage'

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}))
vi.mock('@/hooks/usePageTitle', () => ({ usePageTitle: vi.fn() }))

const mockGetPendingConflicts = vi.fn()
const mockResolveConflict = vi.fn()
const mockGetCurrencies = vi.fn()

vi.mock('@/features/admin/services/adminRatesApi.service', () => ({
  getPendingConflicts: (...a: unknown[]) => mockGetPendingConflicts(...a),
  resolveConflict: (...a: unknown[]) => mockResolveConflict(...a),
}))

vi.mock('@/features/expenses/services/currenciesApi.service', () => ({
  getCurrencies: (...a: unknown[]) => mockGetCurrencies(...a),
}))

const currencies = [
  { id: 1, code: 'USD', name: 'US Dollar', symbol: '$', decimals: 2 },
  { id: 2, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
]

const conflicts = [
  {
    id: 1,
    sourceCurrencyId: 1,
    destinationCurrencyId: 2,
    date: '2024-01-01',
    autoRate: 1.08,
    manualRate: 1.10,
    conflictStatusId: 1,
  },
]

function renderPage() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={qc}>
      <AdminRateConflictsPage />
    </QueryClientProvider>
  )
}

describe('AdminRateConflictsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockGetPendingConflicts.mockResolvedValue({ ok: true, data: conflicts })
    mockGetCurrencies.mockResolvedValue({ ok: true, data: currencies })
    mockResolveConflict.mockResolvedValue({ ok: true })
  })

  it('renders conflicts after loading', async () => {
    renderPage()
    await waitFor(() => expect(screen.getByText('2024-01-01')).toBeInTheDocument())
  })

  it('shows resolve all button when conflicts exist', async () => {
    renderPage()
    await waitFor(() => screen.getByText('2024-01-01'))
    expect(screen.getByText('admin.rateConflicts.resolveAll')).toBeInTheDocument()
  })

  it('shows auto/manual rate info', async () => {
    renderPage()
    await waitFor(() => screen.getByText('2024-01-01'))
    expect(document.body.textContent).toContain('admin.rateConflicts.autoRate')
    expect(document.body.textContent).toContain('admin.rateConflicts.manualRate')
  })

  it('calls resolveConflict with AcceptAuto when resolve clicked', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('admin.rateConflicts.resolve'))
    await user.click(screen.getByText('admin.rateConflicts.resolve'))
    await waitFor(() => expect(mockResolveConflict).toHaveBeenCalledWith(1, 'AcceptAuto', undefined))
  })

  it('calls resolveConflict for all when resolve all clicked', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('admin.rateConflicts.resolveAll'))
    await user.click(screen.getByText('admin.rateConflicts.resolveAll'))
    await waitFor(() => expect(mockResolveConflict).toHaveBeenCalledWith(1, 'AcceptAuto', undefined))
  })

  it('shows no conflicts message when list is empty', async () => {
    mockGetPendingConflicts.mockResolvedValue({ ok: true, data: [] })
    renderPage()
    await waitFor(() => expect(screen.getByText('admin.rateConflicts.noConflicts')).toBeInTheDocument())
  })

  it('shows custom rate input when Custom radio selected', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('admin.rateConflicts.resolve'))
    await user.click(screen.getByText('admin.rateConflicts.custom'))
    expect(screen.getByPlaceholderText('Custom rate')).toBeInTheDocument()
  })
})
