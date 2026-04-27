import { describe, it, expect } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { render, screen } from '@testing-library/react'
import NotFoundPage from '@/features/public/pages/NotFoundPage'

describe('NotFoundPage', () => {
  it('renders 404 heading', () => {
    render(
      <MemoryRouter>
        <NotFoundPage />
      </MemoryRouter>
    )

    expect(screen.getByRole('heading', { name: /page not found/i })).toBeInTheDocument()
  })

  it('renders descriptive message', () => {
    render(
      <MemoryRouter>
        <NotFoundPage />
      </MemoryRouter>
    )

    expect(screen.getByText(/doesn't exist or has been moved/i)).toBeInTheDocument()
  })

  it('renders a link back to home', () => {
    render(
      <MemoryRouter>
        <NotFoundPage />
      </MemoryRouter>
    )

    const homeLink = screen.getByRole('link', { name: /go to home/i })
    expect(homeLink).toBeInTheDocument()
    expect(homeLink).toHaveAttribute('href', '/')
  })
})
