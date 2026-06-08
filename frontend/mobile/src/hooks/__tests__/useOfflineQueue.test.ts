import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useOfflineQueue } from '@/hooks/useOfflineQueue'

vi.mock('@/features/expenses/services/expensesApi.service', () => ({
  addExpense: vi.fn(),
}))

import { addExpense } from '@/features/expenses/services/expensesApi.service'

const mockAddExpense = addExpense as ReturnType<typeof vi.fn>

const basePayload = {
  amount: 10,
  currencyId: 1,
  date: '2024-01-15',
}

describe('useOfflineQueue', () => {
  beforeEach(() => {
    mockAddExpense.mockReset()
  })

  it('enqueue adds item to IndexedDB and returns an id', async () => {
    const { enqueue, getAll } = useOfflineQueue()
    const id = await enqueue(basePayload)
    expect(typeof id).toBe('string')
    expect(id.length).toBeGreaterThan(0)
    const items = await getAll()
    expect(items.some(i => i.id === id)).toBe(true)
  })

  it('drain processes items in order and deletes them on success', async () => {
    mockAddExpense.mockResolvedValue({ ok: true, status: 201 })
    const { enqueue, drain, getAll, clear } = useOfflineQueue()
    await clear()

    const id1 = await enqueue({ ...basePayload, amount: 1 })
    const id2 = await enqueue({ ...basePayload, amount: 2 })

    const result = await drain()
    expect(result.ok).toBe(2)
    expect(result.failed).toBe(0)

    const remaining = await getAll()
    expect(remaining.find(i => i.id === id1)).toBeUndefined()
    expect(remaining.find(i => i.id === id2)).toBeUndefined()
  })

  it('drain keeps failed items in queue', async () => {
    mockAddExpense.mockResolvedValue({ ok: false, status: 500, error: 'Server error' })
    const { enqueue, drain, getAll, clear } = useOfflineQueue()
    await clear()

    const id = await enqueue(basePayload)
    const result = await drain()

    expect(result.ok).toBe(0)
    expect(result.failed).toBe(1)
    const remaining = await getAll()
    expect(remaining.find(i => i.id === id)).toBeDefined()
  })

  it('enqueue assigns unique ids even for identical payloads', async () => {
    const { enqueue, clear } = useOfflineQueue()
    await clear()
    const id1 = await enqueue(basePayload)
    const id2 = await enqueue(basePayload)
    expect(id1).not.toBe(id2)
  })
})
