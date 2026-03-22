import { describe, it, expect, vi, beforeEach } from 'vitest'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import { render, screen } from '@testing-library/react'
import PublicOnlyRoute from '@/components/PublicOnlyRoute'

const mockUseAuth = vi.fn()

vi.mock('@/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth()
}))

describe('PublicOnlyRoute', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders nothing when isLoading is true', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: false, isLoading: true })

    const { container } = render(
      <MemoryRouter initialEntries={['/login']}>
        <Routes>
          <Route path="/home" element={<div>Home Dashboard</div>} />
          <Route path="/login" element={<PublicOnlyRoute><div>Login Page</div></PublicOnlyRoute>} />
        </Routes>
      </MemoryRouter>
    )

    expect(container).toBeEmptyDOMElement()
    expect(screen.queryByText('Login Page')).not.toBeInTheDocument()
    expect(screen.queryByText('Home Dashboard')).not.toBeInTheDocument()
  })

  it('renders children when unauthenticated', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: false, isLoading: false })

    render(
      <MemoryRouter initialEntries={['/login']}>
        <Routes>
          <Route path="/login" element={<PublicOnlyRoute><div>Login Page</div></PublicOnlyRoute>} />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByText('Login Page')).toBeInTheDocument()
  })

  it('redirects to /home when authenticated', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true, isLoading: false })

    render(
      <MemoryRouter initialEntries={['/login']}>
        <Routes>
          <Route path="/home" element={<div>Home Dashboard</div>} />
          <Route path="/login" element={<PublicOnlyRoute><div>Login Page</div></PublicOnlyRoute>} />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByText('Home Dashboard')).toBeInTheDocument()
    expect(screen.queryByText('Login Page')).not.toBeInTheDocument()
  })

  it('redirects from /register to /home when authenticated', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true, isLoading: false })

    render(
      <MemoryRouter initialEntries={['/register']}>
        <Routes>
          <Route path="/home" element={<div>Home Dashboard</div>} />
          <Route path="/register" element={<PublicOnlyRoute><div>Register Page</div></PublicOnlyRoute>} />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByText('Home Dashboard')).toBeInTheDocument()
    expect(screen.queryByText('Register Page')).not.toBeInTheDocument()
  })

  it('redirects from / to /home when authenticated', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true, isLoading: false })

    render(
      <MemoryRouter initialEntries={['/']}>
        <Routes>
          <Route path="/home" element={<div>Home Dashboard</div>} />
          <Route path="/" element={<PublicOnlyRoute><div>Public Home</div></PublicOnlyRoute>} />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByText('Home Dashboard')).toBeInTheDocument()
    expect(screen.queryByText('Public Home')).not.toBeInTheDocument()
  })
})
