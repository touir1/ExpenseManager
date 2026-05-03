import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import VerifyErrorPage from '../VerifyErrorPage'

// Mock usePageTitle
vi.mock('@/hooks/usePageTitle', () => ({
  usePageTitle: vi.fn(),
}))

// Mock useTranslation
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
}))

describe('VerifyErrorPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
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
})
