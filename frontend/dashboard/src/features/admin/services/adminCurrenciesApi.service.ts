import { post } from '@/services/api.service'
import type { ApiResponse } from '@/types/api.type'

const BASE = '/api/expenses/admin/currencies'

export type AdminCurrencyCreated = {
  id: number
  code: string
  name: string
  symbol: string
  decimals: number
}

export function addCurrency(
  code: string,
  name: string,
  symbol: string,
  decimals = 2
): Promise<ApiResponse<AdminCurrencyCreated>> {
  return post<AdminCurrencyCreated>(BASE, { code, name, symbol, decimals })
}

export function setDefaultRate(
  sourceCurrencyId: number,
  destinationCurrencyId: number,
  rate: number
): Promise<ApiResponse<void>> {
  return post<void>(`${BASE}/defaults`, { sourceCurrencyId, destinationCurrencyId, rate })
}
