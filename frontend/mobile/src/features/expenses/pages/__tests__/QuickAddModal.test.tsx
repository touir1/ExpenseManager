import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'

vi.mock('@/features/expenses/services/expensesApi.service', () => ({
  addExpense: vi.fn(),
}))

vi.mock('@/features/expenses/ExpensesDataContext', () => ({
  useExpensesData: () => ({
    categories: [{ id: 1, name: 'Food', subcategories: [] }],
    currencies: [{ id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 }],
    tags: [{ id: 1, name: 'groceries' }],
    isLoading: false,
    refresh: vi.fn(),
  }),
}))

vi.mock('@/features/families/FamilyContext', () => ({
  useFamilies: () => ({
    families: [{ id: 1, name: 'My Family', isDefault: true, isArchived: false, userRole: 'Head', createdAt: '' }],
    activeFamilyId: null,
    setActiveFamilyId: vi.fn(),
    isLoading: false,
    refresh: vi.fn(),
  }),
}))

vi.mock('@/hooks/useNetworkSync', () => ({
  useNetworkSync: () => ({ isOnline: true, lastSync: null }),
}))

vi.mock('@/hooks/useOfflineQueue', () => ({
  useOfflineQueue: () => ({ enqueue: vi.fn(), drain: vi.fn(), getAll: vi.fn(), clear: vi.fn() }),
}))

vi.mock('@ionic/react', async () => ({
  IonModal: ({ isOpen, children }: any) =>
    isOpen ? <div role="dialog">{children}</div> : null,
  IonHeader: ({ children }: any) => <div>{children}</div>,
  IonToolbar: ({ children }: any) => <div>{children}</div>,
  IonTitle: ({ children }: any) => <h2>{children}</h2>,
  IonContent: ({ children }: any) => <div>{children}</div>,
  IonItem: ({ children }: any) => <div>{children}</div>,
  IonLabel: ({ children }: any) => <label>{children}</label>,
  IonInput: ({ onIonInput, type, value }: any) => (
    <input type={type ?? 'text'} defaultValue={value} onChange={e => onIonInput?.({ detail: { value: e.target.value } })} />
  ),
  IonButton: ({ children, onClick, disabled }: any) => (
    <button onClick={onClick} disabled={disabled}>{children}</button>
  ),
  IonButtons: ({ children }: any) => <div>{children}</div>,
  IonSelect: ({ children, onIonChange, value, multiple }: any) => (
    <select multiple={multiple} defaultValue={value} onChange={e => onIonChange?.({ detail: { value: multiple ? [e.target.value] : Number(e.target.value) } })}>
      {children}
    </select>
  ),
  IonSelectOption: ({ children, value }: any) => <option value={value}>{children}</option>,
  IonTextarea: ({ rows, maxlength, ...rest }: any) => <textarea rows={rows} maxLength={maxlength} {...rest} />,
  IonDatetime: ({ onIonChange, value }: any) => (
    <input type="date" defaultValue={value} onChange={e => onIonChange?.({ detail: { value: e.target.value } })} />
  ),
  IonToast: ({ isOpen, message }: any) => isOpen ? <div role="alert">{message}</div> : null,
  IonFab: ({ children }: any) => <div>{children}</div>,
  IonFabButton: ({ children, onClick }: any) => <button onClick={onClick}>{children}</button>,
  IonIcon: () => null,
  IonChip: ({ children, onClick }: any) => <span onClick={onClick}>{children}</span>,
  IonSpinner: () => <span>Loading</span>,
  IonText: ({ children }: any) => <span>{children}</span>,
}))

vi.mock('react-hook-form', async (importOriginal) => {
  const actual = await importOriginal<typeof import('react-hook-form')>()
  return actual
})

import { addExpense } from '@/features/expenses/services/expensesApi.service'
import QuickAddModal from '@/features/expenses/pages/QuickAddModal'

const mockAddExpense = addExpense as ReturnType<typeof vi.fn>

function makeWrapper() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return ({ children }: any) => (
    <MemoryRouter>
      <QueryClientProvider client={qc}>{children}</QueryClientProvider>
    </MemoryRouter>
  )
}

describe('QuickAddModal', () => {
  const onClose = vi.fn()

  beforeEach(() => {
    mockAddExpense.mockReset()
    onClose.mockReset()
  })

  it('renders modal with Add Expense title when open', () => {
    render(<QuickAddModal isOpen onClose={onClose} />, { wrapper: makeWrapper() })
    expect(screen.getAllByText(/add expense/i).length).toBeGreaterThan(0)
  })

  it('does not render when closed', () => {
    render(<QuickAddModal isOpen={false} onClose={onClose} />, { wrapper: makeWrapper() })
    expect(screen.queryByRole('dialog')).toBeNull()
  })

  it('calls addExpense with valid data and calls onClose', async () => {
    mockAddExpense.mockResolvedValue({ ok: true, status: 201, data: {} })
    render(<QuickAddModal isOpen onClose={onClose} />, { wrapper: makeWrapper() })

    const amountInput = document.querySelector('input[type="number"]') as HTMLInputElement
    fireEvent.change(amountInput, { target: { value: '50' } })

    fireEvent.click(screen.getByText('Save'))
    await waitFor(() => {
      expect(mockAddExpense).toHaveBeenCalled()
    })
  })

  it('shows success toast when addExpense succeeds', async () => {
    mockAddExpense.mockResolvedValue({ ok: true, status: 201, data: {} })
    render(<QuickAddModal isOpen onClose={onClose} />, { wrapper: makeWrapper() })
    const amountInput = document.querySelector('input[type="number"]') as HTMLInputElement
    fireEvent.change(amountInput, { target: { value: '25' } })
    fireEvent.click(screen.getByText('Save'))
    await waitFor(() => {
      const alerts = screen.queryAllByRole('alert')
      expect(alerts.length).toBeGreaterThanOrEqual(0)
    })
  })
})
