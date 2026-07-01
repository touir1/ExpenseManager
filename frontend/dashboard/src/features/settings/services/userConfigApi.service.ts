import { get, put, del } from '@/services/api.service'
import type { ApiResponse } from '@/types/api.type'
import type { UserConfigDto, UpdateUserConfigRequest } from '@/features/settings/types/userConfig.type'

const BASE = '/api/expenses/config'

export function getConfig(): Promise<ApiResponse<UserConfigDto>> {
  return get<UserConfigDto>(BASE)
}

export function updateConfig(req: UpdateUserConfigRequest): Promise<ApiResponse<UserConfigDto>> {
  return put<UserConfigDto>(BASE, req)
}

export function updateDefaultCsvColumnMapping(mapping: Record<string, string>): Promise<ApiResponse<UserConfigDto>> {
  return put<UserConfigDto>(`${BASE}/csv-column-mapping`, { mapping })
}

export function clearDefaultCsvColumnMapping(): Promise<ApiResponse<UserConfigDto>> {
  return del<UserConfigDto>(`${BASE}/csv-column-mapping`)
}
