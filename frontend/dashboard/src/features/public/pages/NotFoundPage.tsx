import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { usePageTitle } from '@/hooks/usePageTitle'

export default function NotFoundPage() {
  const { t } = useTranslation()
  usePageTitle(t('public.notFound.pageTitle'))
  return (
    <div className="auth-page">
      <div className="text-center max-w-lg px-4">
        <span className="inline-flex h-16 w-16 items-center justify-center rounded-2xl bg-surface-subtle mb-6 shadow-card-md">
          <svg
            className="h-8 w-8 text-ink-mute"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            strokeWidth={2}
            aria-hidden="true"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
        </span>

        <h1 className="text-3xl font-bold text-ink tracking-tight mb-3">
          {t('public.notFound.title')}
        </h1>

        <p className="text-base text-ink-mute leading-relaxed mb-8">
          {t('public.notFound.description')}
        </p>

        <Link
          to="/"
          className="inline-flex items-center justify-center px-5 py-2.5 rounded-lg bg-brand-600 hover:bg-brand-700 text-white text-sm font-medium transition-colors duration-150 shadow-sm"
        >
          {t('public.notFound.goHome')}
        </Link>
      </div>
    </div>
  )
}
