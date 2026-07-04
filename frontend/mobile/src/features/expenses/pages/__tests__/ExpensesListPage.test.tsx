import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'

vi.mock('@/features/expenses/services/expensesApi.service', () => ({
  getExpenses: vi.fn(),
  deleteExpense: vi.fn(),
  addExpense: vi.fn(),
  getExpenseReceiptBlob: vi.fn(),
  deleteExpenseReceipt: vi.fn(),
}))

// ionicons/icons exports all ~1200 icons — loading causes heap OOM in tests.
vi.mock('ionicons/icons', () => ({
  receiptOutline: 'receipt-outline',
  closeOutline: 'close-outline',
  downloadOutline: 'download-outline',
  trashOutline: 'trash-outline',
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

let mockIsOnline = true
vi.mock('@/hooks/useNetworkSync', () => ({
  useNetworkSync: () => ({ isOnline: mockIsOnline, lastSync: null }),
}))

vi.mock('@capacitor/haptics', () => ({
  Haptics: { impact: vi.fn() },
  ImpactStyle: { Heavy: 'HEAVY' },
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
  IonSearchbar: ({ value, onIonInput, placeholder }: any) => (
    <input
      placeholder={placeholder}
      value={value}
      onChange={e => onIonInput?.({ detail: { value: e.target.value } })}
    />
  ),
  IonToast: ({ isOpen, message, buttons }: any) => isOpen ? (
    <div role="alert">
      {message}
      {buttons?.map((b: any) => <button key={b.text} onClick={b.handler}>{b.text}</button>)}
    </div>
  ) : null,
  IonModal: ({ isOpen, children }: any) => isOpen ? <div role="dialog">{children}</div> : null,
  IonButton: ({ children, onClick, disabled, 'aria-label': ariaLabel }: any) => (
    <button onClick={onClick} disabled={disabled} aria-label={ariaLabel}>{children}</button>
  ),
  IonButtons: ({ children }: any) => <div>{children}</div>,
  IonIcon: () => null,
  IonSpinner: () => <span>Loading</span>,
}))

import { getExpenses, deleteExpense, addExpense, getExpenseReceiptBlob, deleteExpenseReceipt } from '@/features/expenses/services/expensesApi.service'
import ExpensesListPage from '@/features/expenses/pages/ExpensesListPage'

const mockGetExpenses = getExpenses as ReturnType<typeof vi.fn>
const mockDeleteExpense = deleteExpense as ReturnType<typeof vi.fn>
const mockAddExpense = addExpense as ReturnType<typeof vi.fn>
const mockGetExpenseReceiptBlob = getExpenseReceiptBlob as ReturnType<typeof vi.fn>
const mockDeleteExpenseReceipt = deleteExpenseReceipt as ReturnType<typeof vi.fn>

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
  hasReceipt: false,
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
    vi.useRealTimers()
    mockIsOnline = true
    mockGetExpenses.mockReset()
    mockDeleteExpense.mockReset()
    mockAddExpense.mockReset()
    mockGetExpenseReceiptBlob.mockReset()
    mockDeleteExpenseReceipt.mockReset()
    mockGetExpenses.mockResolvedValue({ ok: true, status: 200, data: pagedResponse })
    if (!URL.createObjectURL) (URL as any).createObjectURL = vi.fn()
    if (!URL.revokeObjectURL) (URL as any).revokeObjectURL = vi.fn()
    vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:mock-url')
    vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => {})
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
    await waitFor(() => screen.getAllByText('Food'))
    const refreshBtn = screen.getByText('Refresh')
    fireEvent.click(refreshBtn)
    await waitFor(() => {
      expect(mockGetExpenses).toHaveBeenCalledTimes(2)
    })
  })

  it('shows offline banner when offline', async () => {
    mockIsOnline = false
    render(<ExpensesListPage />, { wrapper: makeWrapper() })
    await waitFor(() => {
      expect(screen.getByText(/offline/i)).toBeTruthy()
    })
  })

  it('does not show offline banner when online', async () => {
    render(<ExpensesListPage />, { wrapper: makeWrapper() })
    await waitFor(() => screen.getAllByText('Food'))
    expect(screen.queryByText(/offline/i)).toBeNull()
  })

  it('shows undo toast after delete and re-adds expense via Undo', async () => {
    mockDeleteExpense.mockResolvedValue({ ok: true, status: 204 })
    mockAddExpense.mockResolvedValue({ ok: true, status: 201, data: {} })
    render(<ExpensesListPage />, { wrapper: makeWrapper() })
    await waitFor(() => screen.getAllByText('Food'))
    fireEvent.click(screen.getAllByText('Delete')[0])
    await waitFor(() => screen.getByRole('alertdialog'))
    const confirmBtn = screen.getAllByRole('button', { name: /delete/i }).find(
      b => b.closest('[role="alertdialog"]') !== null
    )
    if (confirmBtn) fireEvent.click(confirmBtn)
    await waitFor(() => {
      expect(screen.getByRole('alert')).toBeTruthy()
    })
    fireEvent.click(screen.getByText('Undo'))
    await waitFor(() => {
      expect(mockAddExpense).toHaveBeenCalled()
    })
  })

  it('fires haptic feedback on delete confirm', async () => {
    const { Haptics, ImpactStyle } = await import('@capacitor/haptics')
    mockDeleteExpense.mockResolvedValue({ ok: true, status: 204 })
    render(<ExpensesListPage />, { wrapper: makeWrapper() })
    await waitFor(() => screen.getAllByText('Food'))
    fireEvent.click(screen.getAllByText('Delete')[0])
    await waitFor(() => screen.getByRole('alertdialog'))
    const confirmBtn = screen.getAllByRole('button', { name: /delete/i }).find(
      b => b.closest('[role="alertdialog"]') !== null
    )
    if (confirmBtn) fireEvent.click(confirmBtn)
    await waitFor(() => {
      expect(Haptics.impact).toHaveBeenCalledWith({ style: ImpactStyle.Heavy })
    })
  })

  it('filters via searchbar with debounce', async () => {
    vi.useFakeTimers({ shouldAdvanceTime: true })
    render(<ExpensesListPage />, { wrapper: makeWrapper() })
    await vi.waitFor(() => expect(mockGetExpenses).toHaveBeenCalledTimes(1))

    const searchInput = screen.getByPlaceholderText(/search/i)
    fireEvent.change(searchInput, { target: { value: 'coffee' } })

    await vi.advanceTimersByTimeAsync(500)

    await vi.waitFor(() => {
      const lastCall = mockGetExpenses.mock.calls.at(-1)?.[0]
      expect(lastCall?.description).toBe('coffee')
    })
    vi.useRealTimers()
  })

  it('does not render a receipt icon when hasReceipt is false', async () => {
    render(<ExpensesListPage />, { wrapper: makeWrapper() })
    await waitFor(() => screen.getAllByText('Food'))
    expect(screen.queryByLabelText(/view receipt/i)).toBeNull()
  })

  it('renders a receipt icon when hasReceipt is true and opens the viewer on tap', async () => {
    mockGetExpenses.mockResolvedValue({
      ok: true,
      status: 200,
      data: { ...pagedResponse, items: [{ ...mockExpense, hasReceipt: true }] },
    })
    mockGetExpenseReceiptBlob.mockResolvedValue({ ok: true, status: 200, data: new Blob(['x'], { type: 'image/jpeg' }) })
    render(<ExpensesListPage />, { wrapper: makeWrapper() })
    await waitFor(() => screen.getAllByText('Food'))

    const receiptBtn = screen.getByLabelText(/view receipt/i)
    fireEvent.click(receiptBtn)

    await waitFor(() => {
      expect(mockGetExpenseReceiptBlob).toHaveBeenCalledWith(1)
    })
    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeTruthy()
    })
  })

  it('deletes the receipt and removes the icon after confirm', async () => {
    mockGetExpenses.mockResolvedValue({
      ok: true,
      status: 200,
      data: { ...pagedResponse, items: [{ ...mockExpense, hasReceipt: true }] },
    })
    mockGetExpenseReceiptBlob.mockResolvedValue({ ok: true, status: 200, data: new Blob(['x'], { type: 'image/jpeg' }) })
    mockDeleteExpenseReceipt.mockResolvedValue({ ok: true, status: 204 })
    render(<ExpensesListPage />, { wrapper: makeWrapper() })
    await waitFor(() => screen.getAllByText('Food'))

    fireEvent.click(screen.getByLabelText(/view receipt/i))
    await waitFor(() => expect(screen.getByRole('dialog')).toBeTruthy())

    fireEvent.click(screen.getByLabelText(/delete receipt/i))
    await waitFor(() => screen.getByRole('alertdialog'))
    const confirmBtn = screen.getAllByRole('button', { name: /delete/i }).find(
      b => b.closest('[role="alertdialog"]') !== null
    )
    if (confirmBtn) fireEvent.click(confirmBtn)

    await waitFor(() => {
      expect(mockDeleteExpenseReceipt).toHaveBeenCalledWith(1)
    })
    await waitFor(() => {
      expect(screen.queryByLabelText(/view receipt/i)).toBeNull()
    })
  })
})
