type ApiResponse<T> = { ok: boolean; data?: T; status: number; error?: string }

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
  if (status >= 500) return 'Server error, please retry later.'
  if (status === 429) return 'Too many requests. Please wait and try again.'
  if (status === 404) return 'Resource not found.'
  if (status === 403) return 'Access denied. You might not have permission.'
  if (status === 400) return 'Invalid request. Please check your input.'
  return (data?.error || data?.message) || statusText || 'Request failed'
}

export async function request<T>(path: string, init: RequestInit = {}, opts: { skipUnauthorized?: boolean } = {}): Promise<ApiResponse<T>> {
  const headers: Record<string, string> = { ...(init.headers as any) }
  if (init.body && !headers['Content-Type']) headers['Content-Type'] = 'application/json'

  const url = `${API_BASE}${path}`
  try {
    const res = await fetch(url, { ...init, headers, credentials: 'include' })
    const status = res.status
    const data = await parseJsonSafe(res)

    if (status === 401 && !opts.skipUnauthorized) {
      if (unauthorizedHandler) unauthorizedHandler()
      else globalThis.location.assign('/login')
      return { ok: false, status, error: 'Unauthorized' }
    }

    if (!res.ok) {
      const msg = getErrorMessage(status, data, res.statusText)
      if (errorHandler) errorHandler(msg)
      return { ok: false, status, error: msg }
    }

    return { ok: true, status, data }
  } catch {
    const msg = 'Network error. Please check your connection and try again.'
    if (errorHandler) errorHandler(msg)
    return { ok: false, status: 0, error: msg }
  }
}

export async function get<T>(path: string): Promise<ApiResponse<T>> {
  return request<T>(path, { method: 'GET' })
}

export async function post<T>(path: string, body: unknown, opts: { skipUnauthorized?: boolean } = {}): Promise<ApiResponse<T>> {
  return request<T>(path, { method: 'POST', body: JSON.stringify(body) }, opts)
}

export async function put<T>(path: string, body: unknown): Promise<ApiResponse<T>> {
  return request<T>(path, { method: 'PUT', body: JSON.stringify(body) })
}

export async function del<T>(path: string): Promise<ApiResponse<T>> {
  return request<T>(path, { method: 'DELETE' })
}
