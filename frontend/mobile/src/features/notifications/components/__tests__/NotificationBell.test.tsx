import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'

const mockMarkAllRead = vi.fn()
const mockUseNotifications = vi.fn()

vi.mock('@/features/notifications/NotificationContext', () => ({
  useNotifications: () => mockUseNotifications(),
}))

vi.mock('@ionic/react', async () => ({
  IonButton: ({ children, onClick, id, slot }: any) => (
    <button onClick={onClick} id={id} data-slot={slot}>{children}</button>
  ),
  IonIcon: ({ icon, color, ['data-testid']: testId }: any) => (
    <span data-testid={testId} data-icon={icon} data-color={color} />
  ),
  IonBadge: ({ children, color }: any) => <span data-color={color}>{children}</span>,
  IonPopover: ({ children, isOpen }: any) => (isOpen ? <div>{children}</div> : null),
  IonList: ({ children }: any) => <ul>{children}</ul>,
  IonItem: ({ children }: any) => <li>{children}</li>,
  IonLabel: ({ children }: any) => <label>{children}</label>,
  IonText: ({ children }: any) => <span>{children}</span>,
}))

import { NotificationBell } from '../NotificationBell'

const makeNotif = (overrides = {}) => ({
  id: 1,
  type: 'FAMILY_MEMBER_REMOVED',
  payload: { type: 'FAMILY_MEMBER_REMOVED', familyId: 1, familyName: 'Smith', removedByUserId: 2, removedByName: 'Alice', expenseCount: 3 },
  isRead: false,
  createdAt: '2026-06-01T10:00:00Z',
  readAt: null,
  ...overrides,
})

describe('NotificationBell (mobile)', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUseNotifications.mockReturnValue({
      notifications: [],
      unreadCount: 0,
      markAllRead: mockMarkAllRead,
    })
  })

  it('renders without a badge when unreadCount is 0', () => {
    render(<NotificationBell />)
    expect(screen.getByRole('button')).toBeInTheDocument()
  })

  it('shows unread badge count', () => {
    mockUseNotifications.mockReturnValue({
      notifications: [],
      unreadCount: 3,
      markAllRead: mockMarkAllRead,
    })
    render(<NotificationBell />)
    expect(screen.getByText('3')).toBeInTheDocument()
  })

  it('opens popover and renders an icon per notification type', () => {
    mockUseNotifications.mockReturnValue({
      notifications: [makeNotif()],
      unreadCount: 1,
      markAllRead: mockMarkAllRead,
    })
    render(<NotificationBell />)
    fireEvent.click(screen.getByRole('button'))
    expect(screen.getByTestId('notification-icon-FAMILY_MEMBER_REMOVED')).toBeInTheDocument()
  })

  it('renders the default icon for an unrecognized type', () => {
    mockUseNotifications.mockReturnValue({
      notifications: [{ id: 1, type: 'UNKNOWN_TYPE', payload: {}, isRead: true, createdAt: '2026-06-01T10:00:00Z', readAt: null }],
      unreadCount: 0,
      markAllRead: mockMarkAllRead,
    })
    render(<NotificationBell />)
    fireEvent.click(screen.getByRole('button'))
    expect(screen.getByTestId('notification-icon-UNKNOWN_TYPE')).toBeInTheDocument()
  })

  it('calls markAllRead when mark all read is clicked', () => {
    mockUseNotifications.mockReturnValue({
      notifications: [makeNotif()],
      unreadCount: 1,
      markAllRead: mockMarkAllRead,
    })
    render(<NotificationBell />)
    fireEvent.click(screen.getByRole('button'))
    fireEvent.click(screen.getByText(/mark all read/i))
    expect(mockMarkAllRead).toHaveBeenCalled()
  })
})
