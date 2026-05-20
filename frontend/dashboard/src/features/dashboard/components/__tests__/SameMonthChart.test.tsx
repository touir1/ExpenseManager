import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { SameMonthChart } from '../SameMonthChart'
import type { SameMonthYearlyDto } from '../../types/dashboard.type'

vi.mock('recharts', () => ({
  BarChart: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="bar-chart">{children}</div>
  ),
  Bar: () => null,
  XAxis: () => null,
  YAxis: () => null,
  Tooltip: () => null,
  CartesianGrid: () => null,
  ResponsiveContainer: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
}))

const mockYearly: SameMonthYearlyDto[] = [
  { year: 2023, totalAmount: 2100, convertedTotal: null },
  { year: 2024, totalAmount: 2430, convertedTotal: null },
]

describe('SameMonthChart', () => {
  it('renders bar chart when data present', () => {
    render(<SameMonthChart data={mockYearly} isLoading={false} selectedMonth={5} />)
    expect(screen.getByTestId('bar-chart')).toBeInTheDocument()
  })

  it('shows empty state when no data', () => {
    render(<SameMonthChart data={[]} isLoading={false} selectedMonth={5} />)
    expect(screen.getByText(/no data/i)).toBeInTheDocument()
  })

  it('does not show chart when no data', () => {
    render(<SameMonthChart data={[]} isLoading={false} selectedMonth={5} />)
    expect(screen.queryByTestId('bar-chart')).not.toBeInTheDocument()
  })

  it('shows skeleton when loading', () => {
    render(<SameMonthChart data={[]} isLoading={true} selectedMonth={5} />)
    expect(screen.getByRole('status')).toBeInTheDocument()
  })

  it('does not show empty state when loading', () => {
    render(<SameMonthChart data={[]} isLoading={true} selectedMonth={5} />)
    expect(screen.queryByText(/no data/i)).not.toBeInTheDocument()
  })

  it('shows month name in title', () => {
    render(<SameMonthChart data={mockYearly} isLoading={false} selectedMonth={5} />)
    expect(screen.getByText(/may/i)).toBeInTheDocument()
  })

  it('handles all 12 months', () => {
    const MONTHS = ['January', 'February', 'March', 'April', 'May', 'June',
      'July', 'August', 'September', 'October', 'November', 'December']
    MONTHS.forEach((month, i) => {
      const { unmount } = render(
        <SameMonthChart data={mockYearly} isLoading={false} selectedMonth={i + 1} />,
      )
      expect(screen.getByText(new RegExp(month, 'i'))).toBeInTheDocument()
      unmount()
    })
  })
})
