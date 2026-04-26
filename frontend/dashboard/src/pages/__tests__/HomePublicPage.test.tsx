import { describe, it, expect } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { render, screen } from '@testing-library/react'
import HomePublic from '@/pages/HomePublicPage'
import HomePublicPage from '@/pages/HomePublicPage'

describe('HomePublicPage', () => {
  it('renders welcome message', () => {
    render(
      <MemoryRouter>
        <HomePublicPage />
      </MemoryRouter>
    )

    expect(screen.getByRole('heading')).toBeInTheDocument()
    expect(screen.getByText(/track your expenses/i)).toBeInTheDocument()
  })

  it('renders login link', () => {
    render(
      <MemoryRouter>
        <HomePublicPage />
      </MemoryRouter>
    )

    const loginLink = screen.getByRole('link', { name: /sign in/i })
    expect(loginLink).toBeInTheDocument()
    expect(loginLink).toHaveAttribute('href', '/login')
  })

  it('renders register link', () => {
    render(
      <MemoryRouter>
        <HomePublicPage />
      </MemoryRouter>
    )

    const registerLink = screen.getByRole('link', { name: /create account/i })
    expect(registerLink).toBeInTheDocument()
    expect(registerLink).toHaveAttribute('href', '/register')
  })

  it('outer container has centering classes for vertical layout', () => {
    const { container } = render(
      <MemoryRouter>
        <HomePublicPage />
      </MemoryRouter>
    )
    expect(container.firstChild).toHaveClass('auth-page')
  })
})
