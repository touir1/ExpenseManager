import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, waitFor, act } from '@testing-library/react'
import { NotificationProvider, useNotifications } from '../NotificationContext'
import * as notifApi from '@/features/notifications/services/notificationApi.service'

vi.mock('@/features/notifications/services/notificationApi.service', () => ({
  getNotifications: vi.fn(),
  getUnreadCount: vi.fn(),
  markAsRead: vi.fn(),
  markAllAsRead: vi.fn(),
}))

const mockUseAuth = vi.fn()
vi.mock('@/features/auth/AuthContext', () => ({
  useAuth: () => mockUseAuth(),
}))

// Mock SignalR — prevent dynamic import from running in tests
vi.mock('@microsoft/signalr', () => {
  const mockConn = {
    on: vi.fn(),
    start: vi.fn().mockResolvedValue(undefined),
    stop: vi.fn().mockResolvedValue(undefined),
  }
  class MockHubConnectionBuilder {
    withUrl() { return this }
    withAutomaticReconnect() { return this }
    configureLogging() { return this }
    build() { return mockConn }
  }
  return {
    HubConnectionBuilder: MockHubConnectionBuilder,
    LogLevel: { Warning: 2 },
  }
})

const mockNotif = {
  id: 1,
  type: 'FAMILY_MEMBER_REMOVED',
  payload: { type: 'FAMILY_MEMBER_REMOVED', familyId: 1, familyName: 'Smith', removedByUserId: 2, removedByName: 'Alice', expenseCount: 0 },
  isRead: false,
  createdAt: '2026-06-01T10:00:00Z',
  readAt: null,
}

describe('NotificationContext', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUseAuth.mockReturnValue({ isAuthenticated: true })
    vi.mocked(notifApi.getNotifications).mockResolvedValue({ ok: true, status: 200, data: [mockNotif] })
    vi.mocked(notifApi.getUnreadCount).mockResolvedValue({ ok: true, status: 200, data: { count: 1 } })
    vi.mocked(notifApi.markAsRead).mockResolvedValue({ ok: true, status: 204 })
    vi.mocked(notifApi.markAllAsRead).mockResolvedValue({ ok: true, status: 204 })
  })

  describe('initialization', () => {
    it('loads notifications and unread count when authenticated', async () => {
      const { result } = renderHook(() => useNotifications(), { wrapper: NotificationProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.notifications).toEqual([mockNotif])
      expect(result.current.unreadCount).toBe(1)
    })

    it('does not load when unauthenticated', async () => {
      mockUseAuth.mockReturnValue({ isAuthenticated: false })
      const { result } = renderHook(() => useNotifications(), { wrapper: NotificationProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(notifApi.getNotifications).not.toHaveBeenCalled()
      expect(result.current.notifications).toEqual([])
      expect(result.current.unreadCount).toBe(0)
    })

    it('clears state when unauthenticated', async () => {
      mockUseAuth.mockReturnValue({ isAuthenticated: false })
      const { result } = renderHook(() => useNotifications(), { wrapper: NotificationProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.notifications).toEqual([])
      expect(result.current.unreadCount).toBe(0)
    })

    it('handles failed notifications API response gracefully', async () => {
      vi.mocked(notifApi.getNotifications).mockResolvedValue({ ok: false, status: 500 })
      vi.mocked(notifApi.getUnreadCount).mockResolvedValue({ ok: false, status: 500 })
      const { result } = renderHook(() => useNotifications(), { wrapper: NotificationProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.notifications).toEqual([])
      expect(result.current.unreadCount).toBe(0)
    })
  })

  describe('markRead', () => {
    it('calls markAsRead and updates notification + decrements count', async () => {
      const { result } = renderHook(() => useNotifications(), { wrapper: NotificationProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      await act(() => result.current.markRead(1))
      expect(notifApi.markAsRead).toHaveBeenCalledWith(1)
      expect(result.current.notifications[0].isRead).toBe(true)
      expect(result.current.unreadCount).toBe(0)
    })

    it('does not decrement below 0', async () => {
      vi.mocked(notifApi.getUnreadCount).mockResolvedValue({ ok: true, status: 200, data: { count: 0 } })
      const { result } = renderHook(() => useNotifications(), { wrapper: NotificationProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      await act(() => result.current.markRead(1))
      expect(result.current.unreadCount).toBe(0)
    })
  })

  describe('markAllRead', () => {
    it('calls markAllAsRead and marks all read + zeros count', async () => {
      const { result } = renderHook(() => useNotifications(), { wrapper: NotificationProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      await act(() => result.current.markAllRead())
      expect(notifApi.markAllAsRead).toHaveBeenCalled()
      expect(result.current.notifications.every(n => n.isRead)).toBe(true)
      expect(result.current.unreadCount).toBe(0)
    })
  })

  describe('refresh', () => {
    it('re-fetches notifications and count', async () => {
      const { result } = renderHook(() => useNotifications(), { wrapper: NotificationProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(notifApi.getNotifications).toHaveBeenCalledTimes(1)
      await act(() => result.current.refresh())
      expect(notifApi.getNotifications).toHaveBeenCalledTimes(2)
    })
  })

  describe('useNotifications hook', () => {
    it('throws when used outside NotificationProvider', () => {
      expect(() => renderHook(() => useNotifications())).toThrow(
        'useNotifications must be used within NotificationProvider'
      )
    })
  })
})
