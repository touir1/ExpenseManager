import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { RouterProvider, createMemoryRouter, Outlet } from 'react-router-dom'
import { routes } from '../router'

vi.mock('@/layouts/RootLayout', () => ({
  default: () => <Outlet />,
}))

vi.mock('@/features/auth/components/ProtectedRoute', () => ({
  default: ({ children }: { children: React.ReactNode }) => <div data-testid="protected-route">{children}</div>,
}))

vi.mock('@/features/auth/components/PublicOnlyRoute', () => ({
  default: ({ children }: { children: React.ReactNode }) => <div data-testid="public-only-route">{children}</div>,
}))

vi.mock('@/features/admin/components/AdminRoute', () => ({
  default: ({ children }: { children: React.ReactNode }) => <div data-testid="admin-route">{children}</div>,
}))

vi.mock('@/features/admin/components/AdminLayout', () => ({
  default: () => <div data-testid="admin-layout"><Outlet /></div>,
}))

vi.mock('@/features/public/pages/HomePublicPage', () => ({
  default: () => <div data-testid="home-public">Home Public</div>,
}))

vi.mock('@/features/dashboard/pages/HomeDashboardPage', () => ({
  default: () => <div data-testid="home-dashboard">Home Dashboard</div>,
}))

vi.mock('@/features/auth/pages/LoginPage', () => ({
  default: () => <div data-testid="login">Login</div>,
}))

vi.mock('@/features/auth/pages/RegisterPage', () => ({
  default: () => <div data-testid="register">Register</div>,
}))

vi.mock('@/features/dashboard/pages/SettingsPage', () => ({
  default: () => <div data-testid="settings">Settings</div>,
}))

vi.mock('@/features/auth/pages/ChangePasswordPage', () => ({
  default: () => <div data-testid="change-password">Change Password</div>,
}))

vi.mock('@/features/auth/pages/ResetPasswordPage', () => ({
  default: () => <div data-testid="reset-password">Reset Password</div>,
}))

vi.mock('@/features/auth/pages/RequestPasswordResetPage', () => ({
  default: () => <div data-testid="request-password-reset">Request Password Reset</div>,
}))

vi.mock('@/features/public/pages/NotFoundPage', () => ({
  default: () => <div data-testid="not-found">Not Found</div>,
}))

vi.mock('@/features/public/pages/VerifyErrorPage', () => ({
  default: () => <div data-testid="verify-error">Verify Error</div>,
}))

vi.mock('@/features/dashboard/pages/SettingsPage', () => ({
  default: () => <div data-testid="settings">Settings</div>,
}))

vi.mock('@/features/families/pages/FamiliesPage', () => ({
  default: () => <div data-testid="families">Families</div>,
}))

vi.mock('@/features/families/pages/AcceptInvitePage', () => ({
  default: () => <div data-testid="accept-invite">Accept Invite</div>,
}))

vi.mock('@/features/expenses/pages/ExpensesPage', () => ({
  default: () => <div data-testid="expenses">Expenses</div>,
}))

vi.mock('@/features/expenses/pages/CsvImportPage', () => ({
  default: () => <div data-testid="csv-import">CSV Import</div>,
}))

vi.mock('@/features/admin/pages/AdminUsersPage', () => ({
  default: () => <div data-testid="admin-users">Admin Users</div>,
}))

vi.mock('@/features/admin/pages/AdminCategoriesPage', () => ({
  default: () => <div data-testid="admin-categories">Admin Categories</div>,
}))

vi.mock('@/features/admin/pages/AdminCurrenciesPage', () => ({
  default: () => <div data-testid="admin-currencies">Admin Currencies</div>,
}))

vi.mock('@/features/admin/pages/AdminRatesPage', () => ({
  default: () => <div data-testid="admin-rates">Admin Rates</div>,
}))

vi.mock('@/features/admin/pages/AdminRateConflictsPage', () => ({
  default: () => <div data-testid="admin-rate-conflicts">Admin Rate Conflicts</div>,
}))

const renderAtPath = (path: string) => {
  const router = createMemoryRouter(routes, { initialEntries: [path] })
  return render(<RouterProvider router={router} />)
}

describe('router', () => {
  describe('Public routes', () => {
    it('renders home public page at /', () => {
      renderAtPath('/')
      expect(screen.getByTestId('public-only-route')).toBeInTheDocument()
      expect(screen.getByTestId('home-public')).toBeInTheDocument()
    })

    it('renders login page at /login', () => {
      renderAtPath('/login')
      expect(screen.getByTestId('public-only-route')).toBeInTheDocument()
      expect(screen.getByTestId('login')).toBeInTheDocument()
    })

    it('renders register page at /register', () => {
      renderAtPath('/register')
      expect(screen.getByTestId('public-only-route')).toBeInTheDocument()
      expect(screen.getByTestId('register')).toBeInTheDocument()
    })

    it('renders reset password page at /reset-password', () => {
      renderAtPath('/reset-password')
      expect(screen.getByTestId('public-only-route')).toBeInTheDocument()
      expect(screen.getByTestId('reset-password')).toBeInTheDocument()
    })

    it('renders request password reset page at /request-password-reset', () => {
      renderAtPath('/request-password-reset')
      expect(screen.getByTestId('public-only-route')).toBeInTheDocument()
      expect(screen.getByTestId('request-password-reset')).toBeInTheDocument()
    })

    it('wraps all public pages with PublicOnlyRoute', () => {
      const publicPaths = ['/', '/login', '/register', '/reset-password', '/request-password-reset']
      publicPaths.forEach(path => {
        const { container, unmount } = renderAtPath(path)
        expect(container.querySelector('[data-testid="public-only-route"]')).toBeInTheDocument()
        unmount()
      })
    })
  })

  describe('Protected routes', () => {
    it('renders dashboard page at /dashboard', () => {
      renderAtPath('/dashboard')
      expect(screen.getByTestId('protected-route')).toBeInTheDocument()
      expect(screen.getByTestId('home-dashboard')).toBeInTheDocument()
    })

    it('renders settings page at /settings', () => {
      renderAtPath('/settings')
      expect(screen.getByTestId('protected-route')).toBeInTheDocument()
      expect(screen.getByTestId('settings')).toBeInTheDocument()
    })

    it('renders change password page at /change-password', () => {
      renderAtPath('/change-password')
      expect(screen.getByTestId('protected-route')).toBeInTheDocument()
      expect(screen.getByTestId('change-password')).toBeInTheDocument()
    })

    it('renders families page at /families', () => {
      renderAtPath('/families')
      expect(screen.getByTestId('protected-route')).toBeInTheDocument()
      expect(screen.getByTestId('families')).toBeInTheDocument()
    })

    it('renders accept invite page at /families/accept-invite', () => {
      renderAtPath('/families/accept-invite')
      expect(screen.getByTestId('protected-route')).toBeInTheDocument()
      expect(screen.getByTestId('accept-invite')).toBeInTheDocument()
    })

    it('renders expenses page at /expenses', () => {
      renderAtPath('/expenses')
      expect(screen.getByTestId('protected-route')).toBeInTheDocument()
      expect(screen.getByTestId('expenses')).toBeInTheDocument()
    })

    it('renders expenses page at /expenses/add', () => {
      renderAtPath('/expenses/add')
      expect(screen.getByTestId('protected-route')).toBeInTheDocument()
      expect(screen.getByTestId('expenses')).toBeInTheDocument()
    })

    it('renders expenses page at /expenses/:id/edit', () => {
      renderAtPath('/expenses/42/edit')
      expect(screen.getByTestId('protected-route')).toBeInTheDocument()
      expect(screen.getByTestId('expenses')).toBeInTheDocument()
    })

    it('renders CSV import page at /expenses/import', () => {
      renderAtPath('/expenses/import')
      expect(screen.getByTestId('protected-route')).toBeInTheDocument()
      expect(screen.getByTestId('csv-import')).toBeInTheDocument()
    })

    it('wraps all private pages with ProtectedRoute', () => {
      const privatePaths = ['/dashboard', '/settings', '/change-password', '/families', '/expenses']
      privatePaths.forEach(path => {
        const { container, unmount } = renderAtPath(path)
        expect(container.querySelector('[data-testid="protected-route"]')).toBeInTheDocument()
        unmount()
      })
    })
  })

  describe('Admin routes', () => {
    it('renders admin users at /admin/users', () => {
      renderAtPath('/admin/users')
      expect(screen.getByTestId('admin-route')).toBeInTheDocument()
      expect(screen.getByTestId('admin-users')).toBeInTheDocument()
    })

    it('renders admin categories at /admin/categories', () => {
      renderAtPath('/admin/categories')
      expect(screen.getByTestId('admin-categories')).toBeInTheDocument()
    })

    it('renders admin currencies at /admin/currencies', () => {
      renderAtPath('/admin/currencies')
      expect(screen.getByTestId('admin-currencies')).toBeInTheDocument()
    })

    it('renders admin rates at /admin/rates', () => {
      renderAtPath('/admin/rates')
      expect(screen.getByTestId('admin-rates')).toBeInTheDocument()
    })

    it('renders admin rate conflicts at /admin/rate-conflicts', () => {
      renderAtPath('/admin/rate-conflicts')
      expect(screen.getByTestId('admin-rate-conflicts')).toBeInTheDocument()
    })
  })

  describe('Standalone public pages', () => {
    it('renders verify error page at /verify-error', () => {
      renderAtPath('/verify-error')
      expect(screen.getByTestId('verify-error')).toBeInTheDocument()
    })
  })

  describe('Fallback routes', () => {
    it('renders not found for unknown route', () => {
      renderAtPath('/unknown-route')
      expect(screen.getByTestId('not-found')).toBeInTheDocument()
    })

    it('renders not found for deeply nested unknown route', () => {
      renderAtPath('/some/unknown/deep/path')
      expect(screen.getByTestId('not-found')).toBeInTheDocument()
    })

    it('prioritizes exact routes over wildcard', () => {
      renderAtPath('/dashboard')
      expect(screen.getByTestId('home-dashboard')).toBeInTheDocument()
      expect(screen.queryByTestId('not-found')).not.toBeInTheDocument()
    })

    it('handles /verify-error before fallback', () => {
      renderAtPath('/verify-error')
      expect(screen.getByTestId('verify-error')).toBeInTheDocument()
      expect(screen.queryByTestId('not-found')).not.toBeInTheDocument()
    })
  })
})
