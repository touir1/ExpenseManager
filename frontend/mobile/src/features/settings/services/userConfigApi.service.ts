import { get, put } from '@/services/api.service'
import type { ApiResponse } from '@/types/api.type'
import type { UserConfigDto, UpdateUserConfigRequest } from '@/features/settings/types/userConfig.type'

const BASE = '/api/expenses/config'

export function getConfig(): Promise<ApiResponse<UserConfigDto>> {
  return get<UserConfigDto>(BASE)
}

export function updateConfig(req: UpdateUserConfigRequest): Promise<ApiResponse<UserConfigDto>> {
  return put<UserConfigDto>(BASE, req)
}
