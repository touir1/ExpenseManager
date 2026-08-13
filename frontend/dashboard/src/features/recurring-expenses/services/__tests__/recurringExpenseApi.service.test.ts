import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import * as api from '@/services/api.service'
import { getAll, getById, create, update, remove, confirm } from '../recurringExpenseApi.service'
import type { RecurringExpenseRequest } from '@/features/dashboard/types/dashboard.type'

vi.mock('@/services/api.service', () => ({
  get: vi.fn(),
  post: vi.fn(),
  put: vi.fn(),
  del: vi.fn(),
}))

const ok = { ok: true, status: 200, data: {} }
const req: RecurringExpenseRequest = {
  description: 'Netflix',
  amount: 15.99,
  currencyId: 1,
  categoryId: 1,
  frequencyId: 2,
  nextDueDate: '2026-09-01',
  autoCreate: false,
}

beforeEach(() => vi.clearAllMocks())
afterEach(() => vi.clearAllMocks())

describe('getAll', () => {
  it('calls GET with includeInactive=false by default', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getAll()
    expect(api.get).toHaveBeenCalledWith('/api/expenses/recurring-expenses?includeInactive=false')
  })

  it('calls GET with includeInactive=true when requested', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getAll(true)
    expect(api.get).toHaveBeenCalledWith('/api/expenses/recurring-expenses?includeInactive=true')
  })

  it('propagates response', async () => {
    const data = [{ id: 1 }]
    vi.mocked(api.get).mockResolvedValue({ ok: true, status: 200, data })
    const result = await getAll()
    expect(result.data).toEqual(data)
  })
})

describe('getById', () => {
  it('calls GET /recurring-expenses/{id}', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getById(5)
    expect(api.get).toHaveBeenCalledWith('/api/expenses/recurring-expenses/5')
  })
})

describe('create', () => {
  it('calls POST with request body', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await create(req)
    expect(api.post).toHaveBeenCalledWith('/api/expenses/recurring-expenses', req)
  })

  it('propagates response', async () => {
    vi.mocked(api.post).mockResolvedValue({ ok: true, status: 201, data: { id: 1 } })
    const result = await create(req)
    expect(result.ok).toBe(true)
  })
})

describe('update', () => {
  it('calls PUT /recurring-expenses/{id} with request body', async () => {
    vi.mocked(api.put).mockResolvedValue(ok)
    await update(5, req)
    expect(api.put).toHaveBeenCalledWith('/api/expenses/recurring-expenses/5', req)
  })
})

describe('remove', () => {
  it('calls DELETE /recurring-expenses/{id}', async () => {
    vi.mocked(api.del).mockResolvedValue({ ok: true, status: 204 })
    await remove(5)
    expect(api.del).toHaveBeenCalledWith('/api/expenses/recurring-expenses/5')
  })

  it('propagates error response', async () => {
    vi.mocked(api.del).mockResolvedValue({ ok: false, status: 404, error: 'NOT_FOUND' })
    const result = await remove(5)
    expect(result.ok).toBe(false)
  })
})

describe('confirm', () => {
  it('calls POST /recurring-expenses/{id}/confirm', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await confirm(5)
    expect(api.post).toHaveBeenCalledWith('/api/expenses/recurring-expenses/5/confirm', {})
  })

  it('propagates response', async () => {
    vi.mocked(api.post).mockResolvedValue({ ok: true, status: 200, data: { id: 42 } })
    const result = await confirm(5)
    expect(result.ok).toBe(true)
  })

  it('propagates RECURRING_NOT_DUE error', async () => {
    vi.mocked(api.post).mockResolvedValue({ ok: false, status: 400, error: 'RECURRING_NOT_DUE' })
    const result = await confirm(5)
    expect(result.ok).toBe(false)
  })
})
