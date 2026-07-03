import { IonIcon } from '@ionic/react'
import {
  personRemoveOutline,
  checkmarkCircleOutline,
  personAddOutline,
  cashOutline,
  trashOutline,
  documentTextOutline,
  swapHorizontalOutline,
  notificationsOutline,
} from 'ionicons/icons'
import type { useTranslation } from 'react-i18next'
import type { AppNotification } from '@/features/notifications/types/notification.type'

export function getNotificationText(n: AppNotification, t: ReturnType<typeof useTranslation>['t']): string {
  const p = n.payload as any
  switch (n.type) {
    case 'FAMILY_MEMBER_REMOVED':
      return t('notifications.familyMemberRemoved', { removedByName: p.removedByName, familyName: p.familyName, expenseCount: p.expenseCount })
    case 'FAMILY_INVITATION_ACCEPTED':
      return t('notifications.familyInvitationAccepted', { acceptorName: p.acceptorName, familyName: p.familyName })
    case 'FAMILY_MEMBER_JOINED':
      return t('notifications.familyMemberJoined', { joinerName: p.joinerName, familyName: p.familyName })
    case 'FAMILY_EXPENSE_ADDED':
      return t('notifications.familyExpenseAdded', { actorName: p.actorName, amount: p.amount, currencyCode: p.currencyCode, familyName: p.familyName })
    case 'FAMILY_EXPENSE_DELETED':
      return t('notifications.familyExpenseDeleted', { actorName: p.actorName, amount: p.amount, currencyCode: p.currencyCode, familyName: p.familyName })
    case 'CSV_IMPORT_COMPLETED':
      return t('notifications.csvImportCompleted', { importedCount: p.importedCount, totalRows: p.totalRows, skippedCount: p.skippedCount })
    case 'RATE_CONFLICT_CREATED':
      return t('notifications.rateConflictCreated', { sourceCurrencyCode: p.sourceCurrencyCode, destCurrencyCode: p.destCurrencyCode })
    default:
      return n.type
  }
}

type IconDef = { icon: string; color: string }

const ICONS: Record<string, IconDef> = {
  FAMILY_MEMBER_REMOVED: { icon: personRemoveOutline, color: 'danger' },
  FAMILY_INVITATION_ACCEPTED: { icon: checkmarkCircleOutline, color: 'success' },
  FAMILY_MEMBER_JOINED: { icon: personAddOutline, color: 'success' },
  FAMILY_EXPENSE_ADDED: { icon: cashOutline, color: 'primary' },
  FAMILY_EXPENSE_DELETED: { icon: trashOutline, color: 'primary' },
  CSV_IMPORT_COMPLETED: { icon: documentTextOutline, color: 'primary' },
  RATE_CONFLICT_CREATED: { icon: swapHorizontalOutline, color: 'danger' },
}

const DEFAULT_ICON: IconDef = { icon: notificationsOutline, color: 'medium' }

export function getNotificationIcon(type: string) {
  const def = ICONS[type] ?? DEFAULT_ICON
  return (
    <IonIcon
      data-testid={`notification-icon-${type}`}
      icon={def.icon}
      color={def.color}
      style={{ fontSize: 18 }}
      slot="start"
    />
  )
}
