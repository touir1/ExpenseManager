import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import ExpenseForm from '../ExpenseForm'
import { formatAmountDisplay } from '@/features/expenses/utils/amountFormat'
import type { ExpenseDto } from '@/features/expenses/types/expenses.type'

// ── Mocks ────────────────────────────────────────────────────────────────────

const mockCategories = [
  { id: 1, name: 'Food', description: undefined, subcategories: [{ id: 11, name: 'Groceries' }, { id: 12, name: 'Restaurants' }] },
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

vi.mock('@/features/tags/components/TagInput', () => ({
  default: ({ onChange }: { onChange: (tags: Array<{id: number; name: string}>) => void }) => (
    <div data-testid="tag-input">
      <button type="button" onClick={() => onChange([{ id: 5, name: 'food' }])}>
        add-tag
      </button>
    </div>
  ),
}))

vi.mock('@/features/tags/services/tagsApi.service', () => ({
  getTags: vi.fn().mockResolvedValue({ ok: true, data: { own: [], family: [] } }),
  useTag: vi.fn(),
  removeTag: vi.fn(),
}))

const mockUploadExpenseReceipt = vi.fn()
vi.mock('@/features/expenses/services/expensesApi.service', () => ({
  uploadExpenseReceipt: (...args: unknown[]) => mockUploadExpenseReceipt(...args),
  getExpenseReceiptUrl: (id: number, download?: boolean) => `/api/expenses/${id}/receipt${download ? '?download=true' : ''}`,
}))

// ── Helpers ──────────────────────────────────────────────────────────────────

function renderForm(overrides: Partial<Parameters<typeof ExpenseForm>[0]> = {}) {
  const onSubmit = vi.fn().mockResolvedValue(undefined)
  const onCancel = vi.fn()
  render(
    <ExpenseForm
      isSubmitting={false}
      onSubmit={onSubmit}
      onCancel={onCancel}
      {...overrides}
    />
  )
  return { onSubmit, onCancel }
}

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('ExpenseForm', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUploadExpenseReceipt.mockResolvedValue({ ok: true })
    if (!URL.createObjectURL) URL.createObjectURL = vi.fn()
    if (!URL.revokeObjectURL) URL.revokeObjectURL = vi.fn()
    vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:mock-url')
    vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => {})
  })

  describe('rendering', () => {
    it('renders amount, currency, date fields', () => {
      renderForm()
      expect(screen.getByLabelText(/^amount$/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/^currency$/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/^date$/i)).toBeInTheDocument()
    })

    it('renders category select', () => {
      renderForm()
      expect(screen.getByLabelText(/^category$/i)).toBeInTheDocument()
    })

    it('does not render subcategory select until a category with subcategories is selected', () => {
      renderForm()
      expect(screen.queryByLabelText(/^subcategory$/i)).not.toBeInTheDocument()
    })

    it('renders description textarea', () => {
      renderForm()
      expect(screen.getByLabelText(/^description$/i)).toBeInTheDocument()
    })

    it('shows a character counter that updates as description is typed', () => {
      renderForm()
      expect(screen.getByText('0/500')).toBeInTheDocument()
      fireEvent.change(screen.getByLabelText(/^description$/i), { target: { value: 'Weekly shop' } })
      expect(screen.getByText('11/500')).toBeInTheDocument()
    })

    it('renders tag input', () => {
      renderForm()
      expect(screen.getByTestId('tag-input')).toBeInTheDocument()
    })

    it('renders family checkboxes', () => {
      renderForm()
      expect(screen.queryByText('My Family')).not.toBeInTheDocument()
      expect(screen.getByText('Smith')).toBeInTheDocument()
    })

    it('renders currency options from context', () => {
      renderForm()
      fireEvent.focus(screen.getByLabelText(/^currency$/i))
      expect(screen.getByRole('option', { name: 'EUR' })).toBeInTheDocument()
      expect(screen.getByRole('option', { name: 'USD' })).toBeInTheDocument()
    })
  })

  describe('subcategory cascade', () => {
    it('does not render subcategory select when no category selected', () => {
      renderForm()
      expect(screen.queryByLabelText(/^subcategory$/i)).not.toBeInTheDocument()
    })

    it('renders subcategory select when category with subcategories is selected', async () => {
      renderForm()
      fireEvent.focus(screen.getByLabelText(/^category$/i))
      fireEvent.mouseDown(screen.getByRole('option', { name: 'Food' }))
      await waitFor(() => {
        expect(screen.getByLabelText(/^subcategory$/i)).toBeInTheDocument()
      })
    })

    it('shows subcategory options for selected category', async () => {
      renderForm()
      fireEvent.focus(screen.getByLabelText(/^category$/i))
      fireEvent.mouseDown(screen.getByRole('option', { name: 'Food' }))
      await waitFor(() => screen.getByLabelText(/^subcategory$/i))
      fireEvent.focus(screen.getByLabelText(/^subcategory$/i))
      await waitFor(() => {
        expect(screen.getByRole('option', { name: 'Groceries' })).toBeInTheDocument()
        expect(screen.getByRole('option', { name: 'Restaurants' })).toBeInTheDocument()
      })
    })

    it('does not render subcategory select when selected category has no subcategories', async () => {
      renderForm()
      fireEvent.focus(screen.getByLabelText(/^category$/i))
      fireEvent.mouseDown(screen.getByRole('option', { name: 'Transport' }))
      await waitFor(() => {
        expect(screen.queryByLabelText(/^subcategory$/i)).not.toBeInTheDocument()
      })
    })
  })

  describe('family checkboxes', () => {
    it('default family is not shown as a checkbox', () => {
      renderForm()
      const labels = screen.getAllByRole('checkbox')
      const defaultCheckbox = labels.find(el => el.closest('label')?.textContent?.includes('My Family'))
      expect(defaultCheckbox).toBeUndefined()
    })

    it('non-default family checkbox is enabled', () => {
      renderForm()
      const labels = screen.getAllByRole('checkbox')
      const nonDefault = labels.find(el => el.closest('label')?.textContent?.includes('Smith'))
      expect(nonDefault).not.toBeDisabled()
    })

    it('custom checkbox indicator toggles on click without breaking form state', async () => {
      const user = userEvent.setup()
      renderForm()
      const checkbox = screen.getAllByRole('checkbox').find(el => el.closest('label')?.textContent?.includes('Smith'))!
      expect(checkbox).not.toBeChecked()
      await user.click(checkbox.closest('label')!)
      expect(checkbox).toBeChecked()
    })
  })

  describe('validation', () => {
    it('shows error when submitting with empty amount', async () => {
      const user = userEvent.setup()
      const { onSubmit } = renderForm()
      await user.click(screen.getByRole('button', { name: /^save$/i }))
      await waitFor(() => {
        expect(screen.getAllByRole('alert').length).toBeGreaterThan(0)
      })
      expect(onSubmit).not.toHaveBeenCalled()
    })
  })

  describe('edit mode', () => {
    const expense: ExpenseDto = {
      id: 1,
      amount: 42.5,
      currency: { id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
      date: '2026-05-19',
      category: { id: 1, name: 'Food' },
      subcategory: { id: 11, name: 'Groceries' },
      description: 'Weekly shop',
      createdAt: '2026-05-19T10:00:00Z',
      modifiedAt: '2026-05-19T11:00:00Z',
      modifiedFrom: 'Web',
      tags: [{ id: 5, name: 'food' }],
      convertedAmount: null,
      displayCurrency: null,
      hasReceipt: false,
    }

    it('pre-fills amount from initialValues, formatted', () => {
      renderForm({ initialValues: expense })
      expect(screen.getByLabelText(/^amount$/i)).toHaveValue(formatAmountDisplay(42.5))
    })

    it('pre-fills date from initialValues', () => {
      renderForm({ initialValues: expense })
      expect(screen.getByLabelText(/^date$/i)).toHaveValue('2026-05-19')
    })

    it('pre-fills description from initialValues', () => {
      renderForm({ initialValues: expense })
      expect(screen.getByLabelText(/^description$/i)).toHaveValue('Weekly shop')
    })

    it('shows modifiedAt line when modifiedAt present', () => {
      renderForm({ initialValues: expense })
      expect(screen.getByText(/last modified/i)).toBeInTheDocument()
    })
  })

  describe('amount formatting', () => {
    it('shows raw digits while focused, with no thousands separator', () => {
      renderForm()
      const amountInput = screen.getByLabelText(/^amount$/i)
      fireEvent.focus(amountInput)
      fireEvent.change(amountInput, { target: { value: '2430.5' } })
      expect(amountInput).toHaveValue('2430.5')
    })

    it('formats with locale-aware grouping on blur', () => {
      renderForm()
      const amountInput = screen.getByLabelText(/^amount$/i)
      fireEvent.focus(amountInput)
      fireEvent.change(amountInput, { target: { value: '2430.5' } })
      fireEvent.blur(amountInput)
      expect(amountInput).toHaveValue(formatAmountDisplay(2430.5))
    })

    it('reverts to raw value when re-focused after formatting', () => {
      renderForm()
      const amountInput = screen.getByLabelText(/^amount$/i)
      fireEvent.focus(amountInput)
      fireEvent.change(amountInput, { target: { value: '2430.5' } })
      fireEvent.blur(amountInput)
      fireEvent.focus(amountInput)
      expect(amountInput).toHaveValue('2430.5')
    })

    it('filters non-numeric characters while typing', () => {
      renderForm()
      const amountInput = screen.getByLabelText(/^amount$/i)
      fireEvent.focus(amountInput)
      fireEvent.change(amountInput, { target: { value: '12a3b.4c5' } })
      expect(amountInput).toHaveValue('123.45')
    })

    it('submits the raw numeric amount, unaffected by display formatting', async () => {
      const user = userEvent.setup()
      const { onSubmit } = renderForm()
      const amountInput = screen.getByLabelText(/^amount$/i)
      fireEvent.focus(amountInput)
      fireEvent.change(amountInput, { target: { value: '2430.5' } })
      fireEvent.blur(amountInput)

      fireEvent.focus(screen.getByLabelText(/^currency$/i))
      fireEvent.mouseDown(screen.getByRole('option', { name: 'EUR' }))
      fireEvent.focus(screen.getByLabelText(/^category$/i))
      fireEvent.mouseDown(screen.getByRole('option', { name: 'Transport' }))

      await user.click(screen.getByRole('button', { name: /^save$/i }))

      await waitFor(() => {
        expect(onSubmit).toHaveBeenCalledWith(expect.objectContaining({ amount: 2430.5 }))
      })
    })
  })

  describe('cancel', () => {
    it('calls onCancel when cancel button clicked', async () => {
      const user = userEvent.setup()
      const { onCancel } = renderForm()
      await user.click(screen.getByRole('button', { name: /^cancel$/i }))
      expect(onCancel).toHaveBeenCalledOnce()
    })
  })

  describe('receipt photo', () => {
    const receiptFile = new File(['dummy'], 'receipt.jpg', { type: 'image/jpeg' })

    function fillMinimalFields() {
      const amountInput = screen.getByLabelText(/^amount$/i)
      fireEvent.focus(amountInput)
      fireEvent.change(amountInput, { target: { value: '10' } })
      fireEvent.blur(amountInput)
      fireEvent.focus(screen.getByLabelText(/^currency$/i))
      fireEvent.mouseDown(screen.getByRole('option', { name: 'EUR' }))
      fireEvent.focus(screen.getByLabelText(/^category$/i))
      fireEvent.mouseDown(screen.getByRole('option', { name: 'Transport' }))
    }

    it('renders a file input accepting the supported image types', () => {
      renderForm()
      const input = screen.getByLabelText(/^receipt$/i)
      expect(input).toHaveAttribute('type', 'file')
      expect(input).toHaveAttribute('accept', 'image/jpeg,image/png,image/webp')
    })

    it('shows a local preview when a file is selected', () => {
      renderForm()
      const input = screen.getByLabelText(/^receipt$/i)
      fireEvent.change(input, { target: { files: [receiptFile] } })
      expect(URL.createObjectURL).toHaveBeenCalledWith(receiptFile)
      expect(screen.getByAltText(/receipt preview/i)).toHaveAttribute('src', 'blob:mock-url')
    })

    it('shows the existing receipt image when editing an expense that already has one', () => {
      const expense: ExpenseDto = {
        id: 7,
        amount: 12,
        currency: { id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
        date: '2026-05-19',
        category: null,
        subcategory: null,
        description: null,
        createdAt: '2026-05-19T10:00:00Z',
        modifiedAt: null,
        modifiedFrom: null,
        tags: [],
        convertedAmount: null,
        displayCurrency: null,
        hasReceipt: true,
      }
      renderForm({ initialValues: expense })
      expect(screen.getByAltText(/receipt preview/i)).toHaveAttribute('src', '/api/expenses/7/receipt')
    })

    it('uploads the receipt after the expense save succeeds', async () => {
      const user = userEvent.setup()
      const onSubmit = vi.fn().mockResolvedValue({ id: 99 })
      renderForm({ onSubmit })
      fillMinimalFields()
      fireEvent.change(screen.getByLabelText(/^receipt$/i), { target: { files: [receiptFile] } })

      await user.click(screen.getByRole('button', { name: /^save$/i }))

      await waitFor(() => {
        expect(mockUploadExpenseReceipt).toHaveBeenCalledWith(99, receiptFile)
      })
    })

    it('does not attempt an upload when no file was selected', async () => {
      const user = userEvent.setup()
      const onSubmit = vi.fn().mockResolvedValue({ id: 99 })
      renderForm({ onSubmit })
      fillMinimalFields()

      await user.click(screen.getByRole('button', { name: /^save$/i }))

      await waitFor(() => {
        expect(onSubmit).toHaveBeenCalledOnce()
      })
      expect(mockUploadExpenseReceipt).not.toHaveBeenCalled()
    })

    it('does not fail the save flow when the receipt upload fails', async () => {
      mockUploadExpenseReceipt.mockResolvedValue({ ok: false, error: 'upload failed' })
      const user = userEvent.setup()
      const onSubmit = vi.fn().mockResolvedValue({ id: 99 })
      renderForm({ onSubmit })
      fillMinimalFields()
      fireEvent.change(screen.getByLabelText(/^receipt$/i), { target: { files: [receiptFile] } })

      await user.click(screen.getByRole('button', { name: /^save$/i }))

      await waitFor(() => {
        expect(mockUploadExpenseReceipt).toHaveBeenCalledWith(99, receiptFile)
      })
      expect(onSubmit).toHaveBeenCalledOnce()
    })
  })
})
