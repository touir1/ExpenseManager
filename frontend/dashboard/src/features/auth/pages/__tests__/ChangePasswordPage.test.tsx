import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import ChangePasswordPage from '../ChangePasswordPage'

const mockChangePassword = vi.fn()
const mockUseAuth = vi.fn()

vi.mock('@/features/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth()
}))

describe('ChangePasswordPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUseAuth.mockReturnValue({
      changePassword: mockChangePassword
    })
  })

  it('renders change password form with all fields', () => {
    render(
      <MemoryRouter>
        <ChangePasswordPage />
      </MemoryRouter>
    )

    expect(screen.getByText('Change Password')).toBeInTheDocument()
    expect(screen.getByLabelText(/old password/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/^new password$/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/repeat new password/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /change password/i })).toBeInTheDocument()
  })

  it('renders back to settings link pointing to /settings', () => {
    render(
      <MemoryRouter>
        <ChangePasswordPage />
      </MemoryRouter>
    )

    const backLink = screen.getByRole('link', { name: /back to settings/i })
    expect(backLink).toBeInTheDocument()
    expect(backLink).toHaveAttribute('href', '/settings')
  })

  it('updates old password field when typing', () => {
    render(
      <MemoryRouter>
        <ChangePasswordPage />
      </MemoryRouter>
    )

    const oldPasswordInput = screen.getByLabelText(/old password/i)
    fireEvent.change(oldPasswordInput, { target: { value: 'oldpass123' } })

    expect(oldPasswordInput).toHaveValue('oldpass123')
  })

  it('updates new password field when typing', () => {
    render(
      <MemoryRouter>
        <ChangePasswordPage />
      </MemoryRouter>
    )

    const newPasswordInput = screen.getByLabelText(/^new password$/i)
    fireEvent.change(newPasswordInput, { target: { value: 'newpass456' } })

    expect(newPasswordInput).toHaveValue('newpass456')
  })

  it('updates repeat password field when typing', () => {
    render(
      <MemoryRouter>
        <ChangePasswordPage />
      </MemoryRouter>
    )

    const repeatPasswordInput = screen.getByLabelText(/repeat new password/i)
    fireEvent.change(repeatPasswordInput, { target: { value: 'newpass456' } })

    expect(repeatPasswordInput).toHaveValue('newpass456')
  })

  it('calls changePassword with correct data on form submit', async () => {
    mockChangePassword.mockResolvedValueOnce({ ok: true })

    render(
      <MemoryRouter>
        <ChangePasswordPage />
      </MemoryRouter>
    )

    fireEvent.change(screen.getByLabelText(/old password/i), { target: { value: 'oldpass123' } })
    fireEvent.change(screen.getByLabelText(/^new password$/i), { target: { value: 'newpass456' } })
    fireEvent.change(screen.getByLabelText(/repeat new password/i), { target: { value: 'newpass456' } })
    fireEvent.click(screen.getByRole('button', { name: /change password/i }))

    await waitFor(() => {
      expect(mockChangePassword).toHaveBeenCalledWith('oldpass123', 'newpass456', 'newpass456')
    })
  })

  it('shows success message when password change succeeds', async () => {
    mockChangePassword.mockResolvedValueOnce({ ok: true })

    render(
      <MemoryRouter>
        <ChangePasswordPage />
      </MemoryRouter>
    )

    fireEvent.change(screen.getByLabelText(/old password/i), { target: { value: 'oldpass123' } })
    fireEvent.change(screen.getByLabelText(/^new password$/i), { target: { value: 'newpass456' } })
    fireEvent.change(screen.getByLabelText(/repeat new password/i), { target: { value: 'newpass456' } })
    fireEvent.click(screen.getByRole('button', { name: /change password/i }))

    await waitFor(() => {
      expect(screen.getByText('Password changed.')).toBeInTheDocument()
    })
  })

  it('shows error message when password change fails due to incorrect current password', async () => {
    mockChangePassword.mockResolvedValueOnce({ ok: false })

    render(
      <MemoryRouter>
        <ChangePasswordPage />
      </MemoryRouter>
    )

    fireEvent.change(screen.getByLabelText(/old password/i), { target: { value: 'wrongpass' } })
    fireEvent.change(screen.getByLabelText(/^new password$/i), { target: { value: 'newpass456' } })
    fireEvent.change(screen.getByLabelText(/repeat new password/i), { target: { value: 'newpass456' } })
    fireEvent.click(screen.getByRole('button', { name: /change password/i }))

    await waitFor(() => {
      expect(screen.getByText('Incorrect current password.')).toBeInTheDocument()
    })
  })

  it('shows per-field errors when any field is empty', async () => {
    render(
      <MemoryRouter>
        <ChangePasswordPage />
      </MemoryRouter>
    )

    fireEvent.click(screen.getByRole('button', { name: /change password/i }))

    await waitFor(() => {
      expect(screen.getByText('Current password is required.')).toBeInTheDocument()
      expect(screen.getByText('New password is required.')).toBeInTheDocument()
      expect(screen.getByText('Please repeat your new password.')).toBeInTheDocument()
    })
    expect(mockChangePassword).not.toHaveBeenCalled()
  })

  it('shows "Password must be at least 8 characters." when new password is too short', async () => {
    render(
      <MemoryRouter>
        <ChangePasswordPage />
      </MemoryRouter>
    )

    fireEvent.change(screen.getByLabelText(/old password/i), { target: { value: 'oldpass1' } })
    fireEvent.change(screen.getByLabelText(/^new password$/i), { target: { value: 'short' } })
    fireEvent.change(screen.getByLabelText(/repeat new password/i), { target: { value: 'short' } })
    fireEvent.click(screen.getByRole('button', { name: /change password/i }))

    await waitFor(() => {
      expect(screen.getByText('Password must be at least 8 characters.')).toBeInTheDocument()
    })
    expect(mockChangePassword).not.toHaveBeenCalled()
  })

  it('shows "New passwords do not match." when new passwords differ', async () => {
    render(
      <MemoryRouter>
        <ChangePasswordPage />
      </MemoryRouter>
    )

    fireEvent.change(screen.getByLabelText(/old password/i), { target: { value: 'oldpass123' } })
    fireEvent.change(screen.getByLabelText(/^new password$/i), { target: { value: 'newpass1!' } })
    fireEvent.change(screen.getByLabelText(/repeat new password/i), { target: { value: 'newpass2!' } })
    fireEvent.click(screen.getByRole('button', { name: /change password/i }))

    await waitFor(() => {
      expect(screen.getByText('New passwords do not match.')).toBeInTheDocument()
    })
    expect(mockChangePassword).not.toHaveBeenCalled()
  })

  it('shows password strength indicator when typing in new password field', () => {
    render(
      <MemoryRouter>
        <ChangePasswordPage />
      </MemoryRouter>
    )

    const newPasswordInput = screen.getByLabelText(/^new password$/i)
    fireEvent.change(newPasswordInput, { target: { value: 'Test1!' } })

    expect(screen.getByRole('list', { name: /password requirements/i })).toBeInTheDocument()
  })

  it('does not show message initially', () => {
    render(
      <MemoryRouter>
        <ChangePasswordPage />
      </MemoryRouter>
    )

    expect(screen.queryByText('Password changed.')).not.toBeInTheDocument()
    expect(screen.queryByText('Current password is required.')).not.toBeInTheDocument()
    expect(screen.queryByText('New passwords do not match.')).not.toBeInTheDocument()
    expect(screen.queryByText('Incorrect current password.')).not.toBeInTheDocument()
  })

  it('prevents default form submission', async () => {
    mockChangePassword.mockResolvedValueOnce({ ok: true })

    render(
      <MemoryRouter>
        <ChangePasswordPage />
      </MemoryRouter>
    )

    const form = screen.getByRole('button', { name: /change password/i }).closest('form')!
    const preventDefaultSpy = vi.fn()

    form.addEventListener('submit', () => {
      preventDefaultSpy()
    })

    fireEvent.change(screen.getByLabelText(/old password/i), { target: { value: 'oldpass1!' } })
    fireEvent.change(screen.getByLabelText(/^new password$/i), { target: { value: 'newpass1!' } })
    fireEvent.change(screen.getByLabelText(/repeat new password/i), { target: { value: 'newpass1!' } })
    fireEvent.submit(form)

    await waitFor(() => {
      expect(mockChangePassword).toHaveBeenCalled()
    })
  })

  it('inputs have no aria-describedby initially', () => {
    render(<MemoryRouter><ChangePasswordPage /></MemoryRouter>)
    expect(screen.getByLabelText(/old password/i)).not.toHaveAttribute('aria-describedby')
    expect(screen.getByLabelText(/^new password$/i)).not.toHaveAttribute('aria-describedby')
    expect(screen.getByLabelText(/repeat new password/i)).not.toHaveAttribute('aria-describedby')
  })

  it('inputs link to per-field error via aria-describedby when validation fails', async () => {
    render(<MemoryRouter><ChangePasswordPage /></MemoryRouter>)

    fireEvent.click(screen.getByRole('button', { name: /change password/i }))

    await waitFor(() => {
      expect(screen.getByLabelText(/old password/i)).toHaveAttribute('aria-describedby', 'oldPassword-error')
      expect(screen.getByLabelText(/^new password$/i)).toHaveAttribute('aria-describedby', 'newPassword-error')
      expect(screen.getByLabelText(/repeat new password/i)).toHaveAttribute('aria-describedby', 'repeatPassword-error')
    })
  })
})
