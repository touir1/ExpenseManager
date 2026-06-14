import { useTranslation } from 'react-i18next'
import { useTheme, type Theme } from '@/features/settings/ThemeContext'

const options: { value: Theme; labelKey: string; icon: React.ReactNode }[] = [
  {
    value: 'light',
    labelKey: 'settings.theme.light',
    icon: (
      <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
        <path strokeLinecap="round" strokeLinejoin="round" d="M12 3v1m0 16v1m8.66-9h-1M4.34 12h-1m15.07-6.07-.71.71M6.34 17.66l-.71.71m12.73 0-.71-.71M6.34 6.34l-.71-.71M12 7a5 5 0 100 10A5 5 0 0012 7z" />
      </svg>
    ),
  },
  {
    value: 'system',
    labelKey: 'settings.theme.system',
    icon: (
      <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
        <path strokeLinecap="round" strokeLinejoin="round" d="M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
      </svg>
    ),
  },
  {
    value: 'dark',
    labelKey: 'settings.theme.dark',
    icon: (
      <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
        <path strokeLinecap="round" strokeLinejoin="round" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
      </svg>
    ),
  },
]

export default function ThemeToggle({ showLabel = true }: { showLabel?: boolean }) {
  const { theme, setTheme } = useTheme()
  const { t } = useTranslation()

  return (
    <div className="flex items-center rounded-lg border border-surface-border overflow-hidden bg-surface-subtle" role="group" aria-label={t('settings.theme.label')}>
      {options.map(opt => (
        <button
          key={opt.value}
          onClick={() => setTheme(opt.value)}
          aria-pressed={theme === opt.value}
          aria-label={t(opt.labelKey)}
          title={t(opt.labelKey)}
          className={`flex items-center justify-center gap-1.5 px-2.5 py-1.5 text-xs font-medium transition-colors duration-150 cursor-pointer
            ${theme === opt.value
              ? 'bg-brand-500 text-white'
              : 'text-ink-mute hover:text-ink hover:bg-surface-border'
            }`}
        >
          {opt.icon}
          {showLabel && <span className="hidden sm:inline">{t(opt.labelKey)}</span>}
        </button>
      ))}
    </div>
  )
}
