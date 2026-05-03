import { describe, it, expect, vi, afterEach } from 'vitest'

describe('API_BASE env configuration', () => {
  afterEach(() => {
    vi.unstubAllEnvs()
    vi.unstubAllGlobals()
    vi.resetModules()
  })

  it('prefixes requests with VITE_API_BASE when set', async () => {
    vi.stubEnv('VITE_API_BASE', 'http://api.example.com')
    vi.resetModules()
    const { get } = await import('@/services/api.service')
    const mockFetch = vi.fn().mockResolvedValueOnce({
      ok: true, status: 200, statusText: '',
      json: () => Promise.resolve({}),
    } as Response)
    vi.stubGlobal('fetch', mockFetch)
    await get('/endpoint')
    expect(mockFetch).toHaveBeenCalledWith(
      'http://api.example.com/endpoint',
      expect.any(Object)
    )
  })

  it('strips trailing slash from VITE_API_BASE', async () => {
    vi.stubEnv('VITE_API_BASE', 'http://api.example.com/')
    vi.resetModules()
    const { get } = await import('@/services/api.service')
    const mockFetch = vi.fn().mockResolvedValueOnce({
      ok: true, status: 200, statusText: '',
      json: () => Promise.resolve({}),
    } as Response)
    vi.stubGlobal('fetch', mockFetch)
    await get('/endpoint')
    expect(mockFetch).toHaveBeenCalledWith(
      'http://api.example.com/endpoint',
      expect.any(Object)
    )
  })
})
