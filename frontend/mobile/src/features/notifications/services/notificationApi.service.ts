import { get, post } from '@/services/api.service'
import type { AppNotification } from '@/features/notifications/types/notification.type'

const BASE = '/api/notifications'

export function getNotifications(page = 1, pageSize = 20) {
  return get<AppNotification[]>(`${BASE}?page=${page}&pageSize=${pageSize}`)
}

export function getUnreadCount() {
  return get<{ count: number }>(`${BASE}/unread-count`)
}

export function markAsRead(id: number) {
  return post<void>(`${BASE}/${id}/read`, {})
}

export function markAllAsRead() {
  return post<void>(`${BASE}/read-all`, {})
}

export function registerPushToken(token: string, deviceId?: string) {
  return post<void>(`${BASE}/push-token`, { token, deviceId })
}
