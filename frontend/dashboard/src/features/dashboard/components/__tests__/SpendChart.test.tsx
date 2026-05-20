import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { SpendChart } from '../SpendChart'
import type { MonthlyBreakdownDto } from '../../types/dashboard.type'

vi.mock('recharts', () => ({
  ComposedChart: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="composed-chart">{children}</div>
  ),
  Bar: () => null,
  Line: () => null,
  XAxis: () => null,
  YAxis: () => null,
  Tooltip: () => null,
  CartesianGrid: () => null,
  ResponsiveContainer: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
}))

const mockMonthly: MonthlyBreakdownDto[] = [
  { year: 2024, month: 1, totalAmount: 1000, convertedTotal: null, byCategory: [] },
  { year: 2024, month: 2, totalAmount: 1200, convertedTotal: null, byCategory: [] },
]

describe('SpendChart', () => {
  it('renders chart container when data present', () => {
    render(<SpendChart data={mockMonthly} isLoading={false} />)
    expect(screen.getByTestId('composed-chart')).toBeInTheDocument()
  })

  it('shows empty state when no data', () => {
    render(<SpendChart data={[]} isLoading={false} />)
    expect(screen.getByText(/no expenses/i)).toBeInTheDocument()
  })

  it('does not show chart when no data', () => {
    render(<SpendChart data={[]} isLoading={false} />)
    expect(screen.queryByTestId('composed-chart')).not.toBeInTheDocument()
  })

  it('shows skeleton when loading', () => {
    render(<SpendChart data={[]} isLoading={true} />)
    expect(screen.getByRole('status')).toBeInTheDocument()
  })

  it('does not show empty state when loading', () => {
    render(<SpendChart data={[]} isLoading={true} />)
    expect(screen.queryByText(/no expenses/i)).not.toBeInTheDocument()
  })

  it('renders section title', () => {
    render(<SpendChart data={mockMonthly} isLoading={false} />)
    expect(screen.getByText(/monthly spending/i)).toBeInTheDocument()
  })
})
