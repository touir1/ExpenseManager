import { useState, useRef, useEffect, useCallback } from 'react'
import { Link, NavLink, useNavigate, useLocation } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useQueryClient } from '@tanstack/react-query'
import { useAuth } from '@/features/auth/AuthContext'
import LanguageSwitcher from '@/components/LanguageSwitcher'
import ThemeToggle from '@/components/ThemeToggle'
import FamilySelector from '@/features/families/components/FamilySelector'
import DisplayCurrencySelector from '@/features/currencies/components/DisplayCurrencySelector'
import AddExpenseModal from '@/features/expenses/components/AddExpenseModal'
import NotificationBell from '@/features/notifications/components/NotificationBell'

const baseNavClass = 'text-sm font-semibold px-3 py-1.5 rounded-lg transition-colors duration-150'
const activeNavClass = `${baseNavClass} bg-brand-100 text-brand-600`
const inactiveNavClass = `${baseNavClass} text-ink-mute hover:text-ink hover:bg-surface-subtle`

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  isActive ? activeNavClass : inactiveNavClass

export default function NavBar() {
  const { t } = useTranslation()
  const { isAuthenticated, logout, user } = useAuth()
  const isAdmin = user?.isAdmin === true
  const navigate = useNavigate()
  const { pathname } = useLocation()
  const queryClient = useQueryClient()
  const [mobileOpen, setMobileOpen] = useState(false)
  const [userMenuOpen, setUserMenuOpen] = useState(false)
  const [addExpenseOpen, setAddExpenseOpen] = useState(false)
  const loggingOutRef = useRef(false)
  const hamburgerRef = useRef<HTMLButtonElement>(null)
  const menuRef = useRef<HTMLDivElement>(null)
  const userMenuRef = useRef<HTMLDivElement>(null)
  const wasOpenRef = useRef(false)

  const handleLogout = useCallback(() => {
    loggingOutRef.current = true
    logout()
    setMobileOpen(false)
    setUserMenuOpen(false)
  }, [logout])

  useEffect(() => {
    if (loggingOutRef.current && !isAuthenticated) {
      loggingOutRef.current = false
      // Defer past the same-commit effects batch so ProtectedRoute's <Navigate>
      // replaceState fires first; our pushState('/') then overrides it.
      Promise.resolve().then(() => navigate('/'))
    }
  }, [isAuthenticated, navigate])

  const expensesClass = pathname.startsWith('/expenses') ? activeNavClass : inactiveNavClass
  const familiesClass = pathname === '/families' ? activeNavClass : inactiveNavClass
  const settingsClass =
    pathname === '/settings' || pathname === '/change-password' ? activeNavClass : inactiveNavClass

  // Close user menu on outside click
  useEffect(() => {
    if (!userMenuOpen) return
    const handleClick = (e: MouseEvent) => {
      if (userMenuRef.current && !userMenuRef.current.contains(e.target as Node)) {
        setUserMenuOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClick)
    return () => document.removeEventListener('mousedown', handleClick)
  }, [userMenuOpen])

  // Mobile menu focus trap
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

  const userInitials = user
    ? ((user.firstName?.[0] ?? '') + (user.lastName?.[0] ?? '')).toUpperCase() || '?'
    : '?'

  return (
    <>
    <header
      className="sticky top-0 z-40 bg-surface-card border-b border-surface-border"
      style={{ boxShadow: '0 1px 0 rgba(60,30,10,0.06)' }}
    >
      <div className="max-w-6xl mx-auto px-4 sm:px-6 h-14 flex items-center gap-4">
        {/* Logo */}
        <Link
          to={isAuthenticated ? '/dashboard' : '/'}
          className="flex items-center gap-2 shrink-0"
        >
          <span className="flex h-7 w-7 items-center justify-center rounded-[8px] bg-brand-500">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" aria-hidden="true">
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

        {/* Desktop nav — fills remaining space */}
        <nav className="hidden sm:flex items-center gap-1 flex-1">
          {isAuthenticated ? (
            <>
              <NavLink to="/dashboard" className={navLinkClass}>
                {t('nav.dashboard')}
              </NavLink>
              <NavLink to="/expenses" className={() => expensesClass}>
                {t('nav.expenses')}
              </NavLink>
              <NavLink to="/families" className={() => familiesClass}>
                {t('nav.families')}
              </NavLink>
              {isAdmin && (
                <NavLink to="/admin" className={navLinkClass}>
                  {t('nav.admin')}
                </NavLink>
              )}

              {/* Right-side controls */}
              <div className="ml-auto flex items-center gap-2">
                <FamilySelector />
                <DisplayCurrencySelector />

                {/* Add expense */}
                <button
                  onClick={() => setAddExpenseOpen(true)}
                  aria-label={t('nav.addExpense')}
                  className="h-8 w-8 rounded-lg bg-brand-500 hover:bg-brand-600 text-white flex items-center justify-center transition-colors duration-150"
                >
                  <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5} aria-hidden="true">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M12 4v16m8-8H4" />
                  </svg>
                </button>

                {/* Notifications */}
                <NotificationBell />

                {/* User avatar + dropdown
                    Dropdown is always rendered (not conditional) so tests can
                    query Settings / Sign out regardless of open state. */}
                <div className="relative" ref={userMenuRef}>
                  <button
                    onClick={() => setUserMenuOpen(o => !o)}
                    aria-label={t('nav.userMenu')}
                    aria-expanded={userMenuOpen}
                    className="h-8 w-8 rounded-full bg-brand-500 hover:bg-brand-600 text-white text-xs font-bold flex items-center justify-center transition-colors duration-150 cursor-pointer"
                  >
                    {userInitials}
                  </button>

                  <div
                    className={`absolute right-0 top-full mt-2 w-56 bg-surface-card border border-surface-border rounded-2xl py-1.5 z-50 ${userMenuOpen ? '' : 'hidden'}`}
                    style={{ boxShadow: '0 8px 20px -10px rgba(30,20,10,0.5)' }}
                  >
                    <NavLink
                      to="/settings"
                      className={() => `flex items-center gap-2 ${settingsClass} w-full`}
                      onClick={() => setUserMenuOpen(false)}
                    >
                      <svg className="h-4 w-4 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                        <path strokeLinecap="round" strokeLinejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                      </svg>
                      {t('nav.settings')}
                    </NavLink>
                    <div className="border-t border-surface-border my-1" />
                    <div className="px-3 py-1.5 flex items-center gap-2">
                      <span className="text-sm font-semibold text-ink-mute shrink-0">{t('language.label')}</span>
                      <LanguageSwitcher />
                    </div>
                    <div className="px-3 py-1.5 flex items-center gap-2">
                      <span className="text-sm font-semibold text-ink-mute shrink-0">{t('settings.theme.label')}</span>
                      <ThemeToggle showLabel={false} />
                    </div>
                    <div className="border-t border-surface-border my-1" />
                    <button
                      onClick={handleLogout}
                      className="w-full text-left flex items-center gap-2 text-sm font-semibold px-3 py-1.5 rounded-lg text-ink-body hover:text-ink hover:bg-surface-subtle transition-colors duration-150 cursor-pointer"
                    >
                      <svg className="h-4 w-4 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                      </svg>
                      {t('nav.signOut')}
                    </button>
                  </div>
                </div>
              </div>
            </>
          ) : (
            <>
              {/* Marketing nav links */}
              <a href="#how-it-works" className={inactiveNavClass}>
                {t('nav.howItWorks')}
              </a>
              <a href="#families" className={inactiveNavClass}>
                {t('nav.forFamilies')}
              </a>
              <a href="#pricing" className={inactiveNavClass}>
                {t('nav.pricing')}
              </a>
              <a href="#help" className={inactiveNavClass}>
                {t('nav.help')}
              </a>

              {/* Auth buttons */}
              <div className="ml-auto flex items-center gap-2">
                <LanguageSwitcher />
                <NavLink to="/login" className={navLinkClass}>
                  {t('nav.signIn')}
                </NavLink>
                <Link
                  to="/register"
                  className="text-sm font-semibold px-3.5 py-1.5 rounded-full bg-brand-500 hover:bg-brand-600 text-white transition-colors duration-150"
                  style={{ boxShadow: '0 6px 16px -6px rgba(200,98,62,0.6)' }}
                >
                  {t('nav.getStarted')}
                </Link>
              </div>
            </>
          )}
        </nav>

        {/* Mobile hamburger */}
        <button
          ref={hamburgerRef}
          className="sm:hidden ml-auto p-2 rounded-lg text-ink-mute hover:bg-surface-subtle transition-colors duration-150 cursor-pointer"
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
                to="/expenses"
                className={() => expensesClass}
                onClick={() => setMobileOpen(false)}
              >
                {t('nav.expenses')}
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
              {isAdmin && (
                <NavLink
                  to="/admin"
                  className={navLinkClass}
                  onClick={() => setMobileOpen(false)}
                >
                  {t('nav.admin')}
                </NavLink>
              )}
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

    {addExpenseOpen && (
      <AddExpenseModal
        onSuccess={() => {
          setAddExpenseOpen(false)
          queryClient.invalidateQueries({ queryKey: ['expenses'] })
        }}
        onClose={() => setAddExpenseOpen(false)}
      />
    )}
  </>
  )
}
