import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import NotificationBell from '../NotificationBell'

const mockMarkRead = vi.fn()
const mockMarkAllRead = vi.fn()
const mockShow = vi.fn()

const makeNotif = (overrides = {}) => ({
  id: 1,
  type: 'FAMILY_MEMBER_REMOVED',
  payload: { type: 'FAMILY_MEMBER_REMOVED', familyId: 1, familyName: 'Smith', removedByUserId: 2, removedByName: 'Alice', expenseCount: 3 },
  isRead: false,
  createdAt: '2026-06-01T10:00:00Z',
  readAt: null,
  ...overrides,
})

const mockUseNotifications = vi.fn()
vi.mock('@/features/notifications/NotificationContext', () => ({
  useNotifications: () => mockUseNotifications(),
}))
vi.mock('@/components/Toast', () => ({
  useToast: () => ({ show: mockShow }),
}))

function renderBell() {
  return render(<NotificationBell />)
}

describe('NotificationBell', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUseNotifications.mockReturnValue({
      notifications: [],
      unreadCount: 0,
      isLoading: false,
      markRead: mockMarkRead,
      markAllRead: mockMarkAllRead,
    })
  })

  describe('badge', () => {
    it('shows no badge when unreadCount is 0', () => {
      renderBell()
      expect(screen.queryByLabelText(/unread/i)).not.toBeInTheDocument()
    })

    it('shows badge with count', () => {
      mockUseNotifications.mockReturnValue({
        notifications: [],
        unreadCount: 3,
        isLoading: false,
        markRead: mockMarkRead,
        markAllRead: mockMarkAllRead,
      })
      renderBell()
      expect(screen.getByText('3')).toBeInTheDocument()
    })

    it('shows 9+ when unreadCount > 9', () => {
      mockUseNotifications.mockReturnValue({
        notifications: [],
        unreadCount: 15,
        isLoading: false,
        markRead: mockMarkRead,
        markAllRead: mockMarkAllRead,
      })
      renderBell()
      expect(screen.getByText('9+')).toBeInTheDocument()
    })
  })

  describe('dropdown', () => {
    it('is closed by default', () => {
      renderBell()
      expect(screen.queryByText(/notifications/i)).not.toBeInTheDocument()
    })

    it('opens on bell click', async () => {
      const user = userEvent.setup()
      renderBell()
      await user.click(screen.getByRole('button', { name: /notifications/i }))
      expect(screen.getByText('No notifications yet')).toBeInTheDocument()
    })

    it('shows loading indicator when loading and no notifications', async () => {
      mockUseNotifications.mockReturnValue({
        notifications: [],
        unreadCount: 0,
        isLoading: true,
        markRead: mockMarkRead,
        markAllRead: mockMarkAllRead,
      })
      const user = userEvent.setup()
      renderBell()
      await user.click(screen.getByRole('button', { name: /notifications/i }))
      expect(screen.getByText('…')).toBeInTheDocument()
    })

    it('renders notification items', async () => {
      mockUseNotifications.mockReturnValue({
        notifications: [makeNotif()],
        unreadCount: 1,
        isLoading: false,
        markRead: mockMarkRead,
        markAllRead: mockMarkAllRead,
      })
      const user = userEvent.setup()
      renderBell()
      await user.click(screen.getByRole('button', { name: /notifications/i }))
      expect(screen.getByText(/Alice/)).toBeInTheDocument()
    })

    it('shows mark all read button when there are unread notifications', async () => {
      mockUseNotifications.mockReturnValue({
        notifications: [makeNotif()],
        unreadCount: 1,
        isLoading: false,
        markRead: mockMarkRead,
        markAllRead: mockMarkAllRead,
      })
      const user = userEvent.setup()
      renderBell()
      await user.click(screen.getByRole('button', { name: /notifications/i }))
      expect(screen.getByText('Mark all as read')).toBeInTheDocument()
    })

    it('calls markAllRead when mark all read clicked', async () => {
      mockUseNotifications.mockReturnValue({
        notifications: [makeNotif()],
        unreadCount: 1,
        isLoading: false,
        markRead: mockMarkRead,
        markAllRead: mockMarkAllRead,
      })
      const user = userEvent.setup()
      renderBell()
      await user.click(screen.getByRole('button', { name: /notifications/i }))
      await user.click(screen.getByText('Mark all as read'))
      expect(mockMarkAllRead).toHaveBeenCalled()
    })

    it('calls markRead and closes dropdown when notification clicked', async () => {
      mockUseNotifications.mockReturnValue({
        notifications: [makeNotif()],
        unreadCount: 1,
        isLoading: false,
        markRead: mockMarkRead,
        markAllRead: mockMarkAllRead,
      })
      const user = userEvent.setup()
      renderBell()
      await user.click(screen.getByRole('button', { name: /notifications/i }))
      const notifBtn = screen.getByText(/Alice/).closest('button')!
      await user.click(notifBtn)
      expect(mockMarkRead).toHaveBeenCalledWith(1)
      expect(screen.queryByText('No notifications yet')).not.toBeInTheDocument()
    })

    it('closes when clicking outside', async () => {
      const user = userEvent.setup()
      renderBell()
      await user.click(screen.getByRole('button', { name: /notifications/i }))
      expect(screen.getByText('No notifications yet')).toBeInTheDocument()
      await user.click(document.body)
      await waitFor(() => expect(screen.queryByText('No notifications yet')).not.toBeInTheDocument())
    })
  })

  describe('notification text rendering', () => {
    const cases: Array<{ type: string; payload: Record<string, unknown>; expectedText: string }> = [
      {
        type: 'FAMILY_INVITATION_ACCEPTED',
        payload: { type: 'FAMILY_INVITATION_ACCEPTED', familyId: 1, familyName: 'Smith', acceptorName: 'Bob', acceptorEmail: 'bob@example.com' },
        expectedText: 'Bob',
      },
      {
        type: 'FAMILY_MEMBER_JOINED',
        payload: { type: 'FAMILY_MEMBER_JOINED', familyId: 1, familyName: 'Smith', joinerName: 'Carol', joinerUserId: 3 },
        expectedText: 'Carol',
      },
      {
        type: 'FAMILY_EXPENSE_ADDED',
        payload: { type: 'FAMILY_EXPENSE_ADDED', familyId: 1, familyName: 'Smith', expenseId: 9, amount: 42.5, currencyCode: 'EUR', actorName: 'Dave', actorUserId: 4 },
        expectedText: 'Dave',
      },
      {
        type: 'FAMILY_EXPENSE_DELETED',
        payload: { type: 'FAMILY_EXPENSE_DELETED', familyId: 1, familyName: 'Smith', expenseId: 9, amount: 42.5, currencyCode: 'EUR', actorName: 'Eve', actorUserId: 5 },
        expectedText: 'Eve',
      },
      {
        type: 'CSV_IMPORT_COMPLETED',
        payload: { type: 'CSV_IMPORT_COMPLETED', totalRows: 10, importedCount: 8, skippedCount: 2 },
        expectedText: '8',
      },
      {
        type: 'RATE_CONFLICT_CREATED',
        payload: { type: 'RATE_CONFLICT_CREATED', conflictId: 1, sourceCurrencyCode: 'EUR', destCurrencyCode: 'USD', date: '2026-01-01', autoRate: 1.1, manualRate: 1.05 },
        expectedText: 'EUR',
      },
    ]

    cases.forEach(({ type, payload, expectedText }) => {
      it(`renders label for ${type}`, async () => {
        mockUseNotifications.mockReturnValue({
          notifications: [{ id: 1, type, payload, isRead: true, createdAt: '2026-06-01T10:00:00Z', readAt: null }],
          unreadCount: 0,
          isLoading: false,
          markRead: mockMarkRead,
          markAllRead: mockMarkAllRead,
        })
        const user = userEvent.setup()
        renderBell()
        await user.click(screen.getByRole('button', { name: /notifications/i }))
        expect(screen.getAllByText((_, el) => (el?.textContent ?? '').includes(expectedText)).length).toBeGreaterThan(0)
      })
    })

    it('falls back to raw type for unknown notification type', async () => {
      mockUseNotifications.mockReturnValue({
        notifications: [{ id: 1, type: 'UNKNOWN_TYPE', payload: {}, isRead: true, createdAt: '2026-06-01T10:00:00Z', readAt: null }],
        unreadCount: 0,
        isLoading: false,
        markRead: mockMarkRead,
        markAllRead: mockMarkAllRead,
      })
      const user = userEvent.setup()
      renderBell()
      await user.click(screen.getByRole('button', { name: /notifications/i }))
      expect(screen.getByText('UNKNOWN_TYPE')).toBeInTheDocument()
    })
  })

  describe('toast on new notification', () => {
    it('shows toast when unreadCount increases', async () => {
      const { rerender } = render(<NotificationBell />)
      mockUseNotifications.mockReturnValue({
        notifications: [makeNotif()],
        unreadCount: 1,
        isLoading: false,
        markRead: mockMarkRead,
        markAllRead: mockMarkAllRead,
      })
      rerender(<NotificationBell />)
      await waitFor(() => expect(mockShow).toHaveBeenCalled())
    })
  })
})
