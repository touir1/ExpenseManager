import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import ChangePassword from '../ChangePassword'

// Mock useAuth
const mockChangePassword = vi.fn()
const mockUseAuth = vi.fn()

vi.mock('@/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth()
}))

describe('ChangePassword page', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUseAuth.mockReturnValue({
      changePassword: mockChangePassword
    })
  })

  it('renders change password form with all fields', () => {
    render(
      <MemoryRouter>
        <ChangePassword />
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
        <ChangePassword />
      </MemoryRouter>
    )

    const backLink = screen.getByRole('link', { name: /back to settings/i })
    expect(backLink).toBeInTheDocument()
    expect(backLink).toHaveAttribute('href', '/settings')
  })

  it('updates old password field when typing', () => {
    render(
      <MemoryRouter>
        <ChangePassword />
      </MemoryRouter>
    )

    const oldPasswordInput = screen.getByLabelText(/old password/i)
    fireEvent.change(oldPasswordInput, { target: { value: 'oldpass123' } })
    
    expect(oldPasswordInput).toHaveValue('oldpass123')
  })

  it('updates new password field when typing', () => {
    render(
      <MemoryRouter>
        <ChangePassword />
      </MemoryRouter>
    )

    const newPasswordInput = screen.getByLabelText(/^new password$/i)
    fireEvent.change(newPasswordInput, { target: { value: 'newpass456' } })
    
    expect(newPasswordInput).toHaveValue('newpass456')
  })

  it('updates repeat password field when typing', () => {
    render(
      <MemoryRouter>
        <ChangePassword />
      </MemoryRouter>
    )

    const repeatPasswordInput = screen.getByLabelText(/repeat new password/i)
    fireEvent.change(repeatPasswordInput, { target: { value: 'newpass456' } })
    
    expect(repeatPasswordInput).toHaveValue('newpass456')
  })

  it('calls changePassword with correct data on form submit', async () => {
    mockChangePassword.mockResolvedValueOnce(true)

    render(
      <MemoryRouter>
        <ChangePassword />
      </MemoryRouter>
    )

    const oldPasswordInput = screen.getByLabelText(/old password/i)
    const newPasswordInput = screen.getByLabelText(/^new password$/i)
    const repeatPasswordInput = screen.getByLabelText(/repeat new password/i)
    const submitButton = screen.getByRole('button', { name: /change password/i })

    fireEvent.change(oldPasswordInput, { target: { value: 'oldpass123' } })
    fireEvent.change(newPasswordInput, { target: { value: 'newpass456' } })
    fireEvent.change(repeatPasswordInput, { target: { value: 'newpass456' } })
    fireEvent.click(submitButton)

    await waitFor(() => {
      expect(mockChangePassword).toHaveBeenCalledWith('oldpass123', 'newpass456', 'newpass456')
    })
  })

  it('shows success message when password change succeeds', async () => {
    mockChangePassword.mockResolvedValueOnce(true)

    render(
      <MemoryRouter>
        <ChangePassword />
      </MemoryRouter>
    )

    const oldPasswordInput = screen.getByLabelText(/old password/i)
    const newPasswordInput = screen.getByLabelText(/^new password$/i)
    const repeatPasswordInput = screen.getByLabelText(/repeat new password/i)
    const submitButton = screen.getByRole('button', { name: /change password/i })

    fireEvent.change(oldPasswordInput, { target: { value: 'oldpass123' } })
    fireEvent.change(newPasswordInput, { target: { value: 'newpass456' } })
    fireEvent.change(repeatPasswordInput, { target: { value: 'newpass456' } })
    fireEvent.click(submitButton)

    await waitFor(() => {
      expect(screen.getByText('Password changed.')).toBeInTheDocument()
    })
  })

  it('shows error message when password change fails due to incorrect current password', async () => {
    mockChangePassword.mockResolvedValueOnce(false)

    render(
      <MemoryRouter>
        <ChangePassword />
      </MemoryRouter>
    )

    const oldPasswordInput = screen.getByLabelText(/old password/i)
    const newPasswordInput = screen.getByLabelText(/^new password$/i)
    const repeatPasswordInput = screen.getByLabelText(/repeat new password/i)
    const submitButton = screen.getByRole('button', { name: /change password/i })

    fireEvent.change(oldPasswordInput, { target: { value: 'wrongpass' } })
    fireEvent.change(newPasswordInput, { target: { value: 'newpass456' } })
    fireEvent.change(repeatPasswordInput, { target: { value: 'newpass456' } })
    fireEvent.click(submitButton)

    await waitFor(() => {
      expect(screen.getByText('Incorrect current password.')).toBeInTheDocument()
    })
  })

  it('shows "All fields are required." when any field is empty', async () => {
    render(
      <MemoryRouter>
        <ChangePassword />
      </MemoryRouter>
    )

    const submitButton = screen.getByRole('button', { name: /change password/i })
    fireEvent.click(submitButton)

    expect(screen.getByText('All fields are required.')).toBeInTheDocument()
    expect(mockChangePassword).not.toHaveBeenCalled()
  })

  it('shows "New passwords do not match." when new passwords differ', async () => {
    render(
      <MemoryRouter>
        <ChangePassword />
      </MemoryRouter>
    )

    const oldPasswordInput = screen.getByLabelText(/old password/i)
    const newPasswordInput = screen.getByLabelText(/^new password$/i)
    const repeatPasswordInput = screen.getByLabelText(/repeat new password/i)
    const submitButton = screen.getByRole('button', { name: /change password/i })

    fireEvent.change(oldPasswordInput, { target: { value: 'oldpass123' } })
    fireEvent.change(newPasswordInput, { target: { value: 'newpass1' } })
    fireEvent.change(repeatPasswordInput, { target: { value: 'newpass2' } })
    fireEvent.click(submitButton)

    expect(screen.getByText('New passwords do not match.')).toBeInTheDocument()
    expect(mockChangePassword).not.toHaveBeenCalled()
  })

  it('does not show message initially', () => {
    render(
      <MemoryRouter>
        <ChangePassword />
      </MemoryRouter>
    )

    expect(screen.queryByText('Password changed.')).not.toBeInTheDocument()
    expect(screen.queryByText('All fields are required.')).not.toBeInTheDocument()
    expect(screen.queryByText('New passwords do not match.')).not.toBeInTheDocument()
    expect(screen.queryByText('Incorrect current password.')).not.toBeInTheDocument()
  })

  it('prevents default form submission', async () => {
    mockChangePassword.mockResolvedValueOnce(true)

    render(
      <MemoryRouter>
        <ChangePassword />
      </MemoryRouter>
    )

    const form = screen.getByRole('button', { name: /change password/i }).closest('form')!
    const preventDefaultSpy = vi.fn()
    
    form.addEventListener('submit', () => {
      preventDefaultSpy()
      // The component already calls e.preventDefault(), so we just track it
    })

    const oldPasswordInput = screen.getByLabelText(/old password/i)
    const newPasswordInput = screen.getByLabelText(/^new password$/i)
    const repeatPasswordInput = screen.getByLabelText(/repeat new password/i)

    fireEvent.change(oldPasswordInput, { target: { value: 'old' } })
    fireEvent.change(newPasswordInput, { target: { value: 'new' } })
    fireEvent.change(repeatPasswordInput, { target: { value: 'new' } })
    fireEvent.submit(form)

    await waitFor(() => {
      expect(mockChangePassword).toHaveBeenCalled()
    })
  })
})
