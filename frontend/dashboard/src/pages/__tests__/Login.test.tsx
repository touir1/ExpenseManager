import { describe, it, expect, vi, beforeEach } from 'vitest'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import Login from '@/pages/Login'

const mockLogin = vi.fn()
const mockUseAuth = vi.fn()
const mockShow = vi.fn()

vi.mock('@/features/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth()
}))

vi.mock('@/components/Toast', () => ({
  useToast: () => ({ show: mockShow })
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
    expect(screen.getByLabelText(/email address/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/^password$/i)).toBeInTheDocument()
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

    await user.type(screen.getByLabelText(/email address/i), 'user@example.com')
    await user.type(screen.getByLabelText(/^password$/i), 'secret')
    await user.click(screen.getByRole('button', { name: /login/i }))

    await waitFor(() => {
      expect(screen.getByText('Dashboard')).toBeInTheDocument()
    })

    expect(mockLogin).toHaveBeenCalledWith('user@example.com', 'secret', false)
  })

  it('shows toast error on invalid credentials', async () => {
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

    await user.type(screen.getByLabelText(/email address/i), 'user@example.com')
    await user.type(screen.getByLabelText(/^password$/i), 'bad')
    await user.click(screen.getByRole('button', { name: /login/i }))

    await waitFor(() => {
      expect(mockShow).toHaveBeenCalledWith('Invalid credentials. Please try again.', 'error')
    })

    expect(mockLogin).toHaveBeenCalledWith('user@example.com', 'bad', false)
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

    await user.type(screen.getByLabelText(/email address/i), 'wrong@example.com')
    await user.type(screen.getByLabelText(/^password$/i), 'wrongpass')
    await user.click(screen.getByRole('button', { name: /login/i }))

    await waitFor(() => {
      expect(mockShow).toHaveBeenCalledWith('Invalid credentials. Please try again.', 'error')
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

    const emailInput = screen.getByLabelText(/email address/i)
    const passwordInput = screen.getByLabelText(/^password$/i)

    await user.type(emailInput, 'test@test.com')
    await user.type(passwordInput, 'password123')
    await user.click(screen.getByRole('button', { name: /login/i }))

    expect(mockLogin).toHaveBeenCalledTimes(1)
    expect(mockLogin).toHaveBeenCalledWith('test@test.com', 'password123', false)
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

    const emailInput = screen.getByLabelText(/email address/i)
    const passwordInput = screen.getByLabelText(/^password$/i)

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

    const emailInput = screen.getByLabelText(/email address/i)
    const passwordInput = screen.getByLabelText(/^password$/i)

    expect(emailInput).toHaveAttribute('type', 'email')
    expect(passwordInput).toHaveAttribute('type', 'password')
  })

  it('does not show inline error initially', () => {
    mockUseAuth.mockReturnValue({ login: mockLogin })

    render(
      <MemoryRouter initialEntries={["/login"]}>
        <Routes>
          <Route path="/login" element={<Login />} />
        </Routes>
      </MemoryRouter>
    )

    expect(mockShow).not.toHaveBeenCalled()
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

    const emailInput = screen.getByLabelText(/email address/i) as HTMLInputElement

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

    const passwordInput = screen.getByLabelText(/^password$/i) as HTMLInputElement

    await user.type(passwordInput, 'mypassword')

    expect(passwordInput.value).toBe('mypassword')
  })

  it('disables button and shows spinner while submitting', async () => {
    const user = userEvent.setup()
    let resolve: (v: boolean) => void
    mockLogin.mockReturnValue(new Promise(r => { resolve = r }))
    mockUseAuth.mockReturnValue({ login: mockLogin })

    render(
      <MemoryRouter initialEntries={["/login"]}>
        <Routes>
          <Route path="/login" element={<Login />} />
        </Routes>
      </MemoryRouter>
    )

    await user.type(screen.getByLabelText(/email address/i), 'user@example.com')
    await user.type(screen.getByLabelText(/^password$/i), 'secret')
    await user.click(screen.getByRole('button', { name: /login/i }))

    await waitFor(() => {
      expect(screen.getByText(/signing in/i)).toBeInTheDocument()
    })
    expect(screen.getByRole('button', { name: /signing in/i })).toBeDisabled()

    resolve!(true)
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

    await user.type(screen.getByLabelText(/email address/i), 'test@test.com')
    await user.type(screen.getByLabelText(/^password$/i), 'pass{Enter}')

    await waitFor(() => {
      expect(screen.getByText('Dashboard')).toBeInTheDocument()
    })
  })

  it('renders remember me checkbox unchecked by default', () => {
    mockUseAuth.mockReturnValue({ login: mockLogin })

    render(
      <MemoryRouter initialEntries={["/login"]}>
        <Routes>
          <Route path="/login" element={<Login />} />
        </Routes>
      </MemoryRouter>
    )

    const checkbox = screen.getByRole('checkbox', { name: /remember me/i }) as HTMLInputElement
    expect(checkbox).toBeInTheDocument()
    expect(checkbox.checked).toBe(false)
  })

  it('passes rememberMe=true to login when checkbox is checked', async () => {
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

    await user.type(screen.getByLabelText(/email address/i), 'user@example.com')
    await user.type(screen.getByLabelText(/^password$/i), 'secret')
    await user.click(screen.getByRole('checkbox', { name: /remember me/i }))
    await user.click(screen.getByRole('button', { name: /login/i }))

    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith('user@example.com', 'secret', true)
    })
  })
})
