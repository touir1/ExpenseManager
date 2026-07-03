import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { usePageTitle } from '@/hooks/usePageTitle'
import { useNotifications } from '@/features/notifications/NotificationContext'
import { getNotifications } from '@/features/notifications/services/notificationApi.service'
import { getNotificationText, getNotificationIcon } from '@/features/notifications/notificationDisplay'
import type { AppNotification } from '@/features/notifications/types/notification.type'

const PAGE_SIZE = 20

export default function NotificationsPage() {
  const { t } = useTranslation()
  usePageTitle(t('notifications.pageTitle'))
  const { markRead, markAllRead, unreadCount } = useNotifications()

  const [page, setPage] = useState(1)
  const [items, setItems] = useState<AppNotification[]>([])
  const [hasMore, setHasMore] = useState(false)
  const [isLoading, setIsLoading] = useState(true)

  const load = useCallback(async (p: number) => {
    setIsLoading(true)
    const res = await getNotifications(p, PAGE_SIZE)
    if (res.ok && res.data) {
      setItems(res.data)
      setHasMore(res.data.length === PAGE_SIZE)
    }
    setIsLoading(false)
  }, [])

  useEffect(() => {
    load(page)
  }, [page, load])

  async function handleMarkRead(n: AppNotification) {
    if (n.isRead) return
    await markRead(n.id)
    setItems(prev => prev.map(i => (i.id === n.id ? { ...i, isRead: true } : i)))
  }

  async function handleMarkAllRead() {
    await markAllRead()
    setItems(prev => prev.map(i => ({ ...i, isRead: true })))
  }

  return (
    <div className="max-w-3xl mx-auto w-full px-4 sm:px-6 py-8">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-semibold text-ink tracking-tight">{t('notifications.pageTitle')}</h1>
          <p className="text-sm text-ink-mute mt-1">{t('notifications.pageSubtitle')}</p>
        </div>
        {unreadCount > 0 && (
          <button
            onClick={handleMarkAllRead}
            className="text-sm text-brand-600 hover:text-brand-700 font-medium cursor-pointer shrink-0"
          >
            {t('notifications.markAllRead')}
          </button>
        )}
      </div>

      <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card overflow-hidden">
        {isLoading ? (
          <div className="px-4 py-8 text-sm text-ink-mute text-center">…</div>
        ) : items.length === 0 ? (
          <div className="px-4 py-12 text-center">
            <p className="text-sm font-semibold text-ink mb-1">{t('notifications.emptyTitle')}</p>
            <p className="text-xs text-ink-mute">{t('notifications.empty')}</p>
          </div>
        ) : (
          items.map(n => (
            <button
              key={n.id}
              onClick={() => handleMarkRead(n)}
              className={`w-full flex items-start gap-3 text-left px-4 py-3.5 border-b border-surface-border last:border-b-0 hover:bg-surface-subtle transition-colors duration-100 cursor-pointer ${!n.isRead ? 'bg-brand-soft' : ''}`}
            >
              {getNotificationIcon(n.type)}
              <span className="min-w-0 flex-1">
                <p className={`text-sm ${!n.isRead ? 'font-semibold text-ink' : 'text-ink-body'}`}>
                  {getNotificationText(n, t)}
                </p>
                <p className="text-xs text-ink-mute mt-0.5">
                  {new Date(n.createdAt).toLocaleString()}
                </p>
              </span>
            </button>
          ))
        )}
      </div>

      {(page > 1 || hasMore) && (
        <div className="flex items-center justify-between mt-4">
          <button
            onClick={() => setPage(p => Math.max(1, p - 1))}
            disabled={page <= 1}
            className="text-sm font-medium text-ink-mute border border-surface-border hover:bg-surface-hover disabled:opacity-40 disabled:cursor-not-allowed px-3 py-1.5 rounded-lg transition-colors duration-150"
          >
            {t('notifications.prevPage')}
          </button>
          <span className="text-xs text-ink-mute">{t('notifications.pageLabel', { page })}</span>
          <button
            onClick={() => setPage(p => p + 1)}
            disabled={!hasMore}
            className="text-sm font-medium text-ink-mute border border-surface-border hover:bg-surface-hover disabled:opacity-40 disabled:cursor-not-allowed px-3 py-1.5 rounded-lg transition-colors duration-150"
          >
            {t('notifications.nextPage')}
          </button>
        </div>
      )}
    </div>
  )
}
