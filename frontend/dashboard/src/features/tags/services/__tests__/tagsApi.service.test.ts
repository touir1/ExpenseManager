import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import * as api from '@/services/api.service'
import { getTags, useTag, removeTag } from '../tagsApi.service'

vi.mock('@/services/api.service', () => ({
  get: vi.fn(),
  post: vi.fn(),
  del: vi.fn(),
}))

const ok = { ok: true, status: 200, data: {} }

beforeEach(() => vi.clearAllMocks())
afterEach(() => vi.clearAllMocks())

describe('getTags', () => {
  it('calls GET /api/expenses/tags', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getTags()
    expect(api.get).toHaveBeenCalledWith('/api/expenses/tags')
  })

  it('propagates response', async () => {
    vi.mocked(api.get).mockResolvedValue({ ok: false, status: 401 })
    const result = await getTags()
    expect(result.ok).toBe(false)
  })
})

describe('useTag', () => {
  it('calls POST /api/expenses/tags with name', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await useTag('food')
    expect(api.post).toHaveBeenCalledWith('/api/expenses/tags', { name: 'food' })
  })
})

describe('removeTag', () => {
  it('calls DELETE /api/expenses/tags/{id}', async () => {
    vi.mocked(api.del).mockResolvedValue(ok)
    await removeTag(3)
    expect(api.del).toHaveBeenCalledWith('/api/expenses/tags/3')
  })
})
