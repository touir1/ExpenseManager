import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import ExpenseFilters from '../ExpenseFilters'
import type { ExpenseFilter } from '@/features/expenses/types/expenses.type'

// ── Mocks ────────────────────────────────────────────────────────────────────

const mockCategories = [
  { id: 1, name: 'Food', description: undefined, subcategories: [{ id: 11, name: 'Groceries' }] },
  { id: 2, name: 'Transport', description: undefined, subcategories: [] },
]
const mockCurrencies = [
  { id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
]

vi.mock('@/features/expenses/ExpensesDataContext', () => ({
  useExpensesData: () => ({ categories: mockCategories, currencies: mockCurrencies, tags: [], isLoading: false, refresh: vi.fn() }),
}))

// ── Helpers ──────────────────────────────────────────────────────────────────

function renderFilters(filter: ExpenseFilter = {}, onApply = vi.fn()) {
  render(<ExpenseFilters filter={filter} onApply={onApply} />)
  return { onApply }
}

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('ExpenseFilters', () => {
  beforeEach(() => vi.clearAllMocks())

  describe('toggle', () => {
    it('filter panel hidden by default', () => {
      renderFilters()
      expect(screen.queryByRole('region')).not.toBeInTheDocument()
    })

    it('opens filter panel on toggle button click', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /filter/i }))
      expect(screen.getByRole('region')).toBeInTheDocument()
    })

    it('closes filter panel on second toggle click', async () => {
      const user = userEvent.setup()
      renderFilters()
      const btn = screen.getByRole('button', { name: /filter/i })
      await user.click(btn)
      await user.click(btn)
      expect(screen.queryByRole('region')).not.toBeInTheDocument()
    })

    it('toggle button has aria-expanded=false when closed', () => {
      renderFilters()
      expect(screen.getByRole('button', { name: /filter/i })).toHaveAttribute('aria-expanded', 'false')
    })

    it('toggle button has aria-expanded=true when open', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /filter/i }))
      expect(screen.getByRole('button', { name: /filter/i })).toHaveAttribute('aria-expanded', 'true')
    })
  })

  describe('apply', () => {
    it('calls onApply with filter values on apply click', async () => {
      const user = userEvent.setup()
      const { onApply } = renderFilters()
      await user.click(screen.getByRole('button', { name: /filter/i }))
      await user.type(screen.getByLabelText(/from/i), '2026-01-01')
      await user.click(screen.getByRole('button', { name: /apply/i }))
      expect(onApply).toHaveBeenCalledOnce()
      expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ dateFrom: '2026-01-01', page: 1 }))
    })

    it('resets page to 1 on apply', async () => {
      const user = userEvent.setup()
      const { onApply } = renderFilters({ page: 5 })
      await user.click(screen.getByRole('button', { name: /filter/i }))
      await user.click(screen.getByRole('button', { name: /apply/i }))
      expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ page: 1 }))
    })

    it('closes panel after apply', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /filter/i }))
      await user.click(screen.getByRole('button', { name: /apply/i }))
      expect(screen.queryByRole('region')).not.toBeInTheDocument()
    })
  })

  describe('reset', () => {
    it('calls onApply with empty filter on reset', async () => {
      const user = userEvent.setup()
      const { onApply } = renderFilters({ dateFrom: '2026-01-01' })
      await user.click(screen.getByRole('button', { name: /filter/i }))
      await user.click(screen.getByRole('button', { name: /reset/i }))
      expect(onApply).toHaveBeenCalledWith({})
    })

    it('closes panel after reset', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /filter/i }))
      await user.click(screen.getByRole('button', { name: /reset/i }))
      expect(screen.queryByRole('region')).not.toBeInTheDocument()
    })
  })

  describe('subcategory visibility', () => {
    it('does not show subcategory select when no category selected', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /filter/i }))
      expect(screen.queryByLabelText(/subcategory/i)).not.toBeInTheDocument()
    })

    it('shows subcategory select when category with subcategories selected', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /filter/i }))
      await user.click(screen.getByLabelText(/^category$/i))
      await user.click(screen.getByRole('option', { name: 'Food' }))
      expect(screen.getByLabelText(/subcategory/i)).toBeInTheDocument()
      await user.click(screen.getByLabelText(/subcategory/i))
      expect(screen.getByRole('option', { name: 'Groceries' })).toBeInTheDocument()
    })

    it('hides subcategory when category has no subcategories', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /filter/i }))
      await user.click(screen.getByLabelText(/^category$/i))
      await user.click(screen.getByRole('option', { name: 'Transport' }))
      expect(screen.queryByLabelText(/subcategory/i)).not.toBeInTheDocument()
    })
  })

  describe('pre-filled filter', () => {
    it('pre-fills dateFrom input from filter prop', async () => {
      const user = userEvent.setup()
      renderFilters({ dateFrom: '2026-03-01' })
      await user.click(screen.getByRole('button', { name: /filter/i }))
      expect(screen.getByLabelText(/from/i)).toHaveValue('2026-03-01')
    })

    it('pre-fills dateTo input from filter prop', async () => {
      const user = userEvent.setup()
      renderFilters({ dateTo: '2026-03-31' })
      await user.click(screen.getByRole('button', { name: /filter/i }))
      expect(screen.getByLabelText(/^to$/i)).toHaveValue('2026-03-31')
    })

    it('pre-filled categoryId shows category name in combobox', async () => {
      const user = userEvent.setup()
      renderFilters({ categoryId: 1 })
      await user.click(screen.getByRole('button', { name: /filter/i }))
      expect(screen.getByLabelText(/^category$/i)).toHaveValue('Food')
    })

    it('pre-filled currencyId shows currency code in combobox', async () => {
      const user = userEvent.setup()
      renderFilters({ currencyId: 1 })
      await user.click(screen.getByRole('button', { name: /filter/i }))
      expect(screen.getByLabelText(/currency/i)).toHaveValue('EUR')
    })

    it('renders currency options', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /filter/i }))
      await user.click(screen.getByLabelText(/currency/i))
      expect(screen.getByRole('option', { name: 'EUR' })).toBeInTheDocument()
    })
  })

  describe('combobox search', () => {
    it('filters category options case-insensitively as user types', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /filter/i }))
      await user.click(screen.getByLabelText(/^category$/i))
      await user.type(screen.getByLabelText(/^category$/i), 'foo')
      expect(screen.getByRole('option', { name: 'Food' })).toBeInTheDocument()
      expect(screen.queryByRole('option', { name: 'Transport' })).not.toBeInTheDocument()
    })

    it('filters case-insensitively on uppercase input', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /filter/i }))
      await user.click(screen.getByLabelText(/^category$/i))
      await user.type(screen.getByLabelText(/^category$/i), 'TRAN')
      expect(screen.getByRole('option', { name: 'Transport' })).toBeInTheDocument()
      expect(screen.queryByRole('option', { name: 'Food' })).not.toBeInTheDocument()
    })

    it('shows only dash option when no options match query', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /filter/i }))
      await user.click(screen.getByLabelText(/^category$/i))
      await user.type(screen.getByLabelText(/^category$/i), 'zzz')
      expect(screen.queryByRole('option', { name: 'Food' })).not.toBeInTheDocument()
      expect(screen.queryByRole('option', { name: 'Transport' })).not.toBeInTheDocument()
    })

    it('closes dropdown after selecting an option', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /filter/i }))
      await user.click(screen.getByLabelText(/^category$/i))
      expect(screen.getByRole('listbox')).toBeInTheDocument()
      await user.click(screen.getByRole('option', { name: 'Food' }))
      expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
    })

    it('shows selected name in input after selection', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /filter/i }))
      await user.click(screen.getByLabelText(/^category$/i))
      await user.click(screen.getByRole('option', { name: 'Food' }))
      expect(screen.getByLabelText(/^category$/i)).toHaveValue('Food')
    })
  })

  describe('combobox selection → applied filter', () => {
    it('includes categoryId in applied filter', async () => {
      const user = userEvent.setup()
      const { onApply } = renderFilters()
      await user.click(screen.getByRole('button', { name: /filter/i }))
      await user.click(screen.getByLabelText(/^category$/i))
      await user.click(screen.getByRole('option', { name: 'Food' }))
      await user.click(screen.getByRole('button', { name: /apply/i }))
      expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ categoryId: 1 }))
    })

    it('includes currencyId in applied filter', async () => {
      const user = userEvent.setup()
      const { onApply } = renderFilters()
      await user.click(screen.getByRole('button', { name: /filter/i }))
      await user.click(screen.getByLabelText(/currency/i))
      await user.click(screen.getByRole('option', { name: 'EUR' }))
      await user.click(screen.getByRole('button', { name: /apply/i }))
      expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ currencyId: 1 }))
    })

    it('includes subcategoryId in applied filter', async () => {
      const user = userEvent.setup()
      const { onApply } = renderFilters()
      await user.click(screen.getByRole('button', { name: /filter/i }))
      await user.click(screen.getByLabelText(/^category$/i))
      await user.click(screen.getByRole('option', { name: 'Food' }))
      await user.click(screen.getByLabelText(/subcategory/i))
      await user.click(screen.getByRole('option', { name: 'Groceries' }))
      await user.click(screen.getByRole('button', { name: /apply/i }))
      expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ categoryId: 1, subcategoryId: 11 }))
    })

    it('clears subcategoryId when category changes', async () => {
      const user = userEvent.setup()
      const { onApply } = renderFilters()
      await user.click(screen.getByRole('button', { name: /filter/i }))
      await user.click(screen.getByLabelText(/^category$/i))
      await user.click(screen.getByRole('option', { name: 'Food' }))
      await user.click(screen.getByLabelText(/subcategory/i))
      await user.click(screen.getByRole('option', { name: 'Groceries' }))
      await user.click(screen.getByLabelText(/^category$/i))
      await user.click(screen.getByRole('option', { name: 'Transport' }))
      await user.click(screen.getByRole('button', { name: /apply/i }))
      expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ categoryId: 2 }))
      expect(onApply).toHaveBeenCalledWith(expect.not.objectContaining({ subcategoryId: expect.anything() }))
    })

    it('clears categoryId when dash selected', async () => {
      const user = userEvent.setup()
      const { onApply } = renderFilters({ categoryId: 1 })
      await user.click(screen.getByRole('button', { name: /filter/i }))
      await user.click(screen.getByLabelText(/^category$/i))
      await user.click(screen.getAllByRole('option', { name: '—' })[0])
      await user.click(screen.getByRole('button', { name: /apply/i }))
      expect(onApply).toHaveBeenCalledWith(expect.not.objectContaining({ categoryId: expect.anything() }))
    })
  })
})
