import { useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { usePageTitle } from '@/hooks/usePageTitle'
import { acceptInvite } from '@/features/families/services/familyApi.service'

type Status = 'loading' | 'success' | 'error'

export default function AcceptInvitePage() {
  const { t } = useTranslation()
  usePageTitle(t('families.acceptInvite.pageTitle'))
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')
  const [status, setStatus] = useState<Status>('loading')
  const [errorMessage, setErrorMessage] = useState<string>('')

  useEffect(() => {
    if (!token) {
      setErrorMessage(t('families.acceptInvite.error'))
      setStatus('error')
      return
    }
    acceptInvite(token, { silent: true }).then(res => {
      if (res.ok) {
        setStatus('success')
      } else {
        setErrorMessage(res.error ?? t('families.acceptInvite.error'))
        setStatus('error')
      }
    })
  }, [token, t])

  const icon =
    status === 'loading' ? (
      <span className="inline-flex h-16 w-16 items-center justify-center rounded-2xl bg-slate-100 mb-6 shadow-card-md">
        <svg className="h-8 w-8 text-ink-faint animate-spin" fill="none" viewBox="0 0 24 24">
          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
        </svg>
      </span>
    ) : status === 'success' ? (
      <span className="inline-flex h-16 w-16 items-center justify-center rounded-2xl bg-emerald-100 mb-6 shadow-card-md">
        <svg className="h-8 w-8 text-emerald-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
          <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
        </svg>
      </span>
    ) : (
      <span className="inline-flex h-16 w-16 items-center justify-center rounded-2xl bg-red-100 mb-6 shadow-card-md">
        <svg className="h-8 w-8 text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
          <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v2m0 4h.01M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z" />
        </svg>
      </span>
    )

  const title =
    status === 'loading'
      ? t('families.acceptInvite.loading')
      : status === 'success'
        ? t('families.acceptInvite.success')
        : errorMessage

  return (
    <div className="auth-page">
      <div className="text-center max-w-lg px-4">
        {icon}
        <h1 className="text-3xl font-bold text-ink tracking-tight mb-3">{title}</h1>
        {status === 'success' && (
          <Link
            to="/families"
            className="inline-flex items-center justify-center px-5 py-2.5 rounded-lg bg-brand-600 hover:bg-brand-700 text-white text-sm font-medium transition-colors duration-150 shadow-sm mt-6"
          >
            {t('families.acceptInvite.goToFamilies')}
          </Link>
        )}
        {status === 'error' && (
          <Link
            to="/families"
            className="inline-flex items-center justify-center px-5 py-2.5 rounded-lg bg-surface-subtle hover:bg-surface-muted text-ink text-sm font-medium transition-colors duration-150 shadow-sm mt-6"
          >
            {t('families.acceptInvite.goToFamilies')}
          </Link>
        )}
      </div>
    </div>
  )
}
