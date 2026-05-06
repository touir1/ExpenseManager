import { get } from '@/services/api.service'
import type { ApiResponse } from '@/types/api.type'
import type { Currency } from '@/features/expenses/types/expenses.type'

const EXPENSES_BASE = '/api/expenses'

export function getCurrencies(): Promise<ApiResponse<Currency[]>> {
  return get<Currency[]>(`${EXPENSES_BASE}/currencies`)
}
