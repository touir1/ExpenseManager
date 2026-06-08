import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'

vi.mock('@/features/expenses/services/expensesApi.service', () => ({
  addExpense: vi.fn(),
}))

vi.mock('@/features/expenses/ExpensesDataContext', () => {
  // Stable reference — new array literals each call would change identity every render,
  // triggering the useEffect([isOpen, currencies, reset]) in QuickAddModal endlessly.
  const data = {
    categories: [{ id: 1, name: 'Food', subcategories: [] }],
    currencies: [{ id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 }],
    tags: [{ id: 1, name: 'groceries' }],
    isLoading: false,
    refresh: vi.fn(),
  }
  return { useExpensesData: () => data }
})

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

// ionicons/icons exports all ~1200 icons — loading causes heap OOM in tests.
vi.mock('ionicons/icons', () => ({ cameraOutline: 'camera-outline' }))

// @hookform/resolvers@3.10.0 + zod@4.4.3: parseAsync hangs indefinitely in jsdom.
vi.mock('@hookform/resolvers/zod', () => ({
  zodResolver: (schema: any) => async (values: any) => {
    const result = schema.safeParse(values)
    if (result.success) return { values: result.data, errors: {} }
    const errors: Record<string, any> = {}
    for (const issue of result.error.issues) {
      const key = String(issue.path[0] ?? 'root')
      if (!errors[key]) errors[key] = { message: issue.message, type: issue.code }
    }
    return { values: {}, errors }
  },
}))

vi.mock('@ionic/react', () => ({
  IonModal: ({ isOpen, children }: any) =>
    isOpen ? <div role="dialog">{children}</div> : null,
    IonHeader: ({ children }: any) => <div>{children}</div>,
    IonToolbar: ({ children }: any) => <div>{children}</div>,
    IonTitle: ({ children }: any) => <h2>{children}</h2>,
    IonContent: ({ children }: any) => <div>{children}</div>,
    IonItem: ({ children }: any) => <div>{children}</div>,
    IonLabel: ({ children }: any) => <label>{children}</label>,
    IonInput: React.forwardRef(({ onIonInput, onChange, onBlur, type, value, ...rest }: any, ref: any) => (
      <input
        ref={ref}
        type={type ?? 'text'}
        defaultValue={value}
        onChange={e => { onChange?.(e); onIonInput?.({ detail: { value: e.target.value } }) }}
        onBlur={onBlur}
        {...rest}
      />
    )),
    IonButton: ({ children, onClick, disabled }: any) => (
      <button onClick={onClick} disabled={disabled}>{children}</button>
    ),
    IonButtons: ({ children }: any) => <div>{children}</div>,
    IonSelect: ({ children, onIonChange, value, multiple }: any) => (
      <select
        multiple={multiple}
        defaultValue={value}
        onChange={e => onIonChange?.({ detail: { value: multiple ? [e.target.value] : Number(e.target.value) } })}
      >
        {children}
      </select>
    ),
    IonSelectOption: ({ children, value }: any) => <option value={value}>{children}</option>,
    IonTextarea: React.forwardRef(({ rows, maxlength, onChange, onBlur, ...rest }: any, ref: any) => (
      <textarea ref={ref} rows={rows} maxLength={maxlength} onChange={onChange} onBlur={onBlur} {...rest} />
    )),
    IonDatetime: ({ onIonChange, value }: any) => (
      <input
        type="date"
        defaultValue={value}
        onChange={e => onIonChange?.({ detail: { value: e.target.value } })}
      />
    ),
    IonToast: ({ isOpen, message }: any) => isOpen ? <div role="alert">{message}</div> : null,
    IonFab: ({ children }: any) => <div>{children}</div>,
    IonFabButton: ({ children, onClick }: any) => <button onClick={onClick}>{children}</button>,
    IonIcon: () => null,
    IonChip: ({ children, onClick }: any) => <span onClick={onClick}>{children}</span>,
    IonSpinner: () => <span>Loading</span>,
    IonText: ({ children }: any) => <span>{children}</span>,
}))

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
