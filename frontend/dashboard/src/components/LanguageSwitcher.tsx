import { useTranslation } from 'react-i18next'

const LANGUAGES = [
  { code: 'en', key: 'language.en', flag: '🇬🇧' },
  { code: 'fr', key: 'language.fr', flag: '🇫🇷' },
  { code: 'es', key: 'language.es', flag: '🇪🇸' },
  { code: 'de', key: 'language.de', flag: '🇩🇪' },
] as const

export default function LanguageSwitcher() {
  const { t, i18n } = useTranslation()

  return (
    <select
      value={i18n.resolvedLanguage ?? i18n.language}
      onChange={e => i18n.changeLanguage(e.target.value)}
      aria-label={t('language.label')}
      className="text-sm text-ink-mute bg-transparent border border-surface-border rounded-lg px-2 py-1 cursor-pointer hover:border-surface-muted focus:outline-none focus:ring-2 focus:ring-brand-500 transition-colors duration-150"
    >
      {LANGUAGES.map(({ code, key, flag }) => (
        <option key={code} value={code}>{flag} {t(key)}</option>
      ))}
    </select>
  )
}
