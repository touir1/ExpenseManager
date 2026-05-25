import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { CurrenciesPanel } from '../CurrenciesPanel'
import type { CurrencyBreakdownDto } from '../../types/dashboard.type'

const mockCurrencies: CurrencyBreakdownDto[] = [
  {
    currency: { id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
    totalAmount: 1500,
    convertedAmount: null,
    expenseCount: 20,
  },
  {
    currency: { id: 2, code: 'USD', name: 'Dollar', symbol: '$', decimals: 2 },
    totalAmount: 400,
    convertedAmount: 370,
    expenseCount: 5,
  },
]

describe('CurrenciesPanel', () => {
  it('renders a row per currency', () => {
    render(<CurrenciesPanel data={mockCurrencies} isLoading={false} />)
    expect(screen.getByText('EUR')).toBeInTheDocument()
    expect(screen.getByText('USD')).toBeInTheDocument()
  })

  it('renders total amount for each currency', () => {
    render(<CurrenciesPanel data={mockCurrencies} isLoading={false} />)
    expect(screen.getByText('€ 1500.00')).toBeInTheDocument()
    expect(screen.getByText('$ 400.00')).toBeInTheDocument()
  })

  it('shows converted amount when present', () => {
    render(<CurrenciesPanel data={mockCurrencies} isLoading={false} />)
    expect(screen.getByText(/≈ 370\.00/)).toBeInTheDocument()
  })

  it('does not show converted amount when null', () => {
    render(<CurrenciesPanel data={mockCurrencies} isLoading={false} />)
    const converted = screen.getAllByText(/≈/)
    expect(converted).toHaveLength(1)
  })

  it('shows expense count per currency', () => {
    render(<CurrenciesPanel data={mockCurrencies} isLoading={false} />)
    expect(screen.getByText(/20 expenses/)).toBeInTheDocument()
    expect(screen.getByText(/5 expenses/)).toBeInTheDocument()
  })

  it('renders currency symbol badge', () => {
    render(<CurrenciesPanel data={mockCurrencies} isLoading={false} />)
    expect(screen.getAllByText('€').length).toBeGreaterThan(0)
    expect(screen.getAllByText('$').length).toBeGreaterThan(0)
  })

  it('shows skeleton when loading', () => {
    render(<CurrenciesPanel data={[]} isLoading={true} />)
    expect(screen.getByRole('status')).toBeInTheDocument()
  })

  it('shows empty state when no data', () => {
    render(<CurrenciesPanel data={[]} isLoading={false} />)
    expect(screen.getByText(/no currencies/i)).toBeInTheDocument()
  })

  it('does not show empty state when loading', () => {
    render(<CurrenciesPanel data={[]} isLoading={true} />)
    expect(screen.queryByText(/no currencies/i)).not.toBeInTheDocument()
  })
})
