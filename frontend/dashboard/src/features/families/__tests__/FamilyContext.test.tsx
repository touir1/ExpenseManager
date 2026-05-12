import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, waitFor, act } from '@testing-library/react'
import { FamilyProvider, useFamilies } from '../FamilyContext'
import * as familyApi from '@/features/families/services/familyApi.service'
import type { Family } from '../types/family.type'

vi.mock('@/features/families/services/familyApi.service', () => ({
  getFamilies: vi.fn(),
}))

const mockUseAuth = vi.fn()
vi.mock('@/features/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth(),
}))

const mockFamily: Family = {
  id: 1,
  name: 'Smith household',
  isDefault: true,
  isArchived: false,
  userRole: 'Head',
  createdAt: '2024-01-01T00:00:00Z',
}
const mockFamily2: Family = {
  id: 2,
  name: 'Holiday',
  isDefault: false,
  isArchived: false,
  userRole: 'Member',
  createdAt: '2024-02-01T00:00:00Z',
}

describe('FamilyContext', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    localStorage.clear()
    mockUseAuth.mockReturnValue({ isAuthenticated: true })
    vi.mocked(familyApi.getFamilies).mockResolvedValue({ ok: true, status: 200, data: [mockFamily] })
  })

  describe('FamilyProvider initialization', () => {
    it('loads families on mount when authenticated', async () => {
      const { result } = renderHook(() => useFamilies(), { wrapper: FamilyProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.families).toEqual([mockFamily])
    })

    it('sets isLoading true while fetching', async () => {
      let resolve!: (v: any) => void
      vi.mocked(familyApi.getFamilies).mockReturnValue(new Promise(r => { resolve = r }))
      const { result } = renderHook(() => useFamilies(), { wrapper: FamilyProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(true))
      resolve({ ok: true, status: 200, data: [] })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
    })

    it('does not fetch when not authenticated', async () => {
      mockUseAuth.mockReturnValue({ isAuthenticated: false })
      const { result } = renderHook(() => useFamilies(), { wrapper: FamilyProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(familyApi.getFamilies).not.toHaveBeenCalled()
      expect(result.current.families).toEqual([])
    })

    it('leaves families empty when API fails', async () => {
      vi.mocked(familyApi.getFamilies).mockResolvedValue({ ok: false, status: 500 })
      const { result } = renderHook(() => useFamilies(), { wrapper: FamilyProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.families).toEqual([])
    })

    it('clears families and activeFamilyId when unauthenticated', async () => {
      vi.mocked(familyApi.getFamilies).mockResolvedValue({ ok: true, status: 200, data: [mockFamily] })
      mockUseAuth.mockReturnValue({ isAuthenticated: false })
      localStorage.setItem('activeFamilyId', '1')
      const { result } = renderHook(() => useFamilies(), { wrapper: FamilyProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.families).toEqual([])
      expect(result.current.activeFamilyId).toBeNull()
      expect(localStorage.getItem('activeFamilyId')).toBeNull()
    })
  })

  describe('activeFamilyId', () => {
    it('reads stored activeFamilyId from localStorage on init', () => {
      localStorage.setItem('activeFamilyId', '42')
      const { result } = renderHook(() => useFamilies(), { wrapper: FamilyProvider })
      expect(result.current.activeFamilyId).toBe(42)
    })

    it('defaults activeFamilyId to null when localStorage is empty', () => {
      const { result } = renderHook(() => useFamilies(), { wrapper: FamilyProvider })
      expect(result.current.activeFamilyId).toBeNull()
    })

    it('clears stored activeFamilyId if family no longer exists in fetched list', async () => {
      localStorage.setItem('activeFamilyId', '99')
      vi.mocked(familyApi.getFamilies).mockResolvedValue({ ok: true, status: 200, data: [mockFamily] })
      const { result } = renderHook(() => useFamilies(), { wrapper: FamilyProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.activeFamilyId).toBeNull()
      expect(localStorage.getItem('activeFamilyId')).toBeNull()
    })

    it('clears stored activeFamilyId if family is archived', async () => {
      localStorage.setItem('activeFamilyId', '2')
      const archived: Family = { ...mockFamily2, id: 2, isArchived: true }
      vi.mocked(familyApi.getFamilies).mockResolvedValue({ ok: true, status: 200, data: [archived] })
      const { result } = renderHook(() => useFamilies(), { wrapper: FamilyProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.activeFamilyId).toBeNull()
    })

    it('keeps stored activeFamilyId if family still exists and is active', async () => {
      localStorage.setItem('activeFamilyId', '2')
      vi.mocked(familyApi.getFamilies).mockResolvedValue({
        ok: true, status: 200, data: [mockFamily, mockFamily2],
      })
      const { result } = renderHook(() => useFamilies(), { wrapper: FamilyProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.activeFamilyId).toBe(2)
    })
  })

  describe('setActiveFamilyId', () => {
    it('sets activeFamilyId and persists to localStorage', async () => {
      const { result } = renderHook(() => useFamilies(), { wrapper: FamilyProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      act(() => result.current.setActiveFamilyId(5))
      expect(result.current.activeFamilyId).toBe(5)
      expect(localStorage.getItem('activeFamilyId')).toBe('5')
    })

    it('clears activeFamilyId and removes from localStorage when set to null', async () => {
      localStorage.setItem('activeFamilyId', '5')
      const { result } = renderHook(() => useFamilies(), { wrapper: FamilyProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      act(() => result.current.setActiveFamilyId(null))
      expect(result.current.activeFamilyId).toBeNull()
      expect(localStorage.getItem('activeFamilyId')).toBeNull()
    })
  })

  describe('refresh', () => {
    it('exposes a refresh function', async () => {
      const { result } = renderHook(() => useFamilies(), { wrapper: FamilyProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(typeof result.current.refresh).toBe('function')
    })

    it('re-fetches families when refresh is called', async () => {
      const { result } = renderHook(() => useFamilies(), { wrapper: FamilyProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(familyApi.getFamilies).toHaveBeenCalledTimes(1)
      await act(() => result.current.refresh())
      expect(familyApi.getFamilies).toHaveBeenCalledTimes(2)
    })
  })

  describe('useFamilies hook', () => {
    it('throws when used outside FamilyProvider', () => {
      expect(() => renderHook(() => useFamilies())).toThrow(
        'useFamilies must be used within FamilyProvider'
      )
    })
  })
})
