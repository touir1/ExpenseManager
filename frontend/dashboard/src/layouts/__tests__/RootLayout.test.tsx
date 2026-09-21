import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import RootLayout from '../RootLayout'

const mockUseAuth = vi.fn()

vi.mock('@/features/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth(),
}))

vi.mock('@/providers/AppProviders', () => ({
  AppProviders: ({ children }: { children: React.ReactNode }) => <div data-testid="app-providers">{children}</div>,
}))

vi.mock('@/layouts/MarketingHeader', () => ({
  default: () => <header data-testid="marketing-header">MarketingHeader</header>,
}))

vi.mock('@/layouts/SideNav', () => ({
  default: () => <nav data-testid="sidenav">SideNav</nav>,
}))

vi.mock('@/layouts/AppHeader', () => ({
  default: () => <header data-testid="appheader">AppHeader</header>,
}))

vi.mock('@/services/api.service', () => ({
  onError: vi.fn(),
}))

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom')
  return {
    ...actual,
    Outlet: () => <div data-testid="outlet">Outlet</div>,
  }
})

const renderInRouter = () =>
  render(
    <MemoryRouter>
      <RootLayout />
    </MemoryRouter>
  )

describe('RootLayout', () => {
  it('renders MarketingHeader when unauthenticated', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: false })
    renderInRouter()
    expect(screen.getByTestId('marketing-header')).toBeInTheDocument()
    expect(screen.queryByTestId('sidenav')).not.toBeInTheDocument()
  })

  it('renders SideNav and AppHeader when authenticated', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true })
    renderInRouter()
    expect(screen.getByTestId('sidenav')).toBeInTheDocument()
    expect(screen.getByTestId('appheader')).toBeInTheDocument()
  })

  it('renders AppProviders', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: false })
    renderInRouter()
    expect(screen.getByTestId('app-providers')).toBeInTheDocument()
  })

  it('renders Outlet inside main', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true })
    renderInRouter()
    const outlet = screen.getByTestId('outlet')
    expect(outlet).toBeInTheDocument()
    const main = outlet.closest('main')
    expect(main).toBeInTheDocument()
  })

  it('main has flex layout classes', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true })
    renderInRouter()
    const main = screen.getByTestId('outlet').closest('main')
    expect(main).toHaveClass('flex-1', 'flex', 'flex-col')
  })
})
