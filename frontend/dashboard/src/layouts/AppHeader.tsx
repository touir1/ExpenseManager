import { useState } from 'react'
import { useLocation } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useQueryClient } from '@tanstack/react-query'
import FamilySelector from '@/features/families/components/FamilySelector'
import DisplayCurrencySelector from '@/features/currencies/components/DisplayCurrencySelector'
import NavBarThemeButton from '@/components/NavBarThemeButton'
import AddExpenseModal from '@/features/expenses/components/AddExpenseModal'
import NotificationBell from '@/features/notifications/components/NotificationBell'

type Props = {
  onOpenMobileNav: () => void
}

function pageTitleKey(pathname: string): string {
  if (pathname.startsWith('/expenses')) return 'nav.expenses'
  if (pathname.startsWith('/families')) return 'nav.families'
  if (pathname.startsWith('/admin')) return 'nav.admin'
  if (pathname.startsWith('/settings') || pathname.startsWith('/change-password')) return 'nav.settings'
  if (pathname.startsWith('/notifications')) return 'notifications.title'
  if (pathname.startsWith('/recurring-expenses')) return 'recurringExpenses.title'
  return 'nav.dashboard'
}

export default function AppHeader({ onOpenMobileNav }: Readonly<Props>) {
  const { t } = useTranslation()
  const { pathname } = useLocation()
  const queryClient = useQueryClient()
  const [addExpenseOpen, setAddExpenseOpen] = useState(false)

  const showContextSelectors = pathname.startsWith('/dashboard') || pathname.startsWith('/expenses')

  return (
    <>
      <header className="sticky top-0 z-30 bg-surface-card border-b border-surface-border">
        <div className="h-14 flex items-center gap-3 px-4 sm:px-6">
          <button
            className="md:hidden p-2 -ml-2 rounded-lg text-ink-mute hover:bg-surface-subtle transition-colors duration-150 cursor-pointer"
            onClick={onOpenMobileNav}
            aria-label={t('nav.toggleMenu')}
          >
            <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
              <path strokeLinecap="round" strokeLinejoin="round" d="M4 6h16M4 12h16M4 18h16" />
            </svg>
          </button>

          <h1 className="text-base font-semibold text-ink tracking-tight">{t(pageTitleKey(pathname))}</h1>

          <div className="ml-auto flex items-center gap-2">
            {showContextSelectors && <FamilySelector />}
            {showContextSelectors && <DisplayCurrencySelector />}

            <button
              onClick={() => setAddExpenseOpen(true)}
              aria-label={t('nav.addExpense')}
              title={t('nav.addExpenseTooltip')}
              className="h-10 w-10 rounded-lg bg-brand-500 hover:bg-brand-600 text-white flex items-center justify-center transition-colors duration-150"
            >
              <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5} aria-hidden="true">
                <path strokeLinecap="round" strokeLinejoin="round" d="M12 4v16m8-8H4" />
              </svg>
            </button>

            <NotificationBell />
            <NavBarThemeButton />
          </div>
        </div>
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
