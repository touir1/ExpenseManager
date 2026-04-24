import { describe, it, expect, vi, beforeEach } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { render, screen } from '@testing-library/react'
import HomeDashboard from '@/pages/HomeDashboard'

const mockUseAuth = vi.fn()

vi.mock('@/features/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth()
}))

describe('HomeDashboard', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders dashboard heading', () => {
    mockUseAuth.mockReturnValue({ user: { email: 'test@test.com' } })

    render(
      <MemoryRouter>
        <HomeDashboard />
      </MemoryRouter>
    )

    expect(screen.getByRole('heading', { name: /dashboard/i })).toBeInTheDocument()
  })

  it('displays firstName when available', () => {
    mockUseAuth.mockReturnValue({ user: { email: 'john@example.com', firstName: 'John' } })

    render(
      <MemoryRouter>
        <HomeDashboard />
      </MemoryRouter>
    )

    expect(screen.getByText(/welcome john!/i)).toBeInTheDocument()
  })

  it('falls back to email when firstName is not available', () => {
    mockUseAuth.mockReturnValue({ user: { email: 'john@example.com' } })

    render(
      <MemoryRouter>
        <HomeDashboard />
      </MemoryRouter>
    )

    expect(screen.getByText(/welcome john@example.com/i)).toBeInTheDocument()
  })

  it('displays default user text when no email', () => {
    mockUseAuth.mockReturnValue({ user: null })

    render(
      <MemoryRouter>
        <HomeDashboard />
      </MemoryRouter>
    )

    expect(screen.getByText(/welcome user/i)).toBeInTheDocument()
  })

  it('renders change password link', () => {
    mockUseAuth.mockReturnValue({ user: { email: 'test@test.com' } })

    render(
      <MemoryRouter>
        <HomeDashboard />
      </MemoryRouter>
    )

    const changePasswordLink = screen.getByRole('link', { name: /change password/i })
    expect(changePasswordLink).toBeInTheDocument()
    expect(changePasswordLink).toHaveAttribute('href', '/change-password')
  })

})
