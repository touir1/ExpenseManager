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

    expect(screen.getByRole('heading', { name: /welcome/i })).toBeInTheDocument()
    expect(screen.getByText(/this is the public home page/i)).toBeInTheDocument()
  })

  it('renders login link', () => {
    render(
      <MemoryRouter>
        <HomePublic />
      </MemoryRouter>
    )

    const loginLink = screen.getByRole('link', { name: /login/i })
    expect(loginLink).toBeInTheDocument()
    expect(loginLink).toHaveAttribute('href', '/login')
  })

  it('renders register link', () => {
    render(
      <MemoryRouter>
        <HomePublic />
      </MemoryRouter>
    )

    const registerLink = screen.getByRole('link', { name: /register/i })
    expect(registerLink).toBeInTheDocument()
    expect(registerLink).toHaveAttribute('href', '/register')
  })
})
