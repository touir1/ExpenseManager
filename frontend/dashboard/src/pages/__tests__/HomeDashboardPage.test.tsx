import { describe, it, expect, vi, beforeEach } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { render, screen } from '@testing-library/react'
import HomeDashboardPage from '@/pages/HomeDashboardPage'

const mockUseAuth = vi.fn()

vi.mock('@/features/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth()
}))

describe('HomeDashboardPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders dashboard heading', () => {
    mockUseAuth.mockReturnValue({ user: { email: 'test@test.com' } })

    render(
      <MemoryRouter>
        <HomeDashboardPage />
      </MemoryRouter>
    )

    expect(screen.getByRole('heading', { name: /dashboard/i })).toBeInTheDocument()
  })

  it('displays firstName when available', () => {
    mockUseAuth.mockReturnValue({ user: { email: 'john@example.com', firstName: 'John' } })

    render(
      <MemoryRouter>
        <HomeDashboardPage />
      </MemoryRouter>
    )

    expect(screen.getByText(/welcome john!/i)).toBeInTheDocument()
  })

  it('falls back to email when firstName is not available', () => {
    mockUseAuth.mockReturnValue({ user: { email: 'john@example.com' } })

    render(
      <MemoryRouter>
        <HomeDashboardPage />
      </MemoryRouter>
    )

    expect(screen.getByText(/welcome john@example.com/i)).toBeInTheDocument()
  })

  it('displays default user text when no email', () => {
    mockUseAuth.mockReturnValue({ user: null })

    render(
      <MemoryRouter>
        <HomeDashboardPage />
      </MemoryRouter>
    )

    expect(screen.getByText(/welcome user/i)).toBeInTheDocument()
  })

  it('renders change password link', () => {
    mockUseAuth.mockReturnValue({ user: { email: 'test@test.com' } })

    render(
      <MemoryRouter>
        <HomeDashboardPage />
      </MemoryRouter>
    )

    const changePasswordLink = screen.getByRole('link', { name: /change password/i })
    expect(changePasswordLink).toBeInTheDocument()
    expect(changePasswordLink).toHaveAttribute('href', '/change-password')
  })

  it('shows loading skeleton when session is restoring', () => {
    mockUseAuth.mockReturnValue({ user: null, isLoading: true })

    render(
      <MemoryRouter>
        <HomeDashboardPage />
      </MemoryRouter>
    )

    expect(screen.getByLabelText(/loading dashboard/i)).toBeInTheDocument()
    expect(screen.queryByRole('heading', { name: /dashboard/i })).not.toBeInTheDocument()
  })

  it('does not show skeleton when loaded', () => {
    mockUseAuth.mockReturnValue({ user: { email: 'test@test.com' }, isLoading: false })

    render(
      <MemoryRouter>
        <HomeDashboardPage />
      </MemoryRouter>
    )

    expect(screen.queryByLabelText(/loading dashboard/i)).not.toBeInTheDocument()
    expect(screen.getByRole('heading', { name: /dashboard/i })).toBeInTheDocument()
  })

})
