import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import BackLink from '../BackLink'

function renderBackLink(to = '/home', children = 'Back') {
  return render(
    <MemoryRouter>
      <BackLink to={to}>{children}</BackLink>
    </MemoryRouter>
  )
}

describe('BackLink', () => {
  it('renders children text', () => {
    renderBackLink('/home', 'Go back')
    expect(screen.getByText('Go back')).toBeInTheDocument()
  })

  it('renders as a link', () => {
    renderBackLink('/home', 'Back')
    expect(screen.getByRole('link', { name: /back/i })).toBeInTheDocument()
  })

  it('points to the provided to path', () => {
    renderBackLink('/login', 'Back')
    expect(screen.getByRole('link')).toHaveAttribute('href', '/login')
  })

  it('renders a chevron icon (svg)', () => {
    const { container } = renderBackLink()
    expect(container.querySelector('svg')).toBeInTheDocument()
  })
})
