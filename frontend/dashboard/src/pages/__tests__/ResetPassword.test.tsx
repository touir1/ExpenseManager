import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, fireEvent } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import ResetPassword from '../ResetPassword'

const mockResetPassword = vi.fn()
const mockUseAuth = vi.fn()

vi.mock('@/features/auth/AuthContext', () => ({
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

  it('shows "Password must be at least 8 characters." when new password is too short', async () => {
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

    fireEvent.change(newPasswordInput, { target: { value: 'short' } })
    fireEvent.change(repeatPasswordInput, { target: { value: 'short' } })
    fireEvent.submit(form)

    expect(screen.getByText('Password must be at least 8 characters.')).toBeInTheDocument()
    expect(mockResetPassword).not.toHaveBeenCalled()
  })

  it('shows password strength indicator when typing in new password field', () => {
    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com&h=abc123']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </MemoryRouter>
    )

    const newPasswordInput = screen.getByLabelText(/^new password$/i)
    fireEvent.change(newPasswordInput, { target: { value: 'Test1!' } })

    expect(screen.getByRole('list', { name: /password requirements/i })).toBeInTheDocument()
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

    fireEvent.change(newPasswordInput, { target: { value: 'Password1' } })
    fireEvent.change(repeatPasswordInput, { target: { value: 'Password2' } })
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

    fireEvent.change(newPasswordInput, { target: { value: 'testpass1' } })
    fireEvent.change(repeatPasswordInput, { target: { value: 'testpass1' } })
    fireEvent.submit(form)

    await waitFor(() => {
      expect(mockResetPassword).toHaveBeenCalledWith('user@test.com', 'xyz789', 'testpass1', 'testpass1')
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

    fireEvent.change(newPasswordInput, { target: { value: 'testpass1' } })
    fireEvent.change(repeatPasswordInput, { target: { value: 'testpass1' } })
    fireEvent.submit(form)

    await waitFor(() => {
      expect(mockResetPassword).toHaveBeenCalled()
    })
  })

  describe('create mode (mode=create)', () => {
    it('shows "Create Password" heading and "Create" button', () => {
      render(
        <MemoryRouter initialEntries={['/reset-password?email=test@example.com&h=abc123&mode=create']}>
          <Routes>
            <Route path="/reset-password" element={<ResetPassword />} />
          </Routes>
        </MemoryRouter>
      )

      expect(screen.getByRole('heading', { name: /create password/i })).toBeInTheDocument()
      expect(screen.getByRole('button', { name: /^create$/i })).toBeInTheDocument()
      expect(screen.queryByRole('heading', { name: /reset password/i })).not.toBeInTheDocument()
    })

    it('shows success message for create mode', async () => {
      mockResetPassword.mockResolvedValueOnce(true)

      render(
        <MemoryRouter initialEntries={['/reset-password?email=test@example.com&h=abc123&mode=create']}>
          <Routes>
            <Route path="/reset-password" element={<ResetPassword />} />
          </Routes>
        </MemoryRouter>
      )

      fireEvent.change(screen.getByLabelText(/^new password$/i), { target: { value: 'newpass123' } })
      fireEvent.change(screen.getByLabelText(/repeat new password/i), { target: { value: 'newpass123' } })
      fireEvent.submit(screen.getByRole('button', { name: /^create$/i }).closest('form')!)

      await waitFor(() => {
        expect(screen.getByText('Password created successfully. Redirecting to home\u2026')).toBeInTheDocument()
      })
    })

    it('shows error message for create mode on failure', async () => {
      mockResetPassword.mockResolvedValueOnce(false)

      render(
        <MemoryRouter initialEntries={['/reset-password?email=test@example.com&h=abc123&mode=create']}>
          <Routes>
            <Route path="/reset-password" element={<ResetPassword />} />
          </Routes>
        </MemoryRouter>
      )

      fireEvent.change(screen.getByLabelText(/^new password$/i), { target: { value: 'newpass123' } })
      fireEvent.change(screen.getByLabelText(/repeat new password/i), { target: { value: 'newpass123' } })
      fireEvent.submit(screen.getByRole('button', { name: /^create$/i }).closest('form')!)

      await waitFor(() => {
        expect(screen.getByText('Password creation failed. Please try again.')).toBeInTheDocument()
      })
    })

    it('shows "Reset Password" heading and "Reset" button when mode is absent', () => {
      render(
        <MemoryRouter initialEntries={['/reset-password?email=test@example.com&h=abc123']}>
          <Routes>
            <Route path="/reset-password" element={<ResetPassword />} />
          </Routes>
        </MemoryRouter>
      )

      expect(screen.getByRole('heading', { name: /reset password/i })).toBeInTheDocument()
      expect(screen.getByRole('button', { name: /^reset$/i })).toBeInTheDocument()
    })
  })

  it('inputs have no aria-describedby initially', () => {
    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com&h=abc123']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </MemoryRouter>
    )
    expect(screen.getByLabelText(/^new password$/i)).not.toHaveAttribute('aria-describedby')
    expect(screen.getByLabelText(/repeat new password/i)).not.toHaveAttribute('aria-describedby')
  })

  it('inputs link to error message via aria-describedby when validation fails', () => {
    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com&h=abc123']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPassword />} />
        </Routes>
      </MemoryRouter>
    )

    fireEvent.submit(screen.getByRole('button', { name: /reset/i }).closest('form')!)

    const errorEl = screen.getByText('All fields are required.')
    expect(errorEl).toHaveAttribute('id', 'reset-password-msg')
    expect(screen.getByLabelText(/^new password$/i)).toHaveAttribute('aria-describedby', 'reset-password-msg')
    expect(screen.getByLabelText(/repeat new password/i)).toHaveAttribute('aria-describedby', 'reset-password-msg')
  })
})
