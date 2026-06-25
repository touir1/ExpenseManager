export type FamilyRole = 'Head' | 'Member'

export type FamilyMember = {
  userId: number
  firstName: string
  lastName: string
  email: string
  role: FamilyRole
  joinedAt: string
}

export type Family = {
  id: number
  name: string
  isDefault: boolean
  isArchived: boolean
  userRole: FamilyRole
  createdAt: string
}

export type FamilyDetail = Family & {
  members: FamilyMember[]
}

export type FamilyPendingInvitation = {
  token: string
  inviteeEmail: string
  invitedAt: string
  expiresAt: string
}
