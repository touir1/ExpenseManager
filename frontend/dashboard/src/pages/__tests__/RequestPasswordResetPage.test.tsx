import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, fireEvent } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import RequestPasswordResetPage from '../RequestPasswordResetPage'

const mockRequestPasswordReset = vi.fn()
const mockUseAuth = vi.fn()
const mockShow = vi.fn()
const mockUseToast = vi.fn()

vi.mock('@/features/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth()
}))

vi.mock('@/components/Toast', () => ({
  useToast: () => mockUseToast()
}))

describe('RequestPasswordReset page', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUseAuth.mockReturnValue({
      requestPasswordReset: mockRequestPasswordReset
    })
    mockUseToast.mockReturnValue({
      show: mockShow
    })
  })

  it('renders request password reset form with email field', () => {
    render(
      <MemoryRouter>
        <RequestPasswordResetPage />
      </MemoryRouter>
    )

    expect(screen.getByRole('heading', { name: /request password reset/i })).toBeInTheDocument()
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /send reset link/i })).toBeInTheDocument()
  })

  it('renders info message', () => {
    render(
      <MemoryRouter>
        <RequestPasswordResetPage />
      </MemoryRouter>
    )

    expect(screen.getByText(/you will receive an email with a verification link/i)).toBeInTheDocument()
  })

  it('email input has email type and is required', () => {
    render(
      <MemoryRouter>
        <RequestPasswordResetPage />
      </MemoryRouter>
    )

    const emailInput = screen.getByLabelText(/email/i)
    expect(emailInput).toHaveAttribute('type', 'email')
    expect(emailInput).toBeRequired()
  })

  it('updates email field when typing', () => {
    render(
      <MemoryRouter>
        <RequestPasswordResetPage />
      </MemoryRouter>
    )

    const emailInput = screen.getByLabelText(/email/i) as HTMLInputElement
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } })
    
    expect(emailInput.value).toBe('test@example.com')
  })

  it('shows error toast when requestPasswordReset is not available', async () => {
    mockUseAuth.mockReturnValue({
      requestPasswordReset: null
    })

    render(
      <MemoryRouter>
        <RequestPasswordResetPage />
      </MemoryRouter>
    )

    const emailInput = screen.getByLabelText(/email/i)
    const form = screen.getByRole('button', { name: /send reset link/i }).closest('form')!

    fireEvent.change(emailInput, { target: { value: 'test@example.com' } })
    fireEvent.submit(form)

    await waitFor(() => {
      expect(mockShow).toHaveBeenCalledWith('Reset request not available', 'error')
    })

    expect(mockRequestPasswordReset).not.toHaveBeenCalled()
  })

  it('shows success toast when password reset request succeeds', async () => {
    mockRequestPasswordReset.mockResolvedValueOnce({ ok: true })

    render(
      <MemoryRouter>
        <RequestPasswordResetPage />
      </MemoryRouter>
    )

    const emailInput = screen.getByLabelText(/email/i)
    const form = screen.getByRole('button', { name: /send reset link/i }).closest('form')!

    fireEvent.change(emailInput, { target: { value: 'test@example.com' } })
    fireEvent.submit(form)

    await waitFor(() => {
      expect(mockRequestPasswordReset).toHaveBeenCalledWith('test@example.com')
    })

    await waitFor(() => {
      expect(mockShow).toHaveBeenCalledWith('If the email exists, a reset link has been sent.', 'success')
    })
  })

  it('shows error toast when password reset request fails', async () => {
    mockRequestPasswordReset.mockResolvedValueOnce({ ok: false })

    render(
      <MemoryRouter>
        <RequestPasswordResetPage />
      </MemoryRouter>
    )

    const emailInput = screen.getByLabelText(/email/i)
    const form = screen.getByRole('button', { name: /send reset link/i }).closest('form')!

    fireEvent.change(emailInput, { target: { value: 'unknown@example.com' } })
    fireEvent.submit(form)

    await waitFor(() => {
      expect(mockRequestPasswordReset).toHaveBeenCalledWith('unknown@example.com')
    })

    await waitFor(() => {
      expect(mockShow).toHaveBeenCalledWith('Please enter a valid email.', 'error')
    })
  })

  it('disables button and changes text while submitting', async () => {
    let resolveRequest: (value: { ok: boolean }) => void
    const requestPromise = new Promise<{ ok: boolean }>((resolve) => {
      resolveRequest = resolve
    })
    mockRequestPasswordReset.mockReturnValueOnce(requestPromise)

    render(
      <MemoryRouter>
        <RequestPasswordResetPage />
      </MemoryRouter>
    )

    const emailInput = screen.getByLabelText(/email/i)
    const form = screen.getByRole('button', { name: /send reset link/i }).closest('form')!
    
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } })
    fireEvent.submit(form)

    // Check button is disabled and text changed during submission
    await waitFor(() => {
      const button = screen.getByRole('button', { name: /sending/i })
      expect(button).toBeDisabled()
      expect(button).toHaveTextContent('Sending…')
    })

    // Resolve the promise
    resolveRequest!({ ok: true })

    // Check button is re-enabled after submission
    await waitFor(() => {
      const button = screen.getByRole('button', { name: /send reset link/i })
      expect(button).toBeEnabled()
    })
  })

  it('renders back to login link pointing to /login', () => {
    render(
      <MemoryRouter>
        <RequestPasswordResetPage />
      </MemoryRouter>
    )

    const backLink = screen.getByRole('link', { name: /back to login/i })
    expect(backLink).toBeInTheDocument()
    expect(backLink).toHaveAttribute('href', '/login')
  })

  it('prevents default form submission', async () => {
    mockRequestPasswordReset.mockResolvedValueOnce({ ok: true })

    render(
      <MemoryRouter>
        <RequestPasswordResetPage />
      </MemoryRouter>
    )

    const form = screen.getByRole('button', { name: /send reset link/i }).closest('form')!
    const emailInput = screen.getByLabelText(/email/i)

    fireEvent.change(emailInput, { target: { value: 'test@example.com' } })
    fireEvent.submit(form)

    await waitFor(() => {
      expect(mockRequestPasswordReset).toHaveBeenCalled()
    })
  })

  it('input has no aria-describedby initially', () => {
    render(
      <MemoryRouter>
        <RequestPasswordResetPage />
      </MemoryRouter>
    )

    expect(screen.getByLabelText(/email/i)).not.toHaveAttribute('aria-describedby')
  })

  it('shows per-field error when email is empty', async () => {
    render(
      <MemoryRouter>
        <RequestPasswordResetPage />
      </MemoryRouter>
    )

    fireEvent.submit(screen.getByRole('button', { name: /send reset link/i }).closest('form')!)

    await waitFor(() => {
      expect(screen.getByText('Email is required.')).toBeInTheDocument()
    })
    expect(mockRequestPasswordReset).not.toHaveBeenCalled()
  })

  it('shows per-field error when email format is invalid', async () => {
    render(
      <MemoryRouter>
        <RequestPasswordResetPage />
      </MemoryRouter>
    )

    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'notanemail' } })
    fireEvent.submit(screen.getByRole('button', { name: /send reset link/i }).closest('form')!)

    await waitFor(() => {
      expect(screen.getByText('Please enter a valid email address.')).toBeInTheDocument()
    })
    expect(mockRequestPasswordReset).not.toHaveBeenCalled()
  })

  it('input links to per-field error via aria-describedby when validation fails', async () => {
    render(
      <MemoryRouter>
        <RequestPasswordResetPage />
      </MemoryRouter>
    )

    fireEvent.submit(screen.getByRole('button', { name: /send reset link/i }).closest('form')!)

    await waitFor(() => {
      expect(screen.getByLabelText(/email/i)).toHaveAttribute('aria-describedby', 'email-error')
    })
  })
})
