import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { request, get, post, put, del, onUnauthorized, onError } from '@/services/api.service'
import { API_ERRORS } from '@/constants/apiErrors.constant'

const mockFetch = vi.fn()
vi.stubGlobal('fetch', mockFetch)

const mockLocationAssign = vi.fn()
Object.defineProperty(globalThis, 'location', { value: { assign: mockLocationAssign }, writable: true })

function makeResponse(status: number, data?: unknown, statusText = '') {
  return {
    ok: status >= 200 && status < 300,
    status,
    statusText,
    json: () => Promise.resolve(data),
  } as Response
}

function makeThrowingResponse(status: number) {
  return {
    ok: status >= 200 && status < 300,
    status,
    statusText: '',
    json: () => Promise.reject(new Error('Invalid JSON')),
  } as unknown as Response
}

describe('api.service', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    onUnauthorized(null)
    onError(null)
  })

  afterEach(() => {
    onUnauthorized(null)
    onError(null)
  })

  // ── Basic request methods ─────────────────────────────────────────────────

  it('GET returns ok=true and data on 200', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(200, { foo: 'bar' }))
    const result = await get<{ foo: string }>('/test')
    expect(result).toEqual({ ok: true, status: 200, data: { foo: 'bar' } })
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/test'),
      expect.objectContaining({ method: 'GET', credentials: 'include' })
    )
  })

  it('POST sets Content-Type and sends serialized body', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(201))
    await post('/test', { a: 1 })
    expect(mockFetch).toHaveBeenCalledWith(
      expect.any(String),
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({ a: 1 }),
        headers: expect.objectContaining({ 'Content-Type': 'application/json' }),
      })
    )
  })

  it('PUT sends PUT method', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(200))
    await put('/test', { b: 2 })
    expect(mockFetch).toHaveBeenCalledWith(
      expect.any(String),
      expect.objectContaining({ method: 'PUT' })
    )
  })

  it('del sends DELETE method', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(200))
    await del('/test')
    expect(mockFetch).toHaveBeenCalledWith(
      expect.any(String),
      expect.objectContaining({ method: 'DELETE' })
    )
  })

  it('does not set Content-Type when body is absent', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(200))
    await get('/test')
    const callHeaders = mockFetch.mock.calls[0][1].headers
    expect(callHeaders['Content-Type']).toBeUndefined()
  })

  // ── JSON parsing ──────────────────────────────────────────────────────────

  it('returns undefined data when response body is not valid JSON', async () => {
    mockFetch.mockResolvedValueOnce(makeThrowingResponse(200))
    const result = await get('/test')
    expect(result.ok).toBe(true)
    expect(result.data).toBeUndefined()
  })

  // ── Error classification ──────────────────────────────────────────────────

  it('returns SERVER error message on 500', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(500, null, 'Internal Server Error'))
    const result = await get('/test')
    expect(result.ok).toBe(false)
    expect(result.error).toBe(API_ERRORS.SERVER)
  })

  it('returns RATE_LIMIT error on 429', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(429))
    const result = await get('/test')
    expect(result.ok).toBe(false)
    expect(result.error).toBe(API_ERRORS.RATE_LIMIT)
  })

  it('returns NOT_FOUND error on 404', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(404))
    const result = await get('/test')
    expect(result.ok).toBe(false)
    expect(result.error).toBe(API_ERRORS.NOT_FOUND)
  })

  it('returns FORBIDDEN error on 403', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(403))
    const result = await get('/test')
    expect(result.ok).toBe(false)
    expect(result.error).toBe(API_ERRORS.FORBIDDEN)
  })

  it('returns BAD_REQUEST error on 400 when body has no message', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(400))
    const result = await get('/test')
    expect(result.ok).toBe(false)
    expect(result.error).toBe(API_ERRORS.BAD_REQUEST)
  })

  it('returns backend message when body contains a recognized error code', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(400, { message: 'INVALID_USERNAME_OR_PASSWORD' }))
    const result = await get('/test')
    expect(result.ok).toBe(false)
    expect(result.error).toBe('Invalid email or password.')
  })

  it('falls back to statusText when status is unclassified and no backend message', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(418, null, "I'm a teapot"))
    const result = await get('/test')
    expect(result.ok).toBe(false)
    expect(result.error).toBe("I'm a teapot")
  })

  it('falls back to "Request failed" when status is unclassified with no statusText or body', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(418, null, ''))
    const result = await get('/test')
    expect(result.ok).toBe(false)
    expect(result.error).toBe('Request failed')
  })

  // ── Handlers ──────────────────────────────────────────────────────────────

  it('calls errorHandler on non-401 error response', async () => {
    const handler = vi.fn()
    onError(handler)
    mockFetch.mockResolvedValueOnce(makeResponse(500))
    await get('/test')
    expect(handler).toHaveBeenCalledOnce()
  })

  it('returns NETWORK error and calls errorHandler when fetch throws', async () => {
    const handler = vi.fn()
    onError(handler)
    mockFetch.mockRejectedValueOnce(new Error('Network error'))
    const result = await get('/test')
    expect(result).toEqual({ ok: false, status: 0, error: API_ERRORS.NETWORK })
    expect(handler).toHaveBeenCalledOnce()
  })

  it('returns NETWORK error via location.assign when fetch throws and no errorHandler', async () => {
    mockFetch.mockRejectedValueOnce(new Error('Network error'))
    const result = await get('/test')
    expect(result.ok).toBe(false)
    expect(result.error).toBe(API_ERRORS.NETWORK)
  })

  // ── silent ────────────────────────────────────────────────────────────────

  it('does not call errorHandler on error response when silent=true', async () => {
    const handler = vi.fn()
    onError(handler)
    mockFetch.mockResolvedValueOnce(makeResponse(401))
    await get('/test', { skipUnauthorized: true, silent: true })
    expect(handler).not.toHaveBeenCalled()
  })

  it('still returns error response when silent=true', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(401))
    const result = await get('/test', { skipUnauthorized: true, silent: true })
    expect(result.ok).toBe(false)
    expect(result.status).toBe(401)
  })

  // ── skipUnauthorized ──────────────────────────────────────────────────────

  it('returns 401 immediately without refresh when skipUnauthorized=true', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(401))
    const result = await get('/test', { skipUnauthorized: true })
    expect(result.ok).toBe(false)
    expect(result.status).toBe(401)
    expect(mockFetch).toHaveBeenCalledTimes(1)
  })

  it('calls unauthorizedHandler on 401 without skipUnauthorized when refresh fails', async () => {
    const handler = vi.fn()
    onUnauthorized(handler)
    mockFetch
      .mockResolvedValueOnce(makeResponse(401)) // original request
      .mockResolvedValueOnce(makeResponse(401)) // refresh fails (r.ok=false)
    await get('/protected')
    expect(handler).toHaveBeenCalledOnce()
  })

  it('calls location.assign on 401 when refresh fails and no unauthorizedHandler', async () => {
    mockFetch
      .mockResolvedValueOnce(makeResponse(401)) // original request
      .mockResolvedValueOnce(makeResponse(401)) // refresh fails
    await get('/protected')
    expect(mockLocationAssign).toHaveBeenCalledWith('/login')
  })

  // ── Refresh + retry (covers lines 41, 60–69) ──────────────────────────────

  describe('transparent refresh and retry', () => {
    it('retries and returns success when refresh succeeds and retry returns 200', async () => {
      mockFetch
        .mockResolvedValueOnce(makeResponse(401))            // original → 401
        .mockResolvedValueOnce(makeResponse(200))            // refresh → ok
        .mockResolvedValueOnce(makeResponse(200, { x: 1 })) // retry → 200
      const result = await get<{ x: number }>('/protected')
      expect(result).toEqual({ ok: true, status: 200, data: { x: 1 } })
      expect(mockFetch).toHaveBeenCalledTimes(3)
    })

    it('calls unauthorizedHandler when refresh succeeds but retry returns 401', async () => {
      const handler = vi.fn()
      onUnauthorized(handler)
      mockFetch
        .mockResolvedValueOnce(makeResponse(401)) // original → 401
        .mockResolvedValueOnce(makeResponse(200)) // refresh → ok
        .mockResolvedValueOnce(makeResponse(401)) // retry → 401
      const result = await get('/protected')
      expect(handler).toHaveBeenCalledOnce()
      expect(result).toEqual({ ok: false, status: 401, error: API_ERRORS.UNAUTHORIZED })
    })

    it('calls location.assign when refresh succeeds, retry returns 401, and no handler', async () => {
      mockFetch
        .mockResolvedValueOnce(makeResponse(401)) // original → 401
        .mockResolvedValueOnce(makeResponse(200)) // refresh → ok
        .mockResolvedValueOnce(makeResponse(401)) // retry → 401
      await get('/protected')
      expect(mockLocationAssign).toHaveBeenCalledWith('/login')
    })

    it('returns error when refresh succeeds but retry returns a non-401 error', async () => {
      mockFetch
        .mockResolvedValueOnce(makeResponse(401)) // original → 401
        .mockResolvedValueOnce(makeResponse(200)) // refresh → ok
        .mockResolvedValueOnce(makeResponse(500)) // retry → 500
      const result = await get('/protected')
      expect(result.ok).toBe(false)
      expect(result.status).toBe(500)
      expect(result.error).toBe(API_ERRORS.SERVER)
    })

    it('calls unauthorizedHandler when network error occurs during refresh (catch → false)', async () => {
      const handler = vi.fn()
      onUnauthorized(handler)
      mockFetch
        .mockResolvedValueOnce(makeResponse(401))           // original → 401
        .mockRejectedValueOnce(new Error('Network down'))   // refresh fetch throws → false
      await get('/protected')
      expect(handler).toHaveBeenCalledOnce()
    })

    it('deduplicates concurrent refresh calls — second 401 reuses in-flight refresh promise', async () => {
      onUnauthorized(() => {})
      mockFetch
        .mockResolvedValueOnce(makeResponse(401))            // /a → 401
        .mockResolvedValueOnce(makeResponse(401))            // /b → 401
        .mockResolvedValueOnce(makeResponse(200))            // refresh (shared, called once)
        .mockResolvedValueOnce(makeResponse(200, { a: 1 })) // /a retry → 200
        .mockResolvedValueOnce(makeResponse(200, { b: 2 })) // /b retry → 200

      const [r1, r2] = await Promise.all([get('/a'), get('/b')])

      expect(r1).toEqual({ ok: true, status: 200, data: { a: 1 } })
      expect(r2).toEqual({ ok: true, status: 200, data: { b: 2 } })
      const refreshCalls = mockFetch.mock.calls.filter(([url]) =>
        typeof url === 'string' && url.includes('/auth/refresh')
      )
      expect(refreshCalls).toHaveLength(1)
    })
  })

  // ── request() directly ────────────────────────────────────────────────────

  it('request() forwards custom headers alongside Content-Type', async () => {
    mockFetch.mockResolvedValueOnce(makeResponse(200))
    await request('/test', {
      method: 'POST',
      body: JSON.stringify({}),
      headers: { 'X-Custom': 'value' },
    })
    expect(mockFetch).toHaveBeenCalledWith(
      expect.any(String),
      expect.objectContaining({
        headers: expect.objectContaining({ 'X-Custom': 'value', 'Content-Type': 'application/json' }),
      })
    )
  })
})
