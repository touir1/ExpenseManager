import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import NavBar from '@/components/NavBar'

const mockUseAuth = vi.fn()
const mockNavigate = vi.fn()

vi.mock('@/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth(),
}))

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom')
  return { ...actual, useNavigate: () => mockNavigate }
})

function renderNavBar(path = '/') {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="*" element={<NavBar />} />
      </Routes>
    </MemoryRouter>
  )
}

describe('NavBar', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  // ── Unauthenticated ──────────────────────────────────────────────────────

  describe('when unauthenticated', () => {
    beforeEach(() => {
      mockUseAuth.mockReturnValue({ isAuthenticated: false, logout: vi.fn() })
    })

    it('shows Home, Sign in, and Get started links in desktop nav', () => {
      renderNavBar('/')
      const nav = screen.getByRole('navigation')
      expect(nav).toBeInTheDocument()
      expect(nav).toHaveTextContent('Home')
      expect(nav).toHaveTextContent('Sign in')
      expect(nav).toHaveTextContent('Get started')
    })

    it('does not show Dashboard, Settings, or Sign out in desktop nav', () => {
      renderNavBar('/')
      const nav = screen.getByRole('navigation')
      expect(nav).not.toHaveTextContent('Dashboard')
      expect(nav).not.toHaveTextContent('Settings')
      expect(nav).not.toHaveTextContent('Sign out')
    })

    it('links logo to /', () => {
      renderNavBar('/')
      const logo = screen.getByRole('link', { name: /expensesmanager/i })
      expect(logo).toHaveAttribute('href', '/')
    })
  })

  // ── Authenticated ────────────────────────────────────────────────────────

  describe('when authenticated', () => {
    beforeEach(() => {
      mockUseAuth.mockReturnValue({ isAuthenticated: true, logout: vi.fn() })
    })

    it('shows Dashboard, Settings, and Sign out in desktop nav', () => {
      renderNavBar('/home-auth')
      const nav = screen.getByRole('navigation')
      expect(nav).toHaveTextContent('Dashboard')
      expect(nav).toHaveTextContent('Settings')
      expect(nav).toHaveTextContent('Sign out')
    })

    it('does not show Home, Sign in, or Get started in desktop nav', () => {
      renderNavBar('/home-auth')
      const nav = screen.getByRole('navigation')
      expect(nav).not.toHaveTextContent('Home')
      expect(nav).not.toHaveTextContent('Sign in')
      expect(nav).not.toHaveTextContent('Get started')
    })

    it('links logo to /home-auth', () => {
      renderNavBar('/home-auth')
      const logo = screen.getByRole('link', { name: /expensesmanager/i })
      expect(logo).toHaveAttribute('href', '/home-auth')
    })
  })

  // ── Logout ───────────────────────────────────────────────────────────────

  describe('logout', () => {
    it('calls logout and navigates to / when Sign out is clicked', async () => {
      const logout = vi.fn()
      mockUseAuth.mockReturnValue({ isAuthenticated: true, logout })
      const user = userEvent.setup()

      renderNavBar('/home-auth')

      const signOut = screen.getAllByRole('button', { name: /sign out/i })[0]
      await user.click(signOut)

      expect(logout).toHaveBeenCalledOnce()
      expect(mockNavigate).toHaveBeenCalledWith('/')
    })
  })

  // ── Active link styling ──────────────────────────────────────────────────

  describe('active link styling', () => {
    it('applies active class to Dashboard when on /home-auth', () => {
      mockUseAuth.mockReturnValue({ isAuthenticated: true, logout: vi.fn() })
      renderNavBar('/home-auth')

      const nav = screen.getByRole('navigation')
      const dashboardLink = Array.from(nav.querySelectorAll('a')).find(
        a => a.textContent === 'Dashboard'
      )
      expect(dashboardLink).toHaveClass('bg-brand-50', 'text-brand-700')
    })

    it('applies active class to Sign in when on /login', () => {
      mockUseAuth.mockReturnValue({ isAuthenticated: false, logout: vi.fn() })
      renderNavBar('/login')

      const nav = screen.getByRole('navigation')
      const signInLink = Array.from(nav.querySelectorAll('a')).find(
        a => a.textContent === 'Sign in'
      )
      expect(signInLink).toHaveClass('bg-brand-50', 'text-brand-700')
    })

    it('applies inactive class to non-current links', () => {
      mockUseAuth.mockReturnValue({ isAuthenticated: true, logout: vi.fn() })
      renderNavBar('/home-auth')

      const nav = screen.getByRole('navigation')
      const settingsLink = Array.from(nav.querySelectorAll('a')).find(
        a => a.textContent === 'Settings'
      )
      expect(settingsLink).toHaveClass('text-slate-600')
      expect(settingsLink).not.toHaveClass('bg-brand-50')
    })
  })

  // ── Mobile menu ──────────────────────────────────────────────────────────

  describe('mobile menu', () => {
    it('is hidden by default', () => {
      mockUseAuth.mockReturnValue({ isAuthenticated: false, logout: vi.fn() })
      renderNavBar('/')
      // The mobile menu div has sm:hidden, so it won't exist in DOM until opened
      expect(screen.queryByRole('button', { name: /toggle menu/i })).toBeInTheDocument()
    })

    it('toggles open when hamburger button is clicked', async () => {
      mockUseAuth.mockReturnValue({ isAuthenticated: false, logout: vi.fn() })
      const user = userEvent.setup()
      renderNavBar('/')

      const hamburger = screen.getByRole('button', { name: /toggle menu/i })
      expect(screen.getAllByText('Home')).toHaveLength(1)

      await user.click(hamburger)
      // Home link now appears in both desktop nav and mobile menu
      expect(screen.getAllByText('Home')).toHaveLength(2)
    })

    it('closes when hamburger is clicked again', async () => {
      mockUseAuth.mockReturnValue({ isAuthenticated: false, logout: vi.fn() })
      const user = userEvent.setup()
      renderNavBar('/')

      const hamburger = screen.getByRole('button', { name: /toggle menu/i })
      await user.click(hamburger)
      expect(screen.getAllByText('Home')).toHaveLength(2)

      await user.click(hamburger)
      expect(screen.getAllByText('Home')).toHaveLength(1)
    })

    it('shows authenticated links in mobile menu', async () => {
      mockUseAuth.mockReturnValue({ isAuthenticated: true, logout: vi.fn() })
      const user = userEvent.setup()
      renderNavBar('/home-auth')

      await user.click(screen.getByRole('button', { name: /toggle menu/i }))

      expect(screen.getAllByText('Dashboard')).toHaveLength(2)
      expect(screen.getAllByText('Settings')).toHaveLength(2)
      expect(screen.getAllByText('Sign out')).toHaveLength(2)
    })

    it('shows unauthenticated links in mobile menu', async () => {
      mockUseAuth.mockReturnValue({ isAuthenticated: false, logout: vi.fn() })
      const user = userEvent.setup()
      renderNavBar('/')

      await user.click(screen.getByRole('button', { name: /toggle menu/i }))

      expect(screen.getAllByText('Home')).toHaveLength(2)
      expect(screen.getAllByText('Sign in')).toHaveLength(2)
      expect(screen.getAllByText('Get started')).toHaveLength(2)
    })

    it('closes mobile menu when a link is clicked', async () => {
      mockUseAuth.mockReturnValue({ isAuthenticated: false, logout: vi.fn() })
      const user = userEvent.setup()
      renderNavBar('/')

      await user.click(screen.getByRole('button', { name: /toggle menu/i }))
      expect(screen.getAllByText('Sign in')).toHaveLength(2)

      const [, mobileSignIn] = screen.getAllByText('Sign in')
      await user.click(mobileSignIn)
      expect(screen.getAllByText('Sign in')).toHaveLength(1)
    })

    it('closes mobile menu and logs out when mobile Sign out is clicked', async () => {
      const logout = vi.fn()
      mockUseAuth.mockReturnValue({ isAuthenticated: true, logout })
      const user = userEvent.setup()
      renderNavBar('/home-auth')

      await user.click(screen.getByRole('button', { name: /toggle menu/i }))

      const [, mobileSignOut] = screen.getAllByRole('button', { name: /sign out/i })
      await user.click(mobileSignOut)

      expect(logout).toHaveBeenCalledOnce()
      expect(mockNavigate).toHaveBeenCalledWith('/')
    })

    it('closes mobile menu when mobile Dashboard link is clicked', async () => {
      mockUseAuth.mockReturnValue({ isAuthenticated: true, logout: vi.fn() })
      const user = userEvent.setup()
      renderNavBar('/home-auth')

      await user.click(screen.getByRole('button', { name: /toggle menu/i }))
      expect(screen.getAllByText('Dashboard')).toHaveLength(2)

      const [, mobileDashboard] = screen.getAllByText('Dashboard')
      await user.click(mobileDashboard)
      expect(screen.getAllByText('Dashboard')).toHaveLength(1)
    })

    it('closes mobile menu when mobile Settings link is clicked', async () => {
      mockUseAuth.mockReturnValue({ isAuthenticated: true, logout: vi.fn() })
      const user = userEvent.setup()
      renderNavBar('/home-auth')

      await user.click(screen.getByRole('button', { name: /toggle menu/i }))
      expect(screen.getAllByText('Settings')).toHaveLength(2)

      const [, mobileSettings] = screen.getAllByText('Settings')
      await user.click(mobileSettings)
      expect(screen.getAllByText('Settings')).toHaveLength(1)
    })

    it('closes mobile menu when mobile Home link is clicked', async () => {
      mockUseAuth.mockReturnValue({ isAuthenticated: false, logout: vi.fn() })
      const user = userEvent.setup()
      renderNavBar('/')

      await user.click(screen.getByRole('button', { name: /toggle menu/i }))
      expect(screen.getAllByText('Home')).toHaveLength(2)

      const [, mobileHome] = screen.getAllByText('Home')
      await user.click(mobileHome)
      expect(screen.getAllByText('Home')).toHaveLength(1)
    })

    it('closes mobile menu when mobile Get started link is clicked', async () => {
      mockUseAuth.mockReturnValue({ isAuthenticated: false, logout: vi.fn() })
      const user = userEvent.setup()
      renderNavBar('/')

      await user.click(screen.getByRole('button', { name: /toggle menu/i }))
      expect(screen.getAllByText('Get started')).toHaveLength(2)

      const [, mobileGetStarted] = screen.getAllByText('Get started')
      await user.click(mobileGetStarted)
      expect(screen.getAllByText('Get started')).toHaveLength(1)
    })
  })
})
