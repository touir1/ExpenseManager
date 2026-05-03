import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import LanguageSwitcher from '../LanguageSwitcher'

// Mock useTranslation
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
  })

  it('should render select dropdown', () => {
    render(<LanguageSwitcher />)
    const select = screen.getByRole('combobox', { name: /select language/i })
    expect(select).toBeInTheDocument()
  })

  it('should display all language options', () => {
    render(<LanguageSwitcher />)
    const options = screen.getAllByRole('option')
    expect(options).toHaveLength(4)
    expect(options[0]).toHaveTextContent('English')
    expect(options[1]).toHaveTextContent('Français')
    expect(options[2]).toHaveTextContent('Español')
    expect(options[3]).toHaveTextContent('Deutsch')
  })

  it('should have correct language codes as option values', () => {
    render(<LanguageSwitcher />)
    const options = screen.getAllByRole('option')
    expect((options[0] as HTMLOptionElement).value).toBe('en')
    expect((options[1] as HTMLOptionElement).value).toBe('fr')
    expect((options[2] as HTMLOptionElement).value).toBe('es')
    expect((options[3] as HTMLOptionElement).value).toBe('de')
  })

  it('should have language.label as aria-label', () => {
    render(<LanguageSwitcher />)
    const select = screen.getByRole('combobox', { name: /select language/i })
    expect(select).toHaveAttribute('aria-label', 'Select Language')
  })

  it('should call changeLanguage when selecting different language', async () => {
    const user = userEvent.setup()
    render(<LanguageSwitcher />)

    const select = screen.getByRole('combobox') as HTMLSelectElement
    await user.selectOptions(select, 'fr')

    expect(mockChangeLanguage).toHaveBeenCalledWith('fr')
  })

  it('should call changeLanguage when changing to Spanish', async () => {
    const user = userEvent.setup()
    render(<LanguageSwitcher />)

    const select = screen.getByRole('combobox') as HTMLSelectElement
    await user.selectOptions(select, 'es')

    expect(mockChangeLanguage).toHaveBeenCalledWith('es')
  })

  it('should call changeLanguage when changing to German', async () => {
    const user = userEvent.setup()
    render(<LanguageSwitcher />)

    const select = screen.getByRole('combobox') as HTMLSelectElement
    await user.selectOptions(select, 'de')

    expect(mockChangeLanguage).toHaveBeenCalledWith('de')
  })

  it('should have correct styling classes', () => {
    render(<LanguageSwitcher />)
    const select = screen.getByRole('combobox')
    expect(select).toHaveClass(
      'text-sm',
      'text-slate-600',
      'bg-transparent',
      'border',
      'border-slate-200',
      'rounded-lg',
      'px-2',
      'py-1',
      'cursor-pointer'
    )
  })

  it('should have hover and focus styles', () => {
    render(<LanguageSwitcher />)
    const select = screen.getByRole('combobox')
    expect(select).toHaveClass(
      'hover:border-slate-300',
      'focus:outline-none',
      'focus:ring-2',
      'focus:ring-brand-500'
    )
  })

  it('falls back to i18n.language when resolvedLanguage is undefined', () => {
    mockResolvedLanguage = undefined
    render(<LanguageSwitcher />)
    const select = screen.getByRole('combobox') as HTMLSelectElement
    expect(select.value).toBe('en')
    mockResolvedLanguage = 'en'
  })
})
