import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import * as api from '@/services/api.service'
import {
  getUsers,
  getRoles,
  disableUser,
  enableUser,
  setUserRoles,
} from '../adminUsersApi.service'

vi.mock('@/services/api.service', () => ({
  get: vi.fn(),
  patch: vi.fn(),
  put: vi.fn(),
}))

const ok = { ok: true, status: 200, data: {} }
const BASE = '/api/users/admin/users'

beforeEach(() => vi.clearAllMocks())
afterEach(() => vi.clearAllMocks())

describe('getUsers', () => {
  it('calls GET with default params', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getUsers()
    const call = vi.mocked(api.get).mock.calls[0][0] as string
    expect(call).toContain('page=1')
    expect(call).toContain('pageSize=20')
  })

  it('includes search when provided', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getUsers('alice')
    const call = vi.mocked(api.get).mock.calls[0][0] as string
    expect(call).toContain('search=alice')
  })

  it('omits search when not provided', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getUsers()
    const call = vi.mocked(api.get).mock.calls[0][0] as string
    expect(call).not.toContain('search=')
  })

  it('uses provided page and pageSize', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getUsers(undefined, 3, 50)
    const call = vi.mocked(api.get).mock.calls[0][0] as string
    expect(call).toContain('page=3')
    expect(call).toContain('pageSize=50')
  })
})

describe('getRoles', () => {
  it('calls GET {BASE}/roles', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getRoles()
    expect(api.get).toHaveBeenCalledWith(`${BASE}/roles`)
  })
})

describe('disableUser', () => {
  it('calls PATCH {id}/disable', async () => {
    vi.mocked(api.patch).mockResolvedValue(ok)
    await disableUser(5)
    expect(api.patch).toHaveBeenCalledWith(`${BASE}/5/disable`, {})
  })
})

describe('enableUser', () => {
  it('calls PATCH {id}/enable', async () => {
    vi.mocked(api.patch).mockResolvedValue(ok)
    await enableUser(5)
    expect(api.patch).toHaveBeenCalledWith(`${BASE}/5/enable`, {})
  })
})

describe('setUserRoles', () => {
  it('calls PUT {id}/roles with roleIds', async () => {
    vi.mocked(api.put).mockResolvedValue(ok)
    await setUserRoles(3, [1, 2])
    expect(api.put).toHaveBeenCalledWith(`${BASE}/3/roles`, { roleIds: [1, 2] })
  })
})
