import { get, post, del } from '@/services/api.service'
import type { ApiResponse } from '@/types/api.type'
import type { Tag, TagList } from '@/features/tags/types/tag.type'

const BASE = '/api/expenses/tags'

export function getTags(): Promise<ApiResponse<TagList>> {
  return get<TagList>(BASE)
}

export function useTag(name: string): Promise<ApiResponse<Tag>> {
  return post<Tag>(BASE, { name })
}

export function removeTag(id: number): Promise<ApiResponse<void>> {
  return del<void>(`${BASE}/${id}`)
}
