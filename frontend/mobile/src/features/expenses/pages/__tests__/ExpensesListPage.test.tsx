import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'

vi.mock('@/features/expenses/services/expensesApi.service', () => ({
  getExpenses: vi.fn(),
  deleteExpense: vi.fn(),
}))

vi.mock('@/features/families/FamilyContext', () => ({
  useFamilies: () => ({ families: [], activeFamilyId: null, setActiveFamilyId: vi.fn(), isLoading: false, refresh: vi.fn() }),
}))

vi.mock('@/features/currencies/DisplayCurrencyContext', () => ({
  useDisplayCurrency: () => ({ displayCurrencyId: null, setDisplayCurrencyId: vi.fn() }),
}))

vi.mock('@/features/notifications/components/NotificationBell', () => ({
  NotificationBell: () => <div data-testid="notification-bell" />,
}))

vi.mock('@ionic/react', async () => ({
  IonPage: ({ children }: any) => <div>{children}</div>,
  IonHeader: ({ children }: any) => <div>{children}</div>,
  IonToolbar: ({ children }: any) => <div>{children}</div>,
  IonTitle: ({ children }: any) => <h1>{children}</h1>,
  IonContent: ({ children }: any) => <div>{children}</div>,
  IonList: ({ children }: any) => <ul>{children}</ul>,
  IonItem: ({ children }: any) => <li>{children}</li>,
  IonLabel: ({ children }: any) => <span>{children}</span>,
  IonItemDivider: ({ children }: any) => <li style={{ fontWeight: 700 }}>{children}</li>,
  IonItemSliding: ({ children, ref: _r }: any) => <div>{children}</div>,
  IonItemOption: ({ children, onClick }: any) => <button onClick={onClick}>{children}</button>,
  IonItemOptions: ({ children }: any) => <div>{children}</div>,
  IonRefresher: ({ onIonRefresh }: any) => <button onClick={() => onIonRefresh?.({ target: { complete: vi.fn() } })}>Refresh</button>,
  IonRefresherContent: () => null,
  IonInfiniteScroll: ({ children }: any) => <div>{children}</div>,
  IonInfiniteScrollContent: () => null,
  IonSegment: ({ children, onIonChange, value }: any) => (
    <div data-value={value} onChange={onIonChange}>{children}</div>
  ),
  IonSegmentButton: ({ children, value }: any) => <button value={value}>{children}</button>,
  IonAlert: ({ isOpen, buttons }: any) => isOpen ? (
    <div role="alertdialog">
      {buttons?.map((b: any) => <button key={b.text} onClick={b.handler}>{b.text}</button>)}
    </div>
  ) : null,
  IonText: ({ children }: any) => <span>{children}</span>,
  IonSkeletonText: () => <span>Loading</span>,
  IonBadge: ({ children }: any) => <span>{children}</span>,
}))

import { getExpenses, deleteExpense } from '@/features/expenses/services/expensesApi.service'
import ExpensesListPage from '@/features/expenses/pages/ExpensesListPage'

const mockGetExpenses = getExpenses as ReturnType<typeof vi.fn>
const mockDeleteExpense = deleteExpense as ReturnType<typeof vi.fn>

const mockExpense = {
  id: 1,
  amount: 25.5,
  currency: { id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
  date: '2024-01-15',
  category: { id: 1, name: 'Food' },
  subcategory: null,
  description: 'Groceries',
  createdAt: '2024-01-15T10:00:00Z',
  modifiedAt: null,
  modifiedFrom: null,
  tags: [],
  families: [],
  convertedAmount: null,
  displayCurrency: null,
}

const pagedResponse = {
  items: [mockExpense],
  totalCount: 1,
  page: 1,
  pageSize: 20,
  totalPages: 1,
}

function makeWrapper() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return ({ children }: any) => (
    <MemoryRouter>
      <QueryClientProvider client={qc}>{children}</QueryClientProvider>
    </MemoryRouter>
  )
}

describe('ExpensesListPage', () => {
  beforeEach(() => {
    mockGetExpenses.mockReset()
    mockDeleteExpense.mockReset()
    mockGetExpenses.mockResolvedValue({ ok: true, status: 200, data: pagedResponse })
  })

  it('renders day-grouped expense list', async () => {
    render(<ExpensesListPage />, { wrapper: makeWrapper() })
    await waitFor(() => {
      expect(screen.getAllByText('Food').length).toBeGreaterThan(0)
    })
  })

  it('shows empty state when no expenses', async () => {
    mockGetExpenses.mockResolvedValue({ ok: true, status: 200, data: { ...pagedResponse, items: [], totalCount: 0 } })
    render(<ExpensesListPage />, { wrapper: makeWrapper() })
    await waitFor(() => {
      expect(screen.getAllByText(/no expenses/i).length).toBeGreaterThan(0)
    })
  })

  it('calls deleteExpense after swipe-delete confirm', async () => {
    mockDeleteExpense.mockResolvedValue({ ok: true, status: 204 })
    render(<ExpensesListPage />, { wrapper: makeWrapper() })
    await waitFor(() => screen.getAllByText('Food'))
    const deleteBtn = screen.getAllByText('Delete')[0]
    fireEvent.click(deleteBtn)
    await waitFor(() => screen.getByRole('alertdialog'))
    const confirmBtn = screen.getAllByRole('button', { name: /delete/i }).find(
      b => b.closest('[role="alertdialog"]') !== null
    )
    if (confirmBtn) fireEvent.click(confirmBtn)
    await waitFor(() => {
      expect(mockDeleteExpense).toHaveBeenCalledWith(1)
    })
  })

  it('calls getExpenses on pull-to-refresh', async () => {
    render(<ExpensesListPage />, { wrapper: makeWrapper() })
    const refreshBtn = screen.getByText('Refresh')
    fireEvent.click(refreshBtn)
    await waitFor(() => {
      expect(mockGetExpenses).toHaveBeenCalledTimes(2)
    })
  })
})
