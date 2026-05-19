import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import ExpenseForm from '../ExpenseForm'
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
  beforeEach(() => vi.clearAllMocks())

  describe('rendering', () => {
    it('renders amount, currency, date fields', () => {
      renderForm()
      expect(screen.getByLabelText(/^amount$/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/^currency$/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/^date$/i)).toBeInTheDocument()
    })

    it('renders category and subcategory selects', () => {
      renderForm()
      expect(screen.getByLabelText(/^category$/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/^subcategory$/i)).toBeInTheDocument()
    })

    it('renders description textarea', () => {
      renderForm()
      expect(screen.getByLabelText(/^description$/i)).toBeInTheDocument()
    })

    it('renders tag input', () => {
      renderForm()
      expect(screen.getByTestId('tag-input')).toBeInTheDocument()
    })

    it('renders family checkboxes', () => {
      renderForm()
      expect(screen.getByText('My Family')).toBeInTheDocument()
      expect(screen.getByText('Smith')).toBeInTheDocument()
    })

    it('renders currency options from context', () => {
      renderForm()
      expect(screen.getByRole('option', { name: 'EUR' })).toBeInTheDocument()
      expect(screen.getByRole('option', { name: 'USD' })).toBeInTheDocument()
    })
  })

  describe('subcategory cascade', () => {
    it('disables subcategory select when no category selected', () => {
      renderForm()
      expect(screen.getByLabelText(/^subcategory$/i)).toBeDisabled()
    })

    it('enables subcategory select when category with subcategories is selected', async () => {
      const user = userEvent.setup()
      renderForm()
      await user.selectOptions(screen.getByLabelText(/^category$/i), '1')
      expect(screen.getByLabelText(/^subcategory$/i)).not.toBeDisabled()
    })

    it('shows subcategory options for selected category', async () => {
      const user = userEvent.setup()
      renderForm()
      await user.selectOptions(screen.getByLabelText(/^category$/i), '1')
      expect(screen.getByRole('option', { name: 'Groceries' })).toBeInTheDocument()
      expect(screen.getByRole('option', { name: 'Restaurants' })).toBeInTheDocument()
    })

    it('keeps subcategory disabled when selected category has no subcategories', async () => {
      const user = userEvent.setup()
      renderForm()
      await user.selectOptions(screen.getByLabelText(/^category$/i), '2')
      expect(screen.getByLabelText(/^subcategory$/i)).toBeDisabled()
    })
  })

  describe('family checkboxes', () => {
    it('default family checkbox is disabled (pre-checked)', () => {
      renderForm()
      const labels = screen.getAllByRole('checkbox')
      const defaultCheckbox = labels.find(el => el.closest('label')?.textContent?.includes('My Family'))
      expect(defaultCheckbox).toBeDisabled()
    })

    it('non-default family checkbox is enabled', () => {
      renderForm()
      const labels = screen.getAllByRole('checkbox')
      const nonDefault = labels.find(el => el.closest('label')?.textContent?.includes('Smith'))
      expect(nonDefault).not.toBeDisabled()
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
    }

    it('pre-fills amount from initialValues', () => {
      renderForm({ initialValues: expense })
      expect(screen.getByLabelText(/^amount$/i)).toHaveValue(42.5)
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

  describe('cancel', () => {
    it('calls onCancel when cancel button clicked', async () => {
      const user = userEvent.setup()
      const { onCancel } = renderForm()
      await user.click(screen.getByRole('button', { name: /^cancel$/i }))
      expect(onCancel).toHaveBeenCalledOnce()
    })
  })
})
