import { get, put } from '@/services/api.service'
import type { ApiResponse } from '@/types/api.type'
import type { NotificationPreferenceDto } from '@/features/settings/types/userConfig.type'

const BASE = '/api/users/config/notifications'

export function getNotificationPreferences(): Promise<ApiResponse<NotificationPreferenceDto[]>> {
  return get<NotificationPreferenceDto[]>(BASE)
}

export function updateNotificationPreferences(
  preferences: NotificationPreferenceDto[]
): Promise<ApiResponse<NotificationPreferenceDto[]>> {
  return put<NotificationPreferenceDto[]>(BASE, { preferences })
}
