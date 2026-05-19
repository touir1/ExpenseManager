import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import * as api from '@/services/api.service'
import {
  getExpenses,
  getExpenseById,
  addExpense,
  updateExpense,
  deleteExpense,
} from '../expensesApi.service'
import type { ExpenseRequest } from '@/features/expenses/types/expenses.type'

vi.mock('@/services/api.service', () => ({
  get: vi.fn(),
  post: vi.fn(),
  put: vi.fn(),
  del: vi.fn(),
}))

const ok = { ok: true, status: 200, data: {} }
const req: ExpenseRequest = { amount: 10, currencyId: 1, date: '2026-05-19' }

beforeEach(() => vi.clearAllMocks())
afterEach(() => vi.clearAllMocks())

describe('getExpenses', () => {
  it('calls GET /api/expenses with no params when filter is empty', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getExpenses({})
    expect(api.get).toHaveBeenCalledWith('/api/expenses')
  })

  it('serialises page and categoryId into query string', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getExpenses({ page: 2, categoryId: 3 })
    expect(api.get).toHaveBeenCalledWith('/api/expenses?categoryId=3&page=2')
  })

  it('serialises multiple tagIds as repeated params', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getExpenses({ tagIds: [1, 2] })
    expect(api.get).toHaveBeenCalledWith('/api/expenses?tagIds=1&tagIds=2')
  })

  it('omits undefined/null filter fields', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getExpenses({ categoryId: undefined, dateFrom: undefined })
    expect(api.get).toHaveBeenCalledWith('/api/expenses')
  })

  it('propagates response', async () => {
    const data = { items: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 }
    vi.mocked(api.get).mockResolvedValue({ ok: true, status: 200, data })
    const result = await getExpenses()
    expect(result.data).toEqual(data)
  })

  it('propagates error response', async () => {
    vi.mocked(api.get).mockResolvedValue({ ok: false, status: 500, error: 'SERVER_ERROR' })
    const result = await getExpenses()
    expect(result.ok).toBe(false)
    expect(result.error).toBe('SERVER_ERROR')
  })
})

describe('getExpenseById', () => {
  it('calls GET /api/expenses/{id}', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getExpenseById(5)
    expect(api.get).toHaveBeenCalledWith('/api/expenses/5')
  })

  it('appends displayCurrencyId when provided', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getExpenseById(5, 2)
    expect(api.get).toHaveBeenCalledWith('/api/expenses/5?displayCurrencyId=2')
  })

  it('propagates response', async () => {
    const data = { id: 5, amount: 42 }
    vi.mocked(api.get).mockResolvedValue({ ok: true, status: 200, data })
    const result = await getExpenseById(5)
    expect(result.data).toEqual(data)
  })
})

describe('addExpense', () => {
  it('calls POST /api/expenses with request body', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await addExpense(req)
    expect(api.post).toHaveBeenCalledWith('/api/expenses', req)
  })

  it('propagates response', async () => {
    vi.mocked(api.post).mockResolvedValue({ ok: true, status: 201, data: { id: 1 } })
    const result = await addExpense(req)
    expect(result.ok).toBe(true)
  })
})

describe('updateExpense', () => {
  it('calls PUT /api/expenses/{id} with request body', async () => {
    vi.mocked(api.put).mockResolvedValue(ok)
    await updateExpense(5, req)
    expect(api.put).toHaveBeenCalledWith('/api/expenses/5', req)
  })

  it('propagates response', async () => {
    vi.mocked(api.put).mockResolvedValue({ ok: true, status: 200, data: { id: 5 } })
    const result = await updateExpense(5, req)
    expect(result.ok).toBe(true)
  })
})

describe('deleteExpense', () => {
  it('calls DELETE /api/expenses/{id}', async () => {
    vi.mocked(api.del).mockResolvedValue({ ok: true, status: 204 })
    await deleteExpense(5)
    expect(api.del).toHaveBeenCalledWith('/api/expenses/5')
  })

  it('propagates response', async () => {
    vi.mocked(api.del).mockResolvedValue({ ok: false, status: 404, error: 'NOT_FOUND' })
    const result = await deleteExpense(5)
    expect(result.ok).toBe(false)
  })
})
