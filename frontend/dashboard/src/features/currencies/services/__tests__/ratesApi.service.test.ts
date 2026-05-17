import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import * as api from '@/services/api.service'
import { refreshRates } from '../ratesApi.service'

vi.mock('@/services/api.service', () => ({
  post: vi.fn(),
}))

describe('ratesApi.service', () => {
  beforeEach(() => vi.clearAllMocks())
  afterEach(() => vi.clearAllMocks())

  describe('refreshRates', () => {
    it('calls post with correct endpoint and from date', async () => {
      vi.mocked(api.post).mockResolvedValue({ ok: true, status: 204, data: undefined })

      await refreshRates({ from: '2024-06-01' })

      expect(api.post).toHaveBeenCalledWith('/api/expenses/rates/refresh', { from: '2024-06-01' })
    })

    it('passes sourceCurrencyId when provided', async () => {
      vi.mocked(api.post).mockResolvedValue({ ok: true, status: 204, data: undefined })

      await refreshRates({ from: '2024-06-01', sourceCurrencyId: 1 })

      expect(api.post).toHaveBeenCalledWith('/api/expenses/rates/refresh', {
        from: '2024-06-01',
        sourceCurrencyId: 1,
      })
    })

    it('passes destinationCurrencyId when provided', async () => {
      vi.mocked(api.post).mockResolvedValue({ ok: true, status: 204, data: undefined })

      await refreshRates({ from: '2024-06-01', destinationCurrencyId: 2 })

      expect(api.post).toHaveBeenCalledWith('/api/expenses/rates/refresh', {
        from: '2024-06-01',
        destinationCurrencyId: 2,
      })
    })

    it('passes both source and destination when provided', async () => {
      vi.mocked(api.post).mockResolvedValue({ ok: true, status: 204, data: undefined })

      await refreshRates({ from: '2024-06-01', sourceCurrencyId: 1, destinationCurrencyId: 2 })

      expect(api.post).toHaveBeenCalledWith('/api/expenses/rates/refresh', {
        from: '2024-06-01',
        sourceCurrencyId: 1,
        destinationCurrencyId: 2,
      })
    })

    it('returns successful response', async () => {
      vi.mocked(api.post).mockResolvedValue({ ok: true, status: 204, data: undefined })

      const result = await refreshRates({ from: '2024-06-01' })

      expect(result.ok).toBe(true)
      expect(result.status).toBe(204)
    })

    it('returns error response on failure', async () => {
      vi.mocked(api.post).mockResolvedValue({ ok: false, status: 400, error: 'SERVER_ERROR' })

      const result = await refreshRates({ from: '2024-06-01' })

      expect(result.ok).toBe(false)
      expect(result.error).toBe('SERVER_ERROR')
    })

    it('returns unauthorized response on 401', async () => {
      vi.mocked(api.post).mockResolvedValue({ ok: false, status: 401, error: 'MISSING_USER' })

      const result = await refreshRates({ from: '2024-06-01' })

      expect(result.ok).toBe(false)
      expect(result.status).toBe(401)
    })

    it('calls post exactly once', async () => {
      vi.mocked(api.post).mockResolvedValue({ ok: true, status: 204, data: undefined })

      await refreshRates({ from: '2024-06-01', sourceCurrencyId: 1, destinationCurrencyId: 2 })

      expect(api.post).toHaveBeenCalledTimes(1)
    })
  })
})
