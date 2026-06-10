import { describe, it, expect, vi, beforeEach } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import SettingsPage from '@/features/dashboard/pages/SettingsPage'

const { mockUseExpensesData, mockGetConfig, mockShow } = vi.hoisted(() => ({
  mockUseExpensesData: vi.fn(),
  mockGetConfig: vi.fn(),
  mockShow: vi.fn(),
}))

vi.mock('@/features/expenses/ExpensesDataContext', () => ({
  useExpensesData: () => mockUseExpensesData(),
}))

vi.mock('@/features/settings/services/userConfigApi.service', () => ({
  getConfig: () => mockGetConfig(),
  updateConfig: vi.fn(),
}))

vi.mock('@/components/Toast', () => ({
  useToast: () => ({ show: mockShow }),
}))

vi.mock('@/features/settings/ThemeContext', () => ({
  useTheme: () => ({ theme: 'system', setTheme: vi.fn() }),
}))

const currencies = [
  { id: 1, code: 'USD', name: 'US Dollar', symbol: '$', decimals: 2 },
  { id: 2, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
]

function makeQueryClient() {
  return new QueryClient({ defaultOptions: { queries: { retry: false } } })
}

function renderSettings(queryClient?: QueryClient) {
  const qc = queryClient ?? makeQueryClient()
  return render(
    <QueryClientProvider client={qc}>
      <MemoryRouter>
        <SettingsPage />
      </MemoryRouter>
    </QueryClientProvider>
  )
}

describe('Settings page', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUseExpensesData.mockReturnValue({ currencies, isLoading: false, refresh: vi.fn() })
    mockGetConfig.mockResolvedValue({ ok: false, data: null })
  })

  it('renders Settings heading', () => {
    renderSettings()
    expect(screen.getByRole('heading', { name: /settings/i })).toBeInTheDocument()
  })

  it('renders the Password section heading', () => {
    renderSettings()
    expect(screen.getByRole('heading', { name: /password/i })).toBeInTheDocument()
  })

  it('renders a Change Password link pointing to /change-password', () => {
    renderSettings()
    const link = screen.getByRole('link', { name: /change password/i })
    expect(link).toBeInTheDocument()
    expect(link).toHaveAttribute('href', '/change-password')
  })

  it('renders Default Currency section heading', () => {
    renderSettings()
    expect(screen.getByRole('heading', { name: /default currency/i })).toBeInTheDocument()
  })

  it('renders Save button in default currency card', () => {
    renderSettings()
    expect(screen.getByRole('button', { name: /save/i })).toBeInTheDocument()
  })

  it('pre-selects currency from config on load', async () => {
    mockGetConfig.mockResolvedValue({ ok: true, data: { defaultCurrencyId: 2, defaultCurrency: currencies[1] } })
    renderSettings()
    const select = await screen.findByRole('combobox')
    await waitFor(() => expect(select).toHaveValue('2'))
  })
})
