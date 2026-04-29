import { Link } from 'react-router-dom'
import { usePageTitle } from '@/hooks/usePageTitle'

export default function VerifyErrorPage() {
  usePageTitle('Verification Failed')
  return (
    <div className="auth-page">
      <div className="text-center max-w-lg px-4">
        <span className="inline-flex h-16 w-16 items-center justify-center rounded-2xl bg-red-100 mb-6 shadow-card-md">
          <svg
            className="h-8 w-8 text-red-500"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            strokeWidth={2}
            aria-hidden="true"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M12 9v2m0 4h.01M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z"
            />
          </svg>
        </span>

        <h1 className="text-3xl font-bold text-slate-900 tracking-tight mb-3">
          Verification link expired
        </h1>

        <p className="text-base text-slate-500 leading-relaxed mb-8">
          This email verification link has already been used or has expired.
          Please register again to receive a new verification email.
        </p>

        <Link
          to="/register"
          className="inline-flex items-center justify-center px-5 py-2.5 rounded-lg bg-brand-600 hover:bg-brand-700 text-white text-sm font-medium transition-colors duration-150 shadow-sm"
        >
          Back to register
        </Link>
      </div>
    </div>
  )
}
