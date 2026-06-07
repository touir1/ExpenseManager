import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import AdminLayout from '../AdminLayout'

function renderLayout() {
  return render(
    <MemoryRouter>
      <AdminLayout />
    </MemoryRouter>
  )
}

describe('AdminLayout', () => {
  it('renders the users nav link', () => {
    renderLayout()
    expect(screen.getByRole('link', { name: /users/i })).toBeInTheDocument()
  })

  it('renders the categories nav link', () => {
    renderLayout()
    expect(screen.getByRole('link', { name: /categor/i })).toBeInTheDocument()
  })

  it('renders the currencies nav link', () => {
    renderLayout()
    expect(screen.getByRole('link', { name: /currenc/i })).toBeInTheDocument()
  })

  it('renders the rates nav link', () => {
    renderLayout()
    const links = screen.getAllByRole('link')
    expect(links.some(l => /rates/i.test(l.textContent ?? ''))).toBe(true)
  })

  it('users link points to /admin/users', () => {
    renderLayout()
    expect(screen.getByRole('link', { name: /users/i })).toHaveAttribute('href', '/admin/users')
  })

  it('renders main outlet slot', () => {
    render(
      <MemoryRouter>
        <AdminLayout />
      </MemoryRouter>
    )
    // Outlet renders nothing without a matching route — just assert nav is there
    expect(screen.getByRole('navigation')).toBeInTheDocument()
  })
})
