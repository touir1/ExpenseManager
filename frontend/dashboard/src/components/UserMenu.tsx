import { useTranslation } from 'react-i18next'
import LanguageSwitcher from '@/components/LanguageSwitcher'

type Props = {
  onLogout: () => void
}

export default function UserMenu({ onLogout }: Readonly<Props>) {
  const { t } = useTranslation()

  return (
    <div className="w-56 py-1.5">
      <div className="px-3 py-1.5 flex items-center gap-2">
        <span className="text-sm font-semibold text-ink-mute shrink-0">{t('language.label')}</span>
        <LanguageSwitcher placement="up" />
      </div>
      <div className="border-t border-surface-border my-1" />
      <button
        onClick={onLogout}
        className="w-full text-left flex items-center gap-2 text-sm font-semibold px-3 py-1.5 rounded-lg text-ink-body hover:text-ink hover:bg-surface-subtle transition-colors duration-150 cursor-pointer"
      >
        <svg className="h-4 w-4 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
          <path strokeLinecap="round" strokeLinejoin="round" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
        </svg>
        {t('nav.signOut')}
      </button>
    </div>
  )
}
