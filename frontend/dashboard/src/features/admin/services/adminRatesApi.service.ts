import { get, post, put } from '@/services/api.service'
import type { ApiResponse } from '@/types/api.type'

const BASE = '/api/expenses/admin/rates'

export type RateDto = {
  id: number
  sourceCurrencyId: number
  destinationCurrencyId: number
  rate: number
  date: string
  rateSource: string
}

export type PagedRatesResponse = {
  rates: RateDto[]
  total: number
  page: number
  pageSize: number
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

export function getRateHistory(
  srcId?: number,
  dstId?: number,
  page = 1,
  pageSize = 50
): Promise<ApiResponse<PagedRatesResponse>> {
  const params = new URLSearchParams()
  if (srcId != null) params.set('sourceCurrencyId', String(srcId))
  if (dstId != null) params.set('destinationCurrencyId', String(dstId))
  params.set('page', String(page))
  params.set('pageSize', String(pageSize))
  return get<PagedRatesResponse>(`${BASE}/history?${params.toString()}`)
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

export function refreshRates(from: string, to?: string, sourceCurrencyId?: number, destinationCurrencyId?: number): Promise<ApiResponse<void>> {
  return post<void>(`${BASE}/refresh`, { from, to, sourceCurrencyId, destinationCurrencyId })
}
