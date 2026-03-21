import { useState } from 'react'
import { Link, useNavigate, useLocation } from 'react-router-dom'
import { useAuth } from '@/auth/AuthContext'

export default function NavBar() {
  const { isAuthenticated, logout } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [mobileOpen, setMobileOpen] = useState(false)

  const handleLogout = () => {
    logout()
    setMobileOpen(false)
    navigate('/')
  }

  const isActive = (path: string) => location.pathname === path

  const linkClass = (path: string) =>
    `text-sm font-medium px-3 py-1.5 rounded-lg transition-colors duration-150 ${
      isActive(path)
        ? 'bg-brand-50 text-brand-700'
        : 'text-slate-600 hover:text-slate-900 hover:bg-slate-100'
    }`

  return (
    <header className="sticky top-0 z-40 bg-white border-b border-slate-200 shadow-sm">
      <div className="max-w-5xl mx-auto px-4 sm:px-6 h-14 flex items-center justify-between">
        {/* Logo */}
        <Link
          to={isAuthenticated ? '/home-auth' : '/'}
          className="flex items-center gap-1.5 shrink-0"
        >
          {/* Minimal icon mark */}
          <span className="flex h-7 w-7 items-center justify-center rounded-lg bg-brand-600">
            <svg
              className="h-4 w-4 text-white"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={2.2}
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M12 6v6m0 0v6m0-6h6m-6 0H6"
              />
            </svg>
          </span>
          <span className="font-semibold text-slate-900 text-[15px] tracking-tight">
            Expenses<span className="text-brand-600">Manager</span>
          </span>
        </Link>

        {/* Desktop nav */}
        <nav className="hidden sm:flex items-center gap-1">
          {isAuthenticated ? (
            <>
              <Link to="/home-auth" className={linkClass('/home-auth')}>
                Dashboard
              </Link>
              <Link to="/change-password" className={linkClass('/change-password')}>
                Settings
              </Link>
              <button
                onClick={handleLogout}
                className="ml-2 text-sm font-medium px-3.5 py-1.5 rounded-lg bg-slate-100 hover:bg-slate-200 text-slate-700 transition-colors duration-150 cursor-pointer"
              >
                Sign out
              </button>
            </>
          ) : (
            <>
              <Link to="/" className={linkClass('/')}>
                Home
              </Link>
              <Link to="/login" className={linkClass('/login')}>
                Sign in
              </Link>
              <Link
                to="/register"
                className="ml-2 text-sm font-medium px-3.5 py-1.5 rounded-lg bg-brand-600 hover:bg-brand-700 text-white transition-colors duration-150"
              >
                Get started
              </Link>
            </>
          )}
        </nav>

        {/* Mobile hamburger */}
        <button
          className="sm:hidden p-2 rounded-lg text-slate-500 hover:bg-slate-100 transition-colors duration-150 cursor-pointer"
          onClick={() => setMobileOpen(o => !o)}
          aria-label="Toggle menu"
        >
          <svg
            className="h-5 w-5"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            strokeWidth={2}
            aria-hidden="true"
          >
            {mobileOpen ? (
              <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
            ) : (
              <path strokeLinecap="round" strokeLinejoin="round" d="M4 6h16M4 12h16M4 18h16" />
            )}
          </svg>
        </button>
      </div>

      {/* Mobile menu */}
      {mobileOpen && (
        <div className="sm:hidden border-t border-slate-200 bg-white px-4 py-3 flex flex-col gap-1">
          {isAuthenticated ? (
            <>
              <Link
                to="/home-auth"
                className={linkClass('/home-auth')}
                onClick={() => setMobileOpen(false)}
              >
                Dashboard
              </Link>
              <Link
                to="/change-password"
                className={linkClass('/change-password')}
                onClick={() => setMobileOpen(false)}
              >
                Settings
              </Link>
              <button
                onClick={handleLogout}
                className="text-left text-sm font-medium px-3 py-1.5 rounded-lg text-slate-600 hover:text-slate-900 hover:bg-slate-100 transition-colors duration-150 cursor-pointer"
              >
                Sign out
              </button>
            </>
          ) : (
            <>
              <Link
                to="/"
                className={linkClass('/')}
                onClick={() => setMobileOpen(false)}
              >
                Home
              </Link>
              <Link
                to="/login"
                className={linkClass('/login')}
                onClick={() => setMobileOpen(false)}
              >
                Sign in
              </Link>
              <Link
                to="/register"
                className="text-sm font-medium px-3 py-1.5 rounded-lg text-brand-600 hover:bg-brand-50 transition-colors duration-150"
                onClick={() => setMobileOpen(false)}
              >
                Get started
              </Link>
            </>
          )}
        </div>
      )}
    </header>
  )
}
