import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import * as api from '@/services/api.service'
import { getConfig, updateConfig } from '../userConfigApi.service'

vi.mock('@/services/api.service', () => ({
  get: vi.fn(),
  put: vi.fn(),
}))

describe('userConfigApi.service', () => {
  beforeEach(() => vi.clearAllMocks())
  afterEach(() => vi.clearAllMocks())

  describe('getConfig', () => {
    it('calls get with correct endpoint', async () => {
      vi.mocked(api.get).mockResolvedValue({ ok: true, status: 200, data: { defaultCurrencyId: 1, defaultCurrency: null } })

      await getConfig()

      expect(api.get).toHaveBeenCalledWith('/api/expenses/config')
    })

    it('returns successful response with config data', async () => {
      const data = { defaultCurrencyId: 2, defaultCurrency: null }
      vi.mocked(api.get).mockResolvedValue({ ok: true, status: 200, data })

      const result = await getConfig()

      expect(result.ok).toBe(true)
      expect(result.data).toEqual(data)
    })

    it('returns null data when no config exists', async () => {
      vi.mocked(api.get).mockResolvedValue({ ok: true, status: 200, data: { defaultCurrencyId: null, defaultCurrency: null } })

      const result = await getConfig()

      expect(result.ok).toBe(true)
      expect(result.data?.defaultCurrencyId).toBeNull()
    })

    it('returns error response on failure', async () => {
      vi.mocked(api.get).mockResolvedValue({ ok: false, status: 401, error: 'UNAUTHORIZED' })

      const result = await getConfig()

      expect(result.ok).toBe(false)
    })

    it('calls get exactly once', async () => {
      vi.mocked(api.get).mockResolvedValue({ ok: true, status: 200, data: { defaultCurrencyId: null, defaultCurrency: null } })

      await getConfig()

      expect(api.get).toHaveBeenCalledTimes(1)
    })
  })

  describe('updateConfig', () => {
    it('calls put with correct endpoint and body', async () => {
      vi.mocked(api.put).mockResolvedValue({ ok: true, status: 200, data: { defaultCurrencyId: 3, defaultCurrency: null } })

      await updateConfig({ defaultCurrencyId: 3 })

      expect(api.put).toHaveBeenCalledWith('/api/expenses/config', { defaultCurrencyId: 3 })
    })

    it('passes null defaultCurrencyId when clearing', async () => {
      vi.mocked(api.put).mockResolvedValue({ ok: true, status: 200, data: { defaultCurrencyId: null, defaultCurrency: null } })

      await updateConfig({ defaultCurrencyId: null })

      expect(api.put).toHaveBeenCalledWith('/api/expenses/config', { defaultCurrencyId: null })
    })

    it('returns successful response with updated config', async () => {
      const data = { defaultCurrencyId: 3, defaultCurrency: null }
      vi.mocked(api.put).mockResolvedValue({ ok: true, status: 200, data })

      const result = await updateConfig({ defaultCurrencyId: 3 })

      expect(result.ok).toBe(true)
      expect(result.data).toEqual(data)
    })

    it('returns error response on failure', async () => {
      vi.mocked(api.put).mockResolvedValue({ ok: false, status: 400, error: 'INVALID_CURRENCY' })

      const result = await updateConfig({ defaultCurrencyId: 999 })

      expect(result.ok).toBe(false)
      expect(result.error).toBe('INVALID_CURRENCY')
    })

    it('calls put exactly once', async () => {
      vi.mocked(api.put).mockResolvedValue({ ok: true, status: 200, data: { defaultCurrencyId: 1, defaultCurrency: null } })

      await updateConfig({ defaultCurrencyId: 1 })

      expect(api.put).toHaveBeenCalledTimes(1)
    })
  })
})
