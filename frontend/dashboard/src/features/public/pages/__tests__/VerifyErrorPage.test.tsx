import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { BrowserRouter, MemoryRouter } from 'react-router-dom'
import VerifyErrorPage from '../VerifyErrorPage'

vi.mock('@/hooks/usePageTitle', () => ({
  usePageTitle: vi.fn(),
}))

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
}))

const mockResend = vi.fn()
vi.mock('@/features/auth/services/authApi.service', () => ({
  resendVerificationRequest: (...args: unknown[]) => mockResend(...args),
}))

describe('VerifyErrorPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockResend.mockResolvedValue({ ok: true })
  })

  it('should render verify error page with error icon', () => {
    render(
      <BrowserRouter>
        <VerifyErrorPage />
      </BrowserRouter>
    )

    const svgIcon = document.querySelector('svg[aria-hidden="true"]')
    expect(svgIcon).toBeInTheDocument()
  })

  it('should render title with translation key', () => {
    render(
      <BrowserRouter>
        <VerifyErrorPage />
      </BrowserRouter>
    )

    const title = screen.getByRole('heading', { level: 1 })
    expect(title).toHaveTextContent('public.verifyError.title')
  })

  it('should render description paragraph with translation key', () => {
    render(
      <BrowserRouter>
        <VerifyErrorPage />
      </BrowserRouter>
    )

    const description = screen.getByText('public.verifyError.description')
    expect(description).toBeInTheDocument()
  })

  it('should render link to register page', () => {
    render(
      <BrowserRouter>
        <VerifyErrorPage />
      </BrowserRouter>
    )

    const link = screen.getByRole('link', { name: /public.verifyError.backToRegister/i })
    expect(link).toHaveAttribute('href', '/register')
  })

  it('should have correct styling classes', () => {
    render(
      <BrowserRouter>
        <VerifyErrorPage />
      </BrowserRouter>
    )

    const container = screen.getByRole('heading').closest('.text-center')
    expect(container).toHaveClass('text-center', 'max-w-lg', 'px-4')
  })

  it('should have auth-page class on main container', () => {
    const { container } = render(
      <BrowserRouter>
        <VerifyErrorPage />
      </BrowserRouter>
    )

    const authPage = container.querySelector('.auth-page')
    expect(authPage).toBeInTheDocument()
  })

  it('should not show resend button when email param is absent', () => {
    render(
      <BrowserRouter>
        <VerifyErrorPage />
      </BrowserRouter>
    )

    expect(screen.queryByText('public.verifyError.resend')).not.toBeInTheDocument()
  })

  it('should show resend button when email param is present', () => {
    render(
      <MemoryRouter initialEntries={['/?email=test%40example.com&app_code=APP1']}>
        <VerifyErrorPage />
      </MemoryRouter>
    )

    expect(screen.getByText('public.verifyError.resend')).toBeInTheDocument()
  })

  it('should call resendVerificationRequest and show success message on resend click', async () => {
    render(
      <MemoryRouter initialEntries={['/?email=test%40example.com&app_code=APP1']}>
        <VerifyErrorPage />
      </MemoryRouter>
    )

    fireEvent.click(screen.getByText('public.verifyError.resend'))

    await waitFor(() => {
      expect(mockResend).toHaveBeenCalledWith('test@example.com', 'APP1')
      expect(screen.getByText('public.verifyError.resendSent')).toBeInTheDocument()
    })
  })

  it('should hide resend button and show success after resend', async () => {
    render(
      <MemoryRouter initialEntries={['/?email=test%40example.com&app_code=APP1']}>
        <VerifyErrorPage />
      </MemoryRouter>
    )

    fireEvent.click(screen.getByText('public.verifyError.resend'))

    await waitFor(() => {
      expect(screen.queryByText('public.verifyError.resend')).not.toBeInTheDocument()
      expect(screen.getByRole('alert')).toHaveTextContent('public.verifyError.resendSent')
    })
  })
})
