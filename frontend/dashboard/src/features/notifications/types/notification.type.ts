export type NotificationPayloadFamilyMemberRemoved = {
  type: 'FAMILY_MEMBER_REMOVED'
  familyId: number
  familyName: string
  removedByUserId: number
  removedByName: string
  expenseCount: number
}

export type NotificationPayload = NotificationPayloadFamilyMemberRemoved

export type AppNotification = {
  id: number
  type: string
  payload: NotificationPayload
  isRead: boolean
  createdAt: string
  readAt: string | null
}
