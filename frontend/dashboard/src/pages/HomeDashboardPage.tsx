import { Link } from 'react-router-dom'
import { useAuth } from '@/features/auth/AuthContext'
import { usePageTitle } from '@/hooks/usePageTitle'

function DashboardSkeleton() {
  return (
    <div className="max-w-5xl mx-auto w-full px-4 sm:px-6 py-8" aria-busy="true" aria-label="Loading dashboard">
      <div className="mb-8 animate-pulse">
        <div className="h-7 bg-slate-200 rounded-lg w-32 mb-2" />
        <div className="h-4 bg-slate-100 rounded w-64" />
      </div>
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {[0, 1, 2].map(i => (
          <div key={i} className="bg-white rounded-2xl border border-slate-200 shadow-card p-6 animate-pulse">
            <div className="flex items-center gap-3 mb-4">
              <div className="h-9 w-9 rounded-xl bg-slate-100 shrink-0" />
              <div className="h-4 bg-slate-200 rounded w-20" />
            </div>
            <div className="h-3 bg-slate-100 rounded w-full mb-2" />
            <div className="h-4 bg-slate-200 rounded w-32" />
          </div>
        ))}
      </div>
    </div>
  )
}

export default function HomeDashboardPage() {
  usePageTitle('Dashboard')
  const { user, isLoading } = useAuth()

  if (isLoading) return <DashboardSkeleton />

  return (
    <div className="max-w-5xl mx-auto w-full px-4 sm:px-6 py-8">
      {/* Page header */}
      <div className="mb-8">
        <h1 className="text-2xl font-semibold text-slate-900 tracking-tight">Dashboard</h1>
        <p className="text-sm text-slate-500 mt-1">
          Welcome {user?.firstName ?? user?.email ?? 'user'}! This is your private home page.
        </p>
      </div>

      {/* Cards grid */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {/* Account card */}
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
                <path strokeLinecap="round" strokeLinejoin="round" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
              </svg>
            </span>
            <h2 className="text-sm font-semibold text-slate-900">Account</h2>
          </div>
          <p className="text-xs text-slate-500 mb-0.5">Signed in as</p>
          <p className="text-sm font-medium text-slate-800 truncate">{user?.email ?? '—'}</p>
        </div>

        {/* Quick actions card */}
        <div className="bg-white rounded-2xl border border-slate-200 shadow-card p-6">
          <div className="flex items-center gap-3 mb-4">
            <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-slate-100">
              <svg
                className="h-4.5 w-4.5 text-slate-600"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                strokeWidth={2}
                aria-hidden="true"
              >
                <path strokeLinecap="round" strokeLinejoin="round" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                <path strokeLinecap="round" strokeLinejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
            </span>
            <h2 className="text-sm font-semibold text-slate-900">Settings</h2>
          </div>
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

        {/* Coming soon card */}
        <div className="bg-white rounded-2xl border border-slate-200 shadow-card p-6 sm:col-span-2 lg:col-span-1">
          <div className="flex items-center gap-3 mb-4">
            <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-amber-50">
              <svg
                className="h-4.5 w-4.5 text-amber-500"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                strokeWidth={2}
                aria-hidden="true"
              >
                <path strokeLinecap="round" strokeLinejoin="round" d="M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 14h.01M12 14h.01M15 11h.01M12 11h.01M9 11h.01M7 21h10a2 2 0 002-2V5a2 2 0 00-2-2H7a2 2 0 00-2 2v14a2 2 0 002 2z" />
              </svg>
            </span>
            <h2 className="text-sm font-semibold text-slate-900">Expenses</h2>
          </div>
          <p className="text-sm text-slate-400 italic">Coming soon…</p>
        </div>
      </div>

    </div>
  )
}
