import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { LargestExpenses } from '../LargestExpenses'
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
  hasReceipt: false,
  ...overrides,
})

const mockExpenses = [
  makeExpense(1, { amount: 500 }),
  makeExpense(2, { amount: 300 }),
  makeExpense(3, { amount: 100 }),
]

function renderPanel(expenses = mockExpenses, isLoading = false) {
  render(
    <MemoryRouter>
      <LargestExpenses data={expenses} isLoading={isLoading} />
    </MemoryRouter>,
  )
}

describe('LargestExpenses', () => {
  it('renders expense rows in the order given (already sorted desc by amount)', () => {
    renderPanel()
    const rows = screen.getAllByText(/^Expense \d$/)
    expect(rows.map(r => r.textContent)).toEqual(['Expense 1', 'Expense 2', 'Expense 3'])
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
    expect(screen.getByText(/no expenses/i)).toBeInTheDocument()
  })

  it('does not show skeleton when not loading', () => {
    renderPanel()
    expect(screen.queryByRole('status')).not.toBeInTheDocument()
  })

  it('renders rank numbers', () => {
    renderPanel()
    expect(screen.getByText('1')).toBeInTheDocument()
    expect(screen.getByText('2')).toBeInTheDocument()
    expect(screen.getByText('3')).toBeInTheDocument()
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
    renderPanel([makeExpense(1, { amount: 500 })])
    expect(screen.getByText('€ 500.00')).toBeInTheDocument()
  })

  it('shows converted amount secondary line when convertedAmount set', () => {
    const expense = makeExpense(1, {
      amount: 10.50,
      currency: { id: 1, code: 'USD', name: 'US Dollar', symbol: '$', decimals: 2 },
      convertedAmount: 9.00,
      displayCurrency: { id: 2, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
    })
    renderPanel([expense])
    expect(screen.getByText('≈ € 9.00')).toBeInTheDocument()
  })

  it('does not show secondary line when no conversion', () => {
    renderPanel()
    expect(screen.queryByText(/≈/)).not.toBeInTheDocument()
  })
})
