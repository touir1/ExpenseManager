import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useTheme } from '@/features/settings/ThemeContext'

function getSystemDark(): boolean {
  if (typeof window === 'undefined' || !window.matchMedia) return false
  return window.matchMedia('(prefers-color-scheme: dark)').matches
}

function useResolvedTheme(): 'light' | 'dark' {
  const { theme } = useTheme()
  const [systemDark, setSystemDark] = useState(getSystemDark)

  useEffect(() => {
    if (theme !== 'system') return
    const mq = window.matchMedia('(prefers-color-scheme: dark)')
    const handler = (e: MediaQueryListEvent) => setSystemDark(e.matches)
    mq.addEventListener('change', handler)
    return () => mq.removeEventListener('change', handler)
  }, [theme])

  if (theme === 'light') return 'light'
  if (theme === 'dark') return 'dark'
  return systemDark ? 'dark' : 'light'
}

export default function NavBarThemeButton() {
  const { setTheme } = useTheme()
  const { t } = useTranslation()
  const resolved = useResolvedTheme()

  const nextTheme = resolved === 'light' ? 'dark' : 'light'
  const label = resolved === 'light' ? t('nav.toggleThemeDark') : t('nav.toggleThemeLight')

  return (
    <button
      onClick={() => setTheme(nextTheme)}
      aria-label={label}
      title={label}
      className="h-8 w-8 rounded-lg text-ink-mute hover:text-ink hover:bg-surface-subtle transition-colors duration-150 flex items-center justify-center cursor-pointer"
    >
      {resolved === 'light' ? (
        <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
          <path strokeLinecap="round" strokeLinejoin="round" d="M12 3v1m0 16v1m8.66-9h-1M4.34 12h-1m15.07-6.07-.71.71M6.34 17.66l-.71.71m12.73 0-.71-.71M6.34 6.34l-.71-.71M12 7a5 5 0 100 10A5 5 0 0012 7z" />
        </svg>
      ) : (
        <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
          <path strokeLinecap="round" strokeLinejoin="round" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
        </svg>
      )}
    </button>
  )
}
