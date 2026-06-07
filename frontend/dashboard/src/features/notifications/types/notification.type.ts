export type NotificationPayloadFamilyMemberRemoved = {
  type: 'FAMILY_MEMBER_REMOVED'
  familyId: number
  familyName: string
  removedByUserId: number
  removedByName: string
  expenseCount: number
}

export type NotificationPayloadFamilyInvitationAccepted = {
  type: 'FAMILY_INVITATION_ACCEPTED'
  familyId: number
  familyName: string
  acceptorName: string
  acceptorEmail: string
}

export type NotificationPayloadFamilyMemberJoined = {
  type: 'FAMILY_MEMBER_JOINED'
  familyId: number
  familyName: string
  joinerName: string
  joinerUserId: number
}

export type NotificationPayloadFamilyExpense = {
  type: 'FAMILY_EXPENSE_ADDED' | 'FAMILY_EXPENSE_DELETED'
  familyId: number
  familyName: string
  expenseId: number
  amount: number
  currencyCode: string
  actorName: string
  actorUserId: number
}

export type NotificationPayloadCsvImportCompleted = {
  type: 'CSV_IMPORT_COMPLETED'
  totalRows: number
  importedCount: number
  skippedCount: number
}

export type NotificationPayloadRateConflictCreated = {
  type: 'RATE_CONFLICT_CREATED'
  conflictId: number
  sourceCurrencyCode: string
  destCurrencyCode: string
  date: string
  autoRate: number
  manualRate: number
}

export type NotificationPayload =
  | NotificationPayloadFamilyMemberRemoved
  | NotificationPayloadFamilyInvitationAccepted
  | NotificationPayloadFamilyMemberJoined
  | NotificationPayloadFamilyExpense
  | NotificationPayloadCsvImportCompleted
  | NotificationPayloadRateConflictCreated

export type AppNotification = {
  id: number
  type: string
  payload: NotificationPayload
  isRead: boolean
  createdAt: string
  readAt: string | null
}
