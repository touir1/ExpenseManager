import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import AppRoutes from '../router'

// Mock all page components
vi.mock('@/features/auth/components/ProtectedRoute', () => ({
  default: ({ children }: { children: React.ReactNode }) => <div data-testid="protected-route">{children}</div>,
}))

vi.mock('@/features/auth/components/PublicOnlyRoute', () => ({
  default: ({ children }: { children: React.ReactNode }) => <div data-testid="public-only-route">{children}</div>,
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

describe('AppRoutes', () => {
  const renderAtPath = (path: string) => {
    return render(
      <MemoryRouter initialEntries={[path]}>
        <AppRoutes />
      </MemoryRouter>
    )
  }

  describe('Public routes', () => {
    it('should render home public page at /', () => {
      renderAtPath('/')
      expect(screen.getByTestId('public-only-route')).toBeInTheDocument()
      expect(screen.getByTestId('home-public')).toBeInTheDocument()
    })

    it('should render login page at /login', () => {
      renderAtPath('/login')
      expect(screen.getByTestId('public-only-route')).toBeInTheDocument()
      expect(screen.getByTestId('login')).toBeInTheDocument()
    })

    it('should render register page at /register', () => {
      renderAtPath('/register')
      expect(screen.getByTestId('public-only-route')).toBeInTheDocument()
      expect(screen.getByTestId('register')).toBeInTheDocument()
    })

    it('should render reset password page at /reset-password', () => {
      renderAtPath('/reset-password')
      expect(screen.getByTestId('public-only-route')).toBeInTheDocument()
      expect(screen.getByTestId('reset-password')).toBeInTheDocument()
    })

    it('should render request password reset page at /request-password-reset', () => {
      renderAtPath('/request-password-reset')
      expect(screen.getByTestId('public-only-route')).toBeInTheDocument()
      expect(screen.getByTestId('request-password-reset')).toBeInTheDocument()
    })
  })

  describe('Protected routes', () => {
    it('should render dashboard page at /dashboard with protected route', () => {
      renderAtPath('/dashboard')
      expect(screen.getByTestId('protected-route')).toBeInTheDocument()
      expect(screen.getByTestId('home-dashboard')).toBeInTheDocument()
    })

    it('should render settings page at /settings with protected route', () => {
      renderAtPath('/settings')
      expect(screen.getByTestId('protected-route')).toBeInTheDocument()
      expect(screen.getByTestId('settings')).toBeInTheDocument()
    })

    it('should render change password page at /change-password with protected route', () => {
      renderAtPath('/change-password')
      expect(screen.getByTestId('protected-route')).toBeInTheDocument()
      expect(screen.getByTestId('change-password')).toBeInTheDocument()
    })
  })

  describe('Standalone public pages', () => {
    it('should render verify error page at /verify-error', () => {
      renderAtPath('/verify-error')
      expect(screen.getByTestId('verify-error')).toBeInTheDocument()
    })
  })

  describe('Fallback routes', () => {
    it('should render not found page for unknown routes', () => {
      renderAtPath('/unknown-route')
      expect(screen.getByTestId('not-found')).toBeInTheDocument()
    })

    it('should render not found page for deeply nested unknown routes', () => {
      renderAtPath('/some/unknown/deep/path')
      expect(screen.getByTestId('not-found')).toBeInTheDocument()
    })
  })

  describe('Route hierarchy', () => {
    it('should prioritize exact routes over wildcard', () => {
      renderAtPath('/dashboard')
      expect(screen.getByTestId('home-dashboard')).toBeInTheDocument()
      expect(screen.queryByTestId('not-found')).not.toBeInTheDocument()
    })

    it('should handle verify-error before fallback', () => {
      renderAtPath('/verify-error')
      expect(screen.getByTestId('verify-error')).toBeInTheDocument()
      expect(screen.queryByTestId('not-found')).not.toBeInTheDocument()
    })
  })

  describe('PublicOnlyRoute wrapper', () => {
    it('should wrap public routes with PublicOnlyRoute component', () => {
      renderAtPath('/')
      const publicRoute = screen.getByTestId('public-only-route')
      expect(publicRoute).toBeInTheDocument()
    })

    it('should have PublicOnlyRoute around all public pages', () => {
      const publicPaths = ['/', '/login', '/register', '/reset-password', '/request-password-reset']
      publicPaths.forEach(path => {
        const { container } = render(
          <MemoryRouter initialEntries={[path]}>
            <AppRoutes />
          </MemoryRouter>
        )
        expect(container.querySelector('[data-testid="public-only-route"]')).toBeInTheDocument()
      })
    })
  })

  describe('ProtectedRoute wrapper', () => {
    it('should wrap private routes with ProtectedRoute component', () => {
      renderAtPath('/dashboard')
      const protectedRoute = screen.getByTestId('protected-route')
      expect(protectedRoute).toBeInTheDocument()
    })

    it('should have ProtectedRoute around all private pages', () => {
      const privatePaths = ['/dashboard', '/settings', '/change-password']
      privatePaths.forEach(path => {
        const { container } = render(
          <MemoryRouter initialEntries={[path]}>
            <AppRoutes />
          </MemoryRouter>
        )
        expect(container.querySelector('[data-testid="protected-route"]')).toBeInTheDocument()
      })
    })
  })
})
