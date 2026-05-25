import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { DisplayCurrencyProvider, useDisplayCurrency } from '../DisplayCurrencyContext'

const mockUseAuth = vi.fn()
const mockUseExpensesData = vi.fn()
const mockGetConfig = vi.fn()

vi.mock('@/features/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth(),
}))

vi.mock('@/features/expenses/ExpensesDataContext', () => ({
  useExpensesData: () => mockUseExpensesData(),
}))

vi.mock('@/features/settings/services/userConfigApi.service', () => ({
  getConfig: () => mockGetConfig(),
}))

const currencies = [
  { id: 1, code: 'USD', name: 'US Dollar', symbol: '$', decimals: 2 },
  { id: 2, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
  { id: 3, code: 'GBP', name: 'British Pound', symbol: '£', decimals: 2 },
]

function Consumer() {
  const { displayCurrencyId, setDisplayCurrencyId } = useDisplayCurrency()
  return (
    <>
      <span data-testid="value">{displayCurrencyId ?? 'null'}</span>
      <button onClick={() => setDisplayCurrencyId(3)}>set-3</button>
      <button onClick={() => setDisplayCurrencyId(null)}>clear</button>
    </>
  )
}

function makeQueryClient() {
  return new QueryClient({ defaultOptions: { queries: { retry: false } } })
}

function renderWithProvider(queryClient?: QueryClient) {
  const qc = queryClient ?? makeQueryClient()
  return render(
    <QueryClientProvider client={qc}>
      <DisplayCurrencyProvider>
        <Consumer />
      </DisplayCurrencyProvider>
    </QueryClientProvider>
  )
}

describe('DisplayCurrencyContext', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUseAuth.mockReturnValue({ isAuthenticated: false })
    mockUseExpensesData.mockReturnValue({ currencies: [] })
    mockGetConfig.mockResolvedValue({ ok: false, data: null })
  })

  it('default value is null when not authenticated', () => {
    renderWithProvider()
    expect(screen.getByTestId('value').textContent).toBe('null')
  })

  it('setDisplayCurrencyId updates value', async () => {
    renderWithProvider()
    await userEvent.click(screen.getByText('set-3'))
    expect(screen.getByTestId('value').textContent).toBe('3')
  })

  it('setDisplayCurrencyId(null) clears value', async () => {
    renderWithProvider()
    await userEvent.click(screen.getByText('set-3'))
    await userEvent.click(screen.getByText('clear'))
    expect(screen.getByTestId('value').textContent).toBe('null')
  })

  it('persists within same render (re-render keeps value)', async () => {
    const qc = makeQueryClient()
    const { rerender } = renderWithProvider(qc)
    await userEvent.click(screen.getByText('set-3'))
    rerender(
      <QueryClientProvider client={qc}>
        <DisplayCurrencyProvider>
          <Consumer />
        </DisplayCurrencyProvider>
      </QueryClientProvider>
    )
    expect(screen.getByTestId('value').textContent).toBe('3')
  })

  it('throws when used outside provider', () => {
    const consoleError = console.error
    console.error = () => {}
    expect(() => render(<Consumer />)).toThrow('useDisplayCurrency must be used within DisplayCurrencyProvider')
    console.error = consoleError
  })

  it('initializes from user config defaultCurrencyId when authenticated', async () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true })
    mockUseExpensesData.mockReturnValue({ currencies })
    mockGetConfig.mockResolvedValue({ ok: true, data: { defaultCurrencyId: 3, defaultCurrency: currencies[2] } })
    renderWithProvider()
    await waitFor(() => expect(screen.getByTestId('value').textContent).toBe('3'))
  })

  it('falls back to EUR when config has no defaultCurrencyId', async () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true })
    mockUseExpensesData.mockReturnValue({ currencies })
    mockGetConfig.mockResolvedValue({ ok: true, data: { defaultCurrencyId: null, defaultCurrency: null } })
    renderWithProvider()
    await waitFor(() => expect(screen.getByTestId('value').textContent).toBe('2'))
  })

  it('falls back to first currency when config has no defaultCurrencyId and no EUR', async () => {
    const noCurrencies = [
      { id: 1, code: 'USD', name: 'US Dollar', symbol: '$', decimals: 2 },
      { id: 3, code: 'GBP', name: 'British Pound', symbol: '£', decimals: 2 },
    ]
    mockUseAuth.mockReturnValue({ isAuthenticated: true })
    mockUseExpensesData.mockReturnValue({ currencies: noCurrencies })
    mockGetConfig.mockResolvedValue({ ok: true, data: { defaultCurrencyId: null, defaultCurrency: null } })
    renderWithProvider()
    await waitFor(() => expect(screen.getByTestId('value').textContent).toBe('1'))
  })

  it('does not reinitialize after user manually sets currency', async () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true })
    mockUseExpensesData.mockReturnValue({ currencies })
    mockGetConfig.mockResolvedValue({ ok: true, data: { defaultCurrencyId: 2, defaultCurrency: currencies[1] } })
    renderWithProvider()
    await waitFor(() => expect(screen.getByTestId('value').textContent).toBe('2'))
    await userEvent.click(screen.getByText('set-3'))
    expect(screen.getByTestId('value').textContent).toBe('3')
  })
})
