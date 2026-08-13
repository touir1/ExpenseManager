import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import RecurringExpenseForm from '../RecurringExpenseForm'
import type { RecurringExpenseDto } from '@/features/dashboard/types/dashboard.type'

const mockCategories = [
  { id: 1, name: 'Food', description: undefined, subcategories: [{ id: 11, name: 'Groceries' }] },
  { id: 2, name: 'Transport', description: undefined, subcategories: [] },
]
const mockCurrencies = [
  { id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
  { id: 2, code: 'USD', name: 'US Dollar', symbol: '$', decimals: 2 },
]
const mockFamilies = [
  { id: 10, name: 'My Family', isDefault: true, isArchived: false, userRole: 'Head' as const, createdAt: '' },
  { id: 20, name: 'Smith', isDefault: false, isArchived: false, userRole: 'Member' as const, createdAt: '' },
]

vi.mock('@/features/expenses/ExpensesDataContext', () => ({
  useExpensesData: () => ({ categories: mockCategories, currencies: mockCurrencies, isLoading: false, refresh: vi.fn() }),
}))

vi.mock('@/features/families/FamilyContext', () => ({
  useFamilies: () => ({ families: mockFamilies, activeFamilyId: null, setActiveFamilyId: vi.fn(), isLoading: false, refresh: vi.fn() }),
}))

function renderForm(overrides: Partial<Parameters<typeof RecurringExpenseForm>[0]> = {}) {
  const onSubmit = vi.fn().mockResolvedValue(undefined)
  const onCancel = vi.fn()
  render(<RecurringExpenseForm isSubmitting={false} onSubmit={onSubmit} onCancel={onCancel} {...overrides} />)
  return { onSubmit, onCancel }
}

describe('RecurringExpenseForm', () => {
  beforeEach(() => vi.clearAllMocks())

  it('renders description, amount, currency, category, frequency, and next due date fields', () => {
    renderForm()
    expect(screen.getByLabelText(/description/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/amount/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/currency/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/category/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/frequency/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/next due date/i)).toBeInTheDocument()
  })

  it('renders the auto-create checkbox', () => {
    renderForm()
    expect(screen.getByText(/automatically create/i)).toBeInTheDocument()
  })

  it('does not render the active checkbox when creating', () => {
    renderForm()
    expect(screen.queryByText(/^active$/i)).not.toBeInTheDocument()
  })

  it('renders the active checkbox when editing', () => {
    const initialValues: RecurringExpenseDto = {
      id: 1,
      description: 'Netflix',
      amount: 15.99,
      currencyId: 1,
      currency: mockCurrencies[0],
      categoryId: 1,
      category: { id: 1, name: 'Food' },
      subcategoryId: null,
      subcategory: null,
      familyId: null,
      frequencyId: 2,
      nextDueDate: '2026-09-01',
      frequency: 'Monthly',
      isActive: true,
      autoCreate: false,
    }
    renderForm({ initialValues })
    expect(screen.getByText(/^active$/i)).toBeInTheDocument()
  })

  it('does not render subcategory select until a category with subcategories is selected', () => {
    renderForm()
    expect(screen.queryByLabelText(/subcategory/i)).not.toBeInTheDocument()
  })

  it('renders subcategory select when category with subcategories is selected', async () => {
    renderForm()
    fireEvent.focus(screen.getByLabelText(/category/i))
    fireEvent.mouseDown(screen.getByRole('option', { name: 'Food' }))
    await waitFor(() => {
      expect(screen.getByLabelText(/subcategory/i)).toBeInTheDocument()
    })
  })

  it('shows validation errors when submitting with empty required fields', async () => {
    const user = userEvent.setup()
    const { onSubmit } = renderForm()
    await user.click(screen.getByRole('button', { name: /save/i }))
    await waitFor(() => {
      expect(screen.getAllByRole('alert').length).toBeGreaterThan(0)
    })
    expect(onSubmit).not.toHaveBeenCalled()
  })

  it('pre-fills fields from initialValues in edit mode', () => {
    const initialValues: RecurringExpenseDto = {
      id: 1,
      description: 'Netflix',
      amount: 15.99,
      currencyId: 1,
      currency: mockCurrencies[0],
      categoryId: 1,
      category: { id: 1, name: 'Food' },
      subcategoryId: null,
      subcategory: null,
      familyId: null,
      frequencyId: 2,
      nextDueDate: '2026-09-01',
      frequency: 'Monthly',
      isActive: true,
      autoCreate: false,
    }
    renderForm({ initialValues })
    expect(screen.getByLabelText(/description/i)).toHaveValue('Netflix')
    expect(screen.getByLabelText(/next due date/i)).toHaveValue('2026-09-01')
  })

  it('submits valid data', async () => {
    const user = userEvent.setup()
    const { onSubmit } = renderForm()

    fireEvent.change(screen.getByLabelText(/description/i), { target: { value: 'Netflix' } })
    const amountInput = screen.getByLabelText(/amount/i)
    fireEvent.focus(amountInput)
    fireEvent.change(amountInput, { target: { value: '15.99' } })
    fireEvent.blur(amountInput)
    fireEvent.focus(screen.getByLabelText(/currency/i))
    fireEvent.mouseDown(screen.getByRole('option', { name: 'EUR' }))
    fireEvent.focus(screen.getByLabelText(/category/i))
    fireEvent.mouseDown(screen.getByRole('option', { name: 'Transport' }))
    fireEvent.change(screen.getByLabelText(/next due date/i), { target: { value: '2026-09-01' } })

    await user.click(screen.getByRole('button', { name: /save/i }))

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalledWith(expect.objectContaining({ description: 'Netflix', amount: 15.99 }))
    })
  })

  it('calls onCancel when cancel button clicked', async () => {
    const user = userEvent.setup()
    const { onCancel } = renderForm()
    await user.click(screen.getByRole('button', { name: /cancel/i }))
    expect(onCancel).toHaveBeenCalledOnce()
  })
})
