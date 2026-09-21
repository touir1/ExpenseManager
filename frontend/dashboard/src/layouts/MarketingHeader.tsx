import { useState, useRef, useEffect } from 'react'
import { Link, NavLink } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import LanguageSwitcher from '@/components/LanguageSwitcher'

const baseNavClass = 'text-sm font-semibold px-3 py-1.5 rounded-lg transition-colors duration-150'
const activeNavClass = `${baseNavClass} bg-brand-100 text-brand-600`
const inactiveNavClass = `${baseNavClass} text-ink-mute hover:text-ink hover:bg-surface-subtle`

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  isActive ? activeNavClass : inactiveNavClass

// Top bar for logged-out pages (marketing / auth). Authenticated pages use
// the SideNav + AppHeader shell instead — see RootLayout.tsx.
export default function MarketingHeader() {
  const { t } = useTranslation()
  const [mobileOpen, setMobileOpen] = useState(false)
  const hamburgerRef = useRef<HTMLButtonElement>(null)
  const menuRef = useRef<HTMLDivElement>(null)
  const wasOpenRef = useRef(false)

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
    <header
      className="sticky top-0 z-40 bg-surface-card border-b border-surface-border"
      style={{ boxShadow: '0 1px 0 rgba(60,30,10,0.06)' }}
    >
      <div className="max-w-6xl mx-auto px-4 sm:px-6 h-14 flex items-center gap-4">
        <Link to="/" className="flex items-center gap-2 shrink-0">
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
          <span className="font-serif font-bold text-ink text-[17px] tracking-tight">
            Expense<span className="text-brand-500">Manager.</span>
          </span>
        </Link>

        <nav className="hidden sm:flex items-center gap-1 flex-1">
          <a href="#how-it-works" className={inactiveNavClass}>{t('nav.howItWorks')}</a>
          <a href="#families" className={inactiveNavClass}>{t('nav.forFamilies')}</a>
          <a href="#pricing" className={inactiveNavClass}>{t('nav.pricing')}</a>
          <a href="#help" className={inactiveNavClass}>{t('nav.help')}</a>

          <div className="ml-auto flex items-center gap-2">
            <LanguageSwitcher />
            <NavLink to="/login" className={navLinkClass}>{t('nav.signIn')}</NavLink>
            <Link
              to="/register"
              className="text-sm font-semibold px-3.5 py-1.5 rounded-full bg-brand-500 hover:bg-brand-600 text-white transition-colors duration-150"
              style={{ boxShadow: '0 6px 16px -6px rgba(200,98,62,0.6)' }}
            >
              {t('nav.getStarted')}
            </Link>
          </div>
        </nav>

        <button
          ref={hamburgerRef}
          className="sm:hidden ml-auto p-2 rounded-lg text-ink-mute hover:bg-surface-subtle transition-colors duration-150 cursor-pointer"
          onClick={() => setMobileOpen(o => !o)}
          aria-label={t('nav.toggleMenu')}
          aria-expanded={mobileOpen}
          aria-controls="mobile-menu"
        >
          <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
            {mobileOpen ? (
              <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
            ) : (
              <path strokeLinecap="round" strokeLinejoin="round" d="M4 6h16M4 12h16M4 18h16" />
            )}
          </svg>
        </button>
      </div>

      {mobileOpen && (
        <nav
          id="mobile-menu"
          ref={menuRef}
          aria-label={t('nav.mobileNav')}
          className="sm:hidden border-t border-surface-border bg-surface-card px-4 py-3 flex flex-col gap-1"
        >
          <NavLink to="/" end className={navLinkClass} onClick={() => setMobileOpen(false)}>
            {t('nav.home')}
          </NavLink>
          <NavLink to="/login" className={navLinkClass} onClick={() => setMobileOpen(false)}>
            {t('nav.signIn')}
          </NavLink>
          <Link
            to="/register"
            className="text-sm font-semibold px-3 py-1.5 rounded-lg text-brand-500 hover:bg-brand-100 transition-colors duration-150"
            onClick={() => setMobileOpen(false)}
          >
            {t('nav.getStarted')}
          </Link>
          <div className="mt-1">
            <LanguageSwitcher />
          </div>
        </nav>
      )}
    </header>
  )
}
