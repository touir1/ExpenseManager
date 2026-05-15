import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import LanguageSwitcher from '../LanguageSwitcher'

const mockChangeLanguage = vi.fn()
let mockResolvedLanguage: string | undefined = 'en'

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => {
      const translations: Record<string, string> = {
        'language.label': 'Select Language',
        'language.en': 'English',
        'language.fr': 'Français',
        'language.es': 'Español',
        'language.de': 'Deutsch',
      }
      return translations[key] || key
    },
    i18n: {
      language: 'en',
      get resolvedLanguage() { return mockResolvedLanguage },
      changeLanguage: mockChangeLanguage,
    },
  }),
}))

describe('LanguageSwitcher', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockResolvedLanguage = 'en'
  })

  it('renders a button with aria-label', () => {
    render(<LanguageSwitcher />)
    expect(screen.getByRole('button', { name: /select language/i })).toBeInTheDocument()
  })

  it('shows current language name in the button', () => {
    render(<LanguageSwitcher />)
    expect(screen.getByRole('button', { name: /select language/i })).toHaveTextContent('English')
  })

  it('does not show options before button is clicked', () => {
    render(<LanguageSwitcher />)
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
  })

  it('opens dropdown and shows all 4 options on click', async () => {
    const user = userEvent.setup()
    render(<LanguageSwitcher />)

    await user.click(screen.getByRole('button', { name: /select language/i }))

    expect(screen.getByRole('listbox')).toBeInTheDocument()
    const options = screen.getAllByRole('option')
    expect(options).toHaveLength(4)
    expect(options[0]).toHaveTextContent('English')
    expect(options[1]).toHaveTextContent('Français')
    expect(options[2]).toHaveTextContent('Español')
    expect(options[3]).toHaveTextContent('Deutsch')
  })

  it('button has aria-expanded=false when closed', () => {
    render(<LanguageSwitcher />)
    expect(screen.getByRole('button', { name: /select language/i })).toHaveAttribute('aria-expanded', 'false')
  })

  it('button has aria-expanded=true when open', async () => {
    const user = userEvent.setup()
    render(<LanguageSwitcher />)
    await user.click(screen.getByRole('button', { name: /select language/i }))
    expect(screen.getByRole('button', { name: /select language/i })).toHaveAttribute('aria-expanded', 'true')
  })

  it('calls changeLanguage with fr when French option clicked', async () => {
    const user = userEvent.setup()
    render(<LanguageSwitcher />)
    await user.click(screen.getByRole('button', { name: /select language/i }))
    await user.click(screen.getByText('Français'))
    expect(mockChangeLanguage).toHaveBeenCalledWith('fr')
  })

  it('calls changeLanguage with es when Spanish option clicked', async () => {
    const user = userEvent.setup()
    render(<LanguageSwitcher />)
    await user.click(screen.getByRole('button', { name: /select language/i }))
    await user.click(screen.getByText('Español'))
    expect(mockChangeLanguage).toHaveBeenCalledWith('es')
  })

  it('calls changeLanguage with de when German option clicked', async () => {
    const user = userEvent.setup()
    render(<LanguageSwitcher />)
    await user.click(screen.getByRole('button', { name: /select language/i }))
    await user.click(screen.getByText('Deutsch'))
    expect(mockChangeLanguage).toHaveBeenCalledWith('de')
  })

  it('closes dropdown after selecting a language', async () => {
    const user = userEvent.setup()
    render(<LanguageSwitcher />)
    await user.click(screen.getByRole('button', { name: /select language/i }))
    await user.click(screen.getByText('Français'))
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
  })

  it('closes dropdown on outside click', async () => {
    const user = userEvent.setup()
    render(<LanguageSwitcher />)
    await user.click(screen.getByRole('button', { name: /select language/i }))
    expect(screen.getByRole('listbox')).toBeInTheDocument()
    fireEvent.mouseDown(document.body)
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
  })

  it('marks current language option as selected', async () => {
    const user = userEvent.setup()
    render(<LanguageSwitcher />)
    await user.click(screen.getByRole('button', { name: /select language/i }))
    const options = screen.getAllByRole('option')
    expect(options[0]).toHaveAttribute('aria-selected', 'true')
    expect(options[1]).toHaveAttribute('aria-selected', 'false')
  })

  it('falls back to i18n.language when resolvedLanguage is undefined', () => {
    mockResolvedLanguage = undefined
    render(<LanguageSwitcher />)
    expect(screen.getByRole('button', { name: /select language/i })).toHaveTextContent('English')
  })

  it('dropdown opens downward by default (placement=down)', async () => {
    const user = userEvent.setup()
    render(<LanguageSwitcher />)
    await user.click(screen.getByRole('button', { name: /select language/i }))
    const listbox = screen.getByRole('listbox')
    expect(listbox.className).toContain('top-full')
  })

  it('dropdown opens upward when placement=up', async () => {
    const user = userEvent.setup()
    render(<LanguageSwitcher placement="up" />)
    await user.click(screen.getByRole('button', { name: /select language/i }))
    const listbox = screen.getByRole('listbox')
    expect(listbox.className).toContain('bottom-full')
  })
})
