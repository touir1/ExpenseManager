import { get, post, put } from '@/services/api.service'
import type { ApiResponse } from '@/types/api.type'

const BASE = '/api/expenses/admin/categories'

export type AdminSubcategory = {
  id: number
  name: string
  description?: string
  isArchived: boolean
}

export type AdminCategory = {
  id: number
  name: string
  description?: string
  isArchived: boolean
  subcategories: AdminSubcategory[]
}

export function getCategories(): Promise<ApiResponse<AdminCategory[]>> {
  return get<AdminCategory[]>(BASE)
}

export function addCategory(name: string, description?: string): Promise<ApiResponse<AdminCategory>> {
  return post<AdminCategory>(BASE, { name, description })
}

export function updateCategory(id: number, name: string, description?: string): Promise<ApiResponse<AdminCategory>> {
  return put<AdminCategory>(`${BASE}/${id}`, { name, description })
}

export function archiveCategory(id: number): Promise<ApiResponse<void>> {
  return post<void>(`${BASE}/${id}/archive`, {})
}

export function unarchiveCategory(id: number): Promise<ApiResponse<void>> {
  return post<void>(`${BASE}/${id}/unarchive`, {})
}

export function addSubcategory(categoryId: number, name: string, description?: string): Promise<ApiResponse<AdminSubcategory>> {
  return post<AdminSubcategory>(`${BASE}/${categoryId}/subcategories`, { name, description })
}

export function updateSubcategory(categoryId: number, subId: number, name: string, description?: string): Promise<ApiResponse<AdminSubcategory>> {
  return put<AdminSubcategory>(`${BASE}/${categoryId}/subcategories/${subId}`, { name, description })
}

export function archiveSubcategory(categoryId: number, subId: number): Promise<ApiResponse<void>> {
  return post<void>(`${BASE}/${categoryId}/subcategories/${subId}/archive`, {})
}

export function unarchiveSubcategory(categoryId: number, subId: number): Promise<ApiResponse<void>> {
  return post<void>(`${BASE}/${categoryId}/subcategories/${subId}/unarchive`, {})
}
