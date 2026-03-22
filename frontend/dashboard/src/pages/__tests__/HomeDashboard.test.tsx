import { describe, it, expect, vi, beforeEach } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import HomeDashboard from '@/pages/HomeDashboard'

const mockLogout = vi.fn()
const mockUseAuth = vi.fn()

vi.mock('@/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth()
}))

describe('HomeDashboard', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders dashboard heading', () => {
    mockUseAuth.mockReturnValue({ user: { email: 'test@test.com' }, logout: mockLogout })

    render(
      <MemoryRouter>
        <HomeDashboard />
      </MemoryRouter>
    )

    expect(screen.getByRole('heading', { name: /dashboard/i })).toBeInTheDocument()
  })

  it('displays firstName when available', () => {
    mockUseAuth.mockReturnValue({ user: { email: 'john@example.com', firstName: 'John' }, logout: mockLogout })

    render(
      <MemoryRouter>
        <HomeDashboard />
      </MemoryRouter>
    )

    expect(screen.getByText(/welcome john!/i)).toBeInTheDocument()
  })

  it('falls back to email when firstName is not available', () => {
    mockUseAuth.mockReturnValue({ user: { email: 'john@example.com' }, logout: mockLogout })

    render(
      <MemoryRouter>
        <HomeDashboard />
      </MemoryRouter>
    )

    expect(screen.getByText(/welcome john@example.com/i)).toBeInTheDocument()
  })

  it('displays default user text when no email', () => {
    mockUseAuth.mockReturnValue({ user: null, logout: mockLogout })

    render(
      <MemoryRouter>
        <HomeDashboard />
      </MemoryRouter>
    )

    expect(screen.getByText(/welcome user/i)).toBeInTheDocument()
  })

  it('renders change password link', () => {
    mockUseAuth.mockReturnValue({ user: { email: 'test@test.com' }, logout: mockLogout })

    render(
      <MemoryRouter>
        <HomeDashboard />
      </MemoryRouter>
    )

    const changePasswordLink = screen.getByRole('link', { name: /change password/i })
    expect(changePasswordLink).toBeInTheDocument()
    expect(changePasswordLink).toHaveAttribute('href', '/change-password')
  })

  it('renders logout button', () => {
    mockUseAuth.mockReturnValue({ user: { email: 'test@test.com' }, logout: mockLogout })

    render(
      <MemoryRouter>
        <HomeDashboard />
      </MemoryRouter>
    )

    expect(screen.getByRole('button', { name: /logout/i })).toBeInTheDocument()
  })

  it('calls logout when logout button is clicked', async () => {
    const user = userEvent.setup()
    mockUseAuth.mockReturnValue({ user: { email: 'test@test.com' }, logout: mockLogout })

    render(
      <MemoryRouter>
        <HomeDashboard />
      </MemoryRouter>
    )

    await user.click(screen.getByRole('button', { name: /logout/i }))

    expect(mockLogout).toHaveBeenCalledTimes(1)
  })
})
