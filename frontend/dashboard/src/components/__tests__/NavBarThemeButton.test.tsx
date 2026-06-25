import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import NavBarThemeButton from '@/components/NavBarThemeButton'
import type { Theme } from '@/features/settings/ThemeContext'

let mockTheme: Theme = 'system'
const mockSetTheme = vi.fn()

vi.mock('@/features/settings/ThemeContext', () => ({
  useTheme: () => ({ theme: mockTheme, setTheme: mockSetTheme }),
}))

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => {
      const map: Record<string, string> = {
        'nav.toggleThemeLight': 'Switch to light mode',
        'nav.toggleThemeDark': 'Switch to dark mode',
        'nav.toggleThemeSystem': 'Back to system default',
      }
      return map[key] ?? key
    },
  }),
}))

describe('NavBarThemeButton', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockTheme = 'system'
  })

  // ── Icon rendering ────────────────────────────────────────────────────────

  it('renders monitor icon and "Switch to light mode" label when theme is system', () => {
    mockTheme = 'system'
    render(<NavBarThemeButton />)
    const btn = screen.getByRole('button', { name: 'Switch to light mode' })
    expect(btn).toBeInTheDocument()
    expect(btn).toHaveAttribute('title', 'Switch to light mode')
    expect(btn).toHaveAttribute('aria-label', 'Switch to light mode')
  })

  it('renders sun icon and "Switch to dark mode" label when theme is light', () => {
    mockTheme = 'light'
    render(<NavBarThemeButton />)
    const btn = screen.getByRole('button', { name: 'Switch to dark mode' })
    expect(btn).toBeInTheDocument()
    expect(btn).toHaveAttribute('title', 'Switch to dark mode')
    expect(btn).toHaveAttribute('aria-label', 'Switch to dark mode')
  })

  it('renders moon icon and "Back to system default" label when theme is dark', () => {
    mockTheme = 'dark'
    render(<NavBarThemeButton />)
    const btn = screen.getByRole('button', { name: 'Back to system default' })
    expect(btn).toBeInTheDocument()
    expect(btn).toHaveAttribute('title', 'Back to system default')
    expect(btn).toHaveAttribute('aria-label', 'Back to system default')
  })

  // ── Cycle behavior ────────────────────────────────────────────────────────

  it('calls setTheme("light") when clicked in system state', async () => {
    mockTheme = 'system'
    const user = userEvent.setup()
    render(<NavBarThemeButton />)
    await user.click(screen.getByRole('button', { name: 'Switch to light mode' }))
    expect(mockSetTheme).toHaveBeenCalledWith('light')
  })

  it('calls setTheme("dark") when clicked in light state', async () => {
    mockTheme = 'light'
    const user = userEvent.setup()
    render(<NavBarThemeButton />)
    await user.click(screen.getByRole('button', { name: 'Switch to dark mode' }))
    expect(mockSetTheme).toHaveBeenCalledWith('dark')
  })

  it('calls setTheme("system") when clicked in dark state', async () => {
    mockTheme = 'dark'
    const user = userEvent.setup()
    render(<NavBarThemeButton />)
    await user.click(screen.getByRole('button', { name: 'Back to system default' }))
    expect(mockSetTheme).toHaveBeenCalledWith('system')
  })

  // ── Accessibility ─────────────────────────────────────────────────────────

  it('aria-label matches title attribute', () => {
    mockTheme = 'light'
    render(<NavBarThemeButton />)
    const btn = screen.getByRole('button', { name: 'Switch to dark mode' })
    expect(btn.getAttribute('aria-label')).toBe(btn.getAttribute('title'))
  })

  it('SVG icon has aria-hidden=true', () => {
    mockTheme = 'system'
    render(<NavBarThemeButton />)
    const btn = screen.getByRole('button', { name: 'Switch to light mode' })
    const svg = btn.querySelector('svg')
    expect(svg).toHaveAttribute('aria-hidden', 'true')
  })
})
