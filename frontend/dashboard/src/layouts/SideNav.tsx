import { useEffect, useRef, useCallback } from 'react'
import { Link, NavLink, useNavigate, useLocation } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import * as Popover from '@radix-ui/react-popover'
import { useAuth } from '@/features/auth/AuthContext'
import UserMenu from '@/components/UserMenu'

const basePrimaryClass =
  'flex items-center gap-2.5 text-sm font-semibold px-3 py-2 rounded-lg transition-colors duration-150'
const activePrimaryClass = `${basePrimaryClass} bg-brand-100 text-brand-600`
const inactivePrimaryClass = `${basePrimaryClass} text-ink-mute hover:text-ink hover:bg-surface-subtle`
const primaryLinkClass = ({ isActive }: { isActive: boolean }) =>
  isActive ? activePrimaryClass : inactivePrimaryClass

function HomeIcon() {
  return (
    <svg className="h-4.5 w-4.5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
      <path strokeLinecap="round" strokeLinejoin="round" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
    </svg>
  )
}

function ReceiptIcon() {
  return (
    <svg className="h-4.5 w-4.5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
      <path strokeLinecap="round" strokeLinejoin="round" d="M9 14l2 2 4-4m3 9l-2-1.5-2 1.5-2-1.5-2 1.5-2-1.5-2 1.5V5a2 2 0 012-2h8a2 2 0 012 2v16z" />
    </svg>
  )
}

function UsersIcon() {
  return (
    <svg className="h-4.5 w-4.5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
      <path strokeLinecap="round" strokeLinejoin="round" d="M17 20h5v-2a4 4 0 00-3-3.87M9 20H4v-2a4 4 0 013-3.87m5-3.13a4 4 0 100-8 4 4 0 000 8zm6 4a4 4 0 10-8 0" />
    </svg>
  )
}

function ShieldIcon() {
  return (
    <svg className="h-4.5 w-4.5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
      <path strokeLinecap="round" strokeLinejoin="round" d="M12 3l7 3v6c0 4.5-3 8-7 9-4-1-7-4.5-7-9V6l7-3z" />
    </svg>
  )
}

function GearIcon() {
  return (
    <svg className="h-4.5 w-4.5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
      <path strokeLinecap="round" strokeLinejoin="round" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
      <path strokeLinecap="round" strokeLinejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
    </svg>
  )
}

type Props = {
  isMobileOpen: boolean
  onMobileClose: () => void
}

function SideNavContent({ onNavigate }: Readonly<{ onNavigate?: () => void }>) {
  const { t } = useTranslation()
  const { logout, user, isAuthenticated } = useAuth()
  const isAdmin = user?.isAdmin === true
  const navigate = useNavigate()
  const { pathname } = useLocation()
  const loggingOutRef = useRef(false)

  const settingsClass =
    pathname === '/settings' || pathname === '/change-password' ? activePrimaryClass : inactivePrimaryClass

  const handleLogout = useCallback(() => {
    loggingOutRef.current = true
    logout()
  }, [logout])

  useEffect(() => {
    if (loggingOutRef.current && !isAuthenticated) {
      loggingOutRef.current = false
      Promise.resolve().then(() => navigate('/'))
    }
  }, [isAuthenticated, navigate])

  const userInitials = user
    ? ((user.firstName?.[0] ?? '') + (user.lastName?.[0] ?? '')).toUpperCase() || '?'
    : '?'
  const userName = user ? [user.firstName, user.lastName].filter(Boolean).join(' ') || user.email : ''

  return (
    <div className="flex flex-col h-full">
      <Link to="/dashboard" className="flex items-center gap-2 shrink-0 px-4 h-14" onClick={onNavigate}>
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

      <nav className="flex flex-col gap-1 px-3 mt-2" aria-label={t('nav.mobileNav')}>
        <NavLink to="/dashboard" className={primaryLinkClass} onClick={onNavigate}>
          <HomeIcon />
          {t('nav.dashboard')}
        </NavLink>
        <NavLink to="/expenses" className={primaryLinkClass} onClick={onNavigate}>
          <ReceiptIcon />
          {t('nav.expenses')}
        </NavLink>
        <NavLink to="/families" className={primaryLinkClass} onClick={onNavigate}>
          <UsersIcon />
          {t('nav.families')}
        </NavLink>
        {isAdmin && (
          <NavLink to="/admin" className={primaryLinkClass} onClick={onNavigate}>
            <ShieldIcon />
            {t('nav.admin')}
          </NavLink>
        )}
      </nav>

      <div className="border-t border-surface-border mx-3 my-3" />

      <nav className="flex flex-col gap-1 px-3">
        <NavLink to="/settings" className={() => `${settingsClass} w-full`} onClick={onNavigate}>
          <GearIcon />
          {t('nav.settings')}
        </NavLink>
      </nav>

      <div className="mt-auto p-3 border-t border-surface-border">
        <Popover.Root>
          <Popover.Trigger asChild>
            <button
              aria-label={t('nav.userMenu')}
              className="w-full flex items-center gap-2.5 px-2 py-2 rounded-lg hover:bg-surface-subtle transition-colors duration-150 cursor-pointer"
            >
              <span className="h-9 w-9 shrink-0 rounded-full bg-brand-500 text-white text-xs font-bold flex items-center justify-center">
                {userInitials}
              </span>
              <span className="flex-1 min-w-0 text-left">
                <span className="block text-sm font-semibold text-ink truncate">{userName}</span>
                <span className="block text-xs text-ink-faint truncate">{user?.email}</span>
              </span>
            </button>
          </Popover.Trigger>
          <Popover.Portal>
            <Popover.Content
              side="top"
              align="start"
              sideOffset={8}
              className="bg-surface-card border border-surface-border rounded-2xl shadow-warm z-50"
              style={{ zIndex: 9999 }}
            >
              <UserMenu onLogout={handleLogout} />
            </Popover.Content>
          </Popover.Portal>
        </Popover.Root>
      </div>
    </div>
  )
}

export default function SideNav({ isMobileOpen, onMobileClose }: Readonly<Props>) {
  const { t } = useTranslation()
  const drawerRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!isMobileOpen) return
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onMobileClose()
    }
    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [isMobileOpen, onMobileClose])

  return (
    <>
      {/* Desktop: permanent sidebar */}
      <aside className="hidden md:flex md:w-64 md:shrink-0 md:flex-col md:sticky md:top-0 md:h-screen bg-surface-card border-r border-surface-border">
        <SideNavContent />
      </aside>

      {/* Mobile: overlay drawer */}
      {isMobileOpen && (
        <div className="md:hidden fixed inset-0 z-50">
          <button
            aria-label={t('nav.toggleMenu')}
            className="absolute inset-0 bg-black/40 cursor-default"
            onClick={onMobileClose}
          />
          <div
            ref={drawerRef}
            role="navigation"
            aria-label={t('nav.mobileNav')}
            className="absolute left-0 top-0 h-full w-72 bg-surface-card border-r border-surface-border shadow-warm"
          >
            <SideNavContent onNavigate={onMobileClose} />
          </div>
        </div>
      )}
    </>
  )
}
