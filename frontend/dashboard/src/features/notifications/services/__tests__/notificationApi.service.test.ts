import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import * as api from '@/services/api.service'
import {
  getNotifications,
  getUnreadCount,
  markAsRead,
  markAllAsRead,
} from '../notificationApi.service'

vi.mock('@/services/api.service', () => ({
  get: vi.fn(),
  post: vi.fn(),
}))

const ok = { ok: true, status: 200, data: {} }

beforeEach(() => vi.clearAllMocks())
afterEach(() => vi.clearAllMocks())

describe('getNotifications', () => {
  it('calls GET with default page/pageSize', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getNotifications()
    expect(api.get).toHaveBeenCalledWith('/api/notifications?page=1&pageSize=20')
  })

  it('uses provided page and pageSize', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getNotifications(2, 10)
    expect(api.get).toHaveBeenCalledWith('/api/notifications?page=2&pageSize=10')
  })

  it('propagates response', async () => {
    vi.mocked(api.get).mockResolvedValue({ ok: false, status: 401 })
    const result = await getNotifications()
    expect(result.ok).toBe(false)
  })
})

describe('getUnreadCount', () => {
  it('calls GET /api/notifications/unread-count', async () => {
    vi.mocked(api.get).mockResolvedValue({ ok: true, status: 200, data: { count: 3 } })
    const result = await getUnreadCount()
    expect(api.get).toHaveBeenCalledWith('/api/notifications/unread-count')
    expect(result.data).toEqual({ count: 3 })
  })
})

describe('markAsRead', () => {
  it('calls POST /api/notifications/{id}/read', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await markAsRead(5)
    expect(api.post).toHaveBeenCalledWith('/api/notifications/5/read', {})
  })
})

describe('markAllAsRead', () => {
  it('calls POST /api/notifications/read-all', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await markAllAsRead()
    expect(api.post).toHaveBeenCalledWith('/api/notifications/read-all', {})
  })
})
