import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'

vi.mock('@/features/auth/AuthContext', () => ({
  useAuth: vi.fn(),
  AuthProvider: ({ children }: any) => <>{children}</>,
}))

vi.mock('@/features/currencies/DisplayCurrencyContext', () => ({
  useDisplayCurrency: vi.fn(),
}))

vi.mock('@/features/expenses/ExpensesDataContext', () => ({
  useExpensesData: vi.fn(),
}))

vi.mock('@/features/settings/ThemeContext', () => ({
  useTheme: () => ({ theme: 'system', setTheme: vi.fn() }),
}))

vi.mock('@/features/settings/services/userConfigApi.service', () => ({
  getConfig: vi.fn(),
  updateConfig: vi.fn(),
}))

vi.mock('@/features/settings/services/notificationPreferencesApi.service', () => ({
  getNotificationPreferences: vi.fn(),
  updateNotificationPreferences: vi.fn(),
}))

vi.mock('@/features/auth/services/authApi.service', () => ({
  deleteAccountRequest: vi.fn(),
}))

vi.mock('@ionic/react', async () => ({
  IonPage: ({ children }: any) => <div>{children}</div>,
  IonHeader: ({ children }: any) => <div>{children}</div>,
  IonToolbar: ({ children }: any) => <div>{children}</div>,
  IonTitle: ({ children }: any) => <h1>{children}</h1>,
  IonContent: ({ children }: any) => <div>{children}</div>,
  IonList: ({ children }: any) => <ul>{children}</ul>,
  IonItem: ({ children }: any) => <li>{children}</li>,
  IonLabel: ({ children }: any) => <label>{children}</label>,
  IonSelect: ({ children, onIonChange, value, slot }: any) => (
    <select data-slot={slot} defaultValue={value} onChange={e => onIonChange?.({ detail: { value: isNaN(Number(e.target.value)) ? e.target.value : Number(e.target.value) } })}>
      {children}
    </select>
  ),
  IonSelectOption: ({ children, value }: any) => <option value={value ?? ''}>{children}</option>,
  IonButton: ({ children, onClick, color }: any) => (
    <button onClick={onClick} data-color={color}>{children}</button>
  ),
  IonText: ({ children }: any) => <span>{children}</span>,
  IonToggle: ({ checked, onIonChange }: any) => (
    <input type="checkbox" role="switch" checked={checked} onChange={() => onIonChange?.({ detail: { checked: !checked } })} />
  ),
  IonAlert: ({ isOpen, buttons }: any) => isOpen ? (
    <div role="alertdialog">
      {buttons?.map((b: any) => <button key={b.text} onClick={b.handler}>{b.text}</button>)}
    </div>
  ) : null,
}))

import { useAuth } from '@/features/auth/AuthContext'
import { useDisplayCurrency } from '@/features/currencies/DisplayCurrencyContext'
import { useExpensesData } from '@/features/expenses/ExpensesDataContext'
import { getConfig, updateConfig } from '@/features/settings/services/userConfigApi.service'
import { getNotificationPreferences, updateNotificationPreferences } from '@/features/settings/services/notificationPreferencesApi.service'
import { deleteAccountRequest } from '@/features/auth/services/authApi.service'
import SettingsPage from '@/features/settings/pages/SettingsPage'

const useAuthMock = useAuth as ReturnType<typeof vi.fn>
const useDisplayCurrencyMock = useDisplayCurrency as ReturnType<typeof vi.fn>
const useExpensesDataMock = useExpensesData as ReturnType<typeof vi.fn>
const mockGetConfig = getConfig as ReturnType<typeof vi.fn>
const mockUpdateConfig = updateConfig as ReturnType<typeof vi.fn>
const mockGetNotificationPreferences = getNotificationPreferences as ReturnType<typeof vi.fn>
const mockUpdateNotificationPreferences = updateNotificationPreferences as ReturnType<typeof vi.fn>
const mockDeleteAccountRequest = deleteAccountRequest as ReturnType<typeof vi.fn>

const mockLogout = vi.fn()
const mockSetDisplayCurrencyId = vi.fn()

function makeWrapper() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return ({ children }: any) => (
    <MemoryRouter>
      <QueryClientProvider client={qc}>{children}</QueryClientProvider>
    </MemoryRouter>
  )
}

describe('SettingsPage', () => {
  beforeEach(() => {
    useAuthMock.mockReturnValue({
      user: { email: 'test@example.com' },
      logout: mockLogout,
      isAuthenticated: true,
    })
    useDisplayCurrencyMock.mockReturnValue({
      displayCurrencyId: 1,
      setDisplayCurrencyId: mockSetDisplayCurrencyId,
    })
    useExpensesDataMock.mockReturnValue({
      currencies: [
        { id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
        { id: 2, code: 'USD', name: 'US Dollar', symbol: '$', decimals: 2 },
      ],
      categories: [{ id: 1, name: 'Food', subcategories: [] }],
      tags: [],
      isLoading: false,
      refresh: vi.fn(),
    })
    mockLogout.mockReset()
    mockSetDisplayCurrencyId.mockReset()
    mockGetConfig.mockReset().mockResolvedValue({ ok: true, status: 200, data: { defaultCurrencyId: 1, defaultCurrency: null, defaultCategoryId: null } })
    mockUpdateConfig.mockReset().mockResolvedValue({ ok: true, status: 200, data: {} })
    mockGetNotificationPreferences.mockReset().mockResolvedValue({ ok: true, status: 200, data: [] })
    mockUpdateNotificationPreferences.mockReset().mockResolvedValue({ ok: true, status: 200, data: [] })
    mockDeleteAccountRequest.mockReset().mockResolvedValue({ ok: true, status: 204 })
  })

  it('renders user email', () => {
    render(<SettingsPage />, { wrapper: makeWrapper() })
    expect(screen.getByText('test@example.com')).toBeDefined()
  })

  it('renders currency selector with currencies', () => {
    render(<SettingsPage />, { wrapper: makeWrapper() })
    expect(screen.getAllByRole('combobox').length).toBeGreaterThan(0)
    expect(screen.getByText('EUR')).toBeDefined()
  })

  it('calls logout when Sign out clicked', () => {
    render(<SettingsPage />, { wrapper: makeWrapper() })
    const signOutBtn = screen.getByText(/sign out/i)
    fireEvent.click(signOutBtn)
    expect(mockLogout).toHaveBeenCalled()
  })

  it('renders language selector', () => {
    render(<SettingsPage />, { wrapper: makeWrapper() })
    const selects = screen.getAllByRole('combobox')
    expect(selects.length).toBeGreaterThanOrEqual(2)
  })

  it('renders default category selector', () => {
    render(<SettingsPage />, { wrapper: makeWrapper() })
    expect(screen.getByText(/default category/i)).toBeDefined()
    expect(screen.getByText('Food')).toBeDefined()
  })

  it('renders notification preference toggles', async () => {
    render(<SettingsPage />, { wrapper: makeWrapper() })
    await waitFor(() => {
      expect(screen.getAllByRole('switch').length).toBeGreaterThan(0)
    })
  })

  it('toggling a notification preference calls the update endpoint', async () => {
    render(<SettingsPage />, { wrapper: makeWrapper() })
    await waitFor(() => screen.getAllByRole('switch'))
    fireEvent.click(screen.getAllByRole('switch')[0])
    await waitFor(() => {
      expect(mockUpdateNotificationPreferences).toHaveBeenCalled()
    })
  })

  it('shows delete confirmation and deletes account', async () => {
    render(<SettingsPage />, { wrapper: makeWrapper() })
    fireEvent.click(screen.getByText(/delete account/i))
    await waitFor(() => screen.getByRole('alertdialog'))
    const confirmBtn = screen.getAllByRole('button', { name: /delete/i }).find(
      b => b.closest('[role="alertdialog"]') !== null
    )
    if (confirmBtn) fireEvent.click(confirmBtn)
    await waitFor(() => {
      expect(mockDeleteAccountRequest).toHaveBeenCalled()
      expect(mockLogout).toHaveBeenCalled()
    })
  })
})
