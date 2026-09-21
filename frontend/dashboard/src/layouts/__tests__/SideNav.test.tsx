import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, act } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import SideNav from '@/layouts/SideNav'

const mockUseAuth = vi.fn()
const mockNavigate = vi.fn()

vi.mock('@/features/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth(),
}))

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom')
  return { ...actual, useNavigate: () => mockNavigate }
})

function renderSideNav(path = '/dashboard') {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="*" element={<SideNav isMobileOpen={false} onMobileClose={vi.fn()} />} />
      </Routes>
    </MemoryRouter>
  )
}

describe('SideNav', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      logout: vi.fn(),
      user: { firstName: 'John', lastName: 'Doe', email: 'john@test.com' },
    })
  })

  it('shows primary nav links', () => {
    renderSideNav()
    expect(screen.getByRole('link', { name: /dashboard/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /expenses/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /families/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /^settings$/i })).toBeInTheDocument()
  })

  it('does not show Admin link for non-admin user', () => {
    renderSideNav()
    expect(screen.queryByRole('link', { name: /^admin$/i })).not.toBeInTheDocument()
  })

  it('shows Admin link for admin user', () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      logout: vi.fn(),
      user: { firstName: 'John', lastName: 'Doe', email: 'john@test.com', isAdmin: true },
    })
    renderSideNav()
    expect(screen.getByRole('link', { name: /^admin$/i })).toBeInTheDocument()
  })

  it('applies active class to Dashboard when on /dashboard', () => {
    renderSideNav('/dashboard')
    expect(screen.getByRole('link', { name: /dashboard/i })).toHaveClass('bg-brand-100', 'text-brand-600')
  })

  it('shows user name and email in the user card', () => {
    renderSideNav()
    expect(screen.getByText('John Doe')).toBeInTheDocument()
    expect(screen.getByText('john@test.com')).toBeInTheDocument()
  })

  it('shows initials when user has no name', () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      logout: vi.fn(),
      user: { firstName: '', lastName: '', email: 'x@test.com' },
    })
    renderSideNav()
    expect(screen.getByRole('button', { name: /user menu/i })).toHaveTextContent('?')
  })

  it('opens user menu popover and signs out', async () => {
    const logout = vi.fn()
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      logout,
      user: { firstName: 'John', lastName: 'Doe', email: 'john@test.com' },
    })
    const user = userEvent.setup()
    renderSideNav()

    await user.click(screen.getByRole('button', { name: /user menu/i }))
    const signOut = await screen.findByRole('button', { name: /sign out/i })
    await user.click(signOut)

    expect(logout).toHaveBeenCalledOnce()
  })

  it('navigates to / after logout completes', async () => {
    const logout = vi.fn()
    mockUseAuth.mockReturnValue({ isAuthenticated: true, logout, user: { firstName: 'John', lastName: 'Doe', email: 'a@b.com' } })
    const user = userEvent.setup()
    const { rerender } = renderSideNav()

    await user.click(screen.getByRole('button', { name: /user menu/i }))
    const signOut = await screen.findByRole('button', { name: /sign out/i })
    await user.click(signOut)

    mockUseAuth.mockReturnValue({ isAuthenticated: false, logout, user: { firstName: 'John', lastName: 'Doe', email: 'a@b.com' } })
    await act(async () => {
      rerender(
        <MemoryRouter initialEntries={['/dashboard']}>
          <Routes>
            <Route path="*" element={<SideNav isMobileOpen={false} onMobileClose={vi.fn()} />} />
          </Routes>
        </MemoryRouter>
      )
    })

    expect(mockNavigate).toHaveBeenCalledWith('/')
  })

  describe('mobile drawer', () => {
    it('renders drawer nav when isMobileOpen is true', () => {
      render(
        <MemoryRouter initialEntries={['/dashboard']}>
          <SideNav isMobileOpen onMobileClose={vi.fn()} />
        </MemoryRouter>
      )
      expect(screen.getAllByRole('link', { name: /dashboard/i }).length).toBeGreaterThan(0)
    })

    it('calls onMobileClose when backdrop is clicked', async () => {
      const onMobileClose = vi.fn()
      const user = userEvent.setup()
      render(
        <MemoryRouter initialEntries={['/dashboard']}>
          <SideNav isMobileOpen onMobileClose={onMobileClose} />
        </MemoryRouter>
      )
      await user.click(screen.getByRole('button', { name: /toggle menu/i }))
      expect(onMobileClose).toHaveBeenCalledOnce()
    })

    it('calls onMobileClose on Escape', async () => {
      const onMobileClose = vi.fn()
      const user = userEvent.setup()
      render(
        <MemoryRouter initialEntries={['/dashboard']}>
          <SideNav isMobileOpen onMobileClose={onMobileClose} />
        </MemoryRouter>
      )
      await user.keyboard('{Escape}')
      expect(onMobileClose).toHaveBeenCalledOnce()
    })
  })
})
