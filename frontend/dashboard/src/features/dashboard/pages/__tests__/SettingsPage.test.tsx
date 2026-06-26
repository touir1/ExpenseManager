import { describe, it, expect, vi, beforeEach } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import SettingsPage from '@/features/dashboard/pages/SettingsPage'

const { mockUseExpensesData, mockGetConfig, mockShow, mockGetNotificationPreferences, mockUseAuth } = vi.hoisted(() => ({
  mockUseExpensesData: vi.fn(),
  mockGetConfig: vi.fn(),
  mockShow: vi.fn(),
  mockGetNotificationPreferences: vi.fn(),
  mockUseAuth: vi.fn(),
}))

vi.mock('@/features/expenses/ExpensesDataContext', () => ({
  useExpensesData: () => mockUseExpensesData(),
}))

vi.mock('@/features/settings/services/userConfigApi.service', () => ({
  getConfig: () => mockGetConfig(),
  updateConfig: vi.fn(),
}))

vi.mock('@/features/settings/services/notificationPreferencesApi.service', () => ({
  getNotificationPreferences: () => mockGetNotificationPreferences(),
  updateNotificationPreferences: vi.fn(),
}))

vi.mock('@/features/auth/services/authApi.service', () => ({
  deleteAccountRequest: vi.fn(),
}))

vi.mock('@/features/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth(),
}))

vi.mock('@/components/Toast', () => ({
  useToast: () => ({ show: mockShow }),
}))

vi.mock('@/features/settings/ThemeContext', () => ({
  useTheme: () => ({ theme: 'system', setTheme: vi.fn() }),
}))

vi.mock('@/components/ThemeToggle', () => ({
  default: () => null,
}))

const currencies = [
  { id: 1, code: 'USD', name: 'US Dollar', symbol: '$', decimals: 2 },
  { id: 2, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
]

const categories = [
  { id: 1, name: 'Food', subcategories: [] },
  { id: 2, name: 'Transport', subcategories: [] },
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
    mockUseExpensesData.mockReturnValue({ currencies, categories, tags: [], isLoading: false, refresh: vi.fn() })
    mockGetConfig.mockResolvedValue({ ok: false, data: null })
    mockGetNotificationPreferences.mockResolvedValue({ ok: true, data: [] })
    mockUseAuth.mockReturnValue({ logout: vi.fn(), isAuthenticated: true })
  })

  it('renders Settings heading', () => {
    renderSettings()
    expect(screen.getByRole('heading', { name: /settings/i })).toBeInTheDocument()
  })

  it('renders the Password section heading', () => {
    renderSettings()
    expect(screen.getAllByRole('heading', { name: /password/i }).length).toBeGreaterThan(0)
  })

  it('renders a Change Password link pointing to /change-password', () => {
    renderSettings()
    const link = screen.getByRole('link', { name: /change password/i })
    expect(link).toBeInTheDocument()
    expect(link).toHaveAttribute('href', '/change-password')
  })

  it('renders Default Currency section heading', () => {
    renderSettings()
    expect(screen.getAllByRole('heading', { name: /default currency/i }).length).toBeGreaterThan(0)
  })

  it('renders Save button in default currency card', () => {
    renderSettings()
    expect(screen.getAllByRole('button', { name: /save/i }).length).toBeGreaterThan(0)
  })

  it('pre-selects currency from config on load', async () => {
    mockGetConfig.mockResolvedValue({ ok: true, data: { defaultCurrencyId: 2, defaultCurrency: currencies[1] } })
    renderSettings()
    const selects = await screen.findAllByRole('combobox')
    const currencySelect = selects.find(s => s.getAttribute('aria-label') === 'Default Currency')
    expect(currencySelect).toBeDefined()
    await waitFor(() => expect(currencySelect).toHaveValue('2'))
  })

  it('renders Account section heading', () => {
    renderSettings()
    expect(screen.getByText('Account')).toBeInTheDocument()
  })

  it('renders Preferences section heading', () => {
    renderSettings()
    expect(screen.getByText('Preferences')).toBeInTheDocument()
  })

  it('renders Danger Zone section heading', () => {
    renderSettings()
    expect(screen.getByText('Danger Zone')).toBeInTheDocument()
  })

  it('renders Download CSV button', () => {
    renderSettings()
    expect(screen.getByRole('button', { name: /download csv/i })).toBeInTheDocument()
  })

  it('renders Delete Account button', () => {
    renderSettings()
    expect(screen.getByRole('button', { name: /delete account/i })).toBeInTheDocument()
  })

  it('renders notification preferences toggles', async () => {
    renderSettings()
    const checkboxes = await screen.findAllByRole('checkbox')
    expect(checkboxes.length).toBe(7)
  })
})
