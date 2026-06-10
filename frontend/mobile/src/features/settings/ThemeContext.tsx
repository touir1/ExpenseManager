import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'

export type Theme = 'light' | 'dark' | 'system'

const STORAGE_KEY = 'em-theme'

function applyTheme(theme: Theme) {
  const root = document.documentElement
  if (theme === 'dark') {
    root.classList.add('dark')
    root.classList.remove('light')
  } else if (theme === 'light') {
    root.classList.add('light')
    root.classList.remove('dark')
  } else {
    root.classList.remove('dark', 'light')
  }
}

interface ThemeContextValue {
  theme: Theme
  setTheme: (t: Theme) => void
}

const ThemeContext = createContext<ThemeContextValue | null>(null)

export function ThemeProvider({ children }: Readonly<{ children: ReactNode }>) {
  const [theme, setThemeState] = useState<Theme>('system')

  useEffect(() => {
    async function loadTheme() {
      let stored: Theme | null = null
      try {
        const { Preferences } = await import('@capacitor/preferences')
        const { value } = await Preferences.get({ key: STORAGE_KEY })
        stored = (value as Theme) ?? null
      } catch {
        stored = (localStorage.getItem(STORAGE_KEY) as Theme) ?? null
      }
      const t = stored ?? 'system'
      setThemeState(t)
      applyTheme(t)
    }
    loadTheme()
  }, [])

  async function setTheme(t: Theme) {
    try {
      const { Preferences } = await import('@capacitor/preferences')
      await Preferences.set({ key: STORAGE_KEY, value: t })
    } catch {
      localStorage.setItem(STORAGE_KEY, t)
    }
    setThemeState(t)
    applyTheme(t)
  }

  return <ThemeContext.Provider value={{ theme, setTheme }}>{children}</ThemeContext.Provider>
}

export function useTheme() {
  const ctx = useContext(ThemeContext)
  if (!ctx) throw new Error('useTheme must be used inside ThemeProvider')
  return ctx
}
