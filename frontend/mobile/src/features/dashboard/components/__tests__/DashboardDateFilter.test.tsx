import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'

vi.mock('@ionic/react', async () => {
  const React = await import('react')
  return {
    IonSegment: ({ children, value, onIonChange }: any) => (
      <div data-testid="segment" data-value={value}>
        {React.Children.map(children, (child: any) =>
          React.cloneElement(child, {
            onClick: () => onIonChange?.({ detail: { value: child.props.value } }),
          }),
        )}
      </div>
    ),
    IonSegmentButton: ({ children, value, onClick }: any) => (
      <button data-testid={`seg-btn-${value}`} value={value} onClick={onClick}>
        {children}
      </button>
    ),
    IonLabel: ({ children }: any) => <span>{children}</span>,
    IonDatetimeButton: ({ datetime }: any) => <button data-testid={`datetime-btn-${datetime}`} />,
    IonModal: ({ children }: any) => <div>{children}</div>,
    IonDatetime: ({ id, value, onIonChange }: any) => (
      <input
        data-testid={id}
        type="date"
        value={value}
        onChange={e => onIonChange?.({ detail: { value: e.target.value } })}
      />
    ),
    IonText: ({ children }: any) => <span>{children}</span>,
  }
})

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, fallback?: string) => {
      const map: Record<string, string> = {
        'dashboard.filters.thisMonth': 'This month',
        'dashboard.filters.sixMonths': '6 Months',
        'dashboard.filters.thisYear': 'This year',
        'dashboard.filters.custom': 'Custom',
        'dashboard.filters.invalidRange': 'From date must be before To date.',
      }
      return map[key] ?? fallback ?? key
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

  it('selecting custom reveals date pickers', () => {
    render(<DashboardDateFilter value="custom" onChange={vi.fn()} />)
    expect(screen.getByTestId('dashboard-date-from')).toBeDefined()
    expect(screen.getByTestId('dashboard-date-to')).toBeDefined()
  })

  it('changing custom dates calls onChange with correct shape', () => {
    const onChange = vi.fn()
    render(<DashboardDateFilter value="custom" onChange={onChange} />)
    const fromInput = screen.getByTestId('dashboard-date-from')
    fireEvent.change(fromInput, { target: { value: '2024-01-01' } })
    expect(onChange).toHaveBeenCalledWith(
      expect.objectContaining({ dateFrom: '2024-01-01', period: 'custom' }),
    )
  })

  it('rejects a from date after the to date', () => {
    const onChange = vi.fn()
    render(<DashboardDateFilter value="custom" onChange={onChange} />)
    const fromInput = screen.getByTestId('dashboard-date-from')
    const toInput = screen.getByTestId('dashboard-date-to')
    fireEvent.change(fromInput, { target: { value: '2024-01-01' } })
    fireEvent.change(toInput, { target: { value: '2024-01-31' } })
    onChange.mockClear()
    fireEvent.change(fromInput, { target: { value: '2024-06-01' } })
    expect(onChange).not.toHaveBeenCalled()
    expect(screen.getByText('From date must be before To date.')).toBeDefined()
  })
})
