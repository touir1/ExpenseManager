import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import * as api from '@/services/api.service'
import {
  getRateHistory,
  addManualRate,
  bulkAddRates,
  getPendingConflicts,
  resolveConflict,
  refreshRates,
} from '../adminRatesApi.service'

vi.mock('@/services/api.service', () => ({
  get: vi.fn(),
  post: vi.fn(),
  put: vi.fn(),
}))

const ok = { ok: true, status: 200, data: {} }
const BASE = '/api/expenses/admin/rates'

beforeEach(() => vi.clearAllMocks())
afterEach(() => vi.clearAllMocks())

describe('getRateHistory', () => {
  it('calls GET history with default params', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getRateHistory()
    expect(api.get).toHaveBeenCalledWith(`${BASE}/history?page=1&pageSize=50`)
  })

  it('includes srcId and dstId when provided', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getRateHistory(1, 2, 2, 25)
    expect(api.get).toHaveBeenCalledWith(`${BASE}/history?sourceCurrencyId=1&destinationCurrencyId=2&page=2&pageSize=25`)
  })

  it('omits srcId and dstId when not provided', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getRateHistory(undefined, undefined, 3)
    const call = vi.mocked(api.get).mock.calls[0][0] as string
    expect(call).not.toContain('sourceCurrencyId')
    expect(call).not.toContain('destinationCurrencyId')
    expect(call).toContain('page=3')
  })
})

describe('addManualRate', () => {
  it('calls POST base with fields', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await addManualRate(1, 2, '2026-01-01', 1.05)
    expect(api.post).toHaveBeenCalledWith(BASE, {
      sourceCurrencyId: 1,
      destinationCurrencyId: 2,
      date: '2026-01-01',
      rate: 1.05,
    })
  })
})

describe('bulkAddRates', () => {
  it('calls POST bulk with rates array', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    const rates = [{ sourceCurrencyId: 1, destinationCurrencyId: 2, date: '2026-01-01', rate: 1.1 }]
    await bulkAddRates(rates)
    expect(api.post).toHaveBeenCalledWith(`${BASE}/bulk`, { rates })
  })
})

describe('getPendingConflicts', () => {
  it('calls GET conflicts/pending', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getPendingConflicts()
    expect(api.get).toHaveBeenCalledWith(`${BASE}/conflicts/pending`)
  })
})

describe('resolveConflict', () => {
  it('calls PUT conflicts/{id}/resolve with resolution', async () => {
    vi.mocked(api.put).mockResolvedValue(ok)
    await resolveConflict(5, 'AcceptAuto')
    expect(api.put).toHaveBeenCalledWith(`${BASE}/conflicts/5/resolve`, { resolution: 'AcceptAuto', customRate: undefined })
  })

  it('includes customRate when provided', async () => {
    vi.mocked(api.put).mockResolvedValue(ok)
    await resolveConflict(5, 'Custom', 1.23)
    expect(api.put).toHaveBeenCalledWith(`${BASE}/conflicts/5/resolve`, { resolution: 'Custom', customRate: 1.23 })
  })
})

describe('refreshRates', () => {
  it('calls POST refresh with from and optional fields', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await refreshRates('2026-01-01')
    expect(api.post).toHaveBeenCalledWith(`${BASE}/refresh`, {
      from: '2026-01-01',
      to: undefined,
      sourceCurrencyId: undefined,
      destinationCurrencyId: undefined,
    })
  })

  it('includes optional fields when provided', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await refreshRates('2026-01-01', '2026-01-31', 1, 2)
    expect(api.post).toHaveBeenCalledWith(`${BASE}/refresh`, {
      from: '2026-01-01',
      to: '2026-01-31',
      sourceCurrencyId: 1,
      destinationCurrencyId: 2,
    })
  })
})
