import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'

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
  IonSelectOption: ({ children, value }: any) => <option value={value}>{children}</option>,
  IonButton: ({ children, onClick, color }: any) => (
    <button onClick={onClick} data-color={color}>{children}</button>
  ),
  IonText: ({ children }: any) => <span>{children}</span>,
}))

import { useAuth } from '@/features/auth/AuthContext'
import { useDisplayCurrency } from '@/features/currencies/DisplayCurrencyContext'
import { useExpensesData } from '@/features/expenses/ExpensesDataContext'
import SettingsPage from '@/features/settings/pages/SettingsPage'

const useAuthMock = useAuth as ReturnType<typeof vi.fn>
const useDisplayCurrencyMock = useDisplayCurrency as ReturnType<typeof vi.fn>
const useExpensesDataMock = useExpensesData as ReturnType<typeof vi.fn>

const mockLogout = vi.fn()
const mockSetDisplayCurrencyId = vi.fn()

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
      categories: [],
      tags: [],
      isLoading: false,
      refresh: vi.fn(),
    })
    mockLogout.mockReset()
    mockSetDisplayCurrencyId.mockReset()
  })

  it('renders user email', () => {
    render(<SettingsPage />, { wrapper: ({ children }) => <MemoryRouter>{children}</MemoryRouter> })
    expect(screen.getByText('test@example.com')).toBeDefined()
  })

  it('renders currency selector with currencies', () => {
    render(<SettingsPage />, { wrapper: ({ children }) => <MemoryRouter>{children}</MemoryRouter> })
    expect(screen.getAllByRole('combobox').length).toBeGreaterThan(0)
    expect(screen.getByText('EUR')).toBeDefined()
  })

  it('calls logout when Sign out clicked', () => {
    render(<SettingsPage />, { wrapper: ({ children }) => <MemoryRouter>{children}</MemoryRouter> })
    const signOutBtn = screen.getByText(/sign out/i)
    fireEvent.click(signOutBtn)
    expect(mockLogout).toHaveBeenCalled()
  })

  it('renders language selector', () => {
    render(<SettingsPage />, { wrapper: ({ children }) => <MemoryRouter>{children}</MemoryRouter> })
    const selects = screen.getAllByRole('combobox')
    expect(selects.length).toBeGreaterThanOrEqual(2)
  })
})
