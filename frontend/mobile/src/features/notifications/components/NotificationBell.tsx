import { useState } from 'react'
import {
  IonButton,
  IonIcon,
  IonBadge,
  IonPopover,
  IonList,
  IonItem,
  IonLabel,
  IonText,
} from '@ionic/react'
import { notificationsOutline } from 'ionicons/icons'
import { useTranslation } from 'react-i18next'
import { useNotifications } from '@/features/notifications/NotificationContext'
import type { AppNotification } from '@/features/notifications/types/notification.type'

function getNotificationText(n: AppNotification, t: ReturnType<typeof useTranslation>['t']): string {
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

interface Props {
  slot?: string
}

export function NotificationBell({ slot }: Props) {
  const { t } = useTranslation()
  const { notifications, unreadCount, markAllRead } = useNotifications()
  const [popoverOpen, setPopoverOpen] = useState(false)
  const [popoverEvent, setPopoverEvent] = useState<MouseEvent | null>(null)

  function handleBellClick(e: React.MouseEvent) {
    setPopoverEvent(e.nativeEvent)
    setPopoverOpen(true)
  }

  return (
    <>
      <IonButton slot={slot} fill="clear" color="dark" onClick={handleBellClick} id="notification-bell">
        <IonIcon icon={notificationsOutline} />
        {unreadCount > 0 && (
          <IonBadge color="danger" style={{ position: 'absolute', top: 4, right: 4, fontSize: 10 }}>
            {unreadCount}
          </IonBadge>
        )}
      </IonButton>

      <IonPopover
        isOpen={popoverOpen}
        event={popoverEvent ?? undefined}
        onDidDismiss={() => setPopoverOpen(false)}
        style={{ '--width': '300px' }}
      >
        <div style={{ padding: '8px 16px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <strong>{t('notifications.title', 'Notifications')}</strong>
          {unreadCount > 0 && (
            <IonButton fill="clear" size="small" onClick={markAllRead}>
              {t('notifications.markAllRead', 'Mark all read')}
            </IonButton>
          )}
        </div>
        <IonList style={{ maxHeight: 320, overflowY: 'auto' }}>
          {notifications.length === 0 && (
            <IonItem lines="none">
              <IonText color="medium">{t('notifications.empty', 'No notifications')}</IonText>
            </IonItem>
          )}
          {notifications.slice(0, 10).map(n => (
            <IonItem
              key={n.id}
              lines="full"
              style={{ opacity: n.isRead ? 0.6 : 1 }}
            >
              <IonLabel className="ion-text-wrap">
                <p style={{ fontSize: 13 }}>{getNotificationText(n, t)}</p>
                <p style={{ fontSize: 11, color: 'var(--ion-color-medium)' }}>
                  {new Date(n.createdAt).toLocaleDateString()}
                </p>
              </IonLabel>
              {!n.isRead && <IonBadge slot="end" color="primary" style={{ width: 8, height: 8, borderRadius: '50%' }} />}
            </IonItem>
          ))}
        </IonList>
      </IonPopover>
    </>
  )
}
