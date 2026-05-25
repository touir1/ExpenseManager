import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { RecentExpenses } from '../RecentExpenses'
import type { ExpenseDto } from '@/features/expenses/types/expenses.type'

const makeExpense = (id: number, overrides: Partial<ExpenseDto> = {}): ExpenseDto => ({
  id,
  amount: 10.50,
  currency: { id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
  date: '2024-11-01',
  category: { id: 1, name: 'Food', description: undefined },
  subcategory: null,
  description: `Expense ${id}`,
  createdAt: '2024-11-01T10:00:00Z',
  modifiedAt: null,
  modifiedFrom: null,
  tags: [],
  families: [],
  convertedAmount: null,
  displayCurrency: null,
  ...overrides,
})

const mockExpenses = Array.from({ length: 7 }, (_, i) => makeExpense(i + 1))

function renderPanel(expenses = mockExpenses, isLoading = false) {
  render(
    <MemoryRouter>
      <RecentExpenses data={expenses} isLoading={isLoading} />
    </MemoryRouter>,
  )
}

describe('RecentExpenses', () => {
  it('renders expense rows', () => {
    renderPanel()
    expect(screen.getByText('Expense 1')).toBeInTheDocument()
    expect(screen.getByText('Expense 7')).toBeInTheDocument()
  })

  it('renders "View all" link pointing to /expenses', () => {
    renderPanel()
    const link = screen.getByRole('link', { name: /view all/i })
    expect(link).toHaveAttribute('href', '/expenses')
  })

  it('shows skeleton when loading', () => {
    renderPanel([], true)
    expect(screen.getByRole('status')).toBeInTheDocument()
  })

  it('shows empty state when no expenses', () => {
    renderPanel([])
    expect(screen.getByText(/no recent/i)).toBeInTheDocument()
  })

  it('does not show skeleton when not loading', () => {
    renderPanel()
    expect(screen.queryByRole('status')).not.toBeInTheDocument()
  })

  it('renders category pill', () => {
    renderPanel()
    expect(screen.getAllByText('Food').length).toBeGreaterThan(0)
  })

  it('renders formatted date', () => {
    renderPanel()
    expect(screen.getAllByText('01/11/24').length).toBeGreaterThan(0)
  })

  it('renders amount with currency symbol', () => {
    renderPanel()
    expect(screen.getAllByText('€10.50').length).toBeGreaterThan(0)
  })

  it('shows category and subcategory separated by slash when both present', () => {
    const expense = makeExpense(1, {
      category: { id: 1, name: 'Food', description: undefined },
      subcategory: { id: 2, name: 'Groceries', description: undefined },
    })
    renderPanel([expense])
    expect(screen.getByText('Food / Groceries')).toBeInTheDocument()
  })

  it('shows icon prefix when category has icon', () => {
    const expense = makeExpense(1, {
      category: { id: 2, name: 'Food', description: undefined, icon: '🍽️' },
    })
    renderPanel([expense])
    expect(screen.getByText('🍽️ Food')).toBeInTheDocument()
  })

  it('shows converted amount secondary line when convertedAmount set', () => {
    const expense = makeExpense(1, {
      amount: 10.50,
      currency: { id: 1, code: 'USD', name: 'US Dollar', symbol: '$', decimals: 2 },
      convertedAmount: 9.00,
      displayCurrency: { id: 2, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
    })
    renderPanel([expense])
    expect(screen.getByText('≈ $10.50')).toBeInTheDocument()
  })

  it('does not show secondary line when no conversion', () => {
    renderPanel()
    expect(screen.queryByText(/≈/)).not.toBeInTheDocument()
  })
})
