import { useState, useRef, useEffect } from 'react'
import { useTranslation } from 'react-i18next'

const FlagEN = () => (
  <svg width="20" height="14" viewBox="0 0 60 42" className="rounded-sm shrink-0" aria-hidden="true">
    <rect width="60" height="42" fill="#012169" />
    <path d="M0,0 L60,42 M60,0 L0,42" stroke="white" strokeWidth="9" />
    <path d="M0,0 L60,42 M60,0 L0,42" stroke="#C8102E" strokeWidth="6" />
    <rect x="24" y="0" width="12" height="42" fill="white" />
    <rect x="0" y="15" width="60" height="12" fill="white" />
    <rect x="27" y="0" width="6" height="42" fill="#C8102E" />
    <rect x="0" y="18" width="60" height="6" fill="#C8102E" />
  </svg>
)

const FlagFR = () => (
  <svg width="20" height="14" viewBox="0 0 60 42" className="rounded-sm shrink-0" aria-hidden="true">
    <rect width="20" height="42" fill="#002395" />
    <rect x="20" width="20" height="42" fill="white" />
    <rect x="40" width="20" height="42" fill="#ED2939" />
  </svg>
)

const FlagES = () => (
  <svg width="20" height="14" viewBox="0 0 60 42" className="rounded-sm shrink-0" aria-hidden="true">
    <rect width="60" height="42" fill="#AA151B" />
    <rect y="11" width="60" height="20" fill="#F1BF00" />
  </svg>
)

const FlagDE = () => (
  <svg width="20" height="14" viewBox="0 0 60 42" className="rounded-sm shrink-0" aria-hidden="true">
    <rect width="60" height="14" fill="#000000" />
    <rect y="14" width="60" height="14" fill="#DD0000" />
    <rect y="28" width="60" height="14" fill="#FFCE00" />
  </svg>
)

const LANGUAGES = [
  { code: 'en', key: 'language.en', Flag: FlagEN },
  { code: 'fr', key: 'language.fr', Flag: FlagFR },
  { code: 'es', key: 'language.es', Flag: FlagES },
  { code: 'de', key: 'language.de', Flag: FlagDE },
] as const

interface Props {
  readonly placement?: 'down' | 'up'
}

export default function LanguageSwitcher({ placement = 'down' }: Props) {
  const { t, i18n } = useTranslation()
  const [open, setOpen] = useState(false)
  const ref = useRef<HTMLDivElement>(null)

  const currentCode = i18n.resolvedLanguage ?? i18n.language
  const current = LANGUAGES.find(l => l.code === currentCode) ?? LANGUAGES[0]

  useEffect(() => {
    if (!open) return
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false)
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [open])

  const dropdownClass = placement === 'up'
    ? 'absolute right-0 bottom-full mb-1'
    : 'absolute right-0 top-full mt-1'

  return (
    <div ref={ref} className="relative">
      <button
        onClick={() => setOpen(o => !o)}
        aria-haspopup="menu"
        aria-expanded={open}
        aria-label={t('language.label')}
        className="flex items-center gap-1.5 text-sm font-medium px-2.5 py-1.5 rounded-lg text-ink-mute hover:text-ink hover:bg-surface-subtle transition-colors duration-150 cursor-pointer"
      >
        <current.Flag />
        <span>{t(current.key)}</span>
        <svg
          className={`h-3 w-3 shrink-0 transition-transform duration-150 ${open ? 'rotate-180' : ''}`}
          fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true"
        >
          <path strokeLinecap="round" strokeLinejoin="round" d="M19 9l-7 7-7-7" />
        </svg>
      </button>

      {open && (
        <div
          role="menu"
          aria-label={t('language.label')}
          className={`${dropdownClass} w-36 bg-surface-card border border-surface-border rounded-xl py-1 z-50 shadow-lg`}
        >
          {LANGUAGES.map(({ code, key, Flag }) => (
            <button
              key={code}
              role="menuitemradio"
              aria-checked={code === current.code}
              onClick={() => { i18n.changeLanguage(code); setOpen(false) }}
              className={`w-full flex items-center gap-2 px-3 py-1.5 text-sm transition-colors duration-100 cursor-pointer ${
                code === current.code
                  ? 'text-brand-700 bg-brand-50 font-medium'
                  : 'text-ink-body hover:bg-surface-subtle'
              }`}
            >
              <Flag />
              {t(key)}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}
