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
      }
      return map[key] ?? key
    },
  }),
}))

function mockMatchMedia(prefersDark: boolean) {
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    value: vi.fn().mockImplementation((query: string) => ({
      matches: prefersDark,
      media: query,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
    })),
  })
}

describe('NavBarThemeButton', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockTheme = 'system'
    mockMatchMedia(false) // default: system = light
  })

  // ── Resolved icon ─────────────────────────────────────────────────────────

  it('shows sun + "Switch to dark mode" when theme is light', () => {
    mockTheme = 'light'
    render(<NavBarThemeButton />)
    const btn = screen.getByRole('button', { name: 'Switch to dark mode' })
    expect(btn).toBeInTheDocument()
    expect(btn).toHaveAttribute('title', 'Switch to dark mode')
  })

  it('shows moon + "Switch to light mode" when theme is dark', () => {
    mockTheme = 'dark'
    render(<NavBarThemeButton />)
    const btn = screen.getByRole('button', { name: 'Switch to light mode' })
    expect(btn).toBeInTheDocument()
    expect(btn).toHaveAttribute('title', 'Switch to light mode')
  })

  it('resolves system to light when OS prefers light', () => {
    mockTheme = 'system'
    mockMatchMedia(false)
    render(<NavBarThemeButton />)
    expect(screen.getByRole('button', { name: 'Switch to dark mode' })).toBeInTheDocument()
  })

  it('resolves system to dark when OS prefers dark', () => {
    mockTheme = 'system'
    mockMatchMedia(true)
    render(<NavBarThemeButton />)
    expect(screen.getByRole('button', { name: 'Switch to light mode' })).toBeInTheDocument()
  })

  // ── Click behavior (only light ↔ dark, never system) ─────────────────────

  it('clicking when resolved=light sets theme to dark', async () => {
    mockTheme = 'light'
    const user = userEvent.setup()
    render(<NavBarThemeButton />)
    await user.click(screen.getByRole('button', { name: 'Switch to dark mode' }))
    expect(mockSetTheme).toHaveBeenCalledWith('dark')
  })

  it('clicking when resolved=dark sets theme to light', async () => {
    mockTheme = 'dark'
    const user = userEvent.setup()
    render(<NavBarThemeButton />)
    await user.click(screen.getByRole('button', { name: 'Switch to light mode' }))
    expect(mockSetTheme).toHaveBeenCalledWith('light')
  })

  it('clicking when system=light sets theme to dark (not system)', async () => {
    mockTheme = 'system'
    mockMatchMedia(false)
    const user = userEvent.setup()
    render(<NavBarThemeButton />)
    await user.click(screen.getByRole('button', { name: 'Switch to dark mode' }))
    expect(mockSetTheme).toHaveBeenCalledWith('dark')
    expect(mockSetTheme).not.toHaveBeenCalledWith('system')
  })

  it('clicking when system=dark sets theme to light (not system)', async () => {
    mockTheme = 'system'
    mockMatchMedia(true)
    const user = userEvent.setup()
    render(<NavBarThemeButton />)
    await user.click(screen.getByRole('button', { name: 'Switch to light mode' }))
    expect(mockSetTheme).toHaveBeenCalledWith('light')
    expect(mockSetTheme).not.toHaveBeenCalledWith('system')
  })

  // ── Accessibility ─────────────────────────────────────────────────────────

  it('aria-label matches title attribute', () => {
    mockTheme = 'light'
    render(<NavBarThemeButton />)
    const btn = screen.getByRole('button', { name: 'Switch to dark mode' })
    expect(btn.getAttribute('aria-label')).toBe(btn.getAttribute('title'))
  })

  it('SVG icon has aria-hidden=true', () => {
    mockTheme = 'light'
    render(<NavBarThemeButton />)
    const btn = screen.getByRole('button', { name: 'Switch to dark mode' })
    expect(btn.querySelector('svg')).toHaveAttribute('aria-hidden', 'true')
  })
})
