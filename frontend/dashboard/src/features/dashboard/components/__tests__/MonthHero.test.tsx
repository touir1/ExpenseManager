import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MonthHero } from '../MonthHero'
import type { DashboardSummaryDto } from '../../types/dashboard.type'

const mockSummary: DashboardSummaryDto = {
  totalAmount: 2430.50,
  convertedTotal: null,
  displayCurrency: null,
  expenseCount: 42,
  previousPeriodTotal: 2250,
  changePercent: 8.0,
  topCategory: { id: 1, name: 'Food', description: null },
  topCategoryAmount: 890,
}

describe('MonthHero', () => {
  it('renders total amount', () => {
    render(<MonthHero data={mockSummary} isLoading={false} />)
    expect(screen.getByText((text) => text.replace(/[\s  ]/g, '').includes('2430'))).toBeInTheDocument()
  })

  it('renders positive delta chip with green class', () => {
    render(<MonthHero data={mockSummary} isLoading={false} />)
    const chip = screen.getByText(/\+8\.0%/)
    expect(chip.closest('span')).toHaveClass('text-green-700')
  })

  it('renders negative delta chip with red class', () => {
    render(<MonthHero data={{ ...mockSummary, changePercent: -5.2 }} isLoading={false} />)
    const chip = screen.getByText(/-5\.2%/)
    expect(chip.closest('span')).toHaveClass('text-red-700')
  })

  it('renders expense count', () => {
    render(<MonthHero data={mockSummary} isLoading={false} />)
    expect(screen.getByText(/42/)).toBeInTheDocument()
  })

  it('renders top category name', () => {
    render(<MonthHero data={mockSummary} isLoading={false} />)
    expect(screen.getByText('Food')).toBeInTheDocument()
  })

  it('renders converted total when displayCurrency present', () => {
    const data: DashboardSummaryDto = {
      ...mockSummary,
      convertedTotal: 2200.00,
      displayCurrency: { id: 2, code: 'USD', name: 'Dollar', symbol: '$', decimals: 2 },
    }
    render(<MonthHero data={data} isLoading={false} />)
    expect(screen.getByText((text) => text.replace(/[\s  ]/g, '').includes('2200'))).toBeInTheDocument()
    expect(screen.getByText(/USD/)).toBeInTheDocument()
  })

  it('shows currency symbol when displayCurrency present', () => {
    const data: DashboardSummaryDto = {
      ...mockSummary,
      convertedTotal: 2200.00,
      displayCurrency: { id: 2, code: 'USD', name: 'Dollar', symbol: '$', decimals: 2 },
    }
    render(<MonthHero data={data} isLoading={false} />)
    expect(screen.getByText('$')).toBeInTheDocument()
  })

  it('shows skeleton when loading', () => {
    render(<MonthHero data={undefined} isLoading={true} />)
    expect(screen.getByRole('status')).toBeInTheDocument()
  })

  it('shows skeleton when data is undefined even if not loading', () => {
    render(<MonthHero data={undefined} isLoading={false} />)
    expect(screen.getByRole('status')).toBeInTheDocument()
  })

  it('does not render delta chip when changePercent is null', () => {
    render(<MonthHero data={{ ...mockSummary, changePercent: null }} isLoading={false} />)
    expect(screen.queryByText(/%/)).not.toBeInTheDocument()
  })

  it('does not render top category pill when topCategory is null', () => {
    render(<MonthHero data={{ ...mockSummary, topCategory: null }} isLoading={false} />)
    expect(screen.queryByText('Food')).not.toBeInTheDocument()
  })
})
