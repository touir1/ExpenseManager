import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from 'react'
import type { HubConnection } from '@microsoft/signalr'
import { useAuth } from '@/features/auth/AuthContext'
import {
  getNotifications,
  getUnreadCount,
  markAsRead,
  markAllAsRead,
  registerPushToken,
} from '@/features/notifications/services/notificationApi.service'
import type { AppNotification } from '@/features/notifications/types/notification.type'

type NotificationContextValue = {
  notifications: AppNotification[]
  unreadCount: number
  isLoading: boolean
  markRead: (id: number) => Promise<void>
  markAllRead: () => Promise<void>
  refresh: () => Promise<void>
}

const NotificationContext = createContext<NotificationContextValue | undefined>(undefined)

const API_BASE = (import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/$/, '')

async function setupPushNotifications() {
  try {
    const { PushNotifications } = await import('@capacitor/push-notifications')
    const permResult = await PushNotifications.requestPermissions()
    if (permResult.receive !== 'granted') return

    await PushNotifications.register()

    await PushNotifications.addListener('registration', async ({ value: token }) => {
      await registerPushToken(token)
    })
  } catch {
    // Capacitor push not available in web preview — ignore
  }
}

export function NotificationProvider({ children }: Readonly<{ children: ReactNode }>) {
  const { isAuthenticated } = useAuth()
  const [notifications, setNotifications] = useState<AppNotification[]>([])
  const [unreadCount, setUnreadCount] = useState(0)
  const [isLoading, setIsLoading] = useState(false)
  const connRef = useRef<HubConnection | null>(null)

  const load = useCallback(async () => {
    setIsLoading(true)
    const [listRes, countRes] = await Promise.all([getNotifications(1, 20), getUnreadCount()])
    if (listRes.ok && listRes.data) setNotifications(listRes.data)
    if (countRes.ok && countRes.data) setUnreadCount(countRes.data.count)
    setIsLoading(false)
  }, [])

  useEffect(() => {
    if (!isAuthenticated) {
      connRef.current?.stop()
      connRef.current = null
      setNotifications([])
      setUnreadCount(0)
      return
    }

    load()
    setupPushNotifications()

    let cancelled = false
    import('@microsoft/signalr').then(({ HubConnectionBuilder, LogLevel }) => {
      if (cancelled) return
      const conn = new HubConnectionBuilder()
        .withUrl(`${API_BASE}/api/notifications/ws/notifications`, { withCredentials: true })
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Warning)
        .build()

      conn.on('notification', (notif: AppNotification) => {
        setNotifications(prev => [notif, ...prev])
        setUnreadCount(c => c + 1)
      })

      conn.start().catch(err => console.error('[SignalR]', err))
      connRef.current = conn
    })

    return () => {
      cancelled = true
      connRef.current?.stop()
      connRef.current = null
    }
  }, [isAuthenticated, load])

  const markRead = useCallback(async (id: number) => {
    await markAsRead(id)
    setNotifications(prev => prev.map(n => (n.id === id ? { ...n, isRead: true } : n)))
    setUnreadCount(c => Math.max(0, c - 1))
  }, [])

  const markAllRead = useCallback(async () => {
    await markAllAsRead()
    setNotifications(prev => prev.map(n => ({ ...n, isRead: true })))
    setUnreadCount(0)
  }, [])

  const value = useMemo<NotificationContextValue>(
    () => ({ notifications, unreadCount, isLoading, markRead, markAllRead, refresh: load }),
    [notifications, unreadCount, isLoading, markRead, markAllRead, load],
  )

  return <NotificationContext.Provider value={value}>{children}</NotificationContext.Provider>
}

export function useNotifications(): NotificationContextValue {
  const ctx = useContext(NotificationContext)
  if (!ctx) throw new Error('useNotifications must be used within NotificationProvider')
  return ctx
}
