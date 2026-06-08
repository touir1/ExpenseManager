import { post } from '@/services/api.service'
import type { ApiResponse } from '@/types/api.type'

const EXPENSES_BASE = '/api/expenses'

export interface RefreshRatesParams {
  from: string
  sourceCurrencyId?: number
  destinationCurrencyId?: number
}

export function refreshRates(params: RefreshRatesParams): Promise<ApiResponse<void>> {
  return post<void>(`${EXPENSES_BASE}/rates/refresh`, params)
}
