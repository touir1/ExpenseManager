import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import * as api from '@/services/api.service'
import { getCurrencies } from '../currenciesApi.service'

vi.mock('@/services/api.service', () => ({
  get: vi.fn(),
}))

describe('currenciesApi.service', () => {
  beforeEach(() => vi.clearAllMocks())
  afterEach(() => vi.clearAllMocks())

  describe('getCurrencies', () => {
    it('calls get with correct endpoint', async () => {
      vi.mocked(api.get).mockResolvedValue({ ok: true, status: 200, data: [] })

      await getCurrencies()

      expect(api.get).toHaveBeenCalledWith('/api/expenses/currencies')
    })

    it('returns response on success', async () => {
      const data = [
        { id: 1, code: 'USD', name: 'US Dollar', symbol: '$', decimals: 2 },
        { id: 2, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
      ]
      vi.mocked(api.get).mockResolvedValue({ ok: true, status: 200, data })

      const result = await getCurrencies()

      expect(result.ok).toBe(true)
      expect(result.data).toEqual(data)
    })

    it('returns error response on failure', async () => {
      vi.mocked(api.get).mockResolvedValue({ ok: false, status: 500, error: 'SERVER_ERROR' })

      const result = await getCurrencies()

      expect(result.ok).toBe(false)
      expect(result.error).toBe('SERVER_ERROR')
    })

    it('calls get exactly once', async () => {
      vi.mocked(api.get).mockResolvedValue({ ok: true, status: 200, data: [] })

      await getCurrencies()

      expect(api.get).toHaveBeenCalledTimes(1)
    })
  })
})
