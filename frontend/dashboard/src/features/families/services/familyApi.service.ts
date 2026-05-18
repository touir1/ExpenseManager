import { get, post, put, del } from '@/services/api.service'
import type { ApiResponse } from '@/types/api.type'
import type { Family, FamilyDetail } from '@/features/families/types/family.type'

const BASE = '/api/expenses/families'

export function getFamilies(): Promise<ApiResponse<Family[]>> {
  return get<Family[]>(BASE)
}

export function getFamilyById(id: number): Promise<ApiResponse<FamilyDetail>> {
  return get<FamilyDetail>(`${BASE}/${id}`)
}

export function createFamily(name: string): Promise<ApiResponse<Family>> {
  return post<Family>(BASE, { name })
}

export function renameFamily(id: number, name: string): Promise<ApiResponse<Family>> {
  return put<Family>(`${BASE}/${id}/name`, { name })
}

export function archiveFamily(id: number): Promise<ApiResponse<void>> {
  return post<void>(`${BASE}/${id}/archive`, {})
}

export function unarchiveFamily(id: number): Promise<ApiResponse<void>> {
  return post<void>(`${BASE}/${id}/unarchive`, {})
}

export function inviteMember(familyId: number, email: string): Promise<ApiResponse<{ token: string }>> {
  return post<{ token: string }>(`${BASE}/${familyId}/invite`, { email })
}

export function acceptInvite(token: string, opts?: { silent?: boolean }): Promise<ApiResponse<void>> {
  return post<void>(`${BASE}/accept-invite/${token}`, {}, opts)
}

export function removeMember(familyId: number, targetUserId: number): Promise<ApiResponse<void>> {
  return del<void>(`${BASE}/${familyId}/members/${targetUserId}`)
}

export function changeMemberRole(familyId: number, targetUserId: number, role: string): Promise<ApiResponse<void>> {
  return put<void>(`${BASE}/${familyId}/members/${targetUserId}/role`, { role })
}

export function leaveFamily(familyId: number): Promise<ApiResponse<void>> {
  return del<void>(`${BASE}/${familyId}/leave`)
}
