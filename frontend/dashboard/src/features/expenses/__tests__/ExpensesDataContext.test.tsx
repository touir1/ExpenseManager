import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { ExpensesDataProvider, useExpensesData } from '../ExpensesDataContext'
import * as categoriesService from '@/features/expenses/services/categoriesApi.service'
import * as currenciesService from '@/features/expenses/services/currenciesApi.service'
import * as tagsService from '@/features/tags/services/tagsApi.service'
import type { Category, Currency } from '../types/expenses.type'

vi.mock('@/features/expenses/services/categoriesApi.service', () => ({
  getCategories: vi.fn(),
}))

vi.mock('@/features/expenses/services/currenciesApi.service', () => ({
  getCurrencies: vi.fn(),
}))

vi.mock('@/features/tags/services/tagsApi.service', () => ({
  getTags: vi.fn(),
}))

vi.mock('@/features/auth/AuthContext', () => ({
  useAuth: vi.fn().mockReturnValue({ isAuthenticated: true }),
}))

const mockCategories: Category[] = [
  { id: 1, name: 'Food', subcategories: [{ id: 2, name: 'Groceries' }] },
]
const mockCurrencies: Currency[] = [
  { id: 1, code: 'USD', name: 'US Dollar', symbol: '$', decimals: 2 },
]

describe('ExpensesDataContext', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(categoriesService.getCategories).mockResolvedValue({ ok: true, status: 200, data: mockCategories })
    vi.mocked(currenciesService.getCurrencies).mockResolvedValue({ ok: true, status: 200, data: mockCurrencies })
    vi.mocked(tagsService.getTags).mockResolvedValue({ ok: true, status: 200, data: { own: [], family: [] } })
  })

  describe('ExpensesDataProvider initialization', () => {
    it('resolves isLoading to false after fetch', async () => {
      const { result } = renderHook(() => useExpensesData(), { wrapper: ExpensesDataProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))
    })

    it('loads categories and currencies on mount when authenticated', async () => {
      const { result } = renderHook(() => useExpensesData(), { wrapper: ExpensesDataProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))

      expect(categoriesService.getCategories).toHaveBeenCalledTimes(1)
      expect(currenciesService.getCurrencies).toHaveBeenCalledTimes(1)
    })

    it('sets categories from successful API response', async () => {
      const { result } = renderHook(() => useExpensesData(), { wrapper: ExpensesDataProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))

      expect(result.current.categories).toEqual(mockCategories)
    })

    it('sets currencies from successful API response', async () => {
      const { result } = renderHook(() => useExpensesData(), { wrapper: ExpensesDataProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))

      expect(result.current.currencies).toEqual(mockCurrencies)
    })

    it('leaves categories empty when API fails', async () => {
      vi.mocked(categoriesService.getCategories).mockResolvedValue({ ok: false, status: 500 })

      const { result } = renderHook(() => useExpensesData(), { wrapper: ExpensesDataProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))

      expect(result.current.categories).toEqual([])
    })

    it('leaves currencies empty when API fails', async () => {
      vi.mocked(currenciesService.getCurrencies).mockResolvedValue({ ok: false, status: 500 })

      const { result } = renderHook(() => useExpensesData(), { wrapper: ExpensesDataProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))

      expect(result.current.currencies).toEqual([])
    })

    it('fetches both APIs in parallel on mount', async () => {
      const order: string[] = []
      vi.mocked(categoriesService.getCategories).mockImplementation(async () => {
        order.push('categories')
        return { ok: true, status: 200, data: mockCategories }
      })
      vi.mocked(currenciesService.getCurrencies).mockImplementation(async () => {
        order.push('currencies')
        return { ok: true, status: 200, data: mockCurrencies }
      })

      const { result } = renderHook(() => useExpensesData(), { wrapper: ExpensesDataProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))

      expect(order).toContain('categories')
      expect(order).toContain('currencies')
    })

    it('does not fetch when not authenticated', async () => {
      const { useAuth } = await import('@/features/auth/AuthContext')
      vi.mocked(useAuth).mockReturnValue({ isAuthenticated: false } as any)

      const { result } = renderHook(() => useExpensesData(), { wrapper: ExpensesDataProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))

      expect(categoriesService.getCategories).not.toHaveBeenCalled()
      expect(currenciesService.getCurrencies).not.toHaveBeenCalled()

      // restore
      vi.mocked(useAuth).mockReturnValue({ isAuthenticated: true } as any)
    })

    it('clears data when unauthenticated', async () => {
      const { useAuth } = await import('@/features/auth/AuthContext')
      vi.mocked(useAuth).mockReturnValue({ isAuthenticated: false } as any)

      const { result } = renderHook(() => useExpensesData(), { wrapper: ExpensesDataProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))

      expect(result.current.categories).toEqual([])
      expect(result.current.currencies).toEqual([])

      vi.mocked(useAuth).mockReturnValue({ isAuthenticated: true } as any)
    })
  })

  describe('refresh', () => {
    it('exposes a refresh function', async () => {
      const { result } = renderHook(() => useExpensesData(), { wrapper: ExpensesDataProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))

      expect(typeof result.current.refresh).toBe('function')
    })

    it('re-fetches data when refresh is called', async () => {
      const { result } = renderHook(() => useExpensesData(), { wrapper: ExpensesDataProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(categoriesService.getCategories).toHaveBeenCalledTimes(1)

      await result.current.refresh()

      expect(categoriesService.getCategories).toHaveBeenCalledTimes(2)
      expect(currenciesService.getCurrencies).toHaveBeenCalledTimes(2)
    })
  })

  describe('useExpensesData hook', () => {
    it('throws when used outside ExpensesDataProvider', () => {
      expect(() => renderHook(() => useExpensesData())).toThrow(
        'useExpensesData must be used within ExpensesDataProvider'
      )
    })
  })
})
