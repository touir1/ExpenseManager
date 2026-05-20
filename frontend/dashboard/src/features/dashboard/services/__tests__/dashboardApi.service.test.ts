import { describe, it, expect, vi, beforeEach } from 'vitest'
import * as api from '@/services/api.service'
import {
  getSummary,
  getMonthly,
  getCategories,
  getSameMonthYearly,
  getByCurrency,
  getRecent,
} from '../dashboardApi.service'

vi.mock('@/services/api.service', () => ({ get: vi.fn() }))

const BASE = '/api/expenses/dashboard'

beforeEach(() => vi.clearAllMocks())

describe('getSummary', () => {
  it('calls correct URL with all filter params', async () => {
    vi.mocked(api.get).mockResolvedValue({ ok: true, data: {} })
    await getSummary({ familyId: 1, dateFrom: '2024-01-01', dateTo: '2024-01-31', displayCurrencyId: 2 })
    expect(api.get).toHaveBeenCalledWith(
      `${BASE}/summary?familyId=1&dateFrom=2024-01-01&dateTo=2024-01-31&displayCurrencyId=2`,
    )
  })

  it('omits undefined params', async () => {
    vi.mocked(api.get).mockResolvedValue({ ok: true, data: {} })
    await getSummary({})
    const url = vi.mocked(api.get).mock.calls[0][0] as string
    expect(url).toBe(`${BASE}/summary`)
  })

  it('uses no params when called with no args', async () => {
    vi.mocked(api.get).mockResolvedValue({ ok: true, data: {} })
    await getSummary()
    expect(api.get).toHaveBeenCalledWith(`${BASE}/summary`)
  })
})

describe('getMonthly', () => {
  it('calls correct URL with filter params', async () => {
    vi.mocked(api.get).mockResolvedValue({ ok: true, data: [] })
    await getMonthly({ familyId: 3, dateFrom: '2024-01-01', dateTo: '2024-12-31' })
    expect(api.get).toHaveBeenCalledWith(
      `${BASE}/monthly?familyId=3&dateFrom=2024-01-01&dateTo=2024-12-31`,
    )
  })

  it('calls without params when empty filter', async () => {
    vi.mocked(api.get).mockResolvedValue({ ok: true, data: [] })
    await getMonthly({})
    expect(api.get).toHaveBeenCalledWith(`${BASE}/monthly`)
  })
})

describe('getCategories', () => {
  it('calls correct URL with filter params', async () => {
    vi.mocked(api.get).mockResolvedValue({ ok: true, data: [] })
    await getCategories({ dateFrom: '2024-11-01', dateTo: '2024-11-30', displayCurrencyId: 5 })
    expect(api.get).toHaveBeenCalledWith(
      `${BASE}/categories?dateFrom=2024-11-01&dateTo=2024-11-30&displayCurrencyId=5`,
    )
  })

  it('calls without params when empty filter', async () => {
    vi.mocked(api.get).mockResolvedValue({ ok: true, data: [] })
    await getCategories()
    expect(api.get).toHaveBeenCalledWith(`${BASE}/categories`)
  })
})

describe('getSameMonthYearly', () => {
  it('calls correct URL with month and optional params', async () => {
    vi.mocked(api.get).mockResolvedValue({ ok: true, data: [] })
    await getSameMonthYearly(5, 1, 2)
    expect(api.get).toHaveBeenCalledWith(
      `${BASE}/same-month-across-years?month=5&familyId=1&displayCurrencyId=2`,
    )
  })

  it('omits familyId and displayCurrencyId when undefined', async () => {
    vi.mocked(api.get).mockResolvedValue({ ok: true, data: [] })
    await getSameMonthYearly(11)
    expect(api.get).toHaveBeenCalledWith(`${BASE}/same-month-across-years?month=11`)
  })

  it('includes only familyId when displayCurrencyId omitted', async () => {
    vi.mocked(api.get).mockResolvedValue({ ok: true, data: [] })
    await getSameMonthYearly(3, 7)
    expect(api.get).toHaveBeenCalledWith(`${BASE}/same-month-across-years?month=3&familyId=7`)
  })
})

describe('getByCurrency', () => {
  it('calls correct URL with filter params', async () => {
    vi.mocked(api.get).mockResolvedValue({ ok: true, data: [] })
    await getByCurrency({ familyId: 2, dateFrom: '2024-06-01', dateTo: '2024-06-30' })
    expect(api.get).toHaveBeenCalledWith(
      `${BASE}/by-currency?familyId=2&dateFrom=2024-06-01&dateTo=2024-06-30`,
    )
  })

  it('calls without params when empty filter', async () => {
    vi.mocked(api.get).mockResolvedValue({ ok: true, data: [] })
    await getByCurrency()
    expect(api.get).toHaveBeenCalledWith(`${BASE}/by-currency`)
  })
})

describe('getRecent', () => {
  it('calls correct URL with filter params', async () => {
    vi.mocked(api.get).mockResolvedValue({ ok: true, data: { items: [], totalCount: 0 } })
    await getRecent({ dateFrom: '2024-01-01', dateTo: '2024-12-31', displayCurrencyId: 3 })
    expect(api.get).toHaveBeenCalledWith(
      `${BASE}/recent?dateFrom=2024-01-01&dateTo=2024-12-31&displayCurrencyId=3`,
    )
  })

  it('calls without params when empty filter', async () => {
    vi.mocked(api.get).mockResolvedValue({ ok: true, data: { items: [], totalCount: 0 } })
    await getRecent()
    expect(api.get).toHaveBeenCalledWith(`${BASE}/recent`)
  })
})
