import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import AppHeader from '@/layouts/AppHeader'

vi.mock('@/features/families/components/FamilySelector', () => ({
  default: () => <div data-testid="family-selector" />,
}))
vi.mock('@/features/currencies/components/DisplayCurrencySelector', () => ({
  default: () => <div data-testid="currency-selector" />,
}))
vi.mock('@/features/expenses/components/AddExpenseModal', () => ({
  default: () => <div data-testid="add-expense-modal" />,
}))
vi.mock('@/features/notifications/components/NotificationBell', () => ({
  default: () => <div data-testid="notification-bell" />,
}))
vi.mock('@/features/notifications/NotificationContext', () => ({
  useNotifications: () => ({ notifications: [], unreadCount: 0, isLoading: false, markRead: vi.fn(), markAllRead: vi.fn(), refresh: vi.fn() }),
}))
vi.mock('@/features/settings/ThemeContext', () => ({
  useTheme: () => ({ theme: 'system', setTheme: vi.fn() }),
}))

function mockMatchMedia() {
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    value: vi.fn().mockImplementation((query: string) => ({
      matches: false,
      media: query,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
    })),
  })
}
mockMatchMedia()

function renderHeader(path = '/dashboard') {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={qc}>
      <MemoryRouter initialEntries={[path]}>
        <Routes>
          <Route path="*" element={<AppHeader onOpenMobileNav={vi.fn()} />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>
  )
}

describe('AppHeader', () => {
  it('shows the mobile menu toggle button', () => {
    renderHeader()
    expect(screen.getByRole('button', { name: /toggle menu/i })).toBeInTheDocument()
  })

  it('calls onOpenMobileNav when toggle button is clicked', async () => {
    const onOpenMobileNav = vi.fn()
    const user = userEvent.setup()
    const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={qc}>
        <MemoryRouter initialEntries={['/dashboard']}>
          <AppHeader onOpenMobileNav={onOpenMobileNav} />
        </MemoryRouter>
      </QueryClientProvider>
    )
    await user.click(screen.getByRole('button', { name: /toggle menu/i }))
    expect(onOpenMobileNav).toHaveBeenCalledOnce()
  })

  it('shows page title for the dashboard route', () => {
    renderHeader('/dashboard')
    expect(screen.getByRole('heading', { name: /dashboard/i })).toBeInTheDocument()
  })

  it('shows page title for the expenses route', () => {
    renderHeader('/expenses')
    expect(screen.getByRole('heading', { name: /expenses/i })).toBeInTheDocument()
  })

  it('shows FamilySelector and DisplayCurrencySelector on dashboard route', () => {
    renderHeader('/dashboard')
    expect(screen.getByTestId('family-selector')).toBeInTheDocument()
    expect(screen.getByTestId('currency-selector')).toBeInTheDocument()
  })

  it('hides FamilySelector and DisplayCurrencySelector on settings route', () => {
    renderHeader('/settings')
    expect(screen.queryByTestId('family-selector')).not.toBeInTheDocument()
    expect(screen.queryByTestId('currency-selector')).not.toBeInTheDocument()
  })

  it('shows add expense button', () => {
    renderHeader()
    expect(screen.getByRole('button', { name: /add expense/i })).toBeInTheDocument()
  })

  it('opens the add expense modal when clicked', async () => {
    const user = userEvent.setup()
    renderHeader()
    await user.click(screen.getByRole('button', { name: /add expense/i }))
    expect(screen.getByTestId('add-expense-modal')).toBeInTheDocument()
  })
})
