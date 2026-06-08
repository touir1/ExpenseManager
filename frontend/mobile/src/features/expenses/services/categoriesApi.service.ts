import { get } from '@/services/api.service'
import type { ApiResponse } from '@/types/api.type'
import type { Category } from '@/features/expenses/types/expenses.type'

const EXPENSES_BASE = '/api/expenses'

export function getCategories(): Promise<ApiResponse<Category[]>> {
  return get<Category[]>(`${EXPENSES_BASE}/categories`)
}
