import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, act, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'

vi.mock('@/features/notifications/services/notificationApi.service', () => ({
  getNotifications: vi.fn(),
  getUnreadCount: vi.fn(),
  markAsRead: vi.fn(),
  markAllAsRead: vi.fn(),
  registerPushToken: vi.fn(),
}))

vi.mock('@/features/auth/AuthContext', () => ({
  useAuth: vi.fn(),
  AuthProvider: ({ children }: any) => <>{children}</>,
}))

vi.mock('@capacitor/push-notifications', () => ({
  PushNotifications: {
    requestPermissions: vi.fn().mockResolvedValue({ receive: 'denied' }),
    register: vi.fn(),
    addListener: vi.fn().mockResolvedValue({ remove: vi.fn() }),
  },
}))

class MockHubConnectionBuilder {
  private handlers: Record<string, (data: any) => void> = {}
  private conn: any

  withUrl() { return this }
  withAutomaticReconnect() { return this }
  configureLogging() { return this }

  build() {
    this.conn = {
      on: (event: string, handler: (data: any) => void) => { this.handlers[event] = handler },
      start: () => Promise.resolve(),
      stop: () => Promise.resolve(),
      _trigger: (event: string, data: any) => this.handlers[event]?.(data),
    }
    return this.conn
  }
}

vi.mock('@microsoft/signalr', () => ({
  HubConnectionBuilder: MockHubConnectionBuilder,
  LogLevel: { Warning: 1 },
}))

import { useAuth } from '@/features/auth/AuthContext'
import {
  getNotifications,
  getUnreadCount,
  markAllAsRead,
} from '@/features/notifications/services/notificationApi.service'
import { NotificationProvider, useNotifications } from '@/features/notifications/NotificationContext'

const useAuthMock = useAuth as ReturnType<typeof vi.fn>
const mockGetNotifications = getNotifications as ReturnType<typeof vi.fn>
const mockGetUnreadCount = getUnreadCount as ReturnType<typeof vi.fn>
const mockMarkAllAsRead = markAllAsRead as ReturnType<typeof vi.fn>

const mockNotification = {
  id: 1,
  type: 'FAMILY_MEMBER_REMOVED',
  payload: { type: 'FAMILY_MEMBER_REMOVED', familyId: 1, familyName: 'Smith', removedByUserId: 2, removedByName: 'Alice', expenseCount: 5 },
  isRead: false,
  createdAt: new Date().toISOString(),
  readAt: null,
}

function TestConsumer() {
  const { notifications, unreadCount, markAllRead } = useNotifications()
  return (
    <div>
      <span data-testid="count">{unreadCount}</span>
      <span data-testid="notif-count">{notifications.length}</span>
      <button onClick={markAllRead}>Mark all</button>
    </div>
  )
}

function makeWrapper(authenticated = true) {
  const qc = new QueryClient()
  useAuthMock.mockReturnValue({ isAuthenticated: authenticated })
  return ({ children }: any) => (
    <QueryClientProvider client={qc}>
      <NotificationProvider>{children}</NotificationProvider>
    </QueryClientProvider>
  )
}

describe('NotificationContext', () => {
  beforeEach(() => {
    mockGetNotifications.mockReset()
    mockGetUnreadCount.mockReset()
    mockMarkAllAsRead.mockReset()
    mockGetNotifications.mockResolvedValue({ ok: true, status: 200, data: [mockNotification] })
    mockGetUnreadCount.mockResolvedValue({ ok: true, status: 200, data: { count: 1 } })
    mockMarkAllAsRead.mockResolvedValue({ ok: true, status: 204 })
  })

  it('loads notifications and unread count on mount', async () => {
    render(<TestConsumer />, { wrapper: makeWrapper() })
    await waitFor(() => {
      expect(screen.getByTestId('count').textContent).toBe('1')
      expect(screen.getByTestId('notif-count').textContent).toBe('1')
    })
  })

  it('does not load when not authenticated', async () => {
    render(<TestConsumer />, { wrapper: makeWrapper(false) })
    await act(async () => { await Promise.resolve() })
    expect(mockGetNotifications).not.toHaveBeenCalled()
    expect(screen.getByTestId('count').textContent).toBe('0')
  })

  it('markAllRead calls API and resets unread count to 0', async () => {
    render(<TestConsumer />, { wrapper: makeWrapper() })
    await waitFor(() => expect(screen.getByTestId('count').textContent).toBe('1'))
    act(() => { screen.getByRole('button', { name: 'Mark all' }).click() })
    await waitFor(() => {
      expect(mockMarkAllAsRead).toHaveBeenCalled()
      expect(screen.getByTestId('count').textContent).toBe('0')
    })
  })
})
