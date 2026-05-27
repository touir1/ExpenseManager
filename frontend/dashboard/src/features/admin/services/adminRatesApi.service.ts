import { get, post, put } from '@/services/api.service'
import type { ApiResponse } from '@/types/api.type'

const BASE = '/api/expenses/admin/rates'

export type RateDto = {
  id: number
  sourceCurrencyId: number
  destinationCurrencyId: number
  rate: number
  date: string
  rateSourceId: number
}

export type RateConflictDto = {
  id: number
  sourceCurrencyId: number
  destinationCurrencyId: number
  date: string
  autoRate: number
  manualRate: number
  conflictStatusId: number
}

export function getRateHistory(sourceCurrencyId: number, destinationCurrencyId: number): Promise<ApiResponse<RateDto[]>> {
  return get<RateDto[]>(`${BASE}/history?sourceCurrencyId=${sourceCurrencyId}&destinationCurrencyId=${destinationCurrencyId}`)
}

export function addManualRate(sourceCurrencyId: number, destinationCurrencyId: number, date: string, rate: number): Promise<ApiResponse<RateDto>> {
  return post<RateDto>(BASE, { sourceCurrencyId, destinationCurrencyId, date, rate })
}

export function bulkAddRates(rates: { sourceCurrencyId: number; destinationCurrencyId: number; date: string; rate: number }[]): Promise<ApiResponse<void>> {
  return post<void>(`${BASE}/bulk`, { rates })
}

export function getPendingConflicts(): Promise<ApiResponse<RateConflictDto[]>> {
  return get<RateConflictDto[]>(`${BASE}/conflicts/pending`)
}

export function resolveConflict(id: number, resolution: string, customRate?: number): Promise<ApiResponse<void>> {
  return put<void>(`${BASE}/conflicts/${id}/resolve`, { resolution, customRate })
}

export function refreshRates(from: string, sourceCurrencyId?: number, destinationCurrencyId?: number): Promise<ApiResponse<void>> {
  return post<void>(`${BASE}/refresh`, { from, sourceCurrencyId, destinationCurrencyId })
}
