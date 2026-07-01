import { useEffect, useRef, useState } from 'react'
import { useTranslation, TFunction } from 'react-i18next'
import { useNotifications } from '@/features/notifications/NotificationContext'
import { useToast } from '@/components/Toast'
import type { AppNotification } from '@/features/notifications/types/notification.type'

function getNotificationText(n: AppNotification, t: TFunction): string {
  const p = n.payload as Record<string, unknown>
  switch (n.type) {
    case 'FAMILY_MEMBER_REMOVED':
      return t('notifications.familyMemberRemoved', {
        removedByName: p.removedByName,
        familyName: p.familyName,
        expenseCount: p.expenseCount,
      })
    case 'FAMILY_INVITATION_ACCEPTED':
      return t('notifications.familyInvitationAccepted', {
        acceptorName: p.acceptorName,
        familyName: p.familyName,
      })
    case 'FAMILY_MEMBER_JOINED':
      return t('notifications.familyMemberJoined', {
        joinerName: p.joinerName,
        familyName: p.familyName,
      })
    case 'FAMILY_EXPENSE_ADDED':
      return t('notifications.familyExpenseAdded', {
        actorName: p.actorName,
        amount: p.amount,
        currencyCode: p.currencyCode,
        familyName: p.familyName,
      })
    case 'FAMILY_EXPENSE_DELETED':
      return t('notifications.familyExpenseDeleted', {
        actorName: p.actorName,
        amount: p.amount,
        currencyCode: p.currencyCode,
        familyName: p.familyName,
      })
    case 'CSV_IMPORT_COMPLETED':
      return t('notifications.csvImportCompleted', {
        importedCount: p.importedCount,
        totalRows: p.totalRows,
        skippedCount: p.skippedCount,
      })
    case 'RATE_CONFLICT_CREATED':
      return t('notifications.rateConflictCreated', {
        sourceCurrencyCode: p.sourceCurrencyCode,
        destCurrencyCode: p.destCurrencyCode,
      })
    default:
      return n.type
  }
}

export default function NotificationBell() {
  const { t } = useTranslation()
  const { show } = useToast()
  const { notifications, unreadCount, isLoading, markRead, markAllRead } = useNotifications()
  const [open, setOpen] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)
  const prevCountRef = useRef(unreadCount)

  // Toast on new real-time notification
  useEffect(() => {
    if (unreadCount > prevCountRef.current && notifications[0]) {
      show(getNotificationText(notifications[0], t), 'info')
    }
    prevCountRef.current = unreadCount
  }, [unreadCount, notifications, show, t])

  // Outside-click closes dropdown
  useEffect(() => {
    if (!open) return
    const handler = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setOpen(false)
      }
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [open])

  const badgeLabel = unreadCount > 9 ? '9+' : String(unreadCount)

  return (
    <div className="relative" ref={containerRef}>
      <button
        onClick={() => setOpen(o => !o)}
        aria-label={t('nav.notifications')}
        aria-expanded={open}
        className="relative h-8 w-8 rounded-lg text-ink-mute hover:text-ink hover:bg-surface-subtle flex items-center justify-center transition-colors duration-150 cursor-pointer"
      >
        <svg className="h-4.5 w-4.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
          <path strokeLinecap="round" strokeLinejoin="round" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
        </svg>
        {unreadCount > 0 && (
          <span
            aria-label={t('notifications.badge', { count: unreadCount })}
            className="absolute -top-0.5 -right-0.5 min-w-[16px] h-4 px-0.5 rounded-full bg-red-500 text-white text-[10px] font-bold flex items-center justify-center leading-none"
          >
            {badgeLabel}
          </span>
        )}
      </button>

      {open && (
        <div
          className="absolute right-0 top-full mt-2 w-80 bg-surface-card border border-surface-border rounded-2xl z-50 overflow-hidden"
          style={{ boxShadow: '0 8px 20px -10px rgba(30,20,10,0.5)' }}
        >
          {/* Header */}
          <div className="flex items-center justify-between px-3 py-2.5 border-b border-surface-border">
            <span className="text-sm font-semibold text-ink">{t('notifications.title')}</span>
            {unreadCount > 0 && (
              <button
                onClick={() => markAllRead()}
                className="text-xs text-brand-600 hover:text-brand-700 font-medium cursor-pointer"
              >
                {t('notifications.markAllRead')}
              </button>
            )}
          </div>

          {/* List */}
          <div className="max-h-80 overflow-y-auto">
            {isLoading && notifications.length === 0 ? (
              <div className="px-3 py-4 text-sm text-ink-mute text-center">…</div>
            ) : notifications.length === 0 ? (
              <div className="px-3 py-4 text-sm text-ink-mute text-center">
                {t('notifications.empty')}
              </div>
            ) : (
              notifications.map(n => (
                <button
                  key={n.id}
                  onClick={() => { markRead(n.id); setOpen(false) }}
                  className={`w-full text-left px-3 py-2.5 border-b border-surface-border last:border-b-0 hover:bg-surface-subtle transition-colors duration-100 cursor-pointer ${!n.isRead ? 'bg-brand-soft' : ''}`}
                >
                  <p className={`text-sm ${!n.isRead ? 'font-semibold text-ink' : 'text-ink-body'}`}>
                    {getNotificationText(n, t)}
                  </p>
                  <p className="text-xs text-ink-mute mt-0.5">
                    {new Date(n.createdAt).toLocaleString()}
                  </p>
                </button>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  )
}
