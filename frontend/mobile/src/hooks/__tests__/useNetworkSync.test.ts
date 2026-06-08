import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, act } from '@testing-library/react'
import { useNetworkSync } from '@/hooks/useNetworkSync'

vi.mock('@/hooks/useOfflineQueue', () => ({
  useOfflineQueue: () => ({
    enqueue: vi.fn(),
    drain: vi.fn().mockResolvedValue({ ok: 0, failed: 0 }),
    getAll: vi.fn().mockResolvedValue([]),
    clear: vi.fn(),
  }),
}))

const mockAddListener = vi.fn()
const mockGetStatus = vi.fn()

vi.mock('@capacitor/network', () => ({
  Network: {
    addListener: mockAddListener,
    getStatus: mockGetStatus,
    removeAllListeners: vi.fn(),
  },
}))

describe('useNetworkSync', () => {
  beforeEach(() => {
    mockGetStatus.mockResolvedValue({ connected: true, connectionType: 'wifi' })
    mockAddListener.mockResolvedValue({ remove: vi.fn() })
  })

  it('reports online when Network status is connected', async () => {
    mockGetStatus.mockResolvedValue({ connected: true, connectionType: 'wifi' })
    const { result } = renderHook(() => useNetworkSync())
    await act(async () => { await Promise.resolve() })
    expect(result.current.isOnline).toBe(true)
  })

  it('reports offline when Network status is disconnected', async () => {
    mockGetStatus.mockResolvedValue({ connected: false, connectionType: 'none' })
    const { result } = renderHook(() => useNetworkSync())
    await act(async () => { await Promise.resolve() })
    expect(result.current.isOnline).toBe(false)
  })

  it('registers a networkStatusChange listener on mount', async () => {
    renderHook(() => useNetworkSync())
    await act(async () => { await Promise.resolve() })
    expect(mockAddListener).toHaveBeenCalledWith('networkStatusChange', expect.any(Function))
  })

  it('lastSync is null initially', () => {
    const { result } = renderHook(() => useNetworkSync())
    expect(result.current.lastSync).toBeNull()
  })
})
