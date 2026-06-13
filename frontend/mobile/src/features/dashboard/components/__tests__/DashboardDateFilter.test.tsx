import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'

vi.mock('@ionic/react', () => ({
  IonSegment: ({ children, value }: any) => (
    <div data-testid="segment" data-value={value}>
      {/* simulate change via the child buttons */}
      {children}
    </div>
  ),
  IonSegmentButton: ({ children, value, onClick }: any) => (
    <button data-testid={`seg-btn-${value}`} onClick={onClick} value={value}>
      {children}
    </button>
  ),
  IonLabel: ({ children }: any) => <span>{children}</span>,
}))

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => {
      const map: Record<string, string> = {
        'dashboard.filters.thisMonth': 'This month',
        'dashboard.filters.sixMonths': '6 Months',
        'dashboard.filters.thisYear': 'This year',
      }
      return map[key] ?? key
    },
  }),
}))

import { DashboardDateFilter, getPeriodDates } from '../DashboardDateFilter'

describe('getPeriodDates', () => {
  it('month: dateFrom is 1st of current month', () => {
    const { dateFrom, dateTo, period } = getPeriodDates('month')
    const now = new Date()
    const expected = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-01`
    expect(dateFrom).toBe(expected)
    expect(dateTo).toBe(now.toISOString().substring(0, 10))
    expect(period).toBe('month')
  })

  it('year: dateFrom is Jan 1st of current year', () => {
    const { dateFrom, period } = getPeriodDates('year')
    expect(dateFrom).toBe(`${new Date().getFullYear()}-01-01`)
    expect(period).toBe('year')
  })

  it('6m: dateFrom is ~6 months ago', () => {
    const { dateFrom, period } = getPeriodDates('6m')
    const from = new Date(dateFrom)
    const diff = (new Date().getTime() - from.getTime()) / (1000 * 60 * 60 * 24)
    expect(diff).toBeGreaterThan(170)
    expect(diff).toBeLessThan(190)
    expect(period).toBe('6m')
  })
})

describe('DashboardDateFilter', () => {
  it('renders three segment buttons', () => {
    render(<DashboardDateFilter value="month" onChange={vi.fn()} />)
    expect(screen.getByTestId('seg-btn-month')).toBeDefined()
    expect(screen.getByTestId('seg-btn-6m')).toBeDefined()
    expect(screen.getByTestId('seg-btn-year')).toBeDefined()
  })

  it('shows translated labels', () => {
    render(<DashboardDateFilter value="month" onChange={vi.fn()} />)
    expect(screen.getByText('This month')).toBeDefined()
    expect(screen.getByText('6 Months')).toBeDefined()
    expect(screen.getByText('This year')).toBeDefined()
  })
})
