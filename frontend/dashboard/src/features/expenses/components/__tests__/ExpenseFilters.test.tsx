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
const mockTags = [
  { id: 1, name: 'groceries' },
  { id: 2, name: 'work' },
  { id: 3, name: 'family' },
]

const mockUseExpensesData = vi.fn()

vi.mock('@/features/expenses/ExpensesDataContext', () => ({
  useExpensesData: () => mockUseExpensesData(),
}))

// ── Helpers ──────────────────────────────────────────────────────────────────

function renderFilters(filter: ExpenseFilter = {}, onApply = vi.fn()) {
  render(<ExpenseFilters filter={filter} onApply={onApply} />)
  return { onApply }
}

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('ExpenseFilters', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUseExpensesData.mockReturnValue({
      categories: mockCategories,
      currencies: mockCurrencies,
      tags: [],
      isLoading: false,
      refresh: vi.fn(),
    })
  })

  describe('toggle', () => {
    it('filter panel hidden by default', () => {
      renderFilters()
      expect(screen.queryByRole('region')).not.toBeInTheDocument()
    })

    it('opens filter panel on toggle button click', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      expect(screen.getByRole('region')).toBeInTheDocument()
    })

    it('closes filter panel on second toggle click', async () => {
      const user = userEvent.setup()
      renderFilters()
      const btn = screen.getByRole('button', { name: /^filters$/i })
      await user.click(btn)
      await user.click(btn)
      expect(screen.queryByRole('region')).not.toBeInTheDocument()
    })

    it('toggle button has aria-expanded=false when closed', () => {
      renderFilters()
      expect(screen.getByRole('button', { name: /^filters$/i })).toHaveAttribute('aria-expanded', 'false')
    })

    it('toggle button has aria-expanded=true when open', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      expect(screen.getByRole('button', { name: /^filters$/i })).toHaveAttribute('aria-expanded', 'true')
    })

  })

  describe('apply', () => {
    it('calls onApply with filter values on apply click', async () => {
      const user = userEvent.setup()
      const { onApply } = renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.type(screen.getByLabelText(/from/i), '2026-01-01')
      await user.click(screen.getByRole('button', { name: /apply/i }))
      expect(onApply).toHaveBeenCalledOnce()
      expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ dateFrom: '2026-01-01', page: 1 }))
    })

    it('resets page to 1 on apply', async () => {
      const user = userEvent.setup()
      const { onApply } = renderFilters({ page: 5 })
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.click(screen.getByRole('button', { name: /apply/i }))
      expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ page: 1 }))
    })

    it('closes panel after apply', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.click(screen.getByRole('button', { name: /apply/i }))
      expect(screen.queryByRole('region')).not.toBeInTheDocument()
    })
  })

  describe('reset', () => {
    it('calls onApply with empty filter on reset', async () => {
      const user = userEvent.setup()
      const { onApply } = renderFilters({ dateFrom: '2026-01-01' })
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.click(screen.getByRole('button', { name: /reset/i }))
      expect(onApply).toHaveBeenCalledWith({})
    })

    it('closes panel after reset', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.click(screen.getByRole('button', { name: /reset/i }))
      expect(screen.queryByRole('region')).not.toBeInTheDocument()
    })
  })

  describe('subcategory visibility', () => {
    it('does not show subcategory select when no category selected', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      expect(screen.queryByLabelText(/subcategory/i)).not.toBeInTheDocument()
    })

    it('shows subcategory select when category with subcategories selected', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.click(screen.getByLabelText(/^category$/i))
      await user.click(screen.getByRole('option', { name: 'Food' }))
      expect(screen.getByLabelText(/subcategory/i)).toBeInTheDocument()
      await user.click(screen.getByLabelText(/subcategory/i))
      expect(screen.getByRole('option', { name: 'Groceries' })).toBeInTheDocument()
    })

    it('hides subcategory when category has no subcategories', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.click(screen.getByLabelText(/^category$/i))
      await user.click(screen.getByRole('option', { name: 'Transport' }))
      expect(screen.queryByLabelText(/subcategory/i)).not.toBeInTheDocument()
    })
  })

  describe('pre-filled filter', () => {
    it('pre-fills dateFrom input from filter prop', async () => {
      const user = userEvent.setup()
      renderFilters({ dateFrom: '2026-03-01' })
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      expect(screen.getByLabelText(/from/i)).toHaveValue('2026-03-01')
    })

    it('pre-fills dateTo input from filter prop', async () => {
      const user = userEvent.setup()
      renderFilters({ dateTo: '2026-03-31' })
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      expect(screen.getByLabelText(/^to$/i)).toHaveValue('2026-03-31')
    })

    it('pre-filled categoryId shows category name in combobox', async () => {
      const user = userEvent.setup()
      renderFilters({ categoryId: 1 })
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      expect(screen.getByLabelText(/^category$/i)).toHaveValue('Food')
    })

    it('pre-filled currencyId shows currency code in combobox', async () => {
      const user = userEvent.setup()
      renderFilters({ currencyId: 1 })
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      expect(screen.getByLabelText(/currency/i)).toHaveValue('EUR')
    })

    it('renders currency options', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.click(screen.getByLabelText(/currency/i))
      expect(screen.getByRole('option', { name: 'EUR' })).toBeInTheDocument()
    })
  })

  describe('combobox search', () => {
    it('filters category options case-insensitively as user types', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.click(screen.getByLabelText(/^category$/i))
      await user.type(screen.getByLabelText(/^category$/i), 'foo')
      expect(screen.getByRole('option', { name: 'Food' })).toBeInTheDocument()
      expect(screen.queryByRole('option', { name: 'Transport' })).not.toBeInTheDocument()
    })

    it('filters case-insensitively on uppercase input', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.click(screen.getByLabelText(/^category$/i))
      await user.type(screen.getByLabelText(/^category$/i), 'TRAN')
      expect(screen.getByRole('option', { name: 'Transport' })).toBeInTheDocument()
      expect(screen.queryByRole('option', { name: 'Food' })).not.toBeInTheDocument()
    })

    it('shows only dash option when no options match query', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.click(screen.getByLabelText(/^category$/i))
      await user.type(screen.getByLabelText(/^category$/i), 'zzz')
      expect(screen.queryByRole('option', { name: 'Food' })).not.toBeInTheDocument()
      expect(screen.queryByRole('option', { name: 'Transport' })).not.toBeInTheDocument()
    })

    it('closes dropdown after selecting an option', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.click(screen.getByLabelText(/^category$/i))
      expect(screen.getByRole('listbox')).toBeInTheDocument()
      await user.click(screen.getByRole('option', { name: 'Food' }))
      expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
    })

    it('shows selected name in input after selection', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.click(screen.getByLabelText(/^category$/i))
      await user.click(screen.getByRole('option', { name: 'Food' }))
      expect(screen.getByLabelText(/^category$/i)).toHaveValue('Food')
    })
  })

  describe('combobox selection → applied filter', () => {
    it('includes categoryId in applied filter', async () => {
      const user = userEvent.setup()
      const { onApply } = renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.click(screen.getByLabelText(/^category$/i))
      await user.click(screen.getByRole('option', { name: 'Food' }))
      await user.click(screen.getByRole('button', { name: /apply/i }))
      expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ categoryId: 1 }))
    })

    it('includes currencyId in applied filter', async () => {
      const user = userEvent.setup()
      const { onApply } = renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.click(screen.getByLabelText(/currency/i))
      await user.click(screen.getByRole('option', { name: 'EUR' }))
      await user.click(screen.getByRole('button', { name: /apply/i }))
      expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ currencyId: 1 }))
    })

    it('includes subcategoryId in applied filter', async () => {
      const user = userEvent.setup()
      const { onApply } = renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
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
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
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
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.click(screen.getByLabelText(/^category$/i))
      await user.click(screen.getAllByRole('option', { name: '—' })[0])
      await user.click(screen.getByRole('button', { name: /apply/i }))
      expect(onApply).toHaveBeenCalledWith(expect.not.objectContaining({ categoryId: expect.anything() }))
    })
  })

  describe('tags filter', () => {
    it('does not show tags filter when no tags exist', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      expect(screen.queryByLabelText(/^tags$/i)).not.toBeInTheDocument()
    })

    it('shows tags filter when tags exist', async () => {
      mockUseExpensesData.mockReturnValue({
        categories: mockCategories,
        currencies: mockCurrencies,
        tags: mockTags,
        isLoading: false,
        refresh: vi.fn(),
      })
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      expect(screen.getByLabelText(/^tags$/i)).toBeInTheDocument()
    })

    it('shows tag options in listbox', async () => {
      mockUseExpensesData.mockReturnValue({
        categories: mockCategories,
        currencies: mockCurrencies,
        tags: mockTags,
        isLoading: false,
        refresh: vi.fn(),
      })
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.click(screen.getByLabelText(/^tags$/i))
      expect(screen.getByRole('option', { name: 'groceries' })).toBeInTheDocument()
      expect(screen.getByRole('option', { name: 'work' })).toBeInTheDocument()
    })

    it('includes tagIds in applied filter when tags selected', async () => {
      mockUseExpensesData.mockReturnValue({
        categories: mockCategories,
        currencies: mockCurrencies,
        tags: mockTags,
        isLoading: false,
        refresh: vi.fn(),
      })
      const user = userEvent.setup()
      const { onApply } = renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.click(screen.getByLabelText(/^tags$/i))
      await user.click(screen.getByRole('option', { name: 'groceries' }))
      await user.click(screen.getByRole('option', { name: 'work' }))
      await user.click(screen.getByRole('button', { name: /apply/i }))
      expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ tagIds: [1, 2] }))
    })

    it('deselecting a tag removes it from tagIds', async () => {
      mockUseExpensesData.mockReturnValue({
        categories: mockCategories,
        currencies: mockCurrencies,
        tags: mockTags,
        isLoading: false,
        refresh: vi.fn(),
      })
      const user = userEvent.setup()
      const { onApply } = renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.click(screen.getByLabelText(/^tags$/i))
      await user.click(screen.getByRole('option', { name: 'groceries' }))
      await user.click(screen.getByRole('option', { name: 'work' }))
      await user.click(screen.getByRole('option', { name: 'groceries' }))
      await user.click(screen.getByRole('button', { name: /apply/i }))
      expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ tagIds: [2] }))
    })

    it('filters tag options case-insensitively as user types', async () => {
      mockUseExpensesData.mockReturnValue({
        categories: mockCategories,
        currencies: mockCurrencies,
        tags: mockTags,
        isLoading: false,
        refresh: vi.fn(),
      })
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.click(screen.getByLabelText(/^tags$/i))
      await user.type(screen.getByLabelText(/^tags$/i), 'GRO')
      expect(screen.getByRole('option', { name: 'groceries' })).toBeInTheDocument()
      expect(screen.queryByRole('option', { name: 'work' })).not.toBeInTheDocument()
    })

    it('omits tagIds from applied filter when all tags deselected', async () => {
      mockUseExpensesData.mockReturnValue({
        categories: mockCategories,
        currencies: mockCurrencies,
        tags: mockTags,
        isLoading: false,
        refresh: vi.fn(),
      })
      const user = userEvent.setup()
      const { onApply } = renderFilters()
      await user.click(screen.getByRole('button', { name: /^filters$/i }))
      await user.click(screen.getByLabelText(/^tags$/i))
      await user.click(screen.getByRole('option', { name: 'groceries' }))
      await user.click(screen.getByRole('option', { name: 'groceries' }))
      await user.click(screen.getByRole('button', { name: /apply/i }))
      expect(onApply).toHaveBeenCalledWith(expect.not.objectContaining({ tagIds: expect.anything() }))
    })
  })

  describe('clear filters shortcut', () => {
    it('hides Clear filters button when no active filters', () => {
      renderFilters({})
      expect(screen.queryByRole('button', { name: /clear filters/i })).not.toBeInTheDocument()
    })

    it('shows Clear filters button once a filter is set', () => {
      renderFilters({ description: 'lunch' })
      expect(screen.getByRole('button', { name: /clear filters/i })).toBeInTheDocument()
    })

    it('does not count page/pageSize/familyId/displayCurrencyId as active filters', () => {
      renderFilters({ page: 2, pageSize: 20, familyId: 1, displayCurrencyId: 1 })
      expect(screen.queryByRole('button', { name: /clear filters/i })).not.toBeInTheDocument()
    })

    it('clicking Clear filters applies an empty filter', async () => {
      const user = userEvent.setup()
      const { onApply } = renderFilters({ description: 'lunch' })
      await user.click(screen.getByRole('button', { name: /clear filters/i }))
      expect(onApply).toHaveBeenCalledWith({})
    })
  })
})
