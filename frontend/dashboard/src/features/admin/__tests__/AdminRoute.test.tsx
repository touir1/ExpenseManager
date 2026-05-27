import { describe, it, expect, vi, beforeEach } from 'vitest'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import { render, screen } from '@testing-library/react'
import AdminRoute from '@/features/admin/components/AdminRoute'

const mockUseAuth = vi.fn()

vi.mock('@/features/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth()
}))

describe('AdminRoute', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders nothing when isLoading is true', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: false, isLoading: true, user: null })
    const { container } = render(
      <MemoryRouter initialEntries={['/admin']}>
        <Routes>
          <Route path="/dashboard" element={<div>Dashboard</div>} />
          <Route path="/admin" element={<AdminRoute><div>Admin</div></AdminRoute>} />
        </Routes>
      </MemoryRouter>
    )
    expect(container).toBeEmptyDOMElement()
  })

  it('redirects to /login when unauthenticated', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: false, isLoading: false, user: null })
    render(
      <MemoryRouter initialEntries={['/admin']}>
        <Routes>
          <Route path="/login" element={<div>Login</div>} />
          <Route path="/admin" element={<AdminRoute><div>Admin</div></AdminRoute>} />
        </Routes>
      </MemoryRouter>
    )
    expect(screen.getByText('Login')).toBeInTheDocument()
    expect(screen.queryByText('Admin')).not.toBeInTheDocument()
  })

  it('redirects to /dashboard when authenticated but not admin', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true, isLoading: false, user: { isAdmin: false } })
    render(
      <MemoryRouter initialEntries={['/admin']}>
        <Routes>
          <Route path="/dashboard" element={<div>Dashboard</div>} />
          <Route path="/admin" element={<AdminRoute><div>Admin</div></AdminRoute>} />
        </Routes>
      </MemoryRouter>
    )
    expect(screen.getByText('Dashboard')).toBeInTheDocument()
    expect(screen.queryByText('Admin')).not.toBeInTheDocument()
  })

  it('renders children when isAdmin is true', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true, isLoading: false, user: { isAdmin: true } })
    render(
      <MemoryRouter initialEntries={['/admin']}>
        <Routes>
          <Route path="/admin" element={<AdminRoute><div>Admin Panel</div></AdminRoute>} />
        </Routes>
      </MemoryRouter>
    )
    expect(screen.getByText('Admin Panel')).toBeInTheDocument()
  })

  it('redirects to /dashboard when user has no isAdmin property', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true, isLoading: false, user: {} })
    render(
      <MemoryRouter initialEntries={['/admin']}>
        <Routes>
          <Route path="/dashboard" element={<div>Dashboard</div>} />
          <Route path="/admin" element={<AdminRoute><div>Admin</div></AdminRoute>} />
        </Routes>
      </MemoryRouter>
    )
    expect(screen.getByText('Dashboard')).toBeInTheDocument()
  })
})
