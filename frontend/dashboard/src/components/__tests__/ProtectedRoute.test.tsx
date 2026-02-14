import { describe, it, expect, vi, beforeEach } from 'vitest'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import { render, screen } from '@testing-library/react'
import ProtectedRoute from '@/components/ProtectedRoute'

const mockUseAuth = vi.fn()

vi.mock('@/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth()
}))

describe('ProtectedRoute', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('redirects to /login when unauthenticated', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: false })

    render(
      <MemoryRouter initialEntries={["/home-auth"]}>
        <Routes>
          <Route path="/login" element={<div>Login Page</div>} />
          <Route path="/home-auth" element={<ProtectedRoute><div>Private</div></ProtectedRoute>} />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByText('Login Page')).toBeInTheDocument()
    expect(screen.queryByText('Private')).not.toBeInTheDocument()
  })

  it('renders children when authenticated', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true })

    render(
      <MemoryRouter initialEntries={["/home-auth"]}>
        <Routes>
          <Route path="/home-auth" element={<ProtectedRoute><div>Private</div></ProtectedRoute>} />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByText('Private')).toBeInTheDocument()
  })

  it('does not render private content when unauthenticated', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: false })

    render(
      <MemoryRouter initialEntries={["/dashboard"]}>
        <Routes>
          <Route path="/login" element={<div>Login Page</div>} />
          <Route 
            path="/dashboard" 
            element={
              <ProtectedRoute>
                <div>
                  <h1>Dashboard</h1>
                  <p>Sensitive Data</p>
                </div>
              </ProtectedRoute>
            } 
          />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.queryByText('Dashboard')).not.toBeInTheDocument()
    expect(screen.queryByText('Sensitive Data')).not.toBeInTheDocument()
    expect(screen.getByText('Login Page')).toBeInTheDocument()
  })

  it('renders complex nested children when authenticated', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true })

    render(
      <MemoryRouter initialEntries={["/profile"]}>
        <Routes>
          <Route 
            path="/profile" 
            element={
              <ProtectedRoute>
                <div>
                  <header>Profile Header</header>
                  <main>Profile Content</main>
                  <footer>Profile Footer</footer>
                </div>
              </ProtectedRoute>
            } 
          />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByText('Profile Header')).toBeInTheDocument()
    expect(screen.getByText('Profile Content')).toBeInTheDocument()
    expect(screen.getByText('Profile Footer')).toBeInTheDocument()
  })

  it('works with multiple protected routes', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true })

    render(
      <MemoryRouter initialEntries={["/settings"]}>
        <Routes>
          <Route path="/login" element={<div>Login Page</div>} />
          <Route path="/dashboard" element={<ProtectedRoute><div>Dashboard</div></ProtectedRoute>} />
          <Route path="/profile" element={<ProtectedRoute><div>Profile</div></ProtectedRoute>} />
          <Route path="/settings" element={<ProtectedRoute><div>Settings</div></ProtectedRoute>} />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByText('Settings')).toBeInTheDocument()
  })

  it('redirects from different protected routes when unauthenticated', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: false })

    render(
      <MemoryRouter initialEntries={["/profile"]}>
        <Routes>
          <Route path="/login" element={<div>Login Page</div>} />
          <Route path="/profile" element={<ProtectedRoute><div>Profile</div></ProtectedRoute>} />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByText('Login Page')).toBeInTheDocument()
    expect(screen.queryByText('Profile')).not.toBeInTheDocument()
  })

  it('calls useAuth hook', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true })

    render(
      <MemoryRouter initialEntries={["/protected"]}>
        <Routes>
          <Route path="/protected" element={<ProtectedRoute><div>Protected</div></ProtectedRoute>} />
        </Routes>
      </MemoryRouter>
    )

    expect(mockUseAuth).toHaveBeenCalled()
  })

  it('handles authentication false explicitly', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: false })

    render(
      <MemoryRouter initialEntries={["/secret"]}>
        <Routes>
          <Route path="/login" element={<div>Login Required</div>} />
          <Route path="/secret" element={<ProtectedRoute><div>Secret Content</div></ProtectedRoute>} />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByText('Login Required')).toBeInTheDocument()
    expect(screen.queryByText('Secret Content')).not.toBeInTheDocument()
  })

  it('renders children immediately when authenticated without redirect', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true })

    render(
      <MemoryRouter initialEntries={["/admin"]}>
        <Routes>
          <Route path="/login" element={<div>Login Page</div>} />
          <Route path="/admin" element={<ProtectedRoute><div data-testid="admin-panel">Admin Panel</div></ProtectedRoute>} />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByTestId('admin-panel')).toBeInTheDocument()
    expect(screen.queryByText('Login Page')).not.toBeInTheDocument()
  })

  it('protects route with JSX Element children', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true })

    const ProtectedContent = () => <div>Protected Component</div>

    render(
      <MemoryRouter initialEntries={["/component"]}>
        <Routes>
          <Route path="/component" element={<ProtectedRoute><ProtectedContent /></ProtectedRoute>} />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByText('Protected Component')).toBeInTheDocument()
  })
})
