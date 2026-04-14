import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { render, screen, waitFor, fireEvent, act } from '@testing-library/react'
import Register from '@/pages/Register'

const mockRegister = vi.fn()
const mockUseAuth = vi.fn()

vi.mock('@/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth()
}))

describe('Register page', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('renders register form with all fields', () => {
    mockUseAuth.mockReturnValue({ register: mockRegister })

    render(
      <MemoryRouter>
        <Register />
      </MemoryRouter>
    )

    expect(screen.getByRole('heading', { name: /create an account/i })).toBeInTheDocument()
    expect(screen.getByLabelText(/first name/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/last name/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /register/i })).toBeInTheDocument()
  })

  it('renders login link', () => {
    mockUseAuth.mockReturnValue({ register: mockRegister })

    render(
      <MemoryRouter>
        <Register />
      </MemoryRouter>
    )

    const loginLink = screen.getByRole('link', { name: /go to login/i })
    expect(loginLink).toBeInTheDocument()
    expect(loginLink).toHaveAttribute('href', '/login')
  })

  it('requires all fields', () => {
    mockUseAuth.mockReturnValue({ register: mockRegister })

    render(
      <MemoryRouter>
        <Register />
      </MemoryRouter>
    )

    expect(screen.getByLabelText(/first name/i)).toBeRequired()
    expect(screen.getByLabelText(/last name/i)).toBeRequired()
    expect(screen.getByLabelText(/email/i)).toBeRequired()
  })

  it('email input has email type', () => {
    mockUseAuth.mockReturnValue({ register: mockRegister })

    render(
      <MemoryRouter>
        <Register />
      </MemoryRouter>
    )

    expect(screen.getByLabelText(/email/i)).toHaveAttribute('type', 'email')
  })

  it('updates form fields correctly', () => {
    mockUseAuth.mockReturnValue({ register: mockRegister })

    render(
      <MemoryRouter>
        <Register />
      </MemoryRouter>
    )

    const firstNameInput = screen.getByLabelText(/first name/i) as HTMLInputElement
    const lastNameInput = screen.getByLabelText(/last name/i) as HTMLInputElement
    const emailInput = screen.getByLabelText(/email/i) as HTMLInputElement

    fireEvent.change(firstNameInput, { target: { value: 'Jane' } })
    fireEvent.change(lastNameInput, { target: { value: 'Smith' } })
    fireEvent.change(emailInput, { target: { value: 'jane@example.com' } })

    expect(firstNameInput.value).toBe('Jane')
    expect(lastNameInput.value).toBe('Smith')
    expect(emailInput.value).toBe('jane@example.com')
  })

  it('does not show message initially', () => {
    mockUseAuth.mockReturnValue({ register: mockRegister })

    render(
      <MemoryRouter>
        <Register />
      </MemoryRouter>
    )

    expect(screen.queryByText(/registered successfully/i)).not.toBeInTheDocument()
    expect(screen.queryByText(/all fields are required/i)).not.toBeInTheDocument()
  })

  it('shows success message after successful registration', async () => {
    mockRegister.mockResolvedValueOnce(true)
    mockUseAuth.mockReturnValue({ register: mockRegister })

    render(
      <MemoryRouter>
        <Register />
      </MemoryRouter>
    )

    const firstNameInput = screen.getByLabelText(/first name/i)
    const lastNameInput = screen.getByLabelText(/last name/i)
    const emailInput = screen.getByLabelText(/email/i)
    const form = screen.getByRole('button', { name: /register/i }).closest('form')!

    fireEvent.change(firstNameInput, { target: { value: 'Jane' } })
    fireEvent.change(lastNameInput, { target: { value: 'Smith' } })
    fireEvent.change(emailInput, { target: { value: 'jane@example.com' } })
    fireEvent.submit(form)

    await waitFor(() => {
      expect(mockRegister).toHaveBeenCalledWith('Jane', 'Smith', 'jane@example.com')
    })

    await waitFor(() => {
      expect(screen.getByText('Registration successful! Check your inbox for a verification email. Click the link to verify your address and set your password — you will then be able to log in.')).toBeInTheDocument()
    })
  })

  it('shows go-to-login link and hides form after successful registration', async () => {
    mockRegister.mockResolvedValueOnce(true)
    mockUseAuth.mockReturnValue({ register: mockRegister })

    render(
      <MemoryRouter>
        <Register />
      </MemoryRouter>
    )

    fireEvent.change(screen.getByLabelText(/first name/i), { target: { value: 'Jane' } })
    fireEvent.change(screen.getByLabelText(/last name/i), { target: { value: 'Smith' } })
    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'jane@example.com' } })

    await act(async () => {
      fireEvent.submit(screen.getByRole('button', { name: /register/i }).closest('form')!)
    })

    expect(screen.getByText('Registration successful! Check your inbox for a verification email. Click the link to verify your address and set your password — you will then be able to log in.')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /register/i })).not.toBeInTheDocument()

    const loginLink = screen.getByRole('link', { name: /go to login/i })
    expect(loginLink).toHaveAttribute('href', '/login')
  })

  it('shows error when email format is invalid', async () => {
    mockUseAuth.mockReturnValue({ register: mockRegister })

    render(
      <MemoryRouter>
        <Register />
      </MemoryRouter>
    )

    fireEvent.change(screen.getByLabelText(/first name/i), { target: { value: 'Jane' } })
    fireEvent.change(screen.getByLabelText(/last name/i), { target: { value: 'Smith' } })
    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'notanemail' } })
    fireEvent.submit(screen.getByRole('button', { name: /register/i }).closest('form')!)

    await waitFor(() => {
      expect(screen.getByText('Please enter a valid email address.')).toBeInTheDocument()
    })
    expect(mockRegister).not.toHaveBeenCalled()
  })

  it('shows error when fields are empty', async () => {
    mockUseAuth.mockReturnValue({ register: mockRegister })

    render(
      <MemoryRouter>
        <Register />
      </MemoryRouter>
    )

    fireEvent.submit(screen.getByRole('button', { name: /register/i }).closest('form')!)

    await waitFor(() => {
      expect(screen.getByText('All fields are required.')).toBeInTheDocument()
    })
    expect(mockRegister).not.toHaveBeenCalled()
  })

  it('shows error message when registration fails', async () => {
    mockRegister.mockResolvedValueOnce(false)
    mockUseAuth.mockReturnValue({ register: mockRegister })

    render(
      <MemoryRouter>
        <Register />
      </MemoryRouter>
    )

    fireEvent.change(screen.getByLabelText(/first name/i), { target: { value: 'Jane' } })
    fireEvent.change(screen.getByLabelText(/last name/i), { target: { value: 'Smith' } })
    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'jane@example.com' } })
    fireEvent.submit(screen.getByRole('button', { name: /register/i }).closest('form')!)

    await waitFor(() => {
      expect(mockRegister).toHaveBeenCalledWith('Jane', 'Smith', 'jane@example.com')
    })

    await waitFor(() => {
      expect(screen.getByText('Registration failed. Please try again.')).toBeInTheDocument()
    })
  })
})