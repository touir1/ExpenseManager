import { Link } from 'react-router-dom'
import { usePageTitle } from '@/hooks/usePageTitle'

export default function Settings() {
  usePageTitle('Settings')
  return (
    <div className="max-w-5xl mx-auto w-full px-4 sm:px-6 py-8">
      {/* Page header */}
      <div className="mb-8">
        <h1 className="text-2xl font-semibold text-slate-900 tracking-tight">Settings</h1>
        <p className="text-sm text-slate-500 mt-1">Manage your account settings.</p>
      </div>

      {/* Settings cards */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {/* Password card */}
        <div className="bg-white rounded-2xl border border-slate-200 shadow-card p-6">
          <div className="flex items-center gap-3 mb-4">
            <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-brand-50">
              <svg
                className="h-4.5 w-4.5 text-brand-600"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                strokeWidth={2}
                aria-hidden="true"
              >
                <path strokeLinecap="round" strokeLinejoin="round" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
              </svg>
            </span>
            <h2 className="text-sm font-semibold text-slate-900">Password</h2>
          </div>
          <p className="text-xs text-slate-500 mb-3">Update the password used to sign in to your account.</p>
          <Link
            to="/change-password"
            className="inline-flex items-center gap-1.5 text-sm text-brand-600 hover:text-brand-700 font-medium transition-colors duration-150"
          >
            Change Password
            <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5} aria-hidden="true">
              <path strokeLinecap="round" strokeLinejoin="round" d="M9 5l7 7-7 7" />
            </svg>
          </Link>
        </div>
      </div>
    </div>
  )
}
