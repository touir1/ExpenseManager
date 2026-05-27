import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import AdminCurrenciesPage from '../AdminCurrenciesPage'

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}))
vi.mock('@/hooks/usePageTitle', () => ({ usePageTitle: vi.fn() }))

const mockGetCurrencies = vi.fn()
const mockAddCurrency = vi.fn()
const mockSetDefaultRate = vi.fn()

vi.mock('@/features/expenses/services/currenciesApi.service', () => ({
  getCurrencies: (...a: unknown[]) => mockGetCurrencies(...a),
}))

vi.mock('@/features/admin/services/adminCurrenciesApi.service', () => ({
  addCurrency: (...a: unknown[]) => mockAddCurrency(...a),
  setDefaultRate: (...a: unknown[]) => mockSetDefaultRate(...a),
}))

const currencies = [
  { id: 1, code: 'USD', name: 'US Dollar', symbol: '$', decimals: 2 },
  { id: 2, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
]

function renderPage() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={qc}>
      <AdminCurrenciesPage />
    </QueryClientProvider>
  )
}

describe('AdminCurrenciesPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockGetCurrencies.mockResolvedValue({ ok: true, data: currencies })
    mockAddCurrency.mockResolvedValue({ ok: true })
    mockSetDefaultRate.mockResolvedValue({ ok: true })
  })

  it('renders currency list after loading', async () => {
    renderPage()
    await waitFor(() => expect(screen.getByText('USD')).toBeInTheDocument())
    expect(screen.getByText('EUR')).toBeInTheDocument()
  })

  it('shows Add Currency and Set Default Rate buttons', async () => {
    renderPage()
    await waitFor(() => screen.getByText('USD'))
    expect(screen.getByText('admin.currencies.add')).toBeInTheDocument()
    expect(screen.getByText('admin.currencies.setDefaultRate')).toBeInTheDocument()
  })

  it('opens add currency modal', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('USD'))
    await user.click(screen.getByText('admin.currencies.add'))
    expect(screen.getByPlaceholderText('Code (3 chars)')).toBeInTheDocument()
  })

  it('calls addCurrency when form submitted', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('USD'))
    await user.click(screen.getByText('admin.currencies.add'))
    await user.type(screen.getByPlaceholderText('Code (3 chars)'), 'JPY')
    await user.type(screen.getByPlaceholderText('Name'), 'Japanese Yen')
    await user.type(screen.getByPlaceholderText('Symbol'), '¥')
    const saves = screen.getAllByText('common.save')
    await user.click(saves[0])
    await waitFor(() => expect(mockAddCurrency).toHaveBeenCalled())
  })

  it('opens set default rate modal', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('USD'))
    await user.click(screen.getByText('admin.currencies.setDefaultRate'))
    expect(screen.getByPlaceholderText('Rate')).toBeInTheDocument()
  })
})
