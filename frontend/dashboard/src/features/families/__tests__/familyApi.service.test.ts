import { describe, it, expect, vi, beforeEach } from 'vitest'
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
} from '../services/familyApi.service'

vi.mock('@/services/api.service', () => ({
  get: vi.fn(),
  post: vi.fn(),
  put: vi.fn(),
  del: vi.fn(),
}))

const BASE = '/api/expenses/families'

describe('familyApi.service', () => {
  beforeEach(() => vi.clearAllMocks())

  it('getFamilies calls GET /api/expenses/families', () => {
    getFamilies()
    expect(api.get).toHaveBeenCalledWith(BASE)
  })

  it('getFamilyById calls GET /api/expenses/families/:id', () => {
    getFamilyById(7)
    expect(api.get).toHaveBeenCalledWith(`${BASE}/7`)
  })

  it('createFamily calls POST /api/expenses/families with name', () => {
    createFamily('Holiday')
    expect(api.post).toHaveBeenCalledWith(BASE, { name: 'Holiday' })
  })

  it('renameFamily calls PUT /api/expenses/families/:id/name', () => {
    renameFamily(3, 'New Name')
    expect(api.put).toHaveBeenCalledWith(`${BASE}/3/name`, { name: 'New Name' })
  })

  it('archiveFamily calls POST /api/expenses/families/:id/archive', () => {
    archiveFamily(4)
    expect(api.post).toHaveBeenCalledWith(`${BASE}/4/archive`, {})
  })

  it('unarchiveFamily calls POST /api/expenses/families/:id/unarchive', () => {
    unarchiveFamily(4)
    expect(api.post).toHaveBeenCalledWith(`${BASE}/4/unarchive`, {})
  })

  it('inviteMember calls POST /api/expenses/families/:id/invite', () => {
    inviteMember(5, 'user@example.com')
    expect(api.post).toHaveBeenCalledWith(`${BASE}/5/invite`, { email: 'user@example.com' })
  })

  it('acceptInvite calls POST /api/expenses/families/accept-invite/:token', () => {
    acceptInvite('tok123')
    expect(api.post).toHaveBeenCalledWith(`${BASE}/accept-invite/tok123`, {}, undefined)
  })

  it('acceptInvite passes opts to post', () => {
    acceptInvite('tok123', { silent: true })
    expect(api.post).toHaveBeenCalledWith(`${BASE}/accept-invite/tok123`, {}, { silent: true })
  })

  it('removeMember calls DELETE /api/expenses/families/:id/members/:userId', () => {
    removeMember(6, 99)
    expect(api.del).toHaveBeenCalledWith(`${BASE}/6/members/99`)
  })

  it('changeMemberRole calls PUT /api/expenses/families/:id/members/:userId/role', () => {
    changeMemberRole(6, 99, 'Head')
    expect(api.put).toHaveBeenCalledWith(`${BASE}/6/members/99/role`, { role: 'Head' })
  })

  it('leaveFamily calls DELETE /api/expenses/families/:id/leave', () => {
    leaveFamily(7)
    expect(api.del).toHaveBeenCalledWith(`${BASE}/7/leave`)
  })
})
