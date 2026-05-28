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
const rateHistory = {
  rates: [
    { id: 1, sourceCurrencyId: 1, destinationCurrencyId: 2, rate: 0.92, date: '2024-01-01', rateSource: 'Auto' },
    { id: 2, sourceCurrencyId: 1, destinationCurrencyId: 2, rate: 0.90, date: '2023-12-31', rateSource: 'Manual' },
  ],
  total: 2, page: 1, pageSize: 50,
}

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

  it('renders rate table on page load without currency selection', async () => {
    renderPage()
    await waitFor(() => expect(screen.getByText('2024-01-01')).toBeInTheDocument())
    expect(screen.getByText('2023-12-31')).toBeInTheDocument()
  })

  it('shows empty state when no rates', async () => {
    mockGetRateHistory.mockResolvedValue({ ok: true, data: { rates: [], total: 0, page: 1, pageSize: 50 } })
    renderPage()
    await waitFor(() => expect(screen.getByText('admin.rates.noRates')).toBeInTheDocument())
  })

  it('shows rateSource string directly', async () => {
    renderPage()
    await waitFor(() => expect(screen.getByText('Auto')).toBeInTheDocument())
    expect(screen.getByText('Manual')).toBeInTheDocument()
  })

  it('shows Add Manual button always enabled', () => {
    renderPage()
    const addBtn = screen.getByText('admin.rates.addManual')
    expect(addBtn).not.toBeDisabled()
  })

  it('opens add manual modal with source/destination combobox inputs', async () => {
    const user = userEvent.setup()
    renderPage()
    await user.click(screen.getByText('admin.rates.addManual'))
    expect(screen.getByText('admin.rates.sourceCurrency')).toBeInTheDocument()
    expect(screen.getByText('admin.rates.destinationCurrency')).toBeInTheDocument()
    expect(screen.getByPlaceholderText('admin.rates.rateValue')).toBeInTheDocument()
  })

  it('opens backfill modal with From and To date inputs', async () => {
    const user = userEvent.setup()
    renderPage()
    await user.click(screen.getByText('admin.rates.backfill'))
    expect(screen.getByText('admin.rates.from')).toBeInTheDocument()
    expect(screen.getByText('admin.rates.to')).toBeInTheDocument()
  })

  it('calls refreshRates with from and to when backfill submitted', async () => {
    const user = userEvent.setup()
    renderPage()
    await user.click(screen.getByText('admin.rates.backfill'))
    const dateInputs = screen.getAllByDisplayValue('')
    const fromInput = dateInputs.find(el => el.getAttribute('type') === 'date' && el.closest('.fixed'))
    fireEvent.change(fromInput!, { target: { value: '2024-01-01' } })
    const toInputs = screen.getAllByDisplayValue('')
    const toInput = toInputs.find(el => el.getAttribute('type') === 'date' && el.closest('.fixed') && el !== fromInput)
    if (toInput) {
      fireEvent.change(toInput, { target: { value: '2024-01-31' } })
    }
    const backfillBtns = screen.getAllByText('admin.rates.backfill')
    const submitBtn = backfillBtns.find(el => el.tagName === 'BUTTON' && el.closest('.fixed'))
    await user.click(submitBtn!)
    await waitFor(() => expect(mockRefreshRates).toHaveBeenCalled())
  })

  it('shows pagination when totalPages > 1', async () => {
    mockGetRateHistory.mockResolvedValue({
      ok: true,
      data: { rates: rateHistory.rates, total: 200, page: 1, pageSize: 50 },
    })
    renderPage()
    await waitFor(() => expect(screen.getByText('1 / 4')).toBeInTheDocument())
  })

  it('shows From and To column headers', () => {
    renderPage()
    expect(screen.getByText('admin.rates.fromCurrency')).toBeInTheDocument()
    expect(screen.getByText('admin.rates.toCurrency')).toBeInTheDocument()
  })

  it('shows source and destination currency codes in rate rows', async () => {
    renderPage()
    await waitFor(() => expect(screen.getByText('2024-01-01')).toBeInTheDocument())
    expect(screen.getAllByText('USD').length).toBeGreaterThanOrEqual(1)
    expect(screen.getAllByText('EUR').length).toBeGreaterThanOrEqual(1)
  })

  it('calls addManualRate when add modal form submitted', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('2024-01-01'))
    await user.click(screen.getByText('admin.rates.addManual'))

    const modal = document.querySelector('.fixed')!
    const inputs = modal.querySelectorAll('input[type="text"]')
    const srcInput = inputs[0] as HTMLElement
    const dstInput = inputs[1] as HTMLElement

    fireEvent.focus(srcInput)
    await waitFor(() => screen.getByRole('option', { name: 'USD' }))
    fireEvent.mouseDown(screen.getByRole('option', { name: 'USD' }))

    fireEvent.focus(dstInput)
    await waitFor(() => screen.getByRole('option', { name: 'EUR' }))
    fireEvent.mouseDown(screen.getByRole('option', { name: 'EUR' }))

    const dateInput = modal.querySelector('input[type="date"]') as HTMLInputElement
    fireEvent.change(dateInput, { target: { value: '2024-02-01' } })

    const rateInput = modal.querySelector('input[type="number"]') as HTMLInputElement
    fireEvent.change(rateInput, { target: { value: '0.95' } })

    const saveBtn = Array.from(modal.querySelectorAll('button')).find(b => b.textContent === 'common.save')!
    await user.click(saveBtn)
    await waitFor(() => expect(mockAddManualRate).toHaveBeenCalledWith(1, 2, '2024-02-01', 0.95))
  })
})
