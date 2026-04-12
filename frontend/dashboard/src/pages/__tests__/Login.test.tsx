import { describe, it, expect, vi, beforeEach } from 'vitest'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import Login from '@/pages/Login'

const mockLogin = vi.fn()
const mockUseAuth = vi.fn()

vi.mock('@/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth()
}))

describe('Login page', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders login form with all fields', () => {
    mockUseAuth.mockReturnValue({ login: mockLogin })

    render(
      <MemoryRouter initialEntries={["/login"]}>
        <Routes>
          <Route path="/login" element={<Login />} />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByRole('heading', { name: /welcome back/i })).toBeInTheDocument()
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /login/i })).toBeInTheDocument()
  })

  it('renders navigation links', () => {
    mockUseAuth.mockReturnValue({ login: mockLogin })

    render(
      <MemoryRouter initialEntries={["/login"]}>
        <Routes>
          <Route path="/login" element={<Login />} />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByRole('link', { name: /register/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /forgot your password/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /have a verification link/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /back to home/i })).toBeInTheDocument()
  })

  it('navigates to /dashboard on successful login', async () => {
    const user = userEvent.setup()
    mockLogin.mockResolvedValue(true)
    mockUseAuth.mockReturnValue({ login: mockLogin })

    render(
      <MemoryRouter initialEntries={["/login"]}>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/dashboard" element={<div>Dashboard</div>} />
        </Routes>
      </MemoryRouter>
    )

    await user.type(screen.getByLabelText(/email/i), 'user@example.com')
    await user.type(screen.getByLabelText(/password/i), 'secret')
    await user.click(screen.getByRole('button', { name: /login/i }))

    await waitFor(() => {
      expect(screen.getByText('Dashboard')).toBeInTheDocument()
    })

    expect(mockLogin).toHaveBeenCalledWith('user@example.com', 'secret')
  })

  it('shows error on invalid credentials', async () => {
    const user = userEvent.setup()
    mockLogin.mockResolvedValue(false)
    mockUseAuth.mockReturnValue({ login: mockLogin })

    render(
      <MemoryRouter initialEntries={["/login"]}>
        <Routes>
          <Route path="/login" element={<Login />} />
        </Routes>
      </MemoryRouter>
    )

    await user.type(screen.getByLabelText(/email/i), 'user@example.com')
    await user.type(screen.getByLabelText(/password/i), 'bad')
    await user.click(screen.getByRole('button', { name: /login/i }))

    await waitFor(() => {
      expect(screen.getByText(/invalid credentials/i)).toBeInTheDocument()
    })

    expect(mockLogin).toHaveBeenCalledWith('user@example.com', 'bad')
  })

  it('clears error when user starts typing again', async () => {
    const user = userEvent.setup()
    mockLogin.mockResolvedValue(false)
    mockUseAuth.mockReturnValue({ login: mockLogin })

    render(
      <MemoryRouter initialEntries={["/login"]}>
        <Routes>
          <Route path="/login" element={<Login />} />
        </Routes>
      </MemoryRouter>
    )

    await user.type(screen.getByLabelText(/email/i), 'user@example.com')
    await user.type(screen.getByLabelText(/password/i), 'wrong')
    await user.click(screen.getByRole('button', { name: /login/i }))

    await waitFor(() => {
      expect(screen.getByText(/invalid credentials/i)).toBeInTheDocument()
    })

    // Error should still be visible
    expect(screen.getByText(/invalid credentials/i)).toBeInTheDocument()
  })

  it('does not navigate on failed login', async () => {
    const user = userEvent.setup()
    mockLogin.mockResolvedValue(false)
    mockUseAuth.mockReturnValue({ login: mockLogin })

    render(
      <MemoryRouter initialEntries={["/login"]}>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/dashboard" element={<div>Dashboard</div>} />
        </Routes>
      </MemoryRouter>
    )

    await user.type(screen.getByLabelText(/email/i), 'wrong@example.com')
    await user.type(screen.getByLabelText(/password/i), 'wrongpass')
    await user.click(screen.getByRole('button', { name: /login/i }))

    await waitFor(() => {
      expect(screen.getByText(/invalid credentials/i)).toBeInTheDocument()
    })

    expect(screen.queryByText('Dashboard')).not.toBeInTheDocument()
  })

  it('calls login with correct credentials', async () => {
    const user = userEvent.setup()
    mockLogin.mockResolvedValue(true)
    mockUseAuth.mockReturnValue({ login: mockLogin })

    render(
      <MemoryRouter initialEntries={["/login"]}>
        <Routes>
          <Route path="/login" element={<Login />} />
        </Routes>
      </MemoryRouter>
    )

    const emailInput = screen.getByLabelText(/email/i)
    const passwordInput = screen.getByLabelText(/password/i)

    await user.type(emailInput, 'test@test.com')
    await user.type(passwordInput, 'password123')
    await user.click(screen.getByRole('button', { name: /login/i }))

    expect(mockLogin).toHaveBeenCalledTimes(1)
    expect(mockLogin).toHaveBeenCalledWith('test@test.com', 'password123')
  })

  it('requires email and password fields', () => {
    mockUseAuth.mockReturnValue({ login: mockLogin })

    render(
      <MemoryRouter initialEntries={["/login"]}>
        <Routes>
          <Route path="/login" element={<Login />} />
        </Routes>
      </MemoryRouter>
    )

    const emailInput = screen.getByLabelText(/email/i)
    const passwordInput = screen.getByLabelText(/password/i)

    expect(emailInput).toBeRequired()
    expect(passwordInput).toBeRequired()
  })

  it('email input has correct type', () => {
    mockUseAuth.mockReturnValue({ login: mockLogin })

    render(
      <MemoryRouter initialEntries={["/login"]}>
        <Routes>
          <Route path="/login" element={<Login />} />
        </Routes>
      </MemoryRouter>
    )

    const emailInput = screen.getByLabelText(/email/i)
    const passwordInput = screen.getByLabelText(/password/i)

    expect(emailInput).toHaveAttribute('type', 'email')
    expect(passwordInput).toHaveAttribute('type', 'password')
  })

  it('does not show error initially', () => {
    mockUseAuth.mockReturnValue({ login: mockLogin })

    render(
      <MemoryRouter initialEntries={["/login"]}>
        <Routes>
          <Route path="/login" element={<Login />} />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.queryByText(/invalid credentials/i)).not.toBeInTheDocument()
  })

  it('updates email input value', async () => {
    const user = userEvent.setup()
    mockUseAuth.mockReturnValue({ login: mockLogin })

    render(
      <MemoryRouter initialEntries={["/login"]}>
        <Routes>
          <Route path="/login" element={<Login />} />
        </Routes>
      </MemoryRouter>
    )

    const emailInput = screen.getByLabelText(/email/i) as HTMLInputElement

    await user.type(emailInput, 'new@email.com')
    
    expect(emailInput.value).toBe('new@email.com')
  })

  it('updates password input value', async () => {
    const user = userEvent.setup()
    mockUseAuth.mockReturnValue({ login: mockLogin })

    render(
      <MemoryRouter initialEntries={["/login"]}>
        <Routes>
          <Route path="/login" element={<Login />} />
        </Routes>
      </MemoryRouter>
    )

    const passwordInput = screen.getByLabelText(/password/i) as HTMLInputElement

    await user.type(passwordInput, 'mypassword')
    
    expect(passwordInput.value).toBe('mypassword')
  })

  it('handles form submission with enter key', async () => {
    const user = userEvent.setup()
    mockLogin.mockResolvedValue(true)
    mockUseAuth.mockReturnValue({ login: mockLogin })

    render(
      <MemoryRouter initialEntries={["/login"]}>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/dashboard" element={<div>Dashboard</div>} />
        </Routes>
      </MemoryRouter>
    )

    await user.type(screen.getByLabelText(/email/i), 'test@test.com')
    await user.type(screen.getByLabelText(/password/i), 'pass{Enter}')

    await waitFor(() => {
      expect(screen.getByText('Dashboard')).toBeInTheDocument()
    })
  })
})
