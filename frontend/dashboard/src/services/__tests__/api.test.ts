import { describe, it, expect, vi, beforeEach } from 'vitest'
import { request, get, post, put, del, onUnauthorized, onError } from '@/services/api.service'

// Mock fetch globally
const mockFetch = vi.fn()
globalThis.fetch = mockFetch as any

describe('API utilities', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    onUnauthorized(null)
    onError(null)
  })

  describe('request', () => {
    it('makes a successful GET request', async () => {
      mockFetch.mockResolvedValue({
        ok: true,
        status: 200,
        json: async () => ({ id: 1, name: 'Test' })
      })

      const result = await request('/test')

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/test'),
        expect.objectContaining({ headers: expect.any(Object) })
      )
      expect(result.ok).toBe(true)
      expect(result.status).toBe(200)
      expect(result.data).toEqual({ id: 1, name: 'Test' })
    })

    it('includes credentials: include on every request', async () => {
      mockFetch.mockResolvedValue({
        ok: true,
        status: 200,
        json: async () => ({})
      })

      await request('/protected')

      expect(mockFetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({ credentials: 'include' })
      )
    })

    it('sets Content-Type header for POST with body', async () => {
      mockFetch.mockResolvedValue({
        ok: true,
        status: 200,
        json: async () => ({})
      })

      await request('/test', { method: 'POST', body: JSON.stringify({ data: 'test' }) })

      expect(mockFetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          headers: expect.objectContaining({
            'Content-Type': 'application/json'
          })
        })
      )
    })

    it('handles 401 unauthorized with handler', async () => {
      const unauthorizedHandler = vi.fn()
      onUnauthorized(unauthorizedHandler)

      mockFetch.mockResolvedValue({
        ok: false,
        status: 401,
        json: async () => ({})
      })

      const result = await request('/protected')

      expect(unauthorizedHandler).toHaveBeenCalled()
      expect(result.ok).toBe(false)
      expect(result.status).toBe(401)
      expect(result.error).toBe('Unauthorized')
    })

    it('skips unauthorized handler on 401 when skipUnauthorized is true', async () => {
      const unauthorizedHandler = vi.fn()
      onUnauthorized(unauthorizedHandler)

      mockFetch.mockResolvedValue({
        ok: false,
        status: 401,
        json: async () => ({})
      })

      const result = await request('/auth/login', {}, { skipUnauthorized: true })

      expect(unauthorizedHandler).not.toHaveBeenCalled()
      expect(result.ok).toBe(false)
      expect(result.status).toBe(401)
    })

    it('redirects to /login on 401 when no unauthorized handler is set', async () => {
      const mockAssign = vi.fn()
      Object.defineProperty(window, 'location', { value: { assign: mockAssign }, writable: true })

      mockFetch.mockResolvedValue({
        ok: false,
        status: 401,
        json: async () => ({})
      })

      const result = await request('/protected')

      expect(mockAssign).toHaveBeenCalledWith('/login')
      expect(result.ok).toBe(false)
      expect(result.status).toBe(401)
    })

    it('does not throw when non-ok response has no error handler set', async () => {
      mockFetch.mockResolvedValue({
        ok: false,
        status: 500,
        json: async () => ({})
      })

      const result = await request('/error')

      expect(result.ok).toBe(false)
      expect(result.status).toBe(500)
      expect(result.error).toBe('Server error, please retry later.')
    })

    it('does not throw on network error when no error handler is set', async () => {
      mockFetch.mockRejectedValue(new Error('Network failure'))

      const result = await request('/fail')

      expect(result.ok).toBe(false)
      expect(result.status).toBe(0)
      expect(result.error).toBe('Network error. Please check your connection and try again.')
    })

    it('handles 404 not found', async () => {
      const errorHandler = vi.fn()
      onError(errorHandler)

      mockFetch.mockResolvedValue({
        ok: false,
        status: 404,
        statusText: 'Not Found',
        json: async () => ({})
      })

      const result = await request('/not-found')

      expect(errorHandler).toHaveBeenCalledWith('Resource not found.')
      expect(result.ok).toBe(false)
      expect(result.status).toBe(404)
    })

    it('handles 400 bad request', async () => {
      const errorHandler = vi.fn()
      onError(errorHandler)

      mockFetch.mockResolvedValue({
        ok: false,
        status: 400,
        json: async () => ({})
      })

      const result = await request('/bad')

      expect(errorHandler).toHaveBeenCalledWith('Invalid request. Please check your input.')
      expect(result.status).toBe(400)
    })

    it('handles 403 forbidden', async () => {
      const errorHandler = vi.fn()
      onError(errorHandler)

      mockFetch.mockResolvedValue({
        ok: false,
        status: 403,
        json: async () => ({})
      })

      const result = await request('/forbidden')

      expect(errorHandler).toHaveBeenCalledWith('Access denied. You might not have permission.')
      expect(result.status).toBe(403)
    })

    it('handles 429 too many requests', async () => {
      const errorHandler = vi.fn()
      onError(errorHandler)

      mockFetch.mockResolvedValue({
        ok: false,
        status: 429,
        json: async () => ({})
      })

      const result = await request('/rate-limited')

      expect(errorHandler).toHaveBeenCalledWith('Too many requests. Please wait and try again.')
      expect(result.status).toBe(429)
    })

    it('handles 500 server error', async () => {
      const errorHandler = vi.fn()
      onError(errorHandler)

      mockFetch.mockResolvedValue({
        ok: false,
        status: 500,
        json: async () => ({})
      })

      const result = await request('/error')

      expect(errorHandler).toHaveBeenCalledWith('Server error, please retry later.')
      expect(result.status).toBe(500)
    })

    it('handles network errors', async () => {
      const errorHandler = vi.fn()
      onError(errorHandler)

      mockFetch.mockRejectedValue(new Error('Network error'))

      const result = await request('/network-fail')

      expect(errorHandler).toHaveBeenCalledWith('Network error. Please check your connection and try again.')
      expect(result.ok).toBe(false)
      expect(result.status).toBe(0)
    })

    it('handles invalid JSON responses gracefully', async () => {
      mockFetch.mockResolvedValue({
        ok: true,
        status: 200,
        json: async () => { throw new Error('Invalid JSON') }
      })

      const result = await request('/invalid-json')

      expect(result.ok).toBe(true)
      expect(result.status).toBe(200)
      expect(result.data).toBeUndefined()
    })

    it('uses custom error message from API response', async () => {
      const errorHandler = vi.fn()
      onError(errorHandler)

      mockFetch.mockResolvedValue({
        ok: false,
        status: 422,
        json: async () => ({ error: 'Custom error message' })
      })

      await request('/test')

      expect(errorHandler).toHaveBeenCalledWith('Custom error message')
    })
  })

  describe('HTTP method helpers', () => {
    it('get() makes GET request', async () => {
      mockFetch.mockResolvedValue({
        ok: true,
        status: 200,
        json: async () => ({ data: 'test' })
      })

      const result = await get('/test')

      expect(mockFetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({ method: 'GET' })
      )
      expect(result.data).toEqual({ data: 'test' })
    })

    it('post() makes POST request with body', async () => {
      mockFetch.mockResolvedValue({
        ok: true,
        status: 201,
        json: async () => ({ id: 1 })
      })

      const body = { name: 'Test' }
      const result = await post('/create', body)

      expect(mockFetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          method: 'POST',
          body: JSON.stringify(body)
        })
      )
      expect(result.data).toEqual({ id: 1 })
    })

    it('put() makes PUT request with body', async () => {
      mockFetch.mockResolvedValue({
        ok: true,
        status: 200,
        json: async () => ({ updated: true })
      })

      const body = { name: 'Updated' }
      const result = await put('/update/1', body)

      expect(mockFetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          method: 'PUT',
          body: JSON.stringify(body)
        })
      )
      expect(result.data).toEqual({ updated: true })
    })

    it('del() makes DELETE request', async () => {
      mockFetch.mockResolvedValue({
        ok: true,
        status: 204,
        json: async () => ({})
      })

      const result = await del('/delete/1')

      expect(mockFetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({ method: 'DELETE' })
      )
      expect(result.ok).toBe(true)
    })
  })

})
