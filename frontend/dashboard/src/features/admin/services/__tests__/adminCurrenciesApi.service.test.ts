import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import * as api from '@/services/api.service'
import {
  addCurrency,
  updateCurrency,
  deleteCurrency,
  getCurrencyDefaults,
  setDefaultRate,
} from '../adminCurrenciesApi.service'

vi.mock('@/services/api.service', () => ({
  get: vi.fn(),
  post: vi.fn(),
  put: vi.fn(),
  del: vi.fn(),
}))

const ok = { ok: true, status: 200, data: {} }
const BASE = '/api/expenses/admin/currencies'

beforeEach(() => vi.clearAllMocks())
afterEach(() => vi.clearAllMocks())

describe('addCurrency', () => {
  it('calls POST base with fields', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await addCurrency('USD', 'US Dollar', '$')
    expect(api.post).toHaveBeenCalledWith(BASE, { code: 'USD', name: 'US Dollar', symbol: '$', decimals: 2 })
  })

  it('uses provided decimals', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await addCurrency('JPY', 'Yen', '¥', 0)
    expect(api.post).toHaveBeenCalledWith(BASE, { code: 'JPY', name: 'Yen', symbol: '¥', decimals: 0 })
  })
})

describe('updateCurrency', () => {
  it('calls PUT {id} with fields', async () => {
    vi.mocked(api.put).mockResolvedValue(ok)
    await updateCurrency(1, 'Euro', '€', 2)
    expect(api.put).toHaveBeenCalledWith(`${BASE}/1`, { name: 'Euro', symbol: '€', decimals: 2 })
  })
})

describe('deleteCurrency', () => {
  it('calls DELETE {id}', async () => {
    vi.mocked(api.del).mockResolvedValue(ok)
    await deleteCurrency(3)
    expect(api.del).toHaveBeenCalledWith(`${BASE}/3`)
  })
})

describe('getCurrencyDefaults', () => {
  it('calls GET {id}/defaults', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getCurrencyDefaults(2)
    expect(api.get).toHaveBeenCalledWith(`${BASE}/2/defaults`)
  })
})

describe('setDefaultRate', () => {
  it('calls POST defaults with fields', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await setDefaultRate(1, 2, 1.1)
    expect(api.post).toHaveBeenCalledWith(`${BASE}/defaults`, {
      sourceCurrencyId: 1,
      destinationCurrencyId: 2,
      rate: 1.1,
    })
  })
})
