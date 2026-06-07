import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import AdminRoute from '../AdminRoute'

const mockUseAuth = vi.fn()
vi.mock('@/features/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth(),
}))

function renderRoute(path = '/admin') {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="/login" element={<div>Login</div>} />
        <Route path="/dashboard" element={<div>Dashboard</div>} />
        <Route path="/admin" element={<AdminRoute><div>Admin</div></AdminRoute>} />
      </Routes>
    </MemoryRouter>
  )
}

describe('AdminRoute', () => {
  beforeEach(() => vi.clearAllMocks())

  it('renders nothing while loading', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: false, isLoading: true, user: null })
    const { container } = renderRoute()
    expect(container).toBeEmptyDOMElement()
  })

  it('redirects to /login when unauthenticated', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: false, isLoading: false, user: null })
    renderRoute()
    expect(screen.getByText('Login')).toBeInTheDocument()
    expect(screen.queryByText('Admin')).not.toBeInTheDocument()
  })

  it('redirects to /dashboard when authenticated but not admin', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true, isLoading: false, user: { isAdmin: false } })
    renderRoute()
    expect(screen.getByText('Dashboard')).toBeInTheDocument()
    expect(screen.queryByText('Admin')).not.toBeInTheDocument()
  })

  it('renders children when authenticated and admin', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true, isLoading: false, user: { isAdmin: true } })
    renderRoute()
    expect(screen.getByText('Admin')).toBeInTheDocument()
  })
})
