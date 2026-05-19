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
  useExpensesData: () => ({ categories: mockCategories, currencies: mockCurrencies, isLoading: false, refresh: vi.fn() }),
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
      await user.selectOptions(screen.getByLabelText(/^category$/i), '1')
      expect(screen.getByLabelText(/subcategory/i)).toBeInTheDocument()
      expect(screen.getByRole('option', { name: 'Groceries' })).toBeInTheDocument()
    })

    it('hides subcategory when category has no subcategories', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /filter/i }))
      await user.selectOptions(screen.getByLabelText(/^category$/i), '2')
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

    it('renders currency options', async () => {
      const user = userEvent.setup()
      renderFilters()
      await user.click(screen.getByRole('button', { name: /filter/i }))
      expect(screen.getByRole('option', { name: 'EUR' })).toBeInTheDocument()
    })
  })
})
