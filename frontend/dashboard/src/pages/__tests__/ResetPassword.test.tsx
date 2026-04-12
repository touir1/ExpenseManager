import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, fireEvent } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import ResetPassword from '../ResetPassword'

const mockResetPassword = vi.fn()
const mockUseAuth = vi.fn()

vi.mock('@/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth()
}))

describe('ResetPassword page', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUseAuth.mockReturnValue({
      resetPassword: mockResetPassword
    })
  })

  it('renders reset password form with password fields', () => {
    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com&h=abc123']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByRole('heading', { name: /reset password/i })).toBeInTheDocument()
    expect(screen.getByLabelText(/^new password$/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/repeat new password/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /reset/i })).toBeInTheDocument()
  })

  it('extracts email and verification hash from URL parameters', () => {
    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com&h=abc123']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </MemoryRouter>
    )

    const button = screen.getByRole('button', { name: /reset/i })
    expect(button).toBeEnabled()
    expect(screen.queryByText(/invalid or missing verification link/i)).not.toBeInTheDocument()
  })

  it('extracts email and verificationHash parameter (alternative name)', () => {
    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com&verificationHash=xyz789']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </MemoryRouter>
    )

    const button = screen.getByRole('button', { name: /reset/i })
    expect(button).toBeEnabled()
  })

  it('shows warning when email is missing', () => {
    render(
      <MemoryRouter initialEntries={['/reset-password?h=abc123']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByText(/invalid or missing verification link/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /reset/i })).toBeDisabled()
  })

  it('shows warning when verification hash is missing', () => {
    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByText(/invalid or missing verification link/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /reset/i })).toBeDisabled()
  })

  it('shows link to request password reset when params are missing', () => {
    render(
      <MemoryRouter initialEntries={['/reset-password']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </MemoryRouter>
    )

    const link = screen.getByRole('link', { name: /request password reset/i })
    expect(link).toBeInTheDocument()
    expect(link).toHaveAttribute('href', '/request-password-reset')
  })

  it('does not show request link when params are valid', () => {
    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com&h=abc123']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.queryByRole('link', { name: /request password reset/i })).not.toBeInTheDocument()
  })

  it('password inputs have password type and are required', () => {
    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com&h=abc123']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </MemoryRouter>
    )

    const newPasswordInput = screen.getByLabelText(/^new password$/i)
    const repeatPasswordInput = screen.getByLabelText(/repeat new password/i)

    expect(newPasswordInput).toHaveAttribute('type', 'password')
    expect(newPasswordInput).toBeRequired()
    expect(repeatPasswordInput).toHaveAttribute('type', 'password')
    expect(repeatPasswordInput).toBeRequired()
  })

  it('updates new password field when typing', () => {
    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com&h=abc123']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </MemoryRouter>
    )

    const newPasswordInput = screen.getByLabelText(/^new password$/i) as HTMLInputElement
    fireEvent.change(newPasswordInput, { target: { value: 'newpass123' } })
    
    expect(newPasswordInput.value).toBe('newpass123')
  })

  it('updates repeat password field when typing', () => {
    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com&h=abc123']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </MemoryRouter>
    )

    const repeatPasswordInput = screen.getByLabelText(/repeat new password/i) as HTMLInputElement
    fireEvent.change(repeatPasswordInput, { target: { value: 'newpass123' } })
    
    expect(repeatPasswordInput.value).toBe('newpass123')
  })

  it('does not show message initially', () => {
    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com&h=abc123']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.queryByText(/password reset\./i)).not.toBeInTheDocument()
    expect(screen.queryByText(/all fields are required/i)).not.toBeInTheDocument()
    expect(screen.queryByText(/new passwords do not match/i)).not.toBeInTheDocument()
    expect(screen.queryByText(/password reset failed/i)).not.toBeInTheDocument()
  })

  it('shows success message when password reset succeeds', async () => {
    mockResetPassword.mockResolvedValueOnce(true)

    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com&h=abc123']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </MemoryRouter>
    )

    const newPasswordInput = screen.getByLabelText(/^new password$/i)
    const repeatPasswordInput = screen.getByLabelText(/repeat new password/i)
    const form = screen.getByRole('button', { name: /reset/i }).closest('form')!

    fireEvent.change(newPasswordInput, { target: { value: 'newpass123' } })
    fireEvent.change(repeatPasswordInput, { target: { value: 'newpass123' } })
    fireEvent.submit(form)

    await waitFor(() => {
      expect(mockResetPassword).toHaveBeenCalledWith('test@example.com', 'abc123', 'newpass123', 'newpass123')
    })

    await waitFor(() => {
      expect(screen.getByText('Password reset successfully. Redirecting to home\u2026')).toBeInTheDocument()
    })
  })

  it('shows error message when password reset fails', async () => {
    mockResetPassword.mockResolvedValueOnce(false)

    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com&h=abc123']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </MemoryRouter>
    )

    const newPasswordInput = screen.getByLabelText(/^new password$/i)
    const repeatPasswordInput = screen.getByLabelText(/repeat new password/i)
    const form = screen.getByRole('button', { name: /reset/i }).closest('form')!

    fireEvent.change(newPasswordInput, { target: { value: 'newpass123' } })
    fireEvent.change(repeatPasswordInput, { target: { value: 'newpass123' } })
    fireEvent.submit(form)

    await waitFor(() => {
      expect(mockResetPassword).toHaveBeenCalledWith('test@example.com', 'abc123', 'newpass123', 'newpass123')
    })

    await waitFor(() => {
      expect(screen.getByText('Password reset failed. Please try again.')).toBeInTheDocument()
    })
  })

  it('shows "All fields are required." when any field is empty', async () => {
    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com&h=abc123']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </MemoryRouter>
    )

    const form = screen.getByRole('button', { name: /reset/i }).closest('form')!
    fireEvent.submit(form)

    expect(screen.getByText('All fields are required.')).toBeInTheDocument()
    expect(mockResetPassword).not.toHaveBeenCalled()
  })

  it('shows "New passwords do not match." when passwords differ', async () => {
    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com&h=abc123']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </MemoryRouter>
    )

    const newPasswordInput = screen.getByLabelText(/^new password$/i)
    const repeatPasswordInput = screen.getByLabelText(/repeat new password/i)
    const form = screen.getByRole('button', { name: /reset/i }).closest('form')!

    fireEvent.change(newPasswordInput, { target: { value: 'pass1' } })
    fireEvent.change(repeatPasswordInput, { target: { value: 'pass2' } })
    fireEvent.submit(form)

    expect(screen.getByText('New passwords do not match.')).toBeInTheDocument()
    expect(mockResetPassword).not.toHaveBeenCalled()
  })

  it('calls resetPassword with verificationHash parameter', async () => {
    mockResetPassword.mockResolvedValueOnce(true)

    render(
      <MemoryRouter initialEntries={['/reset-password?email=user@test.com&verificationHash=xyz789']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </MemoryRouter>
    )

    const newPasswordInput = screen.getByLabelText(/^new password$/i)
    const repeatPasswordInput = screen.getByLabelText(/repeat new password/i)
    const form = screen.getByRole('button', { name: /reset/i }).closest('form')!

    fireEvent.change(newPasswordInput, { target: { value: 'test123' } })
    fireEvent.change(repeatPasswordInput, { target: { value: 'test123' } })
    fireEvent.submit(form)

    await waitFor(() => {
      expect(mockResetPassword).toHaveBeenCalledWith('user@test.com', 'xyz789', 'test123', 'test123')
    })
  })

  it('prevents default form submission', async () => {
    mockResetPassword.mockResolvedValueOnce(true)

    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com&h=abc123']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </MemoryRouter>
    )

    const form = screen.getByRole('button', { name: /reset/i }).closest('form')!
    const newPasswordInput = screen.getByLabelText(/^new password$/i)
    const repeatPasswordInput = screen.getByLabelText(/repeat new password/i)

    fireEvent.change(newPasswordInput, { target: { value: 'test' } })
    fireEvent.change(repeatPasswordInput, { target: { value: 'test' } })
    fireEvent.submit(form)

    await waitFor(() => {
      expect(mockResetPassword).toHaveBeenCalled()
    })
  })
})
