import { API_ERRORS, BACKEND_ERROR_CODES } from '@/constants/apiErrors.constant'
import type { ApiResponse } from '@/types/api.type'

const API_BASE = (import.meta.env.VITE_API_BASE ?? '').replace(/\/$/, '')

let unauthorizedHandler: (() => void) | null = null
let errorHandler: ((message: string) => void) | null = null

export function onUnauthorized(handler: (() => void) | null) {
  unauthorizedHandler = handler
}

export function onError(handler: ((message: string) => void) | null) {
  errorHandler = handler
}

async function parseJsonSafe(res: Response) {
  try { return await res.json() } catch { return undefined }
}

function getErrorMessage(status: number, data: any, statusText: string): string {
  if (status >= 500) return API_ERRORS.SERVER
  if (status === 429) return API_ERRORS.RATE_LIMIT
  if (status === 404) return API_ERRORS.NOT_FOUND
  if (status === 403) return API_ERRORS.FORBIDDEN
  const backendCode: string | undefined = data?.message ?? data?.Message ?? data?.error
  if (backendCode && BACKEND_ERROR_CODES[backendCode]) return BACKEND_ERROR_CODES[backendCode]
  if (status === 400) return API_ERRORS.BAD_REQUEST
  return backendCode || statusText || 'Request failed'
}

let refreshInFlight: Promise<boolean> | null = null

async function attemptTokenRefresh(): Promise<boolean> {
  refreshInFlight ??= fetch(`${API_BASE}/api/users/auth/refresh`, {
    method: 'POST',
    credentials: 'include',
  })
    .then(r => r.ok)
    .catch(() => false)
    .finally(() => { refreshInFlight = null })
  return refreshInFlight
}

function redirectToLogin(): void {
  if (unauthorizedHandler) unauthorizedHandler()
  else globalThis.location.assign('/login')
}

function buildErrorResponse<T>(status: number, data: unknown, statusText: string, silent = false): ApiResponse<T> {
  const msg = getErrorMessage(status, data, statusText)
  if (errorHandler && !silent) errorHandler(msg)
  return { ok: false, status, error: msg }
}

async function retryRequest<T>(url: string, init: RequestInit, headers: Record<string, string>): Promise<ApiResponse<T>> {
  const retryRes = await fetch(url, { ...init, headers, credentials: 'include' })
  const retryStatus = retryRes.status
  const retryData = await parseJsonSafe(retryRes)
  if (retryStatus >= 200 && retryStatus < 300) return { ok: true, status: retryStatus, data: retryData as T }
  if (retryStatus === 401) {
    redirectToLogin()
    return { ok: false, status: retryStatus, error: API_ERRORS.UNAUTHORIZED }
  }
  return buildErrorResponse(retryStatus, retryData, retryRes.statusText)
}

export async function request<T>(path: string, init: RequestInit = {}, opts: { skipUnauthorized?: boolean; silent?: boolean } = {}): Promise<ApiResponse<T>> {
  const headers: Record<string, string> = { ...(init.headers as any) }
  if (init.body && !headers['Content-Type']) headers['Content-Type'] = 'application/json'

  const url = `${API_BASE}${path}`
  try {
    const res = await fetch(url, { ...init, headers, credentials: 'include' })
    const status = res.status
    const data = await parseJsonSafe(res)

    if (status === 401 && !opts.skipUnauthorized) {
      const refreshed = await attemptTokenRefresh()
      if (refreshed) return retryRequest<T>(url, init, headers)
      redirectToLogin()
      return { ok: false, status, error: API_ERRORS.UNAUTHORIZED }
    }

    if (!res.ok) return buildErrorResponse(status, data, res.statusText, opts.silent)
    return { ok: true, status, data }
  } catch {
    const msg = API_ERRORS.NETWORK
    if (errorHandler) errorHandler(msg)
    return { ok: false, status: 0, error: msg }
  }
}

export async function get<T>(path: string, opts: { skipUnauthorized?: boolean; silent?: boolean } = {}): Promise<ApiResponse<T>> {
  return request<T>(path, { method: 'GET' }, opts)
}

export async function post<T>(path: string, body: unknown, opts: { skipUnauthorized?: boolean; silent?: boolean } = {}): Promise<ApiResponse<T>> {
  return request<T>(path, { method: 'POST', body: JSON.stringify(body) }, opts)
}

export async function put<T>(path: string, body: unknown): Promise<ApiResponse<T>> {
  return request<T>(path, { method: 'PUT', body: JSON.stringify(body) })
}

export async function del<T>(path: string): Promise<ApiResponse<T>> {
  return request<T>(path, { method: 'DELETE' })
}
