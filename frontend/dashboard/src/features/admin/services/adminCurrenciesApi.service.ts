import { post, put, del, get } from '@/services/api.service'
import type { ApiResponse } from '@/types/api.type'

const BASE = '/api/expenses/admin/currencies'

export type AdminCurrencyCreated = {
  id: number
  code: string
  name: string
  symbol: string
  decimals: number
}

export type CurrencyDto = AdminCurrencyCreated

export type CurrencyDefaultRateDto = {
  destinationCurrencyId: number
  destinationCode: string
  defaultRate?: number
  lastAutoRate?: number
  lastAutoRateDate?: string
}

export function addCurrency(
  code: string,
  name: string,
  symbol: string,
  decimals = 2
): Promise<ApiResponse<AdminCurrencyCreated>> {
  return post<AdminCurrencyCreated>(BASE, { code, name, symbol, decimals })
}

export function updateCurrency(
  id: number,
  name: string,
  symbol: string,
  decimals: number
): Promise<ApiResponse<CurrencyDto>> {
  return put<CurrencyDto>(`${BASE}/${id}`, { name, symbol, decimals })
}

export function deleteCurrency(id: number): Promise<ApiResponse<void>> {
  return del<void>(`${BASE}/${id}`)
}

export function getCurrencyDefaults(id: number): Promise<ApiResponse<CurrencyDefaultRateDto[]>> {
  return get<CurrencyDefaultRateDto[]>(`${BASE}/${id}/defaults`)
}

export function setDefaultRate(
  sourceCurrencyId: number,
  destinationCurrencyId: number,
  rate: number
): Promise<ApiResponse<void>> {
  return post<void>(`${BASE}/defaults`, { sourceCurrencyId, destinationCurrencyId, rate })
}
