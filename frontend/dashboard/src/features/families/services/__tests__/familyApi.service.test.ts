import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import * as api from '@/services/api.service'
import {
  getFamilies,
  getFamilyById,
  createFamily,
  renameFamily,
  archiveFamily,
  unarchiveFamily,
  inviteMember,
  acceptInvite,
  removeMember,
  changeMemberRole,
  leaveFamily,
} from '../familyApi.service'

vi.mock('@/services/api.service', () => ({
  get: vi.fn(),
  post: vi.fn(),
  put: vi.fn(),
  del: vi.fn(),
}))

const ok = { ok: true, status: 200, data: {} }

beforeEach(() => vi.clearAllMocks())
afterEach(() => vi.clearAllMocks())

describe('getFamilies', () => {
  it('calls GET /api/expenses/families', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getFamilies()
    expect(api.get).toHaveBeenCalledWith('/api/expenses/families')
  })

  it('propagates response', async () => {
    vi.mocked(api.get).mockResolvedValue({ ok: false, status: 401 })
    const result = await getFamilies()
    expect(result.ok).toBe(false)
  })
})

describe('getFamilyById', () => {
  it('calls GET /api/expenses/families/{id}', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getFamilyById(3)
    expect(api.get).toHaveBeenCalledWith('/api/expenses/families/3')
  })
})

describe('createFamily', () => {
  it('calls POST /api/expenses/families with name', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await createFamily('Smith')
    expect(api.post).toHaveBeenCalledWith('/api/expenses/families', { name: 'Smith' })
  })
})

describe('renameFamily', () => {
  it('calls PUT /api/expenses/families/{id}/name with name', async () => {
    vi.mocked(api.put).mockResolvedValue(ok)
    await renameFamily(2, 'New Name')
    expect(api.put).toHaveBeenCalledWith('/api/expenses/families/2/name', { name: 'New Name' })
  })
})

describe('archiveFamily', () => {
  it('calls POST /api/expenses/families/{id}/archive', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await archiveFamily(4)
    expect(api.post).toHaveBeenCalledWith('/api/expenses/families/4/archive', {})
  })
})

describe('unarchiveFamily', () => {
  it('calls POST /api/expenses/families/{id}/unarchive', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await unarchiveFamily(4)
    expect(api.post).toHaveBeenCalledWith('/api/expenses/families/4/unarchive', {})
  })
})

describe('inviteMember', () => {
  it('calls POST /api/expenses/families/{id}/invite with email', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await inviteMember(1, 'user@example.com')
    expect(api.post).toHaveBeenCalledWith('/api/expenses/families/1/invite', { email: 'user@example.com' })
  })
})

describe('acceptInvite', () => {
  it('calls POST /api/expenses/families/accept-invite/{token}', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await acceptInvite('tok123')
    expect(api.post).toHaveBeenCalledWith('/api/expenses/families/accept-invite/tok123', {}, undefined)
  })

  it('forwards opts', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await acceptInvite('tok', { silent: true })
    expect(api.post).toHaveBeenCalledWith('/api/expenses/families/accept-invite/tok', {}, { silent: true })
  })
})

describe('removeMember', () => {
  it('calls DELETE /api/expenses/families/{id}/members/{userId}', async () => {
    vi.mocked(api.del).mockResolvedValue(ok)
    await removeMember(1, 7)
    expect(api.del).toHaveBeenCalledWith('/api/expenses/families/1/members/7')
  })
})

describe('changeMemberRole', () => {
  it('calls PUT /api/expenses/families/{id}/members/{userId}/role with role', async () => {
    vi.mocked(api.put).mockResolvedValue(ok)
    await changeMemberRole(1, 7, 'Member')
    expect(api.put).toHaveBeenCalledWith('/api/expenses/families/1/members/7/role', { role: 'Member' })
  })
})

describe('leaveFamily', () => {
  it('calls DELETE /api/expenses/families/{id}/leave', async () => {
    vi.mocked(api.del).mockResolvedValue(ok)
    await leaveFamily(2)
    expect(api.del).toHaveBeenCalledWith('/api/expenses/families/2/leave')
  })
})
