import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import * as api from '@/services/api.service'
import { getCategories } from '../categoriesApi.service'

vi.mock('@/services/api.service', () => ({
  get: vi.fn(),
}))

describe('categoriesApi.service', () => {
  beforeEach(() => vi.clearAllMocks())
  afterEach(() => vi.clearAllMocks())

  describe('getCategories', () => {
    it('calls get with correct endpoint', async () => {
      vi.mocked(api.get).mockResolvedValue({ ok: true, status: 200, data: [] })

      await getCategories()

      expect(api.get).toHaveBeenCalledWith('/api/expenses/categories')
    })

    it('returns response on success', async () => {
      const data = [
        { id: 1, name: 'Food', subcategories: [{ id: 2, name: 'Groceries' }] },
      ]
      vi.mocked(api.get).mockResolvedValue({ ok: true, status: 200, data })

      const result = await getCategories()

      expect(result.ok).toBe(true)
      expect(result.data).toEqual(data)
    })

    it('returns error response on failure', async () => {
      vi.mocked(api.get).mockResolvedValue({ ok: false, status: 500, error: 'SERVER_ERROR' })

      const result = await getCategories()

      expect(result.ok).toBe(false)
      expect(result.error).toBe('SERVER_ERROR')
    })

    it('calls get exactly once', async () => {
      vi.mocked(api.get).mockResolvedValue({ ok: true, status: 200, data: [] })

      await getCategories()

      expect(api.get).toHaveBeenCalledTimes(1)
    })
  })
})
