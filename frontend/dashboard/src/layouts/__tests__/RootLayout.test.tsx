import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import RootLayout from '../RootLayout'

vi.mock('@/providers/AppProviders', () => ({
  AppProviders: ({ children }: { children: React.ReactNode }) => <div data-testid="app-providers">{children}</div>,
}))

vi.mock('@/layouts/NavBar', () => ({
  default: () => <nav data-testid="navbar">NavBar</nav>,
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
  it('renders NavBar', () => {
    renderInRouter()
    expect(screen.getByTestId('navbar')).toBeInTheDocument()
  })

  it('renders AppProviders', () => {
    renderInRouter()
    expect(screen.getByTestId('app-providers')).toBeInTheDocument()
  })

  it('renders Outlet inside main', () => {
    renderInRouter()
    const outlet = screen.getByTestId('outlet')
    expect(outlet).toBeInTheDocument()
    const main = outlet.closest('main')
    expect(main).toBeInTheDocument()
  })

  it('main has flex layout classes', () => {
    renderInRouter()
    const main = screen.getByTestId('outlet').closest('main')
    expect(main).toHaveClass('flex-1', 'flex', 'flex-col')
  })
})
