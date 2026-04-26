import { Link } from 'react-router-dom'
import { usePageTitle } from '@/hooks/usePageTitle'

export default function HomePublicPage() {
  usePageTitle()
  return (
    <div className="auth-page">
      <div className="text-center max-w-lg px-4">
        {/* Icon mark */}
        <span className="inline-flex h-16 w-16 items-center justify-center rounded-2xl bg-brand-600 mb-6 shadow-card-md">
          <svg
            className="h-8 w-8 text-white"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            strokeWidth={2}
            aria-hidden="true"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 14h.01M12 14h.01M15 11h.01M12 11h.01M9 11h.01M7 21h10a2 2 0 002-2V5a2 2 0 00-2-2H7a2 2 0 00-2 2v14a2 2 0 002 2z"
            />
          </svg>
        </span>

        <h1 className="text-3xl font-bold text-slate-900 tracking-tight mb-3">
          Track your expenses,{' '}
          <span className="text-brand-600">simply.</span>
        </h1>

        <p className="text-base text-slate-500 leading-relaxed mb-8">
          A clean, minimal tool for keeping your finances in check.
          Sign in to your account or create one — it takes under a minute.
        </p>

        <div className="flex flex-col sm:flex-row gap-3 justify-center">
          <Link
            to="/login"
            className="inline-flex items-center justify-center px-5 py-2.5 rounded-lg bg-brand-600 hover:bg-brand-700 text-white text-sm font-medium transition-colors duration-150 shadow-sm"
          >
            Sign in
          </Link>
          <Link
            to="/register"
            className="inline-flex items-center justify-center px-5 py-2.5 rounded-lg bg-white hover:bg-slate-50 text-slate-700 text-sm font-medium border border-slate-300 transition-colors duration-150 shadow-sm"
          >
            Create account
          </Link>
        </div>
      </div>
    </div>
  )
}
