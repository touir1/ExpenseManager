import { describe, it, expect, vi, beforeEach } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { render, screen, waitFor, fireEvent, act } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import SettingsPage from '@/features/dashboard/pages/SettingsPage'

const {
  mockUseExpensesData, mockGetConfig, mockUpdateConfig, mockShow, mockGetNotificationPreferences, mockUseAuth,
  mockUpdateDefaultCsvColumnMapping, mockClearDefaultCsvColumnMapping,
} = vi.hoisted(() => ({
  mockUseExpensesData: vi.fn(),
  mockGetConfig: vi.fn(),
  mockUpdateConfig: vi.fn(),
  mockShow: vi.fn(),
  mockGetNotificationPreferences: vi.fn(),
  mockUseAuth: vi.fn(),
  mockUpdateDefaultCsvColumnMapping: vi.fn(),
  mockClearDefaultCsvColumnMapping: vi.fn(),
}))

vi.mock('@/features/expenses/ExpensesDataContext', () => ({
  useExpensesData: () => mockUseExpensesData(),
}))

vi.mock('@/features/settings/services/userConfigApi.service', () => ({
  getConfig: () => mockGetConfig(),
  updateConfig: (...args: unknown[]) => mockUpdateConfig(...args),
  updateDefaultCsvColumnMapping: (...args: unknown[]) => mockUpdateDefaultCsvColumnMapping(...args),
  clearDefaultCsvColumnMapping: (...args: unknown[]) => mockClearDefaultCsvColumnMapping(...args),
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

  afterEach(() => {
    vi.useRealTimers()
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

  it('shows checkmark icon and saved text after successful currency save', async () => {
    mockUpdateConfig.mockResolvedValue({ ok: true })
    renderSettings()
    const saveBtn = screen.getAllByRole('button', { name: /save/i })[0]
    fireEvent.click(saveBtn)
    await waitFor(() => {
      expect(saveBtn.querySelector('svg')).toBeInTheDocument()
      expect(saveBtn).toHaveTextContent(/saved/i)
    })
  })

  it('aria-live region announces saved state for currency card', async () => {
    mockUpdateConfig.mockResolvedValue({ ok: true })
    renderSettings()
    fireEvent.click(screen.getAllByRole('button', { name: /save/i })[0])
    await waitFor(() => {
      const live = document.querySelector('[aria-live="polite"]')
      expect(live).toHaveTextContent(/saved/i)
    })
  })

  it('shows error toast when currency save fails', async () => {
    mockUpdateConfig.mockResolvedValue({ ok: false })
    renderSettings()
    fireEvent.click(screen.getAllByRole('button', { name: /save/i })[0])
    await waitFor(() => expect(mockShow).toHaveBeenCalledWith(expect.any(String), 'error'))
  })

  it('shows checkmark and saved text after successful category save', async () => {
    mockUpdateConfig.mockResolvedValue({ ok: true })
    renderSettings()
    const saveBtns = screen.getAllByRole('button', { name: /save/i })
    fireEvent.click(saveBtns[1])
    await waitFor(() => {
      expect(saveBtns[1].querySelector('svg')).toBeInTheDocument()
      expect(saveBtns[1]).toHaveTextContent(/saved/i)
    })
  })

  // ── Default CSV Columns card ─────────────────────────────────────────────────

  it('renders empty state when defaultCsvColumnMapping is null', async () => {
    mockGetConfig.mockResolvedValue({ ok: true, data: { defaultCurrencyId: null, defaultCurrency: null, defaultCategoryId: null, defaultCsvColumnMapping: null } })
    renderSettings()
    await waitFor(() => {
      expect(screen.getByText(/no default column mapping saved yet/i)).toBeInTheDocument()
    })
  })

  it('renders existing mapping rows when set', async () => {
    mockGetConfig.mockResolvedValue({
      ok: true,
      data: { defaultCurrencyId: null, defaultCurrency: null, defaultCategoryId: null, defaultCsvColumnMapping: { sum: 'amount', cur: 'currency_code' } },
    })
    renderSettings()
    await waitFor(() => {
      expect(screen.getByDisplayValue('sum')).toBeInTheDocument()
      expect(screen.getByDisplayValue('cur')).toBeInTheDocument()
    })
  })

  it('"Add column" appends an editable row', async () => {
    mockGetConfig.mockResolvedValue({ ok: true, data: { defaultCurrencyId: null, defaultCurrency: null, defaultCategoryId: null, defaultCsvColumnMapping: null } })
    renderSettings()
    await waitFor(() => screen.getByText(/no default column mapping saved yet/i))

    fireEvent.click(screen.getByRole('button', { name: /add column/i }))

    expect(screen.getByPlaceholderText('CSV column')).toBeInTheDocument()
  })

  it('Save persists the edited mapping and shows saved confirmation', async () => {
    mockGetConfig.mockResolvedValue({ ok: true, data: { defaultCurrencyId: null, defaultCurrency: null, defaultCategoryId: null, defaultCsvColumnMapping: null } })
    mockUpdateDefaultCsvColumnMapping.mockResolvedValue({ ok: true })
    renderSettings()
    await waitFor(() => screen.getByText(/no default column mapping saved yet/i))

    fireEvent.click(screen.getByRole('button', { name: /add column/i }))
    fireEvent.change(screen.getByPlaceholderText('CSV column'), { target: { value: 'sum' } })

    const saveBtns = screen.getAllByRole('button', { name: /save/i })
    fireEvent.click(saveBtns[saveBtns.length - 1])

    await waitFor(() => {
      expect(mockUpdateDefaultCsvColumnMapping).toHaveBeenCalledWith({ sum: 'date' })
    })
  })

  it('"Clear default mapping" calls clearDefaultCsvColumnMapping and reverts to empty state', async () => {
    mockGetConfig.mockResolvedValue({
      ok: true,
      data: { defaultCurrencyId: null, defaultCurrency: null, defaultCategoryId: null, defaultCsvColumnMapping: { sum: 'amount' } },
    })
    mockClearDefaultCsvColumnMapping.mockResolvedValue({ ok: true })
    renderSettings()
    await waitFor(() => screen.getByDisplayValue('sum'))

    fireEvent.click(screen.getByRole('button', { name: /clear default mapping/i }))

    await waitFor(() => {
      expect(mockClearDefaultCsvColumnMapping).toHaveBeenCalled()
      expect(screen.getByText(/no default column mapping saved yet/i)).toBeInTheDocument()
    })
  })
})
