import type { TFunction } from 'i18next'
import type { AppNotification } from '@/features/notifications/types/notification.type'

export function getNotificationText(n: AppNotification, t: TFunction): string {
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

type IconDef = { bg: string; color: string; path: string }

const ICONS: Record<string, IconDef> = {
  FAMILY_MEMBER_REMOVED: {
    bg: 'bg-berry-soft',
    color: 'text-berry',
    path: 'M18 6L6 18M6 6l12 12',
  },
  FAMILY_INVITATION_ACCEPTED: {
    bg: 'bg-sage-soft',
    color: 'text-sage',
    path: 'M5 13l4 4L19 7',
  },
  FAMILY_MEMBER_JOINED: {
    bg: 'bg-sage-soft',
    color: 'text-sage',
    path: 'M12 4v16m8-8H4',
  },
  FAMILY_EXPENSE_ADDED: {
    bg: 'bg-brand-100',
    color: 'text-brand-600',
    path: 'M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1',
  },
  FAMILY_EXPENSE_DELETED: {
    bg: 'bg-brand-100',
    color: 'text-brand-600',
    path: 'M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16',
  },
  CSV_IMPORT_COMPLETED: {
    bg: 'bg-brand-100',
    color: 'text-brand-600',
    path: 'M9 17V7m6 10V7M5 5h14a2 2 0 012 2v10a2 2 0 01-2 2H5a2 2 0 01-2-2V7a2 2 0 012-2z',
  },
  RATE_CONFLICT_CREATED: {
    bg: 'bg-berry-soft',
    color: 'text-berry',
    path: 'M8 7h12m0 0l-4-4m4 4l-4 4M16 17H4m0 0l4 4m-4-4l4-4',
  },
}

const DEFAULT_ICON: IconDef = {
  bg: 'bg-surface-subtle',
  color: 'text-ink-mute',
  path: 'M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9',
}

export function getNotificationIcon(type: string): JSX.Element {
  const def = ICONS[type] ?? DEFAULT_ICON
  return (
    <span
      data-testid={`notification-icon-${type}`}
      className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-full ${def.bg}`}
    >
      <svg className={`h-4 w-4 ${def.color}`} fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
        <path strokeLinecap="round" strokeLinejoin="round" d={def.path} />
      </svg>
    </span>
  )
}
