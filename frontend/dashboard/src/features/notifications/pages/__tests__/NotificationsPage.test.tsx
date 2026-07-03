import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import NotificationsPage from '../NotificationsPage'

const mockMarkRead = vi.fn()
const mockMarkAllRead = vi.fn()
const mockGetNotifications = vi.fn()

const mockUseNotifications = vi.fn()
vi.mock('@/features/notifications/NotificationContext', () => ({
  useNotifications: () => mockUseNotifications(),
}))
vi.mock('@/features/notifications/services/notificationApi.service', () => ({
  getNotifications: (...args: unknown[]) => mockGetNotifications(...args),
}))

const makeNotif = (overrides = {}) => ({
  id: 1,
  type: 'FAMILY_MEMBER_REMOVED',
  payload: { type: 'FAMILY_MEMBER_REMOVED', familyId: 1, familyName: 'Smith', removedByUserId: 2, removedByName: 'Alice', expenseCount: 3 },
  isRead: false,
  createdAt: '2026-06-01T10:00:00Z',
  readAt: null,
  ...overrides,
})

function renderPage() {
  return render(<NotificationsPage />)
}

describe('NotificationsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUseNotifications.mockReturnValue({
      unreadCount: 0,
      markRead: mockMarkRead,
      markAllRead: mockMarkAllRead,
    })
    mockGetNotifications.mockResolvedValue({ ok: true, data: [] })
  })

  it('renders the page heading', async () => {
    renderPage()
    expect(screen.getByRole('heading', { name: /notifications/i })).toBeInTheDocument()
  })

  it('shows empty state when there are no notifications', async () => {
    renderPage()
    await waitFor(() => expect(screen.getByText(/all caught up/i)).toBeInTheDocument())
  })

  it('renders paged list of notifications', async () => {
    mockGetNotifications.mockResolvedValue({ ok: true, data: [makeNotif()] })
    renderPage()
    await waitFor(() => expect(screen.getByText(/Alice/)).toBeInTheDocument())
    expect(mockGetNotifications).toHaveBeenCalledWith(1, 20)
  })

  it('marks a notification as read on click', async () => {
    mockGetNotifications.mockResolvedValue({ ok: true, data: [makeNotif()] })
    const user = userEvent.setup()
    renderPage()
    const item = await screen.findByText(/Alice/)
    await user.click(item.closest('button')!)
    expect(mockMarkRead).toHaveBeenCalledWith(1)
  })

  it('shows Mark all read button when unreadCount > 0 and calls markAllRead', async () => {
    mockUseNotifications.mockReturnValue({
      unreadCount: 2,
      markRead: mockMarkRead,
      markAllRead: mockMarkAllRead,
    })
    mockGetNotifications.mockResolvedValue({ ok: true, data: [makeNotif()] })
    const user = userEvent.setup()
    renderPage()
    await screen.findByText(/Alice/)
    await user.click(screen.getByText('Mark all as read'))
    expect(mockMarkAllRead).toHaveBeenCalled()
  })

  it('paginates to the next page', async () => {
    const pageOne = Array.from({ length: 20 }, (_, i) => makeNotif({ id: i + 1 }))
    mockGetNotifications.mockResolvedValueOnce({ ok: true, data: pageOne })
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => expect(screen.getAllByText(/Alice/).length).toBeGreaterThan(0))

    mockGetNotifications.mockResolvedValueOnce({ ok: true, data: [] })
    await user.click(screen.getByText(/next/i))
    await waitFor(() => expect(mockGetNotifications).toHaveBeenCalledWith(2, 20))
  })
})
