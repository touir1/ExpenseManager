import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import AdminCurrenciesPage from '../AdminCurrenciesPage'

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string, opts?: Record<string, unknown>) => opts ? `${key}:${JSON.stringify(opts)}` : key }),
}))
vi.mock('@/hooks/usePageTitle', () => ({ usePageTitle: vi.fn() }))

const mockGetCurrencies = vi.fn()
const mockAddCurrency = vi.fn()
const mockUpdateCurrency = vi.fn()
const mockDeleteCurrency = vi.fn()
const mockGetCurrencyDefaults = vi.fn()
const mockSetDefaultRate = vi.fn()

vi.mock('@/features/expenses/services/currenciesApi.service', () => ({
  getCurrencies: (...a: unknown[]) => mockGetCurrencies(...a),
}))

vi.mock('@/features/admin/services/adminCurrenciesApi.service', () => ({
  addCurrency: (...a: unknown[]) => mockAddCurrency(...a),
  updateCurrency: (...a: unknown[]) => mockUpdateCurrency(...a),
  deleteCurrency: (...a: unknown[]) => mockDeleteCurrency(...a),
  getCurrencyDefaults: (...a: unknown[]) => mockGetCurrencyDefaults(...a),
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
    mockUpdateCurrency.mockResolvedValue({ ok: true })
    mockDeleteCurrency.mockResolvedValue({ ok: true })
    mockGetCurrencyDefaults.mockResolvedValue({ ok: true, data: [] })
    mockSetDefaultRate.mockResolvedValue({ ok: true })
  })

  it('renders currency list after loading', async () => {
    renderPage()
    await waitFor(() => expect(screen.getByText('USD')).toBeInTheDocument())
    expect(screen.getByText('EUR')).toBeInTheDocument()
  })

  it('shows Add button and per-row action buttons', async () => {
    renderPage()
    await waitFor(() => screen.getByText('USD'))
    expect(screen.getByText('admin.currencies.add')).toBeInTheDocument()
    const editBtns = screen.getAllByText('admin.currencies.edit')
    expect(editBtns).toHaveLength(2)
    const deleteBtns = screen.getAllByText('admin.currencies.delete')
    expect(deleteBtns).toHaveLength(2)
    const defaultsBtns = screen.getAllByText('admin.currencies.defaults')
    expect(defaultsBtns).toHaveLength(2)
  })

  it('opens add currency modal', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('USD'))
    await user.click(screen.getByText('admin.currencies.add'))
    expect(screen.getByPlaceholderText('admin.currencies.codeLabel')).toBeInTheDocument()
  })

  it('calls addCurrency when add form submitted', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('USD'))
    await user.click(screen.getByText('admin.currencies.add'))
    await user.type(screen.getByPlaceholderText('admin.currencies.codeLabel'), 'JPY')
    await user.type(screen.getByPlaceholderText('admin.currencies.nameLabel'), 'Japanese Yen')
    await user.type(screen.getByPlaceholderText('admin.currencies.symbolLabel'), '¥')
    const saveBtn = screen.getByText('admin.currencies.save')
    await user.click(saveBtn)
    await waitFor(() => expect(mockAddCurrency).toHaveBeenCalledWith('JPY', 'Japanese Yen', '¥', 2))
  })

  it('opens edit modal when edit button clicked', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('USD'))
    const editBtns = screen.getAllByText('admin.currencies.edit')
    await user.click(editBtns[0])
    const nameInput = screen.getByDisplayValue('US Dollar')
    expect(nameInput).toBeInTheDocument()
  })

  it('calls updateCurrency when edit form saved', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('USD'))
    const editBtns = screen.getAllByText('admin.currencies.edit')
    await user.click(editBtns[0])
    const nameInput = screen.getByDisplayValue('US Dollar')
    await user.clear(nameInput)
    await user.type(nameInput, 'United States Dollar')
    await user.click(screen.getByText('admin.currencies.save'))
    await waitFor(() => expect(mockUpdateCurrency).toHaveBeenCalledWith(1, 'United States Dollar', '$', 2))
  })

  it('opens delete modal when delete button clicked', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('USD'))
    const deleteBtns = screen.getAllByText('admin.currencies.delete')
    await user.click(deleteBtns[0])
    expect(screen.getByText('admin.currencies.deleteConfirm')).toBeInTheDocument()
  })

  it('calls deleteCurrency when delete confirmed', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('USD'))
    const deleteBtns = screen.getAllByText('admin.currencies.delete')
    await user.click(deleteBtns[0])
    const confirmBtn = screen.getAllByText('admin.currencies.delete').find(el => el.tagName === 'BUTTON' && el.closest('.fixed'))
    await user.click(confirmBtn!)
    await waitFor(() => expect(mockDeleteCurrency).toHaveBeenCalledWith(1))
  })

  it('shows in-use error when delete returns 409', async () => {
    mockDeleteCurrency.mockRejectedValue({ status: 409 })
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('USD'))
    const deleteBtns = screen.getAllByText('admin.currencies.delete')
    await user.click(deleteBtns[0])
    const confirmBtn = screen.getAllByText('admin.currencies.delete').find(el => el.tagName === 'BUTTON' && el.closest('.fixed'))
    await user.click(confirmBtn!)
    await waitFor(() => expect(screen.getByText('admin.currencies.inUse')).toBeInTheDocument())
  })

  it('opens defaults modal when defaults button clicked', async () => {
    mockGetCurrencyDefaults.mockResolvedValue({
      ok: true,
      data: [{ destinationCurrencyId: 2, destinationCode: 'EUR', defaultRate: 0.92, lastAutoRate: 0.91, lastAutoRateDate: '2024-01-01' }],
    })
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('USD'))
    const defaultsBtns = screen.getAllByText('admin.currencies.defaults')
    await user.click(defaultsBtns[0])
    await waitFor(() => expect(screen.getAllByText('EUR').length).toBeGreaterThanOrEqual(2))
    expect(mockGetCurrencyDefaults).toHaveBeenCalledWith(1)
  })

  it('renders search input', () => {
    renderPage()
    expect(screen.getByPlaceholderText('admin.currencies.search')).toBeInTheDocument()
  })

  it('filters list by code', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('USD'))
    await user.type(screen.getByPlaceholderText('admin.currencies.search'), 'USD')
    expect(screen.getByText('USD')).toBeInTheDocument()
    expect(screen.queryByText('EUR')).not.toBeInTheDocument()
  })

  it('filters list by name (case-insensitive)', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('USD'))
    await user.type(screen.getByPlaceholderText('admin.currencies.search'), 'euro')
    expect(screen.queryByText('USD')).not.toBeInTheDocument()
    expect(screen.getByText('EUR')).toBeInTheDocument()
  })

  it('shows no-results message when search matches nothing', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('USD'))
    await user.type(screen.getByPlaceholderText('admin.currencies.search'), 'XYZ')
    expect(screen.getByText('admin.currencies.noResults')).toBeInTheDocument()
  })

  it('shows pagination when more than PAGE_SIZE currencies', async () => {
    const many = Array.from({ length: 12 }, (_, i) => ({
      id: i + 1, code: `C${String(i + 1).padStart(2, '0')}`, name: `Currency ${i + 1}`, symbol: '$', decimals: 2,
    }))
    mockGetCurrencies.mockResolvedValue({ ok: true, data: many })
    renderPage()
    await waitFor(() => expect(screen.getByText('1 / 2')).toBeInTheDocument())
  })
})
