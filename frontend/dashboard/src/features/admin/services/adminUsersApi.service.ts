import { get, patch, put } from '@/services/api.service'
import type { ApiResponse } from '@/types/api.type'

const BASE = '/api/users/admin/users'

export type AdminUser = {
  id: number
  email: string
  firstName: string
  lastName: string
  isDisabled: boolean
  isDeleted: boolean
  isEmailValidated: boolean
  createdAt: string
  roles: { id: number; code: string; name: string }[]
}

export type AdminRole = {
  id: number
  code: string
  name: string
  description?: string
  application?: { id: number; code: string; name: string }
}

export type PagedUsersResponse = {
  users: AdminUser[]
  total: number
  page: number
  pageSize: number
}

export function getUsers(search?: string, page = 1, pageSize = 20): Promise<ApiResponse<PagedUsersResponse>> {
  const params = new URLSearchParams()
  if (search) params.set('search', search)
  params.set('page', String(page))
  params.set('pageSize', String(pageSize))
  return get<PagedUsersResponse>(`${BASE}?${params}`)
}

export function getRoles(): Promise<ApiResponse<AdminRole[]>> {
  return get<AdminRole[]>(`${BASE}/roles`)
}

export function disableUser(id: number): Promise<ApiResponse<void>> {
  return patch<void>(`${BASE}/${id}/disable`, {})
}

export function enableUser(id: number): Promise<ApiResponse<void>> {
  return patch<void>(`${BASE}/${id}/enable`, {})
}

export function setUserRoles(id: number, roleIds: number[]): Promise<ApiResponse<void>> {
  return put<void>(`${BASE}/${id}/roles`, { roleIds })
}
