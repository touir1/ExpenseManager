import { describe, it, expect } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { render, screen } from '@testing-library/react'
import HomePublic from '@/pages/HomePublic'

describe('HomePublic', () => {
  it('renders welcome message', () => {
    render(
      <MemoryRouter>
        <HomePublic />
      </MemoryRouter>
    )

    expect(screen.getByRole('heading')).toBeInTheDocument()
    expect(screen.getByText(/track your expenses/i)).toBeInTheDocument()
  })

  it('renders login link', () => {
    render(
      <MemoryRouter>
        <HomePublic />
      </MemoryRouter>
    )

    const loginLink = screen.getByRole('link', { name: /sign in/i })
    expect(loginLink).toBeInTheDocument()
    expect(loginLink).toHaveAttribute('href', '/login')
  })

  it('renders register link', () => {
    render(
      <MemoryRouter>
        <HomePublic />
      </MemoryRouter>
    )

    const registerLink = screen.getByRole('link', { name: /create account/i })
    expect(registerLink).toBeInTheDocument()
    expect(registerLink).toHaveAttribute('href', '/register')
  })
})
