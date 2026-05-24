import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { CategoryDonut } from '../CategoryDonut'
import type { CategoryBreakdownDto } from '../../types/dashboard.type'

vi.mock('recharts', () => ({
  PieChart: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="pie-chart">{children}</div>
  ),
  Pie: () => null,
  Cell: () => null,
  Tooltip: () => null,
  ResponsiveContainer: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
}))

const mockCategories: CategoryBreakdownDto[] = [
  {
    category: { id: 1, name: 'Food', description: null },
    totalAmount: 890,
    convertedTotal: null,
    percentage: 36.6,
    subcategories: [],
  },
  {
    category: { id: 2, name: 'Transport', description: null },
    totalAmount: 340,
    convertedTotal: null,
    percentage: 14.0,
    subcategories: [],
  },
]

describe('CategoryDonut', () => {
  it('renders pie chart when data present', () => {
    render(<CategoryDonut data={mockCategories} isLoading={false} />)
    expect(screen.getByTestId('pie-chart')).toBeInTheDocument()
  })

  it('renders legend with category names', () => {
    render(<CategoryDonut data={mockCategories} isLoading={false} />)
    expect(screen.getByText('Food')).toBeInTheDocument()
    expect(screen.getByText('Transport')).toBeInTheDocument()
  })

  it('renders amount and percentage in legend', () => {
    render(<CategoryDonut data={mockCategories} isLoading={false} />)
    expect(screen.getByText('890.00 (37%)')).toBeInTheDocument()
    expect(screen.getByText('340.00 (14%)')).toBeInTheDocument()
  })

  it('renders converted amount and percentage when displayCurrency provided', () => {
    const withConverted: CategoryBreakdownDto[] = [
      { ...mockCategories[0], convertedTotal: 950 },
      { ...mockCategories[1], convertedTotal: 360 },
    ]
    render(<CategoryDonut data={withConverted} isLoading={false} displayCurrency={{ symbol: '€', decimals: 2 }} />)
    expect(screen.getByText('€950.00 (37%)')).toBeInTheDocument()
    expect(screen.getByText('€360.00 (14%)')).toBeInTheDocument()
  })

  it('shows empty state when no data', () => {
    render(<CategoryDonut data={[]} isLoading={false} />)
    expect(screen.getByText(/no expenses/i)).toBeInTheDocument()
  })

  it('does not show chart when no data', () => {
    render(<CategoryDonut data={[]} isLoading={false} />)
    expect(screen.queryByTestId('pie-chart')).not.toBeInTheDocument()
  })

  it('shows skeleton when loading', () => {
    render(<CategoryDonut data={[]} isLoading={true} />)
    expect(screen.getByRole('status')).toBeInTheDocument()
  })

  it('does not show empty state when loading', () => {
    render(<CategoryDonut data={[]} isLoading={true} />)
    expect(screen.queryByText(/no expenses/i)).not.toBeInTheDocument()
  })

  it('renders section title', () => {
    render(<CategoryDonut data={mockCategories} isLoading={false} />)
    expect(screen.getByText(/by category/i)).toBeInTheDocument()
  })
})
