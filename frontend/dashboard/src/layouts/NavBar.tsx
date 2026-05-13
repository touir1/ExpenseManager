import { useState, useRef, useEffect } from 'react'
import { Link, NavLink, useNavigate, useLocation } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useAuth } from '@/features/auth/AuthContext'
import LanguageSwitcher from '@/components/LanguageSwitcher'
import FamilySelector from '@/features/families/components/FamilySelector'

const baseNavClass = 'text-sm font-semibold px-3 py-1.5 rounded-lg transition-colors duration-150'
const activeNavClass = `${baseNavClass} bg-brand-100 text-brand-600`
const inactiveNavClass = `${baseNavClass} text-ink-mute hover:text-ink hover:bg-surface-subtle`

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  isActive ? activeNavClass : inactiveNavClass

export default function NavBar() {
  const { t } = useTranslation()
  const { isAuthenticated, logout } = useAuth()
  const navigate = useNavigate()
  const { pathname } = useLocation()
  const [mobileOpen, setMobileOpen] = useState(false)
  const hamburgerRef = useRef<HTMLButtonElement>(null)
  const menuRef = useRef<HTMLDivElement>(null)
  const wasOpenRef = useRef(false)

  const handleLogout = () => {
    logout()
    setMobileOpen(false)
    navigate('/')
  }

  // Settings is active on /settings, /change-password; families on /families
  const familiesClass = pathname === '/families' ? activeNavClass : inactiveNavClass
  const settingsClass = pathname === '/settings' || pathname === '/change-password'
    ? activeNavClass
    : inactiveNavClass

  useEffect(() => {
    if (mobileOpen) {
      wasOpenRef.current = true
      const menu = menuRef.current!
      const focusables = Array.from(
        menu.querySelectorAll<HTMLElement>('a[href], button:not([disabled])')
      )
      focusables[0]?.focus()
      const handleKeyDown = (e: KeyboardEvent) => {
        if (e.key === 'Escape') {
          setMobileOpen(false)
          return
        }
        if (e.key !== 'Tab' || focusables.length === 0) return
        const first = focusables[0]
        const last = focusables[focusables.length - 1]
        if (e.shiftKey && document.activeElement === first) {
          e.preventDefault()
          last.focus()
        } else if (!e.shiftKey && document.activeElement === last) {
          e.preventDefault()
          first.focus()
        }
      }
      menu.addEventListener('keydown', handleKeyDown)
      return () => menu.removeEventListener('keydown', handleKeyDown)
    } else if (wasOpenRef.current) {
      hamburgerRef.current?.focus()
    }
  }, [mobileOpen])

  return (
    <header className="sticky top-0 z-40 bg-surface-card border-b border-surface-border" style={{ boxShadow: '0 1px 0 rgba(60,30,10,0.06)' }}>
      <div className="max-w-6xl mx-auto px-4 sm:px-6 h-14 flex items-center justify-between">
        {/* Logo */}
        <Link
          to={isAuthenticated ? '/dashboard' : '/'}
          className="flex items-center gap-2 shrink-0"
        >
          <span className="flex h-7 w-7 items-center justify-center rounded-[8px] bg-brand-500">
            <svg
              width="16"
              height="16"
              viewBox="0 0 24 24"
              fill="none"
              aria-hidden="true"
            >
              <path
                d="M5 20V8c0-1 .8-2 2-2h2c1.2 0 2 .8 2 2v4h2V8c0-1 .8-2 2-2h2c1.2 0 2 .8 2 2v12"
                stroke="white"
                strokeWidth="2.2"
                strokeLinecap="round"
                strokeLinejoin="round"
              />
            </svg>
          </span>
          <span className="font-bold text-ink text-[15px] tracking-tight">
            Expense<span className="text-brand-500">Manager.</span>
          </span>
        </Link>

        {/* Desktop nav */}
        <nav className="hidden sm:flex items-center gap-1">
          {isAuthenticated ? (
            <>
              <NavLink to="/dashboard" className={navLinkClass}>
                {t('nav.dashboard')}
              </NavLink>
              <NavLink to="/families" className={() => familiesClass}>
                {t('nav.families')}
              </NavLink>
              <NavLink to="/settings" className={() => settingsClass}>
                {t('nav.settings')}
              </NavLink>
              <FamilySelector />
              <button
                onClick={handleLogout}
                className="ml-2 text-sm font-semibold px-3.5 py-1.5 rounded-lg bg-surface-subtle hover:bg-surface-border text-ink-body transition-colors duration-150 cursor-pointer"
              >
                {t('nav.signOut')}
              </button>
            </>
          ) : (
            <>
              <NavLink to="/" end className={navLinkClass}>
                {t('nav.home')}
              </NavLink>
              <NavLink to="/login" className={navLinkClass}>
                {t('nav.signIn')}
              </NavLink>
              <Link
                to="/register"
                className="ml-2 text-sm font-semibold px-3.5 py-1.5 rounded-full bg-brand-500 hover:bg-brand-600 text-white transition-colors duration-150"
                style={{ boxShadow: '0 6px 16px -6px rgba(200,98,62,0.6)' }}
              >
                {t('nav.getStarted')}
              </Link>
            </>
          )}
          <div className="ml-2">
            <LanguageSwitcher />
          </div>
        </nav>

        {/* Mobile hamburger */}
        <button
          ref={hamburgerRef}
          className="sm:hidden p-2 rounded-lg text-ink-mute hover:bg-surface-subtle transition-colors duration-150 cursor-pointer"
          onClick={() => setMobileOpen(o => !o)}
          aria-label={t('nav.toggleMenu')}
          aria-expanded={mobileOpen}
          aria-controls="mobile-menu"
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
        <nav
          id="mobile-menu"
          ref={menuRef}
          aria-label={t('nav.mobileNav')}
          className="sm:hidden border-t border-surface-border bg-surface-card px-4 py-3 flex flex-col gap-1"
        >
          {isAuthenticated ? (
            <>
              <NavLink
                to="/dashboard"
                className={navLinkClass}
                onClick={() => setMobileOpen(false)}
              >
                {t('nav.dashboard')}
              </NavLink>
              <NavLink
                to="/families"
                className={() => familiesClass}
                onClick={() => setMobileOpen(false)}
              >
                {t('nav.families')}
              </NavLink>
              <NavLink
                to="/settings"
                className={() => settingsClass}
                onClick={() => setMobileOpen(false)}
              >
                {t('nav.settings')}
              </NavLink>
              <button
                onClick={handleLogout}
                className="text-left text-sm font-semibold px-3 py-1.5 rounded-lg text-ink-body hover:text-ink hover:bg-surface-subtle transition-colors duration-150 cursor-pointer"
              >
                {t('nav.signOut')}
              </button>
            </>
          ) : (
            <>
              <NavLink
                to="/"
                end
                className={navLinkClass}
                onClick={() => setMobileOpen(false)}
              >
                {t('nav.home')}
              </NavLink>
              <NavLink
                to="/login"
                className={navLinkClass}
                onClick={() => setMobileOpen(false)}
              >
                {t('nav.signIn')}
              </NavLink>
              <Link
                to="/register"
                className="text-sm font-semibold px-3 py-1.5 rounded-lg text-brand-500 hover:bg-brand-100 transition-colors duration-150"
                onClick={() => setMobileOpen(false)}
              >
                {t('nav.getStarted')}
              </Link>
            </>
          )}
          <div className="mt-1">
            <LanguageSwitcher />
          </div>
        </nav>
      )}
    </header>
  )
}
