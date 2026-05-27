import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import AdminRatesPage from '../AdminRatesPage'

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}))
vi.mock('@/hooks/usePageTitle', () => ({ usePageTitle: vi.fn() }))

const mockGetCurrencies = vi.fn()
const mockGetRateHistory = vi.fn()
const mockAddManualRate = vi.fn()
const mockRefreshRates = vi.fn()

vi.mock('@/features/expenses/services/currenciesApi.service', () => ({
  getCurrencies: (...a: unknown[]) => mockGetCurrencies(...a),
}))

vi.mock('@/features/admin/services/adminRatesApi.service', () => ({
  getRateHistory: (...a: unknown[]) => mockGetRateHistory(...a),
  addManualRate: (...a: unknown[]) => mockAddManualRate(...a),
  refreshRates: (...a: unknown[]) => mockRefreshRates(...a),
}))

const currencies = [
  { id: 1, code: 'USD', name: 'US Dollar', symbol: '$', decimals: 2 },
  { id: 2, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
]

const rateHistory = [
  { id: 1, date: '2024-01-01', rate: 1.08, rateSourceId: 1 },
  { id: 2, date: '2024-01-02', rate: 1.09, rateSourceId: 2 },
]

function renderPage() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={qc}>
      <AdminRatesPage />
    </QueryClientProvider>
  )
}

describe('AdminRatesPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockGetCurrencies.mockResolvedValue({ ok: true, data: currencies })
    mockGetRateHistory.mockResolvedValue({ ok: true, data: rateHistory })
    mockAddManualRate.mockResolvedValue({ ok: true })
    mockRefreshRates.mockResolvedValue({ ok: true })
  })

  it('renders page heading and currency dropdowns', async () => {
    renderPage()
    await waitFor(() => screen.getByText('admin.rates.pageTitle'))
    await waitFor(() => screen.getAllByRole('option', { name: 'USD' }))
    const sources = screen.getAllByRole('option', { name: 'USD' })
    expect(sources.length).toBeGreaterThan(0)
  })

  it('shows rate history when both currencies selected', async () => {
    renderPage()
    await waitFor(() => screen.getAllByRole('option', { name: 'USD' }))
    const selects = screen.getAllByRole('combobox')
    fireEvent.change(selects[0], { target: { value: '1' } })
    fireEvent.change(selects[1], { target: { value: '2' } })
    await waitFor(() => expect(screen.getByText('2024-01-01')).toBeInTheDocument())
    expect(screen.getByText('2024-01-02')).toBeInTheDocument()
  })

  it('opens backfill modal when backfill button clicked', async () => {
    renderPage()
    await waitFor(() => screen.getByText('admin.rates.backfill'))
    await userEvent.setup().click(screen.getByText('admin.rates.backfill'))
    const dateInputs = screen.getAllByDisplayValue('')
    expect(dateInputs.some(el => (el as HTMLInputElement).type === 'date')).toBe(true)
  })

  it('calls refreshRates when backfill modal submitted', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('admin.rates.backfill'))
    await user.click(screen.getByText('admin.rates.backfill'))
    const dateInputs = screen.getAllByDisplayValue('')
    const dateInput = dateInputs.find(el => (el as HTMLInputElement).type === 'date')!
    fireEvent.change(dateInput, { target: { value: '2024-01-01' } })
    const backfillBtns = screen.getAllByText('admin.rates.backfill')
    const submitBtn = backfillBtns.find(el => el.tagName === 'BUTTON' && el.closest('.fixed'))!
    await user.click(submitBtn)
    await waitFor(() => expect(mockRefreshRates).toHaveBeenCalled())
  })
})
